namespace Server.Setting;

/// <summary>
/// 需要小于1000，因为1000以上作为服务器id了
/// </summary>
public enum IDModule
{
    MIN = 0,

    //单服/玩家不同即可
    Pet = 101,
    Equip = 102,
    WorkerActor = 103,
    MAX = 999
}