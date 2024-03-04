using System.Buffers;
using Server.Launcher.Message;
using Server.Launcher.StartUp;
using Server.NetWork;
using Server.NetWork.Messages;
using Server.NetWork.TCPSocket;
using Server.Proto;
using Server.Proto.Proto;
using Server.Utility;

/// <summary>
/// 网关服务器
/// </summary>
[StartUpTag(ServerType.Gateway)]
internal sealed class AppStartUpGateway : AppStartUpBase
{
    static readonly NLog.Logger Log = LogManager.GetCurrentClassLogger();
    private TcpClientMessage client;
    IMessageEncoderHandler messageEncoderHandler = new MessageEncoderHandler();
    IMessageDecoderHandler messageDecoderHandler = new MessageDecoderHandler();

    public override async Task EnterAsync()
    {
        try
        {
            Log.Info($"开始启动服务器{ServerType}");
            // UDP server address
            string address = Setting.CenterUrl;
            // UDP server port
            int port = Setting.GrpcPort;
            Console.WriteLine($"启动服务器{ServerType} 开始! address: {address}  port: {port}");


            // Create a new TCP chat client
            client = new TcpClientMessage(address, port);
            // Connect the client
            Console.Write("开始链接到中心服务器 ...");
            client.Connect();
            Console.WriteLine("链接完成!");
            ReqHeartBeat reqHeartBeat = new ReqHeartBeat();
            while (client.IsConnected)
            {
                await Task.Delay(5000);

                if (!AppExitToken.IsCompleted && client.IsConnected)
                {
                    reqHeartBeat.Timestamp = TimeHelper.UnixTimeSeconds();
                    SendMessage(reqHeartBeat);
                }
            }

            Console.WriteLine("等待!");
            await AppExitToken;
            Console.Write("全部断开...");
            Console.WriteLine("Done!");
        }
        catch (Exception e)
        {
            Stop(e.Message);
            AppExitSource.TrySetException(e);
            Console.WriteLine(e);
        }
    }

    private void SendMessage(IMessage message)
    {
        message.UniqueId = Guid.NewGuid().ToString("N");
        var span = messageEncoderHandler.Handler(message);
        if (Setting.IsDebug && Setting.IsDebugSend)
        {
            Console.WriteLine($"---发送消息ID:[{ProtoMessageIdHandler.GetReqMessageIdByType(message.GetType())}] ==>消息类型:{message.GetType()} 消息内容:{message}");
        }

        client.Send(span);
        ArrayPool<byte>.Shared.Return(span);
    }

    public override void Stop(string message = "")
    {
        base.Stop(message);
        client.DisconnectAndStop();
    }
}