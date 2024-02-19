using CMS.DataEngine;

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// Declares members for <see cref="AzureSearchIndexLanguageItemInfo"/> management.
/// </summary>
public partial interface IAzureSearchIndexLanguageItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}
