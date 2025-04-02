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

        public async Task<IEnumerable<Notification>> GetUserNotifications(bool unreadOnly)
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
        public async Task<bool> MarkNotificationAsRead(int id)
        {
            Notification? notification = await _context.Notifications.FindAsync(id);
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
