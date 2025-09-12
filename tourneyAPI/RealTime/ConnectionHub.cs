using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class ConnectionHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "GameServiceHub");
        await Clients.Caller.SendAsync("Player Joined"); 
    }
}