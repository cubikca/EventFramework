namespace EventFramework.EventSourcing;

public interface ICheckpointStore
{
    // all null if the checkpoint doesn't exist
    Task<(string?, int?, long?)> GetCheckpoint();
    Task StoreCheckpoint(string topic, int partition, long offset);
}