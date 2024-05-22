using CMS.Core;

using DancingGoat.Models;

using Kentico.Xperience.AzureSearch.Admin;
using Kentico.Xperience.AzureSearch.Indexing;
using Kentico.Xperience.AzureSearch.Tests.Base;

namespace Kentico.Xperience.AzureSearch.Tests.Tests;

internal class IndexedItemModelExtensionsTests
{

    [Test]
    public void IsIndexedByIndex()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();

        AzureSearchIndexStore.Instance.SetIndicies(new List<AzureSearchConfigurationModel>());
        AzureSearchIndexStore.Instance.AddIndex(MockDataProvider.Index);

        var fixture = new Fixture();
        var item = fixture.Create<IndexEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);
        Assert.That(model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }

    [Test]
    public void WildCard()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();
        var fixture = new Fixture();
        var item = fixture.Create<IndexEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);
        model.WebPageItemTreePath = "/Home";

        var index = MockDataProvider.Index;
        var path = new AzureSearchIndexIncludedPath("/%") { ContentTypes = [new(ArticlePage.CONTENT_TYPE_NAME, nameof(ArticlePage))] };

        index.IncludedPaths = new List<AzureSearchIndexIncludedPath>() { path };

        AzureSearchIndexStore.Instance.SetIndicies(new List<AzureSearchConfigurationModel>());
        AzureSearchIndexStore.Instance.AddIndex(index);

        Assert.That(model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }

    [Test]
    public void WrongWildCard()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();
        var fixture = new Fixture();
        var item = fixture.Create<IndexEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);
        model.WebPageItemTreePath = "/Home";

        var index = MockDataProvider.Index;
        var path = new AzureSearchIndexIncludedPath("/Index/%") { ContentTypes = [new(ArticlePage.CONTENT_TYPE_NAME, nameof(ArticlePage))] };

        index.IncludedPaths = new List<AzureSearchIndexIncludedPath>() { path };

        AzureSearchIndexStore.Instance.SetIndicies(new List<AzureSearchConfigurationModel>());
        AzureSearchIndexStore.Instance.AddIndex(index);

        Assert.That(!model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }

    [Test]
    public void WrongPath()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();
        var fixture = new Fixture();
        var item = fixture.Create<IndexEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);
        model.WebPageItemTreePath = "/Home";

        var index = MockDataProvider.Index;
        var path = new AzureSearchIndexIncludedPath("/Index") { ContentTypes = [new(ArticlePage.CONTENT_TYPE_NAME, nameof(ArticlePage))] };

        index.IncludedPaths = new List<AzureSearchIndexIncludedPath>() { path };

        AzureSearchIndexStore.Instance.SetIndicies(new List<AzureSearchConfigurationModel>());
        AzureSearchIndexStore.Instance.AddIndex(index);

        Assert.That(!model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }

    [Test]
    public void WrongContentType()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();

        var fixture = new Fixture();
        var item = fixture.Create<IndexEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);
        model.ContentTypeName = "DancingGoat.HomePage";

        AzureSearchIndexStore.Instance.SetIndicies(new List<AzureSearchConfigurationModel>());
        AzureSearchIndexStore.Instance.AddIndex(MockDataProvider.Index);

        Assert.That(!model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }

    [Test]
    public void WrongIndex()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();

        var fixture = new Fixture();
        var item = fixture.Create<IndexEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);

        AzureSearchIndexStore.Instance.SetIndicies(new List<AzureSearchConfigurationModel>());
        AzureSearchIndexStore.Instance.AddIndex(MockDataProvider.Index);

        Assert.That(!MockDataProvider.WebModel(model).IsIndexedByIndex(log, "NewIndex", MockDataProvider.EventName));
    }

    [Test]
    public void WrongLanguage()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();

        var fixture = new Fixture();
        var item = fixture.Create<IndexEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);
        model.LanguageName = "sk";

        AzureSearchIndexStore.Instance.SetIndicies(new List<AzureSearchConfigurationModel>());
        AzureSearchIndexStore.Instance.AddIndex(MockDataProvider.Index);

        Assert.That(!model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }
}
