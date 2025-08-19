using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using RestroManagement.Data;
using RestroManagement.ML;
using RestroManagement.Models.Domain;
using System;

public class InventoryDemandPredictionService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly MLContext _mlContext;

    public InventoryDemandPredictionService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _mlContext = new MLContext();
    }

    public List<InventoryPredictionResponse> Train()
    {
        var inventoryUsage = _dbContext.Orders
                                  .Include(o => o.OrderItems)
                                  .ThenInclude(op => op.Product)
                                  .ThenInclude(pi => pi.ProductInventories)
                                  .SelectMany(order => order.OrderItems.SelectMany(op =>
                                                                        op.Product.ProductInventories.Select(pi =>
                                                                        new
                                                                        {
                                                                            pi.InventoryId,
                                                                            UsedQuantity = pi.QuantityUsed * op.Quantity
                                                                        })))
                                  .ToList();

        // Generate numeric index for InventoryId
        var inventoryIds = inventoryUsage.Select(x => x.InventoryId).Distinct().ToList();
        var inventoryIndexLookup = inventoryIds.Select((id, index) => new { id, index })
            .ToDictionary(x => x.id, x => (float)x.index);

        var trainingDataList = inventoryUsage.Select(x => new InventoryUsageModel
        {
            InventoryId = x.InventoryId,
            InventoryIndex = inventoryIndexLookup[x.InventoryId],
            UsedQuantity = (float)x.UsedQuantity,
            Label =(float)x.UsedQuantity
        }).ToList();

        var trainingData = _mlContext.Data.LoadFromEnumerable(trainingDataList);

        var pipeline = _mlContext.Transforms.Concatenate("Features", nameof(InventoryUsageModel.InventoryIndex))
                                            .Append(_mlContext.Regression.Trainers.FastTree());

        var model = pipeline.Fit(trainingData);

        var predictionEngine = _mlContext.Model.CreatePredictionEngine<InventoryUsageModel, InventoryDemandPrediction>(model);

        var inventoryPrediction = new List<InventoryPredictionResponse>();

        foreach (var inventoryid in inventoryIds)
        {
            var testInput = new InventoryUsageModel
            {
                InventoryId = inventoryid,
                InventoryIndex = inventoryIndexLookup[inventoryid],
                UsedQuantity = 0
            };
            var prediction = predictionEngine.Predict(testInput);

            inventoryPrediction.Add(new InventoryPredictionResponse
            {
                Inventory = _dbContext.Inventories.Find(inventoryid),
                PredictedInventoryUsed = prediction.PredictedDemand
            });
        }

        return inventoryPrediction;
    }


}