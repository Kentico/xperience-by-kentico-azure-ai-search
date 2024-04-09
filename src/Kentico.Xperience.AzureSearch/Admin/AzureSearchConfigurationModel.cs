using System.ComponentModel.DataAnnotations;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;

namespace Kentico.Xperience.AzureSearch.Admin;

public class AzureSearchConfigurationModel
{
    public int Id { get; set; }

    [TextInputComponent(
        Label = "Index Name",
        Order = 1)]
    [Required]
    [MinLength(1)]
    [MaxLength(128)]
    [RegularExpression("^(?!-)[a-z0-9-]+(?<!-)$", ErrorMessage = "Index name must only contain lowercase letters, digits or dashes, cannot start or end with dashes and is limited to 128 characters.")]
    public string IndexName { get; set; } = "";

    [GeneralSelectorComponent(dataProviderType: typeof(LanguageOptionsProvider), Label = "Indexed Languages", Order = 2)]
    [MinLength(1, ErrorMessage = "You must select at least one Language Name")]
    public IEnumerable<string> LanguageNames { get; set; } = Enumerable.Empty<string>();

    [DropDownComponent(Label = "Channel Name", DataProviderType = typeof(ChannelOptionsProvider), Order = 3)]
    [Required]
    public string ChannelName { get; set; } = "";

    [DropDownComponent(Label = "Indexing Strategy", DataProviderType = typeof(IndexingStrategyOptionsProvider), Order = 4, ExplanationText = "Changing strategy which has an incompatible configuration will result in deleting indexed items.")]
    [Required]
    public string StrategyName { get; set; } = "";

    [TextInputComponent(Label = "Rebuild Hook")]
    public string RebuildHook { get; set; } = "";

    [AzureSearchIndexConfigurationComponent(Label = "Included Paths")]
    public IEnumerable<AzureSearchIndexIncludedPath> Paths { get; set; } = Enumerable.Empty<AzureSearchIndexIncludedPath>();

    public AzureSearchConfigurationModel() { }

    public AzureSearchConfigurationModel(
        AzureSearchIndexItemInfo index,
        IEnumerable<AzureSearchIndexLanguageItemInfo> indexLanguages,
        IEnumerable<AzureSearchIncludedPathItemInfo> indexPaths,
        IEnumerable<AzureSearchIndexContentType> contentTypes
    )
    {
        Id = index.AzureSearchIndexItemId;
        IndexName = index.AzureSearchIndexItemIndexName;
        ChannelName = index.AzureSearchIndexItemChannelName;
        RebuildHook = index.AzureSearchIndexItemRebuildHook;
        StrategyName = index.AzureSearchIndexItemStrategyName;
        LanguageNames = indexLanguages
            .Where(l => l.AzureSearchIndexLanguageItemIndexItemId == index.AzureSearchIndexItemId)
            .Select(l => l.AzureSearchIndexLanguageItemName)
            .ToList();
        Paths = indexPaths
            .Where(p => p.AzureSearchIncludedPathItemIndexItemId == index.AzureSearchIndexItemId)
            .Select(p => new AzureSearchIndexIncludedPath(p, contentTypes))
            .ToList();
    }
}
