using System.ComponentModel.DataAnnotations;

namespace PartSphere.Models
{
    public enum NotificationType
    {
        LowStock,
        CreditReminder,
        MaintenanceSuggestion,
        General
    }

    public class Notification
    {
        public int Id { get; set; }

        [Required, MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }

        public bool IsRead { get; set; } = false;

        // Optional: target user for the notification
        public int? UserId { get; set; }
        public User? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
