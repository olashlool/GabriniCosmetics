using GabriniCosmetics.Areas.Admin.Models.DTOs;
using GabriniCosmetics.Areas.Admin.Models.Interface;
using GabriniCosmetics.Data;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace GabriniCosmetics.Areas.Admin.Models.Services
{
    public class ProductService : IProduct
    {
        private readonly GabriniCosmeticsContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProductService> _logger;
        private readonly IProductImageService _productImageService;
        private readonly IProductColorService _productColorService;
        private readonly IProductFlagService _productFlagService;

        public ProductService(GabriniCosmeticsContext context, IWebHostEnvironment environment, ILogger<ProductService> logger,
                              IProductImageService productImageService, IProductColorService productColorService, IProductFlagService productFlagService)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
            _productImageService = productImageService;
            _productColorService = productColorService;
            _productFlagService = productFlagService;
        }

        public async Task<List<ProductDTO>> GetProducts()
        {
            try
            {
                return await _context.Products
                    .Include(p => p.Images)
                    .Include(p => p.Colors)
                    .Include(p => p.Flags)
                    .Select(p => new ProductDTO
                    {
                        Product = p,
                        Images = p.Images.ToList(),
                        Colors = p.Colors.ToList(),
                        Flags = p.Flags.ToList()
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                throw;
            }
        }

        public async Task<ProductDTO> GetProductById(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Images)
                    .Include(p => p.Colors)
                    .Include(p => p.Flags)
                    .Select(p => new ProductDTO
                    {
                        Product = p,
                        Images = p.Images.ToList(),
                        Colors = p.Colors.ToList(),
                        Flags = p.Flags.ToList()
                    })
                    .FirstOrDefaultAsync(p => p.Product.Id == id);

                return product ?? throw new KeyNotFoundException($"Product with id {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting product by id {id}");
                throw;
            }
        }
        public async Task<List<ProductDTO>> GetProductBySubCategory(int id)
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Images)
                    .Include(p => p.Colors)
                    .Include(p => p.Flags)
                    .Where(p => p.SubcategoryId == id)
                    .Select(p => new ProductDTO
                    {
                        Product = p,
                        Images = p.Images.ToList(),
                        Colors = p.Colors.ToList(),
                        Flags = p.Flags.ToList()
                    })
                    .ToListAsync();

                if (!products.Any())
                {
                    throw new KeyNotFoundException($"No products found for subcategory id {id}");
                }

                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting products for subcategory id {id}");
                throw;
            }
        }

        public async Task<ProductDTO> CreateProduct(CreateProductDTO createProductDto)
        {
            try
            {
                var product = new Product
                {
                    NameEn = createProductDto.NameEn,
                    NameAr = createProductDto.NameAr,
                    DescriptionEn = createProductDto.DescriptionEn,
                    DescriptionAr = createProductDto.DescriptionAr,
                    Price = createProductDto.Price,
                    PriceAfterDiscount = createProductDto.PriceAfterDiscount,
                    PersantageSale = createProductDto.PersantageSale,
                    SubcategoryId = createProductDto.SubcategoryId,
                    Images = new List<ProductImage>(),
                    Colors = new List<ProductColor>(),
                    Flags = new List<ProductFlag>(),
                    IsDealOfDay = createProductDto.IsDealOfDay,
                    IsSale = createProductDto.IsSale,
                    FlagNamesString = createProductDto.FlagNamesString,
                    IsAvailability = createProductDto.IsAvailability
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                if (createProductDto.ImageUploads != null && createProductDto.ImageUploads.Any())
                {
                    await _productImageService.AddImagesAsync(product, createProductDto.ImageUploads);
                }

                if (createProductDto.Colors != null && createProductDto.Colors.Any())
                {
                    await _productColorService.AddColorsAsync(product, createProductDto.Colors);
                }

                if (createProductDto.Flags != null && createProductDto.Flags.Any())
                {
                    await _productFlagService.AddFlagsAsync(product, createProductDto.Flags);
                }

                return new ProductDTO
                {
                    Product = product,
                    Images = product.Images.ToList(),
                    Colors = product.Colors.ToList(),
                    Flags = product.Flags.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                throw;
            }
        }

        public async Task<ProductDTO> UpdateProduct(UpdateProductDTO updateProductDto)
        {
            try
            {
                var existingProduct = await _context.Products
                    .Include(p => p.Images)
                    .Include(p => p.Colors)
                    .Include(p => p.Flags)
                    .FirstOrDefaultAsync(p => p.Id == updateProductDto.Id);

                if (existingProduct == null)
                {
                    throw new KeyNotFoundException($"Product with id {updateProductDto.Id} not found.");
                }

                existingProduct.NameEn = updateProductDto.NameEn;
                existingProduct.NameAr = updateProductDto.NameAr;
                existingProduct.DescriptionEn = updateProductDto.DescriptionEn;
                existingProduct.DescriptionAr = updateProductDto.DescriptionAr;
                existingProduct.Price = updateProductDto.Price;
                existingProduct.PriceAfterDiscount = updateProductDto.PriceAfterDiscount;
                existingProduct.PersantageSale = updateProductDto.PersantageSale;
                existingProduct.SubcategoryId = updateProductDto.SubcategoryId;
                existingProduct.IsDealOfDay = updateProductDto.IsDealOfDay;
                existingProduct.IsSale = updateProductDto.IsSale;
                existingProduct.IsAvailability = updateProductDto.IsAvailability;

                // Handle existing images - Remove images that are not in ExistingImagePaths
                if (updateProductDto.ExistingImagePaths != null)
                {
                    var imagesToRemove = existingProduct.Images
                        .Where(image => !updateProductDto.ExistingImagePaths.Contains(image.ImagePath))
                        .Select(image => image.ImagePath)
                        .ToList();

                    if (imagesToRemove.Any())
                    {
                        await _productImageService.RemoveImagesAsync(existingProduct.Id, imagesToRemove);
                    }
                }
                else
                {
                    // If no existing image paths are provided, remove all images
                    var allImages = existingProduct.Images.Select(x => x.ImagePath).ToList();
                    await _productImageService.RemoveImagesAsync(existingProduct.Id, allImages);
                }

                // Add new images if provided
                if (updateProductDto.ImageUploads != null && updateProductDto.ImageUploads.Any())
                {
                    await _productImageService.AddImagesAsync(existingProduct, updateProductDto.ImageUploads);
                }

                if (updateProductDto.Colors != null && updateProductDto.Colors.Any())
                {
                    var listColor = existingProduct.Colors.Select(x => x.ColorName).ToList();
                    await _productColorService.RemoveColorsAsync(existingProduct, listColor);

                    await _productColorService.AddColorsAsync(existingProduct, updateProductDto.Colors);
                }


                // Handle flags
                if (updateProductDto.Flags != null && updateProductDto.Flags.Any())
                {
                    await _productFlagService.AddFlagsAsync(existingProduct, updateProductDto.Flags);
                }

                await _context.SaveChangesAsync();

                return new ProductDTO
                {
                    Product = existingProduct,
                    Images = existingProduct.Images.ToList(),
                    Colors = existingProduct.Colors.ToList(),
                    Flags = existingProduct.Flags.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product with id {updateProductDto.Id}");
                throw;
            }
        }

        public async Task<bool> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Images)
                    .Include(p => p.Colors)
                    .Include(p => p.Flags)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                    return false;

                _context.ProductImages.RemoveRange(product.Images);
                _context.ProductColors.RemoveRange(product.Colors);
                _context.ProductFlags.RemoveRange(product.Flags);
                _context.Products.Remove(product);

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product with id {id}");
                throw;
            }
        }

        public IQueryable<Product> Queryable()
        {
            return _context.Set<Product>().AsQueryable();
        }

        public async Task<int> GetProductsCountAsync()
        {
            return await _context.Products.CountAsync();
        }
        public async Task<int> GetNewProductsCountAsync()
        {
            return await _context.Products
                .Where(p => p.Flags.Any(x => x.FlagType == "New"))
                .CountAsync();
        }
        public async Task<int> GetSaleProductsCountAsync()
        {
            return await _context.Products
                .Where(p => p.Flags.Any(x => x.FlagType == "Sale"))
                .CountAsync();
        }
        public async Task<int> GetFeatureProductsCountAsync()
        {
            return await _context.Products
                .Where(p => p.Flags.Any(x => x.FlagType == "Feature"))
                .CountAsync();
        }

        public async Task<List<Product>> GetProductsByIdsAsync(List<int> productIds)
        {
            return await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .Include(p => p.Colors)
                .ToListAsync();
        }
    }
}
