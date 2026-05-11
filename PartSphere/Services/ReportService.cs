using Microsoft.EntityFrameworkCore;
using PartSphere.DTOs;
using PartSphere.Models;
using PartSphere.Repositories;
using PartSphere.Data;

namespace PartSphere.Services
{
    public interface IReportService
    {
        Task<ReportDto> GetDailyReportAsync(DateTime date);
        Task<ReportDto> GetMonthlyReportAsync(int year, int month);
        Task<ReportDto> GetYearlyReportAsync(int year);
        Task<CustomerReportDto> GetCustomerReportAsync();
        Task<DashboardDto> GetDashboardStatsAsync();
        Task<IEnumerable<SalesReportItemDto>> GetSalesReportAsync(DateTime from, DateTime to);
        Task<IEnumerable<MonthlySalesDto>> GetRevenueTrendAsync();
    }

    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ReportDto> GetDailyReportAsync(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await GenerateFinancialReport("Daily", startOfDay, endOfDay);
        }

        public async Task<ReportDto> GetMonthlyReportAsync(int year, int month)
        {
            var startOfMonth = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfMonth = startOfMonth.AddMonths(1);

            return await GenerateFinancialReport("Monthly", startOfMonth, endOfMonth);
        }

        public async Task<ReportDto> GetYearlyReportAsync(int year)
        {
            var startOfYear = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfYear = startOfYear.AddYears(1);

            return await GenerateFinancialReport("Yearly", startOfYear, endOfYear);
        }

        private async Task<ReportDto> GenerateFinancialReport(string period, DateTime start, DateTime end)
        {
            var invoices = await _context.SalesInvoices
                .Include(s => s.Items)
                .Where(s => s.Date >= start && s.Date < end)
                .ToListAsync();

            var totalRevenue = invoices.Sum(s => s.TotalAmount);
            var totalSales = invoices.Count;
            var totalItemsSold = invoices.SelectMany(s => s.Items).Sum(i => i.Quantity);

            return new ReportDto
            {
                Period = period,
                TotalRevenue = totalRevenue,
                TotalSales = totalSales,
                TotalItemsSold = totalItemsSold,
                AverageOrderValue = totalSales > 0 ? totalRevenue / totalSales : 0
            };
        }

        public async Task<CustomerReportDto> GetCustomerReportAsync()
        {
            var topSpenders = await _context.SalesInvoices
                .Include(s => s.Customer)
                .GroupBy(s => new { s.CustomerId, s.Customer.Name })
                .Select(g => new TopSpenderDto
                {
                    CustomerId = g.Key.CustomerId,
                    CustomerName = g.Key.Name,
                    TotalSpent = g.Sum(s => s.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(10)
                .ToListAsync();

            var frequentCustomers = await _context.SalesInvoices
                .Include(s => s.Customer)
                .GroupBy(s => new { s.CustomerId, s.Customer.Name })
                .Select(g => new FrequentCustomerDto
                {
                    CustomerId = g.Key.CustomerId,
                    CustomerName = g.Key.Name,
                    VisitCount = g.Count(),
                    LastVisit = g.Max(s => s.Date)
                })
                .OrderByDescending(x => x.VisitCount)
                .Take(10)
                .ToListAsync();

            var pendingCredits = await _context.CreditPayments
                .Include(c => c.Customer)
                .Where(c => c.Status != CreditStatus.Paid)
                .Select(c => new CreditPaymentDto
                {
                    Id = c.Id,
                    CustomerId = c.CustomerId,
                    CustomerName = c.Customer.Name,
                    SalesInvoiceId = c.SalesInvoiceId,
                    DueAmount = c.DueAmount,
                    DueDate = c.DueDate,
                    Status = c.Status.ToString(),
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return new CustomerReportDto
            {
                TopSpenders = topSpenders,
                FrequentCustomers = frequentCustomers,
                PendingCredits = pendingCredits
            };
        }

        public async Task<DashboardDto> GetDashboardStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var todayInvoices = await _context.SalesInvoices
                .Where(s => s.Date >= today)
                .ToListAsync();

            var monthInvoices = await _context.SalesInvoices
                .Where(s => s.Date >= startOfMonth)
                .ToListAsync();

            return new DashboardDto
            {
                TotalParts = await _context.VehicleParts.CountAsync(),
                TotalCustomers = await _context.Customers.CountAsync(),
                TotalVendors = await _context.Vendors.CountAsync(),
                TotalStaff = await _context.Users.CountAsync(u => u.Role == UserRole.Staff),
                TodayRevenue = todayInvoices.Sum(s => s.TotalAmount),
                MonthRevenue = monthInvoices.Sum(s => s.TotalAmount),
                TodaySalesCount = todayInvoices.Count,
                MonthSalesCount = monthInvoices.Count,
                LowStockCount = await _context.VehicleParts.CountAsync(p => p.StockQuantity < 10),
                PendingAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending),
                PendingCredits = await _context.CreditPayments.CountAsync(c => c.Status != CreditStatus.Paid),
                RecentSales = await _context.SalesInvoices
                    .Include(s => s.Customer)
                    .OrderByDescending(s => s.Date)
                    .Take(5)
                    .Select(s => new RecentSaleDto
                    {
                        Id = s.Id,
                        CustomerName = s.Customer.Name,
                        TotalAmount = s.TotalAmount,
                        Date = s.Date
                    }).ToListAsync(),
                RecentNotifications = await _context.Notifications
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(5)
                    .Select(n => new NotificationDto
                    {
                        Id = n.Id,
                        Message = n.Message,
                        Type = n.Type.ToString(),
                        IsRead = n.IsRead,
                        CreatedAt = n.CreatedAt
                    }).ToListAsync()
            };
        }

        public async Task<IEnumerable<SalesReportItemDto>> GetSalesReportAsync(DateTime from, DateTime to)
        {
            var invoices = await _context.SalesInvoices
                .Include(s => s.Customer)
                .Include(s => s.Staff)
                .Include(s => s.Items)
                .Where(s => s.Date >= from && s.Date <= to)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            return invoices.Select(s => new SalesReportItemDto
            {
                InvoiceId = s.Id,
                CustomerName = s.Customer?.Name ?? "Walk-in",
                StaffName = s.Staff?.Name ?? "System",
                TotalAmount = s.TotalAmount,
                DiscountAmount = s.DiscountAmount,
                PaymentMethod = s.PaymentMethod,
                Date = s.Date,
                ItemCount = s.Items.Count
            });
        }

        public async Task<IEnumerable<MonthlySalesDto>> GetRevenueTrendAsync()
        {
            var now = DateTime.UtcNow;
            var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-11);
            
            var invoices = await _context.SalesInvoices
                .Where(s => s.Date >= start)
                .ToListAsync();

            var trends = new List<MonthlySalesDto>();
            for (int i = 0; i < 12; i++)
            {
                var monthStart = start.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1);
                var monthInvoices = invoices.Where(s => s.Date >= monthStart && s.Date < monthEnd);

                trends.Add(new MonthlySalesDto
                {
                    Month = monthStart.ToString("MMM"),
                    Total = monthInvoices.Sum(s => s.TotalAmount),
                    Count = monthInvoices.Count()
                });
            }

            return trends;
        }
    }
}
