using CMS.DataEngine;

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// Declares members for <see cref="AzureSearchContentTypeItemInfo"/> management.
/// </summary>
public partial interface IAzureSearchContentTypeItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}
