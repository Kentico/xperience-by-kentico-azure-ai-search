using System.Text.Json.Serialization;

namespace Kentico.Xperience.AzureSearch.Admin;

public class AzureSearchIndexIncludedPath
{
    /// <summary>
    /// The node alias pattern that will be used to match pages in the content tree for indexing.
    /// </summary>
    /// <remarks>For example, "/Blogs/Products/" will index all pages under the "Products" page.</remarks>
    public string AliasPath { get; }

    /// <summary>
    /// A list of content types under the specified <see cref="AliasPath"/> that will be indexed.
    /// </summary>
    public List<string> ContentTypes { get; set; } = new();

    /// <summary>
    /// The internal identifier of the included path.
    /// </summary>
    public string? Identifier { get; set; }

    [JsonConstructor]
    public AzureSearchIndexIncludedPath(string aliasPath) => AliasPath = aliasPath;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="indexPath"></param>
    /// <param name="contentTypes"></param>
    public AzureSearchIndexIncludedPath(AzureSearchIncludedPathItemInfo indexPath, IEnumerable<AzureSearchContentTypeItemInfo> contentTypes)
    {
        AliasPath = indexPath.AzureSearchIncludedPathItemAliasPath;
        ContentTypes = contentTypes.Where(y => indexPath.AzureSearchIncludedPathItemId == y.AzureSearchContentTypeItemIncludedPathItemId).Select(y => y.AzureSearchContentTypeItemContentTypeName).ToList();
        Identifier = indexPath.AzureSearchIncludedPathItemId.ToString();
    }
}
