# Use OData geo-spatial functions in Azure AI Search

In Azure AI Search, semantic ranking is query-side functionality that uses natural language understanding from Microsoft to rescore search results, promoting results that have more semantic relevance to the top of the list. Depending on the content and the query, semantic ranking can significantly improve search relevance, with minimal work for the developer.

## Implement Semantic ranking in your strategy

Create a custom `BaseAzureSearchIndexingStrategy` which implements `CreateSemanticRankingConfigurationOrNull`. This method retrieves a `SemanticRankingConfiguration` which is used to configure semantic ranking in Azure Portal.

You can use the same SearchModel which you use in any strategy which does not implement Semantic Ranking. This method only extends the index. 

```csharp
public class SemanticRankingSearchStrategy : BaseAzureSearchIndexingStrategy<DancingGoatSearchModel>
{
    private readonly WebScraperHtmlSanitizer htmlSanitizer;
    private readonly WebCrawlerService webCrawler;
    private readonly StrategyHelper strategyHelper;

    public const string DANCING_GOAT_SUGGESTER_NAME = "dancing-goat-suggester";
    public const string DANCING_GOAT_SEMANTIC_SEARCH_CONFIGURATION_NAME = "dancing-goat-semantic-configuration";

    public SemanticRankingSearchStrategy(
        WebScraperHtmlSanitizer htmlSanitizer,
        WebCrawlerService webCrawler,
        StrategyHelper strategyHelper
    )
    {
        this.strategyHelper = strategyHelper;
        this.htmlSanitizer = htmlSanitizer;
        this.webCrawler = webCrawler;
    }

    public override SemanticRankingConfiguration CreateSemanticRankingConfigurationOrNull()
    {
        var semanticSearch = new SemanticSearch
        {
            Configurations =
            {
                new SemanticConfiguration(DANCING_GOAT_SEMANTIC_SEARCH_CONFIGURATION_NAME, new()
                {
                    TitleField = new SemanticField(nameof(DancingGoatSearchModel.Title)),
                    ContentFields =
                    {
                        new SemanticField(nameof(DancingGoatSearchModel.Content))
                    }
                })
            }
        };

        var suggester = new SearchSuggester(DANCING_GOAT_SUGGESTER_NAME, nameof(DancingGoatSearchModel.Content), nameof(DancingGoatSearchModel.Title));

        var semanticRankingConfiguration = new SemanticRankingConfiguration(semanticSearch);

        semanticRankingConfiguration.Suggesters.Add(suggester);

        return semanticRankingConfiguration;
    }

    public override async Task<IAzureSearchModel> MapToAzureSearchModelOrNull(IIndexEventItemModel item)
    {
        var result = new DancingGoatSearchModel();

        // IIndexEventItemModel could be a reusable content item or a web page item, so we use
        // pattern matching to get access to the web page item specific type and fields
        if (item is IndexEventWebPageItemModel indexedPage)
        {
            if (string.Equals(item.ContentTypeName, CafePage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
            {
                // The implementation of GetPage<T>() is below
                var page = await strategyHelper.GetPage<CafePage>(
                    indexedPage.ItemGuid,
                    indexedPage.WebsiteChannelName,
                    indexedPage.LanguageName,
                    CafePage.CONTENT_TYPE_NAME);

                if (page is null)
                {
                    return null;
                }

                result.Title = page?.CafeTitle ?? "";
                string rawContent = await webCrawler.CrawlWebPage(page!);
                result.Content = htmlSanitizer.SanitizeHtmlDocument(rawContent);
            }
            else if (string.Equals(item.ContentTypeName, ArticlePage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
            {
                // The implementation of GetPage<T>() is below
                var page = await strategyHelper.GetPage<ArticlePage>(
                    indexedPage.ItemGuid,
                    indexedPage.WebsiteChannelName,
                    indexedPage.LanguageName,
                    ArticlePage.CONTENT_TYPE_NAME);

                if (page is null)
                {
                    return null;
                }

                result.Title = page?.ArticleTitle ?? "";
                string rawContent = await webCrawler.CrawlWebPage(page!);
                result.Content = htmlSanitizer.SanitizeHtmlDocument(rawContent);
            }
            else if (string.Equals(item.ContentTypeName, HomePage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
            {
                var page = await strategyHelper.GetPage<HomePage>(
                    indexedPage.ItemGuid,
                    indexedPage.WebsiteChannelName,
                    indexedPage.LanguageName,
                    HomePage.CONTENT_TYPE_NAME);

                if (page is null)
                {
                    return null;
                }

                if (page.HomePageBanner.IsNullOrEmpty())
                {
                    return null;
                }

                result.Title = page!.HomePageBanner.First().BannerHeaderText;
                string rawContent = await webCrawler.CrawlWebPage(page!);
                result.Content = htmlSanitizer.SanitizeHtmlDocument(rawContent);
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }

        return result;
    }
}
```

We retrieve data from the index by specifying `SemanticSearch` with `SemanticConfigurationName` in a search service.

```csharp
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
```