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
    [Test]
    public void RegisterStrategy_WithAdditionalPropertiesWithoutAttributes_ShouldNotThrow()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var inMemorySettings = new Dictionary<string, string>
        {
            {"CMSAzureSearch:SearchServiceEndPoint", "https://test.search.windows.net"},
            {"CMSAzureSearch:SearchServiceAdminApiKey", "test-admin-key"},
            {"CMSAzureSearch:SearchServiceQueryApiKey", "test-query-key"}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        // Act & Assert - This should not throw because properties without SimpleFieldAttribute/SearchableFieldAttribute are now allowed
        Assert.DoesNotThrow(() =>
        {
            serviceCollection.AddKenticoAzureSearch(builder =>
            {
                builder.RegisterStrategy<TestSearchModelWithAdditionalPropertiesStrategy, TestSearchModelWithAdditionalProperties>("TestStrategyWithAdditionalProps");
            }, configuration);
        });
    }

    [Test]
    public void RegisterStrategy_WithoutKeyField_ShouldThrow()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var inMemorySettings = new Dictionary<string, string>
        {
            {"CMSAzureSearch:SearchServiceEndPoint", "https://test.search.windows.net"},
            {"CMSAzureSearch:SearchServiceAdminApiKey", "test-admin-key"},
            {"CMSAzureSearch:SearchServiceQueryApiKey", "test-query-key"}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            serviceCollection.AddKenticoAzureSearch(builder =>
            {
                builder.RegisterStrategy<InvalidTestSearchModelStrategy, InvalidTestSearchModel>("InvalidTestStrategy");
            }, configuration);
        });

        // Assert
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Does.Contain("document key"));
    }
}

// Test search model with additional properties that don't have field attributes
internal class TestSearchModelWithAdditionalProperties : BaseAzureSearchModel
{
    [SearchableField]
    public string Title { get; set; } = string.Empty;

    // This property uses JsonExtensionData and should not require SimpleFieldAttribute or SearchableFieldAttribute
    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalFields { get; set; } = [];

    // This property is for internal use and should not require an attribute
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
    
    // No key field defined
    public string ObjectID { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;

    [SearchableField]
    public string Title { get; set; } = string.Empty;
}

internal class InvalidTestSearchModelStrategy : BaseAzureSearchIndexingStrategy<InvalidTestSearchModel>
{
}
