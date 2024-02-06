using CMS.DataEngine;

namespace Kentico.Xperience.AzureSearch.Admin
{
    /// <summary>
    /// Declares members for <see cref="AzureSearchIndexItemInfo"/> management.
    /// </summary>
    public partial interface IAzureSearchIndexItemInfoProvider : IInfoProvider<AzureSearchIndexItemInfo>, IInfoByIdProvider<AzureSearchIndexItemInfo>, IInfoByNameProvider<AzureSearchIndexItemInfo>
    {
    }
}
