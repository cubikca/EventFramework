namespace EventFramework.EventSourcing;

public abstract class AggregateId<T> : Value<AggregateId<T>>
{
    protected string Value { get; }
    
    protected AggregateId(string? value)
    {
        if (value is null or null) throw new ArgumentNullException(nameof(value), "The Id cannot be empty");
        Value = value;
    }
    
    public static implicit operator string(AggregateId<T> self) => self.Value;
    
    public override string ToString() => Value;
}