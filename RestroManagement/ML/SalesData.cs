namespace RestroManagement.ML
{
    public class SalesData
    {
        public float OrderDateNumeric { get; set; } // e.g., days since a reference date
        public float TotalAmount { get; set; }
    }

    public class SalesPrediction
    {
        public float Score { get; set; } // Predicted sales amount
    }
}
