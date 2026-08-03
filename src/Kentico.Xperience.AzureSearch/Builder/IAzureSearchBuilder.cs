using Azure.Search.Documents;

using Kentico.Xperience.AzureSearch.Indexing;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides methods for configuring and registering Azure Search strategies, models, and search client options.
/// </summary>
/// <remarks>This interface is designed to facilitate the dynamic assignment of indexing strategies to Azure
/// Search indexes. Implementations of this interface allow for the registration of custom indexing strategies and their
/// association with unique strategy names, as well as customization of the <see cref="SearchClientOptions"/> used by
/// the Azure Search clients.</remarks>
public interface IAzureSearchBuilder
{
    /// <summary>
    /// Registers the given <typeparamref name="TStrategy" /> as a transient service under <paramref name="strategyName" />.
    /// </summary>
    /// <typeparam name="TStrategy">The custom type of <see cref="IAzureSearchIndexingStrategy"/>.</typeparam>
    /// <typeparam name="TSearchModel">The custom type of <see cref="IAzureSearchModel"/> used to create and use an index.</typeparam>
    /// <param name="strategyName">Used internally <typeparamref name="TStrategy" /> to enable dynamic assignment of strategies to search indexes. Names must be unique.</param>
    /// <exception cref="ArgumentException">
    ///     Thrown if a strategy has already been registered with the given <paramref name="strategyName"/>.
    /// </exception>
    /// <returns>Azure Search builder for chaining.</returns>
    IAzureSearchBuilder RegisterStrategy<TStrategy, TSearchModel>(string strategyName)
        where TStrategy : BaseAzureSearchIndexingStrategy<TSearchModel>
        where TSearchModel : IAzureSearchModel, new();


    /// <summary>
    /// Registers a configuration delegate used to customize the <see cref="SearchClientOptions"/> applied to the
    /// Azure Search clients created by the integration (for example retry behavior, transport, or diagnostics).
    /// </summary>
    /// <remarks>
    /// The configuration can only be registered once. Use this to improve resilience against transient failures,
    /// such as SNAT port exhaustion, by tuning the retry and timeout policies.
    /// </remarks>
    /// <param name="configureSearchClientOptions">A delegate that configures the <see cref="SearchClientOptions"/> instance.</param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the search client configuration has already been registered.
    /// </exception>
    /// <returns>Azure Search builder for chaining.</returns>
    IAzureSearchBuilder RegisterSearchClientConfiguration(Action<SearchClientOptions> configureSearchClientOptions);
}
