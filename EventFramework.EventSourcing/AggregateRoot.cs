namespace EventFramework.EventSourcing;

public abstract class AggregateRoot<T> : IInternalEventHandler where T : AggregateRoot<T>
{
    private readonly List<object> _changes = new();

    public AggregateId<T> Id { get; protected set; }
    public int Version { get; protected set; } = -1;

    protected AggregateRoot(AggregateId<T> id)
    {
        Id = id;
    }

    protected abstract void EnsureValidState();
    protected abstract void When(object? @event);

    public void Handle(object @event)
    {
        When(@event);
    }

    protected void Apply(object @event)
    {
        When(@event);
        EnsureValidState();
        _changes.Add(@event);
    }

    public IEnumerable<object> GetChanges()
    {
        return _changes.AsEnumerable();
    }

    public void Load(IEnumerable<object?> history)
    {
        foreach (var ev in history)
        {
            When(ev);
            Version++;
        }
    }

    public void ClearChanges()
    {
        _changes.Clear();
    }

    protected static void ApplyToEntity(IInternalEventHandler? entity, object @event)
    {
        entity?.Handle(@event);
    }
}