using CMS.DataEngine;

namespace Kentico.Xperience.AzureSearch.Admin;

public partial interface IAzureSearchIndexAliasItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}
