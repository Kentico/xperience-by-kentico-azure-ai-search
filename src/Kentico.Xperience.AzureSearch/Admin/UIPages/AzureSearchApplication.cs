using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using Kentico.Xperience.AzureSearch.Admin;

[assembly: UIApplication(
    identifier: AzureSearchApplicationPage.IDENTIFIER,
    type: typeof(AzureSearchApplicationPage),
    slug: "azuresearch",
    name: "Azure AI Search",
    category: BaseApplicationCategories.DEVELOPMENT,
    icon: Icons.Magnifier,
    templateName: TemplateNames.SECTION_LAYOUT)]

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// The root application page for the AzureSearch integration.
/// </summary>
[UIPermission(SystemPermissions.VIEW)]
[UIPermission(SystemPermissions.CREATE)]
[UIPermission(SystemPermissions.UPDATE)]
[UIPermission(SystemPermissions.DELETE)]
[UIPermission(AzureSearchIndexPermissions.REBUILD, "Rebuild")]
internal class AzureSearchApplicationPage : ApplicationPage
{
    public const string IDENTIFIER = "Kentico.Xperience.Integrations.AzureSearch";
}
