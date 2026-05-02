using System.ComponentModel.DataAnnotations;

namespace PartSphere.Models
{
    public class PartRequest
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Required, MaxLength(200)]
        public string PartName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Brand { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, InProcess, Available, Cancelled

        public string? StaffNotes { get; set; }
    }
}
