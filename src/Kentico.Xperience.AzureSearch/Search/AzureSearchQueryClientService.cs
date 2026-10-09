using Azure;
using Azure.Search.Documents;

using Kentico.Xperience.AzureSearch.Indexing;

namespace Kentico.Xperience.AzureSearch.Search;

/// <inheritdoc />
public sealed class AzureSearchQueryClientService : IAzureSearchQueryClientService
{
    private readonly AzureSearchQueryClientOptions settings;


    private readonly SearchClientOptions clientOptions;


    private readonly IAzureSearchIndexNameResolver indexNameResolver;


    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchQueryClientService"/> class.
    /// </summary>
    /// <param name="settings">The settings for the Azure Search query client.</param>
    public AzureSearchQueryClientService(AzureSearchQueryClientOptions settings)
        : this(settings, new SearchClientOptions())
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchQueryClientService"/> class with specified settings and client options.
    /// Index and alias names are used without a prefix.
    /// </summary>
    /// <param name="settings">The settings for the Azure Search query client.</param>
    /// <param name="clientOptions">The client options for the Azure Search query client.</param>
    public AzureSearchQueryClientService(AzureSearchQueryClientOptions settings, SearchClientOptions clientOptions)
        : this(settings, clientOptions, new AzureSearchIndexNameResolver(null))
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchQueryClientService"/> class with specified settings, client options and index name resolver.
    /// </summary>
    /// <param name="settings">The settings for the Azure Search query client.</param>
    /// <param name="clientOptions">The client options for the Azure Search query client.</param>
    /// <param name="indexNameResolver">Resolver translating index and alias names stored in Xperience to names in Azure AI Search.</param>
    public AzureSearchQueryClientService(AzureSearchQueryClientOptions settings, SearchClientOptions clientOptions, IAzureSearchIndexNameResolver indexNameResolver)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(clientOptions);
        ArgumentNullException.ThrowIfNull(indexNameResolver);

        this.settings = settings;
        this.clientOptions = clientOptions;
        this.indexNameResolver = indexNameResolver;
    }


    /// <summary>
    /// Gets user settings from appsettings.json and initializes <see cref="SearchClient"/>
    /// </summary>
    /// <param name="indexName">The name of the index or alias as stored in Xperience. The configured index name prefix is applied automatically.</param>
    /// <returns>Initialized <see cref="SearchClient"/></returns>
    public SearchClient CreateSearchClientForQueries(string indexName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        string azureIndexName = indexNameResolver.GetAzureName(indexName);

        return new SearchClient(new Uri(settings.ServiceEndpoint), azureIndexName, new AzureKeyCredential(settings.QueryApiKey), clientOptions);
    }
}
