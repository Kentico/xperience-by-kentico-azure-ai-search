using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Kentico.Xperience.AzureSearch.Admin;
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

        var azureSearchStrategy = serviceProvider.GetRequiredStrategy(azureSearchIndex);
        var searchFields = azureSearchStrategy.GetSearchFields();

        await CreateOrUpdateIndexInternal(searchFields, azureSearchStrategy, indexName, cancellationToken);

        return indexClient.GetSearchClient(indexName);
    }

    public async Task EditIndex(string oldIndexName, AzureSearchConfigurationModel newIndexConfiguration, CancellationToken cancellationToken)
    {
        var oldIndex = AzureSearchIndexStore.Instance.GetIndex(oldIndexName) ??
            throw new InvalidOperationException($"Registered index with name '{oldIndexName}' doesn't exist.");
        var oldStrategy = serviceProvider.GetRequiredStrategy(oldIndex);
        var oldSearchFields = oldStrategy.GetSearchFields();

        var newIndex = AzureSearchIndexStore.Instance.GetIndex(newIndexConfiguration.IndexName) ??
            throw new InvalidOperationException($"Registered index with name '{oldIndexName}' doesn't exist.");
        var newStrategy = serviceProvider.GetRequiredStrategy(newIndex);
        var newSearchFields = newStrategy.GetSearchFields();

        if (Enumerable.SequenceEqual(oldSearchFields, newSearchFields, new SearchIndexComparer()))
        {
            await DeleteIndex(oldIndexName, cancellationToken);
        }
        await CreateOrUpdateIndexInternal(newSearchFields, newStrategy, newIndex.IndexName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteIndex(string indexName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        await indexClient.DeleteIndexAsync(indexName, cancellationToken);
    }

    private async Task CreateOrUpdateIndexInternal(IList<SearchField>? searchFields, IAzureSearchIndexingStrategy strategy, string indexName, CancellationToken cancellationToken)
    {
        var semanticSearchConfiguration = strategy.CreateSemanticRankingConfigurationOrNull();

        var definition = new SearchIndex(indexName, searchFields);

        if (semanticSearchConfiguration is not null)
        {
            foreach (var suggester in semanticSearchConfiguration.Suggesters)
            {
                definition.Suggesters.Add(suggester);
            }

            definition.SemanticSearch = semanticSearchConfiguration.SemanticSearch;
        }

        try
        {
            await indexClient.CreateOrUpdateIndexAsync(definition, cancellationToken: cancellationToken);
        }
        catch
        {
            await indexClient.DeleteIndexAsync(indexName);
            await indexClient.CreateOrUpdateIndexAsync(definition, cancellationToken: cancellationToken);
        }
    }
}

internal class SearchIndexComparer : IEqualityComparer<SearchField>
{
    public bool Equals(SearchField? x, SearchField? y)
    {
        if ((x is null && y is not null) || (x is not null && y is null))
        {
            return false;
        }
        if (x is null && y is null)
        {
            return true;
        }

        return x!.IsKey == y!.IsKey &&
            x!.Name == y!.Name &&
            x!.IsSearchable == y!.IsSearchable &&
            x!.IsSortable == y!.IsSortable &&
            x!.Type == y!.Type &&
            x!.AnalyzerName == y!.AnalyzerName &&
            x!.Fields == y!.Fields &&
            x!.IndexAnalyzerName == y!.IndexAnalyzerName &&
            x!.IsFacetable == y!.IsFacetable &&
            x!.IsFilterable == y!.IsFilterable &&
            x!.IsHidden == y!.IsHidden &&
            x!.SearchAnalyzerName == y!.SearchAnalyzerName &&
            x!.SynonymMapNames == y!.SynonymMapNames &&
            x!.VectorSearchDimensions == y!.VectorSearchDimensions &&
            x!.VectorSearchProfileName == y!.VectorSearchProfileName;
    }

    public int GetHashCode(SearchField obj)
    {
        if (obj == null)
        {
            return 0;
        }

        return obj.Name.GetHashCode();
    }
}
