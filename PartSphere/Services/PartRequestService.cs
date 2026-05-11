using Microsoft.EntityFrameworkCore;
using PartSphere.Data;
using PartSphere.DTOs;
using PartSphere.Models;
using PartSphere.Repositories;

namespace PartSphere.Services
{
    public interface IPartRequestService
    {
        Task<PartRequestDto> GetByIdAsync(int id);
        Task<IEnumerable<PartRequestDto>> GetAllAsync();
        Task<IEnumerable<PartRequestDto>> GetByCustomerAsync(int customerId);
        Task<PartRequestDto> CreateAsync(CreatePartRequestDto dto);
        Task<PartRequestDto> UpdateStatusAsync(int id, string status, string? notes);
    }

    public class PartRequestService : IPartRequestService
    {
        private readonly IRepository<PartRequest> _requestRepo;
        private readonly AppDbContext _context;

        public PartRequestService(IRepository<PartRequest> requestRepo, AppDbContext context)
        {
            _requestRepo = requestRepo;
            _context = context;
        }

        public async Task<PartRequestDto> GetByIdAsync(int id)
        {
            var request = await _requestRepo.Query()
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new KeyNotFoundException("Part request not found.");
            
            return MapToDto(request);
        }

        public async Task<IEnumerable<PartRequestDto>> GetAllAsync()
        {
            var requests = await _requestRepo.Query()
                .Include(r => r.Customer)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return requests.Select(MapToDto);
        }

        public async Task<IEnumerable<PartRequestDto>> GetByCustomerAsync(int customerId)
        {
            var requests = await _requestRepo.Query()
                .Include(r => r.Customer)
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return requests.Select(MapToDto);
        }

        public async Task<PartRequestDto> CreateAsync(CreatePartRequestDto dto)
        {
            var request = new PartRequest
            {
                CustomerId = dto.CustomerId,
                PartName = dto.PartName,
                Brand = dto.Brand,
                Description = dto.Description,
                Status = "Pending"
            };

            await _requestRepo.AddAsync(request);
            return await GetByIdAsync(request.Id);
        }

        public async Task<PartRequestDto> UpdateStatusAsync(int id, string status, string? notes)
        {
            var request = await _requestRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Part request not found.");

            request.Status = status;
            if (notes != null) request.StaffNotes = notes;

            await _requestRepo.UpdateAsync(request);
            return await GetByIdAsync(request.Id);
        }

        private static PartRequestDto MapToDto(PartRequest r) => new()
        {
            Id = r.Id,
            CustomerId = r.CustomerId,
            CustomerName = r.Customer?.Name ?? "Unknown",
            PartName = r.PartName,
            Brand = r.Brand,
            Description = r.Description,
            CreatedAt = r.CreatedAt,
            Status = r.Status,
            StaffNotes = r.StaffNotes
        };
    }
}
