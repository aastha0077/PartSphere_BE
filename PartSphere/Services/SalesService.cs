using Microsoft.EntityFrameworkCore;
using PartSphere.DTOs;
using PartSphere.Helpers;
using PartSphere.Models;
using PartSphere.Repositories;

namespace PartSphere.Services
{
    /// <summary>
    /// Handles sales creation, stock reduction, loyalty program, and email invoices.
    /// </summary>
    public interface ISalesService
    {
        Task<SalesInvoiceDto> GetByIdAsync(int id);
        Task<IEnumerable<SalesInvoiceDto>> GetAllAsync();
        Task<SalesInvoiceDto> CreateAsync(int staffId, CreateSalesInvoiceDto dto);
    }

    public class SalesService : ISalesService
    {
        private readonly IRepository<SalesInvoice> _salesRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<VehiclePart> _partRepo;
        private readonly IEmailService _emailService;
        private readonly ILogger<SalesService> _logger;

        public SalesService(
            IRepository<SalesInvoice> salesRepo,
            IRepository<Customer> customerRepo,
            IRepository<VehiclePart> partRepo,
            IEmailService emailService,
            ILogger<SalesService> logger)
        {
            _salesRepo = salesRepo;
            _customerRepo = customerRepo;
            _partRepo = partRepo;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<SalesInvoiceDto> GetByIdAsync(int id)
        {
            var invoice = await _salesRepo.Query()
                .Include(s => s.Customer)
                .Include(s => s.Staff)
                .Include(s => s.Items)
                    .ThenInclude(i => i.VehiclePart)
                .FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new KeyNotFoundException("Sales invoice not found.");

            return MapToDto(invoice);
        }

        public async Task<IEnumerable<SalesInvoiceDto>> GetAllAsync()
        {
            var invoices = await _salesRepo.Query()
                .Include(s => s.Customer)
                .Include(s => s.Staff)
                .Include(s => s.Items)
                    .ThenInclude(i => i.VehiclePart)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            return invoices.Select(MapToDto);
        }

        public async Task<SalesInvoiceDto> CreateAsync(int staffId, CreateSalesInvoiceDto dto)
        {
            var customer = await _customerRepo.Query()
                .Include(c => c.SalesInvoices)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == dto.CustomerId)
                ?? throw new KeyNotFoundException("Customer not found.");

            var invoice = new SalesInvoice
            {
                CustomerId = dto.CustomerId,
                StaffId = staffId,
                PaymentMethod = dto.PaymentMethod,
                Date = DateTime.UtcNow
            };

            decimal totalAmount = 0;

            foreach (var itemDto in dto.Items)
            {
                var part = await _partRepo.GetByIdAsync(itemDto.VehiclePartId)
                    ?? throw new KeyNotFoundException($"Part {itemDto.VehiclePartId} not found.");

                if (part.StockQuantity < itemDto.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for {part.Name}. Available: {part.StockQuantity}");

                // Reduce stock
                part.StockQuantity -= itemDto.Quantity;
                await _partRepo.UpdateAsync(part);

                var itemPrice = part.Price * itemDto.Quantity;
                totalAmount += itemPrice;

                invoice.Items.Add(new SalesItem
                {
                    VehiclePartId = part.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = part.Price,
                    TotalPrice = itemPrice
                });
            }

            // LOYALTY PROGRAM: Apply 10% discount if total > 5000
            decimal discountAmount = 0;
            if (totalAmount > 5000)
            {
                discountAmount = totalAmount * 0.10m;
            }

            invoice.TotalAmount = totalAmount - discountAmount;
            invoice.DiscountAmount = discountAmount;

            await _salesRepo.AddAsync(invoice);

            _logger.LogInformation("Sales Invoice created: {InvoiceId} for Customer {CustomerId}", invoice.Id, customer.Id);

            // SEND EMAIL INVOICE
            if (customer.User != null && !string.IsNullOrEmpty(customer.User.Email))
            {
                await _emailService.SendInvoiceEmailAsync(
                    customer.User.Email,
                    customer.Name,
                    invoice.Id,
                    invoice.TotalAmount);
            }

            return await GetByIdAsync(invoice.Id);
        }

        private static SalesInvoiceDto MapToDto(SalesInvoice s) => new()
        {
            Id = s.Id,
            CustomerId = s.CustomerId,
            CustomerName = s.Customer?.Name ?? "",
            StaffId = s.StaffId,
            StaffName = s.Staff?.Name ?? "",
            TotalAmount = s.TotalAmount,
            DiscountAmount = s.DiscountAmount,
            PaymentMethod = s.PaymentMethod,
            Date = s.Date,
            Items = s.Items.Select(i => new SalesItemDto
            {
                Id = i.Id,
                VehiclePartId = i.VehiclePartId,
                PartName = i.VehiclePart?.Name ?? "",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList()
        };
    }
}
