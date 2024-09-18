using GabriniCosmetics.Areas.Admin.Models.Interface;
using GabriniCosmetics.Data;

namespace GabriniCosmetics.Areas.Admin.Models.Services
{
    public class ProductColorService : IProductColorService
    {
        private readonly GabriniCosmeticsContext _context;
        private readonly ILogger<ProductColorService> _logger;

        public ProductColorService(GabriniCosmeticsContext context, ILogger<ProductColorService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddColorsAsync(Product product, List<string> colors)
        {
            try
            {
                foreach (var colorId in colors)
                {
                    var productColor = new ProductColor
                    {
                        ColorName = colorId,
                        ProductId = product.Id,
                        Product = product
                    };
                    product.Colors.Add(productColor);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding colors");
                throw;
            }
        }
        public async Task RemoveColorsAsync(Product product, List<string> colorNames)
        {
            try
            {
                var colorsToRemove = _context.ProductColors
                    .Where(pc => pc.ProductId == product.Id && colorNames.Contains(pc.ColorName))
                    .ToList();

                if (colorsToRemove.Any())
                {
                    _context.ProductColors.RemoveRange(colorsToRemove);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing colors");
                throw;
            }
        }
    }

}
