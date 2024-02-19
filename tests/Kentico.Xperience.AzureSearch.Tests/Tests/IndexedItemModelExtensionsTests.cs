using CMS.Core;
using DancingGoat.Models;
using Kentico.Xperience.AzureSearch.Admin;
using Kentico.Xperience.AzureSearch.Indexing;
using Kentico.Xperience.AzureSearch.Tests.Base;
namespace Kentico.Xperience.AzureSearch.Tests.Tests;

internal class MockEventLogService : IEventLogService
{
    public void LogEvent(EventLogData eventLogData)
    {
        // Method intentionally left empty.
    }
}

internal class IndexedItemModelExtensionsTests
{
    private readonly IEventLogService log;

    public IndexedItemModelExtensionsTests() => log = new MockEventLogService();

    [Test]
    public void IsIndexedByIndex()
    {
        AzureSearchIndexStore.Instance.SetIndicies(new List<AzureSearchConfigurationModel>());
        AzureSearchIndexStore.Instance.AddIndex(MockDataProvider.Index);

        Assert.That(MockDataProvider.WebModel.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }

    [Test]
    public void WildCard()
    {
        var model = MockDataProvider.WebModel;
        model.WebPageItemTreePath = "/Home";

        var index = MockDataProvider.Index;
        var path = new AzureSearchIndexIncludedPath("/%") { ContentTypes = [ArticlePage.CONTENT_TYPE_NAME] };

        index.IncludedPaths = new List<AzureSearchIndexIncludedPath>() { path };

        AzureSearchIndexStore.Instance.SetIndicies(new List<AzureSearchConfigurationModel>());
        AzureSearchIndexStore.Instance.AddIndex(index);

        Assert.That(model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }

    [Test]
    public void WrongWildCard()
    {
        var model = MockDataProvider.WebModel;
        model.WebPageItemTreePath = "/Home";

        var index = MockDataProvider.Index;
        var path = new AzureSearchIndexIncludedPath("/Index/%") { ContentTypes = [ArticlePage.CONTENT_TYPE_NAME] };

        index.IncludedPaths = new List<AzureSearchIndexIncludedPath>() { path };

        AzureSearchIndexStore.Instance.SetIndicies(new List<AzureSearchConfigurationModel>());
        AzureSearchIndexStore.Instance.AddIndex(index);

        Assert.That(!model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }

    [Test]
    public void WrongPath()
    {
        var model = MockDataProvider.WebModel;
        model.WebPageItemTreePath = "/Home";

        var index = MockDataProvider.Index;
        var path = new AzureSearchIndexIncludedPath("/Index") { ContentTypes = [ArticlePage.CONTENT_TYPE_NAME] };

        index.IncludedPaths = new List<AzureSearchIndexIncludedPath>() { path };

        AzureSearchIndexStore.Instance.SetIndicies(new List<AzureSearchConfigurationModel>());
        AzureSearchIndexStore.Instance.AddIndex(index);

        Assert.That(!model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }

    [Test]
    public void WrongContentType()
    {
        var model = MockDataProvider.WebModel;
        model.ContentTypeName = "DancingGoat.HomePage";

        AzureSearchIndexStore.Instance.SetIndicies(new List<AzureSearchConfigurationModel>());
        AzureSearchIndexStore.Instance.AddIndex(MockDataProvider.Index);

        Assert.That(!model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }

    [Test]
    public void WrongIndex()
    {
        AzureSearchIndexStore.Instance.SetIndicies(new List<AzureSearchConfigurationModel>());
        AzureSearchIndexStore.Instance.AddIndex(MockDataProvider.Index);

        Assert.That(!MockDataProvider.WebModel.IsIndexedByIndex(log, "NewIndex", MockDataProvider.EventName));
    }

    [Test]
    public void WrongLanguage()
    {
        var model = MockDataProvider.WebModel;
        model.LanguageName = "sk";

        AzureSearchIndexStore.Instance.SetIndicies(new List<AzureSearchConfigurationModel>());
        AzureSearchIndexStore.Instance.AddIndex(MockDataProvider.Index);

        Assert.That(!model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }
}
