using Azure.Search.Documents;
using Azure.Search.Documents.Indexes.Models;

using Kentico.Xperience.AzureSearch.Admin;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Initializes <see cref="SearchClient" /> instances.
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

    /// <summary>
    /// Edits the AzureSearch index in Azure.
    /// </summary>
    /// <param name="oldIndexName">The name of index to edit.</param>
    /// <param name="newIndexConfiguration">New index configuration.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="Azure.RequestFailedException">Thrown when a failure is returned by the Search service.</exception>
    Task EditIndex(string oldIndexName, AzureSearchConfigurationModel newIndexConfiguration, CancellationToken cancellationToken);
}
