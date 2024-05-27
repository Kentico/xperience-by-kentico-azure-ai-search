using Kentico.Xperience.AzureSearch.Admin;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Contains methods to interface with the AzureSearch API.
/// </summary>
public interface IAzureSearchClient
{
    /// <summary>
    /// Removes records from the AzureSearch index.
    /// </summary>
    /// <param name="itemGuids">The AzureSearch internal IDs of the records to delete.</param>
    /// <param name="indexName">The index containing the objects to delete.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// 
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ObjectDisposedException" />
    /// <exception cref="OverflowException" />
    /// <returns>The number of records deleted.</returns>
    Task<int> DeleteRecords(IEnumerable<string> itemGuids, string indexName, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the indices of the AzureSearch application with basic statistics.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// 
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ObjectDisposedException" />
    Task<ICollection<AzureSearchIndexStatisticsViewModel>> GetStatistics(CancellationToken cancellationToken);

    /// <summary>
    /// Updates the AzureSearch index with the dynamic data in each object of the passed <paramref name="models"/>.
    /// </summary>
    /// <remarks>Logs an error if there are issues loading the node data.</remarks>
    /// <param name="models">The document to upsert into AzureSearch.</param>
    /// <param name="indexName">The index to upsert the data to.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ObjectDisposedException" />
    /// <exception cref="OverflowException" />
    /// <returns>The number of objects processed.</returns>
    Task<int> UpsertRecords(IEnumerable<IAzureSearchModel> models, string indexName, CancellationToken cancellationToken);

    /// <summary>
    /// Rebuilds the AzureSearch index by removing existing data from AzureSearch and indexing all
    /// pages in the content tree included in the index.
    /// </summary>
    /// <param name="indexName">The index to rebuild.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ObjectDisposedException" />
    Task Rebuild(string indexName, CancellationToken? cancellationToken);

    /// <summary>
    /// Deletes the AzureSearch index by removing existing index data from Azure.
    /// </summary>
    /// <param name="indexName">The index to delete.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="indexName"/> is null.</exception>
    /// <exception cref="Azure.RequestFailedException">Thrown when a failure is returned by the Search service.</exception>
    Task DeleteIndex(string indexName, CancellationToken cancellationToken);
}
