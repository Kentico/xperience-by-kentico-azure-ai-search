using Azure;
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


    public AzureSearchIndexClientService(SearchIndexClient indexClient,
        IServiceProvider serviceProvider)
    {
        this.indexClient = indexClient;
        this.serviceProvider = serviceProvider;
    }


    /// <inheritdoc />
    public async Task<SearchClient> InitializeIndexClient(string indexName, CancellationToken cancellationToken)
    {
        // Ensure the index exists locally.
        AzureSearchIndexStore.Instance.GetRequiredIndex(indexName);

        // Ensure the index exists in Azure. .
        await indexClient.GetIndexAsync(indexName, cancellationToken);
        return indexClient.GetSearchClient(indexName);
    }


    /// <inheritdoc />
    public async Task<SearchIndex> EditIndex(AzureSearchIndex oldIndex, AzureSearchConfigurationModel newIndexConfiguration, CancellationToken cancellationToken)
    {
        var oldStrategy = serviceProvider.GetRequiredStrategy(oldIndex);
        var oldSearchFields = oldStrategy.GetSearchFields();

        var newIndex = AzureSearchIndexStore.Instance.GetRequiredIndex(newIndexConfiguration.IndexName);
        var newStrategy = serviceProvider.GetRequiredStrategy(newIndex);
        var newSearchFields = newStrategy.GetSearchFields();

        if (!Enumerable.SequenceEqual(oldSearchFields, newSearchFields, new AzureSearchIndexComparer())
            || !string.Equals(oldIndex.IndexName, newIndex.IndexName))
        {
            await TryDeleteIndexIfExists(oldIndex.IndexName, true, cancellationToken);
            return await CreateIndexInternal(newSearchFields, newStrategy, newIndex.IndexName, cancellationToken);
        }

        var existingIndex = await GetIndexIfExists(newIndex.IndexName, cancellationToken);
        if (existingIndex is null)
        {
            // Index was deleted outside of the application - recreate it
            return await CreateIndexInternal(newSearchFields, newStrategy, newIndex.IndexName, cancellationToken);
        }

        return await CreateOrUpdateIndexInternal(existingIndex, newStrategy, cancellationToken);
    }


    /// <inheritdoc />
    public Task<SearchIndex> CreateIndex(AzureSearchConfigurationModel configurationModel, CancellationToken cancellationToken)
    {
        var index = new AzureSearchIndex(configurationModel, StrategyStorage.Strategies);
        return CreateIndex(index, cancellationToken);
    }


    /// <inheritdoc />
    public Task<SearchIndex> CreateIndex(AzureSearchIndex azureSearchIndex, CancellationToken cancellationToken)
    {
        var strategy = serviceProvider.GetRequiredStrategy(azureSearchIndex);
        var searchFields = strategy.GetSearchFields();

        return CreateIndexInternal(searchFields, strategy, azureSearchIndex.IndexName, cancellationToken);
    }


    /// <inheritdoc />
    public async Task<bool> TryDeleteIndexIfExists(string indexName, bool onlyIfUnchanged, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(indexName))
        {
            throw new ArgumentException("Value must not be null or empty", nameof(indexName));
        }

        if (await GetIndexIfExists(indexName, cancellationToken) is SearchIndex index)
        {
            await indexClient.DeleteIndexAsync(index, onlyIfUnchanged: onlyIfUnchanged, cancellationToken: cancellationToken);
            return true;
        }

        return false;
    }


    private async Task<SearchIndex> CreateIndexInternal(IList<SearchField>? searchFields, IAzureSearchIndexingStrategy strategy, string indexName, CancellationToken cancellationToken)
    {
        var definition = new SearchIndex(indexName, searchFields);

        await CreateOrUpdateIndexInternal(definition, strategy, cancellationToken);
        return definition;
    }


    private async Task<SearchIndex> CreateOrUpdateIndexInternal(SearchIndex index, IAzureSearchIndexingStrategy strategy, CancellationToken cancellationToken)
    {
        index = AddSemanticSearchConfigurationIfAny(index, strategy);

        index = AddVectorEmbeddingConfigurationIfAny(index, strategy);

        AzureSearchIndexingEvents.BeforeCreatingOrUpdatingIndex.Execute?.Invoke(this, new OnBeforeCreatingOrUpdatingIndexEventArgs(index));

        return (await indexClient.CreateOrUpdateIndexAsync(index, onlyIfUnchanged: true, cancellationToken: cancellationToken)).Value;
    }


    private async Task<SearchIndex?> GetIndexIfExists(string indexName, CancellationToken cancellationToken)
    {
        try
        {
            var indexResponse = await indexClient.GetIndexAsync(indexName, cancellationToken);
            return indexResponse.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }


    private static SearchIndex AddSemanticSearchConfigurationIfAny(SearchIndex definition, IAzureSearchIndexingStrategy strategy)
    {
        var semanticSearchConfiguration = strategy.CreateSemanticRankingConfigurationOrNull();

        if (semanticSearchConfiguration is not null)
        {
            foreach (var suggester in semanticSearchConfiguration.Suggesters)
            {
                definition.Suggesters.Add(suggester);
            }

            definition.SemanticSearch = semanticSearchConfiguration.SemanticSearch;
        }

        return definition;
    }

    private static SearchIndex AddVectorEmbeddingConfigurationIfAny(SearchIndex definition, IAzureSearchIndexingStrategy strategy)
    {
        var vectorEmbeddingSearchConfiguration = strategy.CreateVectorEmbeddinghConfigurationOrNull();

        if (vectorEmbeddingSearchConfiguration is not null)
        {
            definition.VectorSearch = vectorEmbeddingSearchConfiguration;
        }

        return definition;
    }
}
