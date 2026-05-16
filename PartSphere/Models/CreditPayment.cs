using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartSphere.Models
{
    public enum CreditStatus
    {
        Pending,
        Paid,
        Overdue
    }

    public class CreditPayment
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public int? SalesInvoiceId { get; set; }
        public SalesInvoice? SalesInvoice { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal DueAmount { get; set; }

        public DateTime DueDate { get; set; }

        public CreditStatus Status { get; set; } = CreditStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PaidDate { get; set; }

        public DateTime? LastCreditReminderSentAt { get; set; }
    }
}
