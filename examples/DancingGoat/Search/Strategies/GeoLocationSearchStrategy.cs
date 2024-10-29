using Azure.Core.GeoJson;

using DancingGoat.Models;
using DancingGoat.Search.Models;
using DancingGoat.Search.Services;

using Kentico.Xperience.AzureSearch.Indexing;

namespace DancingGoat.Search;

public class GeoLocationSearchStrategy : BaseAzureSearchIndexingStrategy<GeoLocationSearchModel>
{
    private readonly WebScraperHtmlSanitizer htmlSanitizer;
    private readonly WebCrawlerService webCrawler;
    private readonly StrategyHelper strategyHelper;

    public GeoLocationSearchStrategy(
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
        var result = new GeoLocationSearchModel();

        // IIndexEventItemModel could be a reusable content item or a web page item, so we use
        // pattern matching to get access to the web page item specific type and fields
        if (item is not IndexEventWebPageItemModel indexedPage)
        {
            return null;
        }
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

            result.Title = page.CafeTitle ?? string.Empty;
            result.Location = page.CafeLocation ?? string.Empty;

            //We can use this value later to sort by distance from the user accessing our search page.
            //Example for this scenario is shown in DancingGoatSearchService.GeoSearch
            result.GeoLocation = new GeoPoint((double)page.CafeLocationLatitude, (double)page.CafeLocationLongitude);

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
