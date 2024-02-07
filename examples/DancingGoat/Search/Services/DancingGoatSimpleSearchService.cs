using Azure.Search.Documents;
using DancingGoat.Search.Models;
using Kentico.Xperience.AzureSearch.Search;

namespace DancingGoat.Search.Services;

public class DancingGoatSimpleSearchService
{
    private readonly AzureSearchQueryClientService searchClientService;

    public DancingGoatSimpleSearchService(AzureSearchQueryClientService searchClientService) => this.searchClientService = searchClientService;

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
            TotalPages = (int)response.Value.TotalCount <= 0 ? 0 : (((int)response.Value.TotalCount - 1) / pageSize) + 1,
            PageSize = pageSize,
            Page = page
        };
    }
}
