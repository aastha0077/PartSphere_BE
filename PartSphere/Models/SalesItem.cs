using System.ComponentModel.DataAnnotations.Schema;

namespace PartSphere.Models
{
    public class SalesItem
    {
        public int Id { get; set; }

        public int SalesInvoiceId { get; set; }
        public SalesInvoice SalesInvoice { get; set; } = null!;

        public int VehiclePartId { get; set; }
        public VehiclePart VehiclePart { get; set; } = null!;

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalPrice { get; set; }
    }
}
