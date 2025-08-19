namespace RestroManagement.Models.ViewModels
{
    public class InventorySelection
    {
        public int InventoryItemId { get; set; }
        public string InventoryItemName { get; set; }

        public int Quantity { get; set; }
        public bool Selected { get; set; }
    }
}
