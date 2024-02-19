# Upgrades

## Uninstall

This integration programmatically inserts custom module classes and their configuration into the Xperience solution on startup (see `AzureSearchModuleInstaller.cs`).

To remove this configuration and the added database tables perform one of the following sets of changes to your solution:

### Using Continuous Integration (CI)

1. Remove the `Kentico.Xperience.AzureSearch` NuGet package from the solution
1. Remove any code references to the package and recompile your solution
1. If you are using Xperience's Continuous Integration (CI), delete the files with the paths from your CI repository folder:

   - `\App_Data\CIRepository\@global\cms.class\kenticoazuresearch.*\**`
   - `\App_Data\CIRepository\@global\cms.class\kentico.xperience.azuresearch\**`
   - `\App_Data\CIRepository\@global\kenticoazuresearch.*\**`

1. Run a CI restore, which will clean up the database tables and `CMS_Class` records.

### No Continuous Integration

If you are not using CI run the following SQL _after_ removing the NuGet package from the solution:

```sql
drop table KenticoAzureSearch_AzureSearchContentTypeItem
drop table KenticoAzureSearch_AzureSearchIncludedPathItem
drop table KenticoAzureSearch_AzureSearchIndexAliasIndexItem
drop table KenticoAzureSearch_AzureSearchIndexAliasItem
drop table KenticoAzureSearch_AzureSearchIndexItem
drop table KenticoAzureSearch_AzureSearchIndexLanguageItem

delete
FROM [dbo].[CMS_Class] where ClassName like 'kenticoazuresearch%'

delete
from [CMS_Resource] where ResourceName = 'CMS.Integration.AzureSearch'
```
