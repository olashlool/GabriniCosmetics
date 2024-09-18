using GabriniCosmetics.Areas.Admin.Models.Interface;
using GabriniCosmetics.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GabriniCosmetics.Areas.Admin.Models.Services
{
    public class CartService :  ICartService
    {
        private readonly GabriniCosmeticsContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISubcategory _subcategory;
        private readonly ICategory _category;

        public CartService(GabriniCosmeticsContext db, IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager, ISubcategory subcategory, ICategory category)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _subcategory = subcategory;
            _category = category;
        }

        public async Task<int> AddItem(int productId, string img)
        {
            string userId = GetUserId();

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged in");

                var cart = await GetOrCreateCart(userId);

                var cartItem = cart.CartDetails.FirstOrDefault(a => a.ProductId == productId && a.Image == img);
                if (cartItem != null)
                {
                    cartItem.Quantity += 1;
                }
                else
                {
                    var product = await _db.Products.FindAsync(productId);
                    cartItem = new CartDetail
                    {
                        ProductId = productId,
                        ShoppingCartId = cart.Id,
                        Quantity = 1,
                        Products = product,
                        Image = product.Images.FirstOrDefault().ImagePath,
                        ShoppingCart = cart,
                        UnitPrice = product.Price
                    };
                    _db.CartDetails.Add(cartItem);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Handle the exception appropriately (e.g., log or throw)
            }

            var cartItemCount = await GetCartItemCount();
            return cartItemCount;
        }
        public async Task<int> AddItem(int productId, int qty, string img, string color)
        {
            string userId = GetUserId();

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged in");

                var cart = await GetOrCreateCart(userId);
                await UpdateCartItem(productId, cart, qty, img, color);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Handle the exception appropriately (e.g., log or throw)
            }

            var cartItemCount = await GetCartItemCount();
            return cartItemCount;
        }
        public async Task<int> RemoveItem(int bookId, string img)
        {
            string userId = GetUserId();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged in");

                var cart = await GetCart(userId);
                if (cart == null)
                    throw new Exception("Cart not found");

                //var cartItem = cart.CartDetails.FirstOrDefault(a => a.ProductId == bookId);
                var cartItem = cart.CartDetails.Where(a => a.ProductId == bookId && a.Image == img).FirstOrDefault();

                if (cartItem != null)
                {
                    if (cartItem.Quantity > 1)
                        cartItem.Quantity -= 1;
                    else
                        _db.CartDetails.Remove(cartItem);
                }

                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Handle the exception appropriately (e.g., log or throw)
            }

            var cartItemCount = await GetCartItemCount();
            return cartItemCount;
        }
        public async Task<ShoppingCart> GetUserCart()
        {
            string userId = GetUserId();
            if (userId == null)
            {
                // Handle the case where userId is null (e.g., redirect to login or show an error message)
                // For now, let's return an empty cart or some default response
                var emptyCart = new ShoppingCart(); // You need to replace this with your actual Cart class
                return emptyCart;
            }
            else
            {
                var cart = await GetOrCreateCart(userId);
                return cart;
            }
        }
        public async Task<IEnumerable<CartDetail>> GetCartProductByUserId(string userId)
        {
            var cart = await GetCart(userId);
            if (cart is null)
                throw new Exception("Invalid cart");
            return cart.CartDetails.ToList();
        }
        public async Task RemoveCartProducts(IEnumerable<CartDetail> cartProduct)
        {
            _db.CartDetails.RemoveRange(cartProduct);
            await _db.SaveChangesAsync();
        }
        public async Task<ShoppingCart> GetCart(string userId)
        {
            return await _db.ShoppingCarts.Include(c => c.CartDetails)
                .ThenInclude(d => d.Products).ThenInclude(x => x.Flags)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }
        public async Task<int> GetCartItemCount()
        {
            string userId = GetUserId();
            var cart = await GetCart(userId);
            return cart?.CartDetails.Sum(d => d.Quantity) ?? 0;
        }
        private async Task<ShoppingCart> GetOrCreateCart(string userId)
        {
            var cart = await GetCart(userId);

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserId = userId,
                    CartDetails = new List<CartDetail>()
                };
                _db.ShoppingCarts.Add(cart);
                await _db.SaveChangesAsync();
            }
            return cart;
        }
        private string GetUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var user = httpContext?.User;

            return user?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private async Task UpdateCartItem(int productId, ShoppingCart cart, int qty, string img, string color)
        {
            if (string.IsNullOrEmpty(img))
            {
                throw new ArgumentNullException(nameof(img), "Image URL cannot be null or empty.");
            }

            var cartItem = cart.CartDetails.Where(x => x.Image == img).FirstOrDefault();
            if (cartItem != null)
            {
                if (cartItem.Image != img)
                {
                    var product = await _db.Products.FindAsync(productId);

                    // Detach the existing product instance if it's already being tracked
                    var trackedProduct = _db.Products.Local.FirstOrDefault(p => p.Id == productId);
                    if (trackedProduct != null)
                    {
                        _db.Entry(trackedProduct).State = EntityState.Detached;
                    }

                    _db.Attach(product);

                    cartItem = new CartDetail
                    {
                        ProductId = productId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty,
                        Image = img ?? cartItem.Image,
                        Products = product,
                        ShoppingCart = cart,
                        UnitPrice = product?.Price ?? 0,
                        ColorName = color
                    };

                    _db.CartDetails.Add(cartItem);
                }
                else
                {
                    cartItem.Quantity += qty;
                }
            }
            else
            {
                var product = await _db.Products.FindAsync(productId);

                // Detach the existing product instance if it's already being tracked
                var trackedProduct = _db.Products.Local.FirstOrDefault(p => p.Id == productId);
                if (trackedProduct != null)
                {
                    _db.Entry(trackedProduct).State = EntityState.Detached;
                }

                _db.Attach(product);

                cartItem = new CartDetail
                {
                    ProductId = productId,
                    ShoppingCartId = cart.Id,
                    Quantity = qty,
                    Products = product,
                    Image = img ?? product?.Images.FirstOrDefault()?.ImagePath,
                    ShoppingCart = cart,
                    UnitPrice = product?.Price ?? 0,
                    ColorName = color
                };

                _db.CartDetails.Add(cartItem);
            }

            await _db.SaveChangesAsync();
        }

    }
}
