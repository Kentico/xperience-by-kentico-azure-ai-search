using Azure.Search.Documents;
using Azure.Search.Documents.Indexes.Models;

namespace Kentico.Xperience.AzureSearch.Indexing;

public interface IAzureSearchIndexingStrategy
{
    /// <summary>
    /// Called when indexing a search model. Enables overriding of multiple fields with custom data.
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
    /// Called when creating AzureSearch Index.
    /// Expects a list of <see cref="SearchField"/> objects corresponding to the fields which will be retrieved from the index on searching. 
    /// Some Fields are added by default. These fields can be found in <see cref="IAzureSearchModel"/>.
    /// </summary>
    /// The type for which fields will be created, based on its properties.
    /// <returns>A collection of fields.</returns>
    IList<SearchField> GetSearchFields();

    Task<int> UploadDocuments(IEnumerable<IAzureSearchModel> models, SearchClient searchClient);
}
