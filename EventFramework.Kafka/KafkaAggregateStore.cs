using System.Text;
using Confluent.Kafka;
using EventFramework.EventSourcing;
using BindingFlags = System.Reflection.BindingFlags;

namespace EventFramework.Kafka;

public class KafkaAggregateStore : IAggregateStore
{
    private readonly string _namespace;
    private readonly IConsumer<string, object> _consumer;
    private readonly IProducer<string, object> _producer;

    public KafkaAggregateStore(string @namespace, IConsumer<string, object> consumer, IProducer<string, object> producer)
    {
        _namespace = @namespace;
        _consumer = consumer;
        _producer = producer;
    }
    private string GetTopicName<T>(AggregateId<T> aggregateId)
    {
        return $"{_namespace}.{typeof(T).Name.ToLower()}.{aggregateId}";
    }
    
    public Task<bool> Exists<T>(AggregateId<T> aggregateId) where T : AggregateRoot<T>
    {
        var topicName = GetTopicName(aggregateId);
        _consumer.Subscribe(topicName);
        var result = Task.FromResult(!_consumer.Consume(100).IsPartitionEOF);
        _consumer.Unsubscribe();
        return result;
    }

    public async Task Save<T>(T aggregate) where T : AggregateRoot<T>
    {
        var topicName = GetTopicName(aggregate.Id);
        foreach (var change in aggregate.GetChanges())
        {
            var message = new Message<string, object>
            {
                Key = $"{typeof(T).Name}-{aggregate.Id}",
                Value = change
            };
            await _producer.ProduceAsync(topicName, message);
        }
        aggregate.ClearChanges();
    }

    public Task<T?> Load<T>(AggregateId<T> aggregateId) where T : AggregateRoot<T>
    {
        var topicName = GetTopicName(aggregateId);
        _consumer.Subscribe(topicName);
        var ctr = typeof(T).GetConstructor(BindingFlags.Instance, new[] { aggregateId.GetType() });
        if (ctr == null) return Task.FromResult<T?>(null);
        var aggregate = (T) ctr.Invoke(new object[] {aggregateId});
        var consumeResult = _consumer.Consume(100);
        while (!consumeResult.IsPartitionEOF)
        {
            var @event = consumeResult.Message.Value;
            aggregate?.Handle(@event);
            consumeResult = _consumer.Consume(100);
        }
        _consumer.Unsubscribe();
        aggregate?.ClearChanges();
        return Task.FromResult(aggregate);
    }
}