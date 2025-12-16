using Azure.Search.Documents;
using Azure.Search.Documents.Indexes.Models;

namespace Kentico.Xperience.AzureSearch.Indexing;

public interface IAzureSearchIndexingStrategy
{
    /// <summary>
    /// Called when indexing a search model. Enables overriding of multiple fields with custom data
    /// </summary>
    /// <param name="item">The <see cref="IIndexEventItemModel"/> currently being indexed.</param>
    /// <returns>Modified AzureSearch document.</returns>
    Task<IAzureSearchModel?> MapToAzureSearchModelOrNull(IIndexEventItemModel item);

    /// <summary>
    /// Triggered by modifications to a web page item, which is provided to determine what other items should be included for indexing
    /// </summary>
    /// <param name="changedItem">The web page item that was modified</param>
    /// <returns>Items that should be passed to <see cref="MapToAzureSearchModelOrNull"/> for indexing</returns>
    Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventWebPageItemModel changedItem);

    /// <summary>
    /// Triggered by modifications to a reusable content item, which is provided to determine what other items should be included for indexing
    /// </summary>
    /// <param name="changedItem">The reusable content item that was modified</param>
    /// <returns>Items that should be passed to <see cref="MapToAzureSearchModelOrNull"/> for indexing</returns>
    Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventReusableItemModel changedItem);

    /// <summary>
    /// Called when creating a SearchIndex to optionally add Semantic Ranking
    /// </summary>
    /// <returns><see cref="SemanticRankingConfiguration"/></returns>
    SemanticRankingConfiguration? CreateSemanticRankingConfigurationOrNull();

    /// <summary>
    /// Called when creating a SearchIndex to optionally add Vector Embedding Configuration
    /// </summary>
    /// <returns><see cref="SemanticRankingConfiguration"/></returns>
    VectorSearch? CreateVectorEmbeddingConfigurationOrNull();

    /// <summary>
    /// Called when creating AzureSearch Index and united with properties from <see cref="BaseAzureSearchModel"/>
    /// </summary>
    /// <returns>A collection of user defined search fields.</returns>
    IList<SearchField> GetSearchFields();

    /// <summary>
    /// Called when uploading data to user defined <see cref="AzureSearchIndex"/>.
    /// Expects an <see cref="IEnumerable{IAzureSearchModel}"/> created in <see cref="MapToAzureSearchModelOrNull"/>
    /// </summary>
    /// <returns>Number of uploaded documents</returns>
    Task<int> UploadDocuments(IEnumerable<IAzureSearchModel> models, SearchClient searchClient);
}
