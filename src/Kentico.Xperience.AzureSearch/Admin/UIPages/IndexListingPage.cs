using CMS.Core;
using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.AzureSearch.Admin;
using Kentico.Xperience.AzureSearch.Indexing;

using Microsoft.Extensions.Options;

[assembly: UIPage(
   parentType: typeof(AzureSearchApplicationPage),
   slug: "indexes",
   uiPageType: typeof(IndexListingPage),
   name: "List of registered Azure AI Search indices",
   templateName: TemplateNames.LISTING,
   order: UIPageOrder.NoOrder)]

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// An admin UI page that displays statistics about the registered AzureSearch indexes.
/// </summary>
[UIEvaluatePermission(SystemPermissions.VIEW)]
internal class IndexListingPage : ListingPage
{
    private readonly AzureSearchOptions azureSearchOptions;
    private readonly IAzureSearchClient azureSearchClient;
    private readonly IPageUrlGenerator pageUrlGenerator;
    private readonly IAzureSearchConfigurationStorageService configurationStorageService;
    private readonly IConversionService conversionService;

    protected override string ObjectType => AzureSearchIndexItemInfo.OBJECT_TYPE;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexListingPage"/> class.
    /// </summary>
    public IndexListingPage(
        IAzureSearchClient azureSearchClient,
        IPageUrlGenerator pageUrlGenerator,
        IOptions<AzureSearchOptions> azureSearchOptions,
        IAzureSearchConfigurationStorageService configurationStorageService,
        IConversionService conversionService)
    {
        this.azureSearchClient = azureSearchClient;
        this.azureSearchOptions = azureSearchOptions.Value;
        this.pageUrlGenerator = pageUrlGenerator;
        this.configurationStorageService = configurationStorageService;
        this.conversionService = conversionService;
    }

