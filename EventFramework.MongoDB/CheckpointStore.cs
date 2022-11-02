using EventFramework.EventSourcing;
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
    
    public async Task<ulong?> GetCheckpoint()
    {
        var checkpoints = _mongo.GetDatabase(_databaseName).GetCollection<Checkpoint>(_checkpointName);
        var checkpoint = await checkpoints.AsQueryable().FirstOrDefaultAsync(c => c.Id == _checkpointName);
        return checkpoint?.Position ?? 0UL;
    }

    public async Task StoreCheckpoint(ulong? position)
    {
        var checkpoints = _mongo.GetDatabase(_databaseName).GetCollection<Checkpoint>(_checkpointName);
        var checkpoint = await checkpoints.AsQueryable().SingleOrDefaultAsync(c => c.Id == _checkpointName);
        checkpoint ??= new Checkpoint { Id = _checkpointName };
        checkpoint.Position = position;
        await checkpoints.ReplaceOneAsync(c => c.Id == _checkpointName, checkpoint,
            new ReplaceOptions { IsUpsert = true });
    }
}