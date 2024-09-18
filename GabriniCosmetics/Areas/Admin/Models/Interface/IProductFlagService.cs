namespace GabriniCosmetics.Areas.Admin.Models.Interface
{
    public interface IProductFlagService
    {
        Task AddFlagsAsync(Product product, List<int> flags);
    }
}
