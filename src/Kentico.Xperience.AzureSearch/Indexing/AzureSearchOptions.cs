namespace Kentico.Xperience.AzureSearch.Indexing;

public sealed class AzureSearchOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string CMS_AZURE_SEARCH_SECTION_NAME = "CMSAzureSearch";

    /// <summary>
    /// Azure 
    /// </summary>
    public string SearchServiceEndPoint
    {
        get;
        set;
    } = "";

    /// <summary>
    /// Azure 
    /// </summary>
    public string SearchServiceAdminApiKey
    {
        get;
        set;
    } = "";

    /// <summary>
    /// Azure 
    /// </summary>
    public string SearchServiceQueryApiKey
    {
        get;
        set;
    } = "";
}
