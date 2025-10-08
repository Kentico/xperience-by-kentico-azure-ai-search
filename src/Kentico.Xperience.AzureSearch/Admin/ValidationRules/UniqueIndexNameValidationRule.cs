using CMS.DataEngine;

using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.AzureSearch.Admin;

[assembly: RegisterFormValidationRule(UniqueIndexNameValidationRule.IDENTIFIER, typeof(UniqueIndexNameValidationRule), "Unique Index Name", "Checks whether the name does not conflict with an existing index name.")]

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// Validation rule that ensures the name does not conflict with any existing index name.
/// </summary>
[ValidationRuleAttribute(typeof(UniqueIndexNameValidationRuleAttribute))]
internal class UniqueIndexNameValidationRule : ValidationRule<string>
{
    private readonly IInfoProvider<AzureSearchIndexItemInfo> indexProvider;

    public const string IDENTIFIER = "KenticoAzureSearch.UniqueIndexNameValidationRule";


    /// <summary>
    /// Initializes a new instance of the <see cref="UniqueIndexNameValidationRule"/> class.
    /// </summary>
    public UniqueIndexNameValidationRule(IInfoProvider<AzureSearchIndexItemInfo> indexProvider)
        => this.indexProvider = indexProvider ?? throw new ArgumentNullException(nameof(indexProvider));


    /// <summary>
    /// Validates that the name does not match any existing index name.
    /// </summary>
    /// <param name="value">The name to validate.</param>
    /// <param name="formFieldValueProvider">Provider of values of other form fields for contextual validation.</param>
    /// <returns>Returns the validation result.</returns>
    public override async Task<ValidationResult> Validate(string value, IFormFieldValueProvider formFieldValueProvider)
    {
        var indexId = 0;

        if (FormContext is ModelEditFormContext<AzureSearchConfigurationModel> indexContext)
        {
            indexId = indexContext.EditedModel.Id;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.Success;
        }

        var existingIndexQuery = indexProvider.Get()
            .Column(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemId))
            .WhereEquals(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemIndexName), value);

        if (indexId > 0)
        {
            existingIndexQuery.WhereNotEquals(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemId), indexId);
        }

        var indexExists = await existingIndexQuery
            .TopN(1)
            .GetScalarResultAsync<int>() > 0;

        if (indexExists)
        {
            return new ValidationResult(false, $"An index with the name '{value}' already exists. Name must be unique and cannot match existing index names.");
        }

        return ValidationResult.Success;
    }
}
