using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace pfebackend.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public async Task JoinGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
    }
}