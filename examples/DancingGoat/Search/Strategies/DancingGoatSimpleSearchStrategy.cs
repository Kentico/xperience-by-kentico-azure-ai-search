using DancingGoat.Models;
using DancingGoat.Search.Models;
using DancingGoat.Search.Services;

using Kentico.Xperience.AzureSearch.Indexing;

namespace DancingGoat.Search;

/// <summary>
/// Simple indexing strategy that indexes web page items (<see cref="ArticlePage"/>, <see cref="HomePage"/>) with title only (no page crawling).
/// </summary>
/// <remarks>
/// This strategy indexes only web page items into <see cref="DancingGoatSimpleSearchModel"/> and does not crawl page content.
/// For <see cref="ArticlePage"/> it uses <see cref="ArticlePage.ArticleTitle"/>; for <see cref="HomePage"/> it uses the first <see cref="Banner"/> header.
/// Use this for lightweight indexes where full-text content is not required.
/// </remarks>
public class DancingGoatSimpleSearchStrategy : BaseAzureSearchIndexingStrategy<DancingGoatSimpleSearchModel>
{
    private readonly StrategyHelper strategyHelper;

    public DancingGoatSimpleSearchStrategy(StrategyHelper strategyHelper) => this.strategyHelper = strategyHelper;

    public override async Task<IAzureSearchModel> MapToAzureSearchModelOrNull(IIndexEventItemModel item)
    {
        var result = new DancingGoatSimpleSearchModel();

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
                ArticlePage.CONTENT_TYPE_NAME,
                includeSecuredItems: true);

            if (page is null)
            {
                return null;
            }

            result.Title = page.ArticleTitle ?? string.Empty;
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
        }
        else
        {
            return null;
        }

        return result;
    }
}
