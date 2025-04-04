using Microsoft.AspNetCore.SignalR;
using eStore.Models;

namespace eStore.Hubs;

public class CategoryHub : Hub
{
    public async Task NotifyCategoryChange(string action, int categoryId)
    {
        await Clients.All.SendAsync("ReceiveCategoryUpdate", action, categoryId);
    }
}


