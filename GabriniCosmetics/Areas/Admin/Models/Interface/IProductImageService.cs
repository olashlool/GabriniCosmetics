namespace GabriniCosmetics.Areas.Admin.Models.Interface
{
    public interface IProductImageService
    {
        Task AddImagesAsync(Product product, List<IFormFile> imageUploads);
        Task RemoveImagesAsync(int productId, List<string> imagePath);
    }
}
