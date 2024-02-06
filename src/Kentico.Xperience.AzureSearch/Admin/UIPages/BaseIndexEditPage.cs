using System.ComponentModel.DataAnnotations;
using System.Text;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.AzureSearch.Indexing;
using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

namespace Kentico.Xperience.AzureSearch.Admin;

internal abstract class BaseIndexEditPage : ModelEditPage<AzureSearchConfigurationModel>
{
    protected readonly IAzureSearchConfigurationStorageService StorageService;

    protected BaseIndexEditPage(
        IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        IAzureSearchConfigurationStorageService storageService)
        : base(formItemCollectionProvider, formDataBinder) => StorageService = storageService;

    protected IndexModificationResponse ValidateAndProcess(AzureSearchConfigurationModel configuration)
    {
        configuration.IndexName = RemoveWhitespacesUsingStringBuilder(configuration.IndexName ?? "");

        var context = new ValidationContext(configuration, null, null);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        bool valid = Validator.TryValidateObject(configuration, context, validationResults, true);

        if (!valid)
        {
            return new IndexModificationResponse(IndexModificationResult.Failure,
                validationResults
                    .Where(result => result.ErrorMessage is not null)
                    .Select(result => result.ErrorMessage!)
                    .ToList()
            );
        }

        if (StorageService.GetIndexIds().Exists(x => x == configuration.Id))
        {
            bool edited = StorageService.TryEditIndex(configuration);

            if (edited)
            {
                AzureSearchIndexStore.SetIndicies(StorageService);

                return new IndexModificationResponse(IndexModificationResult.Success);
            }

            return new IndexModificationResponse(IndexModificationResult.Failure);
        }
        else
        {
            bool created = !string.IsNullOrWhiteSpace(configuration.IndexName) && StorageService.TryCreateIndex(configuration);

            if (created)
            {
                AzureSearchIndexStore.Instance.AddIndex(new AzureSearchIndex(configuration, StrategyStorage.Strategies));

                return new IndexModificationResponse(IndexModificationResult.Success);
            }

            return new IndexModificationResponse(IndexModificationResult.Failure);
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
}

internal enum IndexModificationResult
{
    Success,
    Failure
}

internal class IndexModificationResponse
{
    public IndexModificationResult IndexModificationResult { get; set; }
    public List<string>? ErrorMessages { get; set; }

    public IndexModificationResponse(IndexModificationResult result, List<string>? errorMessage = null)
    {
        IndexModificationResult = result;
        ErrorMessages = errorMessage;
    }
}
