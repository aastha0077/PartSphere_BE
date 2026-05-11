using Microsoft.EntityFrameworkCore;
using PartSphere.DTOs;
using PartSphere.Models;
using PartSphere.Repositories;

namespace PartSphere.Services
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentDto>> GetAllAsync();
        Task<IEnumerable<AppointmentDto>> GetByCustomerAsync(int customerId);
        Task<AppointmentDto> GetByIdAsync(int id);
        Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto);
        Task<AppointmentDto> UpdateStatusAsync(int id, string status);
    }

    public class AppointmentService : IAppointmentService
    {
        private readonly IRepository<Appointment> _appointmentRepo;
        private readonly IRepository<Customer> _customerRepo;

        public AppointmentService(
            IRepository<Appointment> appointmentRepo,
            IRepository<Customer> customerRepo)
        {
            _appointmentRepo = appointmentRepo;
            _customerRepo = customerRepo;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAllAsync()
        {
            var appointments = await _appointmentRepo.Query()
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            return appointments.Select(MapToDto);
        }

        public async Task<IEnumerable<AppointmentDto>> GetByCustomerAsync(int customerId)
        {
            var appointments = await _appointmentRepo.Query()
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .Where(a => a.CustomerId == customerId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            return appointments.Select(MapToDto);
        }

        public async Task<AppointmentDto> GetByIdAsync(int id)
        {
            var appointment = await _appointmentRepo.Query()
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .FirstOrDefaultAsync(a => a.Id == id)
                ?? throw new KeyNotFoundException("Appointment not found.");

            return MapToDto(appointment);
        }

        public async Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto)
        {
            if (!await _customerRepo.ExistsAsync(dto.CustomerId))
                throw new KeyNotFoundException("Customer not found.");

            var appointment = new Appointment
            {
                CustomerId = dto.CustomerId,
                VehicleId = dto.VehicleId,
                ServiceType = dto.ServiceType,
                Date = dto.Date,
                Description = dto.Description,
                Status = AppointmentStatus.Pending
            };

            await _appointmentRepo.AddAsync(appointment);

            return await GetByIdAsync(appointment.Id);
        }

        public async Task<AppointmentDto> UpdateStatusAsync(int id, string status)
        {
            var appointment = await _appointmentRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Appointment not found.");

            if (Enum.TryParse<AppointmentStatus>(status, out var parsed))
            {
                appointment.Status = parsed;
                await _appointmentRepo.UpdateAsync(appointment);
            }
            else
            {
                throw new ArgumentException("Invalid status.");
            }

            return await GetByIdAsync(appointment.Id);
        }

        private static AppointmentDto MapToDto(Appointment a) => new()
        {
            Id = a.Id,
            CustomerId = a.CustomerId,
            CustomerName = a.Customer?.Name ?? "",
            VehicleId = a.VehicleId,
            VehicleInfo = a.Vehicle != null ? $"{a.Vehicle.Brand} {a.Vehicle.Model} ({a.Vehicle.VehicleNumber})" : "No Vehicle",
            ServiceType = a.ServiceType,
            Date = a.Date,
            Status = a.Status.ToString(),
            Description = a.Description,
            CreatedAt = a.CreatedAt
        };
    }
}
