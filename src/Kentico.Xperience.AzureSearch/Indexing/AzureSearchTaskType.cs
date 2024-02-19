using Kentico.Xperience.AzureSearch.Indexing;

namespace Kentico.Xperience.AzureSearch;

/// <summary>
/// Represents the type of a <see cref="AzureSearchQueueItem"/>
/// </summary>
public enum AzureSearchTaskType
{
    /// <summary>
    /// Unsupported task type
    /// </summary>
    UNKNOWN,

    /// <summary>
    /// A task for a page which should be removed from the index
    /// </summary>
    DELETE,

    /// <summary>
    /// Task marks the end of indexed items, index is published after this task occurs
    /// </summary>
    PUBLISH_INDEX,

    /// <summary>
    /// A task for a page which should be updated
    /// </summary>
    UPDATE
}
