using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Xperience.AzureSearch.Indexing;

public class AzureSearchIndexClientService : IAzureSearchIndexClientService
{
    private readonly SearchIndexClient indexClient;
    private readonly IServiceProvider serviceProvider;

    public AzureSearchIndexClientService(SearchIndexClient indexClient, IServiceProvider serviceProvider)
    {
        this.indexClient = indexClient;
        this.serviceProvider = serviceProvider;
    }

    public async Task<SearchClient> InitializeIndexClient(string indexName, CancellationToken cancellationToken)
    {
        var azureSearchIndex = AzureSearchIndexStore.Instance.GetIndex(indexName) ??
            throw new InvalidOperationException($"Registered index with name '{indexName}' doesn't exist.");

        var algoliaStrategy = serviceProvider.GetRequiredStrategy(azureSearchIndex);
        var searchFields = algoliaStrategy.GetSearchFields();

        var definition = new SearchIndex(indexName, searchFields);

        await indexClient.CreateOrUpdateIndexAsync(definition, cancellationToken: cancellationToken);

        return indexClient.GetSearchClient(indexName);
    }
}
