using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pfebackend.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public NotificationType Type { get; set; }
        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
    }

    public enum NotificationType
    {
        BudgetAlert,
        BudgetWarning,
        System,
        Other
    }
}
