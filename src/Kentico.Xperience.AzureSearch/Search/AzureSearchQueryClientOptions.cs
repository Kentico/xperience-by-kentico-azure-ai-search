namespace Kentico.Xperience.AzureSearch.Search;

public class AzureSearchQueryClientOptions
{
    public string ServiceEndpoint { get; set; }
    public string QueryApiKey { get; set; }

    /// <summary>
    /// Optional prefix applied to index and alias names when creating query clients. See <see cref="Indexing.AzureSearchOptions.IndexNamePrefix"/>.
    /// </summary>
    public string IndexNamePrefix { get; set; } = string.Empty;

    public AzureSearchQueryClientOptions(string serviceEndpoint, string queryApiKey)
    {
        ServiceEndpoint = serviceEndpoint;
        QueryApiKey = queryApiKey;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchQueryClientOptions"/> class.
    /// </summary>
    /// <param name="serviceEndpoint">Azure AI Search service endpoint.</param>
    /// <param name="queryApiKey">Query API key.</param>
    /// <param name="indexNamePrefix">Prefix applied to index and alias names. See <see cref="Indexing.AzureSearchOptions.IndexNamePrefix"/>.</param>
    public AzureSearchQueryClientOptions(string serviceEndpoint, string queryApiKey, string? indexNamePrefix)
        : this(serviceEndpoint, queryApiKey) => IndexNamePrefix = indexNamePrefix ?? string.Empty;
}
