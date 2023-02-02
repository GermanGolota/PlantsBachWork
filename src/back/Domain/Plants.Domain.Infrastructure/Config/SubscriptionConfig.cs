namespace Plants.Domain.Infrastructure;

[ConfigSection(SectionName)]
internal class SubscriptionConfig
{
    const string SectionName = "Subscription";

    public long CommandProcessingTimeoutInSeconds { get; set; } = 60 * 5;
}