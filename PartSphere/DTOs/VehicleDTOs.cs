using System.ComponentModel.DataAnnotations;

namespace PartSphere.DTOs
{
    public class VehicleDto
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public int Mileage { get; set; }
        public DateTime? LastServiceDate { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
    }

    public class CreateVehicleDto
    {
        [Required, MaxLength(50)]
        public string Brand { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Model { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string VehicleNumber { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int Mileage { get; set; }

        [Required]
        public int CustomerId { get; set; }
    }

    public class UpdateVehicleDto
    {
        [MaxLength(50)]
        public string? Brand { get; set; }

        [MaxLength(100)]
        public string? Model { get; set; }

        [Range(0, int.MaxValue)]
        public int? Mileage { get; set; }

        public DateTime? LastServiceDate { get; set; }
    }
}
