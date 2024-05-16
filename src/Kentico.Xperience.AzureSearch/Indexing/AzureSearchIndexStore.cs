using Kentico.Xperience.AzureSearch.Admin;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Represents a global singleton store of AzureSearch indexes
/// </summary>
public sealed class AzureSearchIndexStore
{
    private static readonly Lazy<AzureSearchIndexStore> mInstance = new();
    private readonly List<AzureSearchIndex> registeredIndexes = [];

    /// <summary>
    /// Gets singleton instance of the <see cref="AzureSearchIndexStore"/>
    /// </summary>
    public static AzureSearchIndexStore Instance => mInstance.Value;

    /// <summary>
    /// Gets all registered indexes.
    /// </summary>
    public IEnumerable<AzureSearchIndex> GetAllIndices() => registeredIndexes;

    /// <summary>
    /// Gets a registered <see cref="AzureSearchIndex"/> with the specified <paramref name="indexName"/>,
    /// or <c>null</c>.
    /// </summary>
    /// <param name="indexName">The name of the index to retrieve.</param>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidOperationException" />
    public AzureSearchIndex? GetIndex(string indexName) => string.IsNullOrEmpty(indexName) ? null
        : registeredIndexes.SingleOrDefault(i => i.IndexName.Equals(indexName, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Gets a registered <see cref="AzureSearchIndex"/> with the specified <paramref name="identifier"/>,
    /// or <c>null</c>.
    /// </summary>
    /// <param name="identifier">The identifier of the index to retrieve.</param>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidOperationException" />
    public AzureSearchIndex? GetIndex(int identifier) => registeredIndexes.Find(i => i.Identifier == identifier);

    /// <summary>
    /// Gets a registered <see cref="AzureSearchIndex"/> with the specified <paramref name="indexName"/>. If no index is found, a <see cref="InvalidOperationException" /> is thrown.
    /// </summary>
    /// <param name="indexName">The name of the index to retrieve.</param>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidOperationException" />
    public AzureSearchIndex GetRequiredIndex(string indexName)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentException("Value must not be null or empty");
        }

        return registeredIndexes.SingleOrDefault(i => i.IndexName.Equals(indexName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"The index '{indexName}' is not registered.");
    }

    /// <summary>
    /// Adds an index to the store.
    /// </summary>
    /// <param name="index">The index to add.</param>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidOperationException" />
    internal void AddIndex(AzureSearchIndex index)
    {
        if (index == null)
        {
            throw new ArgumentNullException(nameof(index));
        }

        if (registeredIndexes.Exists(i => i.IndexName.Equals(index.IndexName, StringComparison.OrdinalIgnoreCase) || index.Identifier == i.Identifier))
        {
            throw new InvalidOperationException($"Attempted to register AzureSearch index with identifier [{index.Identifier}] and name [{index.IndexName}] but it is already registered.");
        }

        registeredIndexes.Add(index);
    }

    /// <summary>
    /// Resets all indicies
    /// </summary>
    /// <param name="models"></param>
    internal void SetIndicies(IEnumerable<AzureSearchConfigurationModel> models)
    {
        registeredIndexes.Clear();

        foreach (var index in models)
        {
            Instance.AddIndex(new AzureSearchIndex(index, StrategyStorage.Strategies));
        }
    }

    /// <summary>
    /// Sets the current indicies to those provided by <paramref name="configurationService"/>
    /// </summary>
    /// <param name="configurationService"></param>
    internal static void SetIndicies(IAzureSearchConfigurationStorageService configurationService)
    {
        var indices = configurationService.GetAllIndexData();

        Instance.SetIndicies(indices);
    }
}
