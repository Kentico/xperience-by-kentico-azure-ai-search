﻿using DancingGoat.Search.Services;

using Microsoft.AspNetCore.Mvc;

namespace DancingGoat.Search;

[Route("[controller]")]
[ApiController]
public class SearchController : Controller
{
    private readonly DancingGoatSearchService searchService;

    public SearchController(DancingGoatSearchService searchService) => this.searchService = searchService;

    public async Task<IActionResult> Index(string? query, int? pageSize, int? page, string? indexName)
    {
        var results = await searchService.GlobalSearch(indexName ?? "advanced", query, page ?? 1, pageSize ?? 10);
        results.Endpoint = nameof(Index);

        return View(results);
    }

    [HttpGet(nameof(Simple))]
    public async Task<IActionResult> Simple(string? query, int? pageSize, int? page, string? indexName)
    {
        var results = await searchService.SimpleSearch(indexName ?? "simple", query ?? "", page ?? 1, pageSize ?? 10);
        results.Endpoint = nameof(Simple);

        return View("~/Views/Search/Index.cshtml", results);
    }

    [HttpGet(nameof(Semantic))]
    public async Task<IActionResult> Semantic(string? query, int? pageSize, int? page, string? indexName)
    {
        var results = await searchService.SemanticSearch(indexName ?? "semantic", query ?? "", page ?? 1, pageSize ?? 10);
        results.Endpoint = nameof(Semantic);

        return View("~/Views/Search/Index.cshtml", results);
    }
}
