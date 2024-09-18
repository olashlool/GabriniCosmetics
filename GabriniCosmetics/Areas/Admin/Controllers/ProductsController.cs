using GabriniCosmetics.Areas.Admin.Models;
using GabriniCosmetics.Areas.Admin.Models.DTOs;
using GabriniCosmetics.Areas.Admin.Models.Interface;
using GabriniCosmetics.Areas.Admin.Models.Services;
using GabriniCosmetics.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GabriniCosmetics.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly GabriniCosmeticsContext _context;
        private readonly IProduct _productService;
        private readonly ICategory _category;
        private readonly ISubcategory _subcategory;
        private readonly ILogger<ProductsController> _logger;
        private readonly IFlagService _flagService;
        private readonly ICompositeViewEngine _viewEngine;


        public ProductsController(IProduct productService, ILogger<ProductsController> logger, ISubcategory subcategory, ICategory category, IFlagService flagService, ICompositeViewEngine viewEngine)
        {
            _productService = productService;
            _logger = logger;
            _subcategory = subcategory;
            _category = category;
            _flagService = flagService;
            _viewEngine = viewEngine;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index(string searchTerm = "")
        {
            var products = await _productService.GetProducts();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => p.Product.NameEn.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()) || p.Product.NameAr.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant())).ToList();

            }
            return View(products);
        }

        // GET: Admin/Products/Create
        public async Task<IActionResult> Create()
        {
            var flags = await _flagService.GetAllFlagsAsync();
            ViewBag.Flags = new SelectList(flags, "Id", "Name");

            var categories = await _category.GetCategories();
            var subcategories = await _subcategory.GetSubcategories();

            var subcategorySelectList = new List<SelectListItem>();

            foreach (var category in categories)
            {
                var group = new SelectListGroup { Name = category.NameEn };

                var subcategoryItems = subcategories
                    .Where(sc => sc.CategoryId == category.Id)
                    .Select(sc => new SelectListItem
                    {
                        Text = sc.NameEn,
                        Value = sc.Id.ToString(),
                        Group = group
                    }).ToList();

                subcategorySelectList.AddRange(subcategoryItems);
            }

            ViewBag.Subcategories = subcategorySelectList;
            return View();

        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductDTO createProductDto)
        {
            createProductDto.Flags = new List<int>();

            if (createProductDto.IsSale)
            {
                var flag = await _flagService.GetFlagByNameAsync("Sale");
                createProductDto.Flags.Add(flag.Id);
            }

            if (createProductDto.FlagNames.First() != null)
            {
                // Define a class that matches the JSON structure
                var parsedFlagNames = JsonSerializer.Deserialize<List<FlagConvert>>(createProductDto.FlagNames.First());

                // Extract values
                var extractedValues = parsedFlagNames?.ConvertAll(flag => flag.value);
                foreach (var item in extractedValues)
                {
                    var flag = await _flagService.GetFlagByNameAsync(item);
                    createProductDto.Flags.Add(flag.Id);
                }
            }
            else if(createProductDto.ImageUploads == null || createProductDto.ImageUploads.Count == 0 || createProductDto.SubcategoryId == 0)
            {
                var flags = await _flagService.GetAllFlagsAsync();
                ViewBag.Flags = new SelectList(flags, "Id", "Name");

                var categories = await _category.GetCategories();
                var subcategories = await _subcategory.GetSubcategories();

                var subcategorySelectList = new List<SelectListItem>();

                foreach (var category in categories)
                {
                    var group = new SelectListGroup { Name = category.NameEn };

                    var subcategoryItems = subcategories
                        .Where(sc => sc.CategoryId == category.Id)
                        .Select(sc => new SelectListItem
                        {
                            Text = sc.NameEn,
                            Value = sc.Id.ToString(),
                            Group = group
                        }).ToList();

                    subcategorySelectList.AddRange(subcategoryItems);
                }

                ViewBag.Subcategories = subcategorySelectList;
                return View(createProductDto);
            }
            await _productService.CreateProduct(createProductDto);
            return Redirect("/Admin/Products");
        }

        // GET: Admin/Products/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var flags = await _flagService.GetAllFlagsAsync();
            ViewBag.Flags = new SelectList(flags, "Id", "Name");

            var categories = await _category.GetCategories();
            var subcategories = await _subcategory.GetSubcategories();

            var subcategorySelectList = new List<SelectListItem>();

            foreach (var category in categories)
            {
                var group = new SelectListGroup { Name = category.NameEn };

                var subcategoryItems = subcategories
                    .Where(sc => sc.CategoryId == category.Id)
                    .Select(sc => new SelectListItem
                    {
                        Text = sc.NameEn,
                        Value = sc.Id.ToString(),
                        Group = group
                    }).ToList();

                subcategorySelectList.AddRange(subcategoryItems);
            }

            ViewBag.Subcategories = subcategorySelectList;

            var product = await _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }

            var updateProductDto = new UpdateProductDTO
            {
                Id = product.Product.Id,
                NameEn = product.Product.NameEn,
                NameAr = product.Product.NameAr,
                DescriptionEn = product.Product.DescriptionEn,
                DescriptionAr = product.Product.DescriptionAr,
                PersantageSale = product.Product.PersantageSale,
                PriceAfterDiscount = product.Product.PriceAfterDiscount,
                Price = product.Product.Price,
                ExistingImagePaths = product.Images.Select(i => i.ImagePath).ToList(),
                Colors = product.Colors.Select(c => c.ColorName).ToList(),
                Flags = product.Flags.Select(f => f.Id).ToList(),
                _Flags = product.Flags,
                SubcategoryId = product.Product.SubcategoryId,
                FlagNames = product.Flags.Select(f => f.FlagType).ToList(),
                FlagNamesString = string.Join(',', product.Flags.Select(f => f.FlagType).ToList()),
                IsSale = product.Product.IsSale,
                IsDealOfDay = product.Product.IsDealOfDay,
                IsAvailability = product.Product.IsAvailability
            };

            ViewBag.Subcategories = new SelectList(await _subcategory.GetSubcategories(), "Id", "NameEn", product.Product.SubcategoryId);
            return View(updateProductDto);
        }

        // POST: Admin/Products/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateProductDTO updateProductDto)
        {
            updateProductDto.Flags = new List<int>();

            if (updateProductDto.IsSale)
            {
                var flag = await _flagService.GetFlagByNameAsync("Sale");
                updateProductDto.Flags.Add(flag.Id);
            }

            if (updateProductDto.FlagNames.First() != null)
            {
                // Define a class that matches the JSON structure
                var parsedFlagNames = JsonSerializer.Deserialize<List<FlagConvert>>(updateProductDto.FlagNames.First());

                // Extract values
                var extractedValues = parsedFlagNames?.ConvertAll(flag => flag.value);
                foreach (var item in extractedValues)
                {
                    var flag = await _flagService.GetFlagByNameAsync(item);
                    updateProductDto.Flags.Add(flag.Id);
                }
            }
            if ( updateProductDto.ImageUploads == null && updateProductDto.ExistingImagePaths == null)
            {
                var flags = await _flagService.GetAllFlagsAsync();
                ViewBag.Flags = new SelectList(flags, "Id", "Name");

                var categories = await _category.GetCategories();
                var subcategories = await _subcategory.GetSubcategories();

                var subcategorySelectList = new List<SelectListItem>();

                foreach (var category in categories)
                {
                    var group = new SelectListGroup { Name = category.NameEn };

                    var subcategoryItems = subcategories
                        .Where(sc => sc.CategoryId == category.Id)
                        .Select(sc => new SelectListItem
                        {
                            Text = sc.NameEn,
                            Value = sc.Id.ToString(),
                            Group = group
                        }).ToList();

                    subcategorySelectList.AddRange(subcategoryItems);
                }

                ViewBag.Subcategories = subcategorySelectList;
                return View(updateProductDto);
            }
            await _productService.UpdateProduct(updateProductDto);
            return Redirect("/Admin/Products");

        }

        // POST: Admin/Products/Delete/{id}
        [HttpPost]
        public async Task<IActionResult> DeleteProduct([FromBody] int id)
        {
            var success = await _productService.DeleteProduct(id);
            if (!success)
            {
                return NotFound();
            }
            return Ok();
        }

        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductById(id);

            // Render the partial view to HTML
            var quickViewHtml = await RenderPartialViewToStringAsync("_QuickView", product);

            return Json(new { quickViewHtml });
        }

        private async Task<string> RenderPartialViewToStringAsync(string viewName, object model)
        {
            // Create a new ViewDataDictionary to hold the model
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            // Create a new ViewContext
            using (var sw = new StringWriter())
            {
                var actionContext = new ActionContext(ControllerContext.HttpContext, ControllerContext.RouteData, ControllerContext.ActionDescriptor);
                var viewResult = _viewEngine.FindView(actionContext, viewName, false);

                if (!viewResult.Success)
                {
                    throw new InvalidOperationException($"Couldn't find view {viewName}");
                }

                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewData,
                    TempData,
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return sw.GetStringBuilder().ToString();
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var productDetails = await _productService.GetProductById(id);
            var subCategory = await _subcategory.GetSubcategoryById(productDetails.Product.SubcategoryId);
            var category = await _category.GetCategoryById(subCategory.CategoryId);
            productDetails.Product.Subcategory = new Subcategory()
            {
                NameAr = subCategory.NameAr,
                NameEn = subCategory.NameEn,
                Category = new Category()
                {
                    NameAr = category.NameAr,
                    NameEn = category.NameEn,
                }
            };
            return View(productDetails);
        }
    }

}
