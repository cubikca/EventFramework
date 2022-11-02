using EventFramework.EventSourcing;
using MongoDB.Driver;

namespace EventFramework.MongoDB;

public class Projection : ISubscription
{
    private readonly IClientSession _session;
    private readonly Projector _projector;
    
    public Projection(IClientSession session, Projector projector)
    {
        _session = session;
        _projector = projector;
    }
    
    public async Task Project(object @event)
    {
        var handler = _projector.Invoke(_session, @event);
        if (handler == null) return;
        await handler();
    }
}

public delegate Func<Task>? Projector(IClientSession session, object @event);