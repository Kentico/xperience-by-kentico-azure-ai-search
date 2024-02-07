using Azure.Search.Documents.Indexes;
using Kentico.Xperience.AzureSearch.Indexing;

namespace DancingGoat.Search.Models;

public class DancingGoatSimpleSearchModel : DefaultAzureSearchModel
{
    [SearchableField]
    public string Title { get; set; }
}
