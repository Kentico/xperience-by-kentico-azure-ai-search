using Azure.Search.Documents.Indexes.Models;

namespace Kentico.Xperience.AzureSearch.Indexing
{
    /// <summary>
    /// Semantic configuration which can be added to a SearchIndex
    /// </summary>
    public class SemanticRankingConfiguration
    {
        /// <summary>
        /// Gets the suggesters for the index.
        /// </summary>
        public IList<SearchSuggester> Suggesters { get; }

        /// <summary> Defines parameters for a search index that influence semantic capabilities. </summary>
        public SemanticSearch SemanticSearch { get; set; }

        public SemanticRankingConfiguration(SemanticSearch semanticSearch)
        {
            SemanticSearch = semanticSearch;
            Suggesters = new List<SearchSuggester>();
        }

        public SemanticRankingConfiguration(SemanticSearch semanticSearch, IList<SearchSuggester> suggesters)
        {
            SemanticSearch = semanticSearch;
            Suggesters = suggesters;
        }
    }
}
