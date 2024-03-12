namespace Server.NetWork;

public interface ISession
{
    long Send(byte[] buffer);
    bool SendAsync(byte[] buffer);
}