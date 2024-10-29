using System.Reflection;

using Azure;
using Azure.Search.Documents.Indexes;

using Kentico.Xperience.AzureSearch.Admin;
using Kentico.Xperience.AzureSearch.Aliasing;
using Kentico.Xperience.AzureSearch.Indexing;
using Kentico.Xperience.AzureSearch.Search;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class AzureSearchStartupExtensions
{
    /// <summary>
    /// Adds Azure search services and custom module to application using the <see cref="BaseAzureSearchIndexingStrategy{BaseAzureSearchModel}"/> for all indexes
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddKenticoAzureSearch(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddAzureSearchServicesInternal(configuration);

        return serviceCollection;
    }

    /// <summary>
    /// Adds AzureSearch services and custom module to application with customized options provided by the <see cref="IAzureSearchBuilder"/>
    /// in the <paramref name="configure" /> action.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configure"></param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns></returns>
    public static IServiceCollection AddKenticoAzureSearch(this IServiceCollection serviceCollection, Action<IAzureSearchBuilder> configure, IConfiguration configuration)
    {
        serviceCollection.AddAzureSearchServicesInternal(configuration);

        var builder = new AzureSearchBuilder(serviceCollection);

        configure(builder);

        if (builder.IncludeDefaultStrategy)
        {
            serviceCollection.AddTransient<BaseAzureSearchIndexingStrategy<BaseAzureSearchModel>>();
            builder.RegisterStrategy<BaseAzureSearchIndexingStrategy<BaseAzureSearchModel>, BaseAzureSearchModel>("Default");
        }

        return serviceCollection;
    }

    private static IServiceCollection AddAzureSearchServicesInternal(this IServiceCollection services, IConfiguration configuration) =>
        services
            .Configure<AzureSearchOptions>(configuration.GetSection(AzureSearchOptions.CMS_AZURE_SEARCH_SECTION_NAME))
            .AddSingleton<AzureSearchModuleInstaller>()
            .AddSingleton(x =>
            {
                var options = x.GetRequiredService<IOptions<AzureSearchOptions>>();
                return new SearchIndexClient(new Uri(options.Value.SearchServiceEndPoint), new AzureKeyCredential(options.Value.SearchServiceAdminApiKey));
            })
            .AddSingleton<IAzureSearchQueryClientService>(x =>
            {
                var options = x.GetRequiredService<IOptions<AzureSearchOptions>>();
                return new AzureSearchQueryClientService(new AzureSearchQueryClientOptions(options.Value.SearchServiceEndPoint, options.Value.SearchServiceQueryApiKey));
            })
            .AddSingleton<IAzureSearchClient, DefaultAzureSearchClient>()
            .AddSingleton<IAzureSearchTaskLogger, DefaultAzureSearchTaskLogger>()
            .AddSingleton<IAzureSearchTaskProcessor, DefaultAzureSearchTaskProcessor>()
            .AddSingleton<IAzureSearchConfigurationStorageService, DefaultAzureSearchConfigurationStorageService>()
            .AddSingleton<IAzureSearchIndexClientService, AzureSearchIndexClientService>()
            .AddSingleton<IAzureSearchIndexAliasService, AzureSearchIndexAliasService>();
}

public interface IAzureSearchBuilder
{
    /// <summary>
    /// Registers the given <typeparamref name="TStrategy" /> as a transient service under <paramref name="strategyName" />
    /// </summary>
    /// <typeparam name="TStrategy">The custom type of <see cref="IAzureSearchIndexingStrategy"/> </typeparam>
    /// <typeparam name="TSearchModel">The custom rype of <see cref="IAzureSearchModel"/> used to create and use an index.</typeparam>
    /// <param name="strategyName">Used internally <typeparamref name="TStrategy" /> to enable dynamic assignment of strategies to search indexes. Names must be unique.</param>
    /// <exception cref="ArgumentException">
    ///     Thrown if an strategy has already been registered with the given <paramref name="strategyName"/>
    /// </exception>
    /// <returns></returns>
    IAzureSearchBuilder RegisterStrategy<TStrategy, TSearchModel>(string strategyName) where TStrategy : BaseAzureSearchIndexingStrategy<TSearchModel> where TSearchModel : IAzureSearchModel, new();
}

internal class AzureSearchBuilder : IAzureSearchBuilder
{
    private readonly IServiceCollection serviceCollection;
    private const string ErrorMessage = "Exactly one field in your index must serve as the document key (IsKey = true). It must be a string, and it must uniquely identify each document. It's also required to have IsHidden = false.";

    /// <summary>
    /// If true, the <see cref="BaseAzureSearchIndexingStrategy{BaseAzureSearchModel}" /> will be available as an explicitly selectable indexing strategy
    /// within the Admin UI. Defaults to <c>true</c>
    /// </summary>
    public bool IncludeDefaultStrategy { get; set; } = true;

    public AzureSearchBuilder(IServiceCollection serviceCollection) => this.serviceCollection = serviceCollection;

    /// <summary>
    /// Registers the <see cref="IAzureSearchIndexingStrategy"/> strategy <typeparamref name="TStrategy" /> in DI and
    /// as a selectable strategy in the Admin UI
    /// </summary>
    /// <typeparam name="TStrategy"></typeparam>
    /// <typeparam name="TSearchModel">The custom rype of <see cref="IAzureSearchModel"/> used to create and use an index.</typeparam>
    /// <param name="strategyName"></param>
    /// <returns></returns>
    public IAzureSearchBuilder RegisterStrategy<TStrategy, TSearchModel>(string strategyName) where TStrategy : BaseAzureSearchIndexingStrategy<TSearchModel> where TSearchModel : IAzureSearchModel, new()
    {
        ValidateIndexSearchModelProperties<TSearchModel>();

        StrategyStorage.AddStrategy<TStrategy>(strategyName);
        serviceCollection.AddTransient<TStrategy>();

        return this;
    }

    private void ValidateIndexSearchModelProperties<TSearchModel>() where TSearchModel : IAzureSearchModel, new()
    {
        var type = typeof(TSearchModel);

        var propertiesWithAttributes = type.GetProperties().Select(x => new
        {
            Attribute = x.GetCustomAttributes<SimpleFieldAttribute>().SingleOrDefault()
                ?? throw new InvalidOperationException(ErrorMessage),
            Type = x.PropertyType
        });

        var keyAttribute = propertiesWithAttributes.SingleOrDefault(x => x.Attribute.IsKey)
            ?? throw new InvalidOperationException(ErrorMessage);

        if (keyAttribute.Type != typeof(string) || keyAttribute.Attribute.IsHidden)
        {
            throw new InvalidOperationException(ErrorMessage);
        }
    }
}
