using PartSphere.Models;

namespace PartSphere.Services
{
    public interface IAIService
    {
        Task<List<MaintenanceSuggestion>> GetPredictiveMaintenanceAsync(int vehicleId);
        Task<List<StockPrediction>> GetInventoryForecastAsync();
    }

    public class MaintenanceSuggestion
    {
        public string PartName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public int Confidence { get; set; }
        public string Urgency { get; set; } = "Medium";
        public int EstimatedRemainingKm { get; set; }
    }

    public class StockPrediction
    {
        public int PartId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public int PredictedDemandNext30Days { get; set; }
        public bool RestockRecommended { get; set; }
    }
}
