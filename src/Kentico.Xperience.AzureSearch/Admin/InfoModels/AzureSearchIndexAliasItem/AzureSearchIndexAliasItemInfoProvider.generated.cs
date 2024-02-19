using CMS.DataEngine;

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// Class providing <see cref="AzureSearchIndexAliasItemInfo"/> management.
/// </summary>
[ProviderInterface(typeof(IAzureSearchIndexAliasItemInfoProvider))]
public partial class AzureSearchIndexAliasItemInfoProvider : AbstractInfoProvider<AzureSearchIndexAliasItemInfo, AzureSearchIndexAliasItemInfoProvider>, IAzureSearchIndexAliasItemInfoProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchIndexAliasItemInfoProvider"/> class.
    /// </summary>
    public AzureSearchIndexAliasItemInfoProvider()
        : base(AzureSearchIndexAliasItemInfo.TYPEINFO)
    {
    }
}
