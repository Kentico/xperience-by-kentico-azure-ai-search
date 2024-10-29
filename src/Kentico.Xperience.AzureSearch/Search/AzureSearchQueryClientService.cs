using Azure;
using Azure.Search.Documents;

namespace Kentico.Xperience.AzureSearch.Search;

/// <inheritdoc />
public sealed class AzureSearchQueryClientService : IAzureSearchQueryClientService
{
    private readonly AzureSearchQueryClientOptions settings;

    public AzureSearchQueryClientService(AzureSearchQueryClientOptions settings) => this.settings = settings;

    /// <summary>
    /// Gets user settings from appsettings.json and initializes <see cref="SearchClient"/>
    /// </summary>
    /// <param name="indexName"></param>
    /// <returns>Initialized <see cref="SearchClient"/></returns>
    public SearchClient CreateSearchClientForQueries(string indexName) => new(new Uri(settings.ServiceEndpoint), indexName, new AzureKeyCredential(settings.QueryApiKey));
}
