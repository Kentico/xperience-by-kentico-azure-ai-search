using DancingGoat.Search.Services;
using Microsoft.AspNetCore.Mvc;

namespace DancingGoat.Search;

[Route("[controller]")]
[ApiController]
public class SearchController : Controller
{
    private readonly DancingGoatSearchService searchService;

    public SearchController(DancingGoatSearchService searchService) => this.searchService = searchService;

    public async Task<IActionResult> Index(string query, int pageSize = 10, int page = 1, string indexName = null)
    {
        var results = await searchService.GlobalSearch(indexName ?? "advanced", query, page, pageSize);
        results.Endpoint = nameof(Index);

        return View(results);
    }

    [HttpGet(nameof(Geo))]
    public async Task<IActionResult> Geo(string query, double latitude, double longitude, bool sortByDistance = true, int pageSize = 10, int page = 1, string indexName = null)
    {
        var results = await searchService.GeoSearch(indexName ?? "geo", query, latitude, longitude, sortByDistance, page, pageSize);
        results.Endpoint = nameof(Geo);

        return View("~/Views/Search/GeoSearch.cshtml", results);
    }

    [HttpGet(nameof(Simple))]
    public async Task<IActionResult> Simple(string query, int pageSize = 10, int page = 1, string indexName = null)
    {
        var results = await searchService.SimpleSearch(indexName ?? "simple", query, page, pageSize);
        results.Endpoint = nameof(Simple);

        return View("~/Views/Search/Index.cshtml", results);
    }

    [HttpGet(nameof(Semantic))]
    public async Task<IActionResult> Semantic(string query, int pageSize = 10, int page = 1, string indexName = null)
    {
        var results = await searchService.SemanticSearch(indexName ?? "semantic", query, page, pageSize);
        results.Endpoint = nameof(Semantic);

        return View("~/Views/Search/Index.cshtml", results);
    }
}
