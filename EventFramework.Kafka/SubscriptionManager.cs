using Confluent.Kafka;
using EventFramework.EventSourcing;

namespace EventFramework.Kafka;

public class SubscriptionManager
{
    private readonly ICheckpointStore _checkpointStore;
    private readonly IConsumer<string, object> _kafka;
    private readonly string _namespace;
    private readonly CancellationTokenSource _tokenSource;
    private readonly ISubscription[] _subscriptions;

    public SubscriptionManager(
        IConsumer<string, object> kafka,
        ICheckpointStore checkpointStore,
        string @namespace,
        CancellationTokenSource tokenSource,
        params ISubscription[] subscriptions)
    {
        _kafka = kafka;
        _checkpointStore = checkpointStore;
        _namespace = @namespace;
        _tokenSource = tokenSource;
        _subscriptions = subscriptions;
    }

    public async Task Start()
    {
        var (topic, partition, offset) = await _checkpointStore.GetCheckpoint();
        if (topic != null && partition != null && offset != null)
        {
            var tp = new TopicPartition(topic, new Partition(partition.Value));
            var tpo = new TopicPartitionOffset(tp, new Offset(offset.Value));
            _kafka.Assign(tpo);
        }
        _kafka.Subscribe($"{_namespace}.#");
        // main event loop
        // disable the "call not awaited" warning
        // we have a CancellationTokenSource that allows us to cancel this task and we need Start() to return
        // before the main event loop terminates
#pragma warning disable 4014
        Task.Factory.StartNew(async () =>
        {
            var result = _kafka.Consume(100);
            if (!result.IsPartitionEOF)
                await EventAppeared(result.Message);
            await _checkpointStore.StoreCheckpoint(result.Topic, result.Partition, result.Offset);
            _kafka.Commit(result);
        }, _tokenSource.Token);
#pragma warning restore
    }

    public Task Stop()
    {
        _tokenSource.Cancel();
        return Task.CompletedTask;
    }
    
    private async Task EventAppeared(object? @event)
    {
        // this method should work silently and ignore errors
        if (@event == null) return;
        try
        {
            await Task.WhenAll(_subscriptions.Select(x => x.Project(@event)));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}