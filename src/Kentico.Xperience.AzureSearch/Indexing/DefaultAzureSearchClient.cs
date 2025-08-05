using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;

using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites;

using Kentico.Xperience.AzureSearch.Admin;

using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Default implementation of <see cref="IAzureSearchClient"/>.
/// </summary>
internal class DefaultAzureSearchClient : IAzureSearchClient
{
    private readonly IAzureSearchIndexClientService azureSearchIndexClientService;
    private readonly IContentQueryExecutor executor;
    private readonly IServiceProvider serviceProvider;
    private readonly IInfoProvider<ContentLanguageInfo> languageProvider;
    private readonly IInfoProvider<ChannelInfo> channelProvider;
    private readonly IConversionService conversionService;
    private readonly IProgressiveCache cache;
    private readonly SearchIndexClient searchIndexClient;

    public DefaultAzureSearchClient(
        IAzureSearchIndexClientService azureSearchIndexClientService,
        IContentQueryExecutor executor,
        IServiceProvider serviceProvider,
        IInfoProvider<ContentLanguageInfo> languageProvider,
        IInfoProvider<ChannelInfo> channelProvider,
        IConversionService conversionService,
        IProgressiveCache cache,
        SearchIndexClient searchIndexClient)
    {
        this.azureSearchIndexClientService = azureSearchIndexClientService;
        this.executor = executor;
        this.serviceProvider = serviceProvider;
        this.languageProvider = languageProvider;
        this.channelProvider = channelProvider;
        this.conversionService = conversionService;
        this.cache = cache;
        this.searchIndexClient = searchIndexClient;
    }

    /// <inheritdoc />
    public async Task<int> DeleteRecords(IEnumerable<string> itemGuids, string indexName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        if (!itemGuids.Any())
        {
            return await Task.FromResult(0);
        }

        return await DeleteRecordsInternal(itemGuids, indexName, cancellationToken);
    }


    /// <inheritdoc/>
    public async Task<ICollection<AzureSearchIndexStatisticsViewModel>> GetStatistics(CancellationToken cancellationToken)
    {
        var indices = AzureSearchIndexStore.Instance.GetAllIndices();

        var stats = new List<AzureSearchIndexStatisticsViewModel>();

        foreach (var index in indices)
        {
            var indexClient = await azureSearchIndexClientService.InitializeIndexClient(index.IndexName, cancellationToken);
            stats.Add(new AzureSearchIndexStatisticsViewModel()
            {
                Name = index.IndexName,
                Entries = await indexClient.GetDocumentCountAsync(cancellationToken)
            });
        }

        return stats;
    }

