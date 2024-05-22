using CMS.Base;
using CMS.Core;
using CMS.Websites;

using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Xperience.AzureSearch.Indexing;

internal class AzureSearchBatchResult
{
    internal int SuccessfulOperations { get; set; } = 0;
    internal HashSet<AzureSearchIndex> PublishedIndices { get; set; } = [];
}

internal class DefaultAzureSearchTaskProcessor : IAzureSearchTaskProcessor
{
    private readonly IWebPageUrlRetriever urlRetriever;
    private readonly IServiceProvider serviceProvider;
    private readonly IAzureSearchClient azureSearchClient;
    private readonly IEventLogService eventLogService;

    public DefaultAzureSearchTaskProcessor(
        IAzureSearchClient azureSearchClient,
        IEventLogService eventLogService,
        IWebPageUrlRetriever urlRetriever,
        IServiceProvider serviceProvider)
    {
        this.azureSearchClient = azureSearchClient;
        this.eventLogService = eventLogService;
        this.urlRetriever = urlRetriever;
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task<int> ProcessAzureSearchTasks(IEnumerable<AzureSearchQueueItem> queueItems, CancellationToken cancellationToken, int maximumBatchSize = 100)
    {
        AzureSearchBatchResult batchResults = new();

        var batches = queueItems.Batch(maximumBatchSize);

        foreach (var batch in batches)
        {
            await ProcessAzureSearchBatch(batch, batchResults, cancellationToken);
        }

        return batchResults.SuccessfulOperations;
    }

    private async Task ProcessAzureSearchBatch(IEnumerable<AzureSearchQueueItem> queueItems, AzureSearchBatchResult previousBatchResults, CancellationToken cancellationToken)
    {
        var groups = queueItems.GroupBy(item => item.IndexName);

        foreach (var group in groups)
        {
            try
            {
                var deleteIds = new List<string>();
                var deleteTasks = group.Where(queueItem => queueItem.TaskType == AzureSearchTaskType.DELETE).ToList();

                var updateTasks = group.Where(queueItem => queueItem.TaskType is AzureSearchTaskType.PUBLISH_INDEX or AzureSearchTaskType.UPDATE);
                var upsertData = new List<IAzureSearchModel>();

                foreach (var queueItem in updateTasks)
                {
                    var document = await GetSearchModel(queueItem);
                    if (document is not null)
                    {
                        upsertData.Add(document);
                    }
                    else
                    {
                        deleteTasks.Add(queueItem);
                    }
                }

                deleteIds.AddRange(GetIdsToDelete(deleteTasks ?? []).Where(x => x is not null).Select(x => x ?? ""));

                if (AzureSearchIndexStore.Instance.GetIndex(group.Key) is { } index)
                {
                    previousBatchResults.SuccessfulOperations += await azureSearchClient.DeleteRecords(deleteIds, group.Key, cancellationToken);
                    previousBatchResults.SuccessfulOperations += await azureSearchClient.UpsertRecords(upsertData, group.Key, cancellationToken);

                    if (group.Any(t => t.TaskType == AzureSearchTaskType.PUBLISH_INDEX) && !previousBatchResults.PublishedIndices.Any(x => x.IndexName == index.IndexName))
                    {
                        previousBatchResults.PublishedIndices.Add(index);
                    }
                }
                else
                {
                    eventLogService.LogError(nameof(DefaultAzureSearchTaskProcessor), nameof(ProcessAzureSearchTasks), "Index instance not exists");
                }
            }
            catch (Exception ex)
            {
                eventLogService.LogError(nameof(DefaultAzureSearchTaskProcessor), nameof(ProcessAzureSearchTasks), ex.Message);
            }
        }
    }

    private static IEnumerable<string?> GetIdsToDelete(IEnumerable<AzureSearchQueueItem> deleteTasks) => deleteTasks.Select(queueItem => queueItem.ItemToIndex.ItemGuid.ToString());

    private async Task<IAzureSearchModel?> GetSearchModel(AzureSearchQueueItem queueItem)
    {
        var azureSearchIndex = AzureSearchIndexStore.Instance.GetRequiredIndex(queueItem.IndexName);

        var strategy = serviceProvider.GetRequiredStrategy(azureSearchIndex);

        var data = await strategy.MapToAzureSearchModelOrNull(queueItem.ItemToIndex);

        if (data is null)
        {
            return null;
        }

        await AddBaseProperties(queueItem.ItemToIndex, data!);

        return data;
    }

    private async Task AddBaseProperties(IIndexEventItemModel item, IAzureSearchModel model)
    {
        model.ContentTypeName = item.ContentTypeName;
        model.LanguageName = item.LanguageName;
        model.ItemGuid = item.ItemGuid.ToString();
        model.ObjectID = item.ItemGuid.ToString();

        if (item is IndexEventWebPageItemModel webpageItem && string.IsNullOrEmpty(model.Url))
        {
            try
            {
                model.Url = (await urlRetriever.Retrieve(webpageItem.WebPageItemTreePath, webpageItem.WebsiteChannelName, webpageItem.LanguageName)).RelativePath;
            }
            catch (Exception)
            {
                // Retrieve can throw an exception when processing a page update AzureSearchQueueItem
                // and the page was deleted before the update task has processed. In this case, upsert an
                // empty URL
                model.Url = string.Empty;
            }
        }
    }
}
