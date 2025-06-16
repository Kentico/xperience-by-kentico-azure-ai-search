namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Abstraction of properties used to define, create and retrieve data from azure search portal.
/// </summary>
public interface IAzureSearchModel
{
    string? Url { get; set; }
    string ContentTypeName { get; set; }
    string LanguageName { get; set; }
    string ItemGuid { get; set; }
    string ObjectID { get; set; }
    string Name { get; set; }
}
