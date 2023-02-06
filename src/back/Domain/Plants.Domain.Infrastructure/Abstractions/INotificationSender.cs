namespace Plants.Domain.Infrastructure;

public interface INotificationSender
{
    Task SendNotificationAsync(string username, string notificationName, bool success, CancellationToken token);
}
