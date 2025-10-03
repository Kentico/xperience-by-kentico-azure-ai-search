using Azure.Search.Documents.Indexes.Models;

namespace Kentico.Xperience.AzureSearch.Aliasing;

/// <summary>
/// Manages <see cref="SearchAlias" /> instances.
/// </summary>
public interface IAzureSearchIndexAliasService
{
    /// <summary>
    /// Creates a new AzureSearch index alias in Azure.
    /// </summary>
    /// <param name="alias">The alias to create.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="alias"/> is null.</exception>
    /// <exception cref="Azure.RequestFailedException">Thrown when a failure is returned by the Search service.</exception>
    Task CreateAlias(SearchAlias alias, CancellationToken cancellationToken);

    /// <summary>
    /// Edits the AzureSearch index alias in Azure.
    /// </summary>
    /// <param name="oldAliasName">The alias to edit.</param>
    /// <param name="newAlias">New alias.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="oldAliasName"/> is null.</exception>
    /// <exception cref="Azure.RequestFailedException">Thrown when a failure is returned by the Search service.</exception>
    Task EditAlias(string oldAliasName, SearchAlias newAlias, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the AzureSearch index alias by removing existing index alias data from Azure.
    /// </summary>
    /// <param name="aliasName">The index to delete.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="aliasName"/> is null.</exception>
    /// <exception cref="Azure.RequestFailedException">Thrown when a failure is returned by the Search service.</exception>
    Task DeleteAlias(string aliasName, CancellationToken cancellationToken);
}
