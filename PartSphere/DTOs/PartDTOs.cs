using System.ComponentModel.DataAnnotations;

namespace PartSphere.DTOs
{
    public class PartDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Description { get; set; } = string.Empty;
        public int VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public bool IsLowStock => StockQuantity < 10;
        public DateTime UpdatedAt { get; set; }
        public byte[] RowVersion { get; set; } = null!;
    }

    public class CreatePartDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        [Range(0.01, 1000000)]
        public decimal Price { get; set; }

        [Range(0, 1000000)]
        public int StockQuantity { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int VendorId { get; set; }
    }

    public class UpdatePartDto : CreatePartDto
    {
        [Required]
        public byte[] RowVersion { get; set; } = null!;
    }

    public class UpdateStockDto
    {
        [Range(0, 1000000)]
        public int StockQuantity { get; set; }
        
        [Required]
        public byte[] RowVersion { get; set; } = null!;
    }
}
