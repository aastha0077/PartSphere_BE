using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartSphere.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        public int SalesInvoiceId { get; set; }
        public SalesInvoice SalesInvoice { get; set; } = null!;

        [MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

        [MaxLength(255)]
        public string? PDFUrl { get; set; }

        [MaxLength(150)]
        public string? SentToEmail { get; set; }
    }
}
