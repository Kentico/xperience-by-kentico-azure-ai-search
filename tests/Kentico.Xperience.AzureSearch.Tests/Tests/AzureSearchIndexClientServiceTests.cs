using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

using Kentico.Xperience.AzureSearch.Admin;
using Kentico.Xperience.AzureSearch.Indexing;
using Kentico.Xperience.AzureSearch.Tests.Base;

using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Xperience.AzureSearch.Tests.Tests;

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
    public void TearDown()
    {
        AzureSearchIndexStore.Instance.SetIndicies([]);
    }
}
