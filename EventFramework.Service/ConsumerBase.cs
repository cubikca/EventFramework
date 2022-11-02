using System.Diagnostics;
using EventFramework.EventSourcing;
using EventFramework.SharedKernel;
using MassTransit;
using OpenTelemetry.Context.Propagation;

namespace EventFramework.Service;

public abstract class ConsumerBase<TAggregate, TEvent> : IConsumer<TEvent>
where TAggregate : AggregateRoot<TAggregate>
where TEvent : class
{
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    protected readonly ApplicationService<TAggregate> Service;
    protected readonly ActivitySource ActivitySource;

    public ConsumerBase(ApplicationService<TAggregate> service, ActivitySource activitySource)
    {
        Service = service;
        ActivitySource = activitySource;
    }
    
    public async Task Consume(ConsumeContext<TEvent> context)
    {
        var parentContext = Propagator.Extract(default, context, ExtractContext);
        using var activity = ActivitySource.StartActivity($"{typeof(TAggregate).Name}.{typeof(TEvent).Name}",
            ActivityKind.Consumer, parentContext.ActivityContext);
        try
        {
            await Service.Handle(context.Message);
            await context.RespondAsync(new ApiResponse { Success = true });
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            var @event = new ActivityEvent("Exception", DateTimeOffset.UtcNow, new ActivityTagsCollection
            {
                { "message", ex.Message },
                { "stackTrace", ex.StackTrace }
            });
            activity?.AddEvent(@event);
            await context.RespondAsync(new ApiResponse
                { Success = false, Message = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    private IEnumerable<string> ExtractContext(ConsumeContext<TEvent> context, string key)
    {
        if (context.Headers.TryGetHeader(key, out var value))
            return new[] { value?.ToString() ?? string.Empty };
        return Enumerable.Empty<string>();
    }
}