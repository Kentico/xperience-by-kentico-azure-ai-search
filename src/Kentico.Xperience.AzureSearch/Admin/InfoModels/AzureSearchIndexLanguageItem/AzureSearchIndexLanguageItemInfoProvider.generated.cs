using CMS.DataEngine;

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// Class providing <see cref="AzureSearchIndexLanguageItemInfo"/> management.
/// </summary>
[ProviderInterface(typeof(IAzureSearchIndexLanguageItemInfoProvider))]
public partial class AzureSearchIndexedLanguageInfoProvider : AbstractInfoProvider<AzureSearchIndexLanguageItemInfo, AzureSearchIndexedLanguageInfoProvider>, IAzureSearchIndexLanguageItemInfoProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchIndexedLanguageInfoProvider"/> class.
    /// </summary>
    public AzureSearchIndexedLanguageInfoProvider()
        : base(AzureSearchIndexLanguageItemInfo.TYPEINFO)
    {
    }
}
