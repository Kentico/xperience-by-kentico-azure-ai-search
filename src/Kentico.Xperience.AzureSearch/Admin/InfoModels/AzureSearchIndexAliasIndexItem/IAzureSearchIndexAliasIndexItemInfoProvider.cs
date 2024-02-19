using CMS.DataEngine;

namespace Kentico.Xperience.AzureSearch.Admin;

public partial interface IAzureSearchIndexAliasIndexItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}
