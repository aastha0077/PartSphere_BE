using Microsoft.EntityFrameworkCore;
using PartSphere.DTOs;
using PartSphere.Models;
using PartSphere.Repositories;

namespace PartSphere.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleDto>> GetAllAsync();
        Task<IEnumerable<VehicleDto>> GetByCustomerIdAsync(int customerId);
        Task<VehicleDto> GetByIdAsync(int id);
        Task<VehicleDto> CreateAsync(CreateVehicleDto dto);
        Task<VehicleDto> UpdateAsync(int id, UpdateVehicleDto dto);
        Task DeleteAsync(int id);
        Task<IEnumerable<VehicleDto>> SearchAsync(string query);
    }

    public class VehicleService : IVehicleService
    {
        private readonly IRepository<Vehicle> _vehicleRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly ILogger<VehicleService> _logger;

        public VehicleService(
            IRepository<Vehicle> vehicleRepo,
            IRepository<Customer> customerRepo,
            ILogger<VehicleService> logger)
        {
            _vehicleRepo = vehicleRepo;
            _customerRepo = customerRepo;
            _logger = logger;
        }

        public async Task<IEnumerable<VehicleDto>> GetAllAsync()
        {
            var vehicles = await _vehicleRepo.Query()
                .Include(v => v.Customer)
                .OrderBy(v => v.Brand)
                .ToListAsync();

            return vehicles.Select(MapToDto);
        }

        public async Task<IEnumerable<VehicleDto>> GetByCustomerIdAsync(int customerId)
        {
            var vehicles = await _vehicleRepo.Query()
                .Include(v => v.Customer)
                .Where(v => v.CustomerId == customerId)
                .ToListAsync();

            return vehicles.Select(MapToDto);
        }

        public async Task<VehicleDto> GetByIdAsync(int id)
        {
            var vehicle = await _vehicleRepo.Query()
                .Include(v => v.Customer)
                .FirstOrDefaultAsync(v => v.Id == id)
                ?? throw new KeyNotFoundException("Vehicle not found.");

            return MapToDto(vehicle);
        }

        public async Task<VehicleDto> CreateAsync(CreateVehicleDto dto)
        {
            if (!await _customerRepo.ExistsAsync(dto.CustomerId))
                throw new KeyNotFoundException("Customer not found.");

            var existing = await _vehicleRepo.Query()
                .FirstOrDefaultAsync(v => v.VehicleNumber == dto.VehicleNumber);
            if (existing != null)
                throw new ArgumentException("Vehicle number already registered.");

            var vehicle = new Vehicle
            {
                Brand = dto.Brand,
                Model = dto.Model,
                VehicleNumber = dto.VehicleNumber,
                Mileage = dto.Mileage,
                CustomerId = dto.CustomerId
            };

            await _vehicleRepo.AddAsync(vehicle);
            _logger.LogInformation("Vehicle created: {VehicleNumber}", dto.VehicleNumber);

            return await GetByIdAsync(vehicle.Id);
        }

        public async Task<VehicleDto> UpdateAsync(int id, UpdateVehicleDto dto)
        {
            var vehicle = await _vehicleRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Vehicle not found.");

            if (dto.Brand != null) vehicle.Brand = dto.Brand;
            if (dto.Model != null) vehicle.Model = dto.Model;
            if (dto.Mileage.HasValue) vehicle.Mileage = dto.Mileage.Value;
            if (dto.LastServiceDate.HasValue) vehicle.LastServiceDate = dto.LastServiceDate.Value;

            await _vehicleRepo.UpdateAsync(vehicle);

            return await GetByIdAsync(vehicle.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var vehicle = await _vehicleRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Vehicle not found.");

            await _vehicleRepo.DeleteAsync(vehicle);
        }

        public async Task<IEnumerable<VehicleDto>> SearchAsync(string query)
        {
            var q = query.ToLower().Trim();

            var vehicles = await _vehicleRepo.Query()
                .Include(v => v.Customer)
                .Where(v =>
                    v.VehicleNumber.ToLower().Contains(q) ||
                    v.Model.ToLower().Contains(q) ||
                    v.Brand.ToLower().Contains(q))
                .OrderBy(v => v.Brand)
                .ToListAsync();

            return vehicles.Select(MapToDto);
        }

        private static VehicleDto MapToDto(Vehicle v) => new()
        {
            Id = v.Id,
            Brand = v.Brand,
            Model = v.Model,
            VehicleNumber = v.VehicleNumber,
            Mileage = v.Mileage,
            LastServiceDate = v.LastServiceDate,
            CustomerId = v.CustomerId,
            CustomerName = v.Customer?.Name ?? ""
        };
    }
}
