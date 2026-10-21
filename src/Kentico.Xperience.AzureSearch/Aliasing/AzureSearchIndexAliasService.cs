using System.Net;

using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

using Kentico.Xperience.AzureSearch.Indexing;

namespace Kentico.Xperience.AzureSearch.Aliasing;

/// <summary>
/// Default implementation of <see cref="IAzureSearchIndexAliasService"/>.
/// </summary>
internal class AzureSearchIndexAliasService : IAzureSearchIndexAliasService
{
    private readonly SearchIndexClient indexClient;
    private readonly IAzureSearchIndexNameResolver indexNameResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchIndexAliasService"/> class.
    /// </summary>
    public AzureSearchIndexAliasService(SearchIndexClient indexClient, IAzureSearchIndexNameResolver indexNameResolver)
    {
        this.indexClient = indexClient;
        this.indexNameResolver = indexNameResolver;
    }

    /// <inheritdoc />
    public async Task CreateAlias(SearchAlias alias, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(alias);

        await indexClient.CreateOrUpdateAliasAsync(ToAzureAlias(alias), cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task EditAlias(string oldAliasName, SearchAlias newAlias, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(oldAliasName))
        {
            throw new ArgumentNullException(nameof(oldAliasName));
        }

        ArgumentNullException.ThrowIfNull(newAlias);

        try
        {
            await DeleteAlias(oldAliasName, cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
        {
            // The old alias may not exist in Azure AI Search, e.g. after the index name prefix was changed.
            // Continue so that re-saving the alias creates it under the current prefix.
        }

        await indexClient.CreateOrUpdateAliasAsync(ToAzureAlias(newAlias), cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAlias(string aliasName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(aliasName))
        {
            throw new ArgumentNullException(nameof(aliasName));
        }

        await indexClient.DeleteAliasAsync(indexNameResolver.GetAzureName(aliasName), cancellationToken: cancellationToken);
    }

    private SearchAlias ToAzureAlias(SearchAlias alias)
    {
        if (string.IsNullOrEmpty(indexNameResolver.IndexNamePrefix))
        {
            return alias;
        }

        return new SearchAlias(indexNameResolver.GetAzureName(alias.Name), alias.Indexes.Select(indexNameResolver.GetAzureName));
    }
}
