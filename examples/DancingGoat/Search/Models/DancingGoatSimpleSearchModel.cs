using Azure.Search.Documents.Indexes;
using Kentico.Xperience.AzureSearch.Indexing;

namespace DancingGoat.Search.Models;

public class DancingGoatSimpleSearchModel : BaseAzureSearchModel
{
    [SearchableField]
    public string Title { get; set; }
}
