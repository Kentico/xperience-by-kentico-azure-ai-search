namespace Kentico.Xperience.AzureSearch.Indexing;

public static class AzureSearchIndexingEvents
{
    public static BeforeCreatingOrUpdatingIndex BeforeCreatingOrUpdatingIndex = new();
}

public class BeforeCreatingOrUpdatingIndex
{
    public EventHandler<OnBeforeCreatingOrUpdatingIndexEventArgs>? Execute;
}
