using RestroManagement.Models.Domain;

namespace RestroManagement.Models.ViewModels
{
    public class MenuViewModel
    {
        public List<Category>? Categories { get; set; }
        public List<Product>? Products { get; set; }
        public Dictionary<int, int> AvailableByProductId { get; set; } = new();
    }
}
