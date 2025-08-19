namespace RestroManagement.Models.ViewModels
{
    public class AddOrderViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
