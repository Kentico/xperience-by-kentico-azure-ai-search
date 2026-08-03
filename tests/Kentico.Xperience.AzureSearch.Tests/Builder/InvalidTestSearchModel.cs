using Azure.Search.Documents.Indexes;

using Kentico.Xperience.AzureSearch.Indexing;

namespace Kentico.Xperience.AzureSearch.Tests;

// Invalid test search model without key field (override the base key field)
internal class InvalidTestSearchModel : IAzureSearchModel
{
    public string? Url { get; set; } = string.Empty;
    public string ContentTypeName { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;
    public string ItemGuid { get; set; } = string.Empty;
    public string ObjectID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    [SearchableField]
    public string Title { get; set; } = string.Empty;
}
