using GabriniCosmetics.Areas.Admin.Models.DTOs;
using GabriniCosmetics.Areas.Admin.Models.Interface;
using GabriniCosmetics.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GabriniCosmetics.Areas.Admin.Models.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly GabriniCosmeticsContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProduct _product;
        private readonly ISubcategory _subcategory;
        private readonly ICategory _category;


        public WishlistService(GabriniCosmeticsContext db, IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager, IProduct product, ISubcategory subcategory, ICategory category)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _product = product;
            _subcategory = subcategory;
            _category = category;
        }

        public async Task<int> AddItem(int productId)
        {
            string userId = GetUserId();
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null.");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var wishlist = await GetOrCreateWishlist(userId);

                var productDTO = await _product.GetProductById(productId);
                var product = new Product()
                {
                    Id = productDTO.Product.Id,
                    NameEn = productDTO.Product.NameEn,
                    NameAr = productDTO.Product.NameAr,
                    DescriptionEn = productDTO.Product.DescriptionEn,
                    DescriptionAr = productDTO.Product.DescriptionAr,

                    Colors = productDTO.Product.Colors,
                    Flags = productDTO.Product.Flags,
                    Images = productDTO.Product.Images,

                    Subcategory = productDTO.Product.Subcategory,
                    SubcategoryId = productDTO.Product.SubcategoryId,

                    Price = productDTO.Product.Price,
                    PriceAfterDiscount = productDTO.Product.PriceAfterDiscount,
                    PersantageSale = productDTO.Product.PersantageSale
                };

                await UpdateWishlistItem(productId, wishlist, 1, productDTO.Images.First().ImagePath, null);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Handle the exception appropriately (e.g., log or throw)
            }

            var wishlistItemCount = await GetWishlistItemCount();
            return wishlistItemCount;
        }
        public async Task<int> AddItem(int productId, int qty, string img, string colorName)
        {
            string userId = GetUserId();
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged in");

                var wishlist = await GetOrCreateWishlist(userId);
                await UpdateWishlistItem(productId, wishlist, qty, img, colorName);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Handle the exception appropriately (e.g., log or throw)
            }

            var wishlistItemCount = await GetWishlistItemCount();
            return wishlistItemCount;
        }

        public async Task<int> RemoveItem(int productId, string img)
        {
            string userId = GetUserId();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged in");

                var wishlist = await GetWishlist(userId);
                if (wishlist == null)
                    throw new Exception("Wishlist not found");

                var wishlistItem = wishlist.WishlistsDetail.Where(a => a.ProductId == productId && a.Image == img).FirstOrDefault();
                if (wishlistItem != null)
                {
                    if (wishlistItem.Quantity > 1)
                        wishlistItem.Quantity -= 1;
                    else
                        _db.WishlistDetail.Remove(wishlistItem);
                }

                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Handle the exception appropriately (e.g., log or throw)
            }

            var wishlistItemCount = await GetWishlistItemCount();
            return wishlistItemCount;
        }

        public async Task<Wishlist> GetUserWishlist()
        {
            string userId = GetUserId();
            if (userId == null)
            {
                // Handle the case where userId is null (e.g., redirect to login or show an error message)
                // For now, let's return an empty wishlist or some default response
                var emptyWishlist = new Wishlist(); // You need to replace this with your actual Wishlist class
                return emptyWishlist;
            }
            else
            {
                var wishlist = await GetOrCreateWishlist(userId);
                return wishlist;
            }
        }

        public async Task<IEnumerable<WishlistDetail>> GetWishlistProductByUserId(string userId)
        {
            var wishlist = await GetWishlist(userId);
            if (wishlist == null)
                throw new Exception("Invalid wishlist");

            return wishlist.WishlistsDetail.ToList();
        }

        public async Task RemoveWishlistProducts(IEnumerable<WishlistDetail> wishlistProducts)
        {
            _db.WishlistDetail.RemoveRange(wishlistProducts);
            await _db.SaveChangesAsync();
        }

        public async Task<Wishlist> GetWishlist(string userId)
        {
            return await _db.Wishlists
                .Include(w => w.WishlistsDetail)
                    .ThenInclude(d => d.Products)
                        .ThenInclude(p => p.Subcategory)
                        .ThenInclude(p => p.Category)
                .Include(w => w.WishlistsDetail)
                    .ThenInclude(d => d.Products)
                        .ThenInclude(p => p.Flags)
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task<int> GetWishlistItemCount()
        {
            string userId = GetUserId();
            var wishlist = await GetWishlist(userId);
            return wishlist?.WishlistsDetail.Sum(d => d.Quantity) ?? 0;
        }

        private async Task<Wishlist> GetOrCreateWishlist(string userId)
        {
            // Fetch existing wishlist
            var wishlist = await GetWishlist(userId);

            // If no existing wishlist is found, create a new one
            if (wishlist == null)
            {
                wishlist = new Wishlist
                {
                    UserId = userId,
                    // Initialize WishlistsDetail as an empty list
                    WishlistsDetail = new List<WishlistDetail>()
                };
                // Add the new wishlist to the database
                _db.Wishlists.Add(wishlist);
                await _db.SaveChangesAsync();
            }

            return wishlist;
        }

        private async Task UpdateWishlistItem(int productId, Wishlist wishlist, int qty, string img, string colorName)
        {
            if (string.IsNullOrEmpty(img))
            {
                throw new ArgumentNullException(nameof(img), "Image URL cannot be null or empty.");
            }

            var wishlistItem = wishlist.WishlistsDetail.Where(x => x.Image == img).FirstOrDefault();
            if (wishlistItem != null)
            {
                if (wishlistItem.Image != img)
                {
                    var productDTO = await _product.GetProductById(productId);
                    var product = new Product()
                    {
                        Id = productDTO.Product.Id,
                        NameEn = productDTO.Product.NameEn,
                        NameAr = productDTO.Product.NameAr,
                        DescriptionEn = productDTO.Product.DescriptionEn,
                        DescriptionAr = productDTO.Product.DescriptionAr,

                        Colors = productDTO.Product.Colors,
                        Flags = productDTO.Product.Flags,
                        Images = productDTO.Product.Images,

                        Subcategory = productDTO.Product.Subcategory,
                        SubcategoryId = productDTO.Product.SubcategoryId,
                        Price = productDTO.Product.Price,
                        PriceAfterDiscount = productDTO.Product.PriceAfterDiscount,
                        PersantageSale = productDTO.Product.PersantageSale
                    };

                    // Attach the existing product instance if it's already being tracked
                    var trackedProduct = _db.Products.Local.FirstOrDefault(p => p.Id == product.Id);
                    if (trackedProduct != null)
                    {
                        _db.Entry(trackedProduct).State = EntityState.Detached;
                    }

                    _db.Attach(product);

                    wishlistItem = new WishlistDetail
                    {
                        ProductId = productId,
                        WishlistId = wishlist.Id,
                        Quantity = qty,
                        Image = img ?? wishlistItem.Image,
                        Products = product,
                        Wishlist = wishlist,
                        UnitPrice = product?.Price ?? 0,
                        ColorName = colorName
                    };


                    _db.WishlistDetail.Add(wishlistItem);
                }
                else
                {
                    wishlistItem.Quantity += qty;
                }
            }
            else
            {
                try
                {
                    var productDTO = await _product.GetProductById(productId);
                    var product = new Product()
                    {
                        Id = productDTO.Product.Id,
                        NameEn = productDTO.Product.NameEn,
                        NameAr = productDTO.Product.NameAr,
                        DescriptionEn = productDTO.Product.DescriptionEn,
                        DescriptionAr = productDTO.Product.DescriptionAr,
                        Colors = productDTO.Product.Colors,
                        Flags = productDTO.Product.Flags,
                        Images = productDTO.Product.Images,
                        Subcategory = productDTO.Product.Subcategory,
                        SubcategoryId = productDTO.Product.SubcategoryId,
                        Price = productDTO.Product.Price,
                        PriceAfterDiscount = productDTO.Product.PriceAfterDiscount,
                        PersantageSale = productDTO.Product.PersantageSale
                    };

                    // Attach the existing product instance if it's already being tracked
                    var trackedProduct = _db.Products.Local.FirstOrDefault(p => p.Id == product.Id);
                    if (trackedProduct != null)
                    {
                        _db.Entry(trackedProduct).State = EntityState.Detached;
                    }

                    _db.Attach(product);

                    wishlistItem = new WishlistDetail
                    {
                        ProductId = productId,
                        WishlistId = wishlist.Id,
                        Quantity = qty,
                        Products = product,
                        Image = img ?? product?.Images.First().ImagePath,
                        Wishlist = wishlist,
                        UnitPrice = product?.Price ?? 0,
                        ColorName = colorName
                    };
                    _db.WishlistDetail.Add(wishlistItem);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            await _db.SaveChangesAsync();
        }


        private string GetUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var user = httpContext?.User;

            return user?.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }

}
