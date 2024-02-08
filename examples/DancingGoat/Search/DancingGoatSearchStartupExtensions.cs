using DancingGoat.Search.Models;
using DancingGoat.Search.Services;

namespace DancingGoat.Search;

public static class DancingGoatSearchStartupExtensions
{
    public static IServiceCollection AddKenticoAzureSearchServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKenticoAzureSearch(builder =>
        {
            builder.RegisterStrategy<SimpleSearchIndexingStrategy, DancingGoatSimpleSearchModel>("Simple");
            builder.RegisterStrategy<AdvancedSearchIndexingStrategy, DancingGoatSearchModel>("Advanced");
        }, configuration);

        services.AddTransient<DancingGoatSearchService>();
        services.AddTransient<DancingGoatSimpleSearchService>();

        services.AddHttpClient<WebCrawlerService>();
        services.AddSingleton<WebScraperHtmlSanitizer>();

        return services;
    }
}
