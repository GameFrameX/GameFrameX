namespace Server.DBServer.Storage;

public static class XxHashExtension
{
    /// <summary>
    /// 判断是否是默认值
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static bool IsDefault(this Standart.Hash.xxHash.uint128 self)
    {
        return (self.high64 == 0) && (self.low64 == 0);
    }
}