namespace EventFramework.EventSourcing;

public interface IAggregateStore
{
    Task<bool> Exists<T>(AggregateId<T> aggregateId) where T : AggregateRoot<T>;

    Task Save<T>(T aggregate) where T : AggregateRoot<T>;

    // will return null if the correct constructor is not provided
    Task<T?> Load<T>(AggregateId<T> aggregateId) where T : AggregateRoot<T>;
}