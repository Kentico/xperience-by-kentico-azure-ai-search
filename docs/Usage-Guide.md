# Usage Guide

This library supports using Azure.Search to index both unstructured and high structured, interrelated content in an Xperience by Kentico solution. This indexed content can then be programmatically queried and displayed in a website channel.

Below are the steps to integrate the library into your solution.

## Create a custom Indexing Strategy

See [Custom index strategy](Custom-index-strategy.md)

## Continuous Integration

When starting your application for the first time after adding this library to your solution, a custom module and custom module classes will automatically be created
to support managing search index configuration within the administration UI.

If you do not see new items added to your [CI repository](https://docs.xperience.io/x/FAKQC) for the new auto-generated Azure search data types, stop your application and perform a [CI store](https://docs.xperience.io/xp/developers-and-admins/ci-cd/continuous-integration#ContinuousIntegration-Storeobjectdatatotherepository) to add the library's custom module configuration to the CI repository.

You should now be able to run a [CI restore](https://docs.xperience.io/xp/developers-and-admins/ci-cd/continuous-integration#ContinuousIntegration-Restorerepositoryfilestothedatabase).
Attempting to run a CI restore without the CI files in the CI repository will result in a SQL error during the restore.

When team members are merging changes that include the addition of this library, they _must_ first run a CI restore to ensure they have the same object metadata for the search custom module as your database.

Future updates to indexes will be tracked in the CI repository [unless they are excluded](https://docs.xperience.io/x/ygAcCQ).

## Managing search indexes

See [Managing search indexes](Managing-Indexes.md)

## Managing search index aliases

See [Managing search index aliases](Managing-Aliases.md)

## Search index querying

See [Search index querying](Search-index-querying.md)

## Adding scoring profiles

See [Adding scoring profiles](Adding-Scoring-Profiles.md)

## Configuring the Azure Search client (resolving SNAT port exhaustion)

The integration creates Azure Search clients using the default [`SearchClientOptions`](https://learn.microsoft.com/en-us/dotnet/api/azure.search.documents.searchclientoptions). Under heavy indexing or querying load, applications can run into [SNAT (Source Network Address Translation) port exhaustion](https://learn.microsoft.com/en-us/azure/app-service/troubleshoot-intermittent-outbound-connection-errors), which manifests as intermittent connection timeouts or `SocketException` errors. This is commonly caused by aggressive retry policies opening too many outbound connections.

You can customize the `SearchClientOptions` used by the integration through the `RegisterSearchClientConfiguration` method on the builder. Use it to tune the retry and timeout policies to be more resilient against transient failures such as SNAT port exhaustion.

```csharp
// Program.cs
using Azure;
using Azure.Core;

services.AddKenticoAzureSearch(builder =>
{
    builder.RegisterStrategy<GlobalAzureSearchStrategy, GlobalSearchModel>("DefaultStrategy");

    // Customize the SearchClientOptions to reduce the risk of SNAT port exhaustion
    builder.RegisterSearchClientConfiguration(options =>
    {
        options.Retry.Mode = RetryMode.Exponential;
        options.Retry.MaxRetries = 3;
        options.Retry.Delay = TimeSpan.FromSeconds(1);
        options.Retry.MaxDelay = TimeSpan.FromSeconds(10);
        options.Retry.NetworkTimeout = TimeSpan.FromSeconds(30);
    });
}, configuration);
```

> The configuration delegate can only be registered once. Attempting to register it more than once throws an `InvalidOperationException`.

## Disable indexing

You can disable indexing. This might be useful if there are any problems with differing Kentico version between this integration
and your application. You can do so in the `appsettings.json`. This option defaults to true and therefore does not need to be specified when you want to enable indexing.

  ```json
   "CMSAzureSearch": {
       "SearchServiceEnabled" : false,         // Add this line to disable indexing
       "SearchServiceEndPoint": "<your application url>",
       "SearchServiceAdminApiKey": "<your application admin key>",
       "SearchServiceQueryApiKey": "<your application query key>"
   }
   ```

This disables reindexing through the rebuild hook and after web page and content item events. The administration UI module is still accessible, but does not show any data.
Disabling indexing does not delete AzureSearch data from database. Already indexed data can still be accessed from your application.

## Share an Azure AI Search service between environments

Index and alias names defined in the administration UI are stored in the database. When the same Azure AI Search service is used by multiple environments
(e.g. DEV, UAT, PROD) or a database is copied between environments, all environments would otherwise write to the same indexes.

Set the `IndexNamePrefix` option per environment (e.g. in `appsettings.{Environment}.json`, environment variables or a secret store) to isolate them:

  ```json
   "CMSAzureSearch": {
       "SearchServiceEndPoint": "<your application url>",
       "SearchServiceAdminApiKey": "<your application admin key>",
       "SearchServiceQueryApiKey": "<your application query key>",
       "IndexNamePrefix": "dev-"
   }
   ```

With the configuration above, an index named `products` in the administration UI is created in Azure AI Search as `dev-products`.

- The prefix is applied only to names sent to Azure AI Search (indexes and aliases). The database, administration UI and indexing strategies keep working with the unprefixed (logical) names.
- `IAzureSearchQueryClientService.CreateSearchClientForQueries` applies the prefix automatically, so querying code uses the logical index or alias name.
- The administration UI index statistics only list indexes that match the configured prefix.
- The prefix may contain only lowercase letters, digits and dashes and must start with a letter or digit. An invalid prefix throws an `ArgumentException` on startup.
- The prefixed name must not exceed 128 characters (Azure AI Search limit). Longer names throw an `InvalidOperationException`.
- `OnBeforeCreatingOrUpdatingIndex` receives the `SearchIndex` with the prefixed name.
- `IAzureSearchIndexNameResolver` can be injected to convert between logical and Azure names in custom code.

> Changing the prefix of an existing environment does not rename or move existing indexes. Rebuild all indexes and re-save all aliases after the change
> and manually delete the indexes and aliases with the old names from the Azure AI Search service.

## Upgrades and Uninstalling

See [Uninstall](Uninstall.md)