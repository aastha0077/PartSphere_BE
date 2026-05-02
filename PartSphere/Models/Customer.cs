using System.ComponentModel.DataAnnotations;

namespace PartSphere.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        public int LoyaltyPoints { get; set; } = 0;

        // Foreign key to User (for self-registered customers)
        public int? UserId { get; set; }
        public User? User { get; set; }

        // Navigation
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<CreditPayment> CreditPayments { get; set; } = new List<CreditPayment>();
    }
}
