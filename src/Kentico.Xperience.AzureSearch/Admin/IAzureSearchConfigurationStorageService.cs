namespace Kentico.Xperience.AzureSearch.Admin;

public interface IAzureSearchConfigurationStorageService
{
    bool TryCreateIndex(AzureSearchConfigurationModel configuration);
    bool TryCreateAlias(AzureSearchAliasConfigurationModel configuration);

    bool TryEditIndex(AzureSearchConfigurationModel configuration);
    bool TryEditAlias(AzureSearchAliasConfigurationModel configuration);
    bool TryDeleteIndex(AzureSearchConfigurationModel configuration);
    bool TryDeleteIndex(int id);
    bool TryDeleteAlias(int id);
    AzureSearchConfigurationModel? GetIndexDataOrNull(int indexId);
    AzureSearchAliasConfigurationModel? GetAliasDataOrNull(int aliasId);
    List<string> GetExistingIndexNames();
    List<string> GetExistingAliasNames();
    List<int> GetIndexIds();
    List<int> GetAliasIds();
    IEnumerable<AzureSearchConfigurationModel> GetAllIndexData();
    IEnumerable<AzureSearchAliasConfigurationModel> GetAllAliasData();
}
