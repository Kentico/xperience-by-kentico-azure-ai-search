using CMS.DataEngine;

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// Class providing <see cref="AzureSearchIncludedPathItemInfo"/> management.
/// </summary>
[ProviderInterface(typeof(IAzureSearchIncludedPathItemInfoProvider))]
public partial class AzureSearchIncludedPathItemInfoProvider : AbstractInfoProvider<AzureSearchIncludedPathItemInfo, AzureSearchIncludedPathItemInfoProvider>, IAzureSearchIncludedPathItemInfoProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchIncludedPathItemInfoProvider"/> class.
    /// </summary>
    public AzureSearchIncludedPathItemInfoProvider()
        : base(AzureSearchIncludedPathItemInfo.TYPEINFO)
    {
    }
}
