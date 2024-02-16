namespace DancingGoat.Search.Models;

public class GeoLocationSearchViewModel
{
    public int TotalHits { get; set; }
    public string Query { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int Page { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public string IndexName { get; set; }
    public bool SortByDistance { get; set; } = true;
    public string Endpoint { get; set; }

    public IEnumerable<GeoLocationSearchResult> Hits { get; set; }
}
