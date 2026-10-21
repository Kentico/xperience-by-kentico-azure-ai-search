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


    private const string OBSOLETE_CONSTRUCTOR_MESSAGE =
        "This constructor ignores the configured CMSAzureSearch:IndexNamePrefix and queries unprefixed index and alias names. " +
        "Resolve IAzureSearchQueryClientService from the dependency injection container or use the constructor accepting IAzureSearchIndexNameResolver.";


    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchQueryClientService"/> class.
    /// </summary>
    /// <remarks>
    /// Index and alias names are used without a prefix, the configured <see cref="AzureSearchOptions.IndexNamePrefix"/> is ignored.
    /// Use <see cref="AzureSearchQueryClientService(AzureSearchQueryClientOptions, SearchClientOptions, IAzureSearchIndexNameResolver)"/> instead.
    /// </remarks>
    /// <param name="settings">The settings for the Azure Search query client.</param>
    [Obsolete(OBSOLETE_CONSTRUCTOR_MESSAGE)]
    public AzureSearchQueryClientService(AzureSearchQueryClientOptions settings)
        : this(settings, new SearchClientOptions())
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchQueryClientService"/> class with specified settings and client options.
    /// </summary>
    /// <remarks>
    /// Index and alias names are used without a prefix, the configured <see cref="AzureSearchOptions.IndexNamePrefix"/> is ignored.
    /// Use <see cref="AzureSearchQueryClientService(AzureSearchQueryClientOptions, SearchClientOptions, IAzureSearchIndexNameResolver)"/> instead.
    /// </remarks>
    /// <param name="settings">The settings for the Azure Search query client.</param>
    /// <param name="clientOptions">The client options for the Azure Search query client.</param>
    [Obsolete(OBSOLETE_CONSTRUCTOR_MESSAGE)]
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
