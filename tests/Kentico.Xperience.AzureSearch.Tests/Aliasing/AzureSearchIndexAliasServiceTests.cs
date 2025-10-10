using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

using CMS.Tests;

using Kentico.Xperience.AzureSearch.Aliasing;
using Kentico.Xperience.AzureSearch.Tests.Base;

namespace Kentico.Xperience.AzureSearch.Tests.Aliasing;

[TestFixture]
[Category.Unit]
internal class AzureSearchIndexAliasServiceTests
{
    private const string TEST_INDEX_NAME = "test-index";


    [Test]
    public async Task CreateAlias_WithValidParameters_CreatesAlias()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient);
        var alias = new SearchAlias(MockDataProvider.ALIAS_NAME, [TEST_INDEX_NAME]);
        var cancellationToken = CancellationToken.None;

        await service.CreateAlias(alias, cancellationToken);

        await mockIndexClient.Received(1).CreateOrUpdateAliasAsync(MockDataProvider.ALIAS_NAME, alias, cancellationToken: cancellationToken);
    }


    [Test]
    public void CreateAlias_WithNullAlias_ThrowsArgumentNullException()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient);
        var cancellationToken = CancellationToken.None;

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.CreateAlias(null!, cancellationToken));
    }


    [Test]
    public async Task EditAlias_WithValidParameters_DeletesOldAndCreatesNew()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient);
        var newAlias = new SearchAlias(MockDataProvider.NEW_ALIAS_NAME, [TEST_INDEX_NAME]);
        var cancellationToken = CancellationToken.None;

        await service.EditAlias(MockDataProvider.ALIAS_NAME, newAlias, cancellationToken);

        await mockIndexClient.Received(1).DeleteAliasAsync(MockDataProvider.ALIAS_NAME, cancellationToken);
        await mockIndexClient.Received(1).CreateOrUpdateAliasAsync(MockDataProvider.NEW_ALIAS_NAME, newAlias, cancellationToken: cancellationToken);
    }


    [Test]
    public void EditAlias_WithNullOldAliasName_ThrowsArgumentNullException()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient);
        var newAlias = new SearchAlias(MockDataProvider.NEW_ALIAS_NAME, [TEST_INDEX_NAME]);
        var cancellationToken = CancellationToken.None;

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.EditAlias(null!, newAlias, cancellationToken));
    }


    [Test]
    public void EditAlias_WithEmptyOldAliasName_ThrowsArgumentNullException()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient);
        var newAlias = new SearchAlias(MockDataProvider.NEW_ALIAS_NAME, [TEST_INDEX_NAME]);
        var cancellationToken = CancellationToken.None;

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.EditAlias(string.Empty, newAlias, cancellationToken));
    }


    [Test]
    public async Task DeleteAlias_WithValidParameters_DeletesAlias()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient);
        var cancellationToken = CancellationToken.None;

        await service.DeleteAlias(MockDataProvider.ALIAS_NAME, cancellationToken);

        await mockIndexClient.Received(1).DeleteAliasAsync(MockDataProvider.ALIAS_NAME, cancellationToken);
    }


    [Test]
    public void DeleteAlias_WithNullAliasName_ThrowsArgumentNullException()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient);
        var cancellationToken = CancellationToken.None;

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.DeleteAlias(null!, cancellationToken));
    }


    [Test]
    public void DeleteAlias_WithEmptyAliasName_ThrowsArgumentNullException()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient);
        var cancellationToken = CancellationToken.None;

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.DeleteAlias(string.Empty, cancellationToken));
    }
}
