using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pfebackend.Data;
using pfebackend.Interfaces;
using pfebackend.Models;

namespace pfebackend.Controllers
{
    // Controllers/NotificationsController.cs
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserService _userService;

        public NotificationsController(AppDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications(bool unreadOnly = false)
        {
            var userId = _userService.GetCurrentUserId();
            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead) as IOrderedQueryable<Notification>;
            }

            return await query.ToListAsync();
        }

        [HttpPut("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
