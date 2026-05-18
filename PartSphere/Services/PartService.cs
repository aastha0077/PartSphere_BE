using Microsoft.EntityFrameworkCore;
using PartSphere.DTOs;
using PartSphere.Models;
using PartSphere.Repositories;

namespace PartSphere.Services
{
    public interface IPartService
    {
        Task<(IEnumerable<PartDto> Items, int Total)> GetAllAsync(string? search, string? category, int page, int pageSize);
        Task<PartDto> GetByIdAsync(int id);
        Task<PartDto> CreateAsync(CreatePartDto dto);
        Task<PartDto> UpdateAsync(int id, UpdatePartDto dto);
        Task<PartDto> UpdateStockAsync(int id, UpdateStockDto dto);
        Task DeleteAsync(int id);
    }

    public class PartService : IPartService
    {
        private readonly IRepository<VehiclePart> _partRepo;
        private readonly IRepository<Vendor> _vendorRepo;
        private readonly INotificationService _notificationService;
        private readonly IAdminLowStockNotifier _adminLowStockNotifier;
        private readonly ILogger<PartService> _logger;

        public PartService(
            IRepository<VehiclePart> partRepo,
            IRepository<Vendor> vendorRepo,
            INotificationService notificationService,
            IAdminLowStockNotifier adminLowStockNotifier,
            ILogger<PartService> logger)
        {
            _partRepo = partRepo;
            _vendorRepo = vendorRepo;
            _notificationService = notificationService;
            _adminLowStockNotifier = adminLowStockNotifier;
            _logger = logger;
        }

        public async Task<(IEnumerable<PartDto> Items, int Total)> GetAllAsync(string? search, string? category, int page, int pageSize)
        {
            var query = _partRepo.Query()
                .Include(p => p.Vendor)
                .Where(p => !p.IsDeleted);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()) ||
                                         p.Brand.ToLower().Contains(search.ToLower()));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            var total = await query.CountAsync();
            var parts = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (parts.Select(MapToDto), total);
        }

        public async Task<PartDto> GetByIdAsync(int id)
        {
            var part = await _partRepo.Query()
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted)
                ?? throw new KeyNotFoundException("Part not found.");

            return MapToDto(part);
        }

        public async Task<PartDto> CreateAsync(CreatePartDto dto)
        {
            if (!await _vendorRepo.ExistsAsync(dto.VendorId))
                throw new KeyNotFoundException("Vendor not found.");

            var part = new VehiclePart
            {
                Name = dto.Name,
                Brand = dto.Brand,
                Category = dto.Category,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                VendorId = dto.VendorId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _partRepo.AddAsync(part);
            await CheckLowStockAsync(part, previousStockQuantity: null);

            return MapToDto(part);
        }

        public async Task<PartDto> UpdateAsync(int id, UpdatePartDto dto)
        {
            var part = await _partRepo.Query()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted)
                ?? throw new KeyNotFoundException("Part not found.");

            if (!part.RowVersion.SequenceEqual(dto.RowVersion))
                throw new DbUpdateConcurrencyException("The record has been modified by another user. Please refresh.");

            if (!await _vendorRepo.ExistsAsync(dto.VendorId))
                throw new KeyNotFoundException("Vendor not found.");

            var previousQty = part.StockQuantity;

            part.Name = dto.Name;
            part.Brand = dto.Brand;
            part.Category = dto.Category;
            part.Price = dto.Price;
            part.StockQuantity = dto.StockQuantity;
            part.Description = dto.Description;
            part.ImageUrl = dto.ImageUrl;
            part.VendorId = dto.VendorId;
            part.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _partRepo.UpdateAsync(part);
                await CheckLowStockAsync(part, previousQty);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new DbUpdateConcurrencyException("The record was updated by another process. Please reload.");
            }

            return await GetByIdAsync(part.Id);
        }

        public async Task<PartDto> UpdateStockAsync(int id, UpdateStockDto dto)
        {
            var part = await _partRepo.Query()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted)
                ?? throw new KeyNotFoundException("Part not found.");

            if (!part.RowVersion.SequenceEqual(dto.RowVersion))
                throw new DbUpdateConcurrencyException("Concurrency conflict. Stock update rejected.");

            var previousQty = part.StockQuantity;

            part.StockQuantity = dto.StockQuantity;
            part.UpdatedAt = DateTime.UtcNow;

            await _partRepo.UpdateAsync(part);
            await CheckLowStockAsync(part, previousQty);

            return await GetByIdAsync(part.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var part = await _partRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Part not found.");

            part.IsDeleted = true;
            part.UpdatedAt = DateTime.UtcNow;
            await _partRepo.UpdateAsync(part);
        }

        private async Task CheckLowStockAsync(VehiclePart part, int? previousStockQuantity)
        {
            if (part.StockQuantity >= 10)
                return;

            await _notificationService.CreateAsync(
                $"LOW STOCK ALERT: {part.Name} ({part.Brand}) is running low ({part.StockQuantity} left).",
                NotificationType.LowStock);

            await _adminLowStockNotifier.NotifyIfCrossedLowThresholdAsync(
                part.Name,
                part.Brand,
                part.StockQuantity,
                previousStockQuantity);
        }

        private static PartDto MapToDto(VehiclePart p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Brand = p.Brand,
            Category = p.Category,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            Description = p.Description,
            ImageUrl = p.ImageUrl,
            VendorId = p.VendorId,
            VendorName = p.Vendor?.Name ?? "Unknown",
            UpdatedAt = p.UpdatedAt,
            RowVersion = p.RowVersion
        };
    }
}
