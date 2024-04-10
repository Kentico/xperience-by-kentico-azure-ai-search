using System.Text;
using CMS.DataEngine;

namespace Kentico.Xperience.AzureSearch.Admin;

internal class DefaultAzureSearchConfigurationStorageService : IAzureSearchConfigurationStorageService
{
    private readonly IAzureSearchIndexItemInfoProvider indexProvider;
    private readonly IAzureSearchIndexAliasItemInfoProvider indexAliasProvider;
    private readonly IAzureSearchIndexAliasIndexItemInfoProvider indexAliasIndexProvider;
    private readonly IAzureSearchIncludedPathItemInfoProvider pathProvider;
    private readonly IAzureSearchContentTypeItemInfoProvider contentTypeProvider;
    private readonly IAzureSearchIndexLanguageItemInfoProvider languageProvider;

    public DefaultAzureSearchConfigurationStorageService(
        IAzureSearchIndexItemInfoProvider indexProvider,
        IAzureSearchIndexAliasItemInfoProvider indexAliasProvider,
        IAzureSearchIndexAliasIndexItemInfoProvider indexAliasIndexProvider,
        IAzureSearchIncludedPathItemInfoProvider pathProvider,
        IAzureSearchContentTypeItemInfoProvider contentTypeProvider,
        IAzureSearchIndexLanguageItemInfoProvider languageProvider
    )
    {
        this.indexProvider = indexProvider;
        this.indexAliasProvider = indexAliasProvider;
        this.pathProvider = pathProvider;
        this.contentTypeProvider = contentTypeProvider;
        this.languageProvider = languageProvider;
        this.indexAliasIndexProvider = indexAliasIndexProvider;
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

        return true;
    }

    public bool TryCreateAlias(AzureSearchAliasConfigurationModel configuration)
    {
        var existingAliases = indexAliasProvider.Get()
            .WhereEquals(nameof(AzureSearchIndexAliasItemInfo.AzureSearchIndexAliasItemIndexAliasName), configuration.AliasName)
            .TopN(1)
            .FirstOrDefault();

        if (existingAliases is not null)
        {
            return false;
        }

        var aliasInfo = new AzureSearchIndexAliasItemInfo()
        {
            AzureSearchIndexAliasItemIndexAliasName = configuration.AliasName ?? "",
        };

        var indexIds = indexProvider
            .Get()
            .Where(index => configuration.IndexNames.Any(name => index.AzureSearchIndexItemIndexName == name))
            .Select(index => index.AzureSearchIndexItemId)
            .ToList();

        indexAliasProvider.Set(aliasInfo);

        foreach (int indexId in indexIds)
        {
            var indexAliasIndexInfo = new AzureSearchIndexAliasIndexItemInfo()
            {
                AzureSearchIndexAliasIndexItemIndexAliasId = aliasInfo.AzureSearchIndexAliasItemId,
                AzureSearchIndexAliasIndexItemIndexItemId = indexId
            };

            indexAliasIndexProvider.Set(indexAliasIndexInfo);
        }

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

        var languages = languageProvider.Get().WhereEquals(nameof(AzureSearchIndexLanguageItemInfo.AzureSearchIndexLanguageItemIndexItemId), indexInfo.AzureSearchIndexItemId).GetEnumerableTypedResult();

        return new AzureSearchConfigurationModel(indexInfo, languages, paths, contentTypes);
    }

    public AzureSearchAliasConfigurationModel? GetAliasDataOrNull(int aliasId)
    {
        var aliasInfo = indexAliasProvider.Get().WithID(aliasId).FirstOrDefault();
        if (aliasInfo == default)
        {
            return default;
        }

        var indexAliasIndexIndexInfoIds = indexAliasIndexProvider.Get()
            .WhereEquals(nameof(AzureSearchIndexAliasIndexItemInfo.AzureSearchIndexAliasIndexItemIndexAliasId), aliasId)
            .GetEnumerableTypedResult()
            .Select(indexAliasIndex => indexAliasIndex.AzureSearchIndexAliasIndexItemIndexItemId);

        var indexNames = indexProvider.Get()
            .WhereIn(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemId), indexAliasIndexIndexInfoIds.ToList())
            .GetEnumerableTypedResult()
            .Select(index => index.AzureSearchIndexItemIndexName);

