using System.ComponentModel.DataAnnotations;

namespace PartSphere.Models
{
    public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        Completed,
        Cancelled
    }

    public class Appointment
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public int? VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        [Required, MaxLength(100)]
        public string ServiceType { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
