using System.Text;
using CMS.DataEngine;

namespace Kentico.Xperience.AzureSearch.Admin;

internal class DefaultAzureSearchConfigurationStorageService : IAzureSearchConfigurationStorageService
{
    private readonly IAzureSearchIndexItemInfoProvider indexProvider;
    private readonly IAzureSearchIncludedPathItemInfoProvider pathProvider;
    private readonly IAzureSearchContentTypeItemInfoProvider contentTypeProvider;
    private readonly IAzureSearchIndexLanguageItemInfoProvider languageProvider;

    public DefaultAzureSearchConfigurationStorageService(
        IAzureSearchIndexItemInfoProvider indexProvider,
        IAzureSearchIncludedPathItemInfoProvider pathProvider,
        IAzureSearchContentTypeItemInfoProvider contentTypeProvider,
        IAzureSearchIndexLanguageItemInfoProvider languageProvider
    )
    {
        this.indexProvider = indexProvider;
        this.pathProvider = pathProvider;
        this.contentTypeProvider = contentTypeProvider;
        this.languageProvider = languageProvider;
    }

    private static string RemoveWhitespacesUsingStringBuilder(string source)
    {
        var builder = new StringBuilder(source.Length);
        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            if (!char.IsWhiteSpace(c))
            {
                builder.Append(c);
            }
        }
        return source.Length == builder.Length ? source : builder.ToString();
    }
    public bool TryCreateIndex(AzureSearchConfigurationModel configuration)
    {
        var existingIndex = indexProvider.Get()
            .WhereEquals(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemIndexName), configuration.IndexName)
            .TopN(1)
            .FirstOrDefault();

        if (existingIndex is not null)
        {
            return false;
        }

        var newInfo = new AzureSearchIndexItemInfo()
        {
            AzureSearchIndexItemIndexName = configuration.IndexName ?? "",
            AzureSearchIndexItemChannelName = configuration.ChannelName ?? "",
            AzureSearchIndexItemStrategyName = configuration.StrategyName ?? "",
            AzureSearchIndexItemRebuildHook = configuration.RebuildHook ?? ""
        };

        indexProvider.Set(newInfo);

        configuration.Id = newInfo.AzureSearchIndexItemId;

        if (configuration.LanguageNames is not null)
        {
            foreach (string? language in configuration.LanguageNames)
            {
                var languageInfo = new AzureSearchIndexLanguageItemInfo()
                {
                    AzureSearchIndexLanguageItemName = language,
                    AzureSearchIndexLanguageItemIndexItemId = newInfo.AzureSearchIndexItemId
                };

                languageInfo.Insert();
            }
        }

        if (configuration.Paths is not null)
        {
            foreach (var path in configuration.Paths)
            {
                var pathInfo = new AzureSearchIncludedPathItemInfo()
                {
                    AzureSearchIncludedPathItemAliasPath = path.AliasPath,
                    AzureSearchIncludedPathItemIndexItemId = newInfo.AzureSearchIndexItemId
                };
                pathProvider.Set(pathInfo);

                if (path.ContentTypes is not null)
                {
                    foreach (string? contentType in path.ContentTypes)
                    {
                        var contentInfo = new AzureSearchContentTypeItemInfo()
                        {
                            AzureSearchContentTypeItemContentTypeName = contentType,
                            AzureSearchContentTypeItemIncludedPathItemId = pathInfo.AzureSearchIncludedPathItemId,
                            AzureSearchContentTypeItemIndexItemId = newInfo.AzureSearchIndexItemId
                        };
                        contentInfo.Insert();
                    }
                }
            }
        }

        return true;
    }

    public AzureSearchConfigurationModel? GetIndexDataOrNull(int indexId)
    {
        var indexInfo = indexProvider.Get().WithID(indexId).FirstOrDefault();
        if (indexInfo == default)
        {
            return default;
        }

        var paths = pathProvider.Get().WhereEquals(nameof(AzureSearchIncludedPathItemInfo.AzureSearchIncludedPathItemIndexItemId), indexInfo.AzureSearchIndexItemId).GetEnumerableTypedResult();
        var contentTypes = contentTypeProvider.Get().WhereEquals(nameof(AzureSearchContentTypeItemInfo.AzureSearchContentTypeItemIndexItemId), indexInfo.AzureSearchIndexItemId).GetEnumerableTypedResult();
        var languages = languageProvider.Get().WhereEquals(nameof(AzureSearchIndexLanguageItemInfo.AzureSearchIndexLanguageItemIndexItemId), indexInfo.AzureSearchIndexItemId).GetEnumerableTypedResult();

        return new AzureSearchConfigurationModel(indexInfo, languages, paths, contentTypes);
    }

    public List<string> GetExistingIndexNames() => indexProvider.Get().Select(x => x.AzureSearchIndexItemIndexName).ToList();

    public List<int> GetIndexIds() => indexProvider.Get().Select(x => x.AzureSearchIndexItemId).ToList();

    public IEnumerable<AzureSearchConfigurationModel> GetAllIndexData()
    {
        var indexInfos = indexProvider.Get().GetEnumerableTypedResult().ToList();
        if (indexInfos.Count == 0)
        {
            return [];
        }

        var paths = pathProvider.Get().ToList();
        var contentTypes = contentTypeProvider.Get().ToList();
        var languages = languageProvider.Get().ToList();

        return indexInfos.Select(index => new AzureSearchConfigurationModel(index, languages, paths, contentTypes));
    }

    public bool TryEditIndex(AzureSearchConfigurationModel configuration)
    {
        configuration.IndexName = RemoveWhitespacesUsingStringBuilder(configuration.IndexName ?? "");

        var indexInfo = indexProvider.Get()
            .WhereEquals(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemId), configuration.Id)
            .TopN(1)
            .FirstOrDefault();

        if (indexInfo is null)
        {
            return false;
        }

        pathProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchIncludedPathItemInfo.AzureSearchIncludedPathItemIndexItemId)} = {configuration.Id}"));
        languageProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchIndexLanguageItemInfo.AzureSearchIndexLanguageItemIndexItemId)} = {configuration.Id}"));
        contentTypeProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchContentTypeItemInfo.AzureSearchContentTypeItemIndexItemId)} = {configuration.Id}"));

        indexInfo.AzureSearchIndexItemRebuildHook = configuration.RebuildHook ?? "";
        indexInfo.AzureSearchIndexItemStrategyName = configuration.StrategyName ?? "";
        indexInfo.AzureSearchIndexItemChannelName = configuration.ChannelName ?? "";
        indexInfo.AzureSearchIndexItemIndexName = configuration.IndexName ?? "";

        indexProvider.Set(indexInfo);

        if (configuration.LanguageNames is not null)
        {
            foreach (string? language in configuration.LanguageNames)
            {
                var languageInfo = new AzureSearchIndexLanguageItemInfo()
                {
                    AzureSearchIndexLanguageItemName = language,
                    AzureSearchIndexLanguageItemIndexItemId = indexInfo.AzureSearchIndexItemId,
                };

                languageProvider.Set(languageInfo);
            }
        }

        if (configuration.Paths is not null)
        {
            foreach (var path in configuration.Paths)
            {
                var pathInfo = new AzureSearchIncludedPathItemInfo()
                {
                    AzureSearchIncludedPathItemAliasPath = path.AliasPath,
                    AzureSearchIncludedPathItemIndexItemId = indexInfo.AzureSearchIndexItemId,
                };
                pathProvider.Set(pathInfo);

                if (path.ContentTypes != null)
                {
                    foreach (string? contentType in path.ContentTypes)
                    {
                        var contentInfo = new AzureSearchContentTypeItemInfo()
                        {
                            AzureSearchContentTypeItemContentTypeName = contentType ?? "",
                            AzureSearchContentTypeItemIncludedPathItemId = pathInfo.AzureSearchIncludedPathItemId,
                            AzureSearchContentTypeItemIndexItemId = indexInfo.AzureSearchIndexItemId,
                        };
                        contentInfo.Insert();
                    }
                }
            }
        }

        return true;
    }

    public bool TryDeleteIndex(int id)
    {
        indexProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemId)} = {id}"));
        pathProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchIncludedPathItemInfo.AzureSearchIncludedPathItemIndexItemId)} = {id}"));
        languageProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchIndexLanguageItemInfo.AzureSearchIndexLanguageItemIndexItemId)} = {id}"));
        contentTypeProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchContentTypeItemInfo.AzureSearchContentTypeItemIndexItemId)} = {id}"));

        return true;
    }

    public bool TryDeleteIndex(AzureSearchConfigurationModel configuration)
    {
        indexProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemId)} = {configuration.Id}"));
        pathProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchIncludedPathItemInfo.AzureSearchIncludedPathItemIndexItemId)} = {configuration.Id}"));
        languageProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchIndexLanguageItemInfo.AzureSearchIndexLanguageItemIndexItemId)} = {configuration.Id}"));
        contentTypeProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchContentTypeItemInfo.AzureSearchContentTypeItemIndexItemId)} = {configuration.Id}"));

        return true;
    }
}
