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

        public async Task SendGeneralNotification(string userId, string message, NotificationType type)
        {
            await SendNotification(userId, message, type, null,null);
        }

        public async Task SendCategoryNotification(string userId, string message,
                                               NotificationType type, int categoryId,string? categoryName)
        {
            await SendNotification(userId, message, type, categoryId, categoryName);
        }

        private async Task SendNotification(string userId, string message,
                                            NotificationType type, int? categoryId, string? categoryName)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                CategoryId = (int)categoryId,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(userId)
                .SendAsync("ReceiveNotification", new NotificationDto(notification, categoryName));
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

        public async Task<bool> DeleteNotificationAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return false;
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}