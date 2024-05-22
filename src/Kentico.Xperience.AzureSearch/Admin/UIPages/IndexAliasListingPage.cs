using CMS.Core;
using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.AzureSearch.Admin;
using Kentico.Xperience.AzureSearch.Aliasing;
using Kentico.Xperience.AzureSearch.Indexing;

using Microsoft.Extensions.Options;

[assembly: UIPage(
   parentType: typeof(IndexListingPage),
   slug: "indexes",
   uiPageType: typeof(IndexAliasListingPage),
   name: "List of registered Azure AI Search index aliases",
   templateName: TemplateNames.LISTING,
   order: UIPageOrder.NoOrder)]

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// An admin UI page that displays statistics about the registered AzureSearch indexes.
/// </summary>
[UIEvaluatePermission(SystemPermissions.VIEW)]
internal class IndexAliasListingPage : ListingPage
{
    private readonly IAzureSearchClient azureSearchClient;
    private readonly IPageUrlGenerator pageUrlGenerator;
    private readonly IAzureSearchIndexAliasService azureSearchIndexAliasService;
    private readonly IAzureSearchConfigurationStorageService configurationStorageService;
    private readonly AzureSearchOptions azureSearchOptions;

    protected override string ObjectType => AzureSearchIndexAliasItemInfo.OBJECT_TYPE;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexListingPage"/> class.
    /// </summary>
    public IndexAliasListingPage(
        IAzureSearchClient azureSearchClient,
        IAzureSearchIndexAliasService azureSearchIndexAliasService,
        IOptions<AzureSearchOptions> azureSearchOptions,
        IPageUrlGenerator pageUrlGenerator,
        IAzureSearchConfigurationStorageService configurationStorageService)
    {
        this.azureSearchClient = azureSearchClient;
        this.pageUrlGenerator = pageUrlGenerator;
        this.azureSearchIndexAliasService = azureSearchIndexAliasService;
        this.configurationStorageService = configurationStorageService;
        this.azureSearchOptions = azureSearchOptions.Value;
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
            if (!AzureSearchIndexAliasStore.Instance.GetAllAliases().Any())
            {
                PageConfiguration.Callouts =
                [
                    new()
                    {
                        Headline = "No aliases",
                        Content = "No AzureSearch index aliases registered. See <a target='_blank' href='https://github.com/Kentico/kentico-xperience-azuresearch'>our instructions</a> to read more about creating and registering AzureSearch alias indexes.",
                        ContentAsHtml = true,
                        Type = CalloutType.FriendlyWarning,
                        Placement = CalloutPlacement.OnDesk
                    }
                ];
            }

            PageConfiguration.ColumnConfigurations
                .AddColumn(nameof(AzureSearchIndexAliasItemInfo.AzureSearchIndexAliasItemId), "ID", defaultSortDirection: SortTypeEnum.Asc, sortable: true)
                .AddColumn(nameof(AzureSearchIndexAliasItemInfo.AzureSearchIndexAliasItemIndexAliasName), "Name", sortable: true, searchable: true);

            PageConfiguration.AddEditRowAction<IndexAliasEditPage>();
            PageConfiguration.TableActions.AddCommand("Rebuild Aliased Index", nameof(Rebuild), icon: Icons.RotateRight);
            PageConfiguration.TableActions.AddDeleteAction(nameof(Delete), "Delete Alias");
            PageConfiguration.HeaderActions.AddLink<IndexAliasCreatePage>("Create Alias");
            PageConfiguration.HeaderActions.AddLink<IndexListingPage>("Indexes");
        }

        await base.ConfigurePage();
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
        var alias = AzureSearchIndexAliasStore.Instance.GetAlias(id);

        if (alias == null)
        {
            return ResponseFrom(result)
                .AddErrorMessage(string.Format("Error loading AzureSearch index alias with identifier {0}.", id));
        }

        foreach (string indexName in alias.IndexNames)
        {
            var index = AzureSearchIndexStore.Instance.GetIndex(indexName);

            if (index is null)
            {
                return ResponseFrom(result)
                    .AddErrorMessage(string.Format("Error loading AzureSearch aliased index with name {0}.", indexName));
            }

            try
            {
                await azureSearchClient.Rebuild(index.IndexName, cancellationToken);
            }
            catch (Exception ex)
            {
                EventLogService.LogException(nameof(IndexAliasListingPage), nameof(Rebuild), ex);

                return ResponseFrom(result)
                   .AddErrorMessage(string.Format("Errors occurred while rebuilding the '{0}' index. Please check the Event Log for more details.", index.IndexName));
            }
        }
        return ResponseFrom(result)
                    .AddSuccessMessage("Indexing in progress. Visit your AzureSearch dashboard for details about the indexing process.");
    }

    [PageCommand(Permission = SystemPermissions.DELETE)]
    public async Task<INavigateResponse> Delete(int id, CancellationToken cancellationToken)
    {
        var response = NavigateTo(pageUrlGenerator.GenerateUrl<IndexAliasListingPage>());
        var alias = AzureSearchIndexAliasStore.Instance.GetAlias(id);
        if (alias == null)
        {
            return response
                .AddErrorMessage(string.Format("Error deleting AzureSearch index alias with identifier {0}.", id));
        }
        try
        {
            bool res = configurationStorageService.TryDeleteAlias(id);

            if (res)
            {
                AzureSearchIndexAliasStore.SetAliases(configurationStorageService);

                await azureSearchIndexAliasService.DeleteAlias(alias.AliasName, cancellationToken);
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
               .AddErrorMessage(string.Format("Errors occurred while deleting the '{0}' index. Please check the Event Log for more details.", alias.IndexNames));
        }
    }
}
