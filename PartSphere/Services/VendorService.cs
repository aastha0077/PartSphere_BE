using Microsoft.EntityFrameworkCore;
using PartSphere.DTOs;
using PartSphere.Models;
using PartSphere.Repositories;

namespace PartSphere.Services
{
    public interface IVendorService
    {
        Task<IEnumerable<VendorDto>> GetAllAsync();
        Task<VendorDto> GetByIdAsync(int id);
        Task<VendorDto> CreateAsync(CreateVendorDto dto);
        Task<VendorDto> UpdateAsync(int id, UpdateVendorDto dto);
        Task DeleteAsync(int id);
    }

    public class VendorService : IVendorService
    {
        private readonly IRepository<Vendor> _vendorRepo;
        private readonly ILogger<VendorService> _logger;

        public VendorService(IRepository<Vendor> vendorRepo, ILogger<VendorService> logger)
        {
            _vendorRepo = vendorRepo;
            _logger = logger;
        }

        public async Task<IEnumerable<VendorDto>> GetAllAsync()
        {
            var vendors = await _vendorRepo.Query()
                .Include(v => v.Parts)
                .OrderBy(v => v.Name)
                .ToListAsync();

            return vendors.Select(MapToDto);
        }

        public async Task<VendorDto> GetByIdAsync(int id)
        {
            var vendor = await _vendorRepo.Query()
                .Include(v => v.Parts)
                .FirstOrDefaultAsync(v => v.Id == id)
                ?? throw new KeyNotFoundException("Vendor not found.");

            return MapToDto(vendor);
        }

        public async Task<VendorDto> CreateAsync(CreateVendorDto dto)
        {
            var vendor = new Vendor
            {
                Name = dto.Name,
                ContactPerson = dto.ContactPerson,
                Contact = dto.Contact,
                Phone = dto.Phone,
                Address = dto.Address,
                Email = dto.Email,
                Category = dto.Category
            };

            await _vendorRepo.AddAsync(vendor);
            _logger.LogInformation("Vendor created: {Name}", dto.Name);

            return MapToDto(vendor);
        }

        public async Task<VendorDto> UpdateAsync(int id, UpdateVendorDto dto)
        {
            var vendor = await _vendorRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Vendor not found.");

            if (dto.Name != null) vendor.Name = dto.Name;
            if (dto.ContactPerson != null) vendor.ContactPerson = dto.ContactPerson;
            if (dto.Contact != null) vendor.Contact = dto.Contact;
            if (dto.Phone != null) vendor.Phone = dto.Phone;
            if (dto.Address != null) vendor.Address = dto.Address;
            if (dto.Email != null) vendor.Email = dto.Email;
            if (dto.Category != null) vendor.Category = dto.Category;

            await _vendorRepo.UpdateAsync(vendor);

            return await GetByIdAsync(vendor.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var vendor = await _vendorRepo.Query()
                .Include(v => v.Parts)
                .Include(v => v.PurchaseInvoices)
                .FirstOrDefaultAsync(v => v.Id == id)
                ?? throw new KeyNotFoundException("Vendor not found.");

            if (vendor.Parts.Any() || vendor.PurchaseInvoices.Any())
                throw new InvalidOperationException("Cannot delete vendor with existing parts or purchase history.");

            await _vendorRepo.DeleteAsync(vendor);
            _logger.LogInformation("Vendor deleted: {Id}", id);
        }

        private static VendorDto MapToDto(Vendor v) => new()
        {
            Id = v.Id,
            Name = v.Name,
            ContactPerson = v.ContactPerson,
            Contact = v.Contact,
            Phone = v.Phone,
            Address = v.Address,
            Email = v.Email,
            Category = v.Category,
            PartsCount = v.Parts?.Count ?? 0
        };
    }
}
