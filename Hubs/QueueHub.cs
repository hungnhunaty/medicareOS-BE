using Microsoft.AspNetCore.SignalR;

namespace BE.Hubs;

public class QueueHub : Hub
{
    // A (doctor / receptionist / screen) join session room
    public async Task JoinSession(string sessionId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            GetGroupName(sessionId)
        );
    }

    public async Task LeaveSession(string sessionId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            GetGroupName(sessionId)
        );
    }

    // optional: heartbeat debug
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    private string GetGroupName(string sessionId)
    {
        return $"session-{sessionId}";
    }
}