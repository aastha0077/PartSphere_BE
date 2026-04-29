using System.ComponentModel.DataAnnotations;

namespace PartSphere.Models
{
    public class Vehicle
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Brand { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Model { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string VehicleNumber { get; set; } = string.Empty;

        public int Mileage { get; set; }

        public DateTime? LastServiceDate { get; set; }

        // Foreign key
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
    }
}
