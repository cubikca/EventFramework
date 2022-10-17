namespace EventFramework.EventSourcing;

public interface IAggregateState<T>
{
    string StreamName { get; }
    long Version { get; }
    T When(T state, object @event);
    string GetStreamName(Ulid id);
}