namespace Server.DBServer.DbService.MongoDB
{
    public class MongoState
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        public const string UniqueId = nameof(Id);

        /// <summary>
        /// 时间戳名称
        /// </summary>
        public const string TimestampName = nameof(Timestamp);

        /// <summary>
        /// 唯一ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 回存时间戳
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data { get; set; }
    }
}