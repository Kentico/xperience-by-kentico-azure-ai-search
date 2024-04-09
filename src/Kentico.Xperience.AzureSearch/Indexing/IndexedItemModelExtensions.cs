using CMS.Core;
using CMS.Websites.Internal;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// AzureSearch extension methods for the <see cref="IIndexEventItemModel"/> class.
/// </summary>
internal static class IndexedItemModelExtensions
{
    /// <summary>
    /// Returns true if the node is included in the AzureSearch index based on the index's defined paths
    /// </summary>
    /// <remarks>Logs an error if the search model cannot be found.</remarks>
    /// <param name="item">The node to check for indexing.</param>
    /// <param name="log"></param>
    /// <param name="indexName">The AzureSearch index code name.</param>
    /// <param name="eventName"></param>
    /// <exception cref="ArgumentNullException" />
    public static bool IsIndexedByIndex(this IndexEventWebPageItemModel item, IEventLogService log, string indexName, string eventName)
    {
        if (string.IsNullOrWhiteSpace(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        var azureSearchIndex = AzureSearchIndexStore.Instance.GetIndex(indexName);

        if (azureSearchIndex is null)
        {
            log.LogError(nameof(IndexedItemModelExtensions), nameof(IsIndexedByIndex), $"Error loading registered AzureSearch index '{indexName}' for event [{eventName}].");
            
            return false;
        }

        if (!azureSearchIndex.LanguageNames.Exists(x => x == item.LanguageName))
        {
            return false;
        }

        return azureSearchIndex.IncludedPaths.Any(path =>
        {
            bool matchesContentType = path.ContentTypes.Exists(x => string.Equals(x.ContentTypeName, item.ContentTypeName));

            if (!matchesContentType)
            {
                return false;
            }

            // Supports wildcard matching
            if (path.AliasPath.EndsWith("/%", StringComparison.OrdinalIgnoreCase))
            {
                string pathToMatch = path.AliasPath[..^2];
                var pathsOnPath = TreePathUtils.GetTreePathsOnPath(item.WebPageItemTreePath, true, false).ToHashSet();

                return pathsOnPath.Any(p => p.StartsWith(pathToMatch, StringComparison.OrdinalIgnoreCase));
            }

            return item.WebPageItemTreePath.Equals(path.AliasPath, StringComparison.OrdinalIgnoreCase);
        });
    }

    /// <summary>
    /// Returns true if the node is included in the AzureSearch index's allowed
    /// </summary>
    /// <remarks>Logs an error if the search model cannot be found.</remarks>
    /// <param name="item">The node to check for indexing.</param>
    /// <param name="log"></param>
    /// <param name="indexName">The AzureSearch index code name.</param>
    /// <param name="eventName"></param>
    /// <exception cref="ArgumentNullException" />
    public static bool IsIndexedByIndex(this IndexEventReusableItemModel item, IEventLogService log, string indexName, string eventName)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        var azureSearchIndex = AzureSearchIndexStore.Instance.GetIndex(indexName);

        if (azureSearchIndex is null)
        {
            log.LogError(nameof(IndexedItemModelExtensions), nameof(IsIndexedByIndex), $"Error loading registered AzureSearch index '{indexName}' for event [{eventName}].");
            
            return false;
        }

        return azureSearchIndex.LanguageNames.Exists(x => x == item.LanguageName);
    }
}
