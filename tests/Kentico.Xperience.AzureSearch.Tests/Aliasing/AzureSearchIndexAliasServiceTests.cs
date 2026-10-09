using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

using CMS.Tests;

using Kentico.Xperience.AzureSearch.Aliasing;
using Kentico.Xperience.AzureSearch.Indexing;
using Kentico.Xperience.AzureSearch.Tests.Base;

namespace Kentico.Xperience.AzureSearch.Tests.Aliasing;

[TestFixture]
[Category.Unit]
internal class AzureSearchIndexAliasServiceTests
{
    private const string TEST_INDEX_NAME = "test-index";
    private const string PREFIX = "dev-";


    [Test]
    public async Task CreateAlias_WithValidParameters_CreatesAlias()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient, new AzureSearchIndexNameResolver(null));
        var alias = new SearchAlias(MockDataProvider.ALIAS_NAME, [TEST_INDEX_NAME]);
        var cancellationToken = CancellationToken.None;

        await service.CreateAlias(alias, cancellationToken);

        await mockIndexClient.Received(1).CreateOrUpdateAliasAsync(alias, cancellationToken: cancellationToken);
    }


    [Test]
    public void CreateAlias_WithNullAlias_ThrowsArgumentNullException()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient, new AzureSearchIndexNameResolver(null));
        var cancellationToken = CancellationToken.None;

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.CreateAlias(null!, cancellationToken));
    }


    [Test]
    public async Task EditAlias_WithValidParameters_DeletesOldAndCreatesNew()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient, new AzureSearchIndexNameResolver(null));
        var newAlias = new SearchAlias(MockDataProvider.NEW_ALIAS_NAME, [TEST_INDEX_NAME]);
        var cancellationToken = CancellationToken.None;

        await service.EditAlias(MockDataProvider.ALIAS_NAME, newAlias, cancellationToken);

        await mockIndexClient.Received(1).DeleteAliasAsync(MockDataProvider.ALIAS_NAME, Arg.Any<MatchConditions>(), cancellationToken);
        await mockIndexClient.Received(1).CreateOrUpdateAliasAsync(newAlias, cancellationToken: cancellationToken);
    }


    [Test]
    public void EditAlias_WithNullOldAliasName_ThrowsArgumentNullException()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient, new AzureSearchIndexNameResolver(null));
        var newAlias = new SearchAlias(MockDataProvider.NEW_ALIAS_NAME, [TEST_INDEX_NAME]);
        var cancellationToken = CancellationToken.None;

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.EditAlias(null!, newAlias, cancellationToken));
    }


    [Test]
    public void EditAlias_WithEmptyOldAliasName_ThrowsArgumentNullException()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient, new AzureSearchIndexNameResolver(null));
        var newAlias = new SearchAlias(MockDataProvider.NEW_ALIAS_NAME, [TEST_INDEX_NAME]);
        var cancellationToken = CancellationToken.None;

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.EditAlias(string.Empty, newAlias, cancellationToken));
    }


    [Test]
    public async Task DeleteAlias_WithValidParameters_DeletesAlias()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient, new AzureSearchIndexNameResolver(null));
        var cancellationToken = CancellationToken.None;

        await service.DeleteAlias(MockDataProvider.ALIAS_NAME, cancellationToken);

        await mockIndexClient.Received(1).DeleteAliasAsync(MockDataProvider.ALIAS_NAME, Arg.Any<MatchConditions>(), cancellationToken);
    }


    [Test]
    public void DeleteAlias_WithNullAliasName_ThrowsArgumentNullException()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient, new AzureSearchIndexNameResolver(null));
        var cancellationToken = CancellationToken.None;

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.DeleteAlias(null!, cancellationToken));
    }


    [Test]
    public void DeleteAlias_WithEmptyAliasName_ThrowsArgumentNullException()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient, new AzureSearchIndexNameResolver(null));
        var cancellationToken = CancellationToken.None;

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.DeleteAlias(string.Empty, cancellationToken));
    }


    [Test]
    public async Task CreateAlias_WithPrefix_CreatesPrefixedAliasForPrefixedIndex()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient, new AzureSearchIndexNameResolver(PREFIX));
        var alias = new SearchAlias(MockDataProvider.ALIAS_NAME, [TEST_INDEX_NAME]);

        await service.CreateAlias(alias, CancellationToken.None);

        await mockIndexClient.Received(1).CreateOrUpdateAliasAsync(
            Arg.Is<SearchAlias>(a => a.Name == PREFIX + MockDataProvider.ALIAS_NAME && a.Indexes.Single() == PREFIX + TEST_INDEX_NAME),
            cancellationToken: CancellationToken.None);
    }


    [Test]
    public async Task EditAlias_WithPrefix_DeletesPrefixedOldAliasAndCreatesPrefixedNewAlias()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient, new AzureSearchIndexNameResolver(PREFIX));
        var newAlias = new SearchAlias(MockDataProvider.NEW_ALIAS_NAME, [TEST_INDEX_NAME]);

        await service.EditAlias(MockDataProvider.ALIAS_NAME, newAlias, CancellationToken.None);

        await mockIndexClient.Received(1).DeleteAliasAsync(PREFIX + MockDataProvider.ALIAS_NAME, Arg.Any<MatchConditions>(), CancellationToken.None);
        await mockIndexClient.Received(1).CreateOrUpdateAliasAsync(
            Arg.Is<SearchAlias>(a => a.Name == PREFIX + MockDataProvider.NEW_ALIAS_NAME && a.Indexes.Single() == PREFIX + TEST_INDEX_NAME),
            cancellationToken: CancellationToken.None);
    }


    [Test]
    public async Task DeleteAlias_WithPrefix_DeletesPrefixedAlias()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient, new AzureSearchIndexNameResolver(PREFIX));

        await service.DeleteAlias(MockDataProvider.ALIAS_NAME, CancellationToken.None);

        await mockIndexClient.Received(1).DeleteAliasAsync(PREFIX + MockDataProvider.ALIAS_NAME, Arg.Any<MatchConditions>(), CancellationToken.None);
    }
}
