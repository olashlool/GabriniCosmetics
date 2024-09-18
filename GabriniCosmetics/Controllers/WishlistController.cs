using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GabriniCosmetics.Areas.Admin.Models.Interface;
using GabriniCosmetics.Areas.Admin.Models;

[Authorize]
public class WishlistController : Controller
{
    private readonly IWishlistService _wishlistService;
    private readonly ICategory _categoryService;
    private readonly ISubcategory _subCategoryService;

    public WishlistController(IWishlistService wishlistService, ICategory categoryService, ISubcategory subCategoryService)
    {
        _wishlistService = wishlistService;
        _categoryService = categoryService;
        _subCategoryService = subCategoryService;
    }
    [Authorize]
    public async Task<IActionResult> AddItem(int productId, int qty = 1, int redirect = 1)
    {
        var wishlistCount = await _wishlistService.AddItem(productId);
        if (redirect == 0)
            return Ok(wishlistCount);

        string returnUrl = Request.Headers["Referer"].ToString() ?? "/";
        return Redirect(returnUrl);
    }

    [Authorize]
    public async Task<IActionResult> AddItemWithQtyAndImage(int productId, int qty, string image, string color)
    {
        color = color.Trim('$');
        color = color.Trim('{');
        color = color.Trim('}');
        if (qty != 0 && image != null)
        {
            var wishlistCount = await _wishlistService.AddItem(productId, qty, image, "#" + color);
        }
        string returnUrl = Request.Headers["Referer"].ToString() ?? "/";
        return Redirect(returnUrl);
    }

    public async Task<IActionResult> RemoveItem(int productId, string img)
    {
        var wishlistCount = await _wishlistService.RemoveItem(productId, img);
        string returnUrl = Request.Headers["Referer"].ToString() ?? "/";
        return Redirect(returnUrl);
    }
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var wishlist = await _wishlistService.GetUserWishlist();
        foreach (var item in wishlist.WishlistsDetail)
        {
            var subCategory = await _subCategoryService.GetSubcategoryById(item.Products.SubcategoryId);
            var category = await _categoryService.GetCategoryById(item.Products.Subcategory.CategoryId);
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
        return View(wishlist);
    }

    public async Task<IActionResult> GetTotalItemInWishlist()
    {
        int wishlistItemCount = await _wishlistService.GetWishlistItemCount();
        return Ok(wishlistItemCount);
    }
    // Example action to display wishlist items by user
    public async Task<IActionResult> GetWishlistItemsByUser(string userId)
    {
        var wishlistItems = await _wishlistService.GetWishlistProductByUserId(userId);
        return View(wishlistItems);
    }
}
