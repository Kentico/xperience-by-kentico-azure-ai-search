# Create a custom index strategy

The primary functionality of this library is enabled through a custom "indexing strategy" which is entirely based on your
content model and search experience. Below we will look at the steps and features available to define this indexing process.

## Implement an index strategy type

Define a custom `BaseAzureSearchIndexingStrategy<TSearchModel>>` implementation to customize how page or content items are processed for indexing.

Your custom implemention of `BaseAzureSearchIndexingStrategy<TSearchModel>>` can use dependency injection to define services and configuration used for gathering the content to be indexed. `BaseAzureSearchIndexingStrategy<TSearchModel>>` implements `IAzureSearchIndexingStrategy` and will be [registered as a transient](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#transient) in the DI container.

## Create a SearchModel

Define a custom `BaseAzureSearchModel` with attribute decorators which will be used to create an index in the Azure administration. Read more about these decorator attributes in Azure .NET sdk documentation

Use this model as a type parameter of your `BaseAzureSearchIndexingStrategy`.

## Specify a mapping process

Override the `SemanticRankingConfiguration CreateSemanticRankingConfigurationOrNull()` method to Add semantic ranking to your index. See Azure AI Search documentation for this.

Override the `Task<IAzureSearchModel?> MapToAzureSearchModelOrNull(IIndexEventItemModel item)` method and define a process for mapping custom properties of each content item event provided to your custom implementation of `BaseAzureSearchModel`. Properties defined in the `BaseAzureSearchModel` base class will be mapped automatically. Retrieve your implementation of `BaseAzureSearchModel` from `Task<IAzureSearchModel?> MapToAzureSearchModelOrNull(IIndexEventItemModel item)`.

The method is given an `IIndexEventItemModel` which is a abstraction of any item being processed for indexing, which includes both `IndexEventWebPageItemModel` for web page items and `IndexEventReusableItemModel` for reusable content items. Every item specified in the admin ui is rebuilt. In the UI you need to specify one or more language, channel name, indexingStrategy and paths with content types. This strategy than evaluates all web page items specified in the administration.

Let's say we specified `ArticlePage` in the admin ui.
Now we implement how we want to save ArticlePage page in our strategy.

The SearchModel is indexed representation of the webpageitem.

You specify what fields should be indexed in the SearchModel by specifying the type parameter `TSearchModel` of the `BaseAzureSearchIndexingStrategy`. The properties of this class are used to create columns of your index. You later retrieve data from the SearchModel based on your implementation.

```csharp
public class ExampleSearchIndexingStrategy : BaseAzureSearchIndexingStrategy<SimpleSearchModel>
{
    public override async Task<IAzureSearchModel> MapToAzureSearchModelOrNull(IIndexEventItemModel item)
    {
        var result = new SimpleSearchModel();

        // IIndexEventItemModel could be a reusable content item or a web page item, so we use
        // pattern matching to get access to the web page item specific type and fields
        if (item is IndexEventWebPageItemModel indexedPage)
        {
            if (string.Equals(item.ContentTypeName, ArticlePage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
            {
                // The implementation of GetPage<T>() is below
                var page = await GetPage<ArticlePage>(
                    indexedPage.ItemGuid,
                    indexedPage.WebsiteChannelName,
                    indexedPage.LanguageName,
                    ArticlePage.CONTENT_TYPE_NAME);

                if (page is null)
                {
                    return null;
                }

                result.Title = page?.ArticleTitle ?? "";
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

Some properties of the `IIndexEventItemModel` are added to the indexed data by default by the library and these can be found in the `BaseAzureSearchModel` class.

```csharp
public class BaseAzureSearchModel : IAzureSearchModel
{
    [SearchableField(IsSortable = true, IsFilterable = true, IsFacetable = true)]
    public string? Url { get; set; } = "";

    [SearchableField(IsFacetable = true, IsFilterable = true)]
    public string ContentTypeName { get; set; } = "";

    [SearchableField(IsSortable = true, IsFacetable = true, IsFilterable = true)]
    public string LanguageName { get; set; } = "";

    [SimpleField(IsKey = false)]
    public string ItemGuid { get; set; } = "";

    [SimpleField(IsKey = true)]
    public string ObjectID { get; set; } = "";

    [SimpleField(IsKey = false)]
    public string Name { get; set; } = "";
}
```

```csharp
public class SimpleSearchModel : BaseAzureSearchModel
{
    [SearchableField]
    public string Title { get; set; }
}
```

The `Url` field is a relative path by default. You can change this by adding this field manually in the `MapToAzureSearchModelOrNull` method.

```csharp
public override async Task<IAzureSearchModel> MapToAzureSearchModelOrNull(IIndexEventItemModel item)
{
    //...

    var result = new SimpleSearchModel();

    // retrieve an absolute URL
    if (item is IndexEventWebPageItemModel webpageItem &&
        string.Equals(indexedModel.ContentTypeName, ArticlePage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnorecase))
    {
        try
        {
            result.Url = (await urlRetriever.Retrieve(
                webpageItem.WebPageItemTreePath,
                webpageItem.WebsiteChannelName,
                webpageItem.LanguageName)).AbsolutePath;
        }
        catch (Exception)
        {
            // Retrieve can throw an exception when processing a page update AzureSearchQueueItem
            // and the page was deleted before the update task has processed. In this case, upsert an
            // empty URL
        }
    }

    //...
}
```

## Data retrieval during indexing

It is up to your implementation how do you want to retrieve the content or data to be indexed, however any web page item could be retrieved using a generic `GetPage<T>` method. In the example below, you specify that you want to retrieve `ArticlePage` item in the provided language on the channel using provided id and content type.

```csharp
public class ExampleSearchIndexingStrategy : BaseAzureSearchIndexingStrategy<SimpleSearchModel>
{
    // Other fields defined in previous examples
    // ...

    private readonly IWebPageQueryResultMapper webPageMapper;
    private readonly IContentQueryExecutor queryExecutor;

    public ExampleSearchIndexingStrategy(
        IWebPageQueryResultMapper webPageMapper,
        WebCrawlerSIContentQueryExecutorervice queryExecutor
    )
    {
        this.webPageMapper = webPageMapper;
        this.queryExecutor = queryExecutor;
    }

    public override async Task<IAzureSearchModel> MapToAzureSearchModelOrNull(IIndexEventItemModel item)
    {
        // Implementation detailed in previous examples, including GetPage<T> call
        // ...
    }

    private async Task<T?> GetPage<T>(Guid id, string channelName, string languageName, string contentTypeName)
        where T : IWebPageFieldsSource, new()
    {
        var query = new ContentItemQueryBuilder()
            .ForContentType(contentTypeName,
                config =>
                    config
                        .WithLinkedItems(4) // You could parameterize this if you want to optimize specific database queries
                        .ForWebsite(channelName)
                        .Where(where => where.WhereEquals(nameof(WebPageFields.WebPageItemGUID), id))
                        .TopN(1))
            .InLanguage(languageName);

        var result = await queryExecutor.GetWebPageResult(query, webPageMapper.Map<T>);

        return result.FirstOrDefault();
    }
}
```

## Keeping indexed related content up to date

If an indexed web page item has relationships to other web page items or reusable content items, and updates to those items should trigger
a reindex of the original web page item, you can override the `Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventWebPageItemModel changedItem)` or `Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventReusableItemModel changedItem)` methods which both return the items that should be indexed based on the incoming item being changed.

In our example an `ArticlePage` web page item has a `ArticlePageArticle` field which represents a reference to related reusable content items that contain the full article content. We include content from the reusable item in our indexed web page, so changes to the reusable item should result in the index being updated for the web page item.

All items returned from either `FindItemsToReindex` method will be passed to `public override async Task<IAzureSearchModel> MapToAzureSearchModelOrNull(IIndexEventItemModel item)` for indexing.

```csharp
public class ExampleSearchIndexingStrategy : BaseAzureSearchIndexingStrategy<SimpleSearchModel>
{
    // Other fields defined in previous examples
    // ...

