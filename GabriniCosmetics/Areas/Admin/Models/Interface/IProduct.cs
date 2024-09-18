using GabriniCosmetics.Areas.Admin.Models.DTOs;

namespace GabriniCosmetics.Areas.Admin.Models.Interface
{
    public interface IProduct
    {
        Task<List<ProductDTO>> GetProducts();
        Task<ProductDTO> CreateProduct(CreateProductDTO createProductDto);
        Task<ProductDTO> GetProductById(int id);
        Task<List<ProductDTO>> GetProductBySubCategory(int id);
        Task<ProductDTO> UpdateProduct(UpdateProductDTO updateProductDto);
        Task<bool> DeleteProduct(int id);
        IQueryable<Product> Queryable();
        Task<int> GetProductsCountAsync();
        Task<int> GetNewProductsCountAsync();
        Task<int> GetSaleProductsCountAsync();
        Task<int> GetFeatureProductsCountAsync();
        Task<List<Product>> GetProductsByIdsAsync(List<int> productIds);

    }
}
