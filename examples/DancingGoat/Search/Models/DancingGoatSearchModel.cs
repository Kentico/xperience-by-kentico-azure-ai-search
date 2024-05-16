using Azure.Search.Documents.Indexes;

using Kentico.Xperience.AzureSearch.Indexing;

namespace DancingGoat.Search.Models;

public class DancingGoatSearchModel : BaseAzureSearchModel
{
    [SearchableField]
    public string Content { get; set; }

    [SearchableField]
    public string Title { get; set; }
}
