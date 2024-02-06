using Azure.Search.Documents;
using Azure.Search.Documents.Indexes.Models;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Initializes <see cref="SearchIndex" /> instances.
/// </summary>
public interface IAzureSearchIndexClientService
{
    /// <summary>
    /// Initializes a new <see cref="SearchIndex" /> for the given <paramref name="indexName" />
    /// </summary>
    /// <param name="indexName">The code name of the index.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="InvalidOperationException" />
    Task<SearchClient> InitializeIndexClient(string indexName, CancellationToken cancellationToken);
}
