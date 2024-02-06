using CMS.DataEngine;

namespace Kentico.Xperience.AzureSearch.Admin
{
    /// <summary>
    /// Class providing <see cref="AzureSearchIndexItemInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IAzureSearchIndexItemInfoProvider))]
    public partial class AzureSearchIndexItemInfoProvider : AbstractInfoProvider<AzureSearchIndexItemInfo, AzureSearchIndexItemInfoProvider>, IAzureSearchIndexItemInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureSearchIndexItemInfoProvider"/> class.
        /// </summary>
        public AzureSearchIndexItemInfoProvider()
            : base(AzureSearchIndexItemInfo.TYPEINFO)
        {
        }
    }
}
