namespace DancingGoat.Search.Models;

public class DancingGoatSearchViewModel
{
    public int TotalHits { get; set; }
    public string Query { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int Page { get; set; }
    public string IndexName { get; set; }
    public string Endpoint { get; set; }

    public IEnumerable<DancingGoatSearchResult> Hits { get; set; }
}
