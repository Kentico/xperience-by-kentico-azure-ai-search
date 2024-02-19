using CMS.Websites;

namespace DancingGoat.Models;

public record CafePageViewModel(string Title, string TeaserUrl, string Text, DateTime PublicationDate, Guid Guid, bool IsSecured, string Url, string Location)
{
    /// <summary>
    /// Validates and maps <see cref="CafePage"/> to a <see cref="CafePageViewModel"/>.
    /// </summary>
    public static async Task<CafePageViewModel> GetViewModel(CafePage cafePage, string languageName, IWebPageUrlRetriever urlRetriever)
    {
        var teaser = cafePage.CafePageTeaser.FirstOrDefault();

        string locationText = "You can find this Cafe at: " + cafePage.CafeLocation;

        var url = await urlRetriever.Retrieve(cafePage, languageName);

        return new CafePageViewModel(
            cafePage.CafeTitle,
            teaser?.ImageFile.Url,
            cafePage.CafePageText,
            cafePage.CafePagePublishDate,
            cafePage.SystemFields.ContentItemGUID,
            cafePage.SystemFields.ContentItemIsSecured,
            url.RelativePath,
            locationText
        );
    }
}
