using Azure.Search.Documents;

using CMS.Tests;

using Kentico.Xperience.AzureSearch.Indexing;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Xperience.AzureSearch.Tests;

/// <summary>
/// Tests for the <see cref="AzureSearchBuilder"/> class.
/// </summary>
[TestFixture]
[Category.Unit]
internal class AzureSearchBuilderTests
{
    private static readonly Dictionary<string, string> inMemorySettings = new()
        {
            {$"{AzureSearchOptions.CMS_AZURE_SEARCH_SECTION_NAME}:{nameof(AzureSearchOptions.SearchServiceEndPoint)}", "https://test.search.windows.net"},
            {$"{AzureSearchOptions.CMS_AZURE_SEARCH_SECTION_NAME}:{nameof(AzureSearchOptions.SearchServiceAdminApiKey)}", "test-admin-key"},
            {$"{AzureSearchOptions.CMS_AZURE_SEARCH_SECTION_NAME}:{nameof(AzureSearchOptions.SearchServiceQueryApiKey)}", "test-query-key"}
        };


    private static AzureSearchBuilder CreateBuilder() => new(new ServiceCollection());


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


    [Test]
    public void RegisterStrategy_ReturnsSameBuilderInstance()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder.RegisterStrategy<TestSearchModelWithAdditionalPropertiesStrategy, TestSearchModelWithAdditionalProperties>("TestStrategyChaining");

        // Assert
        Assert.That(result, Is.SameAs(builder));
    }


    [Test]
    public void IncludeDefaultStrategy_ByDefault_IsTrue()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act & Assert
        Assert.That(builder.IncludeDefaultStrategy, Is.True);
    }


    [Test]
    public void RegisterSearchClientConfiguration_ReturnsSameBuilderInstance()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder.RegisterSearchClientConfiguration(_ => { });

        // Assert
        Assert.That(result, Is.SameAs(builder));
    }


    [Test]
    public void RegisterSearchClientConfiguration_CalledTwice_Throws()
    {
        // Arrange
        var builder = CreateBuilder();
        builder.RegisterSearchClientConfiguration(_ => { });

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() =>
            builder.RegisterSearchClientConfiguration(_ => { }));

        // Assert
        Assert.That(exception.Message, Does.Contain("SearchClientOptions can only be configured once."));
    }


    [Test]
    public void ConfigureOptions_WithRegisteredConfiguration_InvokesAction()
    {
        // Arrange
        var builder = CreateBuilder();
        var wasInvoked = false;
        SearchClientOptions? received = null;
        var options = new SearchClientOptions();

        builder.RegisterSearchClientConfiguration(o =>
        {
            wasInvoked = true;
            received = o;
        });

        // Act
        builder.ConfigureClientOptions(options);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(wasInvoked, Is.True);
            Assert.That(received, Is.SameAs(options));
        });
    }


    [Test]
    public void ConfigureOptions_WithoutRegisteredConfiguration_DoesNotThrow()
    {
        // Arrange
        var builder = CreateBuilder();
        var options = new SearchClientOptions();

        // Act & Assert
        Assert.That(() => builder.ConfigureClientOptions(options), Throws.Nothing);
    }


    [Test]
    public void ConfigureOptions_AppliesConfigurationToOptions()
    {
        // Arrange
        var builder = CreateBuilder();
        var options = new SearchClientOptions();

        builder.RegisterSearchClientConfiguration(o => o.Retry.MaxRetries = 7);

        // Act
        builder.ConfigureClientOptions(options);

        // Assert
        Assert.That(options.Retry.MaxRetries, Is.EqualTo(7));
    }
}