        return new AzureSearchAliasConfigurationModel(aliasInfo, indexNames);
    }

    public List<int> GetIndexIds() => indexProvider.Get().Select(x => x.AzureSearchIndexItemId).ToList();

    public List<int> GetAliasIds() => indexAliasProvider.Get().Select(x => x.AzureSearchIndexAliasItemId).ToList();

    public IEnumerable<AzureSearchConfigurationModel> GetAllIndexData()
    {
        var indexInfos = indexProvider.Get().GetEnumerableTypedResult().ToList();
        if (indexInfos.Count == 0)
        {
            return new List<AzureSearchConfigurationModel>();
        }

        var paths = pathProvider.Get().ToList();

        var contentTypesInfoItems = contentTypeProvider
           .Get()
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

        var languages = languageProvider.Get().ToList();

        return indexInfos.Select(index => new AzureSearchConfigurationModel(index, languages, paths, contentTypes));
    }

    public IEnumerable<AzureSearchAliasConfigurationModel> GetAllAliasData()
    {
        var aliasInfoIds = indexAliasProvider.Get().GetEnumerableTypedResult().Select(x => x.AzureSearchIndexAliasItemId).ToList();
        if (aliasInfoIds.Count == 0)
        {
            return new List<AzureSearchAliasConfigurationModel>();
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
        indexAliasIndexProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchIndexAliasIndexItemInfo.AzureSearchIndexAliasIndexItemIndexItemId)} = {configuration.Id}"));

        indexInfo.AzureSearchIndexItemRebuildHook = configuration.RebuildHook ?? "";
        indexInfo.AzureSearchIndexItemStrategyName = configuration.StrategyName ?? "";
        indexInfo.AzureSearchIndexItemChannelName = configuration.ChannelName ?? "";
        indexInfo.AzureSearchIndexItemIndexName = configuration.IndexName ?? "";

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
                    AzureSearchContentTypeItemContentTypeName = contentType.ContentTypeName ?? "",
                    AzureSearchContentTypeItemIncludedPathItemId = pathInfo.AzureSearchIncludedPathItemId,
                    AzureSearchContentTypeItemIndexItemId = indexInfo.AzureSearchIndexItemId,
                };
                contentInfo.Insert();
            }
        }

        return true;
    }

    public bool TryEditAlias(AzureSearchAliasConfigurationModel configuration)
    {
        configuration.AliasName = RemoveWhitespacesUsingStringBuilder(configuration.AliasName ?? "");

        var aliasInfo = indexAliasProvider.Get()
            .WhereEquals(nameof(AzureSearchIndexAliasItemInfo.AzureSearchIndexAliasItemId), configuration.Id)
            .TopN(1)
            .FirstOrDefault();

        indexAliasIndexProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchIndexAliasIndexItemInfo.AzureSearchIndexAliasIndexItemIndexItemId)} = {configuration.Id}"));

        if (aliasInfo is null)
        {
            return false;
        }

        aliasInfo.AzureSearchIndexAliasItemIndexAliasName = configuration.AliasName ?? "";

        var indexIds = indexProvider
            .Get()
            .Where(index => configuration.IndexNames.Any(name => index.AzureSearchIndexItemIndexName == name))
            .Select(index => index.AzureSearchIndexItemId)
            .ToList();

        foreach (int indexId in indexIds)
        {
            var indexAliasIndexInfo = new AzureSearchIndexAliasIndexItemInfo()
            {
                AzureSearchIndexAliasIndexItemIndexAliasId = aliasInfo.AzureSearchIndexAliasItemId,
                AzureSearchIndexAliasIndexItemIndexItemId = indexId
            };

            indexAliasIndexProvider.Set(indexAliasIndexInfo);
        }

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

        return true;
    }

    public bool TryDeleteAlias(int id)
    {
        indexAliasProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchIndexAliasItemInfo.AzureSearchIndexAliasItemId)} = {id}"));
        indexAliasIndexProvider.BulkDelete(new WhereCondition($"{nameof(AzureSearchIndexAliasIndexItemInfo.AzureSearchIndexAliasIndexItemIndexAliasId)} = {id}"));

        return true;
    }
}
