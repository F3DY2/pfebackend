namespace pfebackend.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string Type { get; set; }  // String pour plus de flexibilité côté client
        public int CategoryNum { get; set; }
    }
    }
