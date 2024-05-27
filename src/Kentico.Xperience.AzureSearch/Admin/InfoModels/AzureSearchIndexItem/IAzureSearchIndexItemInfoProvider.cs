using CMS.DataEngine;

namespace Kentico.Xperience.AzureSearch.Admin;

public partial interface IAzureSearchIndexItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}
