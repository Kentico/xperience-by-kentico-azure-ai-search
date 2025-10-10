using Azure.Search.Documents.Indexes;

using Kentico.Xperience.AzureSearch.Indexing;

namespace Kentico.Xperience.AzureSearch.Tests.Indexing;

[TestFixture]
[Category(Category.Unit)]
internal class AzureSearchIndexClientServiceTests
{
    [Test]
    public void InitializeIndexClient_WithNonExistentIndex_ThrowsInvalidOperationException()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        var service = new AzureSearchIndexClientService(mockIndexClient, mockServiceProvider);
        var cancellationToken = CancellationToken.None;

        AzureSearchIndexStore.Instance.SetIndicies([]);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.InitializeIndexClient("NonExistentIndex", cancellationToken));

        Assert.That(exception!.Message, Is.EqualTo("Registered index with name 'NonExistentIndex' doesn't exist."));
    }


    [TearDown]
    public void TearDown() => AzureSearchIndexStore.Instance.SetIndicies([]);
}
