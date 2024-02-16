using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace Kentico.Xperience.AzureSearch.Aliasing;

internal class AzureSearchIndexAliasService : IAzureSearchIndexAliasService
{
    private readonly SearchIndexClient indexClient;

    public AzureSearchIndexAliasService(SearchIndexClient indexClient) => this.indexClient = indexClient;

    public async Task CreateAlias(string aliasName, IEnumerable<string> indexNames, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(aliasName))
        {
            throw new ArgumentNullException(nameof(aliasName));
        }

        await indexClient.CreateAliasAsync(new SearchAlias(aliasName, indexNames), cancellationToken);
    }

    public async Task EditAlias(string oldAliasName, SearchAlias newAlias, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(oldAliasName))
        {
            throw new ArgumentNullException(nameof(oldAliasName));
        }

        await DeleteAlias(oldAliasName, cancellationToken);
        await indexClient.CreateOrUpdateAliasAsync(newAlias.Name, newAlias, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAlias(string aliasName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(aliasName))
        {
            throw new ArgumentNullException(nameof(aliasName));
        }

        await indexClient.DeleteAliasAsync(aliasName, cancellationToken);
    }
}
