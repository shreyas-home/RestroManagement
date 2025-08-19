using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RestroManagement.Data;
using RestroManagement.Models.Domain;
using RestroManagement.Models.ViewModels;
using RestroManagement.Repository.Implementations;

namespace RestroManagement.Controllers
{
    public class ProductController : Controller
    {
        private GenericRepository<Product> _products;
        private GenericRepository<Inventory> _inventories;
        private GenericRepository<Category> _categories;

        public ProductController(ApplicationDbContext context)
        {
            _products = new GenericRepository<Product>(context);
            _inventories = new GenericRepository<Inventory>(context);
            _categories = new GenericRepository<Category>(context);
        }
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;
            var all = await _products.GetAllAsync();
            var totalCount = all.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(totalPages, 1)));

            var items = all
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.AveragePrice = totalCount == 0 ? 0 : all.Average(p => p.Price);
            ViewBag.CategoriesCount = all.Select(p => p.CategoryId).Distinct().Count();
            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var inventoryItems = await _inventories.GetAllAsync();
            var categories = await _categories.GetAllAsync();

            var viewModel = new CreateProductViewModel
            {
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList(),

                InventorySelections = inventoryItems.Select(i => new InventorySelection
                {
                    InventoryItemId = i.Id,
                    InventoryItemName = i.Name,
                    Quantity = 0,
                    Selected = false
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                { 
                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    CategoryId = model.CategoryId,
                    ProductInventories = model.InventorySelections
                        .Where(i => i.Selected && i.Quantity > 0)
                        .Select(i => new ProductInventory
                        {
                            InventoryId = i.InventoryItemId,
                            QuantityUsed = i.Quantity
                        }).ToList()
                };
                
                
                    await _products.AddAsync(product);
                }
                catch(Exception ex)
                {
                    // Log the exception (ex) here if needed
                    ModelState.AddModelError("", "An error occurred while creating the product. Please try again.");
                    // fall through to repopulate lists below
                }
                if (ModelState.IsValid)
                {
                    return RedirectToAction("Index");
                }
            }
            var categories = await _categories.GetAllAsync();
            model.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            if (model.InventorySelections == null || model.InventorySelections.Count == 0)
            {
                var inventoryItems = await _inventories.GetAllAsync();
                model.InventorySelections = inventoryItems.Select(i => new InventorySelection
                {
                    InventoryItemId = i.Id,
                    InventoryItemName = i.Name,
                    Quantity = 0,
                    Selected = false
                }).ToList();
            }
            return View(model);
        }
    }
}
