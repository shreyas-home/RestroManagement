using System.ComponentModel.DataAnnotations.Schema;

namespace RestroManagement.Models.Domain
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public string? ImageUrl { get; set; }

        // Product Belongs to a Category
        public int CategoryId { get; set; }
        public Category? Category { get; set; } 

        public ICollection<ProductInventory> ProductInventories { get; set; } = new List<ProductInventory>();
    }
}
