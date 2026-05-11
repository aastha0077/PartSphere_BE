using Microsoft.EntityFrameworkCore;
using PartSphere.DTOs;
using PartSphere.Models;
using PartSphere.Repositories;
using PartSphere.Data;

namespace PartSphere.Services
{
    public interface IPurchaseService
    {
        Task<PurchaseInvoiceDto> GetByIdAsync(int id);
        Task<IEnumerable<PurchaseInvoiceDto>> GetAllAsync(int? vendorId = null);
        Task<PurchaseInvoiceDto> CreateAsync(CreatePurchaseInvoiceDto dto);
    }

    public class PurchaseService : IPurchaseService
    {
        private readonly AppDbContext _context;
        private readonly IRepository<PurchaseInvoice> _purchaseRepo;
        private readonly IRepository<Vendor> _vendorRepo;
        private readonly IRepository<VehiclePart> _partRepo;
        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(
            AppDbContext context,
            IRepository<PurchaseInvoice> purchaseRepo,
            IRepository<Vendor> vendorRepo,
            IRepository<VehiclePart> partRepo,
            ILogger<PurchaseService> logger)
        {
            _context = context;
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

        public async Task<IEnumerable<PurchaseInvoiceDto>> GetAllAsync(int? vendorId = null)
        {
            var query = _purchaseRepo.Query()
                .Include(p => p.Vendor)
                .Include(p => p.Items)
                    .ThenInclude(i => i.VehiclePart)
                .AsQueryable();

            if (vendorId.HasValue)
            {
                query = query.Where(p => p.VendorId == vendorId.Value);
            }

            var invoices = await query
                .OrderByDescending(p => p.Date)
                .ToListAsync();

            return invoices.Select(MapToDto);
        }

        public async Task<PurchaseInvoiceDto> CreateAsync(CreatePurchaseInvoiceDto dto)
        {
            if (!await _vendorRepo.ExistsAsync(dto.VendorId))
                throw new KeyNotFoundException("Vendor not found.");

            if (dto.Items == null || !dto.Items.Any())
                throw new ArgumentException("Purchase must contain at least one item.");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
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

                    part.StockQuantity += itemDto.Quantity;

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
                
                await transaction.CommitAsync();

                _logger.LogInformation("Purchase Invoice created: {InvoiceId} for Vendor {VendorId}", invoice.Id, invoice.VendorId);

                return await GetByIdAsync(invoice.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Transaction failed while creating purchase.");
                throw;
            }
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
