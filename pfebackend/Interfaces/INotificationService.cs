using pfebackend.Models;

namespace pfebackend.Interfaces
{
    public interface INotificationService
    {
        Task SendGeneralNotification(string userId, string message, NotificationType type);
        Task SendCategoryNotification(string userId, string message,
                                   NotificationType type, Category category);
        Task<IEnumerable<Notification>> FetchNotifications(bool unreadOnly);
        Task<bool> MarkNotificationAsReadAsync(int id);
        Task<bool> DeleteNotificationAsync(int id);
    }
}
