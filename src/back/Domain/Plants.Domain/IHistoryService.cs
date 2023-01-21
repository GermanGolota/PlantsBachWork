namespace Plants.Domain;

public interface IHistoryService
{
    Task<HistoryModel> GetAsync(AggregateDescription aggregate, CancellationToken token);
}

public record HistoryModel(List<OneOf<Event, Command>> Events, List<AggregateDescription> Related);
