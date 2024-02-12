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

    /// <summary>
    /// Creates the AzureSearch index alias in Azure.
    /// </summary>
    /// <param name="aliasName">The alias to create.</param>
    /// <param name="indexNames">The index to alias.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="aliasName"/> is null.</exception>
    /// <exception cref="Azure.RequestFailedException">Thrown when a failure is returned by the Search service.</exception>
    Task CreateAlias(string aliasName, IEnumerable<string> indexNames, CancellationToken cancellationToken);

    /// <summary>
    /// Edits the AzureSearch index alias in Azure.
    /// </summary>
    /// <param name="oldAliasName">The alias to edit.</param>
    /// <param name="newAlias">New alias.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="oldAliasName"/> is null.</exception>
    /// <exception cref="Azure.RequestFailedException">Thrown when a failure is returned by the Search service.</exception>
    Task EditAlias(string oldAliasName, SearchAlias newAlias, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the AzureSearch index alias by removing existing index alias data from Azure.
    /// </summary>
    /// <param name="aliasName">The index to delete.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="aliasName"/> is null.</exception>
    /// <exception cref="Azure.RequestFailedException">Thrown when a failure is returned by the Search service.</exception>
    Task DeleteAlias(string aliasName, CancellationToken cancellationToken);
}
