namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Provides events related to the Azure Search indexing process.
/// </summary>
public static class AzureSearchIndexingEvents
{
    /// <summary>
    /// The delegate to be invoked before creating or updating an index.
    /// </summary>
    public static BeforeCreatingOrUpdatingIndex BeforeCreatingOrUpdatingIndex { get; set; } = new();
}