    /// <inheritdoc/>
    public override async Task ConfigurePage()
    {
        if (!azureSearchOptions.SearchServiceEnabled)
        {
            PageConfiguration.Callouts =
            [
                new()
                {
                    Headline = "Indexing is disabled",
                    Content = "Indexing is disabled. See <a target='_blank' href='https://github.com/Kentico/kentico-xperience-azuresearch'>our instructions</a> to read more about AzureSearch alias indexes.",
                    ContentAsHtml = true,
                    Type = CalloutType.FriendlyWarning,
                    Placement = CalloutPlacement.OnDesk
                }
            ];
        }
        else
        {

            if (!AzureSearchIndexStore.Instance.GetAllIndices().Any())
            {
                PageConfiguration.Callouts =
                [
                    new()
                    {
                        Headline = "No indexes",
                        Content = "No AzureSearch indexes registered. See <a target='_blank' href='https://github.com/Kentico/kentico-xperience-azuresearch'>our instructions</a> to read more about creating and registering AzureSearch indexes.",
                        ContentAsHtml = true,
                        Type = CalloutType.FriendlyWarning,
                        Placement = CalloutPlacement.OnDesk
                    }
                ];
            }

            PageConfiguration.ColumnConfigurations
                .AddColumn(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemId), "ID", defaultSortDirection: SortTypeEnum.Asc, sortable: true)
                .AddColumn(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemIndexName), "Name", sortable: true, searchable: true)
                .AddColumn(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemChannelName), "Channel", searchable: true, sortable: true)
                .AddColumn(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemStrategyName), "Index Strategy", searchable: true, sortable: true)
                .AddColumn(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemId), "Entries", sortable: true);

            PageConfiguration.AddEditRowAction<IndexEditPage>();
            PageConfiguration.TableActions.AddCommand("Rebuild", nameof(Rebuild), icon: Icons.RotateRight);
            PageConfiguration.TableActions.AddDeleteAction(nameof(Delete), "Delete");
            PageConfiguration.HeaderActions.AddLink<IndexCreatePage>("Create Index");
            PageConfiguration.HeaderActions.AddLink<IndexAliasListingPage>("Index Aliases");
        }

        await base.ConfigurePage();
    }

    protected override async Task<LoadDataResult> LoadData(LoadDataSettings settings, CancellationToken cancellationToken)
    {
        if (!azureSearchOptions.SearchServiceEnabled)
        {
            return new();
        }

        var result = await base.LoadData(settings, cancellationToken);

        var statistics = await azureSearchClient.GetStatistics(default);
        // Add statistics for indexes that are registered but not created in AzureSearch
        AddMissingStatistics(ref statistics);

        if (PageConfiguration.ColumnConfigurations is not List<ColumnConfiguration> columns)
        {
            return result;
        }

        int entriesColIndex = columns.FindIndex(c => c.Caption == "Entries");

        foreach (var row in result.Rows)
        {
            if (row.Cells is not List<Cell> cells)
            {
                continue;
            }

            var stats = GetStatistic(row, statistics);

            if (stats is null)
            {
                continue;
            }

            if (cells[entriesColIndex] is StringCell entriesCell)
            {
                entriesCell.Value = stats.Entries.ToString();
            }
        }

        return result;
    }

    private AzureSearchIndexStatisticsViewModel? GetStatistic(Row row, ICollection<AzureSearchIndexStatisticsViewModel> statistics)
    {
        int indexId = conversionService.GetInteger(row.Identifier, 0);
        string indexName = AzureSearchIndexStore.Instance.GetIndex(indexId) is AzureSearchIndex index
            ? index.IndexName
            : "";

        return statistics.FirstOrDefault(s => string.Equals(s.Name, indexName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// A page command which rebuilds an AzureSearch index.
    /// </summary>
    /// <param name="id">The ID of the row whose action was performed, which corresponds with the internal
    /// <see cref="AzureSearchIndex.Identifier"/> to rebuild.</param>
    /// <param name="cancellationToken">The cancellation token for the action.</param>
    [PageCommand(Permission = AzureSearchIndexPermissions.REBUILD)]
    public async Task<ICommandResponse<RowActionResult>> Rebuild(int id, CancellationToken cancellationToken)
    {
        var result = new RowActionResult(false);
        var index = AzureSearchIndexStore.Instance.GetIndex(id);

        if (index is null)
        {
            return ResponseFrom(result)
                .AddErrorMessage(string.Format("Error loading AzureSearch index with identifier {0}.", id));
        }
        try
        {
            await azureSearchClient.Rebuild(index.IndexName, cancellationToken);

            return ResponseFrom(result)
                .AddSuccessMessage("Indexing in progress. Visit your AzureSearch dashboard for details about the indexing process.");
        }
        catch (Exception ex)
        {
            EventLogService.LogException(nameof(IndexListingPage), nameof(Rebuild), ex);

            return ResponseFrom(result)
               .AddErrorMessage(string.Format("Errors occurred while rebuilding the '{0}' index. Please check the Event Log for more details.", index.IndexName));
        }
    }

    [PageCommand(Permission = SystemPermissions.DELETE)]
    public async Task<INavigateResponse> Delete(int id, CancellationToken cancellationToken)
    {
        var response = NavigateTo(pageUrlGenerator.GenerateUrl<IndexListingPage>());
        var index = AzureSearchIndexStore.Instance.GetIndex(id);
        if (index == null)
        {
            return response
                .AddErrorMessage(string.Format("Error deleting AzureSearch index with identifier {0}.", id));
        }
        try
        {
            await azureSearchClient.DeleteIndex(index.IndexName, cancellationToken);
            bool res = configurationStorageService.TryDeleteIndex(id);
            if (res)
            {
                AzureSearchIndexStore.SetIndicies(configurationStorageService);
            }
            else
            {
                return response
                    .AddErrorMessage(string.Format("Error deleting AzureSearch index with identifier {0}.", id));
            }

            return response.AddSuccessMessage("Index deletion in progress. Visit your Azure dashboard for details about your indexes.");
        }
        catch (Exception ex)
        {
            EventLogService.LogException(nameof(IndexListingPage), nameof(Delete), ex);
            return response
               .AddErrorMessage(string.Format("Errors occurred while deleting the '{0}' index. Please check the Event Log for more details.", index.IndexName));
        }
    }

    private static void AddMissingStatistics(ref ICollection<AzureSearchIndexStatisticsViewModel> statistics)
    {
        foreach (string indexName in AzureSearchIndexStore.Instance.GetAllIndices().Select(i => i.IndexName))
        {
            if (!statistics.Any(stat => stat.Name?.Equals(indexName, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                statistics.Add(new AzureSearchIndexStatisticsViewModel
                {
                    Name = indexName,
                    Entries = 0,
                });
            }
        }
    }
}
