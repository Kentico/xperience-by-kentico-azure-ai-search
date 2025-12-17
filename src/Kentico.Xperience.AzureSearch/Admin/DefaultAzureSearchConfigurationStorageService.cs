using System.Text;

using CMS.Core;
using CMS.DataEngine;

namespace Kentico.Xperience.AzureSearch.Admin;

internal class DefaultAzureSearchConfigurationStorageService : IAzureSearchConfigurationStorageService
{
    private readonly IInfoProvider<AzureSearchIndexItemInfo> indexProvider;
    private readonly IInfoProvider<AzureSearchIndexAliasItemInfo> indexAliasProvider;
    private readonly IInfoProvider<AzureSearchIndexAliasIndexItemInfo> indexAliasIndexProvider;
    private readonly IInfoProvider<AzureSearchIncludedPathItemInfo> pathProvider;
    private readonly IInfoProvider<AzureSearchContentTypeItemInfo> contentTypeProvider;
    private readonly IInfoProvider<AzureSearchIndexLanguageItemInfo> languageProvider;
    private readonly IInfoProvider<AzureSearchReusableContentTypeItemInfo> reusableContentTypeProvider;
    private readonly IEventLogService eventLogService;


    public DefaultAzureSearchConfigurationStorageService(
        IInfoProvider<AzureSearchIndexItemInfo> indexProvider,
        IInfoProvider<AzureSearchIndexAliasItemInfo> indexAliasProvider,
        IInfoProvider<AzureSearchIndexAliasIndexItemInfo> indexAliasIndexProvider,
        IInfoProvider<AzureSearchIncludedPathItemInfo> pathProvider,
        IInfoProvider<AzureSearchContentTypeItemInfo> contentTypeProvider,
        IInfoProvider<AzureSearchIndexLanguageItemInfo> languageProvider,
        IInfoProvider<AzureSearchReusableContentTypeItemInfo> reusableContentTypeProvider,
        IEventLogService eventLogService
    )
    {
        this.indexProvider = indexProvider;
        this.indexAliasProvider = indexAliasProvider;
        this.pathProvider = pathProvider;
        this.contentTypeProvider = contentTypeProvider;
        this.languageProvider = languageProvider;
        this.indexAliasIndexProvider = indexAliasIndexProvider;
        this.reusableContentTypeProvider = reusableContentTypeProvider;
        this.eventLogService = eventLogService;
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
        var indexExists = indexProvider.Get()
            .Column(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemId))
            .WhereEquals(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemIndexName), configuration.IndexName)
            .TopN(1)
            .GetScalarResult<int>() > 0;

        if (indexExists)
        {
            eventLogService.LogError(nameof(DefaultAzureSearchConfigurationStorageService), nameof(TryCreateIndex), $"Index with name '{configuration.IndexName}' already exists.");
            return false;
        }

        var newInfo = new AzureSearchIndexItemInfo()
        {
            AzureSearchIndexItemIndexName = configuration.IndexName ?? string.Empty,
            AzureSearchIndexItemChannelName = configuration.ChannelName ?? string.Empty,
            AzureSearchIndexItemStrategyName = configuration.StrategyName ?? string.Empty,
            AzureSearchIndexItemRebuildHook = configuration.RebuildHook ?? string.Empty
        };

        indexProvider.Set(newInfo);

        configuration.Id = newInfo.AzureSearchIndexItemId;

        foreach (string? language in configuration.LanguageNames)
        {
            var languageInfo = new AzureSearchIndexLanguageItemInfo()
            {
                AzureSearchIndexLanguageItemName = language,
                AzureSearchIndexLanguageItemIndexItemId = newInfo.AzureSearchIndexItemId
            };

            languageInfo.Insert();
        }

        foreach (var path in configuration.Paths)
        {
            var pathInfo = new AzureSearchIncludedPathItemInfo()
            {
                AzureSearchIncludedPathItemAliasPath = path.AliasPath,
                AzureSearchIncludedPathItemIndexItemId = newInfo.AzureSearchIndexItemId
            };
            pathProvider.Set(pathInfo);

            foreach (var contentType in path.ContentTypes)
            {
                var contentInfo = new AzureSearchContentTypeItemInfo()
                {
                    AzureSearchContentTypeItemContentTypeName = contentType.ContentTypeName,
                    AzureSearchContentTypeItemIncludedPathItemId = pathInfo.AzureSearchIncludedPathItemId,
                    AzureSearchContentTypeItemIndexItemId = newInfo.AzureSearchIndexItemId
                };
                contentInfo.Insert();
            }
        }

        if (configuration.ReusableContentTypeNames is not null)
        {
            foreach (string? reusableContentTypeName in configuration.ReusableContentTypeNames)
            {
                var reusableContentTypeItemInfo = new AzureSearchReusableContentTypeItemInfo()
                {
                    AzureSearchReusableContentTypeItemContentTypeName = reusableContentTypeName,
                    AzureSearchReusableContentTypeItemIndexItemId = newInfo.AzureSearchIndexItemId
                };

                reusableContentTypeItemInfo.Insert();
            }
        }

        return true;
    }


    public bool TryCreateAlias(AzureSearchAliasConfigurationModel configuration)
    {
        var aliasExists = indexAliasProvider.Get()
            .Column(nameof(AzureSearchIndexAliasItemInfo.AzureSearchIndexAliasItemId))
            .WhereEquals(nameof(AzureSearchIndexAliasItemInfo.AzureSearchIndexAliasItemIndexAliasName), configuration.AliasName)
            .TopN(1)
            .GetScalarResult<int>() > 0;

        if (aliasExists)
        {
            eventLogService.LogError(nameof(DefaultAzureSearchConfigurationStorageService), nameof(TryCreateAlias), $"Alias with name '{configuration.AliasName}' already exists.");
            return false;
        }

        var aliasInfo = new AzureSearchIndexAliasItemInfo()
        {
            AzureSearchIndexAliasItemIndexAliasName = configuration.AliasName ?? string.Empty,
        };

        var indexId = indexProvider
            .Get()
            .TopN(1)
            .Column(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemId))
            .WhereEquals(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemIndexName), configuration.IndexName)
            .GetScalarResult<int>();

        indexAliasProvider.Set(aliasInfo);

        var indexAliasIndexInfo = new AzureSearchIndexAliasIndexItemInfo()
        {
            AzureSearchIndexAliasIndexItemIndexAliasId = aliasInfo.AzureSearchIndexAliasItemId,
            AzureSearchIndexAliasIndexItemIndexItemId = indexId
        };

        indexAliasIndexProvider.Set(indexAliasIndexInfo);

        configuration.Id = aliasInfo.AzureSearchIndexAliasItemId;

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

        var contentTypesInfoItems = contentTypeProvider
            .Get()
            .WhereEquals(nameof(AzureSearchContentTypeItemInfo.AzureSearchContentTypeItemIndexItemId), indexInfo.AzureSearchIndexItemId)
            .GetEnumerableTypedResult();

        var contentTypes = DataClassInfoProvider.ProviderObject
            .Get()
            .WhereIn(
                nameof(DataClassInfo.ClassName),
                contentTypesInfoItems
                    .Select(x => x.AzureSearchContentTypeItemContentTypeName)
                    .ToArray()
            ).GetEnumerableTypedResult()
            .Select(x => new AzureSearchIndexContentType(x.ClassName, x.ClassDisplayName));

        var reusableContentTypes = reusableContentTypeProvider.Get().WhereEquals(nameof(AzureSearchReusableContentTypeItemInfo.AzureSearchReusableContentTypeItemIndexItemId), indexInfo.AzureSearchIndexItemId).GetEnumerableTypedResult();

        var languages = languageProvider.Get().WhereEquals(nameof(AzureSearchIndexLanguageItemInfo.AzureSearchIndexLanguageItemIndexItemId), indexInfo.AzureSearchIndexItemId).GetEnumerableTypedResult();

        return new AzureSearchConfigurationModel(indexInfo, languages, paths, contentTypes, contentTypesInfoItems, reusableContentTypes);
    }


    public AzureSearchAliasConfigurationModel? GetAliasDataOrNull(int aliasId)
    {
        var aliasInfo = indexAliasProvider.Get().WithID(aliasId).FirstOrDefault();
        if (aliasInfo == default)
        {
            return default;
        }

        var indexAliasIndexIndexInfoId = indexAliasIndexProvider.Get()
            .TopN(1)
            .WhereEquals(nameof(AzureSearchIndexAliasIndexItemInfo.AzureSearchIndexAliasIndexItemIndexAliasId), aliasId)
            .Column(nameof(AzureSearchIndexAliasIndexItemInfo.AzureSearchIndexAliasIndexItemIndexItemId))
            .GetScalarResult<int>();

        var indexName = indexProvider.Get()
            .TopN(1)
            .WhereEquals(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemId), indexAliasIndexIndexInfoId)
            .Column(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemIndexName))
            .GetScalarResult<string>();

        return new AzureSearchAliasConfigurationModel(aliasInfo, indexName);
    }


    public List<int> GetIndexIds() => indexProvider.Get().Select(x => x.AzureSearchIndexItemId).ToList();


    public List<int> GetAliasIds() => indexAliasProvider.Get().Select(x => x.AzureSearchIndexAliasItemId).ToList();


    public IEnumerable<AzureSearchConfigurationModel> GetAllIndexData()
    {
        var indexInfos = indexProvider.Get().GetEnumerableTypedResult().ToList();
        if (indexInfos.Count == 0)
        {
            return [];
        }

        var paths = pathProvider.Get().ToList();

        var contentTypesInfoItems = contentTypeProvider
           .Get()
           .GetEnumerableTypedResult()
           .ToList();

        var contentTypes = DataClassInfoProvider.ProviderObject
            .Get()
            .WhereIn(
                nameof(DataClassInfo.ClassName),
                contentTypesInfoItems
                    .Select(x => x.AzureSearchContentTypeItemContentTypeName)
                    .ToArray()
            ).GetEnumerableTypedResult()
            .Select(x => new AzureSearchIndexContentType(x.ClassName, x.ClassDisplayName))
            .ToList();

        var languages = languageProvider.Get().ToList();

        var reusableContentTypes = reusableContentTypeProvider.Get().ToList();

        return indexInfos.Select(index => new AzureSearchConfigurationModel(index, languages, paths, contentTypes, contentTypesInfoItems, reusableContentTypes));
    }


    public IEnumerable<AzureSearchAliasConfigurationModel> GetAllAliasData()
    {
        var aliasInfoIds = indexAliasProvider.Get().GetEnumerableTypedResult().Select(x => x.AzureSearchIndexAliasItemId).ToList();
        if (aliasInfoIds.Count == 0)
        {
            return [];
        }

        var result = new List<AzureSearchAliasConfigurationModel>();

        foreach (int aliasInfoId in aliasInfoIds)
        {
            var aliasData = GetAliasDataOrNull(aliasInfoId);

            if (aliasData is not null)
            {
                result.Add(aliasData);
            }
        }

        return result;
    }


    public bool TryEditIndex(AzureSearchConfigurationModel configuration)
    {
        configuration.IndexName = RemoveWhitespacesUsingStringBuilder(configuration.IndexName ?? string.Empty);

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
        indexAliasIndexProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchIndexAliasIndexItemInfo.AzureSearchIndexAliasIndexItemIndexItemId)} = {configuration.Id}"));

        indexInfo.AzureSearchIndexItemRebuildHook = configuration.RebuildHook ?? string.Empty;
        indexInfo.AzureSearchIndexItemStrategyName = configuration.StrategyName ?? string.Empty;
        indexInfo.AzureSearchIndexItemChannelName = configuration.ChannelName ?? string.Empty;
        indexInfo.AzureSearchIndexItemIndexName = configuration.IndexName ?? string.Empty;

        indexProvider.Set(indexInfo);

        foreach (string? language in configuration.LanguageNames)
        {
            var languageInfo = new AzureSearchIndexLanguageItemInfo()
            {
                AzureSearchIndexLanguageItemName = language,
                AzureSearchIndexLanguageItemIndexItemId = indexInfo.AzureSearchIndexItemId,
            };

            languageProvider.Set(languageInfo);
        }

        foreach (var path in configuration.Paths)
        {
            var pathInfo = new AzureSearchIncludedPathItemInfo()
            {
                AzureSearchIncludedPathItemAliasPath = path.AliasPath,
                AzureSearchIncludedPathItemIndexItemId = indexInfo.AzureSearchIndexItemId,
            };
            pathProvider.Set(pathInfo);

            foreach (var contentType in path.ContentTypes)
            {
                var contentInfo = new AzureSearchContentTypeItemInfo()
                {
                    AzureSearchContentTypeItemContentTypeName = contentType.ContentTypeName ?? string.Empty,
                    AzureSearchContentTypeItemIncludedPathItemId = pathInfo.AzureSearchIncludedPathItemId,
                    AzureSearchContentTypeItemIndexItemId = indexInfo.AzureSearchIndexItemId,
                };
                contentInfo.Insert();
            }
        }

        RemoveUnusedReusableContentTypes(configuration);
        SetNewIndexReusableContentTypeItems(configuration, indexInfo);

        return true;
    }


    public bool TryEditAlias(AzureSearchAliasConfigurationModel configuration)
    {
        configuration.AliasName = RemoveWhitespacesUsingStringBuilder(configuration.AliasName ?? string.Empty);

        var aliasInfo = indexAliasProvider.Get()
            .WhereEquals(nameof(AzureSearchIndexAliasItemInfo.AzureSearchIndexAliasItemId), configuration.Id)
            .TopN(1)
            .FirstOrDefault();

        indexAliasIndexProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchIndexAliasIndexItemInfo.AzureSearchIndexAliasIndexItemIndexAliasId)} = {configuration.Id}"));

        if (aliasInfo is null)
        {
            return false;
        }

        aliasInfo.AzureSearchIndexAliasItemIndexAliasName = configuration.AliasName ?? string.Empty;

        var indexId = indexProvider
            .Get()
            .TopN(1)
            .Column(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemId))
            .WhereEquals(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemIndexName), configuration.IndexName)
            .GetScalarResult<int>();

        var indexAliasIndexInfo = new AzureSearchIndexAliasIndexItemInfo()
        {
            AzureSearchIndexAliasIndexItemIndexAliasId = aliasInfo.AzureSearchIndexAliasItemId,
            AzureSearchIndexAliasIndexItemIndexItemId = indexId
        };

        indexAliasIndexProvider.Set(indexAliasIndexInfo);

        indexAliasProvider.Set(aliasInfo);

        return true;
    }


    public bool TryDeleteIndex(int id)
    {
        indexProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemId)} = {id}"));
        pathProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchIncludedPathItemInfo.AzureSearchIncludedPathItemIndexItemId)} = {id}"));
        languageProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchIndexLanguageItemInfo.AzureSearchIndexLanguageItemIndexItemId)} = {id}"));
        contentTypeProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchContentTypeItemInfo.AzureSearchContentTypeItemIndexItemId)} = {id}"));
        indexAliasIndexProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchIndexAliasIndexItemInfo.AzureSearchIndexAliasIndexItemIndexItemId)} = {id}"));
        reusableContentTypeProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchReusableContentTypeItemInfo.AzureSearchReusableContentTypeItemIndexItemId)} = {id}"));

        return true;
    }


    public bool TryDeleteAlias(int id)
    {
        indexAliasProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchIndexAliasItemInfo.AzureSearchIndexAliasItemId)} = {id}"));
        indexAliasIndexProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchIndexAliasIndexItemInfo.AzureSearchIndexAliasIndexItemIndexAliasId)} = {id}"));

        return true;
    }


    private void RemoveUnusedReusableContentTypes(AzureSearchConfigurationModel configuration)
    {
        var removeReusableContentTypesQuery = reusableContentTypeProvider
            .Get()
            .WhereEquals(nameof(AzureSearchReusableContentTypeItemInfo.AzureSearchReusableContentTypeItemIndexItemId), configuration.Id)
            .WhereNotIn(nameof(AzureSearchReusableContentTypeItemInfo.AzureSearchReusableContentTypeItemContentTypeName), configuration.ReusableContentTypeNames.ToArray());

        reusableContentTypeProvider.BulkDelete(new WhereCondition(removeReusableContentTypesQuery));
    }


    private void SetNewIndexReusableContentTypeItems(AzureSearchConfigurationModel configuration, AzureSearchIndexItemInfo indexInfo)
    {
        var newReusableContentTypes = GetNewReusableContentTypesOnIndex(configuration);

        foreach (string? reusableContentType in newReusableContentTypes)
        {
            var reusableContentTypeInfo = new AzureSearchReusableContentTypeItemInfo()
            {
                AzureSearchReusableContentTypeItemContentTypeName = reusableContentType,
                AzureSearchReusableContentTypeItemIndexItemId = indexInfo.AzureSearchIndexItemId,
            };

            reusableContentTypeProvider.Set(reusableContentTypeInfo);
        }
    }


    private IEnumerable<string> GetNewReusableContentTypesOnIndex(AzureSearchConfigurationModel configuration)
    {
        var existingReusableContentTypes = reusableContentTypeProvider
            .Get()
            .WhereEquals(nameof(AzureSearchReusableContentTypeItemInfo.AzureSearchReusableContentTypeItemIndexItemId), configuration.Id)
            .GetEnumerableTypedResult();

        return configuration.ReusableContentTypeNames.Where(x => !existingReusableContentTypes.Any(y => y.AzureSearchReusableContentTypeItemContentTypeName == x));
    }
}
