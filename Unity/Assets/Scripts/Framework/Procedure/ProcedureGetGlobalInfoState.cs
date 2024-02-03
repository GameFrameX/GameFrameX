using System.Text;
using Cysharp.Threading.Tasks;
using GameFrameX.Fsm;
using GameFrameX.Runtime;
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;

namespace GameFrameX.Procedure
{
    /// <summary>
    /// 获取全局信息
    /// </summary>
    public sealed class ProcedureGetGlobalInfoState : ProcedureBase
    {
        private UnityWebRequest www;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            // 编辑器下的模拟模式
            if (GameEntry.GetComponent<AssetComponent>().GamePlayMode == EPlayMode.EditorSimulateMode)
            {
                Debug.Log("当前为编辑器模式，直接启动 FsmGetGlobalInfoState");
                ChangeState<ProcedureGetAppVersionInfoState>(procedureOwner);
                return;
            }

            string rootUrl = "http://172.18.0.31:20808/api/GameGlobalInfo/GetInfo";


            www = UnityWebRequest.Post(rootUrl, string.Empty);
            string jsonParams = Utility.Json.ToJson(HttpHelper.GetBaseParams());
            Log.Info(jsonParams);
            www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonParams));
            www.uploadHandler.contentType = "application/json; charset=utf-8";
            www.timeout = 5;
            var async = www.SendWebRequest();
            async.completed += async (AsyncOperation async2) =>
            {
                var json = www.downloadHandler.text;
                Debug.Log(json);
                if (!string.IsNullOrEmpty(www.error) || string.IsNullOrEmpty(json))
                {
                    //todo 提示用户
                    // GameApp.EventSystem.Run(EventIdType.UILoadingMainSetText, "Network error, retrying...");
                    LauncherUIHandler.SetTipText("Network error, retrying...");
                    Debug.LogError($"获取全局信息异常=>Error:{www.error}   Req:{jsonParams}");
                    // GAHelper.DesignEvent("GetGlobalInfoNetworkError");
                    OnEnter(procedureOwner);
                }
                else
                {
                    HttpJsonResult httpJsonResult = Utility.Json.ToObject<HttpJsonResult>(json);
                    if (httpJsonResult.Code > 0)
                    {
                        // GameApp.EventSystem.Run(EventIdType.UILoadingMainSetText, "Server error, retrying...");
                        LauncherUIHandler.SetTipText("Server error, retrying...");
                        Debug.LogError($"获取全局信息返回异常=> Req:{jsonParams} Resp:{json}");

                        await UniTask.Delay(3000);
                        // GAHelper.DesignEvent("GetGlobalInfoServerError");
                        OnEnter(procedureOwner);
                    }
                    else
                    {
                        ResponseGlobalInfo responseGlobalInfo = Utility.Json.ToObject<ResponseGlobalInfo>(httpJsonResult.Data);
                        GlobalConfigComponent globalConfigComponent = GameEntry.GetComponent<GlobalConfigComponent>();
                        globalConfigComponent.CheckAppVersionUrl = responseGlobalInfo.CheckAppVersionUrl;
                        globalConfigComponent.CheckResourceVersionUrl = responseGlobalInfo.CheckResourceVersionUrl;
                        globalConfigComponent.Content = responseGlobalInfo.Content;

                        globalConfigComponent.HostServerUrl = responseGlobalInfo.CheckResourceVersionUrl;
                        // Game.EventSystem.Run(EventIdType.UILoadingMainSetText, "Loading...");
                        LauncherUIHandler.SetTipText("Loading...");
                        ChangeState<ProcedureGetAppVersionInfoState>(procedureOwner);
                    }
                }
            };
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            www?.Dispose();
        }
    }
}