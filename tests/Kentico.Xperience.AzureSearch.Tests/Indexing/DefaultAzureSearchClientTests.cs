using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Tests;

using Kentico.Xperience.AzureSearch.Indexing;
using Kentico.Xperience.AzureSearch.Tests.Base;

namespace Kentico.Xperience.AzureSearch.Tests.Indexing;

[TestFixture]
[Category.Unit]
internal class DefaultAzureSearchClientTests
{
    private SearchIndexClient mockIndexClient;
    private IEventLogService mockEventLogService;
    private DefaultAzureSearchClient client;


    [SetUp]
    public void SetUp()
    {
        mockIndexClient = Substitute.For<SearchIndexClient>();
        mockEventLogService = Substitute.For<IEventLogService>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var resolver = new AzureSearchIndexNameResolver("dev");

        client = new DefaultAzureSearchClient(
            new AzureSearchIndexClientService(mockIndexClient, serviceProvider, resolver),
            Substitute.For<IContentQueryExecutor>(),
            serviceProvider,
            Substitute.For<IInfoProvider<ContentLanguageInfo>>(),
            Substitute.For<IInfoProvider<ChannelInfo>>(),
            Substitute.For<IConversionService>(),
            Substitute.For<IProgressiveCache>(),
            mockIndexClient,
            mockEventLogService,
            resolver);

        AzureSearchIndexStore.Instance.SetIndices([]);
        AzureSearchIndexStore.Instance.AddIndex(MockDataProvider.Index);
    }


    [TearDown]
    public void TearDown() => AzureSearchIndexStore.Instance.SetIndices([]);


    [Test]
    public async Task UpsertRecords_IndexMissingInAzure_LogsIndexNotFoundErrorAndReturnsZero()
    {
        mockIndexClient.GetIndexAsync("dev-" + MockDataProvider.DEFAULT_INDEX, Arg.Any<CancellationToken>())
            .Returns<Response<SearchIndex>>(_ => throw new RequestFailedException(404, "Not Found"));

        int result = await client.UpsertRecords([Substitute.For<IAzureSearchModel>()], MockDataProvider.DEFAULT_INDEX, CancellationToken.None);

        Assert.That(result, Is.Zero);
        mockEventLogService.Received(1).LogEvent(Arg.Any<EventLogData>());
        mockEventLogService.Received(1).LogEvent(Arg.Is<EventLogData>(x =>
            x.EventType == EventTypeEnum.Error
            && x.EventCode == AzureSearchIndexNotFoundException.EVENT_CODE
            && x.EventDescription.Contains("'dev-" + MockDataProvider.DEFAULT_INDEX + "'")));
    }
}
