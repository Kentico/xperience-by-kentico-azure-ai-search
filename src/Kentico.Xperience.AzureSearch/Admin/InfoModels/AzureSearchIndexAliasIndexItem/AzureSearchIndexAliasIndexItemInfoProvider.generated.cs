using CMS.DataEngine;

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// Class providing <see cref="AzureSearchIndexAliasIndexItemInfo"/> management.
/// </summary>
[ProviderInterface(typeof(IAzureSearchIndexAliasIndexItemInfoProvider))]
public partial class AzureSearchIndexAliasIndexItemInfoProvider : AbstractInfoProvider<AzureSearchIndexAliasIndexItemInfo, AzureSearchIndexAliasIndexItemInfoProvider>, IAzureSearchIndexAliasIndexItemInfoProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchIndexAliasIndexItemInfoProvider"/> class.
    /// </summary>
    public AzureSearchIndexAliasIndexItemInfoProvider()
        : base(AzureSearchIndexAliasIndexItemInfo.TYPEINFO)
    {
    }
}
