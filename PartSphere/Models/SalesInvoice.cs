using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartSphere.Models
{
    public class SalesInvoice
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public int StaffId { get; set; }
        public User Staff { get; set; } = null!;

        public int? VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        public int MileageAtSale { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal DiscountAmount { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "Cash";

        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "PAID";

        public ICollection<SalesItem> Items { get; set; } = new List<SalesItem>();
        public Invoice? Invoice { get; set; }
    }
}
