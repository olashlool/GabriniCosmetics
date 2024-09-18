using GabriniCosmetics.Areas.Admin.Models.Interface;
using Microsoft.AspNetCore.Mvc;

namespace GabriniCosmetics.Areas.Admin.Models.Components
{
    [ViewComponent(Name = "SmallCart")]
    public class SmallCartViewComponent : ViewComponent
    {
        private readonly ICartService _cartRepo;

        public SmallCartViewComponent(ICartService cartRepo)
        {
            _cartRepo = cartRepo;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cart = await _cartRepo.GetUserCart();
            return View(cart);
        }
    }
}
