namespace GabriniCosmetics.Areas.Admin.Models.Interface
{
    public interface ICartService
    {
        public Task<int> AddItem(int productId, string img);
        public Task<int> AddItem(int productId, int qty, string img, string color);
        public Task<int> RemoveItem(int bookId, string img);
        public Task<ShoppingCart> GetUserCart();
        public Task<ShoppingCart> GetCart(string userId);
        public Task<int> GetCartItemCount();
        public Task<IEnumerable<CartDetail>> GetCartProductByUserId(string userId);
        public Task RemoveCartProducts(IEnumerable<CartDetail> cartProduct);

    }
}
