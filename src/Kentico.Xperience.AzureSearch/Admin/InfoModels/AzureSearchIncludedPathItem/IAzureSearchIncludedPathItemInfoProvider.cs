using CMS.DataEngine;

namespace Kentico.Xperience.AzureSearch.Admin;

public partial interface IAzureSearchIncludedPathItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}
