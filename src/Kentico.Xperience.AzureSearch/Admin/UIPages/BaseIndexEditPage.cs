using System.ComponentModel.DataAnnotations;
using System.Text;

using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

using CMS.Core;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.AzureSearch.Indexing;

using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

namespace Kentico.Xperience.AzureSearch.Admin;

internal abstract class BaseIndexEditPage : ModelEditPage<AzureSearchConfigurationModel>
{
    protected readonly IAzureSearchConfigurationStorageService StorageService;


    private readonly IAzureSearchIndexClientService indexClientService;


    private readonly SearchIndexClient indexClient;


    private readonly IEventLogService eventLogService;


    protected BaseIndexEditPage(
        IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        IAzureSearchConfigurationStorageService storageService,
        IAzureSearchIndexClientService indexClientService,
        SearchIndexClient indexClient,
        IEventLogService eventLogService)
        : base(formItemCollectionProvider, formDataBinder)
    {
        this.indexClientService = indexClientService;
        StorageService = storageService;
        this.indexClient = indexClient;
        this.eventLogService = eventLogService;
    }


    /// <summary>
    /// Validates and processes the Azure Search configuration.
    /// </summary>
    /// <param name="configuration">The Azure Search configuration to validate and process.</param>
    protected async Task<ModificationResponse> ValidateAndProcess(AzureSearchConfigurationModel configuration)
    {
        configuration.IndexName = RemoveWhitespacesUsingStringBuilder(configuration.IndexName ?? string.Empty);

        var context = new ValidationContext(configuration, null, null);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        bool valid = Validator.TryValidateObject(configuration, context, validationResults, true);

        if (!valid)
        {
            return new ModificationResponse(ModificationResult.Failure,
                validationResults
                    .Where(result => result.ErrorMessage is not null)
                    .Select(result => result.ErrorMessage!)
                    .ToList()
            );
        }

        if (StorageService.GetIndexIds().Exists(x => x == configuration.Id))
        {
            return await EditIndex(configuration);
        }

        return await CreateIndex(configuration);
    }


    protected static string RemoveWhitespacesUsingStringBuilder(string source)
    {
        var builder = new StringBuilder(source.Length);

        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            if (!char.IsWhiteSpace(c))
            {
                builder.Append(c);
            }
        }

        return source.Length == builder.Length ? source : builder.ToString();
    }


    private async Task<ModificationResponse> EditIndex(AzureSearchConfigurationModel configuration)
    {
        var oldIndexConfiguration = StorageService.GetIndexDataOrNull(configuration.Id);
        if (oldIndexConfiguration == null || oldIndexConfiguration.IndexName == null)
        {
            eventLogService.LogError(nameof(BaseIndexEditPage), nameof(EditIndex), "The index to edit could not be found.");
            return new ModificationResponse(ModificationResult.Failure, ["The index to edit could not be found."]);
        }

        var oldIndex = AzureSearchIndexStore.Instance.GetRequiredIndex(oldIndexConfiguration!.IndexName);

        SearchIndex editedSearchIndex;
        try
        {
            editedSearchIndex = await indexClientService.EditIndex(oldIndex!, configuration, default);
        }
        catch (OperationCanceledException ex)
        {
            eventLogService.LogException(nameof(BaseIndexEditPage), nameof(EditIndex), ex, $"Index edit operation was cancelled: {ex.Message}");
            return new ModificationResponse(ModificationResult.Failure);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentNullException or Azure.RequestFailedException)
        {
            eventLogService.LogException(nameof(BaseIndexEditPage), nameof(EditIndex), ex, $"Failed to edit Azure Search index: {ex.Message}");
            return new ModificationResponse(ModificationResult.Failure);
        }

        bool edited = StorageService.TryEditIndex(configuration);

        if (edited)
        {
            try
            {
                AzureSearchIndexStore.SetIndices(StorageService);
                return new ModificationResponse(ModificationResult.Success);
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentNullException)
            {
                eventLogService.LogError(nameof(BaseIndexEditPage), nameof(EditIndex), $"Failed to edit Azure Search index. Local storage and Azure Search storage will be rolled back.{ex.Message}");
            }

            // Rollback local storage if adding to the index store fails.
            if (!StorageService.TryDeleteIndex(configuration.Id))
            {
                eventLogService.LogError(nameof(BaseIndexEditPage), nameof(CreateIndex), $"Failed to rollback local index storage. Manual cleanup may be required. Please check the local storage and remove the index if necessary.");
            }
        }

        try
        {
            // Rollback index edit in Azure if local storage fails.
            await indexClient.DeleteIndexAsync(editedSearchIndex, onlyIfUnchanged: true);
        }
        catch (Exception ex) when (ex is ArgumentNullException or InvalidOperationException)
        {
            eventLogService.LogError(nameof(BaseIndexEditPage), nameof(CreateIndex), $"Failed to rollback index edit. Manual cleanup may be required. Please check the Azure AI Search in Azure portal and delete the index if necessary. {ex.Message}");
        }

        return new ModificationResponse(ModificationResult.Failure);
    }


    private async Task<ModificationResponse> CreateIndex(AzureSearchConfigurationModel configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration.IndexName))
        {
            return new ModificationResponse(ModificationResult.Failure, ["Index name cannot be empty."]);
        }

        SearchIndex searchIndex;
        try
        {
            searchIndex = await indexClientService.CreateIndex(configuration, default);
        }
        catch (OperationCanceledException ex)
        {
            eventLogService.LogError(nameof(BaseIndexEditPage), nameof(CreateIndex), $"Index creation operation was cancelled: {ex.Message}");
            return new ModificationResponse(ModificationResult.Failure);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentNullException or Azure.RequestFailedException)
        {
            eventLogService.LogError(nameof(BaseIndexEditPage), nameof(CreateIndex), $"Failed to create Azure Search index: {ex.Message}");
            return new ModificationResponse(ModificationResult.Failure);
        }

        bool created = StorageService.TryCreateIndex(configuration);

        if (created)
        {
            try
            {
                AzureSearchIndexStore.Instance.AddIndex(new AzureSearchIndex(configuration, StrategyStorage.Strategies));
                return new ModificationResponse(ModificationResult.Success);
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentNullException)
            {
                // We do not return yet, as we still need to rollback the index creation in Azure.
                eventLogService.LogError(nameof(BaseIndexEditPage), nameof(CreateIndex), $"Failed to create Azure Search index locally. Local storage and Azure Search storage will be rolled back. {ex.Message}");
            }

            // Rollback local storage if adding to the index store fails.
            if (!StorageService.TryDeleteIndex(configuration.Id))
            {
                eventLogService.LogError(nameof(BaseIndexEditPage), nameof(CreateIndex), $"Failed to rollback local index storage. Manual cleanup may be required. Please check the local storage and remove the index if necessary.");
            }
        }

        try
        {
            // Rollback index creation in Azure if local storage fails.
            await indexClient.DeleteIndexAsync(searchIndex, onlyIfUnchanged: true);
        }
        catch (Exception ex) when (ex is ArgumentNullException or Azure.RequestFailedException)
        {
            eventLogService.LogError(nameof(BaseIndexEditPage), nameof(CreateIndex), $"Failed to rollback index creation. Manual cleanup may be required. Please check the Azure AI Search in Azure portal and delete the index if necessary. {ex.Message}");
        }

        return new ModificationResponse(ModificationResult.Failure);
    }
}
