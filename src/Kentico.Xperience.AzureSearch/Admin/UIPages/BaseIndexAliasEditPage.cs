using System.ComponentModel.DataAnnotations;
using System.Text;

using Azure.Search.Documents.Indexes.Models;

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
        IAzureSearchConfigurationStorageService storageService)
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
            string oldAliasName = StorageService.GetAliasDataOrNull(configuration.Id)!.AliasName;

            bool edited = StorageService.TryEditAlias(configuration);

            if (edited)
            {
                AzureSearchIndexAliasStore.SetAliases(StorageService);
                await azureSearchIndexAliasService.EditAlias(oldAliasName, new SearchAlias(configuration.AliasName, configuration.IndexNames), default);

                return new ModificationResponse(ModificationResult.Success);
            }

            return new ModificationResponse(ModificationResult.Failure);
        }

        bool created = !configuration.IndexNames.IsNullOrEmpty() && StorageService.TryCreateAlias(configuration);

        if (created)
        {
            AzureSearchIndexAliasStore.Instance.AddAlias(new AzureSearchIndexAlias(configuration));
            await azureSearchIndexAliasService.CreateAlias(new SearchAlias(configuration.AliasName, configuration.IndexNames), default);

            return new ModificationResponse(ModificationResult.Success);
        }

        return new ModificationResponse(ModificationResult.Failure);
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
}
