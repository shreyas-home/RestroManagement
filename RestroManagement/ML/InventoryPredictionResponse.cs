using RestroManagement.Models.Domain;

namespace RestroManagement.ML
{
    public class InventoryPredictionResponse
    {
        public Inventory Inventory { get; set; }
        public float PredictedInventoryUsed { get; set; }
        public string Demand { get; set; }
    }
}
