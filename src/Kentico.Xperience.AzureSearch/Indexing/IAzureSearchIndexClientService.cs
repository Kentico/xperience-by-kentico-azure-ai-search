using Azure.Search.Documents;
using Azure.Search.Documents.Indexes.Models;

using Kentico.Xperience.AzureSearch.Admin;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Initializes <see cref="SearchClient" /> instances.
/// </summary>
internal interface IAzureSearchIndexClientService
{
    /// <summary>
    /// Initializes a new <see cref="SearchIndex" /> for the given <paramref name="indexName" />.
    /// </summary>
    /// <param name="indexName">The code name of the index.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="InvalidOperationException">Thrown when the index with the given <paramref name="indexName"/> doesn't exist.</exception>
    /// <exception cref="Azure.RequestFailedException">Thrown when a failure is returned by the Search service.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="indexName"/> is null or empty.</exception>
    /// <returns>The initialized <see cref="SearchClient" />.</returns>
    Task<SearchClient> InitializeIndexClient(string indexName, CancellationToken cancellationToken);


    /// <summary>
    /// Edits the AzureSearch index in Azure.
    /// </summary>
    /// <param name="oldIndex">The index to edit.</param>
    /// <param name="newIndexConfiguration">New index configuration.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="Azure.RequestFailedException">Thrown when a failure is returned by the Search service.</exception>
    /// <returns>The edited <see cref="SearchIndex" />.</returns>
    Task<SearchIndex> EditIndex(AzureSearchIndex oldIndex, AzureSearchConfigurationModel newIndexConfiguration, CancellationToken cancellationToken);


    /// <summary>
    /// Creates a new AzureSearch index in Azure.
    /// </summary>
    /// <param name="configurationModel">The configuration model for the new index.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="Azure.RequestFailedException">Thrown when a failure is returned by the Search service.</exception>
    /// <returns>The created <see cref="SearchIndex" />.</returns>
    Task<SearchIndex> CreateIndex(AzureSearchConfigurationModel configurationModel, CancellationToken cancellationToken);


    /// <summary>
    /// Creates a new AzureSearch index in Azure.
    /// </summary>
    /// <param name="azureSearchIndex">The AzureSearch index to create.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="Azure.RequestFailedException">Thrown when a failure is returned by the Search service.</exception>
    /// <returns>The created <see cref="SearchIndex" />.</returns>
    Task<SearchIndex> CreateIndex(AzureSearchIndex azureSearchIndex, CancellationToken cancellationToken);
}
