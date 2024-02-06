namespace Kentico.Xperience.AzureSearch.Search;

public class AzureSearchSearchResultModel<T>
{
    public string Query { get; set; } = "";
    public IEnumerable<T> Hits { get; set; } = new List<T>();
    public int TotalHits { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int Page { get; set; }

    public string? Facet { get; set; }
}