    public const string INDEXED_WEBSITECHANNEL_NAME = "mywebsitechannel";

    private readonly IWebPageQueryResultMapper webPageMapper;
    private readonly IContentQueryExecutor queryExecutor;

    public ExampleSearchIndexingStrategy(
        IWebPageQueryResultMapper webPageMapper,
        IContentQueryExecutor queryExecutor,
    )
    {
        this.webPageMapper = webPageMapper;
        this.queryExecutor = queryExecutor;
    }

    public override async Task<IAzureSearchModel> MapToAzureSearchModelOrNull(IIndexEventItemModel item)
    {
        // Implementation detailed in previous examples, including GetPage<T> call
        // ...
    }

    public override async Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventReusableItemModel changedItem)
    {
        var reindexedItems = new List<IIndexEventItemModel>();

        if (string.Equals(indexedModel.ContentTypeName, Article.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnorecase))
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
                            .ForWebsite(INDEXED_WEBSITECHANNEL_NAME)

                            // Retrieves all ArticlePages that link to the Article through the ArticlePage.ArticlePageArticle field
                            .Linking(nameof(ArticlePage.ArticlePageArticle), new[] { changedItem.ItemID }))
                .InLanguage(changedItem.LanguageName);

            var result = await queryExecutor.GetWebPageResult(query, webPageMapper.Map<ArticlePage>);

            foreach (var articlePage in result)
            {
                // This will be a IIndexEventItemModel passed to our MapToAzureSearchModelOrNull method above
                reindexable.Add(new IndexEventWebPageItemModel(
                    page.SystemFields.WebPageItemID,
                    page.SystemFields.WebPageItemGUID,
                    changedItem.LanguageName,
                    ArticlePage.CONTENT_TYPE_NAME,
                    page.SystemFields.WebPageItemName,
                    page.SystemFields.ContentItemIsSecured,
                    page.SystemFields.ContentItemContentTypeID,
                    page.SystemFields.ContentItemCommonDataContentLanguageID,
                    INDEXED_WEBSITECHANNEL_NAME,
                    page.SystemFields.WebPageItemTreePath,
                    page.SystemFields.WebPageItemParentID,
                    page.SystemFields.WebPageItemOrder));
            }
        }

        return reindexedItems;
    }

    private async Task<T?> GetPage<T>(Guid id, string channelName, string languageName, string contentTypeName)
        where T : IWebPageFieldsSource, new()
    {
        // Same as examples above
        // ...
    }
}
```

Note that we are not preparing the AzureSearch `BaseAzureSearchModel` in `FindItemsToReindex`, but instead are generating a collection of
additional items that will need reindexing based on the modification of a related `IIndexEventItemModel`.

## Helper methods

We usually want to retrieve a page item in the same way - we can always use the `GetPage<T>` method. We can create a `StrategyHelper` service. This might be useful if we are creating multiple strategies which typically use the same method to retrieve pages.

```csharp
public class StrategyHelper
{
    private readonly IWebPageQueryResultMapper webPageMapper;
    private readonly IContentQueryExecutor queryExecutor;

