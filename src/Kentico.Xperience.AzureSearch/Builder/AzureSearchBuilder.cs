using System.Reflection;

using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;

using Kentico.Xperience.AzureSearch.Indexing;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Builder for configuring and registering Azure Search strategies and models.
/// </summary>
internal class AzureSearchBuilder : IAzureSearchBuilder
{
    private readonly IServiceCollection serviceCollection;

    private Action<SearchClientOptions>? configureAction;


    private const string ErrorMessage = "Exactly one field in your index must serve as the document key (IsKey = true). It must be a string, and it must uniquely identify each document. It's also required to have IsHidden = false.";

    /// <summary>
    /// If true, the <see cref="BaseAzureSearchIndexingStrategy{BaseAzureSearchModel}" /> will be available as an explicitly selectable indexing strategy
    /// within the Admin UI.
    /// </summary>
    public bool IncludeDefaultStrategy { get; }


    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchBuilder"/> class.
    /// </summary>
    public AzureSearchBuilder(IServiceCollection serviceCollection, bool includeDefaultStrategy = true)
    {
        this.serviceCollection = serviceCollection;
        IncludeDefaultStrategy = includeDefaultStrategy;
    }


    /// <summary>
    /// Registers the <see cref="IAzureSearchIndexingStrategy"/> strategy <typeparamref name="TStrategy" /> in DI and
    /// as a selectable strategy in the Admin UI.
    /// </summary>
    /// <typeparam name="TStrategy">The custom type of <see cref="IAzureSearchIndexingStrategy"/>.</typeparam>
    /// <typeparam name="TSearchModel">The custom type of <see cref="IAzureSearchModel"/> used to create and use an index.</typeparam>
    /// <param name="strategyName">Strategy name used internally to enable dynamic assignment of strategies to search indexes. Names must be unique.</param>
    /// <returns>Azure Search builder for chaining.</returns>
    public IAzureSearchBuilder RegisterStrategy<TStrategy, TSearchModel>(string strategyName) where TStrategy : BaseAzureSearchIndexingStrategy<TSearchModel> where TSearchModel : IAzureSearchModel, new()
    {
        ValidateIndexSearchModelProperties<TSearchModel>();

        StrategyStorage.AddStrategy<TStrategy>(strategyName);
        serviceCollection.AddTransient<TStrategy>();

        return this;
    }


    private static void ValidateIndexSearchModelProperties<TSearchModel>() where TSearchModel : IAzureSearchModel, new()
    {
        var type = typeof(TSearchModel);

        var propertiesWithAttributes = type.GetProperties()
            .Where(x => x.GetCustomAttributes<SimpleFieldAttribute>().Any())
            .Select(x => new
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


    /// <inheritdoc/>
    public IAzureSearchBuilder RegisterSearchClientConfiguration(Action<SearchClientOptions> configureSearchClientOptions)
    {
        if (configureAction is not null)
        {
            throw new InvalidOperationException("SearchClientOptions can only be configured once.");
        }

        configureAction = configureSearchClientOptions;

        return this;
    }


    public void ConfigureClientOptions(SearchClientOptions options) => configureAction?.Invoke(options);
}
