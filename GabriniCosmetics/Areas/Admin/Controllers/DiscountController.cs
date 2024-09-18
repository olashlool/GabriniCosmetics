using GabriniCosmetics.Areas.Admin.Models.Interface;
using GabriniCosmetics.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using GabriniCosmetics.Areas.Admin.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using GabriniCosmetics.Areas.Admin.Models.Services;

namespace GabriniCosmetics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DiscountController : Controller
    {
        private readonly IDiscountService _discountService;

        public DiscountController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Discount> discounts = await _discountService.GetAllDiscountsAsync();
            return View(discounts);
        }

        public IActionResult Create()
        {
            return View();
        }

        // POST: Discount/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiscountViewModel model)
        {
            if (ModelState.IsValid)
            {
                var discount = new Discount
                {
                    Code = model.Code,
                    Percentage = model.Percentage,
                    ValidFrom = model.ValidFrom,
                    ValidTo = model.ValidTo
                };

                await _discountService.AddDiscountAsync(discount);
                return Redirect("/Admin/Discount");
            }

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Fetch the discount by ID
            var discount = await _discountService.GetDiscountById(id);
            if (discount == null)
            {
                return NotFound();
            }

            // Map discount to EditDiscountDTO
            var model = new Discount
            {
                 Id = discount.Id,
                Code = discount.Code,
                Percentage = discount.Percentage,
                ValidFrom = discount.ValidFrom,
                ValidTo = discount.ValidTo
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Discount model)
        {
            if (ModelState.IsValid)
            {
                // Update the discount
                await _discountService.UpdateDiscount(model);
                return Redirect("/Admin/Discount");
            }

            return View(model); // Return the same view with validation errors if the model is invalid
        }


        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            var success = await _discountService.Delete(id);
            if (!success)
            {
                return NotFound();
            }
            return Ok();
        }

        public async Task<IActionResult> ApplyDiscount([FromBody] DiscountRequest request)
        {
            var result = await _discountService.ApplyDiscountAsync(request.Code, request.OriginalPrice);
            if (result.IsSuccess)
            {
                return Ok(new { DiscountedPrice = result.DiscountedPrice });
            }
            return BadRequest("Invalid discount code");
        }
    }

    public class DiscountRequest
    {
        public string Code { get; set; }
        public decimal OriginalPrice { get; set; }
    }
}
