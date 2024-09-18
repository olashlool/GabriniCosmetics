using GabriniCosmetics.Areas.Admin.Models;
using GabriniCosmetics.Areas.Admin.Models.Interface;
using GabriniCosmetics.Areas.Admin.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace GabriniCosmetics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ICategory _categoryService;
        private readonly ISubcategory _subcategoryService;
        private readonly IProduct _productService;
        private readonly IOrder _orderService;

        public HomeController(ICategory category, IProduct product, IOrder order, ISubcategory subcategoryService)
        {
            _categoryService = category;
            _productService = product;
            _orderService = order;
            _subcategoryService = subcategoryService;

        }
        public async Task<IActionResult> Index()
        {
            var listOrder = await _orderService.GetOrders();
            var homeVM = new HomeVM
            {
                TotalCategory = await _categoryService.GetCategoryCountAsync(),
                TotalSubCategory = await _subcategoryService.GetSubCategoryCountAsync(),
                TotalProducts = await _productService.GetProductsCountAsync(),
                TotalNewProducts = await _productService.GetNewProductsCountAsync(),
                TotalSaleProducts = await _productService.GetSaleProductsCountAsync(),
                TotalFeatureProducts = await _productService.GetFeatureProductsCountAsync(),
                Orders = listOrder.OrderByDescending(x => x.ID).Take(10),
                TotalApproveOrders = await _orderService.GetAcceptsOrdersCountAsync(),
                TotalPendingOrders = await _orderService.GetPendingOrdersCountAsync(),
                TotalRejectedOrders = await _orderService.GetRejectedOrdersCountAsync(),
                TotalPaid = await _orderService.GetPaidCountAsync(),
                TotalUnPaid = await _orderService.GetUnPaidCountAsync()
                
        };

            return View(homeVM);
        }
        public IActionResult test()
        {
            return View();
        }
    }
}
