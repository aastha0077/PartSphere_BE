using Microsoft.EntityFrameworkCore;
using PartSphere.DTOs;
using PartSphere.Helpers;
using PartSphere.Models;
using PartSphere.Repositories;
using PartSphere.Data;

namespace PartSphere.Services
{
    public interface ISalesService
    {
        Task<SalesInvoiceDto> GetByIdAsync(int id);
        Task<SalesInvoice> GetModelByIdAsync(int id);
        Task<IEnumerable<SalesInvoiceDto>> GetAllAsync();
        Task<SalesInvoiceDto> CreateAsync(int staffId, CreateSalesInvoiceDto dto);
        Task SendInvoiceEmailAsync(int invoiceId);
        Task<SalesInvoiceDto> CompleteOrderAsync(int invoiceId);
    }

    public class SalesService : ISalesService
    {
        private readonly IRepository<SalesInvoice> _salesRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<VehiclePart> _partRepo;
        private readonly IEmailService _emailService;
        private readonly AppDbContext _context;
        private readonly IAdminLowStockNotifier _adminLowStockNotifier;
        private readonly ILogger<SalesService> _logger;

        public SalesService(
            IRepository<SalesInvoice> salesRepo,
            IRepository<Customer> customerRepo,
            IRepository<VehiclePart> partRepo,
            IEmailService emailService,
            AppDbContext context,
            IAdminLowStockNotifier adminLowStockNotifier,
            ILogger<SalesService> logger)
        {
            _salesRepo = salesRepo;
            _customerRepo = customerRepo;
            _partRepo = partRepo;
            _emailService = emailService;
            _context = context;
            _adminLowStockNotifier = adminLowStockNotifier;
            _logger = logger;
        }

        public async Task<SalesInvoiceDto> GetByIdAsync(int id)
        {
            var invoice = await GetModelByIdAsync(id);
            return MapToDto(invoice);
        }

        public async Task<SalesInvoice> GetModelByIdAsync(int id)
        {
            var invoice = await _salesRepo.Query()
                .Include(s => s.Customer)
                .Include(s => s.Staff)
                .Include(s => s.Items)
                    .ThenInclude(i => i.VehiclePart)
                .FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new KeyNotFoundException("Sales invoice not found.");
            return invoice;
        }

        public async Task<IEnumerable<SalesInvoiceDto>> GetAllAsync()
        {
            var invoices = await _salesRepo.Query()
                .Include(s => s.Customer)
                .Include(s => s.Staff)
                .Include(s => s.Items)
                    .ThenInclude(i => i.VehiclePart)
                .OrderByDescending(s => s.Date)
                .ThenByDescending(s => s.Id)
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

            if (dto.Items == null || !dto.Items.Any())
                throw new ArgumentException("Order must contain at least one item.");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var invoice = new SalesInvoice
                {
                    CustomerId = dto.CustomerId,
                    StaffId = staffId,
                    PaymentMethod = dto.PaymentMethod,
                    PaymentStatus = dto.PaymentStatus,
                    Date = DateTime.UtcNow
                };

                decimal totalAmount = 0;

                foreach (var itemDto in dto.Items)
                {
                    var part = await _partRepo.GetByIdAsync(itemDto.VehiclePartId)
                        ?? throw new KeyNotFoundException($"Part {itemDto.VehiclePartId} not found.");

                    if (part.StockQuantity < itemDto.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for {part.Name}. Available: {part.StockQuantity}");

                    var stockBefore = part.StockQuantity;
                    part.StockQuantity -= itemDto.Quantity;

                    if (part.StockQuantity < 10)
                    {
                        await _context.Notifications.AddAsync(new Notification
                        {
                            Message = $"Low stock alert: {part.Name} has dropped below 10 items. Current stock: {part.StockQuantity}",
                            Type = NotificationType.LowStock,
                            CreatedAt = DateTime.UtcNow
                        });
                        _logger.LogWarning("Low stock alert triggered for Part {PartId}", part.Id);
                    }

                    await _adminLowStockNotifier.NotifyIfCrossedLowThresholdAsync(
                        part.Name,
                        part.Brand,
                        part.StockQuantity,
                        stockBefore);

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

                decimal discountAmount = 0;
                if (totalAmount > 5000)
                {
                    discountAmount = totalAmount * 0.10m;
                }

                invoice.TotalAmount = totalAmount - discountAmount;
                invoice.DiscountAmount = discountAmount;

                await _salesRepo.AddAsync(invoice);

                var generatedInvoice = new Invoice
                {
                    SalesInvoiceId = invoice.Id,
                    InvoiceNumber = $"INV-{DateTime.UtcNow.Year}-{invoice.Id:D4}",
                    SentToEmail = customer.User?.Email
                };
                await _context.Invoices.AddAsync(generatedInvoice);

                // Update Loyalty Points: 1 point for every 100 spent
                int earnedPoints = (int)(invoice.TotalAmount / 100);
                customer.LoyaltyPoints += earnedPoints;
                _logger.LogInformation("Customer {CustomerId} earned {Points} loyalty points", customer.Id, earnedPoints);

                if (dto.PaymentStatus == "CREDIT")
                {
                    var credit = new CreditPayment
                    {
                        CustomerId = customer.Id,
                        SalesInvoiceId = invoice.Id,
                        DueAmount = invoice.TotalAmount,
                        DueDate = DateTime.UtcNow.AddMonths(1),
                        Status = CreditStatus.Pending
                    };
                    await _context.CreditPayments.AddAsync(credit);
                    _logger.LogInformation("Credit record created for Invoice {InvoiceId}", invoice.Id);
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                _logger.LogInformation("Order created: {InvoiceId} for Customer {CustomerId}", invoice.Id, customer.Id);

                try
                {
                    if (customer.User != null && !string.IsNullOrEmpty(customer.User.Email))
                    {
                        await _emailService.SendInvoiceEmailAsync(
                            customer.User.Email,
                            customer.Name,
                            invoice.Id,
                            invoice.TotalAmount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send invoice email, but order was created successfully.");
                }

                return await GetByIdAsync(invoice.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Transaction failed for creating order/invoice");
                throw;
            }
        }
        public async Task SendInvoiceEmailAsync(int invoiceId)
        {
            var invoice = await _salesRepo.Query()
                .Include(s => s.Customer)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(s => s.Id == invoiceId)
                ?? throw new KeyNotFoundException("Invoice not found.");

            var email = invoice.Customer?.User?.Email ?? invoice.Customer?.Email;
            if (string.IsNullOrEmpty(email))
                throw new InvalidOperationException("Customer does not have an email address.");

            await _emailService.SendInvoiceEmailAsync(
                email,
                invoice.Customer?.Name ?? "Customer",
                invoice.Id,
                invoice.TotalAmount);
        }

        public async Task<SalesInvoiceDto> CompleteOrderAsync(int invoiceId)
        {
            var invoice = await _salesRepo.Query()
                .Include(s => s.Customer)
                .Include(s => s.Staff)
                .Include(s => s.Items)
                    .ThenInclude(i => i.VehiclePart)
                .FirstOrDefaultAsync(s => s.Id == invoiceId)
                ?? throw new KeyNotFoundException("Sales invoice not found.");

            if (invoice.PaymentStatus == "PAID")
            {
                throw new InvalidOperationException("Order is already completed/paid.");
            }

            invoice.PaymentStatus = "PAID";
            await _salesRepo.UpdateAsync(invoice);

            return MapToDto(invoice);
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
            PaymentStatus = s.PaymentStatus,
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
