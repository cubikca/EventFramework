namespace EventFramework.EventSourcing;

public interface ICheckpointStore
{
    Task<ulong?> GetCheckpoint();
    Task StoreCheckpoint(ulong? position);
}