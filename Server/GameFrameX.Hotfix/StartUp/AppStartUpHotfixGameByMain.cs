// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
//  
//  使用本项目须严格遵守相应法律法规及开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//  
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory of the source code for the full license text.
//  
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes and liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//  
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using GameFrameX.Apps.Common.Session;
using GameFrameX.Hotfix.Logic.Game.Room;
using GameFrameX.SuperSocket.Connection;
using GameFrameX.SuperSocket.Server.Abstractions.Session;
using GameFrameX.SuperSocket.WebSocket.Server;

namespace GameFrameX.Hotfix.StartUp;

/// <summary>
/// 业务服务器.最后启动。
/// </summary>
internal partial class AppStartUpHotfixGame
{
    public override async Task StartAsync()
    {
        // 启动网络服务
        // 设置压缩和解压缩
        await StartServerAsync<DefaultMessageDecoderHandler, DefaultMessageEncoderHandler>(new DefaultMessageCompressHandler(), new DefaultMessageDecompressHandler(), HotfixManager.GetListHttpHandler(), HotfixManager.GetHttpHandler);
        // 启动Http服务
        // await HttpServer.Start(Setting.HttpPort, Setting.HttpsPort, HotfixManager.GetListHttpHandler(), HotfixManager.GetHttpHandler, null, Setting.HttpUrl);
    }

    public async Task RunServer(bool reload = false)
    {
        // 不管是不是重启服务器，都要加载配置
        await ConfigComponent.Instance.LoadConfig();
        if (reload)
        {
            ActorManager.ClearAgent();
            return;
        }

        await StartAsync();
    }


    protected override async ValueTask OnDisconnected(IAppSession appSession, CloseEventArgs disconnectEventArgs)
    {
        LogHelper.Info("有外部客户端网络断开连接成功！。断开信息：" + appSession.SessionID + "  " + disconnectEventArgs.Reason);
        var session = SessionManager.Remove(appSession.SessionID);
        if (session != null && session.PlayerId > 0)
        {
            var roomAgent = await ActorManager.GetComponentAgent<RoomComponentAgent>();
            await roomAgent.MarkPlayerDisconnected(session.PlayerId);
        }
    }

    protected override async ValueTask OnConnected(IAppSession appSession)
    {
        LogHelper.Info("有外部客户端网络连接成功！。链接信息：SessionID:" + appSession.SessionID + " RemoteEndPoint:" + appSession.RemoteEndPoint);
        var netChannel = new DefaultNetWorkChannel(appSession, Setting);
        var count = SessionManager.Count();
        if (count > Setting.MaxClientCount)
        {
            // 达到最大在线人数限制
            await netChannel.WriteAsync(new NotifyServerFullyLoaded(), (int)OperationStatusCode.ServerFullyLoaded);
            netChannel.Close();
            return;
        }

        var session = new Session(appSession.SessionID, netChannel);
        SessionManager.Add(session);
    }

    /// <summary>
    /// 处理收到的消息结果
    /// </summary>
    /// <param name="appSession"></param>
    /// <param name="message"></param>
    protected override async ValueTask PackageHandler(IAppSession appSession, IMessage message)
    {
        if (message is NetworkMessagePackage messagePackage)
        {
            var netWorkChannel = SessionManager.GetChannel(appSession.SessionID);

            if (netWorkChannel.IsNull())
            {
                return;
            }

            var actorId = netWorkChannel.GetData<long>(GlobalConst.ActorIdKey);
            if (messagePackage.Header.OperationType == (byte)MessageOperationType.HeartBeat)
            {
                if (Setting.IsDebug && Setting.IsDebugReceive && Setting.IsDebugReceiveHeartBeat)
                {
                    LogHelper.Debug($"---收到{messagePackage.ToFormatMessageString(actorId)}");
                }

                // 心跳消息回复
                ReplyHeartBeat(netWorkChannel, (MessageObject)messagePackage.DeserializeMessageObject());
                return;
            }

            if (Setting.IsDebug && Setting.IsDebugReceive)
            {
                LogHelper.Debug($"---收到{messagePackage.ToFormatMessageString(actorId)}");
            }

            var handler = HotfixManager.GetTcpHandler(messagePackage.Header.MessageId);
            if (handler == null)
            {
                LogHelper.Error($"找不到[{messagePackage.Header.MessageId}][{messagePackage.MessageType}]对应的handler");
                return;
            }

            // 执行消息分发处理
            try
            {
                await InvokeMessageHandler(handler, messagePackage.DeserializeMessageObject(), netWorkChannel);
            }
            catch (Exception exception)
            {
                LogHelper.Fatal(exception);
            }
        }
    }

    public override async Task StopAsync(string message = "")
    {
        await base.StopAsync(message);
        // 断开所有连接
        await SessionManager.RemoveAll();
        // 取消所有未执行定时器
        await QuartzTimer.Stop();
        // 保证actor之前的任务都执行完毕
        await ActorManager.AllFinish();
        // 存储所有数据
        await GlobalTimer.Stop();
        // 删除所有actor
        await ActorManager.RemoveAll();
    }
}
