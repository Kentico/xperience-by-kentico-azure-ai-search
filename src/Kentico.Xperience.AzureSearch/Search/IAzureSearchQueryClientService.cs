using Azure.Search.Documents;

namespace Kentico.Xperience.AzureSearch.Search;

/// <summary>
/// Primary service used for querying azuresearch indexes
/// </summary>
public interface IAzureSearchQueryClientService
{
    SearchClient CreateSearchClientForQueries(string indexName);
}
