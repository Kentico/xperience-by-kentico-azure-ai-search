using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Default implementation of <see cref="IAzureSearchIndexNameResolver"/>.
/// </summary>
internal sealed partial class AzureSearchIndexNameResolver : IAzureSearchIndexNameResolver
{
    /// <summary>
    /// Maximum length of an index or alias name allowed by Azure AI Search.
    /// </summary>
    internal const int MAX_NAME_LENGTH = 128;


    /// <summary>
    /// Separator inserted between the prefix and the name.
    /// </summary>
    internal const char SEPARATOR = '-';


    /// <summary>
    /// Maximum length of the prefix, leaving room for the <see cref="SEPARATOR"/> and at least one character of the name.
    /// </summary>
    internal const int MAX_PREFIX_LENGTH = MAX_NAME_LENGTH - 2;


    /// <inheritdoc />
    public string IndexNamePrefix { get; }


    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchIndexNameResolver"/> class.
    /// </summary>
    /// <param name="indexNamePrefix">Prefix applied to all index and alias names. Null or empty value means no prefix.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="indexNamePrefix"/> contains characters not allowed by Azure AI Search.</exception>
    public AzureSearchIndexNameResolver(string? indexNamePrefix)
    {
        IndexNamePrefix = indexNamePrefix?.Trim() ?? string.Empty;

        string? error = GetPrefixValidationError(IndexNamePrefix);
        if (error is not null)
        {
            throw new ArgumentException(error, nameof(indexNamePrefix));
        }
    }


    /// <summary>
    /// Validates the index name prefix.
    /// </summary>
    /// <param name="indexNamePrefix">Prefix to validate. Null or empty value means no prefix.</param>
    /// <returns>Error message when the prefix is invalid, otherwise <see langword="null"/>.</returns>
    internal static string? GetPrefixValidationError(string? indexNamePrefix)
    {
        string prefix = indexNamePrefix?.Trim() ?? string.Empty;

        if (prefix.Length > 0 && !PrefixRegex().IsMatch(prefix))
        {
            return $"The configured Azure Search index name prefix '{prefix}' is invalid. " +
                $"The prefix can only contain lowercase letters and digits. The '{SEPARATOR}' separator is added automatically, e.g. use 'dev' instead of 'dev{SEPARATOR}'.";
        }

        if (prefix.Length > MAX_PREFIX_LENGTH)
        {
            return $"The configured Azure Search index name prefix must not be longer than {MAX_PREFIX_LENGTH} characters.";
        }

        return null;
    }


    /// <inheritdoc />
    public string GetAzureName(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (IndexNamePrefix.Length == 0)
        {
            return name;
        }

        string azureName = $"{IndexNamePrefix}{SEPARATOR}{name}";

        if (azureName.Length > MAX_NAME_LENGTH)
        {
            throw new InvalidOperationException(
                $"The name '{azureName}' (prefix '{IndexNamePrefix}' + '{SEPARATOR}' + '{name}') exceeds the maximum length of {MAX_NAME_LENGTH} characters allowed by Azure AI Search.");
        }

        return azureName;
    }


    /// <inheritdoc />
    public bool TryGetName(string azureName, [NotNullWhen(true)] out string? name)
    {
        if (string.IsNullOrEmpty(azureName))
        {
            name = null;
            return false;
        }

        if (IndexNamePrefix.Length == 0)
        {
            name = azureName;
            return true;
        }

        int prefixLength = IndexNamePrefix.Length + 1;

        if (azureName.Length > prefixLength
            && azureName.StartsWith(IndexNamePrefix, StringComparison.Ordinal)
            && azureName[IndexNamePrefix.Length] == SEPARATOR)
        {
            name = azureName[prefixLength..];
            return true;
        }

        name = null;
        return false;
    }


    [GeneratedRegex("^[a-z0-9]+$")]
    private static partial Regex PrefixRegex();
}
