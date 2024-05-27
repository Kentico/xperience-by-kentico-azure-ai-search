namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// A queued item to be processed by <see cref="AzureSearchQueueWorker"/> which
/// represents a recent change made to an indexed <see cref="ItemToIndex"/> which is a representation of a <see cref="IIndexEventItemModel"/>.
/// </summary>
public sealed class AzureSearchQueueItem
{
    /// <summary>
    /// The <see cref="IIndexEventItemModel"/> that was changed.
    /// </summary>
    public IIndexEventItemModel ItemToIndex { get; }

    /// <summary>
    /// The type of the AzureSearch task.
    /// </summary>
    public AzureSearchTaskType TaskType { get; }

    /// <summary>
    /// The code name of the AzureSearch index to be updated.
    /// </summary>
    public string IndexName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchQueueItem"/> class.
    /// </summary>
    /// <param name="itemToIndex">The <see cref="IIndexEventItemModel"/> that was changed.</param>
    /// <param name="taskType">The type of the AzureSearch task.</param>
    /// <param name="indexName">The code name of the AzureSearch index to be updated.</param>
    /// <exception cref="ArgumentNullException" />
    public AzureSearchQueueItem(IIndexEventItemModel itemToIndex, AzureSearchTaskType taskType, string indexName)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        if (taskType != AzureSearchTaskType.PUBLISH_INDEX && itemToIndex == null)
        {
            throw new ArgumentNullException(nameof(itemToIndex));
        }

        ItemToIndex = itemToIndex;
        TaskType = taskType;
        IndexName = indexName;
    }
}
