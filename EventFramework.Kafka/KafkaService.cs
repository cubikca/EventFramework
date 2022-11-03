using Microsoft.Extensions.Hosting;

namespace EventFramework.Kafka;

public class KafkaService : IHostedService
{
    private readonly IEnumerable<SubscriptionManager> _subscriptionManagers;

    public KafkaService(IEnumerable<SubscriptionManager> subscriptionManagers)
    {
        _subscriptionManagers = subscriptionManagers;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(_subscriptionManagers.Select(sm => sm.Start()));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(_subscriptionManagers.Select(sm => sm.Stop()));
    }
}