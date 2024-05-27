using Kentico.Xperience.AzureSearch.Admin;

namespace Kentico.Xperience.AzureSearch.Aliasing;

/// <summary>
/// Represents a global singleton store of Azure Search index aliases
/// </summary>
public sealed class AzureSearchIndexAliasStore
{
    private static readonly Lazy<AzureSearchIndexAliasStore> mInstance = new();
    private readonly List<AzureSearchIndexAlias> registeredAliases = [];

    /// <summary>
    /// Gets singleton instance of the <see cref="AzureSearchIndexAliasStore"/>
    /// </summary>
    public static AzureSearchIndexAliasStore Instance => mInstance.Value;

    /// <summary>
    /// Gets all registered aliases.
    /// </summary>
    public IEnumerable<AzureSearchIndexAlias> GetAllAliases() => registeredAliases;

    /// <summary>
    /// Gets a registered <see cref="AzureSearchIndexAlias"/> with the specified <paramref name="identifier"/>,
    /// or <c>null</c>.
    /// </summary>
    /// <param name="identifier">The identifier of the alias to retrieve.</param>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidOperationException" />
    public AzureSearchIndexAlias? GetAlias(int identifier) => registeredAliases.Find(i => i.Identifier == identifier);

    /// <summary>
    /// Gets a registered <see cref="AzureSearchIndexAlias"/> with the specified <paramref name="aliasName"/>. If no alias is found, a <see cref="InvalidOperationException" /> is thrown.
    /// </summary>
    /// <param name="aliasName">The name of the alias to retrieve.</param>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidOperationException" />
    public AzureSearchIndexAlias GetRequiredAlias(string aliasName)
    {
        if (string.IsNullOrEmpty(aliasName))
        {
            throw new ArgumentException("Value must not be null or empty");
        }

        return registeredAliases.SingleOrDefault(i => i.AliasName.Equals(aliasName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"The index '{aliasName}' is not registered.");
    }

    /// <summary>
    /// Adds an alias to the store.
    /// </summary>
    /// <param name="alias">The alias to add.</param>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidOperationException" />
    internal void AddAlias(AzureSearchIndexAlias alias)
    {
        if (alias == null)
        {
            throw new ArgumentNullException(nameof(alias));
        }

        if (registeredAliases.Exists(i => i.AliasName.Equals(alias.AliasName, StringComparison.OrdinalIgnoreCase) || alias.Identifier == i.Identifier))
        {
            throw new InvalidOperationException($"Attempted to register AzureSearch index alias with identifier [{alias.Identifier}] and name [{alias.AliasName}] but it is already registered.");
        }

        registeredAliases.Add(alias);
    }

    /// <summary>
    /// Resets all aliases
    /// </summary>
    /// <param name="models"></param>
    internal void SetAliases(IEnumerable<AzureSearchAliasConfigurationModel> models)
    {
        registeredAliases.Clear();
        foreach (var alias in models)
        {
            Instance.AddAlias(new AzureSearchIndexAlias(alias));
        }
    }

    /// <summary>
    /// Sets the current aliases to those provided by <paramref name="configurationService"/>
    /// </summary>
    /// <param name="configurationService"></param>
    internal static void SetAliases(IAzureSearchConfigurationStorageService configurationService)
    {
        var aliases = configurationService.GetAllAliasData();

        Instance.SetAliases(aliases);
    }
}
