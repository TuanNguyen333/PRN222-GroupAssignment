using Microsoft.AspNetCore.SignalR;

namespace eStore.Hubs
{
    public class OrderHub : Hub
    {
        public async Task NotifyOrderChange(string action, int orderId)
        {
            await Clients.All.SendAsync("ReceiveOrderUpdate", action, orderId);
        }
    }
} 