using System.ComponentModel.DataAnnotations;

using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// Represents the model for configuring an Azure Search index alias in the admin interface.
/// </summary>
public class AzureSearchAliasConfigurationModel
{
    /// <summary>
    /// Identifier of the alias configuration.
    /// </summary>
    public int Id { get; set; }


    /// <summary>
    /// Alias name for the Azure Search index.
    /// </summary>
    [TextInputComponent(
        Label = "Alias Name",
        Order = 1)]
    [RequiredValidationRule]
    [MaxLength(128)]
    [RegularExpression("^(?!-)[a-z0-9-]+(?<!-)$", ErrorMessage = "Alias name must only contain lowercase letters, digits or dashes, cannot start or end with dashes and is limited to 128 characters.")]
    public string AliasName { get; set; } = string.Empty;


    /// <summary>
    /// Index name to which the alias will point.
    /// </summary>
    [SingleGeneralSelectorComponent(dataProviderType: typeof(ExistingIndexOptionsProvider), Label = "Index Name", Order = 2)]
    [RequiredValidationRule]
    public string IndexName { get; set; } = string.Empty;


    /// <summary>
    /// Index name to which the alias will point.
    /// </summary>
    [Obsolete("Use IndexName property instead. This property will be removed in future versions.")]
    public IEnumerable<string> IndexNames { get; set; } = Enumerable.Empty<string>();


    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchAliasConfigurationModel"/> class.
    /// </summary>
    public AzureSearchAliasConfigurationModel()
    { }


    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchAliasConfigurationModel"/> class based on the provided alias information and index name.
    /// </summary>
    /// <param name="alias">The alias information.</param>
    /// <param name="aliasIndex">The index name to which the alias will point.</param>
    public AzureSearchAliasConfigurationModel(
        AzureSearchIndexAliasItemInfo alias,
        string aliasIndex)
    {
        Id = alias.AzureSearchIndexAliasItemId;
        AliasName = alias.AzureSearchIndexAliasItemIndexAliasName;
        IndexName = aliasIndex;
    }
}
