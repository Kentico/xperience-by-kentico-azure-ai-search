using System.Text.Json.Serialization;

using Azure.Search.Documents.Indexes;

using CMS.Tests;

using Kentico.Xperience.AzureSearch.Indexing;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Xperience.AzureSearch.Tests.Indexing;

[TestFixture]
[Category.Unit]
internal class AzureSearchBuilderTests
{
    private readonly Dictionary<string, string> inMemorySettings = new()
        {
            {$"{AzureSearchOptions.CMS_AZURE_SEARCH_SECTION_NAME}:{nameof(AzureSearchOptions.SearchServiceEndPoint)}", "https://test.search.windows.net"},
            {$"{AzureSearchOptions.CMS_AZURE_SEARCH_SECTION_NAME}:{nameof(AzureSearchOptions.SearchServiceAdminApiKey)}", "test-admin-key"},
            {$"{AzureSearchOptions.CMS_AZURE_SEARCH_SECTION_NAME}:{nameof(AzureSearchOptions.SearchServiceQueryApiKey)}", "test-query-key"}
        };


    [Test]
    public void RegisterStrategy_WithAdditionalPropertiesWithoutAttributes_ShouldNotThrow()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        // Act & Assert
        Assert.That(() => serviceCollection.AddKenticoAzureSearch(builder =>
            builder.RegisterStrategy<TestSearchModelWithAdditionalPropertiesStrategy, TestSearchModelWithAdditionalProperties>("TestStrategyWithAdditionalProps"),
            configuration),
            Throws.Nothing);
    }


    [Test]
    public void RegisterStrategy_WithoutKeyField_ShouldThrow()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => serviceCollection.AddKenticoAzureSearch(builder =>
            builder.RegisterStrategy<InvalidTestSearchModelStrategy, InvalidTestSearchModel>("InvalidTestStrategy"), configuration));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message,
                Does.Contain("Exactly one field in your index must serve as the document key (IsKey = true). It must be a string, and it must uniquely identify each document. It's also required to have IsHidden = false."));
        });
    }
}


// Test search model with additional properties that don't have field attributes
internal class TestSearchModelWithAdditionalProperties : BaseAzureSearchModel
{
    [SearchableField]
    public string Title { get; set; } = string.Empty;


    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalFields { get; set; } = [];


    public DateTime IndexedAt { get; set; }
}


internal class TestSearchModelWithAdditionalPropertiesStrategy : BaseAzureSearchIndexingStrategy<TestSearchModelWithAdditionalProperties>
{
}


// Invalid test search model without key field (override the base key field)
internal class InvalidTestSearchModel : IAzureSearchModel
{
    public string? Url { get; set; } = string.Empty;
    public string ContentTypeName { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;
    public string ItemGuid { get; set; } = string.Empty;
    public string ObjectID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    [SearchableField]
    public string Title { get; set; } = string.Empty;
}


internal class InvalidTestSearchModelStrategy : BaseAzureSearchIndexingStrategy<InvalidTestSearchModel>
{
}
