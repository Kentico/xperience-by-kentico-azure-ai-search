using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Default indexing strategy that provides simple indexing.
/// Search model <typeparamref name="TSearchModel"/> used for Index definition and data retrieval.
/// </summary>
public class BaseAzureSearchIndexingStrategy<TSearchModel> : IAzureSearchIndexingStrategy where TSearchModel : IAzureSearchModel, new()
{
    private readonly FieldBuilder fieldBuilder = new();

    /// <inheritdoc />
    public virtual Task<IAzureSearchModel?> MapToAzureSearchModelOrNull(IIndexEventItemModel item)
    {
        if (item.IsSecured)
        {
            return Task.FromResult<IAzureSearchModel?>(null);
        }

        var indexDocument = new TSearchModel()
        {
            Name = item.Name
        };

        return Task.FromResult<IAzureSearchModel?>(indexDocument);
    }

    /// <inheritdoc />
    public virtual Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventWebPageItemModel changedItem) => Task.FromResult<IEnumerable<IIndexEventItemModel>>([changedItem]);

    /// <inheritdoc />
    /// <remarks>
    /// Default implementation returns the reusable content item itself so it is indexed directly.
    /// If you index the reusable content indirectly via web page items that reference it, override this method
    /// to return those web page items (<see cref="IndexEventWebPageItemModel"/>) so they get reindexed when the reusable content item changes.
    /// See docs/Custom-index-strategy.md and docs/Managing-Indexes.md for more information.
    /// </remarks>
    public virtual Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventReusableItemModel changedItem) => Task.FromResult<IEnumerable<IIndexEventItemModel>>([changedItem]);

    /// <inheritdoc />
    public virtual SemanticRankingConfiguration? CreateSemanticRankingConfigurationOrNull() => null;

    /// <inheritdoc />
    public IList<SearchField> GetSearchFields() => fieldBuilder.Build(typeof(TSearchModel));

    /// <inheritdoc />
    public virtual async Task<int> UploadDocuments(IEnumerable<IAzureSearchModel> models, SearchClient searchClient)
    {
        var batch = new IndexDocumentsBatch<TSearchModel>();

        foreach (var model in models)
        {
            batch.Actions.Add(IndexDocumentsAction.Upload((TSearchModel)model));
        }

        IndexDocumentsResult result = await searchClient.IndexDocumentsAsync(batch);

        return result.Results.Count(x => x.Succeeded);
    }
}
