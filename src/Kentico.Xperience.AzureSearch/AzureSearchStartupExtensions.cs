using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;

using Kentico.Xperience.AzureSearch.Admin;
using Kentico.Xperience.AzureSearch.Aliasing;
using Kentico.Xperience.AzureSearch.Indexing;
using Kentico.Xperience.AzureSearch.Search;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering Azure Search services and custom modules in an application.
/// </summary>
/// <remarks>This class contains methods to configure Azure Search services using either default strategies or
/// custom options. These methods extend the <see cref="IServiceCollection"/> to simplify the integration of Azure
/// Search functionality into an application.</remarks>
public static class AzureSearchStartupExtensions
{
    /// <summary>
    /// Adds Azure search services and custom module to application using the <see cref="BaseAzureSearchIndexingStrategy{BaseAzureSearchModel}"/> for all indexes.
    /// </summary>
    /// <param name="serviceCollection">Service collection to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>Collection of services with Azure Search services added.</returns>
    public static IServiceCollection AddKenticoAzureSearch(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddAzureSearchServicesInternal(configuration);

        return serviceCollection;
    }


    /// <summary>
    /// Adds AzureSearch services and custom module to application with customized options provided by the <see cref="IAzureSearchBuilder"/>
    /// in the <paramref name="configure" /> action.
    /// </summary>
    /// <param name="serviceCollection">Service collection to add services to.</param>
    /// <param name="configure">Configuration action for the Azure Search builder.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns></returns>
    public static IServiceCollection AddKenticoAzureSearch(this IServiceCollection serviceCollection, Action<IAzureSearchBuilder> configure, IConfiguration configuration)
    {
        var builder = new AzureSearchBuilder(serviceCollection);
        configure(builder);

        serviceCollection.AddAzureSearchServicesInternal(configuration, builder);

        if (builder.IncludeDefaultStrategy)
        {
            serviceCollection.AddTransient<BaseAzureSearchIndexingStrategy<BaseAzureSearchModel>>();
            builder.RegisterStrategy<BaseAzureSearchIndexingStrategy<BaseAzureSearchModel>, BaseAzureSearchModel>("Default");
        }

        return serviceCollection;
    }


    private static IServiceCollection AddAzureSearchServicesInternal(this IServiceCollection services, IConfiguration configuration, AzureSearchBuilder? builder = null) =>
        services
            .Configure<AzureSearchOptions>(configuration.GetSection(AzureSearchOptions.CMS_AZURE_SEARCH_SECTION_NAME))
            .AddSingleton<AzureSearchModuleInstaller>()
            .AddSingleton(x =>
            {
                var options = x.GetRequiredService<IOptions<AzureSearchOptions>>();

                var clientOptions = new SearchClientOptions();
                builder?.ConfigureClientOptions(clientOptions);

                return new SearchIndexClient(new Uri(options.Value.SearchServiceEndPoint), new AzureKeyCredential(options.Value.SearchServiceAdminApiKey), clientOptions);
            })
            .AddSingleton<IAzureSearchQueryClientService>(x =>
            {
                var options = x.GetRequiredService<IOptions<AzureSearchOptions>>();

                var clientOptions = new SearchClientOptions();
                builder?.ConfigureClientOptions(clientOptions);

                return new AzureSearchQueryClientService(new AzureSearchQueryClientOptions(options.Value.SearchServiceEndPoint, options.Value.SearchServiceQueryApiKey), clientOptions);
            })
            .AddSingleton<IAzureSearchClient, DefaultAzureSearchClient>()
            .AddSingleton<IAzureSearchTaskLogger, DefaultAzureSearchTaskLogger>()
            .AddSingleton<IAzureSearchTaskProcessor, DefaultAzureSearchTaskProcessor>()
            .AddSingleton<IAzureSearchConfigurationStorageService, DefaultAzureSearchConfigurationStorageService>()
            .AddSingleton<IAzureSearchIndexClientService, AzureSearchIndexClientService>()
            .AddSingleton<IAzureSearchIndexAliasService, AzureSearchIndexAliasService>();
}
