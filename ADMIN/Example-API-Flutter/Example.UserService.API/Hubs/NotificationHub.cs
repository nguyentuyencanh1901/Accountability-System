using Microsoft.AspNetCore.SignalR;

namespace Example.UserService.API.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.User(user).SendAsync("NotificationChannel", user, message);
        }
    }
}
