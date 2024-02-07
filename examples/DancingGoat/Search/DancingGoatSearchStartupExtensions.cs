using DancingGoat.Search.Services;

namespace DancingGoat.Search;

public static class DancingGoatSearchStartupExtensions
{
    public static IServiceCollection AddKenticoAzureSearchServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKenticoAzureSearch(builder =>
        {
            builder.RegisterStrategy<SimpleSearchIndexingStrategy>("Simple");
            builder.RegisterStrategy<AdvancedSearchIndexingStrategy>("Advnaced");
        }, configuration);

        services.AddHttpClient<WebCrawlerService>();
        services.AddSingleton<WebScraperHtmlSanitizer>();

        return services;
    }
}
