using Kentico.Xperience.AzureSearch.Admin;
using Kentico.Xperience.AzureSearch.Indexing;
using Kentico.Xperience.AzureSearch.Tests.Base;

namespace Kentico.Xperience.AzureSearch.Tests.Tests;
internal class IndexStoreTests
{

    [Test]
    public void AddAndGetIndex()
    {
        AzureSearchIndexStore.Instance.SetIndicies(new List<AzureSearchConfigurationModel>());

        AzureSearchIndexStore.Instance.AddIndex(MockDataProvider.Index);
        AzureSearchIndexStore.Instance.AddIndex(MockDataProvider.GetIndex("TestIndex", 1));

        Assert.Multiple(() =>
        {
            Assert.That(AzureSearchIndexStore.Instance.GetIndex("TestIndex") is not null);
            Assert.That(AzureSearchIndexStore.Instance.GetIndex(MockDataProvider.DefaultIndex) is not null);
        });
    }

    [Test]
    public void AddIndex_AlreadyExists()
    {
        AzureSearchIndexStore.Instance.SetIndicies(new List<AzureSearchConfigurationModel>());
        AzureSearchIndexStore.Instance.AddIndex(MockDataProvider.Index);

        bool hasThrown = false;

        try
        {
            AzureSearchIndexStore.Instance.AddIndex(MockDataProvider.Index);
        }
        catch
        {
            hasThrown = true;
        }

        Assert.That(hasThrown);
    }

    [Test]
    public void SetIndicies()
    {
        var defaultIndex = new AzureSearchConfigurationModel { IndexName = "DefaultIndex", Id = 0 };
        var simpleIndex = new AzureSearchConfigurationModel { IndexName = "SimpleIndex", Id = 1 };

        AzureSearchIndexStore.Instance.SetIndicies(new List<AzureSearchConfigurationModel>() { defaultIndex, simpleIndex });

        Assert.Multiple(() =>
        {
            Assert.That(AzureSearchIndexStore.Instance.GetIndex(defaultIndex.IndexName) is not null);
            Assert.That(AzureSearchIndexStore.Instance.GetIndex(simpleIndex.IndexName) is not null);
        });
    }
}
