using DancingGoat.Models;
using DancingGoat.Search;

using Kentico.Content.Web.Mvc.Routing;

using Microsoft.AspNetCore.Mvc;

namespace DancingGoat.ViewComponents
{
    public class NavigationMenuViewComponent : ViewComponent
    {
        private readonly NavigationService navigationService;
        private readonly IPreferredLanguageRetriever currentLanguageRetriever;

        public NavigationMenuViewComponent(NavigationService navigationService, IPreferredLanguageRetriever currentLanguageRetriever)
        {
            this.navigationService = navigationService;
            this.currentLanguageRetriever = currentLanguageRetriever;
        }


        public async Task<IViewComponentResult> InvokeAsync()
        {
            string languageName = currentLanguageRetriever.Get();

            var navigationViewModels = (await navigationService.GetNavigationItemViewModels(languageName, HttpContext.RequestAborted)).ToList();

            navigationViewModels.AddRange(new List<NavigationItemViewModel>
            {
                new($"Search", "/Search"),
                new($"{nameof(SearchController.Geo)}Search", $"/Search/{nameof(SearchController.Geo)}"),
                new($"{nameof(SearchController.Semantic)}Search", $"/Search/{nameof(SearchController.Semantic)}"),
                new($"{nameof(SearchController.Simple)}Search", $"/Search/{nameof(SearchController.Simple)}"),
            });

            return View($"~/Components/ViewComponents/NavigationMenu/Default.cshtml", navigationViewModels);
        }
    }
}
