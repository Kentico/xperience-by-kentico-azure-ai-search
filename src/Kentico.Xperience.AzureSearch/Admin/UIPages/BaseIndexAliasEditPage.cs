using System.ComponentModel.DataAnnotations;
using System.Text;

using Azure.Search.Documents.Indexes.Models;

using CMS.Core;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.AzureSearch.Aliasing;

using Microsoft.IdentityModel.Tokens;

using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

namespace Kentico.Xperience.AzureSearch.Admin;

internal abstract class BaseIndexAliasEditPage : ModelEditPage<AzureSearchAliasConfigurationModel>
{
    protected readonly IAzureSearchConfigurationStorageService StorageService;
    private readonly IAzureSearchIndexAliasService azureSearchIndexAliasService;

    protected BaseIndexAliasEditPage(
        IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        IAzureSearchIndexAliasService azureSearchIndexAliasService,
        IAzureSearchConfigurationStorageService storageService
        )
        : base(formItemCollectionProvider, formDataBinder)
    {
        StorageService = storageService;
        this.azureSearchIndexAliasService = azureSearchIndexAliasService;
    }

    protected async Task<ModificationResponse> ValidateAndProcess(AzureSearchAliasConfigurationModel configuration)
    {
        configuration.AliasName = RemoveWhitespacesUsingStringBuilder(configuration.AliasName ?? string.Empty);

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

        if (StorageService.GetAliasIds().Exists(x => x == configuration.Id))
        {
            // Editing existing alias - Azure first, then local storage
            return await ProcessEditAlias(configuration);
        }
        else
        {
            // Creating new alias - Azure first, then local storage
            return await ProcessCreateAlias(configuration);
        }
    }

    private async Task<ModificationResponse> ProcessEditAlias(AzureSearchAliasConfigurationModel configuration)
    {
        string? oldAliasName = StorageService.GetAliasDataOrNull(configuration.Id)?.AliasName;
        if (oldAliasName == null)
        {
            return new ModificationResponse(ModificationResult.Failure, ["Alias not found."]);
        }

        try
        {
            // Step 1: Update Azure first
            await azureSearchIndexAliasService.EditAlias(oldAliasName, new SearchAlias(configuration.AliasName, configuration.IndexNames), default);

            // Step 2: Update local storage only if Azure operation succeeded
            bool edited = StorageService.TryEditAlias(configuration);
            if (!edited)
            {
                // Rollback Azure changes if local storage failed
                var oldIndexNames = StorageService.GetAliasDataOrNull(configuration.Id)?.IndexNames ?? [];
                await RollbackEditAlias(oldAliasName, configuration.AliasName, oldIndexNames);
                return new ModificationResponse(ModificationResult.Failure, ["Failed to update local storage."]);
            }

            // Step 3: Update in-memory store only if both Azure and local storage succeeded
            AzureSearchIndexAliasStore.SetAliases(StorageService);

            return new ModificationResponse(ModificationResult.Success);
        }
        catch (Exception ex)
        {
            EventLogService.LogError(nameof(BaseIndexAliasEditPage), nameof(ProcessEditAlias), $"Exception during alias edit: {ex.Message}");
            return new ModificationResponse(ModificationResult.Failure, [$"Failed to update Azure Search alias. See error log for more details."]);
        }
    }

    private async Task<ModificationResponse> ProcessCreateAlias(AzureSearchAliasConfigurationModel configuration)
    {
        if (configuration.IndexNames.IsNullOrEmpty())
        {
            return new ModificationResponse(ModificationResult.Failure, ["Index names cannot be empty."]);
        }

        try
        {
            // Step 1: Create in Azure first
            await azureSearchIndexAliasService.CreateAlias(new SearchAlias(configuration.AliasName, configuration.IndexNames), default);

            // Step 2: Create in local storage only if Azure operation succeeded
            bool created = StorageService.TryCreateAlias(configuration);
            if (!created)
            {
                // Rollback Azure changes if local storage failed
                await RollbackCreateAlias(configuration.AliasName);
                return new ModificationResponse(ModificationResult.Failure, ["Failed to create alias in local storage."]);
            }

            // Step 3: Update in-memory store only if both Azure and local storage succeeded
            AzureSearchIndexAliasStore.Instance.AddAlias(new AzureSearchIndexAlias(configuration));

            return new ModificationResponse(ModificationResult.Success);
        }
        catch (Exception ex)
        {
            EventLogService.LogError(nameof(BaseIndexAliasEditPage), nameof(ProcessCreateAlias), $"Exception during alias creation: {ex.Message}");
            return new ModificationResponse(ModificationResult.Failure, [$"Failed to create Azure Search alias. See error log for more details."]);
        }
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

    private async Task RollbackCreateAlias(string aliasName)
    {
        try
        {
            await azureSearchIndexAliasService.DeleteAlias(aliasName, default);
        }
        catch (Exception ex)
        {
            EventLogService.LogError(nameof(BaseIndexAliasEditPage), nameof(RollbackCreateAlias),
                $"Failed to rollback Azure Search alias creation: {aliasName}.{Environment.NewLine}{ex.Message}");
        }
    }

    private async Task RollbackEditAlias(string oldAliasName, string newAliasName, IEnumerable<string> oldIndexNames)
    {
        try
        {
            await azureSearchIndexAliasService.EditAlias(newAliasName, new SearchAlias(oldAliasName, oldIndexNames), default);
        }
        catch (Exception ex)
        {
            EventLogService.LogError(nameof(BaseIndexAliasEditPage), nameof(RollbackEditAlias),
                $"Failed to rollback Azure Search alias edit: {newAliasName} back to {oldAliasName}.{Environment.NewLine}{ex.Message}");
        }
    }
}
