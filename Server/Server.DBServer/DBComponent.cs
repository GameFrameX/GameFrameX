using MongoDB.Bson;

namespace Geek.Server.DBServer;

using MongoDB.Driver;
using System.Collections.Concurrent;

public sealed class DBComponent
{
    private readonly MongoClient _client;
    private readonly IMongoDatabase _database;
    private readonly ConcurrentQueue<FilterDefinition<BsonDocument>> _filterQueue;

    public DBComponent(string connectionString, string databaseName)
    {
        _client = new MongoClient(connectionString);
        _database = _client.GetDatabase(databaseName);
        _filterQueue = new ConcurrentQueue<FilterDefinition<BsonDocument>>();
    }

    public async Task<long> CountAsync(string collectionName)
    {
        var collection = _database.GetCollection<BsonDocument>(collectionName);
        return await collection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);
    }

    public void EnqueueFilter(FilterDefinition<BsonDocument> filter)
    {
        _filterQueue.Enqueue(filter);
    }

    public async Task<FilterDefinition<BsonDocument>> DequeueFilterAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_filterQueue.TryDequeue(out var filter))
            {
                return filter;
            }

            await Task.Delay(100, cancellationToken);
        }

        return null;
    }

    public async Task<List<BsonDocument>> FindAsync(string collectionName, FilterDefinition<BsonDocument> filter, int limit = 0)
    {
        var collection = _database.GetCollection<BsonDocument>(collectionName);
        var result = new List<BsonDocument>();
        using (var cursor = await collection.FindAsync(filter, new FindOptions<BsonDocument> {Limit = limit}))
        {
            while (await cursor.MoveNextAsync())
            {
                result.AddRange(cursor.Current);
            }
        }

        return result;
    }

    public async Task<List<BsonDocument>> FindAsync(string collectionName, CancellationToken cancellationToken)
    {
        var filter = await DequeueFilterAsync(cancellationToken);
        if (filter == null) return null;
        return await FindAsync(collectionName, filter);
    }

    public async Task InsertOneAsync(string collectionName, BsonDocument document)
    {
        var collection = _database.GetCollection<BsonDocument>(collectionName);
        await collection.InsertOneAsync(document);
    }
}