﻿using GabriniCosmetics.Areas.Admin.Models;
using GabriniCosmetics.Areas.Admin.Models.DTOs;
using GabriniCosmetics.Areas.Admin.Models.Interface;
using GabriniCosmetics.Areas.Admin.Models.Services;
using GabriniCosmetics.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace GabriniCosmetics.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategory _categoryService;
        private readonly ISubcategory _subcategoryService;
        private readonly IProduct _productService;

        public CategoriesController(ICategory category, IProduct productService, ISubcategory subcategoryService)
        {
            _categoryService = category;
            _productService = productService;
            _subcategoryService = subcategoryService;
        }
        public async Task<IActionResult> Index(int id, string sortOrder, List<int> selectedCategories, List<int> selectedSubcategories)
        {
            CategoryVM categoryVM = new CategoryVM();
            categoryVM.Category = await _categoryService.GetCategoryById(id);
            categoryVM.Categories = await _categoryService.GetCategories();

            // Initialize the product list
            categoryVM.Products = new List<ProductDTO>();

            foreach (var item in categoryVM.Category.SubCategories)
            {
                var products = await _productService.GetProductBySubCategory(item.Id);
                categoryVM.Products.AddRange(products);
            }

            // Sorting
            switch (sortOrder)
            {
                case "lowToHigh":
                    categoryVM.Products = categoryVM.Products.OrderBy(p => p.Product.Price).ToList();
                    break;
                case "highToLow":
                    categoryVM.Products = categoryVM.Products.OrderByDescending(p => p.Product.Price).ToList();
                    break;
                case "aToZ":
                    categoryVM.Products = categoryVM.Products.OrderBy(p => p.Product.NameEn).ToList();
                    break;
                case "zToA":
                    categoryVM.Products = categoryVM.Products.OrderByDescending(p => p.Product.NameEn).ToList();
                    break;
                default:
                    // Handle default case or no sorting
                    break;
            }

            // Apply Filters
            categoryVM.Products = await FilteredProducts(categoryVM.Products, selectedCategories, selectedSubcategories);

            // Set ViewData for maintaining checkbox states
            ViewData["SelectedCategories"] = selectedCategories?.Select(c => c.ToString()).ToList();
            ViewData["SelectedSubcategories"] = selectedSubcategories?.Select(s => s.ToString()).ToList();

            return View(categoryVM);
        }


        public async Task<IActionResult> Filters(List<int> selectedColors, List<int> selectedSubcategories, List<int> productIds)
        {
            var products = await _productService.GetProductsByIdsAsync(productIds); // Method to get products by IDs

            if (selectedColors != null && selectedColors.Any())
            {
                products = products.Where(p => p.Colors.Any(c => selectedColors.Contains(c.Id))).ToList();
            }

            if (selectedSubcategories != null && selectedSubcategories.Any())
            {
                products = products.Where(p => selectedSubcategories.Contains(p.SubcategoryId)).ToList();
            }

            // Return the previous URL
            var refererUrl = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(refererUrl))
            {
                // Preserve filters in the URL
                var url = $"{refererUrl}?selectedColors={string.Join(",", selectedColors)}&selectedSubcategories={string.Join(",", selectedSubcategories)}";
                return Redirect(url);
            }

            // Fallback: if there is no previous URL, return to the product list page
            return RedirectToAction("Index", "Product");
        }

        private async Task<List<ProductDTO>> FilteredProducts(List<ProductDTO> products, List<int> selectedCategories, List<int> selectedSubcategories)
        {
            // Fetch and assign subcategory details asynchronously
            foreach (var product in products)
            {
                var subcategory = await _subcategoryService.GetSubcategoryById(product.Product.SubcategoryId);
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
