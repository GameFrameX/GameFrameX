using Server.NetWork.Messages;

namespace Server.NetWork;

public interface INetChannel
{
    string RemoteAddress { get; set; }
    void Write(IMessage messageObject);
    void WriteAsync(IMessage msg, int uniId, int code = 0, string desc = "");

    T GetData<T>(string key);
    void RemoveData(string key);
    void SetData(string key, object v);
}