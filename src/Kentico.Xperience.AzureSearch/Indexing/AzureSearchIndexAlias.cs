using Kentico.Xperience.AzureSearch.Admin;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Represents the configuration of an AzureSearch index alias.
/// </summary>
internal class AzureSearchIndexAlias
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
    public IEnumerable<string> IndexNames { get; }

    internal AzureSearchIndexAlias(AzureSearchAliasConfigurationModel aliasConfiguration)
    {
        Identifier = aliasConfiguration.Id;
        IndexNames = aliasConfiguration.IndexNames;
        AliasName = aliasConfiguration.AliasName;
    }
}
