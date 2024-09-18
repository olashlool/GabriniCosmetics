using GabriniCosmetics.Areas.Admin.Models.Interface;

namespace GabriniCosmetics.Areas.Admin.Models
{
    public class OrderItems
    {
        public int ID { get; set; }
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public string ImageProduct { get; set; }
        public double? TotalPrice { get; set; }
        public string? DiscountCode { get; set; }

        public Order Order { get; set; }
        public Product Product { get; set; }

    }
}
