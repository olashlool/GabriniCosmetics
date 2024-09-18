using GabriniCosmetics.Areas.Admin.Models.Interface;
using System.ComponentModel.DataAnnotations;

namespace GabriniCosmetics.Areas.Admin.Models
{
    public class WishlistDetail
    {
        public int Id { get; set; }
        [Required]
        public int WishlistId { get; set; }
        [Required]
        public int ProductId { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public double UnitPrice { get; set; }
        public string Image { get; set; }
        public string ColorName { get; set; }
        public Product Products { get; set; }
        public Wishlist Wishlist { get; set; }

    }
}