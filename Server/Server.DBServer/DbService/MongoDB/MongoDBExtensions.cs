using MongoDB.Driver;

namespace Server.DBServer.DbService.MongoDB;

public static class MongoDbExtensions
{
    /// <summary>
    /// 获取指定类型的MongoDB集合。
    /// </summary>
    /// <typeparam name="TDocument">文档的类型。</typeparam>
    /// <param name="self">要操作的MongoDB数据库。</param>
    /// <param name="settings">集合的设置。</param>
    /// <returns>指定类型的MongoDB集合。</returns>
    public static IMongoCollection<TDocument> GetCollection<TDocument>(this IMongoDatabase self, MongoCollectionSettings settings = null)
    {
        return self.GetCollection<TDocument>(typeof(TDocument).FullName, settings);
    }
}