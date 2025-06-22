namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Represents an event that is triggered before creating or updating an index.
/// </summary>
public class BeforeCreatingOrUpdatingIndex
{
    /// <summary>
    /// The event handler that is invoked before creating or updating an index.
    /// </summary>
    public EventHandler<OnBeforeCreatingOrUpdatingIndexEventArgs>? Execute { get; set; }
}
