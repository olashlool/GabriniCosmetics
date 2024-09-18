using GabriniCosmetics.Areas.Admin.Models.DTOs;

namespace GabriniCosmetics.Models.ViewModel
{
    public class CategoryVM
    {
        public CategoryDTO Category { get; set; }
        public List<CategoryDTO> Categories { get; set; }
        public List<ProductDTO> Products { get; set; }
    }
}
