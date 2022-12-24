namespace Plants.Domain.Persistence;

[Serializable]
public class EventStoreException : Exception
{
    public EventStoreException() { }
    public EventStoreException(string message) : base(message) { }
    public EventStoreException(string message, Exception inner) : base(message, inner) { }
    protected EventStoreException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

[Serializable]
public class EventStoreCommunicationException : EventStoreException
{
    public EventStoreCommunicationException() { }
    public EventStoreCommunicationException(string message) : base(message) { }
    public EventStoreCommunicationException(string message, Exception inner) : base(message, inner) { }
    protected EventStoreCommunicationException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
