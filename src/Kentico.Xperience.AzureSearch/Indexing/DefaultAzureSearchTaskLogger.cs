using CMS.Core;
using CMS.Websites;
using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Default implementation of <see cref="IAzureSearchTaskLogger"/>.
/// </summary>
internal class DefaultAzureSearchTaskLogger : IAzureSearchTaskLogger
{
    private readonly IEventLogService eventLogService;
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultAzureSearchTaskLogger"/> class.
    /// </summary>
    public DefaultAzureSearchTaskLogger(IEventLogService eventLogService, IServiceProvider serviceProvider)
    {
        this.eventLogService = eventLogService;
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task HandleEvent(IndexEventWebPageItemModel webpageItem, string eventName)
    {
        var taskType = GetTaskType(eventName);

        foreach (var azureSearchIndex in AzureSearchIndexStore.Instance.GetAllIndices())
        {
            if (!webpageItem.IsIndexedByIndex(eventLogService, azureSearchIndex.IndexName, eventName))
            {
                continue;
            }

            var strategy = serviceProvider.GetRequiredStrategy(azureSearchIndex);
            var toReindex = await strategy.FindItemsToReindex(webpageItem);

            foreach (var item in toReindex)
            {
                if (item.ItemGuid == webpageItem.ItemGuid)
                {
                    if (taskType == AzureSearchTaskType.DELETE)
                    {
                        LogIndexTask(new AzureSearchQueueItem(item, AzureSearchTaskType.DELETE, azureSearchIndex.IndexName));
                    }
                    else
                    {
                        LogIndexTask(new AzureSearchQueueItem(item, AzureSearchTaskType.UPDATE, azureSearchIndex.IndexName));
                    }
                }
            }
        }
    }

    public async Task HandleReusableItemEvent(IndexEventReusableItemModel reusableItem, string eventName)
    {
        foreach (var azureSearchIndex in AzureSearchIndexStore.Instance.GetAllIndices())
        {
            if (!reusableItem.IsIndexedByIndex(eventLogService, azureSearchIndex.IndexName, eventName))
            {
                continue;
            }

            var strategy = serviceProvider.GetRequiredStrategy(azureSearchIndex);
            var toReindex = await strategy.FindItemsToReindex(reusableItem);

            foreach (var item in toReindex)
            {
                LogIndexTask(new AzureSearchQueueItem(item, AzureSearchTaskType.UPDATE, azureSearchIndex.IndexName));
            }
        }
    }

    /// <summary>
    /// Logs a single <see cref="AzureSearchQueueItem"/>.
    /// </summary>
    /// <param name="task">The task to log.</param>
    private void LogIndexTask(AzureSearchQueueItem task)
    {
        try
        {
            AzureSearchQueueWorker.EnqueueAzureSearchQueueItem(task);
        }
        catch (InvalidOperationException ex)
        {
            eventLogService.LogException(nameof(DefaultAzureSearchTaskLogger), nameof(LogIndexTask), ex);
        }
    }

    private static AzureSearchTaskType GetTaskType(string eventName)
    {
        if (eventName.Equals(WebPageEvents.Publish.Name, StringComparison.OrdinalIgnoreCase))
        {
            return AzureSearchTaskType.UPDATE;
        }

        if (eventName.Equals(WebPageEvents.Delete.Name, StringComparison.OrdinalIgnoreCase) ||
            eventName.Equals(WebPageEvents.Archive.Name, StringComparison.OrdinalIgnoreCase))
        {
            return AzureSearchTaskType.DELETE;
        }

        return AzureSearchTaskType.UNKNOWN;
    }
}
