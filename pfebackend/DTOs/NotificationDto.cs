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
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }


        public NotificationDto(Notification notification, string? categoryName = null)
        {
            Id = notification.Id;
            UserId = notification.UserId;
            Message = notification.Message;
            CreatedAt = notification.CreatedAt;
            IsRead = notification.IsRead;
            Type = notification.Type; 
            CategoryId = notification.CategoryId;
            CategoryName = categoryName;
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
                CategoryId = this.CategoryId            
            };
        }
    }
}