using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartSphere.Services;

namespace PartSphere.Controllers.Staff
{
    [ApiController]
    [Route("api/staff/reports")]
    [Authorize(Roles = "Admin,Staff")]
    public class StaffReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public StaffReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("customer-reports")]
        public async Task<IActionResult> GetCustomerReports()
        {
            var reports = await _reportService.GetCustomerReportAsync();
            return Ok(reports);
        }

        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> GetStaffDashboardSummary()
        {
            var stats = await _reportService.GetDashboardStatsAsync();
            return Ok(stats);
        }
    }
}
