using GabriniCosmetics.Areas.Admin.Models.Interface;
using GabriniCosmetics.Models;
using GabriniCosmetics.Models.ViewModel;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GabriniCosmetics.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICategory _categoryService;
        private readonly IProduct _productService;
        private readonly ISliderBannerService _sliderBannerService;
        private readonly ISliderAdService _sliderAdService;
        private readonly IDealOfTheDaysService _dealOfTheDaysService;

        public HomeController(ILogger<HomeController> logger,ICategory category, IProduct productService, ISliderBannerService sliderBannerService, ISliderAdService sliderAdService, IDealOfTheDaysService dealOfTheDaysService)
        {
            _logger = logger;
            _categoryService = category;
            _productService = productService;
            _sliderBannerService = sliderBannerService;
            _sliderAdService = sliderAdService;
            _dealOfTheDaysService = dealOfTheDaysService;
        }

        public async Task<IActionResult> Index()
        {
            HomeVM homeVM = new HomeVM();
            homeVM.Categories = await _categoryService.GetCategories(); 
            homeVM.Products = await _productService.GetProducts();

            var productNew = await _productService.GetProducts();
            homeVM.ProductsNew = productNew.Where(x => x.Flags.Any(x => x.FlagType == "New")).ToList();
            homeVM.ProductsFeature = productNew.Where(x => x.Flags.Any(x => x.FlagType == "Feature")).ToList();
            homeVM.ProductsSale = productNew.Where(x => x.Flags.Any(x => x.FlagType == "Sale")).ToList();
            homeVM.ListOfProductsDealOfTheDays = productNew.Where(x => x.Product.IsDealOfDay).ToList();

            homeVM.SlidersBanner = await _sliderBannerService.GetSliderBanners();
            homeVM.SlidersAd = await _sliderAdService.GetAllSliderAds();

            var list = await _dealOfTheDaysService.GetAllDealsAsync();
            homeVM.DealOfTheDays = list.ToList();

            return View(homeVM);
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult PrivacyPolicy()
        {
            return View();
        }

        public IActionResult ConditionsofUse()
        {
            return View();
        }
        public IActionResult DeliveryReturns()
        {
            return View();
        }
        public ActionResult Change()
        {
            string? culture = Request.Query["culture"];
            if (culture != null)
            {
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );
            }


            string returnUrl = Request.Headers["Referer"].ToString() ?? "/";
            return Redirect(returnUrl);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
