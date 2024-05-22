using Kentico.Xperience.AzureSearch.Indexing;

namespace Microsoft.Extensions.DependencyInjection;

internal static class ServiceProviderExtensions
{
    /// <summary>
    /// Returns an instance of the <see cref="IAzureSearchIndexingStrategy"/> assigned to the given <paramref name="index" />.
    /// Used to generate instances of a <see cref="IAzureSearchIndexingStrategy"/> service type that can change at runtime.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="index"></param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the assigned <see cref="IAzureSearchIndexingStrategy"/> cannot be instantiated.
    ///     This shouldn't normally occur because we fallback to <see cref="BaseAzureSearchIndexingStrategy{DefaultAzureSearchModel}" /> if not custom strategy is specified.
    ///     However, incorrect dependency management in user-code could cause issues.
    /// </exception>
    /// <returns></returns>
    public static IAzureSearchIndexingStrategy GetRequiredStrategy(this IServiceProvider serviceProvider, AzureSearchIndex index)
    {
        var strategy = serviceProvider.GetRequiredService(index.AzureSearchIndexingStrategyType) as IAzureSearchIndexingStrategy;

        return strategy!;
    }
}
