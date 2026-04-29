using Microsoft.EntityFrameworkCore;
using PartSphere.DTOs;
using PartSphere.Models;
using PartSphere.Repositories;

namespace PartSphere.Services
{
    /// <summary>
    /// Handles purchase invoices and automatically increases stock.
    /// </summary>
    public interface IPurchaseService
    {
        Task<PurchaseInvoiceDto> GetByIdAsync(int id);
        Task<IEnumerable<PurchaseInvoiceDto>> GetAllAsync();
        Task<PurchaseInvoiceDto> CreateAsync(CreatePurchaseInvoiceDto dto);
    }

    public class PurchaseService : IPurchaseService
    {
        private readonly IRepository<PurchaseInvoice> _purchaseRepo;
        private readonly IRepository<Vendor> _vendorRepo;
        private readonly IRepository<VehiclePart> _partRepo;
        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(
            IRepository<PurchaseInvoice> purchaseRepo,
            IRepository<Vendor> vendorRepo,
            IRepository<VehiclePart> partRepo,
            ILogger<PurchaseService> logger)
        {
            _purchaseRepo = purchaseRepo;
            _vendorRepo = vendorRepo;
            _partRepo = partRepo;
            _logger = logger;
        }

        public async Task<PurchaseInvoiceDto> GetByIdAsync(int id)
        {
            var invoice = await _purchaseRepo.Query()
                .Include(p => p.Vendor)
                .Include(p => p.Items)
                    .ThenInclude(i => i.VehiclePart)
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new KeyNotFoundException("Purchase invoice not found.");

            return MapToDto(invoice);
        }

        public async Task<IEnumerable<PurchaseInvoiceDto>> GetAllAsync()
        {
            var invoices = await _purchaseRepo.Query()
                .Include(p => p.Vendor)
                .Include(p => p.Items)
                    .ThenInclude(i => i.VehiclePart)
                .OrderByDescending(p => p.Date)
                .ToListAsync();

            return invoices.Select(MapToDto);
        }

        public async Task<PurchaseInvoiceDto> CreateAsync(CreatePurchaseInvoiceDto dto)
        {
            if (!await _vendorRepo.ExistsAsync(dto.VendorId))
                throw new KeyNotFoundException("Vendor not found.");

            var invoice = new PurchaseInvoice
            {
                VendorId = dto.VendorId,
                Notes = dto.Notes,
                Date = DateTime.UtcNow
            };

            decimal totalAmount = 0;

            foreach (var itemDto in dto.Items)
            {
                var part = await _partRepo.GetByIdAsync(itemDto.VehiclePartId)
                    ?? throw new KeyNotFoundException($"Part {itemDto.VehiclePartId} not found.");

                // INCREASE STOCK
                part.StockQuantity += itemDto.Quantity;
                await _partRepo.UpdateAsync(part);

                var itemCost = itemDto.UnitCost * itemDto.Quantity;
                totalAmount += itemCost;

                invoice.Items.Add(new PurchaseItem
                {
                    VehiclePartId = part.Id,
                    Quantity = itemDto.Quantity,
                    UnitCost = itemDto.UnitCost,
                    TotalCost = itemCost
                });
            }

            invoice.TotalAmount = totalAmount;

            await _purchaseRepo.AddAsync(invoice);

            _logger.LogInformation("Purchase Invoice created: {InvoiceId} for Vendor {VendorId}", invoice.Id, invoice.VendorId);

            return await GetByIdAsync(invoice.Id);
        }

        private static PurchaseInvoiceDto MapToDto(PurchaseInvoice p) => new()
        {
            Id = p.Id,
            VendorId = p.VendorId,
            VendorName = p.Vendor?.Name ?? "",
            TotalAmount = p.TotalAmount,
            Date = p.Date,
            Notes = p.Notes,
            Items = p.Items.Select(i => new PurchaseItemDto
            {
                Id = i.Id,
                VehiclePartId = i.VehiclePartId,
                PartName = i.VehiclePart?.Name ?? "",
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                TotalCost = i.TotalCost
            }).ToList()
        };
    }
}
