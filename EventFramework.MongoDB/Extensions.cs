using EventFramework.SharedKernel;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace EventFramework.MongoDB;

public static class Extensions
{
    public static async Task Update<T>(
        this IMongoCollection<T> collection,
        string id,
        Func<T, Task> update) where T : IId
    { 
        var item = await collection.AsQueryable().SingleOrDefaultAsync(x => x.Id == id);
        if (item == null)
            throw new Exception($"Unable to update {typeof(T).Name}: not found");
        await update(item);
        await collection.ReplaceOneAsync(Builders<T>.Filter.Where(x => x.Id == id), item);
    }

    public static async Task UpsertItem<T>(
        this IMongoCollection<T> collection,
        string id,
        Func<T, Task> update,
        Func<Task<T>> create) where T : IId
    {
        var item = await collection.AsQueryable().SingleOrDefaultAsync(x => x.Id == id);
        if (item == null)
        {
            item = await create();
            await collection.InsertOneAsync(item);
        }
        else
        {
            await update(item);
            await collection.ReplaceOneAsync(Builders<T>.Filter.Where(x => x.Id == id), item, 
                new ReplaceOptions { IsUpsert = true });
        }
    }

    public static async Task Delete<T>(
        this IMongoCollection<T> collection,
        string id) where T : IId
    {
        await collection.DeleteOneAsync(Builders<T>.Filter.Where(x => x.Id == id));
    }
}