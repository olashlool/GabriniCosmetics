using GabriniCosmetics.Areas.Admin.Models;
using GabriniCosmetics.Areas.Admin.Models.Interface;
using GabriniCosmetics.Areas.Admin.Models.Services;
using GabriniCosmetics.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace GabriniCosmetics.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartRepo;
        private readonly GabriniCosmeticsContext _db;
        private readonly ISubcategory _subCategoryService;
        private readonly ICategory _categoryService;


        public CartController(ICartService cartRepo, GabriniCosmeticsContext gabriniCosmeticsContext, ICategory categoryService, ISubcategory subCategoryService)
        {
            _cartRepo = cartRepo;
            _db = gabriniCosmeticsContext;
            _categoryService = categoryService;
            _subCategoryService = subCategoryService;
        }
        [Authorize]
        public async Task<IActionResult> AddItem(int productId, string img, int qty = 1, int redirect = 1)
        {
            var cartCount = await _cartRepo.AddItem(productId, img);
            if (redirect == 0)
                return Ok(cartCount);
            string returnUrl = Request.Headers["Referer"].ToString() ?? "/";
            return Redirect(returnUrl);
        }
        [Authorize]
        public async Task<IActionResult> AddItemWithQtyAndImage(int productId, int qty, string image , string color)
        {
            color = color.Trim('$');
            color = color.Trim('{');
            color = color.Trim('}');
            if (qty != 0 && image != null)
            {
                var cartCount = await _cartRepo.AddItem(productId, qty, image, "#" + color);
            }
            string returnUrl = Request.Headers["Referer"].ToString() ?? "/";
            return Redirect(returnUrl);
        }
        public async Task<IActionResult> RemoveItem(int productId, string img)
        {
            var cartCount = await _cartRepo.RemoveItem(productId, img);
            string returnUrl = Request.Headers["Referer"].ToString() ?? "/";
            return Redirect(returnUrl);
        }
        [Authorize]
        public async Task<IActionResult> index()
        {
            var cart = await _cartRepo.GetUserCart();
            foreach (var item in cart.CartDetails)
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
            return View(cart);
        }
        public async Task<IActionResult> GetTotalItemInCart()
        {
            int cartItem = await _cartRepo.GetCartItemCount();
            return Ok(cartItem);
        }
        [HttpPost]
        public async Task<IActionResult> AddSelectedToCart([FromBody] SelectedItemsDto selectedItemsDto)
        {
            var selectedItems = selectedItemsDto.SelectedItems;
            foreach (var itemId in selectedItems)
            {
                var wishlistItem = await _db.WishlistDetail.Include(w => w.Products).FirstOrDefaultAsync(w => w.Id == itemId);
                if (wishlistItem != null)
                {
                    await _cartRepo.AddItem(wishlistItem.ProductId, wishlistItem.Quantity, wishlistItem.Image, wishlistItem.ColorName);
                }
            }
            return Redirect("/Cart");
        }
    }
    public class SelectedItemsDto
    {
        public int[] SelectedItems { get; set; }
    }
}
