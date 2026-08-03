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

## Upgrades and Uninstalling

See [Uninstall](Uninstall.md)