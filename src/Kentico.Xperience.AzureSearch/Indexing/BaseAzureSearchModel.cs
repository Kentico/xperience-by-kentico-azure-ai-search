using Azure.Search.Documents.Indexes;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Base implementation of <see cref="IAzureSearchModel"/> with decorators used to specify properties of indexed columns. <see href="https://learn.microsoft.com/en-us/azure/search/search-howto-dotnet-sdk"/>
/// </summary>
public class BaseAzureSearchModel : IAzureSearchModel
{
    [SearchableField(IsSortable = true, IsFilterable = true, IsFacetable = true)]
    public string? Url { get; set; } = "";

    [SearchableField(IsFacetable = true, IsFilterable = true)]
    public string ContentTypeName { get; set; } = "";

    [SearchableField(IsSortable = true, IsFacetable = true, IsFilterable = true)]
    public string LanguageName { get; set; } = "";

    [SimpleField(IsKey = false)]
    public string ItemGuid { get; set; } = "";

    [SimpleField(IsKey = true)]
    public string ObjectID { get; set; } = "";

    [SimpleField(IsKey = false)]
    public string Name { get; set; } = "";
}
