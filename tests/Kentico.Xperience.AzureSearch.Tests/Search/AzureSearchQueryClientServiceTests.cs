using Azure.Search.Documents;

using CMS.Tests;

using Kentico.Xperience.AzureSearch.Indexing;
using Kentico.Xperience.AzureSearch.Search;
using Kentico.Xperience.AzureSearch.Tests.Base;

namespace Kentico.Xperience.AzureSearch.Tests.Search;

/// <summary>
/// Tests for the <see cref="AzureSearchQueryClientService"/> class.
/// </summary>
[TestFixture]
[Category.Unit]
internal class AzureSearchQueryClientServiceTests
{
    [Test]
    public void CreateSearchClientForQueries_WithValidParameters_ReturnsSearchClient()
    {
        var settings = new AzureSearchQueryClientOptions(MockDataProvider.SERVICE_ENDPOINT, MockDataProvider.QUERY_API_KEY);
        var service = new AzureSearchQueryClientService(settings);

        var result = service.CreateSearchClientForQueries(MockDataProvider.DEFAULT_INDEX);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<SearchClient>());
    }


    [Test]
    public void CreateSearchClientForQueries_InitializesWithCorrectEndpoint()
    {
        var settings = new AzureSearchQueryClientOptions(MockDataProvider.SERVICE_ENDPOINT, MockDataProvider.QUERY_API_KEY);
        var service = new AzureSearchQueryClientService(settings);

        var result = service.CreateSearchClientForQueries(MockDataProvider.DEFAULT_INDEX);

        Assert.That(result.Endpoint.ToString(), Is.EqualTo(MockDataProvider.SERVICE_ENDPOINT + "/"));
    }


    [Test]
    public void CreateSearchClientForQueries_InitializesWithCorrectIndexName()
    {
        var settings = new AzureSearchQueryClientOptions(MockDataProvider.SERVICE_ENDPOINT, MockDataProvider.QUERY_API_KEY);
        var service = new AzureSearchQueryClientService(settings);

        var result = service.CreateSearchClientForQueries(MockDataProvider.DEFAULT_INDEX);

        Assert.That(result.IndexName, Is.EqualTo(MockDataProvider.DEFAULT_INDEX));
    }


    [Test]
    public void Constructor_WithClientOptions_DoesNotThrow()
    {
        var settings = new AzureSearchQueryClientOptions(MockDataProvider.SERVICE_ENDPOINT, MockDataProvider.QUERY_API_KEY);
        var clientOptions = new SearchClientOptions();

        Assert.That(() => new AzureSearchQueryClientService(settings, clientOptions), Throws.Nothing);
    }


    [Test]
    public void CreateSearchClientForQueries_WithCustomClientOptions_ReturnsSearchClient()
    {
        var settings = new AzureSearchQueryClientOptions(MockDataProvider.SERVICE_ENDPOINT, MockDataProvider.QUERY_API_KEY);
        var clientOptions = new SearchClientOptions
        {
            Retry = { MaxRetries = 7 }
        };
        var service = new AzureSearchQueryClientService(settings, clientOptions);

        var result = service.CreateSearchClientForQueries(MockDataProvider.DEFAULT_INDEX);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<SearchClient>());
    }


    [Test]
    public void CreateSearchClientForQueries_WithCustomClientOptions_InitializesWithCorrectEndpoint()
    {
        var settings = new AzureSearchQueryClientOptions(MockDataProvider.SERVICE_ENDPOINT, MockDataProvider.QUERY_API_KEY);
        var clientOptions = new SearchClientOptions();
        var service = new AzureSearchQueryClientService(settings, clientOptions);

        var result = service.CreateSearchClientForQueries(MockDataProvider.DEFAULT_INDEX);

        Assert.That(result.Endpoint.ToString(), Is.EqualTo(MockDataProvider.SERVICE_ENDPOINT + "/"));
    }


    [Test]
    public void CreateSearchClientForQueries_WithCustomClientOptions_InitializesWithCorrectIndexName()
    {
        var settings = new AzureSearchQueryClientOptions(MockDataProvider.SERVICE_ENDPOINT, MockDataProvider.QUERY_API_KEY);
        var clientOptions = new SearchClientOptions();
        var service = new AzureSearchQueryClientService(settings, clientOptions);

        var result = service.CreateSearchClientForQueries(MockDataProvider.DEFAULT_INDEX);

        Assert.That(result.IndexName, Is.EqualTo(MockDataProvider.DEFAULT_INDEX));
    }


    [Test]
    public void CreateSearchClientForQueries_WithIndexNameResolver_UsesResolvedIndexName()
    {
        var settings = new AzureSearchQueryClientOptions(MockDataProvider.SERVICE_ENDPOINT, MockDataProvider.QUERY_API_KEY);
        var resolver = Substitute.For<IAzureSearchIndexNameResolver>();
        resolver.GetAzureName(MockDataProvider.ALIAS_NAME).Returns("dev-" + MockDataProvider.ALIAS_NAME);
        var service = new AzureSearchQueryClientService(settings, new SearchClientOptions(), resolver);

        var result = service.CreateSearchClientForQueries(MockDataProvider.ALIAS_NAME);

        Assert.That(result.IndexName, Is.EqualTo("dev-" + MockDataProvider.ALIAS_NAME));
    }


    [Test]
    public void Constructor_WithNullIndexNameResolver_Throws()
    {
        var settings = new AzureSearchQueryClientOptions(MockDataProvider.SERVICE_ENDPOINT, MockDataProvider.QUERY_API_KEY);

        Assert.That(() => new AzureSearchQueryClientService(settings, new SearchClientOptions(), null!), Throws.ArgumentNullException);
    }
}
