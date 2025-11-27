using System.ComponentModel.DataAnnotations;

using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// Represents the model for configuring an Azure Search index in the admin interface.
/// </summary>
public class AzureSearchConfigurationModel
{
    /// <summary>
    /// Identifier of the index configuration.
    /// </summary>
    public int Id { get; set; }


    /// <summary>
    /// Index name for the Azure Search index.
    /// </summary>
    [TextInputComponent(
        Label = "Index Name",
        Order = 1)]
    [RequiredValidationRule]
    [MaxLengthValidationRule(128)]
    [RegularExpression("^(?!-)[a-z0-9-]+(?<!-)$",
        ErrorMessage = "Index name must only contain lowercase letters, digits or dashes, cannot start or end with dashes and is limited to 128 characters.")]
    [UniqueIndexNameValidationRule]
    [UniqueAliasNameValidationRule]
    public string IndexName { get; set; } = string.Empty;


    /// <summary>
    /// Collection of included paths for the Azure Search index configuration.
    /// </summary>
    [AzureSearchIndexConfigurationComponent(Label = "Included Paths", Order = 2)]
    public IEnumerable<AzureSearchIndexIncludedPath> Paths { get; set; } = [];


    /// <summary>
    /// Reusable content types to include in the index.
    /// </summary>
    [GeneralSelectorComponent(dataProviderType: typeof(ReusableContentOptionsProvider), Label = "Included Reusable Content Types", Order = 3)]
    public IEnumerable<string> ReusableContentTypeNames { get; set; } = [];


    /// <summary>
    /// Languages to index.
    /// </summary>
    [GeneralSelectorComponent(dataProviderType: typeof(LanguageOptionsProvider), Label = "Indexed Languages", Order = 4)]
    [RequiredValidationRule]
    public IEnumerable<string> LanguageNames { get; set; } = [];


    /// <summary>
    /// Name of the channel to index.
    /// </summary>
    [DropDownComponent(Label = "Channel Name", DataProviderType = typeof(ChannelOptionsProvider), Order = 5)]
    [RequiredValidationRule]
    public string ChannelName { get; set; } = string.Empty;


    /// <summary>
    /// Name of the indexing strategy to use.
    /// </summary>
    [DropDownComponent(Label = "Indexing Strategy", DataProviderType = typeof(IndexingStrategyOptionsProvider), Order = 6,
        ExplanationText = "Changing strategy which has an incompatible configuration will result in deleting indexed items.")]
    [RequiredValidationRule]
    public string StrategyName { get; set; } = string.Empty;


    /// <summary>
    /// Hook to trigger a rebuild of the index.
    /// </summary>
    [TextInputComponent(Label = "Rebuild Hook", Order = 7)]
    public string RebuildHook { get; set; } = string.Empty;


    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchConfigurationModel"/> class.
    /// </summary>
    public AzureSearchConfigurationModel() { }


    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchConfigurationModel"/> class.
    /// </summary>
    /// <param name="index">Index information.</param>
    /// <param name="indexLanguages">Languages to index.</param>
    /// <param name="indexPaths">Included paths for the index.</param>
    /// <param name="contentTypes">Content types for the index.</param>
    /// <param name="reusableContentTypes">Reusable content types for the index.</param>
    [Obsolete("This constructor does not support content type filtering by path. Use the constructor with contentTypeItems parameter instead.")]
    public AzureSearchConfigurationModel(
        AzureSearchIndexItemInfo index,
        IEnumerable<AzureSearchIndexLanguageItemInfo> indexLanguages,
        IEnumerable<AzureSearchIncludedPathItemInfo> indexPaths,
        IEnumerable<AzureSearchIndexContentType> contentTypes,
        IEnumerable<AzureSearchReusableContentTypeItemInfo> reusableContentTypes
    )
    {
        Id = index.AzureSearchIndexItemId;
        IndexName = index.AzureSearchIndexItemIndexName;
        ChannelName = index.AzureSearchIndexItemChannelName;
        RebuildHook = index.AzureSearchIndexItemRebuildHook;
        StrategyName = index.AzureSearchIndexItemStrategyName;
        ReusableContentTypeNames = [.. reusableContentTypes
             .Where(c => c.AzureSearchReusableContentTypeItemIndexItemId == index.AzureSearchIndexItemId)
             .Select(c => c.AzureSearchReusableContentTypeItemContentTypeName)];
        LanguageNames = [.. indexLanguages
            .Where(l => l.AzureSearchIndexLanguageItemIndexItemId == index.AzureSearchIndexItemId)
            .Select(l => l.AzureSearchIndexLanguageItemName)];
        Paths = [.. indexPaths
            .Where(p => p.AzureSearchIncludedPathItemIndexItemId == index.AzureSearchIndexItemId)
            .Select(p => new AzureSearchIndexIncludedPath(p, contentTypes))];
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchConfigurationModel"/> class with content type filtering by path.
    /// </summary>
    /// <param name="index">Index information.</param>
    /// <param name="indexLanguages">Languages to index.</param>
    /// <param name="indexPaths">Included paths for the index.</param>
    /// <param name="contentTypes">All content types for the index.</param>
    /// <param name="contentTypeItems">Content type items containing path associations.</param>
    /// <param name="reusableContentTypes">Reusable content types for the index.</param>
    public AzureSearchConfigurationModel(
        AzureSearchIndexItemInfo index,
        IEnumerable<AzureSearchIndexLanguageItemInfo> indexLanguages,
        IEnumerable<AzureSearchIncludedPathItemInfo> indexPaths,
        IEnumerable<AzureSearchIndexContentType> contentTypes,
        IEnumerable<AzureSearchContentTypeItemInfo> contentTypeItems,
        IEnumerable<AzureSearchReusableContentTypeItemInfo> reusableContentTypes
    )
    {
        Id = index.AzureSearchIndexItemId;
        IndexName = index.AzureSearchIndexItemIndexName;
        ChannelName = index.AzureSearchIndexItemChannelName;
        RebuildHook = index.AzureSearchIndexItemRebuildHook;
        StrategyName = index.AzureSearchIndexItemStrategyName;
        ReusableContentTypeNames = [.. reusableContentTypes
             .Where(c => c.AzureSearchReusableContentTypeItemIndexItemId == index.AzureSearchIndexItemId)
             .Select(c => c.AzureSearchReusableContentTypeItemContentTypeName)];
        LanguageNames = [.. indexLanguages
            .Where(l => l.AzureSearchIndexLanguageItemIndexItemId == index.AzureSearchIndexItemId)
            .Select(l => l.AzureSearchIndexLanguageItemName)];

        // Create a dictionary to map content type names to their full objects for efficient lookup
        // Use GroupBy to handle potential duplicate content type names
        var contentTypeDict = contentTypes
            .GroupBy(ct => ct.ContentTypeName)
            .ToDictionary(g => g.Key, g => g.First());

        // Group content type items by path ID for efficient filtering
        var contentTypesByPath = contentTypeItems
            .ToLookup(cti => cti.AzureSearchContentTypeItemIncludedPathItemId);

        Paths = [.. indexPaths
            .Where(pathInfo => pathInfo.AzureSearchIncludedPathItemIndexItemId == index.AzureSearchIndexItemId)
            .Select(pathInfo => new AzureSearchIndexIncludedPath(
                pathInfo,
                contentTypesByPath[pathInfo.AzureSearchIncludedPathItemId]
                    .Select(cti => contentTypeDict.TryGetValue(cti.AzureSearchContentTypeItemContentTypeName, out var contentType) ? contentType : null)
                    .OfType<AzureSearchIndexContentType>()
            ))];
    }
}
