using System.ComponentModel.DataAnnotations;

namespace PartSphere.DTOs
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public int VehicleCount { get; set; }
        public decimal TotalSpent { get; set; }
        public int LoyaltyPoints { get; set; }
    }

    public class CreateCustomerDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        /// <summary>Optional vehicle to register with the customer in one step.</summary>
        public CreateCustomerVehicleDto? Vehicle { get; set; }
    }

    public class CreateCustomerVehicleDto
    {
        [Required, MaxLength(20)]
        public string VehicleNumber { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Brand { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Model { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int Mileage { get; set; }
    }

    public class UpdateCustomerDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [EmailAddress, MaxLength(150)]
        public string? Email { get; set; }
    }

    public class CustomerHistoryDto
    {
        public CustomerDto Customer { get; set; } = null!;
        public List<VehicleDto> Vehicles { get; set; } = new();
        public List<SalesInvoiceDto> Purchases { get; set; } = new();
        public List<AppointmentDto> Appointments { get; set; } = new();
        public decimal TotalSpent { get; set; }
        public bool IsLoyalCustomer { get; set; }
    }

    public class CustomerSearchResultDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> VehicleNumbers { get; set; } = new();
    }
}
