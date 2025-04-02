using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pfebackend.Data;
using pfebackend.Interfaces;
using pfebackend.Models;
using pfebackend.Services;

namespace pfebackend.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notificationService;


        public NotificationsController(AppDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications(bool unreadOnly = false)
        {
            var notifications = await _notificationService.GetUserNotifications(unreadOnly);
            return Ok(notifications);
        }

        [HttpPut("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            bool isRead = await _notificationService.MarkNotificationAsRead(id);
            if (isRead)
                return Ok();
            return NotFound();
        }
    }
}
