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

/*using GameFrameX.Apps.Account.Login.Component;
using GameFrameX.Apps.Account.Login.Entity;
using GameFrameX.Hotfix.Common;
using GameFrameX.NetWork.Abstractions;

namespace GameFrameX.Hotfix.Logic.Account.Login;

public class LoginComponentAgent : StateComponentAgent<LoginComponent, LoginState>
{
    public async Task OnLogin(INetWorkChannel workChannel, ReqLogin reqLogin)
    {
        if (reqLogin.UserName.IsNullOrEmpty() || reqLogin.Password.IsNullOrEmpty())
        {
            var respErrorCode = new RespErrorCode
            {
                ErrCode = (int)ResultCode.Failed,
            };
            await workChannel.WriteAsync(respErrorCode, (int)OperationStatusCode.AccountCannotBeNull);
            return;
        }

        var loginState = await OwnerComponent.OnLogin(reqLogin);
        if (loginState == null)
        {
            var accountId = ActorIdGenerator.GetUniqueId();
            loginState = await OwnerComponent.Register(accountId, reqLogin);
        }

        // 构建账号登录返回信息
        var respLogin = new RespLogin
        {
            UniqueId = reqLogin.UniqueId,
            Code = loginState.State,
            CreateTime = loginState.CreateTime,
            Level = loginState.Level,
            Id = loginState.Id,
            RoleName = loginState.NickName,
        };
        await workChannel.WriteAsync(respLogin);
    }


    /// <summary>
    /// 创建角色
    /// </summary>
    /// <param name="workChannel"></param>
    /// <param name="reqPlayerCreate"></param>
    public async Task OnPlayerCreate(INetWorkChannel workChannel, ReqPlayerCreate reqPlayerCreate)
    {
        var playerState = await OwnerComponent.OnPlayerCreate(reqPlayerCreate);
        var respPlayerCreate = new RespPlayerCreate
        {
            UniqueId = reqPlayerCreate.UniqueId,
            PlayerInfo = new PlayerInfo
            {
                Id = playerState.Id,
                Name = playerState.Name,
                Level = playerState.Level,
                State = playerState.State,
                Avatar = playerState.Avatar,
            },
        };
        await workChannel.WriteAsync(respPlayerCreate);
    }

    /// <summary>
    /// 获取角色列表
    /// </summary>
    /// <param name="workChannel"></param>
    /// <param name="reqPlayerList"></param>
    public async Task OnGetPlayerList(INetWorkChannel workChannel, ReqPlayerList reqPlayerList)
    {
        var playerList = await OwnerComponent.GetPlayerList(reqPlayerList);

        var respPlayerList = new RespPlayerList
        {
            UniqueId = reqPlayerList.UniqueId,
            PlayerList = new List<PlayerInfo>(),
        };
        if (playerList != null)
        {
            foreach (var playerState in playerList)
            {
                var playerInfo = new PlayerInfo
                {
                    Id = playerState.Id,
                    Name = playerState.Name,
                };
                playerInfo.Level = playerState.Level;
                playerInfo.State = playerState.State;
                playerInfo.Avatar = playerState.Avatar;
                respPlayerList.PlayerList.Add(playerInfo);
            }
        }

        await workChannel.WriteAsync(respPlayerList);
    }
}*/