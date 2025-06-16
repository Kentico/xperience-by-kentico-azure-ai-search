using DancingGoat.Search.Models;
using DancingGoat.Search.Services;
using DancingGoat.Search.Strategies;

namespace DancingGoat.Search;

public static class DancingGoatSearchStartupExtensions
{
    public static IServiceCollection AddKenticoAzureSearchServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKenticoAzureSearch(builder =>
        {
            builder.RegisterStrategy<SemanticRankingSearchStrategy, DancingGoatSearchModel>(nameof(SemanticRankingSearchStrategy));
            builder.RegisterStrategy<DancingGoatSearchStrategy, DancingGoatSearchModel>(nameof(DancingGoatSearchStrategy));
            builder.RegisterStrategy<DancingGoatSimpleSearchStrategy, DancingGoatSimpleSearchModel>(nameof(DancingGoatSimpleSearchStrategy));
            builder.RegisterStrategy<CustomItemsReindexingSearchStrategy, DancingGoatSearchModel>(nameof(CustomItemsReindexingSearchStrategy));
            builder.RegisterStrategy<ReusableContentItemsIndexingStrategy, DancingGoatSearchModel>(nameof(ReusableContentItemsIndexingStrategy));
        }, configuration);

        services.AddTransient<DancingGoatSearchService>();

        services.AddKenticoAzureSearch(configuration);

        services.AddHttpClient<WebCrawlerService>();
        services.AddSingleton<WebScraperHtmlSanitizer>();
        services.AddTransient<StrategyHelper>();

        return services;
    }
}
