using System.ComponentModel.DataAnnotations.Schema;

namespace RestroManagement.Models.Domain
{
    public class Inventory
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal BuyingPrice { get; set; }
        public int Quantity { get; set; }
        public string? Unit { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int ThresholdValue { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public string? ImageUrl { get; set; }

        public ICollection<ProductInventory> ProductInventories { get; set; } = new List<ProductInventory>();
    }
}
