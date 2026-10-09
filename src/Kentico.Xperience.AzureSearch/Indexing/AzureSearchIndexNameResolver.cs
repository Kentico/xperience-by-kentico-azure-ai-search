using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Default implementation of <see cref="IAzureSearchIndexNameResolver"/>.
/// </summary>
public sealed partial class AzureSearchIndexNameResolver : IAzureSearchIndexNameResolver
{
    /// <summary>
    /// Maximum length of an index or alias name allowed by Azure AI Search.
    /// </summary>
    public const int MAX_NAME_LENGTH = 128;


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

        if (IndexNamePrefix.Length > 0 && !PrefixRegex().IsMatch(IndexNamePrefix))
        {
            throw new ArgumentException(
                $"The configured Azure Search index name prefix '{IndexNamePrefix}' is invalid. " +
                "The prefix can only contain lowercase letters, digits or dashes and must start with a lowercase letter or digit.",
                nameof(indexNamePrefix));
        }

        if (IndexNamePrefix.Length >= MAX_NAME_LENGTH)
        {
            throw new ArgumentException($"The configured Azure Search index name prefix must be shorter than {MAX_NAME_LENGTH} characters.", nameof(indexNamePrefix));
        }
    }


    /// <inheritdoc />
    public string GetAzureName(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (IndexNamePrefix.Length == 0)
        {
            return name;
        }

        string azureName = IndexNamePrefix + name;

        if (azureName.Length > MAX_NAME_LENGTH)
        {
            throw new InvalidOperationException(
                $"The name '{azureName}' (prefix '{IndexNamePrefix}' + '{name}') exceeds the maximum length of {MAX_NAME_LENGTH} characters allowed by Azure AI Search.");
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

        if (azureName.Length > IndexNamePrefix.Length && azureName.StartsWith(IndexNamePrefix, StringComparison.Ordinal))
        {
            name = azureName[IndexNamePrefix.Length..];
            return true;
        }

        name = null;
        return false;
    }


    [GeneratedRegex("^[a-z0-9][a-z0-9-]*$")]
    private static partial Regex PrefixRegex();
}
