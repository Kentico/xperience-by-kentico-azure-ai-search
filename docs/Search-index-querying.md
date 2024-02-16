# Search index querying

## Rebuild the Search Index

Each index will initially be empty after creation until you create or modify some content.

To index all existing content, rebuild the index in Xperience's Administration within the Search application added by this library.

## Create a search model

```csharp
public class ExampleSearchModel : BaseAzureSearchModel
{
    public string Title { get; set; }
    public string Content { get; set; }
}
```

## Create a search service

Execute a search with custom Azure `SearchOptions` using the `IAzureSearchQueryClientService`. Specify Search options and select data which will be retrieved from the Azure Index.

```csharp
public class ExampleSearchService
{
    private readonly IAzureSearchQueryClientService searchClientService;

    public ExampleSearchService(IAzureSearchQueryClientService searchClientService) => this.searchClientService = searchClientService;

    public async Task<ExampleSearchViewModel> GlobalSearch(
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

        var response = await index.SearchAsync<ExampleSearchModel>(searchText, options);

        return new ExampleSearchViewModel()
        {
            Hits = response.Value.GetResults().Select(x => new SearchResultModel()
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
}
```

Map Retrieved SearchModel data to a more simple `SearchResultModel`

```csharp
public class SearchResultModel
{
    public string Title { get; set; } = "";
    public string Url { get; set; } = "";
}
```

You can use a `ExampleSearchViewModel` which specifies most commonly used propeties shown in an UI

```csharp
public class ExampleSearchViewModel
{
    public int TotalHits { get; set; }
    public string Query { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int Page { get; set; }
    public string IndexName { get; set; }
    public string Endpoint { get; set; }

    public IEnumerable<SearchResultModel> Hits { get; set; }
}
```

## Display Results

Create a Controller which uses `ExampleSearchService` to display view with search bar.

```csharp
[Route("[controller]")]
[ApiController]
public class SearchController : Controller
{
    private readonly ExampleSearchService searchService;

    public SearchController(ExampleSearchService searchService) => this.searchService = searchService;

    public async Task<IActionResult> Index(string? query, int? pageSize, int? page, string? indexName)
    {
        var results = await searchService.GlobalSearch(indexName ?? "advanced", query, page ?? 1, pageSize ?? 10);
        results.Endpoint = nameof(Index);

        return View(results);
    }
}
```

The controller retrieves `Index.cshtml` stored in `Views/Search/` solution folder. You can use `GetRouteData` method to assemble the parameters of the url of the endpoint defined in `ExampleSearchService`.

```cshtml
@model ExampleSearchViewModel
@{
    Dictionary<string, string> GetRouteData(int page) =>
        new Dictionary<string, string>() {
            { "query", Model.Query },
            { "pageSize", Model.PageSize.ToString() },
            { "page", page.ToString() },
            { "indexName", page.ToString() }
        };
}

<h1>Search</h1>

<style>
    .form-field {
        margin-bottom: 0.8rem;
    }
</style>

<div class="row" style="padding: 1rem;">
    <div class="col-12">
        <form asp-action=@Model.Endpoint method="get">
            <div class="row">
                <div class="col-md-12">
                    <div class="form-field">
                        <label class="control-label" asp-for="@Model.Query"></label>
                        <div class="editing-form-control-nested-control">
                            <input class="form-control" asp-for="@Model.Query" name="query">
                            <input type="hidden" asp-for="@Model.PageSize" name="pageSize" />
                            <input type="hidden" asp-for="@Model.Page" name="page" />
                            <input type="hidden" asp-for="@Model.IndexName" name="indexName" />
                        </div>
                    </div>
                </div>
            </div>

            <input type="submit" value="Submit">
        </form>
    </div>
</div>

@if (!Model.Hits.Any())
{
    if (!String.IsNullOrWhiteSpace(Model.Query))
    {
        @HtmlLocalizer["Sorry, no results match {0}", Model.Query]
    }

    return;
}

@foreach (var item in Model.Hits)
{
    <div class="col-sm-12">
        <div class="section-text">
            <h3 class="h4 search-tile-title">
                <a href="@item.Url">@item.Title</a>
            </h3>
        </div>
    </div>
}

<div class="pagination-container">
    <ul class="pagination">
        @if (Model.Page > 1)
        {
            <li class="PagedList-skipToPrevious">
                <a asp-controller="Search" asp-all-route-data="GetRouteData(Model.Page - 1)">
                    @HtmlLocalizer["previous"]
                </a>
            </li>
        }

        @for (int pageNumber = 1; pageNumber <= Model.TotalPages; pageNumber++)
        {
            if (pageNumber == Model.Page)
            {
                <li class="active">
                    <span>
                        @pageNumber
                    </span>
                </li>
            }
            else
            {
                <li>
                    <a asp-controller="Search" asp-all-route-data="GetRouteData(pageNumber)">@pageNumber</a>
                </li>
            }
        }

        @if (Model.Page < Model.TotalPages)
        {
            <li class="PagedList-skipToNext">
                <a asp-controller="Search" asp-all-route-data="GetRouteData(Model.Page + 1)">
                    @HtmlLocalizer["next"]
                </a>
            </li>
        }
    </ul>
</div>
```