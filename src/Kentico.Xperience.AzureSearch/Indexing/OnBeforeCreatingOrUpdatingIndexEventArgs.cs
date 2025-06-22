using Azure.Search.Documents.Indexes.Models;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Provides data for the event that occurs before creating or updating a search index.
/// </summary>
/// <remarks>This event allows subscribers to inspect or modify the <see cref="SearchIndex"/> instance before it
/// is created or updated. Changes made to the <see cref="SearchIndex"/> property will affect the subsequent
/// operation.</remarks>
public sealed class OnBeforeCreatingOrUpdatingIndexEventArgs : EventArgs
{
    /// <summary>
    /// The search index used for querying and retrieving data.
    /// </summary>
    public SearchIndex SearchIndex { get; set; }

    public OnBeforeCreatingOrUpdatingIndexEventArgs(SearchIndex searchIndex)
        => SearchIndex = searchIndex;
}
