﻿using GabriniCosmetics.Areas.Admin.Models;
using GabriniCosmetics.Areas.Admin.Models.DTOs;
using GabriniCosmetics.Areas.Admin.Models.Interface;
using GabriniCosmetics.Areas.Admin.Models.Services;
using GabriniCosmetics.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace GabriniCosmetics.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProduct _product;
        private readonly ISubcategory _subcategory;
        private readonly ICategory _category;
        public ProductsController(IProduct product, ISubcategory subcategory, ICategory category)
        {
            _product = product;
            _subcategory = subcategory;
            _category = category;
        }

        public async Task<IActionResult> Detail(int id)
        {
            ProductVM vm = new ProductVM();

            vm.Products = await _product.GetProductById(id);
            vm.ProductBySubCategory = await _product.GetProductBySubCategory(vm.Products.Product.SubcategoryId);
            var subcategory = await _subcategory.GetSubcategoryById(vm.Products.Product.SubcategoryId);
            var category = await _category.GetCategoryById(subcategory.CategoryId);
            vm.Products.Product.Subcategory = new Areas.Admin.Models.Subcategory()
            {
                NameAr = subcategory.NameAr,
                NameEn = subcategory.NameEn,
                CategoryId = subcategory.CategoryId,
                Category = new Areas.Admin.Models.Category()
                {
                    NameEn = category.NameEn,
                    NameAr = category.NameAr,
                }
            };
            return View(vm);
        }
        public async Task<IActionResult> GetProductById( int id)
        {
            try
            {
                var product = await _product.GetProductById(id);
                if (product == null)
                {
                    return NotFound();
                }

                var subcategory = await _subcategory.GetSubcategoryById(product.Product.SubcategoryId);
                var category = await _category.GetCategoryById(subcategory.CategoryId);

                product.Product.Subcategory = new Areas.Admin.Models.Subcategory
                {
                    NameAr = subcategory.NameAr,
                    NameEn = subcategory.NameEn,
                    CategoryId = subcategory.CategoryId,
                    Category = new Areas.Admin.Models.Category
                    {
                        NameEn = category.NameEn,
                        NameAr = category.NameAr
                    }
                };

                return Ok(product);
            }
            catch (Exception ex)
            {
                // Log the exception (you might have a logger set up)
                return StatusCode(500, "Internal server error");
            }
        }

        public async Task<IActionResult> Index(string sortOrder, List<int> selectedCategories, List<int> selectedSubcategories)
        {
            var products = await _product.GetProducts();

            // Sorting logic
            switch (sortOrder)
            {
                case "lowToHigh":
                    products = products.OrderBy(p => p.Product.Price).ToList();
                    break;
                case "highToLow":
                    products = products.OrderByDescending(p => p.Product.Price).ToList();
                    break;
                case "aToZ":
                    products = products.OrderBy(p => p.Product.NameEn).ToList();
                    break;
                case "zToA":
                    products = products.OrderByDescending(p => p.Product.NameEn).ToList();
                    break;
                default:
                    // Handle default case or no sorting
                    break;
            }

            // Apply filters
            products = await FilteredProducts(products, selectedCategories, selectedSubcategories);

            // Set ViewData for maintaining checkbox states
            ViewData["SelectedCategories"] = selectedCategories?.Select(c => c.ToString()).ToList();
            ViewData["SelectedSubcategories"] = selectedSubcategories?.Select(s => s.ToString()).ToList();

            return View(products);
        }

        public async Task<IActionResult> FeatureProducts(string sortOrder, List<int> selectedCategories, List<int> selectedSubcategories)
        {
            var products = await _product.GetProducts();
            var productsFeature = products.Where(x => x.Flags.Any(f => f.FlagType == "Feature")).ToList();

            // Sorting logic
            switch (sortOrder)
            {
                case "lowToHigh":
                    productsFeature = productsFeature.OrderBy(p => p.Product.Price).ToList();
                    break;
                case "highToLow":
                    productsFeature = productsFeature.OrderByDescending(p => p.Product.Price).ToList();
                    break;
                case "aToZ":
                    productsFeature = productsFeature.OrderBy(p => p.Product.NameEn).ToList();
                    break;
                case "zToA":
                    productsFeature = productsFeature.OrderByDescending(p => p.Product.NameEn).ToList();
                    break;
                default:
                    // Handle default case or no sorting
                    break;
            }

            // Apply filters
            productsFeature = await FilteredProducts(productsFeature, selectedCategories, selectedSubcategories);

            // Set ViewData for maintaining checkbox states
            ViewData["SelectedCategories"] = selectedCategories?.Select(c => c.ToString()).ToList();
            ViewData["SelectedSubcategories"] = selectedSubcategories?.Select(s => s.ToString()).ToList();

            return View(productsFeature);
        }

        public async Task<IActionResult> NewProducts(string sortOrder, List<int> selectedCategories, List<int> selectedSubcategories)
        {
            var products = await _product.GetProducts();
            var productsNew = products.Where(x => x.Flags.Any(f => f.FlagType == "New")).ToList();

            // Sorting logic
            switch (sortOrder)
            {
                case "lowToHigh":
                    productsNew = productsNew.OrderBy(p => p.Product.Price).ToList();
                    break;
                case "highToLow":
                    productsNew = productsNew.OrderByDescending(p => p.Product.Price).ToList();
                    break;
                case "aToZ":
                    productsNew = productsNew.OrderBy(p => p.Product.NameEn).ToList();
                    break;
                case "zToA":
                    productsNew = productsNew.OrderByDescending(p => p.Product.NameEn).ToList();
                    break;
                default:
                    // Handle default case or no sorting
                    break;
            }

            // Apply filters
            productsNew = await FilteredProducts(productsNew, selectedCategories, selectedSubcategories);

            // Set ViewData for maintaining checkbox states
            ViewData["SelectedCategories"] = selectedCategories?.Select(c => c.ToString()).ToList();
            ViewData["SelectedSubcategories"] = selectedSubcategories?.Select(s => s.ToString()).ToList();

            return View(productsNew);
        }

        public async Task<IActionResult> SaleProducts(string sortOrder, List<int> selectedCategories, List<int> selectedSubcategories)
        {
            var products = await _product.GetProducts();
            var productsSale = products.Where(x => x.Flags.Any(f => f.FlagType == "Sale")).ToList();

            // Sorting logic
            switch (sortOrder)
            {
                case "lowToHigh":
                    productsSale = productsSale.OrderBy(p => p.Product.Price).ToList();
                    break;
                case "highToLow":
                    productsSale = productsSale.OrderByDescending(p => p.Product.Price).ToList();
                    break;
                case "aToZ":
                    productsSale = productsSale.OrderBy(p => p.Product.NameEn).ToList();
                    break;
                case "zToA":
                    productsSale = productsSale.OrderByDescending(p => p.Product.NameEn).ToList();
                    break;
                default:
                    // Handle default case or no sorting
                    break;
            }

            // Apply filters
            productsSale = await FilteredProducts(productsSale, selectedCategories, selectedSubcategories);

            // Set ViewData for maintaining checkbox states
            ViewData["SelectedCategories"] = selectedCategories?.Select(c => c.ToString()).ToList();
            ViewData["SelectedSubcategories"] = selectedSubcategories?.Select(s => s.ToString()).ToList();

            return View(productsSale);
        }

        private async Task<List<ProductDTO>> FilteredProducts(List<ProductDTO> products, List<int> selectedCategories, List<int> selectedSubcategories)
        {
            // Fetch and assign subcategory details asynchronously
            foreach (var product in products)
            {
                var subcategory = await _subcategory.GetSubcategoryById(product.Product.SubcategoryId);
                product.Product.Subcategory = new Subcategory()
                {
                    Category = new Category
                    {
                        Id = subcategory.CategoryId
                    },
                    NameAr = subcategory.NameAr,
                    NameEn = subcategory.NameEn,
                };
            }

            // Filter by selected subcategories
            if (selectedSubcategories != null && selectedSubcategories.Any())
            {
                products = products.Where(p => p.Product.SubcategoryId != null && selectedSubcategories.Contains(p.Product.SubcategoryId)).ToList();
            }

            // Filter by selected categories
            if (selectedCategories != null && selectedCategories.Any())
            {
                products = products.Where(p =>
                    p.Product.Subcategory != null &&
                    selectedCategories.Contains(p.Product.Subcategory.Category.Id)
                ).ToList();
            }

            return products;
        }


    }
}
