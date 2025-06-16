using CMS.ContentEngine;
using CMS.Websites;

using DancingGoat.Models;
using DancingGoat.Search.Models;
using DancingGoat.Search.Services;

using Kentico.Xperience.AzureSearch.Indexing;

using Microsoft.IdentityModel.Tokens;

namespace DancingGoat.Search.Strategies;

public class CustomItemsReindexingSearchStrategy : BaseAzureSearchIndexingStrategy<DancingGoatSearchModel>
{
    private readonly IContentQueryExecutor queryExecutor;
    private readonly IWebPageQueryResultMapper webPageMapper;
    private readonly StrategyHelper strategyHelper;
    private readonly WebScraperHtmlSanitizer htmlSanitizer;
    private readonly WebCrawlerService webCrawler;

    public CustomItemsReindexingSearchStrategy(
        IWebPageQueryResultMapper webPageMapper,
        IContentQueryExecutor queryExecutor,
        StrategyHelper strategyHelper,
        WebScraperHtmlSanitizer htmlSanitizer,
        WebCrawlerService webCrawler
    )
    {
        this.webPageMapper = webPageMapper;
        this.queryExecutor = queryExecutor;
        this.strategyHelper = strategyHelper;
        this.htmlSanitizer = htmlSanitizer;
        this.webCrawler = webCrawler;
    }

    public override async Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventWebPageItemModel changedItem)
    {
        var reindexedItems = new List<IIndexEventItemModel>();

        if (string.Equals(changedItem.ContentTypeName, ArticlePage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
        {
            var query = new ContentItemQueryBuilder()
                .ForContentType(ArticlePage.CONTENT_TYPE_NAME,
                    config =>
                        config
                            .WithLinkedItems(4)

                            // Because the changedItem is a reusable content item, we don't have a website channel name to use here
                            // so we use a hardcoded channel name.
                            //
                            // This will be resolved with an upcoming Xperience by Kentico feature
                            // https://roadmap.kentico.com/c/193-new-api-cross-content-type-querying
                            .ForWebsite(StrategyHelper.INDEXED_WEBSITECHANNEL_NAME)

                            // Retrieves all ArticlePages that link to the Article through the ArticlePage.ArticlePageArticle field
                            .Linking(nameof(ArticlePage.ArticlePageTeaser), new[] { changedItem.ItemID }))
                .InLanguage(changedItem.LanguageName);

            var result = await queryExecutor.GetWebPageResult(query, webPageMapper.Map<ArticlePage>);

            foreach (var articlePage in result)
            {
                // This will be a IIndexEventItemModel passed to our MapToAzureSearchModelOrNull method
                reindexedItems.Add(new IndexEventWebPageItemModel(
                    articlePage.SystemFields.WebPageItemID,
                    articlePage.SystemFields.WebPageItemGUID,
                    changedItem.LanguageName,
                    ArticlePage.CONTENT_TYPE_NAME,
                    articlePage.SystemFields.WebPageItemName,
                    articlePage.SystemFields.ContentItemIsSecured,
                    articlePage.SystemFields.ContentItemContentTypeID,
                    articlePage.SystemFields.ContentItemCommonDataContentLanguageID,
                    StrategyHelper.INDEXED_WEBSITECHANNEL_NAME,
                    articlePage.SystemFields.WebPageItemTreePath,
                    articlePage.SystemFields.WebPageItemParentID,
                    articlePage.SystemFields.WebPageItemOrder));
            }
        }

        return reindexedItems;
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

        return result;
    }
}
