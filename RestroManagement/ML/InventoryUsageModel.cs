using Microsoft.ML.Data;

namespace RestroManagement.ML
{
    public class InventoryUsageModel
    {
        public int InventoryId { get; set; }

        public float UsedQuantity { get; set; }
        public float InventoryIndex { get; set; }

        [ColumnName("Label")]
        public float Label { get; set; } // Future demand (supervised learning target)
    }

    public class InventoryDemandPrediction
    {
        [ColumnName("Score")]
        public float PredictedDemand { get; set; }
    }

}
