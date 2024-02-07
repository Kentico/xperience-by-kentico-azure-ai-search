using Azure.Search.Documents;
using Azure;

namespace Kentico.Xperience.AzureSearch.Search;

public class AzureSearchQueryClientService : IAzureSearchQueryClientService
{
    private readonly AzureSearchQueryClientOptions settings;

    public AzureSearchQueryClientService(AzureSearchQueryClientOptions settings) => this.settings = settings;

    public SearchClient CreateSearchClientForQueries(string indexName) => new(new Uri(settings.ServiceEndpoint), indexName, new AzureKeyCredential(settings.QueryApiKey));
}
