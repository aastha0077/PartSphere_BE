using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartSphere.Services;

namespace PartSphere.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/reports")]
    [Authorize(Roles = "Admin,Staff")]
    public class AdminReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public AdminReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("financial-summary")]
        public async Task<IActionResult> GetFinancialSummary()
        {
            var dashboard = await _reportService.GetDashboardStatsAsync();
            return Ok(dashboard);
        }

        [HttpGet("top-customers")]
        public async Task<IActionResult> GetTopCustomers()
        {
            var report = await _reportService.GetCustomerReportAsync();
            return Ok(report.TopSpenders);
        }

        [HttpGet("frequent-buyers")]
        public async Task<IActionResult> GetFrequentBuyers()
        {
            var report = await _reportService.GetCustomerReportAsync();
            return Ok(report.FrequentCustomers);
        }

        [HttpGet("pending-payments")]
        public async Task<IActionResult> GetPendingPayments()
        {
            var report = await _reportService.GetCustomerReportAsync();
            return Ok(report.PendingCredits);
        }

        [HttpGet("sales-trends")]
        public async Task<IActionResult> GetSalesTrends([FromQuery] string range = "monthly")
        {
            var to = DateTime.UtcNow;
            var from = range.ToLower() == "monthly" ? to.AddMonths(-1) : to.AddDays(-7);
            var report = await _reportService.GetSalesReportAsync(from, to);
            return Ok(report);
        }

        [HttpGet("revenue-trend")]
        public async Task<IActionResult> GetRevenueTrend()
        {
            var trends = await _reportService.GetRevenueTrendAsync();
            return Ok(trends);
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyReport([FromQuery] DateTime? date)
        {
            var report = await _reportService.GetDailyReportAsync(date ?? DateTime.UtcNow);
            return Ok(report);
        }

        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlyReport([FromQuery] int? year, [FromQuery] int? month)
        {
            var now = DateTime.UtcNow;
            var report = await _reportService.GetMonthlyReportAsync(year ?? now.Year, month ?? now.Month);
            return Ok(report);
        }

        [HttpGet("yearly")]
        public async Task<IActionResult> GetYearlyReport([FromQuery] int? year)
        {
            var report = await _reportService.GetYearlyReportAsync(year ?? DateTime.UtcNow.Year);
            return Ok(report);
        }
    }
}
