using Microsoft.AspNetCore.SignalR;

namespace Plants.Domain.Presentation;

internal sealed class NotificationSender : INotificationSender
{
    private readonly IHubContext<NotificationHub> _context;

    public NotificationSender(IHubContext<NotificationHub> context)
    {
        _context = context;
    }

    public async Task SendNotificationAsync(string username, string notificationName, bool success, CancellationToken token)
    {
        await _context.Clients.User(username).SendAsync("CommandFinished", notificationName, success, token);
    }
     
}
