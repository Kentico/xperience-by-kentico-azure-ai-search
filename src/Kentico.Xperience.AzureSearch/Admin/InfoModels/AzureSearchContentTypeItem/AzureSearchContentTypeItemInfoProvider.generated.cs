using CMS.DataEngine;

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// Class providing <see cref="AzureSearchContentTypeItemInfo"/> management.
/// </summary>
[ProviderInterface(typeof(IAzureSearchContentTypeItemInfoProvider))]
public partial class AzureSearchContentTypeItemInfoProvider : AbstractInfoProvider<AzureSearchContentTypeItemInfo, AzureSearchContentTypeItemInfoProvider>, IAzureSearchContentTypeItemInfoProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchContentTypeItemInfoProvider"/> class.
    /// </summary>
    public AzureSearchContentTypeItemInfoProvider()
        : base(AzureSearchContentTypeItemInfo.TYPEINFO)
    {
    }
}
