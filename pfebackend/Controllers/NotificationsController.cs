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
        private readonly INotificationService _notificationService;

        public NotificationsController(AppDbContext context, IUserService userService, INotificationService notificationService)
        { 
            _notificationService = notificationService; 
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications(bool unreadOnly = false)
        {
            var notifications = await _notificationService.FetchNotifications(unreadOnly);
            return Ok(notifications);
        }



        [HttpPut("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var success = await _notificationService.MarkNotificationAsReadAsync(id);
            return success ? Ok() : NotFound();
        }
    }
}
