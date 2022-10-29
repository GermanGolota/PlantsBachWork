namespace Plants.Domain;

/// <summary>
/// This is supposed to be applied to the aggregate
/// </summary>
public interface IEventHandler<T> where T : Event
{
    void Handle(T @event);
}