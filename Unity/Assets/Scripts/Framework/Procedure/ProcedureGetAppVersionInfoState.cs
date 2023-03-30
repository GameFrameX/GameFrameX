using System.Text;
using Game.Model;
using GameFramework.Fsm;
using GameFramework.Localization;
using GameFramework.Procedure;
using GlobalConfig;
using UnityEngine;
using UnityEngine.Networking;
using UnityGameFramework.Runtime;
using YooAsset;
using Utility = GameFramework.Utility;

namespace UnityGameFramework.Procedure
{
    /// <summary>
    /// 获取版本信息
    /// </summary>
    public sealed class ProcedureGetAppVersionInfoState : ProcedureBase
    {
        private UnityWebRequest _www;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            // 编辑器下的模拟模式
            if (GameEntry.GetComponent<AssetComponent>().GamePlayMode == EPlayMode.EditorSimulateMode)
            {
                Debug.Log("当前为编辑器模式，直接启动 FsmGetAppVersionInfoState");
                ChangeState<ProcedurePatchInit>(procedureOwner);
                return;
            }

            _www = UnityWebRequest.Post(GameEntry.GetComponent<GlobalConfigComponent>().CheckAppVersionUrl, string.Empty);
            string jsonParams = Utility.Json.ToJson(HttpHelper.GetBaseParams());
            Log.Info(jsonParams);
            _www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonParams));
            _www.uploadHandler.contentType = "application/json; charset=utf-8";
            _www.timeout = 3;
            var async = _www.SendWebRequest();
            async.completed += (AsyncOperation async2) =>
            {
                var json = _www.downloadHandler.text;
                Debug.Log(json);
                if (!string.IsNullOrEmpty(_www.error) || string.IsNullOrEmpty(json))
                {
                    //todo 提示用户
                    LauncherUIHandler.SetTipText("Network error, retrying...");
                    Debug.LogError($"获取版本信息异常=>Error:{_www.error}   Req:{jsonParams}");
                    // GAHelper.DesignEvent("GetAppVersionInfoNetworkError");
                    OnEnter(procedureOwner);
                }
                else
                {
                    HttpJsonResult httpJsonResult = Utility.Json.ToObject<HttpJsonResult>(json);
                    if (httpJsonResult.code > 0)
                    {
                        LauncherUIHandler.SetTipText("Server error, retrying...");
                        Debug.LogError($"获取全局信息返回异常=> Req:{jsonParams} Resp:{json}");
                        // GAHelper.DesignEvent("GetAppVersionInfoServerError");
                        OnEnter(procedureOwner);
                    }
                    else
                    {
                        ResponseGameAppVersion responseGameAppVersion = Utility.Json.ToObject<ResponseGameAppVersion>(httpJsonResult.data);

                        if (responseGameAppVersion.IsUpgrade)
                        {
                            var uiLoadingMainScene = GameApp.UI.Get<UILauncher>(UILauncher.UIResName);
                            uiLoadingMainScene.m_IsUpgrade.SetSelectedIndex(1);

                            bool isChinese = GameApp.Localization.SystemLanguage == Language.ChineseSimplified ||
                                             GameApp.Localization.SystemLanguage == Language.ChineseTraditional;

                            uiLoadingMainScene.m_upgrade.m_EnterButton.title = isChinese ? "确认" : "Enter";
                            uiLoadingMainScene.m_upgrade.m_TextContent.title = responseGameAppVersion.UpdateAnnouncement;
                            uiLoadingMainScene.m_upgrade.m_TextContent.onClickLink.Set((context => { Application.OpenURL(context.data.ToString()); }));
                            uiLoadingMainScene.m_upgrade.m_EnterButton.onClick.Set(() =>
                            {
                                if (responseGameAppVersion.IsForce)
                                {
                                    Application.OpenURL(responseGameAppVersion.AppDownloadUrl);
                                }
                                else
                                {
                                    uiLoadingMainScene.m_IsUpgrade.SetSelectedIndex(0);
                                    ChangeState<ProcedurePatchInit>(procedureOwner);
                                }
                            });
                        }
                        else
                        {
                            ChangeState<ProcedurePatchInit>(procedureOwner);
                        }
                    }
                }
            };
        }
    }
}