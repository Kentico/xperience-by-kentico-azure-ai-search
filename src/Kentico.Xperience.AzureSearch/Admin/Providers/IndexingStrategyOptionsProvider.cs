using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.AzureSearch.Indexing;

namespace Kentico.Xperience.AzureSearch.Admin;

internal class IndexingStrategyOptionsProvider : IDropDownOptionsProvider
{
    public Task<IEnumerable<DropDownOptionItem>> GetOptionItems() =>
        Task.FromResult(StrategyStorage.Strategies.Keys.Select(x => new DropDownOptionItem()
        {
            Value = x,
            Text = x
        }));
}
