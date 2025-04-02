using pfebackend.Models;

namespace pfebackend.Interfaces
{
    public interface INotificationService
    {
        Task SendBudgetNotification(string userId, Category category, float totalExpenses, float limitValue, float alertValue);
        Task<IEnumerable<Notification>> FetchNotifications(bool unreadOnly);
        Task<bool> MarkNotificationAsReadAsync(int id);
    }
}
