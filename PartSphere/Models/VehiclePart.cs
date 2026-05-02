using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartSphere.Models
{
    public class VehiclePart
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Category { get; set; } = "General";

        public int LifespanKm { get; set; } = 15000; // Default replacement after 15k km

        // Concurrency & Audit
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        // Foreign key
        public int VendorId { get; set; }
        public Vendor Vendor { get; set; } = null!;

        // Navigation
        public ICollection<SalesItem> SalesItems { get; set; } = new List<SalesItem>();
        public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    }
}
