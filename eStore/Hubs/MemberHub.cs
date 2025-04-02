using Microsoft.AspNetCore.SignalR;

namespace eStore.Hubs
{
    public class MemberHub : Hub
    {
        public async Task NotifyMemberChange(string action, int memberId)
        {
            await Clients.All.SendAsync("ReceiveMemberUpdate", action, memberId);
        }
    }
}
