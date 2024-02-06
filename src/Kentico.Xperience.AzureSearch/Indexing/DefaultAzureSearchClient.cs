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
    private readonly IAzureSearchIndexClientService azuresearchIndexService;
    private readonly IContentQueryExecutor executor;
    private readonly IServiceProvider serviceProvider;
    private readonly IInfoProvider<ContentLanguageInfo> languageProvider;
    private readonly IInfoProvider<ChannelInfo> channelProvider;
    private readonly IConversionService conversionService;
    private readonly IProgressiveCache cache;
    private readonly IEventLogService log;

    public DefaultAzureSearchClient(
        IAzureSearchIndexClientService azuresearchIndexService,
        IContentQueryExecutor executor,
        IServiceProvider serviceProvider,
        IInfoProvider<ContentLanguageInfo> languageProvider,
        IInfoProvider<ChannelInfo> channelProvider,
        IConversionService conversionService,
        IProgressiveCache cache,
        IEventLogService log)
    {
        this.azuresearchIndexService = azuresearchIndexService;
        this.executor = executor;
        this.serviceProvider = serviceProvider;
        this.languageProvider = languageProvider;
        this.channelProvider = channelProvider;
        this.conversionService = conversionService;
        this.cache = cache;
        this.log = log;
    }

    /// <inheritdoc />
    public async Task<int> DeleteRecords(IEnumerable<string> itemGuids, string indexName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        if (itemGuids == null || !itemGuids.Any())
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
            var indexClient = await azuresearchIndexService.InitializeIndexClient(index.IndexName, cancellationToken);
            stats.Add(new AzureSearchIndexStatisticsViewModel()
            {
                Name = index.IndexName,
                Entries = await indexClient.GetDocumentCountAsync()
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

        var azuresearchIndex = AzureSearchIndexStore.Instance.GetRequiredIndex(indexName);
        return RebuildInternal(azuresearchIndex, cancellationToken);
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
        var searchClient = await azuresearchIndexService.InitializeIndexClient(indexName, cancellationToken);

        var batch = IndexDocumentsBatch.Delete(nameof(DefaultAzureSearchModel.ObjectID), itemGuids);

        var result = await searchClient.IndexDocumentsAsync(batch);

        return result.Value.Results.Count(model => model.Succeeded);
    }

    private async Task RebuildInternal(AzureSearchIndex azuresearchIndex, CancellationToken? cancellationToken)
    {
        var indexedItems = new List<IndexEventWebPageItemModel>();
        foreach (var includedPathAttribute in azuresearchIndex.IncludedPaths)
        {
            foreach (string language in azuresearchIndex.LanguageNames)
            {
                var queryBuilder = new ContentItemQueryBuilder();

                if (includedPathAttribute.ContentTypes != null && includedPathAttribute.ContentTypes.Count > 0)
                {
                    foreach (string contentType in includedPathAttribute.ContentTypes)
                    {
                        queryBuilder.ForContentType(contentType, config => config.ForWebsite(azuresearchIndex.WebSiteChannelName, includeUrlPath: true));
                    }
                }
                queryBuilder.InLanguage(language);

                var webpages = await executor.GetWebPageResult(queryBuilder, container => container, cancellationToken: cancellationToken ?? default);

                foreach (var page in webpages)
                {
                    var item = await MapToEventItem(page);
                    indexedItems.Add(item);
                }
            }
        }

        log.LogInformation(
            "Kentico.Xperience.AzureSearch",
            "INDEX_REBUILD",
            $"Rebuilding index [{azuresearchIndex.IndexName}]. {indexedItems.Count} web page items queued for re-indexing"
        );

        indexedItems.ForEach(item => AzureSearchQueueWorker.EnqueueAzureSearchQueueItem(new AzureSearchQueueItem(item, AzureSearchTaskType.PUBLISH_INDEX, azuresearchIndex.IndexName)));
    }

    private async Task<IndexEventWebPageItemModel> MapToEventItem(IWebPageContentQueryDataContainer content)
    {
        var languages = await GetAllLanguages();

        string languageName = languages.FirstOrDefault(l => l.ContentLanguageID == content.ContentItemCommonDataContentLanguageID)?.ContentLanguageName ?? "";

        var websiteChannels = await GetAllWebsiteChannels();

        string channelName = websiteChannels.FirstOrDefault(c => c.WebsiteChannelID == content.WebPageItemWebsiteChannelID).ChannelName ?? "";

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

    private async Task<int> UpsertRecordsInternal(IEnumerable<IAzureSearchModel> models, string indexName, CancellationToken cancellationToken)
    {
        int upsertedCount = 0;
        var searchClient = await azuresearchIndexService.InitializeIndexClient(indexName, cancellationToken);

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
                    items.Add(new(conversionService.GetInteger(channelID, 0), conversionService.GetString(channelName, "")));
                }
            }

            return items.AsEnumerable();
        }, new CacheSettings(5, nameof(DefaultAzureSearchClient), nameof(GetAllWebsiteChannels)));
}
