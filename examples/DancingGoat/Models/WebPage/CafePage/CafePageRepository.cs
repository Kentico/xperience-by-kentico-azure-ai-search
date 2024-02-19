using CMS.ContentEngine;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Routing;


namespace DancingGoat.Models
{
    /// <summary>
    /// Represents a collection of cafe pages.
    /// </summary>
    public class CafePageRepository : ContentRepositoryBase
    {
        private readonly IWebPageLinkedItemsDependencyAsyncRetriever webPageLinkedItemsDependencyRetriever;


        /// <summary>
        /// Initializes new instance of <see cref="CafePageRepository"/>.
        /// </summary>
        public CafePageRepository(
            IWebsiteChannelContext websiteChannelContext,
            IContentQueryExecutor executor,
            IWebPageQueryResultMapper mapper,
            IProgressiveCache cache,
            IWebPageLinkedItemsDependencyAsyncRetriever webPageLinkedItemsDependencyRetriever)
            : base(websiteChannelContext, executor, mapper, cache) =>
                this.webPageLinkedItemsDependencyRetriever = webPageLinkedItemsDependencyRetriever;


        /// <summary>
        /// Returns <see cref="CafePage"/> content item.
        /// </summary>
        /// <param name="webPageItemId">Web page item ID.</param>
        /// <param name="languageName">Language name.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task<CafePage> GetCafePage(int webPageItemId, string languageName, CancellationToken cancellationToken = default)
        {
            var queryBuilder = GetQueryBuilder(webPageItemId, languageName);

            var cacheSettings = new CacheSettings(5, WebsiteChannelContext.WebsiteChannelName, nameof(CafePage), webPageItemId, languageName);

            var result = await GetCachedQueryResult<CafePage>(queryBuilder, null, cacheSettings, GetDependencyCacheKeys, cancellationToken);

            return result.FirstOrDefault();
        }

        private ContentItemQueryBuilder GetQueryBuilder(int webPageItemId, string languageName) =>
            new ContentItemQueryBuilder()
                .ForContentType(CafePage.CONTENT_TYPE_NAME, config => config
                    .ForWebsite(WebsiteChannelContext.WebsiteChannelName, includeUrlPath: false)
                    .WithLinkedItems(1)
                    .Where(where => where
                        .WhereEquals(nameof(IWebPageContentQueryDataContainer.WebPageItemID), webPageItemId))
                    .TopN(1))
                .InLanguage(languageName);

        private async Task<ISet<string>> GetDependencyCacheKeys(IEnumerable<CafePage> cafePages, CancellationToken cancellationToken)
        {
            var dependencyCacheKeys = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var cafePage in cafePages)
            {
                dependencyCacheKeys.UnionWith(GetDependencyCacheKeys(cafePage));
            }

            dependencyCacheKeys.UnionWith(await webPageLinkedItemsDependencyRetriever.Get(cafePages.Select(cafePage => cafePage.SystemFields.WebPageItemID), 1, cancellationToken));
            dependencyCacheKeys.Add(CacheHelper.GetCacheItemName(null, WebsiteChannelInfo.OBJECT_TYPE, "byid", WebsiteChannelContext.WebsiteChannelID));

            return dependencyCacheKeys;
        }


        private IEnumerable<string> GetDependencyCacheKeys(CafePage cafePage)
        {
            if (cafePage == null)
            {
                return Enumerable.Empty<string>();
            }

            return new List<string>()
            {
                CacheHelper.BuildCacheItemName(new[] { "webpageitem", "byid", cafePage.SystemFields.WebPageItemID.ToString() }, false),
                CacheHelper.BuildCacheItemName(new[] { "webpageitem", "bychannel", WebsiteChannelContext.WebsiteChannelName, "bypath", cafePage.SystemFields.WebPageItemTreePath }, false),
                CacheHelper.BuildCacheItemName(new[] { "webpageitem", "bychannel", WebsiteChannelContext.WebsiteChannelName, "childrenofpath", DataHelper.GetParentPath(cafePage.SystemFields.WebPageItemTreePath) }, false),
                CacheHelper.GetCacheItemName(null, ContentLanguageInfo.OBJECT_TYPE, "all")
            };
        }
    }
}
