using CMS.Base;
using CMS.Core;

using Kentico.Xperience.AzureSearch.Indexing;

namespace Kentico.Xperience.AzureSearch;

/// <summary>
/// Thread worker which enqueues recently updated or deleted nodes indexed
/// by AzureSearch and processes the tasks in the background thread.
/// </summary>
internal class AzureSearchQueueWorker : ThreadQueueWorker<AzureSearchQueueItem, AzureSearchQueueWorker>
{
    private readonly IAzureSearchTaskProcessor azureSearchTaskProcessor;

    /// <inheritdoc />
    protected override int DefaultInterval => 10000;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchQueueWorker"/> class.
    /// Should not be called directly- the worker should be initialized during startup using
    /// <see cref="ThreadWorker{T}.EnsureRunningThread"/>.
    /// </summary>
    public AzureSearchQueueWorker() => azureSearchTaskProcessor = Service.Resolve<IAzureSearchTaskProcessor>() ?? throw new InvalidOperationException($"{nameof(IAzureSearchTaskProcessor)} is not registered.");

    /// <summary>
    /// Adds an <see cref="AzureSearchQueueItem"/> to the worker queue to be processed.
    /// </summary>
    /// <param name="queueItem">The item to be added to the queue.</param>
    /// <exception cref="InvalidOperationException" />
    public static void EnqueueAzureSearchQueueItem(AzureSearchQueueItem queueItem)
    {
        if ((queueItem.ItemToIndex == null && queueItem.TaskType != AzureSearchTaskType.PUBLISH_INDEX) || string.IsNullOrEmpty(queueItem.IndexName))
        {
            return;
        }

        if (queueItem.TaskType == AzureSearchTaskType.UNKNOWN)
        {
            return;
        }

        if (AzureSearchIndexStore.Instance.GetIndex(queueItem.IndexName) == null)
        {
            throw new InvalidOperationException($"Attempted to log task for AzureSearch index '{queueItem.IndexName},' but it is not registered.");
        }

        Current.Enqueue(queueItem, false);
    }

    /// <inheritdoc />
    protected override void Finish() => RunProcess();

    /// <inheritdoc/>
    protected override void ProcessItem(AzureSearchQueueItem item)
    {
    }

    /// <inheritdoc />
    protected override int ProcessItems(IEnumerable<AzureSearchQueueItem> items) =>
         azureSearchTaskProcessor.ProcessAzureSearchTasks(items, CancellationToken.None).GetAwaiter().GetResult();

}
