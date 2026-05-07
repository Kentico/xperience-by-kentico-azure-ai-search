using CMS.ContentEngine;
using CMS.Websites;

namespace DancingGoat.Search.Services;

public class StrategyHelper
{
    private readonly IContentQueryModelTypeMapper queryTypeMapper;
    private readonly IContentQueryExecutor queryExecutor;

    public const string INDEXED_WEBSITECHANNEL_NAME = "DancingGoatPages";

    public StrategyHelper(IContentQueryModelTypeMapper queryTypeMapper, IContentQueryExecutor queryExecutor)
    {
        this.queryTypeMapper = queryTypeMapper;
        this.queryExecutor = queryExecutor;
    }

    public async Task<T?> GetPage<T>(Guid id, string channelName, string languageName, string contentTypeName, bool includeSecuredItems = false)
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

        var result = await queryExecutor.GetWebPageResult(
            query,
            queryTypeMapper.Map<T>,
            new ContentQueryExecutionOptions { IncludeSecuredItems = includeSecuredItems });

        return result.FirstOrDefault();
    }
}
