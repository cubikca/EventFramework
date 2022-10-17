using System.Text.Json.Serialization;

namespace EventFramework.EventSourcing;

public abstract class Entity<TId> : IInternalEventHandler
    where TId : Value<TId>
{
    public TId Id { get; set; }
    [JsonIgnore]
    protected readonly Action<object>? Applier;
    
    protected Entity(TId id, Action<object>? applier)
    {
        Id = id;
        Applier = applier;
    }
    
    protected abstract void When(object @event);
    
    public void Handle(object @event)
    {
        When(@event);
        Applier?.Invoke(@event);
    }
}