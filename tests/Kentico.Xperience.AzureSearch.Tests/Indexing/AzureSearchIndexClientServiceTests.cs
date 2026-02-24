using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

using CMS.Tests;

using Kentico.Xperience.AzureSearch.Indexing;

namespace Kentico.Xperience.AzureSearch.Tests.Indexing;

[TestFixture]
[Category.Unit]
internal class AzureSearchIndexClientServiceTests
{
    private const string TEST_FIELD_NAME = "field1";

    private SearchIndexClient mockIndexClient;
    private IServiceProvider mockServiceProvider;
    private AzureSearchIndexClientService service;


    [SetUp]
    public void SetUp()
    {
        mockIndexClient = Substitute.For<SearchIndexClient>();
        mockServiceProvider = Substitute.For<IServiceProvider>();
        service = new AzureSearchIndexClientService(mockIndexClient, mockServiceProvider);
    }


    [Test]
    public void InitializeIndexClient_WithNonExistentIndex_ThrowsInvalidOperationException()
    {
        AzureSearchIndexStore.Instance.SetIndices([]);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.InitializeIndexClient("NonExistentIndex", CancellationToken.None));

        Assert.That(exception!.Message, Is.EqualTo("The index 'NonExistentIndex' is not registered."));
    }


    [Test]
    public async Task TryDeleteIndexIfExists_WithExistingIndex_ReturnsTrue()
    {
        var cancellationToken = CancellationToken.None;
        var indexName = "test-index";

        var mockIndex = new SearchIndex(indexName);
        var mockResponse = Response.FromValue(mockIndex, Substitute.For<Response>());
        mockIndexClient.GetIndexAsync(indexName, cancellationToken).Returns(mockResponse);

        var result = await service.TryDeleteIndexIfExists(indexName, onlyIfUnchanged: false, cancellationToken);

        Assert.That(result, Is.True);
        await mockIndexClient.Received(1).GetIndexAsync(indexName, cancellationToken);
        await mockIndexClient.Received(1).DeleteIndexAsync(Arg.Any<SearchIndex>(), false, cancellationToken);
    }


    [Test]
    public async Task TryDeleteIndexIfExists_WithNonExistentIndex_ReturnsFalse()
    {
        var cancellationToken = CancellationToken.None;
        var indexName = "non-existent-index";

        mockIndexClient.GetIndexAsync(indexName, cancellationToken)
            .Returns<Response<SearchIndex>>(_ => throw new RequestFailedException(404, "Not Found"));

        var result = await service.TryDeleteIndexIfExists(indexName, onlyIfUnchanged: false, cancellationToken);

        Assert.That(result, Is.False);
        await mockIndexClient.Received(1).GetIndexAsync(indexName, cancellationToken);
        await mockIndexClient.DidNotReceive().DeleteIndexAsync(Arg.Any<SearchIndex>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }


    [TestCase(" ")]
    [TestCase("")]
    [TestCase(null)]
    public void TryDeleteIndexIfExists_WithNullIndexName_ThrowsArgumentException(string? invalidInput)
    {
        var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.TryDeleteIndexIfExists(invalidInput!, onlyIfUnchanged: false, CancellationToken.None));

        Assert.That(exception!.ParamName, Is.EqualTo("indexName"));
    }


    [Test]
    public async Task TryDeleteIndexIfExists_WithOnlyIfUnchangedTrue_PassesCorrectParameter()
    {
        var cancellationToken = CancellationToken.None;
        var indexName = "test-index";

        var mockIndex = new SearchIndex(indexName);
        var mockResponse = Response.FromValue(mockIndex, Substitute.For<Response>());
        mockIndexClient.GetIndexAsync(indexName, cancellationToken).Returns(mockResponse);

        await service.TryDeleteIndexIfExists(indexName, onlyIfUnchanged: true, cancellationToken);

        await mockIndexClient.Received(1).DeleteIndexAsync(Arg.Any<SearchIndex>(), true, cancellationToken);
    }


    [TearDown]
    public void TearDown() => AzureSearchIndexStore.Instance.SetIndices([]);


    [Test]
    public async Task CreateIndex_WithSemanticSearchConfiguration_AddsSuggester()
    {
        // Arrange
        var indexName = "test-index";
        var suggesterName = "test-suggester";

        // Create a strategy that returns a semantic ranking configuration with a suggester
        var mockStrategy = Substitute.For<IAzureSearchIndexingStrategy>();
        var suggester = new SearchSuggester(suggesterName, TEST_FIELD_NAME);
        var semanticConfig = new SemanticRankingConfiguration(new SemanticSearch());
        semanticConfig.Suggesters.Add(suggester);
        mockStrategy.CreateSemanticRankingConfigurationOrNull().Returns(semanticConfig);
        mockStrategy.GetSearchFields().Returns([new SearchField(TEST_FIELD_NAME, SearchFieldDataType.String) { IsKey = true }]);

        // Create index
        var azureSearchIndex = new AzureSearchIndex(
            new Admin.AzureSearchConfigurationModel
            {
                IndexName = indexName,
                ChannelName = "test",
                StrategyName = "TestStrategy",
                LanguageNames = ["en"]
            },
            StrategyStorage.Strategies
        );

        // Register the index and mock service provider to return strategy
        AzureSearchIndexStore.Instance.SetIndices([]);
        AzureSearchIndexStore.Instance.AddIndex(azureSearchIndex);
        mockServiceProvider.GetService(Arg.Any<Type>()).Returns(mockStrategy);

        // Mock CreateOrUpdateIndexAsync to capture the index being sent
        SearchIndex? capturedIndex = null;
        mockIndexClient.CreateOrUpdateIndexAsync(Arg.Do<SearchIndex>(x => capturedIndex = x), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Response.FromValue(callInfo.Arg<SearchIndex>(), Substitute.For<Response>()));

        // Act
        await service.CreateIndex(azureSearchIndex, CancellationToken.None);

        // Assert
        Assert.That(capturedIndex, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedIndex!.Suggesters, Has.Count.EqualTo(1));
            Assert.That(capturedIndex.Suggesters[0].Name, Is.EqualTo(suggesterName));
        });
    }


    [Test]
    public async Task EditIndex_WithExistingSuggester_DoesNotDuplicateSuggester()
    {
        // Arrange
        var indexName = "test-index";
        var suggesterName = "test-suggester";

        // Create a strategy that returns a semantic ranking configuration with a suggester
        var mockStrategy = Substitute.For<IAzureSearchIndexingStrategy>();
        var suggester = new SearchSuggester(suggesterName, TEST_FIELD_NAME);
        var semanticConfig = new SemanticRankingConfiguration(new SemanticSearch());
        semanticConfig.Suggesters.Add(suggester);
        mockStrategy.CreateSemanticRankingConfigurationOrNull().Returns(semanticConfig);
        mockStrategy.GetSearchFields().Returns([new SearchField(TEST_FIELD_NAME, SearchFieldDataType.String) { IsKey = true }]);

        // Create index
        var azureSearchIndex = new AzureSearchIndex(
            new Admin.AzureSearchConfigurationModel
            {
                IndexName = indexName,
                ChannelName = "test",
                StrategyName = "TestStrategy",
                LanguageNames = ["en"]
            },
            StrategyStorage.Strategies
        );

        // Register the index and mock service provider to return strategy
        AzureSearchIndexStore.Instance.SetIndices([]);
        AzureSearchIndexStore.Instance.AddIndex(azureSearchIndex);
        mockServiceProvider.GetService(Arg.Any<Type>()).Returns(mockStrategy);

        // Mock GetIndexAsync to return an existing index with the suggester already present
        var existingIndex = new SearchIndex(indexName);
        existingIndex.Fields.Add(new SearchField(TEST_FIELD_NAME, SearchFieldDataType.String) { IsKey = true });
        existingIndex.Suggesters.Add(suggester);
        existingIndex.SemanticSearch = new SemanticSearch();
        mockIndexClient.GetIndexAsync(indexName, Arg.Any<CancellationToken>())
            .Returns(Response.FromValue(existingIndex, Substitute.For<Response>()));

        // Mock CreateOrUpdateIndexAsync to capture the index being sent
        SearchIndex? capturedIndex = null;
        mockIndexClient.CreateOrUpdateIndexAsync(Arg.Do<SearchIndex>(x => capturedIndex = x), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Response.FromValue(callInfo.Arg<SearchIndex>(), Substitute.For<Response>()));

        // Act - Edit the index (simulating a second save)
        var configModel = new Admin.AzureSearchConfigurationModel
        {
            IndexName = indexName,
            ChannelName = "test",
            StrategyName = "TestStrategy",
            LanguageNames = ["en"]
        };
        await service.EditIndex(azureSearchIndex, configModel, CancellationToken.None);

        // Assert - Check that suggester was not duplicated
        Assert.That(capturedIndex, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedIndex!.Suggesters, Has.Count.EqualTo(1), "Suggester should not be duplicated on edit");
            Assert.That(capturedIndex.Suggesters[0].Name, Is.EqualTo(suggesterName));
        });
    }


    [Test]
    public async Task EditIndex_WithUpdatedSuggesterConfiguration_UpdatesSuggester()
    {
        // Arrange
        var indexName = "test-index";
        var suggesterName = "test-suggester";
        var oldFieldName = TEST_FIELD_NAME;
        var newFieldName = "field2";

        // Create a strategy that returns a semantic ranking configuration with an updated suggester
        var mockStrategy = Substitute.For<IAzureSearchIndexingStrategy>();
        var updatedSuggester = new SearchSuggester(suggesterName, newFieldName, TEST_FIELD_NAME);
        var semanticConfig = new SemanticRankingConfiguration(new SemanticSearch());
        semanticConfig.Suggesters.Add(updatedSuggester);
        mockStrategy.CreateSemanticRankingConfigurationOrNull().Returns(semanticConfig);
        mockStrategy.GetSearchFields().Returns([
            new SearchField(TEST_FIELD_NAME, SearchFieldDataType.String) { IsKey = true },
            new SearchField(newFieldName, SearchFieldDataType.String)
        ]);

        // Create index
        var azureSearchIndex = new AzureSearchIndex(
            new Admin.AzureSearchConfigurationModel
            {
                IndexName = indexName,
                ChannelName = "test",
                StrategyName = "TestStrategy",
                LanguageNames = ["en"]
            },
            StrategyStorage.Strategies
        );

        // Register the index and mock service provider to return strategy
        AzureSearchIndexStore.Instance.SetIndices([]);
        AzureSearchIndexStore.Instance.AddIndex(azureSearchIndex);
        mockServiceProvider.GetService(Arg.Any<Type>()).Returns(mockStrategy);

        // Mock GetIndexAsync to return an existing index with the old suggester configuration
        var existingIndex = new SearchIndex(indexName);
        existingIndex.Fields.Add(new SearchField(TEST_FIELD_NAME, SearchFieldDataType.String) { IsKey = true });
        existingIndex.Fields.Add(new SearchField(newFieldName, SearchFieldDataType.String));
        var oldSuggester = new SearchSuggester(suggesterName, oldFieldName);
        existingIndex.Suggesters.Add(oldSuggester);
        existingIndex.SemanticSearch = new SemanticSearch();
        mockIndexClient.GetIndexAsync(indexName, Arg.Any<CancellationToken>())
            .Returns(Response.FromValue(existingIndex, Substitute.For<Response>()));

        // Mock CreateOrUpdateIndexAsync to capture the index being sent
        SearchIndex? capturedIndex = null;
        mockIndexClient.CreateOrUpdateIndexAsync(Arg.Do<SearchIndex>(x => capturedIndex = x), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Response.FromValue(callInfo.Arg<SearchIndex>(), Substitute.For<Response>()));

        // Act - Edit the index (simulating an update to the suggester configuration)
        var configModel = new Admin.AzureSearchConfigurationModel
        {
            IndexName = indexName,
            ChannelName = "test",
            StrategyName = "TestStrategy",
            LanguageNames = ["en"]
        };
        await service.EditIndex(azureSearchIndex, configModel, CancellationToken.None);

        // Assert - Check that suggester was updated with new configuration
        Assert.That(capturedIndex, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedIndex!.Suggesters, Has.Count.EqualTo(1), "Should have exactly one suggester");
            Assert.That(capturedIndex.Suggesters[0].Name, Is.EqualTo(suggesterName));
            Assert.That(capturedIndex.Suggesters[0].SourceFields, Has.Count.EqualTo(2), "Updated suggester should have two source fields");
            Assert.That(capturedIndex.Suggesters[0].SourceFields, Does.Contain(newFieldName), "Updated suggester should include new field");
        });
    }
}
