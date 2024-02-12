using Azure.Search.Documents.Indexes;
using Microsoft.Spatial;

namespace DancingGoat.Search.Models;

public class DancingGoatSearchModel : DancingGoatSimpleSearchModel
{
    [SearchableField]
    public string Content { get; set; }


    //[SearchableField(IsSortable = true, IsFilterable = true, IsFacetable = true)]
    //public GeographyPoint Point { get; set; }
}
