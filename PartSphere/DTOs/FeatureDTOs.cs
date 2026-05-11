using System.ComponentModel.DataAnnotations;

namespace PartSphere.DTOs
{
    // ===== PART REQUEST =====
    public class PartRequestDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? StaffNotes { get; set; }
    }

    public class CreatePartRequestDto
    {
        public int CustomerId { get; set; }

        [Required, MaxLength(200)]
        public string PartName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Brand { get; set; }

        [Required, MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
    }

    // ===== APPOINTMENT =====
    public class AppointmentDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int? VehicleId { get; set; }
        public string VehicleInfo { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAppointmentDto
    {
        public int CustomerId { get; set; }
        public int? VehicleId { get; set; }

        [Required, MaxLength(100)]
        public string ServiceType { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateAppointmentStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }

    // ===== REVIEW =====
    public class ReviewDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateReviewDto
    {
        public int CustomerId { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; } = string.Empty;
    }

    // ===== NOTIFICATION =====
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ===== CREDIT PAYMENT =====
    public class CreditPaymentDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int? SalesInvoiceId { get; set; }
        public decimal DueAmount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidDate { get; set; }
    }

    public class CreateCreditPaymentDto
    {
        [Required]
        public int CustomerId { get; set; }
        public int? SalesInvoiceId { get; set; }

        [Required]
        public decimal DueAmount { get; set; }

        [Required]
        public DateTime DueDate { get; set; }
    }

    // ===== DASHBOARD & REPORTS =====
    public class DashboardDto
    {
        public int TotalParts { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalVendors { get; set; }
        public int TotalStaff { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public int TodaySalesCount { get; set; }
        public int MonthSalesCount { get; set; }
        public int LowStockCount { get; set; }
        public int PendingAppointments { get; set; }
        public int PendingCredits { get; set; }
        public List<RecentSaleDto> RecentSales { get; set; } = new();
        public List<NotificationDto> RecentNotifications { get; set; } = new();
    }

    public class RecentSaleDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime Date { get; set; }
    }

    public class ReportDto
    {
        public string Period { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TotalSales { get; set; }
        public int TotalItemsSold { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class CustomerReportDto
    {
        public List<TopSpenderDto> TopSpenders { get; set; } = new();
        public List<FrequentCustomerDto> FrequentCustomers { get; set; } = new();
        public List<CreditPaymentDto> PendingCredits { get; set; } = new();
    }

    public class TopSpenderDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalSpent { get; set; }
        public int OrderCount { get; set; }
    }

    public class FrequentCustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int VisitCount { get; set; }
        public DateTime LastVisit { get; set; }
    }

    public class MonthlySalesDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int Count { get; set; }
    }

    public class TopPartDto
    {
        public string PartName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
    }

    // ===== SALES REPORT =====
    public class SalesReportItemDto
    {
        public int InvoiceId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string StaffName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int ItemCount { get; set; }
    }
}
