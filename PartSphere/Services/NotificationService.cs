using Microsoft.EntityFrameworkCore;
using PartSphere.DTOs;
using PartSphere.Models;
using PartSphere.Repositories;

namespace PartSphere.Services
{
    /// <summary>
    /// Manages system notifications (low stock, credit reminders, maintenance suggestions).
    /// </summary>
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetAllAsync();
        Task<IEnumerable<NotificationDto>> GetUnreadAsync();
        Task<IEnumerable<NotificationDto>> GetByUserIdAsync(int userId);
        Task CreateAsync(string message, NotificationType type, int? userId = null);
        Task MarkAsReadAsync(int id);
        Task MarkAllReadAsync();
        Task DeleteAsync(int id);
    }

    public class NotificationService : INotificationService
    {
        private readonly IRepository<Notification> _notificationRepo;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IRepository<Notification> notificationRepo,
            ILogger<NotificationService> logger)
        {
            _notificationRepo = notificationRepo;
            _logger = logger;
        }

        public async Task<IEnumerable<NotificationDto>> GetAllAsync()
        {
            var notifications = await _notificationRepo.Query()
                .OrderByDescending(n => n.CreatedAt)
                .Take(100)
                .ToListAsync();

            return notifications.Select(MapToDto);
        }

        public async Task<IEnumerable<NotificationDto>> GetUnreadAsync()
        {
            var notifications = await _notificationRepo.Query()
                .Where(n => !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return notifications.Select(MapToDto);
        }

        public async Task<IEnumerable<NotificationDto>> GetByUserIdAsync(int userId)
        {
            var notifications = await _notificationRepo.Query()
                .Where(n => n.UserId == userId || n.UserId == null)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();

            return notifications.Select(MapToDto);
        }

        public async Task CreateAsync(string message, NotificationType type, int? userId = null)
        {
            var notification = new Notification
            {
                Message = message,
                Type = type,
                UserId = userId
            };

            await _notificationRepo.AddAsync(notification);
            _logger.LogInformation("Notification created: [{Type}] {Message}", type, message);
        }

        public async Task MarkAsReadAsync(int id)
        {
            var notification = await _notificationRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Notification not found.");

            notification.IsRead = true;
            await _notificationRepo.UpdateAsync(notification);
        }

        public async Task MarkAllReadAsync()
        {
            var unread = await _notificationRepo.Query()
                .Where(n => !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
            {
                n.IsRead = true;
            }

            await _notificationRepo.UpdateAsync(unread.First()); // triggers SaveChanges
        }

        public async Task DeleteAsync(int id)
        {
            var notification = await _notificationRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Notification not found.");

            await _notificationRepo.DeleteAsync(notification);
        }

        private static NotificationDto MapToDto(Notification n) => new()
        {
            Id = n.Id,
            Message = n.Message,
            Type = n.Type.ToString(),
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        };
    }
}
