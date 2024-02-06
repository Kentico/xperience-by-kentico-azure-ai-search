using Azure.Search.Documents.Indexes;

namespace DancingGoat.Search.Models;

public class DancingGoatSearchModel : DancingGoatSimpleSearchModel
{
    [SearchableField]
    public string Content { get; set; }
}
