using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.AzureSearch.Admin;
using Kentico.Xperience.AzureSearch.Aliasing;

[assembly: UIPage(
   parentType: typeof(IndexAliasListingPage),
   slug: PageParameterConstants.PARAMETERIZED_SLUG,
   uiPageType: typeof(IndexAliasEditPage),
   name: "Edit index alias",
   templateName: TemplateNames.EDIT,
   order: UIPageOrder.NoOrder)]

namespace Kentico.Xperience.AzureSearch.Admin;


[UIEvaluatePermission(SystemPermissions.UPDATE)]
internal class IndexAliasEditPage : BaseIndexAliasEditPage
{
    private AzureSearchAliasConfigurationModel? model = null;

    [PageParameter(typeof(IntPageModelBinder))]
    public int IndexIdentifier { get; set; }

    public IndexAliasEditPage(Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider formItemCollectionProvider,
                 IFormDataBinder formDataBinder,
                 IAzureSearchConfigurationStorageService storageService,
                 IAzureSearchIndexAliasService azureSearchIndexAliasService)
        : base(formItemCollectionProvider, formDataBinder, azureSearchIndexAliasService, storageService) { }

    protected override AzureSearchAliasConfigurationModel Model
    {
        get
        {
            model ??= StorageService.GetAliasDataOrNull(IndexIdentifier) ?? new();

            return model;
        }
    }

    protected override async Task<ICommandResponse> ProcessFormData(AzureSearchAliasConfigurationModel model, ICollection<IFormItem> formItems)
    {
        var result = await ValidateAndProcess(model);

        var response = ResponseFrom(new FormSubmissionResult(
            result.ModificationResult == ModificationResult.Success
                ? FormSubmissionStatus.ValidationSuccess
                : FormSubmissionStatus.ValidationFailure));

        if (result.ModificationResult == ModificationResult.Failure)
        {
            if (result.ErrorMessages is not null)
            {
                result.ErrorMessages.ForEach(errorMessage => response.AddErrorMessage(errorMessage));
            }
            else
            {
                response.AddErrorMessage("Could not create index alias.");
            }
        }
        else
        {
            response.AddSuccessMessage("Index alias edited");
        }

        return response;
    }
}
