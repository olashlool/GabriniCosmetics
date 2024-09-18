using GabriniCosmetics.Areas.Admin.Models;
using GabriniCosmetics.Areas.Admin.Models.DTOs;

namespace GabriniCosmetics.Models.ViewModel
{
    public class HomeVM
    {
        public List<CategoryDTO> Categories { get; set; }
        public List<ProductDTO> Products { get; set; }
        public List<ProductDTO> ProductsSale { get; set; }
        public List<ProductDTO> ProductsNew { get; set; }
        public List<ProductDTO> ProductsFeature { get; set; }
        public List<SliderBannerDTO> SlidersBanner { get; set; }
        public List<SliderAdDTO> SlidersAd { get; set; }
        public List<DealOfTheDays> DealOfTheDays { get; set; }
        public List<ProductDTO> ListOfProductsDealOfTheDays { get; set; }
    }
}
