using Azure.Core.GeoJson;
using Azure.Search.Documents.Indexes;

using Kentico.Xperience.AzureSearch.Indexing;

namespace DancingGoat.Search.Models;

public class GeoLocationSearchModel : BaseAzureSearchModel
{
    [SearchableField]
    public string Title { get; set; }

    [SearchableField]
    public string Content { get; set; }

    [SimpleField(IsSortable = true, IsFilterable = true, IsKey = false)]
    public GeoPoint GeoLocation { get; set; }

    [SearchableField(IsSortable = true, IsFilterable = true, IsFacetable = true)]
    public string Location { get; set; }
}
