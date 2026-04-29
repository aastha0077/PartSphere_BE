using System.ComponentModel.DataAnnotations;

namespace PartSphere.DTOs
{
    public class VendorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int PartsCount { get; set; }
    }

    public class CreateVendorDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Contact { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateVendorDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? Contact { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }
    }
}
