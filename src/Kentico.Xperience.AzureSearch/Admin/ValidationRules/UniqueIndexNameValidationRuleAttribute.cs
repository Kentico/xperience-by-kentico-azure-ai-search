using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// Validation rule attribute for ensuring names don't conflict with existing index names.
/// </summary>
internal class UniqueIndexNameValidationRuleAttribute : ValidationRuleAttribute
{
}
