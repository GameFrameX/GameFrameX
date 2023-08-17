using MongoDB.Driver;

namespace Server.DBServer.DbService.MongoDB;

public static class MongoDbExtensions
{
    public static IMongoCollection<TDocument> GetCollection<TDocument>(this IMongoDatabase self, MongoCollectionSettings settings = null)
    {
        return self.GetCollection<TDocument>(typeof(TDocument).FullName, settings);
    }
}