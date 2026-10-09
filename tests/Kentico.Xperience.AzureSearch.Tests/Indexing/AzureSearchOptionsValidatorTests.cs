using CMS.Tests;

using Kentico.Xperience.AzureSearch.Indexing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kentico.Xperience.AzureSearch.Tests.Indexing;

[TestFixture]
[Category.Unit]
internal class AzureSearchOptionsValidatorTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("dev-")]
    [TestCase("prod01-")]
    public void Validate_ValidPrefix_Succeeds(string? prefix)
    {
        var result = new AzureSearchOptionsValidator().Validate(null, new AzureSearchOptions { IndexNamePrefix = prefix! });

        Assert.That(result.Succeeded, Is.True);
    }


    [TestCase("DEV-")]
    [TestCase("-dev")]
    [TestCase("dev_")]
    public void Validate_InvalidPrefix_Fails(string prefix)
    {
        var result = new AzureSearchOptionsValidator().Validate(null, new AzureSearchOptions { IndexNamePrefix = prefix });

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(result.FailureMessage, Does.Contain(prefix));
        });
    }


    [Test]
    public void Validate_TooLongPrefix_Fails()
    {
        var result = new AzureSearchOptionsValidator().Validate(null, new AzureSearchOptions { IndexNamePrefix = new string('a', AzureSearchIndexNameResolver.MAX_NAME_LENGTH) });

        Assert.That(result.Failed, Is.True);
    }


    [Test]
    public void StartupValidation_InvalidPrefix_Throws()
    {
        var services = new ServiceCollection();
        services.Configure<AzureSearchOptions>(o => o.IndexNamePrefix = "DEV-");
        services.AddOptions<AzureSearchOptions>().ValidateOnStart();
        services.AddSingleton<IValidateOptions<AzureSearchOptions>, AzureSearchOptionsValidator>();

        using var provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IStartupValidator>().Validate());
    }
}
