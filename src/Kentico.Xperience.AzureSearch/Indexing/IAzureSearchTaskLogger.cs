namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Contains methods for logging <see cref="AzureSearchQueueItem"/>s and <see cref="AzureSearchQueueItem"/>s
/// for processing by <see cref="AzureSearchQueueWorker"/> and <see cref="AzureSearchQueueWorker"/>.
/// </summary>
public interface IAzureSearchTaskLogger
{
    /// <summary>
    /// Logs an <see cref="AzureSearchQueueItem"/> for each registered crawler. Then, loops
    /// through all registered AzureSearch indexes and logs a task if the passed <paramref name="webpageItem"/> is indexed.
    /// </summary>
    /// <param name="webpageItem">The <see cref="IndexEventWebPageItemModel"/> that triggered the event.</param>
    /// <param name="eventName">The name of the Xperience event that was triggered.</param>
    Task HandleEvent(IndexEventWebPageItemModel webpageItem, string eventName);

    Task HandleReusableItemEvent(IndexEventReusableItemModel reusableItem, string eventName);
}
