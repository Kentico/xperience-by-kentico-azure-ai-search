using Azure.Search.Documents.Indexes.Models;

using DancingGoat.Models;
using DancingGoat.Search.Models;
using DancingGoat.Search.Services;

using Kentico.Xperience.AzureSearch.Indexing;

namespace DancingGoat.Search;

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
        if (item is not IndexEventWebPageItemModel indexedPage)
        {
            return null;
        }
        if (string.Equals(item.ContentTypeName, ArticlePage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
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

            if (page.HomePageBanner == null || !page.HomePageBanner.Any())
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

        return result;
    }
}
