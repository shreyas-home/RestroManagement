using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using RestroManagement.Data;
using RestroManagement.Models.Domain;
using RestroManagement.Models.ViewModels;
using RestroManagement.Repository.Implementations;

namespace RestroManagement.Controllers
{
    public class MenuController : Controller
    {
        private GenericRepository<Product> _products;
        private readonly ApplicationDbContext _context;
        public MenuController(ApplicationDbContext context)
        {
            _products = new GenericRepository<Product>(context);
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                                    .Include(c => c.Products)
                                        .ThenInclude(p => p.ProductInventories)
                                            .ThenInclude(pi => pi.Inventory)
                                    .ToListAsync();

            var menu = new MenuViewModel
            {
                Products = categories.SelectMany(c => c.Products).Distinct().ToList(),
                Categories = categories.ToList()
            };

            foreach (var product in categories.SelectMany(c => c.Products))
            {
                var available = CalculateMaxUnitsAvailable(product);
                menu.AvailableByProductId[product.Id] = available;
            }

            return View(menu);
        }

        private static int CalculateMaxUnitsAvailable(Product product)
        {
            if (product == null || product.ProductInventories == null || !product.ProductInventories.Any())
            {
                return 0;
            }

            int? minUnits = null;

            foreach (var productInventory in product.ProductInventories)
            {
                if (productInventory.Inventory == null)
                {
                    return 0;
                }

                var quantityUsed = productInventory.QuantityUsed;
                if (quantityUsed <= 0)
                {
                    // If no inventory consumption is defined, treat as not limiting this product
                    continue;
                }

                var availableForThisInventory = (int)Math.Floor((decimal)productInventory.Inventory.Quantity / quantityUsed);
                availableForThisInventory = Math.Max(availableForThisInventory, 0);

                if (minUnits == null)
                {
                    minUnits = availableForThisInventory;
                }
                else
                {
                    minUnits = Math.Min(minUnits.Value, availableForThisInventory);
                }
            }

            return Math.Max(minUnits ?? 0, 0);
        }
      

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitOrder([FromBody] List<AddOrderViewModel> orders)
        {
            if (orders == null || !orders.Any())
            {
                return BadRequest();
            }

            // Validate the order items for inventory
            foreach(var orderItem in orders)
            {
                var product = await _context.Products
                    .Include(p => p.ProductInventories)
                    .ThenInclude(pi => pi.Inventory)
                    .FirstOrDefaultAsync(p => p.Id == orderItem.Id);

                if (product == null || !product.ProductInventories.Any())
                {
                    return BadRequest($"Product with ID {orderItem.Id} does not exist or has no inventory.");
                }
               
                foreach(var pi in product.ProductInventories)
                {
                    var totalNeeded = pi.QuantityUsed * orderItem.Quantity;

                    var inventory = pi.Inventory;
                    if (inventory == null)
                    {
                        return BadRequest($"Inventory for product {product.Name} is not available.");
                    }
                    if (inventory.Quantity < totalNeeded)
                    {
                        // Compute max available units for this product to inform the client
                        var maxUnits = CalculateMaxUnitsAvailable(product);
                        return BadRequest($"Insufficient inventory for product {product.Name}. Max available units: {maxUnits}, Requested: {orderItem.Quantity}");
                    }
                }
            }


            try
            {
                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    TotalAmount = orders.Sum(o => o.Price * o.Quantity),
                    OrderItems = orders.Select(o => new OrderItem
                    {
                        ProductId = o.Id,
                        Quantity = o.Quantity,
                        Price = o.Price

                    }).ToList()
                };
                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();


                // Deduct inventory after order is saved
                foreach (var orderItem in orders)
                {
                    var product = await _context.Products
                        .Include(p => p.ProductInventories)
                        .ThenInclude(pi => pi.Inventory)
                        .FirstOrDefaultAsync(p => p.Id == orderItem.Id);

                    foreach (var pi in product.ProductInventories)
                    {
                        var totalNeeded = pi.QuantityUsed * orderItem.Quantity;
                        var inventory = pi.Inventory;
                        if (inventory != null)
                        {
                            inventory.Quantity -= (int)totalNeeded;
                            _context.Inventories.Update(inventory);
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here if needed
                return BadRequest();
            }


            return Json(new { success = true });
        }
    }
  

}
