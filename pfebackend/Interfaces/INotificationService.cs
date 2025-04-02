using pfebackend.Models;

namespace pfebackend.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetUserNotifications(bool unreadOnly);
        Task<bool> MarkNotificationAsRead(int id);
    }
}
