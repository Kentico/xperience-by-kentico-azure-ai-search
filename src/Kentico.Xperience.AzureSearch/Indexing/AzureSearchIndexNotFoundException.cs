using Azure;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Thrown when an index registered in Xperience does not exist in the Azure AI Search service,
/// e.g. after the <see cref="AzureSearchOptions.IndexNamePrefix"/> was changed and the index was not rebuilt yet.
/// </summary>
internal sealed class AzureSearchIndexNotFoundException : RequestFailedException
{
    /// <summary>
    /// Event code used when logging the missing index to the event log.
    /// </summary>
    internal const string EVENT_CODE = "AZURE_SEARCH_INDEX_NOT_FOUND";


    public AzureSearchIndexNotFoundException(string indexName, string azureIndexName, Exception? innerException)
        : base(404,
            $"The Azure AI Search index '{azureIndexName}' (index '{indexName}') does not exist. Rebuild the index to create it. " +
            $"This happens e.g. after the 'CMSAzureSearch:{nameof(AzureSearchOptions.IndexNamePrefix)}' option was changed or the index was deleted directly in Azure AI Search.",
            innerException)
    {
    }
}
