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
        var oldIndex = AzureSearchIndexStore.Instance.GetRequiredIndex(oldIndexConfiguration!.IndexName);

        SearchIndex editedSearchIndex;
        try
        {
            editedSearchIndex = await indexClientService.EditIndex(oldIndex!, configuration, default);
        }
        catch (Exception ex)
        {
            eventLogService.LogError(nameof(BaseIndexEditPage), nameof(ValidateAndProcess), $"Failed to edit Azure Search index: {ex.Message}");
            return new ModificationResponse(ModificationResult.Failure);
        }

        bool edited = StorageService.TryEditIndex(configuration);

        if (edited)
        {
            AzureSearchIndexStore.SetIndicies(StorageService);
            return new ModificationResponse(ModificationResult.Success);
        }
        try
        {
            // Rollback index edit in Azure if local storage fails.
            await indexClient.DeleteIndexAsync(editedSearchIndex, onlyIfUnchanged: true);
        }
        catch (Exception ex)
        {
            eventLogService.LogError(nameof(BaseIndexEditPage), nameof(ValidateAndProcess), $"Failed to rollback index edit. {ex.Message}");
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
        catch (Exception ex)
        {
            eventLogService.LogError(nameof(BaseIndexEditPage), nameof(ValidateAndProcess), ex.Message);
            return new ModificationResponse(ModificationResult.Failure);
        }

        bool created = StorageService.TryCreateIndex(configuration);

        if (created)
        {
            AzureSearchIndexStore.Instance.AddIndex(new AzureSearchIndex(configuration, StrategyStorage.Strategies));

            return new ModificationResponse(ModificationResult.Success);
        }

        try
        {
            // Rollback index creation in Azure if local storage fails.
            await indexClient.DeleteIndexAsync(searchIndex, onlyIfUnchanged: true);
        }
        catch (Exception ex)
        {
            eventLogService.LogError(nameof(BaseIndexEditPage), nameof(ValidateAndProcess), $"Failed to rollback index creation. {ex.Message}");
        }

        return new ModificationResponse(ModificationResult.Failure);
    }
}
