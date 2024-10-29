using Kentico.Xperience.AzureSearch.Admin;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Represents the configuration of an AzureSearch index.
/// </summary>
public sealed class AzureSearchIndex
{
    /// <summary>
    /// An arbitrary ID used to identify the AzureSearch index in the admin UI.
    /// </summary>
    public int Identifier { get; set; }

    /// <summary>
    /// The code name of the AzureSearch index.
    /// </summary>
    public string IndexName { get; }

    /// <summary>
    /// The Name of the WebSiteChannel.
    /// </summary>
    public string WebSiteChannelName { get; }

    /// <summary>
    /// The Language used on the WebSite on the Channel which is indexed.
    /// </summary>
    public List<string> LanguageNames { get; }

    /// <summary>
    /// A list of reusable content types that will be indexed.
    /// </summary>
    public List<string> IncludedReusableContentTypes { get; }

    /// <summary>
    /// The type of the class which extends <see cref="AzureSearchIndexingStrategyType"/>.
    /// </summary>
    public Type AzureSearchIndexingStrategyType { get; }

    internal IEnumerable<AzureSearchIndexIncludedPath> IncludedPaths { get; set; }

    internal AzureSearchIndex(AzureSearchConfigurationModel indexConfiguration, Dictionary<string, Type> strategies)
    {
        Identifier = indexConfiguration.Id;
        IndexName = indexConfiguration.IndexName;
        WebSiteChannelName = indexConfiguration.ChannelName;
        LanguageNames = indexConfiguration.LanguageNames.ToList();
        IncludedPaths = indexConfiguration.Paths;
        IncludedReusableContentTypes = indexConfiguration.ReusableContentTypeNames.ToList();

        var strategy = typeof(BaseAzureSearchIndexingStrategy<BaseAzureSearchModel>);

        if (strategies.ContainsKey(indexConfiguration.StrategyName))
        {
            strategy = strategies[indexConfiguration.StrategyName];
        }

        AzureSearchIndexingStrategyType = strategy;
    }
}
