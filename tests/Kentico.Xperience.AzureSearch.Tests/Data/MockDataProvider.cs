using DancingGoat.Models;

using Kentico.Xperience.AzureSearch.Admin;
using Kentico.Xperience.AzureSearch.Indexing;

namespace Kentico.Xperience.AzureSearch.Tests.Base;

internal static class MockDataProvider
{
    // String constants
    public static readonly string DEFAULT_INDEX = "SimpleIndex";
    public static readonly string DEFAULT_CHANNEL = "DefaultChannel";
    public static readonly string ENGLISH_LANGUAGE_NAME = "en";
    public static readonly string CZECH_LANGUAGE_NAME = "cz";
    public static readonly string EVENT_NAME = "publish";
    public static readonly string STRATEGY_NAME = "strategy";
    public static readonly string WEB_PAGE_ITEM_NAME = "Name";
    public static readonly string WEB_PAGE_TREE_PATH = "/%";
    public static readonly string ALIAS_NAME = "test-alias";
    public static readonly string NEW_ALIAS_NAME = "new-test-alias";
    public static readonly string SERVICE_ENDPOINT = "https://test-search.search.windows.net";
    public static readonly string QUERY_API_KEY = "test-query-api-key";
    public static readonly string ADMIN_API_KEY = "test-admin-api-key";

    // Integer constants
    public static readonly int INDEX_ID = 1;
    public static readonly int CONTENT_TYPE_ID = 1;
    public static readonly int CONTENT_LANGUAGE_ID = 1;


    public static IndexEventWebPageItemModel WebModel(IndexEventWebPageItemModel item)
    {
        item.LanguageName = CZECH_LANGUAGE_NAME;
        item.ContentTypeName = ArticlePage.CONTENT_TYPE_NAME;
        item.Name = WEB_PAGE_ITEM_NAME;
        item.ContentTypeID = CONTENT_TYPE_ID;
        item.ContentLanguageID = CONTENT_LANGUAGE_ID;
        item.WebsiteChannelName = DEFAULT_CHANNEL;
        item.WebPageItemTreePath = WEB_PAGE_TREE_PATH;

        return item;
    }


    public static AzureSearchIndexIncludedPath Path => new(WEB_PAGE_TREE_PATH)
    {
        ContentTypes = [new AzureSearchIndexContentType(ArticlePage.CONTENT_TYPE_NAME, nameof(ArticlePage))]
    };


    public static AzureSearchIndex Index => new(
        new AzureSearchConfigurationModel()
        {
            IndexName = DEFAULT_INDEX,
            ChannelName = DEFAULT_CHANNEL,
            LanguageNames = [ENGLISH_LANGUAGE_NAME, CZECH_LANGUAGE_NAME],
            Paths = [Path],
            StrategyName = STRATEGY_NAME
        },
        []
    );


    public static AzureSearchIndex GetIndex(string indexName, int id) => new(
        new AzureSearchConfigurationModel()
        {
            Id = id,
            IndexName = indexName,
            ChannelName = DEFAULT_CHANNEL,
            LanguageNames = [ENGLISH_LANGUAGE_NAME, CZECH_LANGUAGE_NAME],
            Paths = [Path]
        },
        []
    );


    // Backward compatibility properties
    public static readonly string DefaultIndex = DEFAULT_INDEX;
    public static readonly string DefaultChannel = DEFAULT_CHANNEL;
    public static readonly string EnglishLanguageName = ENGLISH_LANGUAGE_NAME;
    public static readonly string CzechLanguageName = CZECH_LANGUAGE_NAME;
    public static readonly int IndexId = INDEX_ID;
    public static readonly string EventName = EVENT_NAME;
}
