using GabriniCosmetics.Areas.Admin.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace GabriniCosmetics.Areas.Admin.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string NameAr { get; set; }

        public string DescriptionEn { get; set; }
        public string DescriptionAr { get; set; }

        public double Price { get; set; }
        public double? PriceAfterDiscount { get; set; }
        public int? PersantageSale { get; set; }
        public string? FlagNamesString { get; set; }
        public bool IsDealOfDay { get; set; }
        public bool IsSale { get; set; }
        public bool IsAvailability { get; set; }
        public ICollection<ProductImage> Images { get; set; }
        public ICollection<ProductColor> Colors { get; set; }
        public ICollection<ProductFlag>? Flags { get; set; }

        public int SubcategoryId { get; set; }
        public Subcategory Subcategory { get; set; }
    }
    public class ProductImage
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }

    public class ProductColor
    {
        public int Id { get; set; }
        public string ColorName { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }

    public class ProductFlag
    {
        public int Id { get; set; }
        public string FlagType { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
