using Microsoft.AspNetCore.SignalR;
using pfebackend.Data;
using pfebackend.Hubs;
using pfebackend.Interfaces;
using pfebackend.Models;

namespace pfebackend.Services
{
    // Services/NotificationService.cs
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly AppDbContext _context;

        public NotificationService(IHubContext<NotificationHub> hubContext, AppDbContext context)
        {
            _hubContext = hubContext;
            _context = context;
        }

        public async Task SendBudgetNotification(string userId, Category category, float totalExpenses, float limitValue, float alertValue)
        {
            try
            {
                string message;
                NotificationType type;

                if (totalExpenses > limitValue)
                {
                    message = $"Budget exceeded for {category}! Limit: {limitValue}, Current: {totalExpenses}";
                    type = NotificationType.BudgetAlert;
                }
                else if (totalExpenses > alertValue)
                {
                    message = $"Approaching budget limit for {category}! Limit: {limitValue}, Current: {totalExpenses}";
                    type = NotificationType.BudgetAlert;
                }
                else
                {
                    return;
                }

                // Save to database
                var notification = new Notification
                {
                    UserId = userId,
                    Message = message,
                    Type = type
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Send via SignalR
                await _hubContext.Clients.Group(userId)
                    .SendAsync("ReceiveNotification", new
                    {
                        Id = notification.Id,
                        Message = notification.Message,
                        CreatedAt = notification.CreatedAt,
                        IsRead = notification.IsRead,
                        Type = notification.Type
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending notification: {ex.Message}");
            }
        }

        public async Task MarkAsRead(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
