using System.ComponentModel.DataAnnotations;

namespace PartSphere.DTOs
{
    public class SalesInvoiceDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public List<SalesItemDto> Items { get; set; } = new();
    }

    public class InvoiceDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public string? PDFUrl { get; set; }
        public string? SentToEmail { get; set; }
    }

    public class SalesItemDto
    {
        public int Id { get; set; }
        public int VehiclePartId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CreateSalesInvoiceDto
    {
        [Required]
        public int CustomerId { get; set; }

        public int? VehicleId { get; set; }

        [Required, MaxLength(50)]
        public string PaymentMethod { get; set; } = "Cash";

        [Required, MaxLength(50)]
        public string PaymentStatus { get; set; } = "PAID";

        [Required]
        public List<CreateSalesItemDto> Items { get; set; } = new();
    }

    public class CreateSalesItemDto
    {
        [Required]
        public int VehiclePartId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class PurchaseInvoiceDto
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<PurchaseItemDto> Items { get; set; } = new();
    }

    public class PurchaseItemDto
    {
        public int Id { get; set; }
        public int VehiclePartId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class CreatePurchaseInvoiceDto
    {
        [Required]
        public int VendorId { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        [Required]
        public List<CreatePurchaseItemDto> Items { get; set; } = new();
    }

    public class CreatePurchaseItemDto
    {
        [Required]
        public int VehiclePartId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal UnitCost { get; set; }
    }
}
