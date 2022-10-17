namespace EventFramework.EventSourcing;

public interface IInternalEventHandler
{
    void Handle(object @event);
}