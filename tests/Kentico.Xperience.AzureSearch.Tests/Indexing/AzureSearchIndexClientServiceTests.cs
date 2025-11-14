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
    [Test]
    public void InitializeIndexClient_WithNonExistentIndex_ThrowsInvalidOperationException()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        var service = new AzureSearchIndexClientService(mockIndexClient, mockServiceProvider);
        var cancellationToken = CancellationToken.None;

        AzureSearchIndexStore.Instance.SetIndices([]);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.InitializeIndexClient("NonExistentIndex", cancellationToken));

        Assert.That(exception!.Message, Is.EqualTo("The index 'NonExistentIndex' is not registered."));
    }


    [Test]
    public async Task TryDeleteIndexIfExists_WithExistingIndex_ReturnsTrue()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        var service = new AzureSearchIndexClientService(mockIndexClient, mockServiceProvider);
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
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        var service = new AzureSearchIndexClientService(mockIndexClient, mockServiceProvider);
        var cancellationToken = CancellationToken.None;
        var indexName = "non-existent-index";

        mockIndexClient.GetIndexAsync(indexName, cancellationToken)
            .Returns<Response<SearchIndex>>(_ => throw new RequestFailedException(404, "Not Found"));

        var result = await service.TryDeleteIndexIfExists(indexName, onlyIfUnchanged: false, cancellationToken);

        Assert.That(result, Is.False);
        await mockIndexClient.Received(1).GetIndexAsync(indexName, cancellationToken);
        await mockIndexClient.DidNotReceive().DeleteIndexAsync(Arg.Any<SearchIndex>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }


    [Test]
    public void TryDeleteIndexIfExists_WithNullIndexName_ThrowsArgumentException()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        var service = new AzureSearchIndexClientService(mockIndexClient, mockServiceProvider);
        var cancellationToken = CancellationToken.None;

        var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.TryDeleteIndexIfExists(null!, onlyIfUnchanged: false, cancellationToken));

        Assert.That(exception!.ParamName, Is.EqualTo("indexName"));
    }


    [Test]
    public void TryDeleteIndexIfExists_WithEmptyIndexName_ThrowsArgumentException()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        var service = new AzureSearchIndexClientService(mockIndexClient, mockServiceProvider);
        var cancellationToken = CancellationToken.None;

        var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.TryDeleteIndexIfExists("", onlyIfUnchanged: false, cancellationToken));

        Assert.That(exception!.ParamName, Is.EqualTo("indexName"));
    }


    [Test]
    public async Task TryDeleteIndexIfExists_WithOnlyIfUnchangedTrue_PassesCorrectParameter()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        var service = new AzureSearchIndexClientService(mockIndexClient, mockServiceProvider);
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
}
