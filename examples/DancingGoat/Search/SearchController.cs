using DancingGoat.Search.Services;
using Microsoft.AspNetCore.Mvc;

namespace DancingGoat.Search;

[Route("[controller]")]
[ApiController]
public class SearchController : Controller
{
    private readonly DancingGoatSimpleSearchService simpleSearchService;
    private readonly DancingGoatSearchService advancedSearchService;

    public SearchController(DancingGoatSimpleSearchService simpleSearchService, DancingGoatSearchService advancedSearchService)
    {
        this.simpleSearchService = simpleSearchService;
        this.advancedSearchService = advancedSearchService;
    }

    public async Task<IActionResult> Index(string query, int pageSize = 10, int page = 1, string indexName = null)
    {
        var results = await advancedSearchService.GlobalSearch(indexName ?? "advanced", query, page, pageSize);
        return View(results);
    }

    [HttpGet("Simple")]
    public async Task<IActionResult> Simple(string query, int pageSize = 10, int page = 1)
    {
        var results = await simpleSearchService.GlobalSearch("simple", query, page, pageSize);

        return View(results);
    }
}
