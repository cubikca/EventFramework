namespace EventFramework.EventSourcing;

public interface ISubscription
{
    Task Project(object @event);  
}