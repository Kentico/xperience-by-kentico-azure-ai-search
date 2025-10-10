using Azure.Search.Documents;

using Kentico.Xperience.AzureSearch.Search;
using Kentico.Xperience.AzureSearch.Tests.Base;

namespace Kentico.Xperience.AzureSearch.Tests.Search;

[TestFixture]
[Category(Category.Unit)]
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
}
