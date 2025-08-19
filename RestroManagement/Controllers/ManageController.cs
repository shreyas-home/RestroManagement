using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestroManagement.Data;

namespace RestroManagement.Controllers
{
    public class ManageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManageController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.Include(p => p.Products).ToListAsync();
            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description))
            {
                ModelState.AddModelError("", "Name and Description cannot be empty.");
                return View("Index", await _context.Categories.Include(p => p.Products).ToListAsync());
            }
            var category = new Models.Domain.Category
            {
                Name = name,
                Description = description
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
