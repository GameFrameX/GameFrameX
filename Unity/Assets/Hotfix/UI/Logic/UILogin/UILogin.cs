using System.Collections.Generic;
using System.Net;
using GameFrameX;
using GameFrameX.Event.Runtime;
using GameFrameX.GlobalConfig.Runtime;
#if ENABLE_UI_FAIRYGUI
using GameFrameX.UI.FairyGUI.Runtime;
#endif
using GameFrameX.Network.Runtime;
using GameFrameX.Runtime;
using GameFrameX.UI.Runtime;
#if ENABLE_UI_UGUI
using GameFrameX.UI.UGUI.Runtime;
#endif
using Hotfix.Manager;
using Hotfix.Network;
using Hotfix.Proto;
using UnityEngine;

namespace Hotfix.UI
{
    public partial class UILogin
    {
        public override void OnAwake()
        {
            UIGroup = GameApp.UI.GetUIGroup(UIGroupConstants.Normal.Name);
            base.OnAwake();
        }

        public override void OnOpen(object userData)
        {
            m_enter.onClick.Set(OnLoginClick);
            base.OnOpen(userData);
        }

        private void OnLoginClick()
        {
            Login();
        }

        private async void Login()
        {
            if (m_UserName.text.IsNullOrWhiteSpace() || m_Password.text.IsNullOrWhiteSpace())
            {
                m_ErrorText.text = "用户名或密码不能为空";
                return;
            }


            #region 账号登录

            var req = new ReqLogin
            {
                SdkType = 0,
                SdkToken = "",
                UserName = m_UserName.text,
                Password = m_Password.text,
                Device = SystemInfo.deviceUniqueIdentifier
            };
            req.Platform = ApplicationHelper.PlatformName;

            var respLogin = await GameApp.WebProtoBuff.Post<RespLogin>($"http://127.0.0.1:28080/game/api/{nameof(ReqLogin).ConvertToSnakeCase()}", req);
            if (respLogin.ErrorCode > 0)
            {
                Log.Error("登录失败，错误信息:" + respLogin.ErrorCode);
                return;
            }

            #endregion

            #region 获取角色列表

            ReqPlayerList reqPlayerList = new ReqPlayerList();

            reqPlayerList.Id = respLogin.Id;
            var respPlayerList = await GameApp.WebProtoBuff.Post<RespPlayerList>($"http://127.0.0.1:28080/game/api/{nameof(ReqPlayerList).ConvertToSnakeCase()}", reqPlayerList);
            if (respPlayerList.ErrorCode > 0)
            {
                Log.Error("登录失败，错误信息:" + respPlayerList.ErrorCode);
                return;
            }

            AccountManager.Instance.PlayerList = respPlayerList.PlayerList;

            #endregion

            if (respPlayerList.PlayerList.Count > 0)
            {
                await GameApp.UI.OpenFullScreenAsync<UIPlayerList>(Utility.Asset.Path.GetUIPath(nameof(UILogin)), respLogin);
            }
            else
            {
                await GameApp.UI.OpenFullScreenAsync<UIPlayerCreate>(Utility.Asset.Path.GetUIPath(nameof(UILogin)), respLogin);
            }

            GameApp.UI.CloseUIForm(this);
        }
    }
}