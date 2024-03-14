namespace Server.DBServer.State;

public interface ICacheState : ISafeDelete, ISafeCreate, ISafeUpdate
{
    /// <summary>
    /// 唯一ID
    /// </summary>
    long Id { get; set; }

    /// <summary>
    /// 是否修改
    /// </summary>
    bool IsModify { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isNew"></param>
    void AfterLoadFromDB(bool isNew);

    void AfterSaveToDB();
}