using System.Buffers;
using Server.Extension;
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
    private TcpClientMessage client;
    IMessageEncoderHandler messageEncoderHandler = new MessageEncoderHandler();
    IMessageDecoderHandler messageDecoderHandler = new MessageDecoderHandler();

    public override async Task EnterAsync()
    public override void Init()
    {
        if (Setting == null)
        {
            Setting = new AppSetting
            {
                ServerId = 2000,
                ServerType = ServerType.Gateway,
                TcpPort = 22000,
                GrpcPort = 33300,
                CenterUrl = "127.0.0.1",
            };
        }

        base.Init();
    }


            // Create a new TCP chat client
            client = new TcpClientMessage(address, port);
            // Connect the client
            LogHelper.Info("开始链接到中心服务器 ...");
            client.Connect();
            LogHelper.Info("链接完成!");
            TimeSpan delay = TimeSpan.FromSeconds(5);
            await Task.Delay(delay);
            ReqHeartBeat reqHeartBeat = new ReqHeartBeat();
            while (client.IsConnected)
            {
                if (!AppExitToken.IsCompleted && client.IsConnected)
                {
                    reqHeartBeat.Timestamp = TimeHelper.UnixTimeSeconds();
                    SendMessage(reqHeartBeat);
                }

                await Task.Delay(delay);
            }

            await AppExitToken;
            Console.Write("全部断开...");
            LogHelper.Info("Done!");
        }
        catch (Exception e)
        {
            Stop(e.Message);
            AppExitSource.TrySetException(e);
            LogHelper.Info(e);
        }
    }

    private void SendMessage(IMessage message)
    {
        message.UniqueId = Guid.NewGuid().ToString("N");
        var span = messageEncoderHandler.Handler(message);
        if (Setting.IsDebug && Setting.IsDebugSend)
        {
            LogHelper.Debug($"---发送消息ID:[{ProtoMessageIdHandler.GetReqMessageIdByType(message.GetType())}] ==>消息类型:{message.GetType()} 消息内容:{message}");
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