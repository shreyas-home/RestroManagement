using Microsoft.AspNetCore.Mvc.Rendering;

namespace RestroManagement.Models.ViewModels
{
    public class CreateProductViewModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public List<SelectListItem>? Categories { get; set; }
        public List<InventorySelection>? InventorySelections { get; set; }
    }
}
