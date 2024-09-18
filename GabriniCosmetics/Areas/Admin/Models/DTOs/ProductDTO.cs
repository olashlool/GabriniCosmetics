namespace GabriniCosmetics.Areas.Admin.Models.DTOs
{
    public class ProductDTO
    {
        public Product Product { get; set; }
        public List<ProductImage> Images { get; set; }
        public List<ProductColor> Colors { get; set; }
        public List<ProductFlag> Flags { get; set; }
    }
    public class UpdateProductDTO
    {
        public int Id { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionAr { get; set; }
        public double Price { get; set; }
        public double? PriceAfterDiscount { get; set; }
        public int? PersantageSale { get; set; }
        public List<IFormFile> ImageUploads { get; set; }
        public List<string> Colors { get; set; }
        public List<int> Flags { get; set; }
        public List<ProductFlag> _Flags { get; set; }
        public List<string> FlagNames { get; set; }
        public string FlagNamesString { get; set; }
        public List<string> ExistingImagePaths { get; set; }
        public int SubcategoryId { get; set; }
        public bool IsDealOfDay { get; set; }
        public bool IsSale { get; set; }
        public bool IsAvailability { get; set; }
    }

    public class CreateProductDTO
    {
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionAr { get; set; }
        public double Price { get; set; }
        public double? PriceAfterDiscount { get; set; }
        public int? PersantageSale { get; set; }
        public List<IFormFile> ImageUploads { get; set; }
        public List<string> Colors { get; set; }
        public List<int> Flags { get; set; }
        public int SubcategoryId { get; set; }
        public bool IsDealOfDay { get; set; }
        public bool IsAvailability { get; set; }
        public bool IsSale { get; set; }
        public List<string> FlagNames { get; set; }
        public string? FlagNamesString { get; set; }
    }
}
