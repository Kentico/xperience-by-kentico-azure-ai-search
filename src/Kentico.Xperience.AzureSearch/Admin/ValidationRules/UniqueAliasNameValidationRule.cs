using CMS.DataEngine;

using Kentico.Xperience.Admin.Base.Forms;

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// Validation rule that ensures the name does not conflict with any existing alias name.
/// </summary>
[ValidationRuleAttribute(typeof(UniqueAliasNameValidationRuleAttribute))]
internal class UniqueAliasNameValidationRule : ValidationRule<string>
{
    private readonly IInfoProvider<AzureSearchIndexAliasItemInfo> indexAliasProvider;

    public const string IDENTIFIER = "KenticoAzureSearch.UniqueAliasNameValidationRule";


    /// <summary>
    /// Initializes a new instance of the <see cref="UniqueAliasNameValidationRule"/> class.
    /// </summary>
    public UniqueAliasNameValidationRule(IInfoProvider<AzureSearchIndexAliasItemInfo> indexAliasProvider)
        => this.indexAliasProvider = indexAliasProvider ?? throw new ArgumentNullException(nameof(indexAliasProvider));


    /// <summary>
    /// Validates that the name does not match any existing alias name.
    /// </summary>
    /// <param name="value">The name to validate.</param>
    /// <param name="formFieldValueProvider">Provider of values of other form fields for contextual validation.</param>
    /// <returns>Returns the validation result.</returns>
    public override async Task<ValidationResult> Validate(string value, IFormFieldValueProvider formFieldValueProvider)
    {
        var aliasId = 0;

        if (FormContext is ModelEditFormContext<AzureSearchAliasConfigurationModel> context)
        {
            aliasId = context.EditedModel.Id;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            // If the value is empty, let the Required validation handle it
            return ValidationResult.Success;
        }

        var existingAliasesQuery = indexAliasProvider.Get()
           .Columns(nameof(AzureSearchIndexAliasItemInfo.AzureSearchIndexAliasItemId))
           .WhereEquals(nameof(AzureSearchIndexAliasItemInfo.AzureSearchIndexAliasItemIndexAliasName), value);

        if (aliasId != 0)
        {
            existingAliasesQuery = existingAliasesQuery.WhereNotEquals(nameof(AzureSearchIndexAliasItemInfo.AzureSearchIndexAliasItemId), aliasId);
        }

        var aliasExists = await existingAliasesQuery
            .TopN(1)
            .GetScalarResultAsync<int>() > 0;

        if (aliasExists)
        {
            return new ValidationResult(false, $"An alias with the name '{value}' already exists. Name must be unique and cannot match existing alias names.");
        }

        return ValidationResult.Success;
    }
}
