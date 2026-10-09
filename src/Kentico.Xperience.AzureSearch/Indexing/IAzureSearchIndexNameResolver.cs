using System.Diagnostics.CodeAnalysis;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Translates index and alias names stored in Xperience (logical names) to names used in the Azure AI Search service
/// and vice versa, applying the <see cref="AzureSearchOptions.IndexNamePrefix"/>.
/// </summary>
public interface IAzureSearchIndexNameResolver
{
    /// <summary>
    /// The configured prefix. Empty when no prefix is configured.
    /// </summary>
    string IndexNamePrefix { get; }


    /// <summary>
    /// Gets the name of the index or alias as stored in the Azure AI Search service.
    /// </summary>
    /// <param name="name">Index or alias name as stored in Xperience.</param>
    /// <returns>The <paramref name="name"/> with the configured prefix applied.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the resulting name exceeds the maximum length allowed by Azure AI Search.</exception>
    string GetAzureName(string name);


    /// <summary>
    /// Gets the name of the index or alias as stored in Xperience from the name used in the Azure AI Search service.
    /// </summary>
    /// <param name="azureName">Index or alias name as stored in the Azure AI Search service.</param>
    /// <param name="name">Index or alias name as stored in Xperience.</param>
    /// <returns><see langword="true"/> if the <paramref name="azureName"/> starts with the configured prefix, otherwise <see langword="false"/>.</returns>
    bool TryGetName(string azureName, [NotNullWhen(true)] out string? name);
}
