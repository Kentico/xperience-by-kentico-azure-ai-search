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

    public async Task CreateAlias(string aliasName, IEnumerable<string> indexNames, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(aliasName))
        {
            throw new ArgumentNullException(nameof(aliasName));
        }

        await indexClient.CreateAliasAsync(new SearchAlias(aliasName, indexNames), cancellationToken);
    }

    public async Task EditAlias(string oldAliasName, SearchAlias newAlias, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(oldAliasName))
        {
            throw new ArgumentNullException(nameof(oldAliasName));
        }

        await DeleteAlias(oldAliasName, cancellationToken);
        await indexClient.CreateOrUpdateAliasAsync(newAlias.Name, newAlias, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAlias(string aliasName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(aliasName))
        {
            throw new ArgumentNullException(nameof(aliasName));
        }

        await indexClient.DeleteAliasAsync(aliasName, cancellationToken);
    }
}
