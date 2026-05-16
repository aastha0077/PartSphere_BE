using Microsoft.EntityFrameworkCore;
using PartSphere.Data;
using PartSphere.DTOs;
using PartSphere.Helpers;
using PartSphere.Models;
using PartSphere.Repositories;

namespace PartSphere.Services
{
    /// <summary>
    /// Handles user registration, login, and JWT token generation.
    /// </summary>
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<UserResponseDto> CreateStaffAsync(CreateStaffDto dto);
        Task<IEnumerable<UserResponseDto>> GetStaffAsync();
        Task<UserResponseDto> UpdateStaffAsync(int id, CreateStaffDto dto);
        Task DeleteStaffAsync(int id);
        Task ToggleStaffStatusAsync(int id);
        Task<AuthResponseDto> GetMeAsync(int userId);
    }

    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly AppDbContext _context;
        private readonly JwtHelper _jwtHelper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IRepository<User> userRepo,
            IRepository<Customer> customerRepo,
            AppDbContext context,
            JwtHelper jwtHelper,
            ILogger<AuthService> logger)
        {
            _userRepo = userRepo;
            _customerRepo = customerRepo;
            _context = context;
            _jwtHelper = jwtHelper;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name is required.");
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("Email is required.");
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                throw new ArgumentException("Password must be at least 6 characters.");

            // Check if email already exists
            var existing = await _userRepo.Query()
                .FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existing != null)
                throw new ArgumentException("Email is already registered.");

            // SECURITY: Public registration ALWAYS creates Customer role only.
            // Admin and Staff accounts can only be created by an Admin.
            var user = new User
            {
                Name = dto.Name.Trim(),
                Email = dto.Email.Trim().ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = UserRole.Customer
            };

            await _userRepo.AddAsync(user);

            // Create linked customer profile
            var customer = new Customer
            {
                Name = dto.Name.Trim(),
                Email = user.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                UserId = user.Id
            };

            await _customerRepo.AddAsync(customer);

            // Reload user with customer
            user.Customer = customer;

            var token = _jwtHelper.GenerateToken(user);

            _logger.LogInformation("New customer registered: {Email}", dto.Email);

            return new AuthResponseDto
            {
                Token = token,
                Role = user.Role.ToString(),
                Name = user.Name,
                Email = user.Email,
                UserId = user.Id,
                CustomerId = customer.Id
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepo.Query()
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is deactivated. Contact administrator.");

            var token = _jwtHelper.GenerateToken(user);

            _logger.LogInformation("User logged in: {Email}", dto.Email);

            return new AuthResponseDto
            {
                Token = token,
                Role = user.Role.ToString(),
                Name = user.Name,
                Email = user.Email,
                UserId = user.Id,
                CustomerId = user.Customer?.Id
            };
        }

        public async Task<UserResponseDto> CreateStaffAsync(CreateStaffDto dto)
        {
            var existing = await _userRepo.Query()
                .FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existing != null)
                throw new ArgumentException("Email is already in use.");

            if (string.IsNullOrEmpty(dto.Password))
                throw new ArgumentException("Password is required for registration.");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = Enum.TryParse<UserRole>(dto.Role, true, out var role) ? role : UserRole.Staff
            };

            await _userRepo.AddAsync(user);

            _logger.LogInformation("Staff created: {Email} as {Role}", dto.Email, user.Role);

            return MapToUserDto(user);
        }

        public async Task<IEnumerable<UserResponseDto>> GetStaffAsync()
        {
            var staff = await _userRepo.Query()
                .Where(u => u.Role == UserRole.Staff || u.Role == UserRole.Admin)
                .OrderBy(u => u.Name)
                .ToListAsync();

            return staff.Select(MapToUserDto);
        }

        public async Task<UserResponseDto> UpdateStaffAsync(int id, CreateStaffDto dto)
        {
            var user = await _userRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Staff member not found.");

            if (user.Role != UserRole.Staff && user.Role != UserRole.Admin)
                throw new ArgumentException("User is not a staff member or administrator.");

            user.Name = dto.Name;
            user.Email = dto.Email;
            
            if (Enum.TryParse<UserRole>(dto.Role, true, out var newRole))
                user.Role = newRole;

            if (!string.IsNullOrEmpty(dto.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _userRepo.UpdateAsync(user);

            return MapToUserDto(user);
        }

        public async Task DeleteStaffAsync(int id)
        {
            var user = await _userRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Staff member not found.");
            await _userRepo.DeleteAsync(user);
        }

        public async Task ToggleStaffStatusAsync(int id)
        {
            var user = await _userRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Staff member not found.");

            if (user.Role != UserRole.Staff && user.Role != UserRole.Admin)
                throw new ArgumentException("User is not a staff member or administrator.");

            user.IsActive = !user.IsActive;
            await _userRepo.UpdateAsync(user);
        }

        public async Task<AuthResponseDto> GetMeAsync(int userId)
        {
            var user = await _userRepo.Query()
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new KeyNotFoundException("User not found.");

            return new AuthResponseDto
            {
                Token = "", // Token not needed for GetMe
                Role = user.Role.ToString(),
                Name = user.Name,
                Email = user.Email,
                UserId = user.Id,
                CustomerId = user.Customer?.Id
            };
        }

        private static UserResponseDto MapToUserDto(User user) => new()
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
