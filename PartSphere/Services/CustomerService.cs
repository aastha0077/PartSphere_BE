using Microsoft.EntityFrameworkCore;
using PartSphere.DTOs;
using PartSphere.Models;
using PartSphere.Repositories;

namespace PartSphere.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllAsync();
        Task<CustomerDto> GetByIdAsync(int id);
        Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
        Task<CustomerDto> UpdateAsync(int id, UpdateCustomerDto dto);
        Task DeleteAsync(int id);
        Task<IEnumerable<CustomerSearchResultDto>> SearchAsync(string query);
        Task<CustomerHistoryDto> GetHistoryAsync(int customerId);
        Task<CustomerDto?> GetByUserIdAsync(int userId);
    }

    public class CustomerService : ICustomerService
    {
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<Vehicle> _vehicleRepo;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(
            IRepository<Customer> customerRepo,
            IRepository<Vehicle> vehicleRepo,
            ILogger<CustomerService> logger)
        {
            _customerRepo = customerRepo;
            _vehicleRepo = vehicleRepo;
            _logger = logger;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            var customers = await _customerRepo.Query()
                .Include(c => c.Vehicles)
                .Include(c => c.SalesInvoices)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return customers.Select(MapToDto);
        }

        public async Task<CustomerDto> GetByIdAsync(int id)
        {
            var customer = await _customerRepo.Query()
                .Include(c => c.Vehicles)
                .Include(c => c.SalesInvoices)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new KeyNotFoundException("Customer not found.");

            return MapToDto(customer);
        }

        public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
        {
            var customer = new Customer
            {
                Name = dto.Name,
                Phone = dto.Phone,
                Address = dto.Address,
                Email = dto.Email
            };

            await _customerRepo.AddAsync(customer);

            if (dto.Vehicle != null &&
                !string.IsNullOrWhiteSpace(dto.Vehicle.VehicleNumber))
            {
                var vehicle = new Vehicle
                {
                    CustomerId = customer.Id,
                    VehicleNumber = dto.Vehicle.VehicleNumber.Trim(),
                    Brand = dto.Vehicle.Brand.Trim(),
                    Model = dto.Vehicle.Model.Trim(),
                    Mileage = dto.Vehicle.Mileage
                };
                await _vehicleRepo.AddAsync(vehicle);
                _logger.LogInformation("Vehicle {Plate} registered for customer {CustomerId}",
                    vehicle.VehicleNumber, customer.Id);
            }

            _logger.LogInformation("Customer created: {Name}", dto.Name);

            return await GetByIdAsync(customer.Id);
        }

        public async Task<CustomerDto> UpdateAsync(int id, UpdateCustomerDto dto)
        {
            var customer = await _customerRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Customer not found.");

            if (dto.Name != null) customer.Name = dto.Name;
            if (dto.Phone != null) customer.Phone = dto.Phone;
            if (dto.Address != null) customer.Address = dto.Address;
            if (dto.Email != null) customer.Email = dto.Email;

            await _customerRepo.UpdateAsync(customer);

            return await GetByIdAsync(customer.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _customerRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Customer not found.");

            await _customerRepo.DeleteAsync(customer);
        }

        public async Task<IEnumerable<CustomerSearchResultDto>> SearchAsync(string query)
        {
            var q = query.ToLower().Trim();
            int.TryParse(q, out int idQuery);

            var customers = await _customerRepo.Query()
                .Include(c => c.Vehicles)
                .Where(c =>
                    c.Id == idQuery ||
                    c.Name.ToLower().Contains(q) ||
                    c.Phone.ToLower().Contains(q) ||
                    c.Email.ToLower().Contains(q) ||
                    c.Vehicles.Any(v => v.VehicleNumber.ToLower().Contains(q)))
                .ToListAsync();

            return customers.Select(c => new CustomerSearchResultDto
            {
                Id = c.Id,
                Name = c.Name,
                Phone = c.Phone,
                Address = c.Address,
                Email = c.Email,
                VehicleNumbers = c.Vehicles.Select(v => v.VehicleNumber).ToList()
            });
        }

        public async Task<CustomerHistoryDto> GetHistoryAsync(int customerId)
        {
            var customer = await _customerRepo.Query()
                .Include(c => c.Vehicles)
                .Include(c => c.SalesInvoices)
                    .ThenInclude(s => s.Items)
                        .ThenInclude(i => i.VehiclePart)
                .Include(c => c.SalesInvoices)
                    .ThenInclude(s => s.Staff)
                .Include(c => c.Appointments)
                .FirstOrDefaultAsync(c => c.Id == customerId)
                ?? throw new KeyNotFoundException("Customer not found.");

            var totalSpent = customer.SalesInvoices.Sum(s => s.TotalAmount);

            return new CustomerHistoryDto
            {
                Customer = MapToDto(customer),
                Vehicles = customer.Vehicles.Select(v => new VehicleDto
                {
                    Id = v.Id,
                    Brand = v.Brand,
                    Model = v.Model,
                    VehicleNumber = v.VehicleNumber,
                    Mileage = v.Mileage,
                    LastServiceDate = v.LastServiceDate,
                    CustomerId = v.CustomerId
                }).ToList(),
                Purchases = customer.SalesInvoices
                    .OrderByDescending(s => s.Date)
                    .ThenByDescending(s => s.Id)
                    .Select(s => new SalesInvoiceDto
                    {
                        Id = s.Id,
                        CustomerId = s.CustomerId,
                        CustomerName = customer.Name,
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
                    }).ToList(),
                Appointments = customer.Appointments.Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    CustomerId = a.CustomerId,
                    CustomerName = customer.Name,
                    Date = a.Date,
                    Status = a.Status.ToString(),
                    Description = a.Description,
                    CreatedAt = a.CreatedAt
                }).ToList(),
                TotalSpent = totalSpent,
                IsLoyalCustomer = totalSpent > 5000
            };
        }

        public async Task<CustomerDto?> GetByUserIdAsync(int userId)
        {
            var customer = await _customerRepo.Query()
                .Include(c => c.Vehicles)
                .Include(c => c.SalesInvoices)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return customer != null ? MapToDto(customer) : null;
        }

        private static CustomerDto MapToDto(Customer c) => new()
        {
            Id = c.Id,
            Name = c.Name,
            Phone = c.Phone,
            Address = c.Address,
            Email = c.Email,
            UserId = c.UserId,
            VehicleCount = c.Vehicles?.Count ?? 0,
            TotalSpent = c.SalesInvoices?.Sum(s => s.TotalAmount) ?? 0,
            LoyaltyPoints = c.LoyaltyPoints
        };
    }
}
