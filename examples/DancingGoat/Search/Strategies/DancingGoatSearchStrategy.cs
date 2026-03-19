using CMS.ContentEngine;

using DancingGoat.Models;
using DancingGoat.Search.Models;
using DancingGoat.Search.Services;

using Kentico.Xperience.AzureSearch.Indexing;

namespace DancingGoat.Search;

/// <summary>
/// Indexing strategy that indexes web page items (<see cref="ArticlePage"/>, <see cref="HomePage"/>) into the search index.
/// </summary>
/// <remarks>
/// This strategy indexes only web page items; <see cref="BaseAzureSearchIndexingStrategy{TSearchModel}.MapToAzureSearchModelOrNull"/> returns null for reusable content.
/// For each page it crawls the page URL, sanitizes the HTML, and builds a <see cref="DancingGoatSearchModel"/> with title and content.
/// ArticlePage uses <see cref="ArticlePage.ArticleTitle"/> and crawled content; HomePage uses the first <see cref="Banner"/> header and crawled content.
/// Uses the base implementation of <c>FindItemsToReindex</c> (reindex only the changed page) and no Semantic Ranking.
/// </remarks>
public class DancingGoatSearchStrategy : BaseAzureSearchIndexingStrategy<DancingGoatSearchModel>
{
    private readonly WebScraperHtmlSanitizer htmlSanitizer;
    private readonly WebCrawlerService webCrawler;
    private readonly StrategyHelper strategyHelper;

    public DancingGoatSearchStrategy(
        WebScraperHtmlSanitizer htmlSanitizer,
        WebCrawlerService webCrawler,
        StrategyHelper strategyHelper
    )
    {
        this.htmlSanitizer = htmlSanitizer;
        this.webCrawler = webCrawler;
        this.strategyHelper = strategyHelper;
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

            result.Title = page.ArticleTitle ?? string.Empty;
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
