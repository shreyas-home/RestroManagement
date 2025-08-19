using Microsoft.ML;
using RestroManagement.ML;
using RestroManagement.Models.Domain;
using RestroManagement.Models.ViewModels;

public class SalesPredictionService
{
    private readonly MLContext _mlContext = new();
    private ITransformer _model;

    public void Train(IEnumerable<Order> orders)
    {
        var data = orders.Select(o => new SalesData
        {
            OrderDateNumeric = (float)(o.OrderDate - new DateTime(2020, 1, 1)).TotalDays,
            TotalAmount = (float)o.TotalAmount
        });

        var trainingData = _mlContext.Data.LoadFromEnumerable(data);

        var pipeline = _mlContext.Regression.Trainers.Sdca(labelColumnName: "TotalAmount", featureColumnName: "OrderDateNumeric");
        _model = pipeline.Fit(trainingData);
    }

    public float Predict(DateTime futureDate)
    {
        var engine = _mlContext.Model.CreatePredictionEngine<SalesData, SalesPrediction>(_model);
        var input = new SalesData
        {
            OrderDateNumeric = (float)(futureDate - new DateTime(2020, 1, 1)).TotalDays
        };
        var prediction = engine.Predict(input);
        return prediction.Score;
    }
}