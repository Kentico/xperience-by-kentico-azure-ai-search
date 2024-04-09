using DancingGoat.Models;

using Kentico.Xperience.AzureSearch.Admin;
using Kentico.Xperience.AzureSearch.Indexing;

namespace Kentico.Xperience.AzureSearch.Tests.Base;
internal static class MockDataProvider
{
    public static IndexEventWebPageItemModel WebModel(IndexEventWebPageItemModel item)
    {
        item.LanguageName = CzechLanguageName;
        item.ContentTypeName = ArticlePage.CONTENT_TYPE_NAME;
        item.Name = "Name";
        item.ContentTypeID = 1;
        item.ContentLanguageID = 1;
        item.WebsiteChannelName = DefaultChannel;
        item.WebPageItemTreePath = "/%";

        return item;
    }

    public static AzureSearchIndexIncludedPath Path => new("/%")
    {
        ContentTypes = [new AzureSearchIndexContentType(ArticlePage.CONTENT_TYPE_NAME, nameof(ArticlePage))]
    };


    public static AzureSearchIndex Index => new(
        new AzureSearchConfigurationModel()
        {
            IndexName = DefaultIndex,
            ChannelName = DefaultChannel,
            LanguageNames = new List<string>() { EnglishLanguageName, CzechLanguageName },
            Paths = new List<AzureSearchIndexIncludedPath>() { Path },
            StrategyName = "strategy"
        },
        []
    );

    public static readonly string DefaultIndex = "SimpleIndex";
    public static readonly string DefaultChannel = "DefaultChannel";
    public static readonly string EnglishLanguageName = "en";
    public static readonly string CzechLanguageName = "cz";
    public static readonly int IndexId = 1;
    public static readonly string EventName = "publish";

    public static AzureSearchIndex GetIndex(string indexName, int id) => new(
        new AzureSearchConfigurationModel()
        {
            Id = id,
            IndexName = indexName,
            ChannelName = DefaultChannel,
            LanguageNames = new List<string>() { EnglishLanguageName, CzechLanguageName },
            Paths = new List<AzureSearchIndexIncludedPath>() { Path }
        },
        []
    );
}
