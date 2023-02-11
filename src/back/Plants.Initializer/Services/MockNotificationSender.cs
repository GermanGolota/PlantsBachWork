namespace Plants.Initializer;

internal sealed class MockNotificationSender : INotificationSender
{
    public Task SendNotificationAsync(string username, NotificationMessage message, CancellationToken token) =>
        Task.CompletedTask;
}
