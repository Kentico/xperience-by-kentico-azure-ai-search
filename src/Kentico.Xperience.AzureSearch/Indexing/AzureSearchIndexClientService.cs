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

        // Ensure the index exists in Azure.
        await indexClient.GetIndexAsync(indexName, cancellationToken);
        return indexClient.GetSearchClient(indexName);
    }


    /// <inheritdoc />
    public async Task<SearchIndex> EditIndex(AzureSearchIndex oldIndex, AzureSearchConfigurationModel newIndexConfiguration, CancellationToken cancellationToken)
    {
        var searchIndexResponse = await indexClient.GetIndexAsync(oldIndex.IndexName, cancellationToken);
        var searchIndex = searchIndexResponse.Value;

        var oldStrategy = serviceProvider.GetRequiredStrategy(oldIndex);
        var oldSearchFields = oldStrategy.GetSearchFields();

        var newIndex = AzureSearchIndexStore.Instance.GetRequiredIndex(newIndexConfiguration.IndexName);
        var newStrategy = serviceProvider.GetRequiredStrategy(newIndex);
        var newSearchFields = newStrategy.GetSearchFields();

        if (!Enumerable.SequenceEqual(oldSearchFields, newSearchFields, new AzureSearchIndexComparer())
            || !string.Equals(oldIndex.IndexName, newIndex.IndexName))
        {
            await indexClient.DeleteIndexAsync(searchIndex, onlyIfUnchanged: true, cancellationToken: cancellationToken);
            return await CreateIndexInternal(newSearchFields, newStrategy, newIndex.IndexName, cancellationToken);
        }

        return await EditIndexInternal(newStrategy, newIndex.IndexName, cancellationToken);
    }


    /// <inheritdoc />
    public async Task<SearchIndex> CreateIndex(AzureSearchConfigurationModel configurationModel, CancellationToken cancellationToken)
    {
        var index = new AzureSearchIndex(configurationModel, StrategyStorage.Strategies);
        return await CreateIndex(index, cancellationToken);
    }


    /// <inheritdoc />
    public async Task<SearchIndex> CreateIndex(AzureSearchIndex azuresearchindex, CancellationToken cancellationToken)
    {
        var strategy = serviceProvider.GetRequiredStrategy(azuresearchindex);
        var searchFields = strategy.GetSearchFields();

        return await CreateIndexInternal(searchFields, strategy, azuresearchindex.IndexName, cancellationToken);
    }


    private async Task<SearchIndex> CreateIndexInternal(IList<SearchField>? searchFields, IAzureSearchIndexingStrategy strategy, string indexName, CancellationToken cancellationToken)
    {
        var definition = new SearchIndex(indexName, searchFields);

        await CreateOrUpdateIndexInternal(definition, strategy, cancellationToken);
        return definition;
    }


    private async Task<SearchIndex> EditIndexInternal(IAzureSearchIndexingStrategy strategy, string indexName, CancellationToken cancellationToken)
    {
        var indexResponse = await indexClient.GetIndexAsync(indexName, cancellationToken);
        var index = indexResponse.Value;

        return await CreateOrUpdateIndexInternal(index, strategy, cancellationToken);
    }


    private async Task<SearchIndex> CreateOrUpdateIndexInternal(SearchIndex index, IAzureSearchIndexingStrategy strategy, CancellationToken cancellationToken)
    {
        index = AddSemanticSearchConfigurationIfAny(index, strategy);

        AzureSearchIndexingEvents.BeforeCreatingOrUpdatingIndex.Execute?.Invoke(this, new OnBeforeCreatingOrUpdatingIndexEventArgs(index));

        return (await indexClient.CreateOrUpdateIndexAsync(index, onlyIfUnchanged: true, cancellationToken: cancellationToken)).Value;
    }


    private SearchIndex AddSemanticSearchConfigurationIfAny(SearchIndex definition, IAzureSearchIndexingStrategy strategy)
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
}