    public const string INDEXED_WEBSITECHANNEL_NAME = "DancingGoatPages";

    public StrategyHelper(IWebPageQueryResultMapper webPageMapper, IContentQueryExecutor queryExecutor)
    {
        this.webPageMapper = webPageMapper;
        this.queryExecutor = queryExecutor;
    }

    public async Task<T?> GetPage<T>(Guid id, string channelName, string languageName, string contentTypeName)
        where T : IWebPageFieldsSource, new()
    {
        var query = new ContentItemQueryBuilder()
            .ForContentType(contentTypeName,
                config =>
                    config
                        .WithLinkedItems(4) // You could parameterize this if you want to optimize specific database queries
                        .ForWebsite(channelName)
                        .Where(where => where.WhereEquals(nameof(WebPageFields.WebPageItemGUID), id))
                        .TopN(1))
            .InLanguage(languageName);

        var result = await queryExecutor.GetWebPageResult(query, webPageMapper.Map<T>);

        return result.FirstOrDefault();
    }
}
```

We register this service as Transient in the `Program.cs`

```csharp
services.AddTransient<StrategyHelper>();
```

Now we can add it to constructor of our custom `BaseAzureSearchIndexingStrategy` implementations.

## Indexing web page content

See [Scraping web page content](Scraping-web-page-content.md)

## DI Registration

Finally, add this library to the application services, registering your custom `BaseAzureSearchIndexingStrategy` and AzureSearch

```csharp
// Program.cs

// Registers all services and uses default indexing behavior (no custom data will be indexed)
services.AddKenticoAzureSearch(configuration);

// or

// Registers all services and enables custom indexing behavior
services.AddKenticoAzureSearch(builder =>
{
    builder.RegisterStrategy<GlobalAzureSearchStrategy, GlobalSearchModel>("DefaultStrategy");
}, configuration);
```
