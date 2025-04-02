namespace pfebackend.Models
{
    // Models/Notification.cs
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public NotificationType Type { get; set; }
        public int CategoryNum { get; set; } // Nouvelle propriété
    }

    public enum NotificationType
    {
        BudgetAlert,
        BudgetWarning,
        System,
        Other
    }
}
