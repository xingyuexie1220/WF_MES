using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WF.MES.Api.Hubs;

[Authorize]
public class NoticeHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var factoryCode = Context.User?.FindFirst("wf:factory_code")?.Value;
        if (!string.IsNullOrWhiteSpace(factoryCode))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, factoryCode);
        }

        await base.OnConnectedAsync();
    }
}
