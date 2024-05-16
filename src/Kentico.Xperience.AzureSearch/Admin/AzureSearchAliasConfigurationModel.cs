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
    public string AliasName { get; set; } = "";

    [GeneralSelectorComponent(dataProviderType: typeof(ExistingIndexOptionsProvider), Label = "Index Names", Order = 2)]
    [MinLength(1, ErrorMessage = "You must select at least one index name")]
    public IEnumerable<string> IndexNames { get; set; } = Enumerable.Empty<string>();

    public AzureSearchAliasConfigurationModel()
    { }

    public AzureSearchAliasConfigurationModel(
        AzureSearchIndexAliasItemInfo alias,
        IEnumerable<string> aliasIndexes)
    {
        Id = alias.AzureSearchIndexAliasItemId;
        AliasName = alias.AzureSearchIndexAliasItemIndexAliasName;
        IndexNames = aliasIndexes;
    }
}
