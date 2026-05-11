using Microsoft.EntityFrameworkCore;
using PartSphere.Data;
using PartSphere.Models;

namespace PartSphere.Services
{
    public class AIService : IAIService
    {
        private readonly AppDbContext _context;

        public AIService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<MaintenanceSuggestion>> GetPredictiveMaintenanceAsync(int vehicleId)
        {
            var suggestions = new List<MaintenanceSuggestion>();
            
            var vehicle = await _context.Vehicles
                .Include(v => v.Customer)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);

            if (vehicle == null) return suggestions;

            // Get all historical parts installed on this vehicle
            var salesHistory = await _context.SalesInvoices
                .Where(s => s.VehicleId == vehicleId && s.PaymentStatus == "PAID")
                .Include(s => s.Items)
                .ThenInclude(i => i.VehiclePart)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            // Simple AI Logic:
            // 1. If a critical part (e.g., Brake Pads) hasn't been replaced in > LifespanKm
            // 2. If the vehicle mileage is high and certain parts are original
            
            var criticalParts = new[] { "Brake Pads", "Oil Filter", "Timing Belt", "Spark Plugs", "Air Filter" };

            foreach (var partName in criticalParts)
            {
                // Find last time this part was sold/installed for this vehicle
                var lastReplacement = salesHistory
                    .SelectMany(s => s.Items)
                    .FirstOrDefault(i => i.VehiclePart.Name.Contains(partName, StringComparison.OrdinalIgnoreCase));

                if (lastReplacement != null)
                {
                    var kmSinceReplacement = vehicle.Mileage - lastReplacement.SalesInvoice.MileageAtSale;
                    var lifespan = lastReplacement.VehiclePart.LifespanKm > 0 ? lastReplacement.VehiclePart.LifespanKm : 10000;

                    if (kmSinceReplacement >= lifespan)
                    {
                        suggestions.Add(new MaintenanceSuggestion
                        {
                            PartName = partName,
                            Reason = $"Last replaced {kmSinceReplacement}km ago. Recommended lifespan is {lifespan}km.",
                            Urgency = kmSinceReplacement > lifespan * 1.2 ? "Critical" : "High",
                            Confidence = 95,
                            EstimatedRemainingKm = Math.Max(0, lifespan - kmSinceReplacement)
                        });
                    }
                    else if (kmSinceReplacement > lifespan * 0.8)
                    {
                        suggestions.Add(new MaintenanceSuggestion
                        {
                            PartName = partName,
                            Reason = $"Approaching end of lifespan ({kmSinceReplacement}/{lifespan}km used).",
                            Urgency = "Medium",
                            Confidence = 80,
                            EstimatedRemainingKm = lifespan - kmSinceReplacement
                        });
                    }
                }
                else
                {
                    // Part never replaced in our records
                    // If vehicle has high mileage, suggest it
                    if (vehicle.Mileage > 20000)
                    {
                        suggestions.Add(new MaintenanceSuggestion
                        {
                            PartName = partName,
                            Reason = $"No record of replacement. Given current mileage of {vehicle.Mileage}km, inspection is recommended.",
                            Urgency = "Medium",
                            Confidence = 70,
                            EstimatedRemainingKm = 0
                        });
                    }
                }
            }

            return suggestions;
        }

        public async Task<List<StockPrediction>> GetInventoryForecastAsync()
        {
            // Simple AI Logic: 
            // Calculate sales velocity of last 30 days and predict next 30
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            
            var recentSales = await _context.SalesItems
                .Include(i => i.SalesInvoice)
                .Include(i => i.VehiclePart)
                .Where(i => i.SalesInvoice.Date >= thirtyDaysAgo)
                .ToListAsync();

            return recentSales
                .GroupBy(i => new { i.VehiclePartId, i.VehiclePart.Name, i.VehiclePart.StockQuantity })
                .Select(g => new StockPrediction
                {
                    PartId = g.Key.VehiclePartId,
                    PartName = g.Key.Name,
                    PredictedDemandNext30Days = (int)Math.Ceiling(g.Sum(x => x.Quantity) * 1.1), // Expect 10% growth
                    RestockRecommended = g.Key.StockQuantity < (g.Sum(x => x.Quantity) * 1.1)
                })
                .OrderByDescending(p => p.PredictedDemandNext30Days)
                .ToList();
        }
    }
}
