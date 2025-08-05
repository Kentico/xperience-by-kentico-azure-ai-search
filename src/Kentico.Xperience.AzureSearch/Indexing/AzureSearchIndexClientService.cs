using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

using Kentico.Xperience.AzureSearch.Admin;

using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Xperience.AzureSearch.Indexing;

internal class AzureSearchIndexClientService : IAzureSearchIndexClientService
{
    private readonly SearchIndexClient indexClient;
    private readonly IServiceProvider serviceProvider;

    public AzureSearchIndexClientService(SearchIndexClient indexClient, IServiceProvider serviceProvider)
    {
        this.indexClient = indexClient;
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task<SearchClient> InitializeIndexClient(string indexName, CancellationToken cancellationToken)
    {
        var azureSearchIndex = AzureSearchIndexStore.Instance.GetIndex(indexName) ??
            throw new InvalidOperationException($"Registered index with name '{indexName}' doesn't exist.");

        var azureSearchStrategy = serviceProvider.GetRequiredStrategy(azureSearchIndex);
        var searchFields = azureSearchStrategy.GetSearchFields();

        await CreateOrUpdateIndexInternal(searchFields, azureSearchStrategy, indexName, cancellationToken);

        return indexClient.GetSearchClient(indexName);
    }

    /// <inheritdoc />
    public async Task EditIndex(AzureSearchIndex oldIndex, AzureSearchConfigurationModel newIndexConfiguration, CancellationToken cancellationToken)
    {
        var oldStrategy = serviceProvider.GetRequiredStrategy(oldIndex);
        var oldSearchFields = oldStrategy.GetSearchFields();

        var newIndex = AzureSearchIndexStore.Instance.GetRequiredIndex(newIndexConfiguration.IndexName);
        var newStrategy = serviceProvider.GetRequiredStrategy(newIndex);
        var newSearchFields = newStrategy.GetSearchFields();

        if (!Enumerable.SequenceEqual(oldSearchFields, newSearchFields, new AzureSearchIndexComparer())
            || !string.Equals(oldIndex.IndexName, newIndex.IndexName))
        {
            await DeleteIndex(oldIndex.IndexName, cancellationToken);
        }

        await CreateOrUpdateIndexInternal(newSearchFields, newStrategy, newIndex.IndexName, cancellationToken);
    }

    private async Task DeleteIndex(string indexName, CancellationToken cancellationToken)
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

        AzureSearchIndexingEvents.BeforeCreatingOrUpdatingIndex.Execute?.Invoke(this, new OnBeforeCreatingOrUpdatingIndexEventArgs(definition));

        try
        {
            await indexClient.CreateOrUpdateIndexAsync(definition, cancellationToken: cancellationToken);
        }
        catch
        {
            await indexClient.DeleteIndexAsync(indexName, cancellationToken);
            await indexClient.CreateOrUpdateIndexAsync(definition, cancellationToken: cancellationToken);
        }
    }
}
