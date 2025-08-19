using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestroManagement.Data;
using RestroManagement.Models.Domain;
using RestroManagement.Repository.Implementations;

namespace RestroManagement.Controllers
{
    public class OrderController : Controller
    {
        private GenericRepository<Order> _orders;
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _orders = new GenericRepository<Order>(context);
            _context = context;
        }
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;

            var baseQuery = _context.Orders.AsQueryable();

            // Totals across all orders
            var totalOrders = await baseQuery.CountAsync();
            var todaysOrders = await baseQuery.CountAsync(o => o.OrderDate.Date == DateTime.Today);
            var totalSales = await baseQuery.SumAsync(o => o.TotalAmount);
            var todaysSales = await baseQuery.Where(o => o.OrderDate.Date == DateTime.Today).SumAsync(o => o.TotalAmount);

            var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(totalPages, 1)));

            var items = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TodaysOrders = todaysOrders;
            ViewBag.TotalSales = totalSales;
            ViewBag.TodaysSales = todaysSales;

            return View(items);
        }
    }
}
