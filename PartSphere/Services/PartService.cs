using Microsoft.EntityFrameworkCore;
using PartSphere.DTOs;
using PartSphere.Models;
using PartSphere.Repositories;

namespace PartSphere.Services
{
    /// <summary>
    /// Handles CRUD operations for vehicle parts.
    /// Triggers low stock notifications when StockQuantity < 10.
    /// </summary>
    public interface IPartService
    {
        Task<IEnumerable<VehiclePartDto>> GetAllAsync();
        Task<VehiclePartDto> GetByIdAsync(int id);
        Task<VehiclePartDto> CreateAsync(CreatePartDto dto);
        Task<VehiclePartDto> UpdateAsync(int id, UpdatePartDto dto);
        Task DeleteAsync(int id);
        Task<IEnumerable<VehiclePartDto>> SearchAsync(string query);
        Task<IEnumerable<VehiclePartDto>> GetLowStockAsync();
    }

    public class PartService : IPartService
    {
        private readonly IRepository<VehiclePart> _partRepo;
        private readonly IRepository<Vendor> _vendorRepo;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PartService> _logger;

        public PartService(
            IRepository<VehiclePart> partRepo,
            IRepository<Vendor> vendorRepo,
            INotificationService notificationService,
            ILogger<PartService> logger)
        {
            _partRepo = partRepo;
            _vendorRepo = vendorRepo;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<IEnumerable<VehiclePartDto>> GetAllAsync()
        {
            var parts = await _partRepo.Query()
                .Include(p => p.Vendor)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return parts.Select(MapToDto);
        }

        public async Task<VehiclePartDto> GetByIdAsync(int id)
        {
            var part = await _partRepo.Query()
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new KeyNotFoundException("Part not found.");

            return MapToDto(part);
        }

        public async Task<VehiclePartDto> CreateAsync(CreatePartDto dto)
        {
            if (!await _vendorRepo.ExistsAsync(dto.VendorId))
                throw new KeyNotFoundException("Vendor not found.");

            var part = new VehiclePart
            {
                Name = dto.Name,
                Brand = dto.Brand,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                Description = dto.Description,
                VendorId = dto.VendorId
            };

            await _partRepo.AddAsync(part);

            // Check low stock alert
            await CheckLowStock(part);

            _logger.LogInformation("Part created: {Name}", dto.Name);

            return await GetByIdAsync(part.Id);
        }

        public async Task<VehiclePartDto> UpdateAsync(int id, UpdatePartDto dto)
        {
            var part = await _partRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Part not found.");

            if (dto.Name != null) part.Name = dto.Name;
            if (dto.Brand != null) part.Brand = dto.Brand;
            if (dto.Price.HasValue) part.Price = dto.Price.Value;
            if (dto.StockQuantity.HasValue) part.StockQuantity = dto.StockQuantity.Value;
            if (dto.Description != null) part.Description = dto.Description;
            if (dto.VendorId.HasValue)
            {
                if (!await _vendorRepo.ExistsAsync(dto.VendorId.Value))
                    throw new KeyNotFoundException("Vendor not found.");
                part.VendorId = dto.VendorId.Value;
            }

            await _partRepo.UpdateAsync(part);

            // Check low stock alert
            await CheckLowStock(part);

            return await GetByIdAsync(part.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var part = await _partRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Part not found.");

            await _partRepo.DeleteAsync(part);
            _logger.LogInformation("Part deleted: {Id}", id);
        }

        public async Task<IEnumerable<VehiclePartDto>> SearchAsync(string query)
        {
            var parts = await _partRepo.Query()
                .Include(p => p.Vendor)
                .Where(p => p.Name.ToLower().Contains(query.ToLower()) ||
                             p.Brand.ToLower().Contains(query.ToLower()))
                .ToListAsync();

            return parts.Select(MapToDto);
        }

        public async Task<IEnumerable<VehiclePartDto>> GetLowStockAsync()
        {
            var parts = await _partRepo.Query()
                .Include(p => p.Vendor)
                .Where(p => p.StockQuantity < 10)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();

            return parts.Select(MapToDto);
        }

        /// <summary>
        /// LOW STOCK ALERT: If StockQuantity < 10, create notification for Admin.
        /// </summary>
        private async Task CheckLowStock(VehiclePart part)
        {
            if (part.StockQuantity < 10)
            {
                await _notificationService.CreateAsync(
                    $"Low stock alert: {part.Name} has only {part.StockQuantity} units left.",
                    NotificationType.LowStock);
            }
        }

        private static VehiclePartDto MapToDto(VehiclePart p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Brand = p.Brand,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            Description = p.Description,
            VendorId = p.VendorId,
            VendorName = p.Vendor?.Name ?? ""
        };
    }
}