    /// <inheritdoc />
    public Task Rebuild(string indexName, CancellationToken? cancellationToken)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        var azureSearchIndex = AzureSearchIndexStore.Instance.GetRequiredIndex(indexName);
        return RebuildInternal(azureSearchIndex, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteIndex(string indexName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        await searchIndexClient.DeleteIndexAsync(indexName, cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> UpsertRecords(IEnumerable<IAzureSearchModel> models, string indexName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        if (models == null || !models.Any())
        {
            return Task.FromResult(0);
        }

        return UpsertRecordsInternal(models, indexName, cancellationToken);
    }

    private async Task<int> DeleteRecordsInternal(IEnumerable<string> itemGuids, string indexName, CancellationToken cancellationToken)
    {
        var searchClient = await azureSearchIndexClientService.InitializeIndexClient(indexName, cancellationToken);

        var batch = IndexDocumentsBatch.Delete(nameof(BaseAzureSearchModel.ObjectID), itemGuids);

        var result = await searchClient.IndexDocumentsAsync(batch, cancellationToken: cancellationToken);

        return result.Value.Results.Count(model => model.Succeeded);
    }

    private async Task RebuildInternal(AzureSearchIndex azureSearchIndex, CancellationToken? cancellationToken)
    {
        var indexedItems = new List<IIndexEventItemModel>();

        foreach (var includedPathAttribute in azureSearchIndex.IncludedPaths)
        {
            var pathMatch =
             includedPathAttribute.AliasPath.EndsWith("/%", StringComparison.OrdinalIgnoreCase)
                 ? PathMatch.Children(includedPathAttribute.AliasPath[..^2])
                 : PathMatch.Single(includedPathAttribute.AliasPath);

            foreach (string language in azureSearchIndex.LanguageNames)
            {
                if (includedPathAttribute.ContentTypes != null && includedPathAttribute.ContentTypes.Count > 0)
                {
                    var queryBuilder = new ContentItemQueryBuilder();

                    foreach (var contentType in includedPathAttribute.ContentTypes)
                    {
                        queryBuilder.ForContentType(contentType.ContentTypeName, config => config.ForWebsite(azureSearchIndex.WebSiteChannelName, includeUrlPath: true, pathMatch: pathMatch));
                    }

                    queryBuilder.InLanguage(language);

                    var webpages = await executor.GetWebPageResult(queryBuilder,
                        container => container,
                        cancellationToken: cancellationToken ?? default);

                    foreach (var page in webpages)
                    {
                        var item = await MapToEventItem(page);
                        indexedItems.Add(item);
                    }
                }
            }
        }
        foreach (string language in azureSearchIndex.LanguageNames)
        {
            var queryBuilder = new ContentItemQueryBuilder();

            if (azureSearchIndex.IncludedReusableContentTypes != null && azureSearchIndex.IncludedReusableContentTypes.Count > 0)
            {
                foreach (string reusableContentType in azureSearchIndex.IncludedReusableContentTypes)
                {
                    queryBuilder.ForContentType(reusableContentType);
                }

                queryBuilder.InLanguage(language);

                var reusableItems = await executor.GetResult(queryBuilder, result => result, cancellationToken: cancellationToken ?? default);

                foreach (var reusableItem in reusableItems)
                {
                    var item = await MapToEventReusableItem(reusableItem);
                    indexedItems.Add(item);
                }
            }
        }

        await searchIndexClient.DeleteIndexAsync(azureSearchIndex.IndexName, cancellationToken ?? default);

        indexedItems.ForEach(item => AzureSearchQueueWorker.EnqueueAzureSearchQueueItem(new AzureSearchQueueItem(item, AzureSearchTaskType.PUBLISH_INDEX, azureSearchIndex.IndexName)));
    }

    private async Task<IndexEventWebPageItemModel> MapToEventItem(IWebPageContentQueryDataContainer content)
    {
        var languages = await GetAllLanguages();

        string languageName = languages.FirstOrDefault(l => l.ContentLanguageID == content.ContentItemCommonDataContentLanguageID)?.ContentLanguageName ?? string.Empty;

        var websiteChannels = await GetAllWebsiteChannels();

        string channelName = websiteChannels.FirstOrDefault(c => c.WebsiteChannelID == content.WebPageItemWebsiteChannelID).ChannelName ?? string.Empty;

        var item = new IndexEventWebPageItemModel(
            content.WebPageItemID,
            content.WebPageItemGUID,
            languageName,
            content.ContentTypeName,
            content.WebPageItemName,
            content.ContentItemIsSecured,
            content.ContentItemContentTypeID,
            content.ContentItemCommonDataContentLanguageID,
            channelName,
            content.WebPageItemTreePath,
            content.WebPageItemOrder);

        return item;
    }

    private async Task<IndexEventReusableItemModel> MapToEventReusableItem(IContentQueryDataContainer content)
    {
        var languages = await GetAllLanguages();

        string languageName = languages.FirstOrDefault(l => l.ContentLanguageID == content.ContentItemCommonDataContentLanguageID)?.ContentLanguageName ?? string.Empty;

        var item = new IndexEventReusableItemModel(
            content.ContentItemID,
            content.ContentItemGUID,
            languageName,
            content.ContentTypeName,
            content.ContentItemName,
            content.ContentItemIsSecured,
            content.ContentItemContentTypeID,
            content.ContentItemCommonDataContentLanguageID);

        return item;
    }

    private async Task<int> UpsertRecordsInternal(IEnumerable<IAzureSearchModel> models, string indexName, CancellationToken cancellationToken)
    {
        int upsertedCount = 0;
        var searchClient = await azureSearchIndexClientService.InitializeIndexClient(indexName, cancellationToken);

        var azureIndex = AzureSearchIndexStore.Instance.GetIndex(indexName) ??
            throw new InvalidOperationException($"Registered index with name '{indexName}' doesn't exist.");

        var strategy = serviceProvider.GetRequiredStrategy(azureIndex);

        upsertedCount += await strategy.UploadDocuments(models, searchClient);

        return upsertedCount;
    }

    private Task<IEnumerable<ContentLanguageInfo>> GetAllLanguages() =>
        cache.LoadAsync(async cs =>
        {
            var results = await languageProvider.Get().GetEnumerableTypedResultAsync();

            cs.GetCacheDependency = () => CacheHelper.GetCacheDependency($"{ContentLanguageInfo.OBJECT_TYPE}|all");

            return results;
        }, new CacheSettings(5, nameof(DefaultAzureSearchClient), nameof(GetAllLanguages)));

    private Task<IEnumerable<(int WebsiteChannelID, string ChannelName)>> GetAllWebsiteChannels() =>
        cache.LoadAsync(async cs =>
        {

            var results = await channelProvider.Get()
                .Source(s => s.Join<WebsiteChannelInfo>(nameof(ChannelInfo.ChannelID), nameof(WebsiteChannelInfo.WebsiteChannelChannelID)))
                .Columns(nameof(WebsiteChannelInfo.WebsiteChannelID), nameof(ChannelInfo.ChannelName))
                .GetDataContainerResultAsync();

            cs.GetCacheDependency = () => CacheHelper.GetCacheDependency(new[] { $"{ChannelInfo.OBJECT_TYPE}|all", $"{WebsiteChannelInfo.OBJECT_TYPE}|all" });

            var items = new List<(int WebsiteChannelID, string ChannelName)>();

            foreach (var item in results)
            {
                if (item.TryGetValue(nameof(WebsiteChannelInfo.WebsiteChannelID), out object channelID) && item.TryGetValue(nameof(ChannelInfo.ChannelName), out object channelName))
                {
                    items.Add(new(conversionService.GetInteger(channelID, 0), conversionService.GetString(channelName, string.Empty)));
                }
            }

            return items.AsEnumerable();
        }, new CacheSettings(5, nameof(DefaultAzureSearchClient), nameof(GetAllWebsiteChannels)));
}
