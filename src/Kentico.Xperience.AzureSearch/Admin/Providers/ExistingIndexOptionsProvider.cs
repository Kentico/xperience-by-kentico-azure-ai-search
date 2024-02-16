using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;

namespace Kentico.Xperience.AzureSearch.Admin;

internal class ExistingIndexOptionsProvider : IGeneralSelectorDataProvider
{
    private readonly IAzureSearchIndexItemInfoProvider indexProvider;

    public ExistingIndexOptionsProvider(IAzureSearchIndexItemInfoProvider indexProvider) => this.indexProvider = indexProvider;

    public async Task<PagedSelectListItems<string>> GetItemsAsync(string searchTerm, int pageIndex, CancellationToken cancellationToken)
    {
        // Prepares a query for retrieving index objects
        var itemQuery = indexProvider.Get();

        // If a search term is entered, only loads indexes whose indexName starts with the term
        if (!string.IsNullOrEmpty(searchTerm))
        {
            itemQuery.WhereStartsWith(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemIndexName), searchTerm);
        }

        // Ensures paging of items
        itemQuery.Page(pageIndex, 20);

        // Retrieves the users and converts them into ObjectSelectorListItem<string> options
        var items = (await itemQuery.GetEnumerableTypedResultAsync()).Select(x => new ObjectSelectorListItem<string>()
        {
            Value = x.AzureSearchIndexItemIndexName,
            Text = x.AzureSearchIndexItemIndexName,
            IsValid = true
        });

        return new PagedSelectListItems<string>()
        {
            NextPageAvailable = itemQuery.NextPageAvailable,
            Items = items
        };
    }

    // Returns ObjectSelectorListItem<string> options for all item values that are currently selected
    public async Task<IEnumerable<ObjectSelectorListItem<string>>> GetSelectedItemsAsync(IEnumerable<string> selectedValues, CancellationToken cancellationToken)
    {
        var itemQuery = indexProvider.Get().Page(0, 20);
        var items = (await itemQuery.GetEnumerableTypedResultAsync()).Select(x => new ObjectSelectorListItem<string>()
        {
            Value = x.AzureSearchIndexItemIndexName,
            Text = x.AzureSearchIndexItemIndexName,
            IsValid = true
        });

        var selectedItems = new List<ObjectSelectorListItem<string>>();

        if (selectedValues is not null)
        {
            foreach (string? value in selectedValues)
            {
                var item = items.FirstOrDefault(x => x.Value == value);

                if (item != default)
                {
                    selectedItems.Add(item);
                }
            }
        }

        return selectedItems;
    }
}
