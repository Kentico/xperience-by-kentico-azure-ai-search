using Azure.Search.Documents.Indexes.Models;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Semantic configuration which can be added to AzureSearchIndex
/// </summary>
public class SemanticRankingConfiguration
{
    /// <summary>
    /// Gets the suggesters for the index.
    /// </summary>
    public IList<SearchSuggester> Suggesters { get; }

    /// <summary> Defines parameters for a search index that influence semantic capabilities. </summary>
    public SemanticSearch SemanticSearch { get; set; }

    /// <summary>
    /// Creates Configuration with empty list of <see cref="SearchSuggester"/>
    /// </summary>
    /// <param name="semanticSearch"></param>
    public SemanticRankingConfiguration(SemanticSearch semanticSearch)
    {
        SemanticSearch = semanticSearch;
        Suggesters = [];
    }

    /// <summary>
    /// Creates Configuration with defined list of <see cref="SearchSuggester"/>
    /// </summary>
    /// <param name="semanticSearch"></param>
    /// <param name="suggesters"></param>
    public SemanticRankingConfiguration(SemanticSearch semanticSearch, IList<SearchSuggester> suggesters)
    {
        SemanticSearch = semanticSearch;
        Suggesters = suggesters;
    }
}
