using EventFramework.EventSourcing;
using EventFramework.SharedKernel;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace EventFramework.MongoDB;

public class CheckpointStore : ICheckpointStore
{
    private readonly string _databaseName;
    private readonly string _checkpointName;
    private readonly IMongoClient _mongo;

    public CheckpointStore(IMongoClient mongo, string databaseName, string checkpointName)
    {
        _mongo = mongo;
        _databaseName = databaseName;
        _checkpointName = checkpointName;
    }
    
    public async Task<(string?, int?, long?)> GetCheckpoint()
    {
        var checkpoints = _mongo.GetDatabase(_databaseName).GetCollection<Checkpoint>(_checkpointName);
        var checkpoint = await checkpoints.AsQueryable().FirstOrDefaultAsync(c => c.Id == _checkpointName);
        return (checkpoint?.Topic, checkpoint?.Partition, checkpoint?.Offset);
    }

    public async Task StoreCheckpoint(string topic, int partition, long offset)
    {
        var checkpoints = _mongo.GetDatabase(_databaseName).GetCollection<Checkpoint>(_checkpointName);
        var checkpoint = await checkpoints.AsQueryable().SingleOrDefaultAsync(c => c.Id == _checkpointName);
        checkpoint ??= new Checkpoint { Id = _checkpointName };
        checkpoint.Topic = topic;
        checkpoint.Partition = partition;
        checkpoint.Offset = offset;
        await checkpoints.ReplaceOneAsync(c => c.Id == _checkpointName, checkpoint,
            new ReplaceOptions { IsUpsert = true });
    }
}