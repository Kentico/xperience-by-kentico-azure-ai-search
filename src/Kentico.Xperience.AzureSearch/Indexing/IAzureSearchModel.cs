namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Abstraction of properties used to define, create and retrieve data from azure search portal.
/// </summary>
public interface IAzureSearchModel
{
    public string? Url { get; set; }
    public string ContentTypeName { get; set; }
    public string LanguageName { get; set; }
    public string ItemGuid { get; set; }
    public string ObjectID { get; set; }
    public string Name { get; set; }
}
