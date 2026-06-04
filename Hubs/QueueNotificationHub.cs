using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace BE.Hubs;

public class QueueNotificationHub : Hub
{
    public async Task JoinUser(string userId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            GetUserGroupName(userId)
        );
    }

    public async Task LeaveUser(string userId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            GetUserGroupName(userId)
        );
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value;

        Console.WriteLine($"[Hub] Connection {Context.ConnectionId} connected. Raw UserId: {userId}");

        if (int.TryParse(userId, out var parsedUserId))
        {
            var groupName = GetUserGroupName(parsedUserId);
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                groupName
            );
            Console.WriteLine($"[Hub] Connection {Context.ConnectionId} joined group: {groupName}");
        }
        else {
            Console.WriteLine($"[Hub] Warning: Connection {Context.ConnectionId} could not be mapped to a user group.");
        }

        await base.OnConnectedAsync();
    }

    // Thêm hàm này để test xem Hub có thể gửi tin trực tiếp không
    public async Task SendTestNotification(string userId)
    {
        await Clients.Group($"user-{userId}").SendAsync("QueueAssigned", "Test từ Hub");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    private static string GetUserGroupName(int userId)
    {
        return $"user-{userId}";
    }

    private static string GetUserGroupName(string userId)
    {
        return $"user-{userId}";
    }
}