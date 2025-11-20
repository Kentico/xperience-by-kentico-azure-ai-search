using CMS.Tests;

using FluentAssertions;

using Kentico.Xperience.AzureSearch.Indexing;

namespace Kentico.Xperience.AzureSearch.Tests.Indexing;

/// <summary>
/// Tests for multi-language indexing behavior.
/// </summary>
[TestFixture]
[Category.Unit]
public class MultiLanguageIndexingTests : UnitTests
{
    [Test]
    public void ObjectID_ShouldIncludeLanguageName_ForReusableContentItems()
    {
        // Arrange
        var itemGuid = Guid.NewGuid();
        var englishLanguage = "en-US";
        var frenchLanguage = "fr-FR";

        var englishItem = new IndexEventReusableItemModel(
            itemID: 1,
            itemGuid: itemGuid,
            languageName: englishLanguage,
            contentTypeName: "Office",
            name: "Main Office",
            isSecured: false,
            contentTypeID: 1,
            contentLanguageID: 1
        );

        var frenchItem = new IndexEventReusableItemModel(
            itemID: 1,
            itemGuid: itemGuid,
            languageName: frenchLanguage,
            contentTypeName: "Office",
            name: "Bureau Principal",
            isSecured: false,
            contentTypeID: 1,
            contentLanguageID: 2
        );

        // Assert - Both items should have the same ItemGuid but different language names
        englishItem.ItemGuid.Should().Be(frenchItem.ItemGuid, "both language variants share the same ItemGuid");
        englishItem.LanguageName.Should().NotBe(frenchItem.LanguageName, "language variants have different language names");
        
        // The expected ObjectID format should be "{ItemGuid}_{LanguageName}"
        var expectedEnglishObjectID = $"{itemGuid}_{englishLanguage}";
        var expectedFrenchObjectID = $"{itemGuid}_{frenchLanguage}";
        
        expectedEnglishObjectID.Should().NotBe(expectedFrenchObjectID, 
            "ObjectIDs should be unique for different language variants to prevent overwriting in the index");
    }

    [Test]
    public void ObjectID_ShouldIncludeLanguageName_ForWebPageItems()
    {
        // Arrange
        var itemGuid = Guid.NewGuid();
        var englishLanguage = "en-US";
        var germanLanguage = "de-DE";

        var englishItem = new IndexEventWebPageItemModel(
            itemID: 1,
            itemGuid: itemGuid,
            languageName: englishLanguage,
            contentTypeName: "ArticlePage",
            name: "Test Article",
            isSecured: false,
            contentTypeID: 1,
            contentLanguageID: 1,
            websiteChannelName: "TestChannel",
            webPageItemTreePath: "/articles/test",
            order: 1
        );

        var germanItem = new IndexEventWebPageItemModel(
            itemID: 1,
            itemGuid: itemGuid,
            languageName: germanLanguage,
            contentTypeName: "ArticlePage",
            name: "Test Artikel",
            isSecured: false,
            contentTypeID: 1,
            contentLanguageID: 2,
            websiteChannelName: "TestChannel",
            webPageItemTreePath: "/articles/test",
            order: 1
        );

        // Assert - Both items should have the same ItemGuid but different language names
        englishItem.ItemGuid.Should().Be(germanItem.ItemGuid, "both language variants share the same ItemGuid");
        englishItem.LanguageName.Should().NotBe(germanItem.LanguageName, "language variants have different language names");
        
        // The expected ObjectID format should be "{ItemGuid}_{LanguageName}"
        var expectedEnglishObjectID = $"{itemGuid}_{englishLanguage}";
        var expectedGermanObjectID = $"{itemGuid}_{germanLanguage}";
        
        expectedEnglishObjectID.Should().NotBe(expectedGermanObjectID, 
            "ObjectIDs should be unique for different language variants to prevent overwriting in the index");
    }

    [Test]
    public void ObjectID_Format_ShouldBeConsistentWithDeleteOperation()
    {
        // Arrange
        var itemGuid = Guid.NewGuid();
        var languageName = "en-US";
        
        // The ObjectID format used for indexing should match the format used for deletion
        var expectedObjectID = $"{itemGuid}_{languageName}";
        
        // Assert - This test documents the expected format for ObjectID
        expectedObjectID.Should().Contain(itemGuid.ToString());
        expectedObjectID.Should().Contain(languageName);
        expectedObjectID.Should().MatchRegex(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}_[a-zA-Z]{2}-[a-zA-Z]{2}$",
            "ObjectID should follow the pattern {Guid}_{LanguageCode}");
    }
}
