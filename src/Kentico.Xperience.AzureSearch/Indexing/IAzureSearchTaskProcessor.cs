namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Processes tasks from <see cref="AzureSearchQueueWorker"/>.
/// </summary>
public interface IAzureSearchTaskProcessor
{
    /// <summary>
    /// Processes multiple queue items from all AzureSearch indexes in batches. AzureSearch
    /// automatically applies batching in multiples of 1,000 when using their API,
    /// so all queue items are forwarded to the API.
    /// </summary>
    /// <param name="queueItems">The items to process.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <param name="maximumBatchSize"></param>
    /// <returns>The number of items processed.</returns>

    Task<int> ProcessAzureSearchTasks(IEnumerable<AzureSearchQueueItem> queueItems, CancellationToken cancellationToken, int maximumBatchSize = 100);

}
