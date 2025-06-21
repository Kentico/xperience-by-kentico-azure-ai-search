using Azure.Search.Documents.Indexes.Models;

namespace Kentico.Xperience.AzureSearch.Indexing;
public sealed class OnBeforeCreatingOrUpdatingIndexEventArgs : EventArgs
{
    public SearchIndex SearchIndex { get; set; }

    public OnBeforeCreatingOrUpdatingIndexEventArgs(SearchIndex searchIndex)
        => SearchIndex = searchIndex;
}
