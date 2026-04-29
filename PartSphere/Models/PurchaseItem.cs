using System.ComponentModel.DataAnnotations.Schema;

namespace PartSphere.Models
{
    public class PurchaseItem
    {
        public int Id { get; set; }

        public int PurchaseInvoiceId { get; set; }
        public PurchaseInvoice PurchaseInvoice { get; set; } = null!;

        public int VehiclePartId { get; set; }
        public VehiclePart VehiclePart { get; set; } = null!;

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitCost { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalCost { get; set; }
    }
}
