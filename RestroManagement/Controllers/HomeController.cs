using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestroManagement.Data;
using RestroManagement.ML;
using RestroManagement.Models;
using RestroManagement.Models.Domain;
using RestroManagement.Models.ViewModels;

namespace RestroManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        private InventoryDemandPredictionService inventoryML;
        public IEnumerable<Order> Orders { get; set; }
        public float PredictedInventoryDemand { get; set; }



        public HomeController(ApplicationDbContext dbContext, ILogger<HomeController> logger)
        {
            _context = dbContext;
            _logger = logger;
            inventoryML = new InventoryDemandPredictionService(dbContext);
        }

       

        public IActionResult Index()
        {
        
            return View();
        }

        public async Task<IActionResult> Predict()
        {
            var inventoryPrediction = inventoryML.Train();

            var avg = inventoryPrediction.Average(x=>x.PredictedInventoryUsed);
            var stdDev = MathF.Sqrt(inventoryPrediction.Sum(x => MathF.Pow(x.PredictedInventoryUsed - avg, 2)) / inventoryPrediction.Count);

            List<InventoryPredictionResponse> response = new List<InventoryPredictionResponse>();

            for (int i = 0; i< inventoryPrediction.Count; i++) 
            {
                string category = inventoryPrediction[i].PredictedInventoryUsed >= avg + stdDev ? "High" :
                                    inventoryPrediction[i].PredictedInventoryUsed <= avg - stdDev ? "Low" : "Medium";
                response.Add(new InventoryPredictionResponse
                {
                    Inventory = inventoryPrediction[i].Inventory,
                    PredictedInventoryUsed = inventoryPrediction[i].PredictedInventoryUsed,
                    Demand = category
                });

                inventoryPrediction[i].Demand = category;
            }



            ViewBag.PredictedInventoryDemands = inventoryPrediction.OrderByDescending(i=>i.PredictedInventoryUsed);

            return View("Index" , response);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
