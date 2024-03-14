/*using MongoDB.Bson;
using MongoDB.Driver;
using Server.DBServer.DbService.MongoDB;
using Server.DBServer.State;
using Server.Log;

namespace Server.DBServer.Storage
{
    public static class FileBackup
    {
        public static FileBackupStatus CheckRestoreFromFile()
        {
            var folder = Environment.CurrentDirectory + "/../State/";
            if (Directory.Exists(folder))
            {
                LogHelper.Warn("need restore...");

                try
                {
                    var curDataBase = GameDb.As<MongoDbServiceConnection>().CurrentDatabase;
                    var root = new DirectoryInfo(folder);
                    foreach (var dir in root.GetDirectories())
                    {
                        foreach (var jsonDir in dir.GetDirectories())
                        {
                            foreach (var file in jsonDir.GetFiles())
                            {
                                var batchList = new List<ReplaceOneModel<BsonDocument>>();
                                var col = curDataBase.GetCollection<BsonDocument>(jsonDir.Name);
                                var fileStr = File.ReadAllText(file.FullName);
                                BsonDocument bsonElements = BsonDocument.Parse(fileStr);
                                var filter = Builders<BsonDocument>.Filter.Eq(CacheState.UniqueId, bsonElements.GetValue(CacheState.UniqueId));
                                var ret = new ReplaceOneModel<BsonDocument>(filter, bsonElements) { IsUpsert = true };
                                batchList.Add(ret);
                                //保存数据
                                var result = col.BulkWrite(batchList);
                                if (!result.IsAcknowledged)
                                {
                                    LogHelper.Warn($"restore {jsonDir.Name} fail");
                                    return FileBackupStatus.StoreToDbFailed;
                                }
                            }
                        }
                    }

                    //删除目录文件夹
                    var destDir = Environment.CurrentDirectory + $"/../State_Back";
                    destDir.CreateAsDirectory();
                    Directory.Move(folder, $"{destDir}/{DateTime.Now:yyyy-MM-dd-HH-mm}");

                    return FileBackupStatus.StoreToDbSuccess;
                }
                catch (Exception e)
                {
                    LogHelper.Fatal(e.ToString());
                    //回存数据失败 不予启服
                    return FileBackupStatus.StoreToDbFailed;
                }
            }
            else
            {
                return FileBackupStatus.NoFile;
            }
        }

        public static async Task SaveToFile(List<ReplaceOneModel<MongoState>> list, string stateName)
        {
            var folder = Environment.CurrentDirectory + $"/../State/{DateTime.Now:yyyy-MM-dd-HH-mm}/{stateName}/";
            folder.CreateAsDirectory();
            foreach (var one in list)
            {
                var state = one.Replacement;
                Newtonsoft.Json.JsonConvert.SerializeObject(state);
                var str = Newtonsoft.Json.JsonConvert.SerializeObject(state);
                var path = folder + state.Id + ".json";
                await File.WriteAllTextAsync(path, str);
            }
        }

        /// <summary>
        /// 根据字符串创建目录,递归
        /// </summary>
        public static void CreateAsDirectory(this string path, bool isFile = false)
        {
            if (isFile)
            {
                path = Path.GetDirectoryName(path);
            }

            if (!Directory.Exists(path))
            {
                CreateAsDirectory(path, true);
                Directory.CreateDirectory(path);
            }
        }
    }


    public enum FileBackupStatus
    {
        NoFile,
        StoreToDbFailed,
        StoreToDbSuccess,
    }
}*/