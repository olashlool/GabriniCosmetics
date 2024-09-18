using GabriniCosmetics.Areas.Admin.Models.Interface;
using GabriniCosmetics.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace GabriniCosmetics.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrder _order;
        private readonly ICartService _cartRepo;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderController(IOrder order, ICartService cart, IHttpContextAccessor httpContextAccessor)
        {
            _order = order;
            _cartRepo = cart;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IActionResult> Index()
        {
            var orderVM = new OrderVM();
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault().Value;
            var orders = await _order.GetOrdersByUserId(userId);
            orderVM.Orders = orders.OrderByDescending(x => x.ID);
            orderVM.OrderItems = await _order.GetOrderItems();
            return View(orderVM);
        }
        public async Task<IActionResult> Detail(int id)
        {
            var orderDetailVM = new OrderDetailVM();
            orderDetailVM.OrderItems = await _order.GetOrderItemsByOrderId(id);

            orderDetailVM.OrderItems.First().Order = await _order.GetOrderByOrderId(orderDetailVM.OrderItems.First().OrderID);

            return View(orderDetailVM);
        }
    }
}
