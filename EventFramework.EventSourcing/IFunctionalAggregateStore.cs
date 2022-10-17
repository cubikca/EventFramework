namespace EventFramework.EventSourcing;

public interface IFunctionalAggregateStore
{
    Task Save<T>(long version, AggregateState<T>.Result update) where T : class, IAggregateState<T>, new();
    Task<T> Load<T>(Ulid id) where T : IAggregateState<T>, new();
}