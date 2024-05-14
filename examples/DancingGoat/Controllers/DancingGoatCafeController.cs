using CMS.Websites;

using DancingGoat;
using DancingGoat.Controllers;
using DancingGoat.Models;

using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;

using Microsoft.AspNetCore.Mvc;

[assembly: RegisterWebPageRoute(CafePage.CONTENT_TYPE_NAME, typeof(DancingGoatCafeController), WebsiteChannelNames = new[] { DancingGoatConstants.WEBSITE_CHANNEL_NAME })]

namespace DancingGoat.Controllers;

public class DancingGoatCafeController : Controller
{
    private readonly IWebPageUrlRetriever urlRetriever;
    private readonly IWebPageDataContextRetriever webPageDataContextRetriever;
    private readonly IPreferredLanguageRetriever currentLanguageRetriever;
    private readonly CafePageRepository cafePageRepository;

    public DancingGoatCafeController(
        IWebPageUrlRetriever urlRetriever,
        IPreferredLanguageRetriever currentLanguageRetriever,
        IWebPageDataContextRetriever webPageDataContextRetriever,
        CafePageRepository cafePageRepository)
    {
        this.urlRetriever = urlRetriever;
        this.currentLanguageRetriever = currentLanguageRetriever;
        this.webPageDataContextRetriever = webPageDataContextRetriever;
        this.cafePageRepository = cafePageRepository;
    }

    public async Task<IActionResult> Index()
    {
        string languageName = currentLanguageRetriever.Get();
        int webPageItemId = webPageDataContextRetriever.Retrieve().WebPage.WebPageItemID;

        var cafePage = await cafePageRepository.GetCafePage(webPageItemId, languageName, cancellationToken: HttpContext.RequestAborted);

        var model = await CafePageViewModel.GetViewModel(cafePage, languageName, urlRetriever);

        return View(model);
    }
}
