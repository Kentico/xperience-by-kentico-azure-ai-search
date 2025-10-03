using Kentico.Xperience.AzureSearch.Admin;

namespace Kentico.Xperience.AzureSearch.Aliasing;

/// <summary>
/// Represents the configuration of an AzureSearch index alias.
/// </summary>
public sealed class AzureSearchIndexAlias
{
    /// <summary>
    /// An arbitrary ID used to identify the AzureSearch index in the admin UI.
    /// </summary>
    public int Identifier { get; set; }

    /// <summary>
    /// The Name of the AzureSearch index alias.
    /// </summary>
    public string AliasName { get; }

    /// <summary>
    /// The code name of the AzureSearch index which is aliased.
    /// </summary>
    public string IndexName { get; }

    internal AzureSearchIndexAlias(AzureSearchAliasConfigurationModel aliasConfiguration)
    {
        Identifier = aliasConfiguration.Id;
        IndexName = aliasConfiguration.IndexName;
        AliasName = aliasConfiguration.AliasName;
    }
}
