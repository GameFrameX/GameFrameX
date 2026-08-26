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

using GameFrameX.Foundation.Utility;

namespace GameFrameX.Apps.Common.Session;

public sealed class Session
{
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="sessionId">连接会话ID</param>
    /// <param name="netWorkChannel">网络渠道对象</param>
    public Session(string sessionId, INetWorkChannel netWorkChannel)
    {
        WorkChannel = netWorkChannel;
        SessionId = sessionId;
        CreateTime = TimerHelper.UnixTimeSeconds();
    }

    /// <summary>
    /// 全局会话ID
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// 玩家ID
    /// </summary>
    public long PlayerId { get; private set; }

    /// <summary>
    /// 连接时间
    /// </summary>
    public long CreateTime { get; }

    /// <summary>
    /// 连接上下文
    /// </summary>
    [JsonIgnore]
    public INetWorkChannel WorkChannel { get; }

    /// <summary>
    /// 连接标示，避免自己顶自己的号,客户端每次启动游戏生成一次/或者每个设备一个
    /// </summary>
    public string Sign { get; private set; }

    /// <summary>
    /// 设置玩家ID
    /// </summary>
    /// <param name="playerId">玩家ID</param>
    public void SetPlayerId(long playerId)
    {
        PlayerId = playerId;
    }

    /// <summary>
    /// 设置签名
    /// </summary>
    /// <param name="sign">签名</param>
    public void SetSign(string sign)
    {
        Sign = sign;
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="messageObject">消息对象</param>
    /// <param name="errorCode">消息错误码</param>
    public async Task WriteAsync(MessageObject messageObject, int errorCode = 0)
    {
        if (WorkChannel != null)
        {
            await WorkChannel.WriteAsync(messageObject, errorCode);
        }
    }
}