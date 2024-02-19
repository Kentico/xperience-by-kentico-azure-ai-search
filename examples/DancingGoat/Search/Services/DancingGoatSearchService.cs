using Azure.Search.Documents;
using DancingGoat.Search.Models;
using Kentico.Xperience.AzureSearch.Search;

namespace DancingGoat.Search.Services;

public class DancingGoatSearchService
{
    private readonly IAzureSearchQueryClientService searchClientService;

    public DancingGoatSearchService(IAzureSearchQueryClientService searchClientService) => this.searchClientService = searchClientService;

    public async Task<DancingGoatSearchViewModel> GlobalSearch(
        string indexName,
        string searchText,
        int page = 1,
        int pageSize = 10)
    {
        var index = searchClientService.CreateSearchClientForQueries(indexName);

        page = Math.Max(page, 1);
        pageSize = Math.Max(1, pageSize);

        var options = new SearchOptions()
        {
            IncludeTotalCount = true,
            Size = pageSize,
            Skip = (page - 1) * pageSize
        };
        options.Select.Add(nameof(DancingGoatSearchModel.Title));
        options.Select.Add(nameof(DancingGoatSearchModel.Url));

        var response = await index.SearchAsync<DancingGoatSearchModel>(searchText, options);

        return new DancingGoatSearchViewModel()
        {
            Hits = response.Value.GetResults().Select(x => new DancingGoatSearchResult()
            {
                Title = x.Document.Title,
                Url = x.Document.Url,
            }),
            TotalHits = (int)response.Value.TotalCount,
            Query = searchText,
            TotalPages = (int)response.Value.TotalCount <= 0 ? 0 : ((int)response.Value.TotalCount - 1) / pageSize + 1,
            PageSize = pageSize,
            Page = page
        };
    }

    public async Task<DancingGoatSearchViewModel> SemanticSearch(
        string indexName,
        string searchText,
        int page = 1,
        int pageSize = 10)
    {
        var index = searchClientService.CreateSearchClientForQueries(indexName);

        page = Math.Max(page, 1);
        pageSize = Math.Max(1, pageSize);

        var options = new SearchOptions()
        {
            SemanticSearch = new()
            {
                SemanticConfigurationName = SemanticRankingSearchStrategy.DANCING_GOAT_SEMANTIC_SEARCH_CONFIGURATION_NAME
            },
            IncludeTotalCount = true,
            Size = pageSize,
            Skip = (page - 1) * pageSize
        };
        options.Select.Add(nameof(DancingGoatSearchModel.Title));
        options.Select.Add(nameof(DancingGoatSearchModel.Url));

        var response = await index.SearchAsync<DancingGoatSearchModel>(searchText, options);

        return new DancingGoatSearchViewModel()
        {
            Hits = response.Value.GetResults().Select(x => new DancingGoatSearchResult()
            {
                Title = x.Document.Title,
                Url = x.Document.Url,
            }),
            TotalHits = (int)response.Value.TotalCount,
            Query = searchText,
            TotalPages = (int)response.Value.TotalCount <= 0 ? 0 : ((int)response.Value.TotalCount - 1) / pageSize + 1,
            PageSize = pageSize,
            Page = page
        };
    }

    public async Task<DancingGoatSearchViewModel> SimpleSearch(
        string indexName,
        string searchText,
        int page = 1,
        int pageSize = 10)
    {
        var index = searchClientService.CreateSearchClientForQueries(indexName);

        page = Math.Max(page, 1);
        pageSize = Math.Max(1, pageSize);

        var options = new SearchOptions()
        {
            IncludeTotalCount = true,
            Size = pageSize,
            Skip = (page - 1) * pageSize,
        };
        options.Select.Add(nameof(DancingGoatSimpleSearchModel.Title));
        options.Select.Add(nameof(DancingGoatSimpleSearchModel.Url));

        var response = await index.SearchAsync<DancingGoatSimpleSearchModel>(searchText, options);

        return new DancingGoatSearchViewModel()
        {
            Hits = response.Value.GetResults().Select(x => new DancingGoatSearchResult()
            {
                Title = x.Document.Title,
                Url = x.Document.Url,
            }),
            TotalHits = (int)response.Value.TotalCount,
            Query = searchText,
            TotalPages = (int)response.Value.TotalCount <= 0 ? 0 : ((int)response.Value.TotalCount - 1) / pageSize + 1,
            PageSize = pageSize,
            Page = page
        };
    }

    public async Task<GeoLocationSearchViewModel> GeoSearch(
        string indexName,
        string searchText,
        double latitude,
        double longitude,
        bool sortByDistance = true,
        int page = 1,
        int pageSize = 10
        )
    {
        var index = searchClientService.CreateSearchClientForQueries(indexName);

        page = Math.Max(page, 1);
        pageSize = Math.Max(1, pageSize);

        var options = new SearchOptions
        {
            IncludeTotalCount = true,
            Size = pageSize,
            Skip = (page - 1) * pageSize
        };

        if (sortByDistance)
        {
            options.OrderBy.Add($"geo.distance({nameof(GeoLocationSearchModel.GeoLocation)}, geography'POINT({longitude} {latitude})') asc");
        }

        options.Select.Add(nameof(GeoLocationSearchModel.Title));
        options.Select.Add(nameof(GeoLocationSearchModel.Url));
        options.Select.Add(nameof(GeoLocationSearchModel.Location));

        var response = await index.SearchAsync<GeoLocationSearchModel>(searchText, options);

        return new GeoLocationSearchViewModel
        {
            Hits = response.Value.GetResults().Select(x => new GeoLocationSearchResult()
            {
                Title = x.Document.Title,
                Url = x.Document.Url,
                Location = x.Document.Location,
            }),
            TotalHits = (int)response.Value.TotalCount,
            Query = searchText,
            TotalPages = (int)response.Value.TotalCount <= 0 ? 0 : ((int)response.Value.TotalCount - 1) / pageSize + 1,
            PageSize = pageSize,
            Page = page
        };
    }
}
