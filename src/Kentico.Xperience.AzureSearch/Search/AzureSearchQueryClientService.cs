using Azure;
using Azure.Search.Documents;

namespace Kentico.Xperience.AzureSearch.Search;

/// <inheritdoc />
public sealed class AzureSearchQueryClientService : IAzureSearchQueryClientService
{
    private readonly AzureSearchQueryClientOptions settings;


    private readonly SearchClientOptions clientOptions;


    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchQueryClientService"/> class.
    /// </summary>
    /// <param name="settings">The settings for the Azure Search query client.</param>
    public AzureSearchQueryClientService(AzureSearchQueryClientOptions settings) : this(settings, new SearchClientOptions())
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchQueryClientService"/> class with specified settings and client options.
    /// </summary>
    /// <param name="settings">The settings for the Azure Search query client.</param>
    /// <param name="clientOptions">The client options for the Azure Search query client.</param>
    public AzureSearchQueryClientService(AzureSearchQueryClientOptions settings, SearchClientOptions clientOptions)
    {
        this.settings = settings;
        this.clientOptions = clientOptions;
    }


    /// <summary>
    /// Gets user settings from appsettings.json and initializes <see cref="SearchClient"/>
    /// </summary>
    /// <param name="indexName"></param>
    /// <returns>Initialized <see cref="SearchClient"/></returns>
    public SearchClient CreateSearchClientForQueries(string indexName) => new(new Uri(settings.ServiceEndpoint), indexName, new AzureKeyCredential(settings.QueryApiKey), clientOptions);
}
