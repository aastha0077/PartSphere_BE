using Microsoft.EntityFrameworkCore;
using PartSphere.Data;
using PartSphere.Models;

namespace PartSphere.Services
{
    public interface IAdminLowStockNotifier
    {
        Task NotifyIfCrossedLowThresholdAsync(string partName, string brand, int newStockQuantity, int? previousStockQuantity);
    }

    public class AdminLowStockNotifier : IAdminLowStockNotifier
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminLowStockNotifier> _logger;

        public AdminLowStockNotifier(
            AppDbContext context,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<AdminLowStockNotifier> logger)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task NotifyIfCrossedLowThresholdAsync(string partName, string brand, int newStockQuantity, int? previousStockQuantity)
        {
            if (newStockQuantity >= 10)
                return;

            if (previousStockQuantity.HasValue && previousStockQuantity.Value < 10)
                return;

            if (!IsEnabled())
            {
                _logger.LogDebug("Admin low-stock emails disabled via configuration.");
                return;
            }

            var adminEmails = await _context.Users
                .AsNoTracking()
                .Where(u => u.Role == UserRole.Admin && u.IsActive && u.Email != "")
                .Select(u => u.Email)
                .Distinct()
                .ToListAsync();

            var fallback = _configuration["Alerts:AdminFallbackEmails"];
            if (!string.IsNullOrWhiteSpace(fallback))
            {
                foreach (var raw in fallback.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (!adminEmails.Contains(raw, StringComparer.OrdinalIgnoreCase))
                        adminEmails.Add(raw);
                }
            }

            if (adminEmails.Count == 0)
            {
                _logger.LogWarning("Low stock for {Part} but no admin email recipients configured.", partName);
                return;
            }

            foreach (var to in adminEmails)
            {
                await _emailService.SendLowStockAlertAsync(to, partName, brand, newStockQuantity);
            }
        }

        private bool IsEnabled()
        {
            var v = _configuration["Alerts:LowStockEmailsEnabled"];
            if (string.IsNullOrWhiteSpace(v))
                return true;
            return bool.TryParse(v, out var b) && b;
        }
    }
}
