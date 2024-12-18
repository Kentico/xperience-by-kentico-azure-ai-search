﻿namespace Kentico.Xperience.AzureSearch.Indexing;

public sealed class AzureSearchOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string CMS_AZURE_SEARCH_SECTION_NAME = "CMSAzureSearch";

    /// <summary>
    /// /// Turn off functionality if application is not configured in the appsettings
    /// </summary>
    public bool SearchServiceEnabled
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Url of azure search provider see <see href="https://learn.microsoft.com/en-us/azure/search/search-manage"/> for more information.
    /// </summary>
    public string SearchServiceEndPoint
    {
        get;
        set;
    } = string.Empty;

    /// <summary>
    /// Admin api key used for azure search service management. see <see href="https://learn.microsoft.com/en-us/azure/search/search-manage"/> for more information. 
    /// </summary>
    public string SearchServiceAdminApiKey
    {
        get;
        set;
    } = string.Empty;

    /// <summary>
    /// Client api key used to retrieve indexed data from azure search portal. see <see href="https://learn.microsoft.com/en-us/azure/search/search-manage"/> for more information.  
    /// </summary>
    public string SearchServiceQueryApiKey
    {
        get;
        set;
    } = string.Empty;

    /// <summary>
    /// Optional delay between indexing individual <see cref="IIndexEventItemModel"/>s.
    /// </summary>
    public int IndexItemDelay { get; set; }
}
