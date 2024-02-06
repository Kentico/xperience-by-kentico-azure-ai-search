using Kentico.Xperience.AzureSearch.Indexing;

namespace Kentico.Xperience.AzureSearch.Search;

internal class DefaultAzureSearchSearchService : IAzureSearchSearchService
{
    private readonly IAzureSearchIndexClientService indexService;
    private readonly IServiceProvider serviceProvider;

    public DefaultAzureSearchSearchService(IAzureSearchIndexClientService indexService, IServiceProvider serviceProvider)
    {
        this.indexService = indexService;
        this.serviceProvider = serviceProvider;
    }

    //TODO
}
