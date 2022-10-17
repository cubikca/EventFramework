namespace EventFramework.EventSourcing;

public abstract class ApplicationService<T> where T : AggregateRoot<T>
{
    private readonly Dictionary<Type, Func<object, Task>> _handlers = new();
    protected readonly IAggregateStore Store;

    protected ApplicationService(IAggregateStore store)
    {
        Store = store;
    }

    public Task Handle<TEvent>(TEvent @event) where TEvent : class
    {
        if (!_handlers.TryGetValue(typeof(TEvent), out var handler))
            throw new InvalidOperationException($"No registered handler for event {typeof(TEvent).Name}");
        return handler(@event);
    }

    private void When<TEvent>(Func<TEvent, Task> handler) where TEvent : class
    {
        _handlers.Add(typeof(TEvent), c => handler((TEvent) c));
    }

    protected void CreateWhen<TEvent>(
            Func<TEvent, AggregateId<T>> getAggregateId,
            Func<TEvent, AggregateId<T>, T> creator
        ) where TEvent : class
    {
        When<TEvent>(
            async ev =>
            {
                var aggregateId = getAggregateId(ev);
                if (await Store.Exists(aggregateId))
                    throw new InvalidOperationException($"Entity with id {aggregateId} already exists");
                var aggregate = creator(ev, aggregateId);
                await Store.Save(aggregate);
            });
    }

    protected void UpdateWhen<TEvent>(
            Func<TEvent, AggregateId<T>> getAggregateId,
            Action<T, TEvent> updater
        ) where TEvent : class
    {
        When<TEvent>(
            async ev =>
            {
                var aggregateId = getAggregateId(ev);
                var aggregate = await Store.Load(aggregateId);
                updater(aggregate, ev);
                await Store.Save(aggregate);
            });
    }
}