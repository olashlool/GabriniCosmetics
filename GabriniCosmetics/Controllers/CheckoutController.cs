using GabriniCosmetics.Areas.Admin.Models.Interface;
using GabriniCosmetics.Areas.Admin.Models;
using GabriniCosmetics.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GabriniCosmetics.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using GabriniCosmetics.Models.Services;
using GabriniCosmetics.Areas.Admin.Models.Services;

namespace GabriniCosmetics.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICartService _shop;
        private readonly IOrder _order;
        private readonly IProduct _product;
        private readonly IAddressService _addresses;
        private readonly ICategory _categoryService;
        private readonly ISubcategory _subCategoryService;

        public CheckoutController(UserManager<ApplicationUser> userManager, ICartService shop, IOrder order, IProduct product, IAddressService addresses, ICategory category, ISubcategory subcategory)
        {
            _userManager = userManager;
            _shop = shop;
            _order = order;
            _product = product;
            _addresses = addresses;
            _categoryService = category;
            _subCategoryService = subcategory;
        }
        [Authorize]
        public async Task<IActionResult> Completed(int order)
        {
            var orderById = await _order.GetOrderByOrderId(order);
            return View(orderById);
        }

        public async Task<IActionResult> Index()
        {
            var checkoutVM = new CheckoutVM();
            var user = await _userManager.GetUserAsync(User);
            checkoutVM.Addresses = await _addresses.GetAddressByUserIdAsync(user.Id);
            checkoutVM.ShoppingCart = await _shop.GetUserCart();

            foreach (var item in checkoutVM.ShoppingCart.CartDetails)
            {
                var subCategory = await _subCategoryService.GetSubcategoryById(item.Products.SubcategoryId);
                var category = await _categoryService.GetCategoryById(subCategory.CategoryId);
                item.Products.Subcategory = new Subcategory
                {
                    Id = subCategory.Id,
                    NameAr = subCategory.NameAr,
                    NameEn = subCategory.NameEn,
                    CategoryId = category.Id,
                    Category = new Category
                    {
                        NameEn = category.NameEn,
                        NameAr = category.NameAr,
                        ImageUpload = category.ImageUpload,
                    }
                };
            }
            return View(checkoutVM);
        }

        [HttpPost]
        [Authorize]

        public async Task<IActionResult> Index(CheckoutVM checkoutInput)
        {
            var user = await _userManager.GetUserAsync(User);
            double? totalPrice = checkoutInput.TotalPrice;

            if (checkoutInput.Order.FullLocation != "0")
            {
                var getFullAddress = await _addresses.GetAddressByIdAsync(Convert.ToInt32(checkoutInput.Order.FullLocation));
                Order order = new Order
                {
                    UserID = user.Id,
                    FirstName = getFullAddress.FirstName,
                    LastName = getFullAddress.LastName,
                    Address = getFullAddress.Address1,
                    Address2 = getFullAddress.Address2,
                    State = getFullAddress.City,
                    City = getFullAddress.City,
                    Country = getFullAddress.Country,
                    Email = getFullAddress.Email,
                    Fax = getFullAddress.FaxNumber,
                    Phone = getFullAddress.PhoneNumber,
                    Zip = checkoutInput.Order.Zip,
                    Timestamp = checkoutInput.Order.Timestamp + ";",
                    OrderStatus = checkoutInput.Order.OrderStatus,
                    PaymentMethod = checkoutInput.Order.PaymentMethod,
                    PaymentStstus = checkoutInput.Order.PaymentStstus
                };

                var createdOrder = await _order.CreateOrder(order);
                var latestOrder = await _order.GetLatestOrderForUser(user.Id);

                IEnumerable<CartDetail> cartItems = await _shop.GetCartProductByUserId(user.Id);
                IList<OrderItems> orderItems = new List<OrderItems>();
                double total = 0;

                foreach (var cartItem in cartItems)
                {
                    OrderItems orderItem = new OrderItems
                    {
                        OrderID = latestOrder.ID,
                        ProductID = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        ImageProduct = cartItem.Image,
                        TotalPrice = totalPrice,
                        DiscountCode = checkoutInput.DiscountCode,
                    };
                    orderItems.Add(orderItem);
                    var product = await _product.GetProductById(cartItem.ProductId);
                    total += product.Product.Price * cartItem.Quantity;
                }
                await _order.CreateOrderItems(orderItems);
                await _shop.RemoveCartProducts(cartItems);

                return RedirectToAction("Completed", "Checkout", new { order = createdOrder.ID });
            }
            else if (checkoutInput.Order.FirstName != null && checkoutInput.Order.LastName != null && checkoutInput.Order.Address != null && checkoutInput.Order.City != null && checkoutInput.Order.Email != null && checkoutInput.Order.Phone != null)
            {
                Order order = new Order
                {
                    UserID = user.Id,
                    FirstName = checkoutInput.Order.FirstName,
                    LastName = checkoutInput.Order.LastName,
                    Address = checkoutInput.Order.Address,
                    Address2 = checkoutInput.Order.Address2,
                    State = checkoutInput.Order.City,
                    City = checkoutInput.Order.City,
                    Country = checkoutInput.Order.Country,
                    Email = checkoutInput.Order.Email,
                    Fax = checkoutInput.Order.Fax,
                    Phone = checkoutInput.Order.Phone,
                    Zip = checkoutInput.Order.Zip,
                    Timestamp = checkoutInput.Order.Timestamp,
                    OrderStatus = checkoutInput.Order.OrderStatus,
                    PaymentMethod = checkoutInput.Order.PaymentMethod,
                    PaymentStstus = checkoutInput.Order.PaymentStstus
                };

                var createdOrder = await _order.CreateOrder(order);
                var latestOrder = await _order.GetLatestOrderForUser(user.Id);

                IEnumerable<CartDetail> cartItems = await _shop.GetCartProductByUserId(user.Id);
                IList<OrderItems> orderItems = new List<OrderItems>();
                double total = 0;

                foreach (var cartItem in cartItems)
                {
                    OrderItems orderItem = new OrderItems
                    {
                        OrderID = latestOrder.ID,
                        ProductID = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        ImageProduct = cartItem.Image
                    };
                    orderItems.Add(orderItem);
                    var product = await _product.GetProductById(cartItem.ProductId);
                    total += product.Product.Price * cartItem.Quantity;
                }

                double finalCost = total * 1.1;

                await _order.CreateOrderItems(orderItems);
                await _shop.RemoveCartProducts(cartItems);

                return RedirectToAction("Completed", "Checkout", new { order = createdOrder.ID });
            }
            return View();
        }
    }
}
