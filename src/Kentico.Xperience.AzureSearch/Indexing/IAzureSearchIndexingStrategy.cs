using Azure.Search.Documents;
using Azure.Search.Documents.Indexes.Models;

namespace Kentico.Xperience.AzureSearch.Indexing;

public interface IAzureSearchIndexingStrategy
{
    /// <summary>
    /// Maps a content item (web page or reusable) to an Azure Search document for indexing.
    /// Return null to exclude the item from the index.
    /// </summary>
    /// <param name="item">The content item (web page or reusable) being indexed.</param>
    /// <returns>The search document to index, or null to skip.</returns>
    Task<IAzureSearchModel?> MapToAzureSearchModelOrNull(IIndexEventItemModel item);

    /// <summary>
    /// Called when a web page item is modified. Returns the set of items to reindex (typically including the changed page).
    /// </summary>
    /// <param name="changedItem">The web page item that was modified.</param>
    /// <returns>Items to pass to <see cref="MapToAzureSearchModelOrNull"/> for indexing.</returns>
    Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventWebPageItemModel changedItem);

    /// <summary>
    /// Called when a reusable content item is modified. Returns the set of items to reindex (e.g. the reusable item itself or web pages that reference it).
    /// </summary>
    /// <param name="changedItem">The reusable content item that was modified.</param>
    /// <returns>Items to pass to <see cref="MapToAzureSearchModelOrNull"/> for indexing.</returns>
    Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventReusableItemModel changedItem);

    /// <summary>
    /// Called when creating the search index. Return a configuration to enable Semantic Ranking, or null to skip it.
    /// </summary>
    /// <returns>Semantic ranking configuration, or null if not used.</returns>
    SemanticRankingConfiguration? CreateSemanticRankingConfigurationOrNull();

    /// <summary>
    /// Called when creating the Azure Search index. Returned fields are combined with properties from <see cref="BaseAzureSearchModel"/>.
    /// </summary>
    /// <returns>The search fields for the index.</returns>
    IList<SearchField> GetSearchFields();

    /// <summary>
    /// Uploads documents to the Azure Search index. The models are typically produced by <see cref="MapToAzureSearchModelOrNull"/>.
    /// </summary>
    /// <param name="models">The search documents to upload.</param>
    /// <param name="searchClient">The Azure Search client for the index.</param>
    /// <returns>The number of documents successfully uploaded.</returns>
    Task<int> UploadDocuments(IEnumerable<IAzureSearchModel> models, SearchClient searchClient);
}
