using CMS.Tests;

using Kentico.Xperience.AzureSearch.Indexing;

namespace Kentico.Xperience.AzureSearch.Tests.Indexing;

[TestFixture]
[Category.Unit]
internal class AzureSearchIndexNameResolverTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void GetAzureName_WithoutPrefix_ReturnsOriginalName(string? prefix)
    {
        var resolver = new AzureSearchIndexNameResolver(prefix);

        Assert.Multiple(() =>
        {
            Assert.That(resolver.IndexNamePrefix, Is.Empty);
            Assert.That(resolver.GetAzureName("my-index"), Is.EqualTo("my-index"));
        });
    }


    [Test]
    public void GetAzureName_WithPrefix_ReturnsPrefixedName()
    {
        var resolver = new AzureSearchIndexNameResolver("dev-");

        Assert.That(resolver.GetAzureName("my-index"), Is.EqualTo("dev-my-index"));
    }


    [Test]
    public void GetAzureName_WithPrefixSurroundedByWhitespace_TrimsPrefix()
    {
        var resolver = new AzureSearchIndexNameResolver(" uat- ");

        Assert.That(resolver.GetAzureName("my-index"), Is.EqualTo("uat-my-index"));
    }


    [TestCase(null)]
    [TestCase("")]
    public void GetAzureName_WithEmptyName_ThrowsArgumentException(string? name)
    {
        var resolver = new AzureSearchIndexNameResolver("dev-");

        Assert.Throws(Is.InstanceOf<ArgumentException>(), () => resolver.GetAzureName(name!));
    }


    [Test]
    public void GetAzureName_WithTooLongResult_ThrowsInvalidOperationException()
    {
        var resolver = new AzureSearchIndexNameResolver("dev-");

        Assert.Throws<InvalidOperationException>(() => resolver.GetAzureName(new string('a', AzureSearchIndexNameResolver.MAX_NAME_LENGTH)));
    }


    [TestCase("Dev-")]
    [TestCase("-dev")]
    [TestCase("dev_")]
    [TestCase("dev.")]
    [TestCase("d ev")]
    public void Constructor_WithInvalidPrefix_ThrowsArgumentException(string prefix) =>
        Assert.Throws<ArgumentException>(() => new AzureSearchIndexNameResolver(prefix));


    [Test]
    public void Constructor_WithTooLongPrefix_ThrowsArgumentException() =>
        Assert.Throws<ArgumentException>(() => new AzureSearchIndexNameResolver(new string('a', AzureSearchIndexNameResolver.MAX_NAME_LENGTH)));


    [Test]
    public void TryGetName_WithMatchingPrefix_ReturnsNameWithoutPrefix()
    {
        var resolver = new AzureSearchIndexNameResolver("dev-");

        bool result = resolver.TryGetName("dev-my-index", out string? name);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(name, Is.EqualTo("my-index"));
        });
    }


    [TestCase("uat-my-index")]
    [TestCase("my-index")]
    [TestCase("dev-")]
    [TestCase("")]
    public void TryGetName_WithNonMatchingPrefix_ReturnsFalse(string azureName)
    {
        var resolver = new AzureSearchIndexNameResolver("dev-");

        bool result = resolver.TryGetName(azureName, out string? name);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(name, Is.Null);
        });
    }


    [Test]
    public void TryGetName_WithoutPrefix_ReturnsOriginalName()
    {
        var resolver = new AzureSearchIndexNameResolver(null);

        bool result = resolver.TryGetName("uat-my-index", out string? name);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(name, Is.EqualTo("uat-my-index"));
        });
    }
}
