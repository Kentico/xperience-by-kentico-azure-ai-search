namespace Kentico.Xperience.AzureSearch.Admin;

public interface IAzureSearchConfigurationStorageService
{
    bool TryCreateIndex(AzureSearchConfigurationModel configuration);

    bool TryEditIndex(AzureSearchConfigurationModel configuration);
    bool TryDeleteIndex(AzureSearchConfigurationModel configuration);
    bool TryDeleteIndex(int id);
    AzureSearchConfigurationModel? GetIndexDataOrNull(int indexId);
    List<string> GetExistingIndexNames();
    List<int> GetIndexIds();
    IEnumerable<AzureSearchConfigurationModel> GetAllIndexData();
}
