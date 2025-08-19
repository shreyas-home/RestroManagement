using Microsoft.AspNetCore.Mvc;
using RestroManagement.Data;
using RestroManagement.Models.Domain;
using RestroManagement.Repository.Implementations;

namespace RestroManagement.Controllers
{
    public class InventoryController : Controller
    {
        private GenericRepository<Inventory> _inventory;

        public InventoryController(ApplicationDbContext context)
        {
            _inventory = new GenericRepository<Inventory>(context);
        }
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;
            var all = await _inventory.GetAllAsync();
            var totalCount = all.Count();
            var lowStockCount = all.Count(i => i.Quantity <= i.ThresholdValue);
            var totalStockValue = all.Sum(i => i.BuyingPrice * i.Quantity);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(totalPages, 1)));

            var items = all
                .OrderBy(i => i.Quantity)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.LowStockCount = lowStockCount;
            ViewBag.TotalStockValue = totalStockValue;
            return View(items);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                await _inventory.AddAsync(inventory);
                return RedirectToAction("Index");
            }
            return View(inventory);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {   
            var inventory = await _inventory.GetByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }
            return View(inventory);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                await _inventory.UpdateAsync(inventory);
                return RedirectToAction("Index");
            }
            return View(inventory);
        }
    }
}
