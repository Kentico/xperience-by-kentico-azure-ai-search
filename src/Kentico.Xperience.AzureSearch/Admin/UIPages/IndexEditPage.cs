using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.AzureSearch.Admin;
using Kentico.Xperience.AzureSearch.Indexing;

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

    public IndexEditPage(
        Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        IAzureSearchConfigurationStorageService storageService,
        IAzureSearchIndexClientService indexClientService)
        : base(formItemCollectionProvider, formDataBinder, storageService, indexClientService) { }

    protected override AzureSearchConfigurationModel Model
    {
        get
        {
            model ??= StorageService.GetIndexDataOrNull(IndexIdentifier) ?? new();

            return model;
        }
    }

    protected override async Task<ICommandResponse> ProcessFormData(AzureSearchConfigurationModel model, ICollection<IFormItem> formItems)
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
                response.AddErrorMessage("Could not create index.");
            }
        }
        else
        {
            response.AddSuccessMessage("Index edited");
        }

        return response;
    }
}
