namespace Plants.Domain.Infrastructure;

public interface INotificationSender
{
    Task SendNotificationAsync(string username, NotificationMessage message, CancellationToken token);
}

public sealed record NotificationMessage(CommandDescription Command, bool Success);