using DancingGoat.Models;
using Kentico.Xperience.AzureSearch.Admin;
using Kentico.Xperience.AzureSearch.Indexing;

namespace Kentico.Xperience.AzureSearch.Tests.Base;
internal static class MockDataProvider
{
    public static IndexEventWebPageItemModel WebModel => new(
        itemID: 0,
        itemGuid: Guid.NewGuid(),
        languageName: CzechLanguageName,
        contentTypeName: ArticlePage.CONTENT_TYPE_NAME,
        name: "Name",
        isSecured: false,
        contentTypeID: 1,
        contentLanguageID: 1,
        websiteChannelName: DefaultChannel,
        webPageItemTreePath: "/",
        order: 0
    );

    public static AzureSearchIndexIncludedPath Path => new("/%")
    {
        ContentTypes = [ArticlePage.CONTENT_TYPE_NAME]
    };


    public static AzureSearchIndex Index => new(
        new AzureSearchConfigurationModel()
        {
            IndexName = DefaultIndex,
            ChannelName = DefaultChannel,
            LanguageNames = new List<string>() { EnglishLanguageName, CzechLanguageName },
            Paths = new List<AzureSearchIndexIncludedPath>() { Path }
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
