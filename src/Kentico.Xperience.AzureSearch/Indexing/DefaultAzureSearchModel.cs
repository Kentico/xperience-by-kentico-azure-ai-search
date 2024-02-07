using Azure.Search.Documents.Indexes;

namespace Kentico.Xperience.AzureSearch.Indexing;

public interface IAzureSearchModel
{
    public string? Url { get; set; }
    public string ContentTypeName { get; set; }
    public string LanguageName { get; set; }
    public string ItemGuid { get; set; }
    public string ObjectID { get; set; }
    public string Name { get; set; }
}

public class DefaultAzureSearchModel : IAzureSearchModel
{
    [SearchableField(IsSortable = true, IsFilterable = true, IsFacetable = true)]
    public string? Url { get; set; } = "";

    [SearchableField(IsFacetable = true, IsFilterable = true)]
    public string ContentTypeName { get; set; } = "";

    [SearchableField(IsSortable = true, IsFacetable = true, IsFilterable = true)]
    public string LanguageName { get; set; } = "";

    [SimpleField(IsKey = false)]
    public string ItemGuid { get; set; } = "";

    [SearchableField(IsKey = true)]
    public string ObjectID { get; set; } = "";

    [SimpleField(IsKey = false)]
    public string Name { get; set; } = "";
}
