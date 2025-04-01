using Microsoft.AspNetCore.SignalR;
using pfebackend.Hubs;
using pfebackend.Interfaces;
using pfebackend.Models;

namespace pfebackend.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendBudgetNotification(string userId, Category category, float totalExpenses, float limitValue, float alertValue)
        {
            try
            {
                if (totalExpenses > limitValue)
                {
                    await _hubContext.Clients.Group(userId)
                        .SendAsync("ReceiveNotification",
                            $"Budget exceeded for {category}! Limit: {limitValue}, Current: {totalExpenses}");
                }
                else if (totalExpenses > alertValue)
                {
                    await _hubContext.Clients.Group(userId)
                        .SendAsync("ReceiveNotification",
                            $"Approaching budget limit for {category}! Limit: {limitValue}, Current: {totalExpenses}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending notification: {ex.Message}");
            }
        }
    }
}
