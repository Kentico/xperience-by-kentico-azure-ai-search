using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.AzureSearch.Admin;

[assembly: UIPage(
   parentType: typeof(IndexListingPage),
   slug: PageParameterConstants.PARAMETERIZED_SLUG,
   uiPageType: typeof(IndexEditPage),
   name: "Edit index",
   templateName: TemplateNames.EDIT,
   order: UIPageOrder.NoOrder)]

namespace Kentico.Xperience.AzureSearch.Admin;

[UIEvaluatePermission(SystemPermissions.UPDATE)]
internal class IndexEditPage : BaseIndexEditPage
{
    private AzureSearchConfigurationModel? model = null;

    [PageParameter(typeof(IntPageModelBinder))]
    public int IndexIdentifier { get; set; }

    public IndexEditPage(Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider formItemCollectionProvider,
                 IFormDataBinder formDataBinder,
                 IAzureSearchConfigurationStorageService storageService)
        : base(formItemCollectionProvider, formDataBinder, storageService) { }

    protected override AzureSearchConfigurationModel Model
    {
        get
        {
            model ??= StorageService.GetIndexDataOrNull(IndexIdentifier) ?? new();

            return model;
        }
    }

    protected override Task<ICommandResponse> ProcessFormData(AzureSearchConfigurationModel model, ICollection<IFormItem> formItems)
    {
        var result = ValidateAndProcess(model);

        var response = ResponseFrom(new FormSubmissionResult(
            result.IndexModificationResult == IndexModificationResult.Success
                ? FormSubmissionStatus.ValidationSuccess
                : FormSubmissionStatus.ValidationFailure));

        if (result.IndexModificationResult == IndexModificationResult.Failure)
        {
            if (result.ErrorMessages is not null)
            {
                result.ErrorMessages.ForEach(errorMessage => response.AddErrorMessage(errorMessage));
            }
            else
            {
                response.AddErrorMessage("Could not create index.");
            }
        }
        else
        {
            response.AddSuccessMessage("Index edited");
        }

        return Task.FromResult<ICommandResponse>(response);
    }
}
