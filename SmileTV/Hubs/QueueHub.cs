using Microsoft.AspNetCore.SignalR;

namespace SmileTV.Hubs
{
    public class QueueHub : Hub
    {
        public async Task Send(string message)
        { 
            await Clients.All.SendAsync(message);
        }

        public override Task OnConnectedAsync()
        { 
            var s = Context.ConnectionId;

            return base.OnConnectedAsync();
        }
    }
}
