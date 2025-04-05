using pfebackend.Models;

public interface INotificationService
{
    Task SendGeneralNotification(string userId, string message, NotificationType type);
    Task SendCategoryNotification(string userId, string message, NotificationType type, string categoryName);
    Task<IEnumerable<Notification>> FetchNotifications(bool unreadOnly);
    Task<bool> MarkNotificationAsReadAsync(int id);
    Task<bool> DeleteNotificationAsync(int id);
}