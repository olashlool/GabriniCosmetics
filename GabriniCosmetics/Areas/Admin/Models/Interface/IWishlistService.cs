namespace GabriniCosmetics.Areas.Admin.Models.Interface
{
    public interface IWishlistService
    {
        Task<int> AddItem(int productId);
        Task<int> AddItem(int productId, int qty, string img, string colorName);
        Task<int> RemoveItem(int productId, string img);
        Task<Wishlist> GetUserWishlist();
        Task<IEnumerable<WishlistDetail>> GetWishlistProductByUserId(string userId);
        Task RemoveWishlistProducts(IEnumerable<WishlistDetail> wishlistProducts);
        Task<Wishlist> GetWishlist(string userId);
        Task<int> GetWishlistItemCount();

    }
}
