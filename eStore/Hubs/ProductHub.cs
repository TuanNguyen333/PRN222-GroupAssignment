using Microsoft.AspNetCore.SignalR;

namespace eStore.Hubs
{
    public class ProductHub : Hub
    {
        public async Task NotifyProductChange(string action, int productId)
        {
            await Clients.All.SendAsync("ReceiveProductUpdate", action, productId);
        }
    }
} 