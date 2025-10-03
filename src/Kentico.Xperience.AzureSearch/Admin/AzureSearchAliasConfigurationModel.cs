using System.ComponentModel.DataAnnotations;

using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace Kentico.Xperience.AzureSearch.Admin;

public class AzureSearchAliasConfigurationModel
{
    public int Id { get; set; }

    [TextInputComponent(
        Label = "Alias Name",
        Order = 1)]
    [Required]
    [MinLength(1)]
    [MaxLength(128)]
    [RegularExpression("^(?!-)[a-z0-9-]+(?<!-)$", ErrorMessage = "Alias name must only contain lowercase letters, digits or dashes, cannot start or end with dashes and is limited to 128 characters.")]
    public string AliasName { get; set; } = string.Empty;

    [SingleGeneralSelectorComponent(dataProviderType: typeof(ExistingIndexOptionsProvider), Label = "Index Name", Order = 2)]
    [Required(ErrorMessage = "You must select an index name")]
    public string IndexName { get; set; } = string.Empty;

    public AzureSearchAliasConfigurationModel()
    { }

    public AzureSearchAliasConfigurationModel(
        AzureSearchIndexAliasItemInfo alias,
        string aliasIndex)
    {
        Id = alias.AzureSearchIndexAliasItemId;
        AliasName = alias.AzureSearchIndexAliasItemIndexAliasName;
        IndexName = aliasIndex;
    }
}
