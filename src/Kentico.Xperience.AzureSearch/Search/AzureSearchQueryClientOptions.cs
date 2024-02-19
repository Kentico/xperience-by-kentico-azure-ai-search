namespace Kentico.Xperience.AzureSearch.Search;

public class AzureSearchQueryClientOptions
{
    public string ServiceEndpoint { get; set; }
    public string QueryApiKey { get; set; }

    public AzureSearchQueryClientOptions(string serviceEndpoint, string queryApiKey)
    {
        ServiceEndpoint = serviceEndpoint;
        QueryApiKey = queryApiKey;
    }
}
