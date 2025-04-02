using pfebackend.Models;

namespace pfebackend.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public NotificationType Type { get; set; }
        public int CategoryNum { get; set; }

        public NotificationDto(Notification notification)
        {
            Id = notification.Id;
            UserId = notification.UserId;
            Message = notification.Message;
            CreatedAt = notification.CreatedAt;
            IsRead = notification.IsRead;
            Type = notification.Type; 
            CategoryNum = notification.CategoryNum;
        }

        public Notification ToEntity()
        {
            return new Notification
            {
                Id = this.Id,
                UserId = this.UserId,
                Message = this.Message,
                CreatedAt = this.CreatedAt,
                IsRead = this.IsRead,
                Type = this.Type,
                CategoryNum = this.CategoryNum
            };
        }
    }
}