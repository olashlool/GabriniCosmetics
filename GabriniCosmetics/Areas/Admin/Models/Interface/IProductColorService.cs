namespace GabriniCosmetics.Areas.Admin.Models.Interface
{
    public interface IProductColorService
    {
        Task AddColorsAsync(Product product, List<string> colors);
        Task RemoveColorsAsync(Product product, List<string> colorNames);
    }
}
