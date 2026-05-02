using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartSphere.Models
{
    public class PurchaseInvoice
    {
        public int Id { get; set; }

        public int VendorId { get; set; }
        public Vendor Vendor { get; set; } = null!;

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string Status { get; set; } = "Completed";

        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, Credit

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        // Navigation
        public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
    }
}
