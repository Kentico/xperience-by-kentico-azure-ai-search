using Azure.Search.Documents.Indexes;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Base implementation of <see cref="IAzureSearchModel"/> with decorators used to specify properties of indexed columns. <see href="https://learn.microsoft.com/en-us/azure/search/search-howto-dotnet-sdk"/>
/// </summary>
public class BaseAzureSearchModel : IAzureSearchModel
{
    [SearchableField(IsSortable = true, IsFilterable = true, IsFacetable = true)]
    public string? Url { get; set; } = string.Empty;

    [SearchableField(IsFacetable = true, IsFilterable = true)]
    public string ContentTypeName { get; set; } = string.Empty;

    [SearchableField(IsSortable = true, IsFacetable = true, IsFilterable = true)]
    public string LanguageName { get; set; } = string.Empty;

    [SimpleField(IsKey = false)]
    public string ItemGuid { get; set; } = string.Empty;

    [SimpleField(IsKey = true)]
    public string ObjectID { get; set; } = string.Empty;

    [SimpleField(IsKey = false)]
    public string Name { get; set; } = string.Empty;
}
