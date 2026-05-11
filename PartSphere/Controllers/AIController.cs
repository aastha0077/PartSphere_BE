using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartSphere.Services;

namespace PartSphere.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;

        public AIController(IAIService aiService)
        {
            _aiService = aiService;
        }

        [HttpGet("predictive-maintenance/{vehicleId}")]
        [Authorize]
        public async Task<IActionResult> GetMaintenanceSuggestions(int vehicleId)
        {
            var suggestions = await _aiService.GetPredictiveMaintenanceAsync(vehicleId);
            return Ok(suggestions);
        }

        [HttpGet("inventory-forecast")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetInventoryForecast()
        {
            var forecast = await _aiService.GetInventoryForecastAsync();
            return Ok(forecast);
        }
    }
}
