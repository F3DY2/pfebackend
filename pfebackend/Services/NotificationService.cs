using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using pfebackend.Data;
using pfebackend.DTOs;
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
        private readonly IUserService _userService;

        public NotificationService(IHubContext<NotificationHub> hubContext, AppDbContext context, IUserService userService)
        {
            _hubContext = hubContext;
            _context = context;
            _userService = userService;
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
                    return; // No notification needed
                }

                // Create and save notification
                var notification = new Notification
                {
                    UserId = userId,
                    Message = message,
                    Type = type,
                    CategoryNum = (int)category,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Prepare DTO for SignalR
                var notificationDto = new NotificationDto
                {
                    Id = notification.Id,
                    UserId = notification.UserId,
                    Message = notification.Message,
                    CreatedAt = notification.CreatedAt,
                    IsRead = notification.IsRead,
                    Type = notification.Type.ToString(),
                    CategoryNum = notification.CategoryNum
                };

                // Send via SignalR
                await _hubContext.Clients.Group(userId)
                    .SendAsync("ReceiveNotification", notificationDto);

                // Optional: Log the notification
            }
            catch (Exception ex)
            {
                throw; // Or handle differently based on your requirements
            }
        }
        public async Task<IEnumerable<Notification>> FetchNotifications(bool unreadOnly)
        {
            string userId = _userService.GetCurrentUserId();
            IOrderedQueryable<Notification>? query = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead) as IOrderedQueryable<Notification>;
            }

            return await query.ToListAsync();
        }
        public async Task<bool> MarkNotificationAsReadAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return false;
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
