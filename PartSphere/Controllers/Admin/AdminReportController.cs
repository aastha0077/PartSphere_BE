using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartSphere.Services;

namespace PartSphere.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/reports")]
    [Authorize(Roles = "Admin")]
    public class AdminReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public AdminReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var dashboard = await _reportService.GetDashboardStatsAsync();
            return Ok(dashboard);
        }

        [HttpGet("sales")]
        public async Task<IActionResult> GetSalesReport([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var report = await _reportService.GetSalesReportAsync(from, to);
            return Ok(report);
        }

        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomerReport()
        {
            var report = await _reportService.GetCustomerReportAsync();
            return Ok(report);
        }
    }
}
