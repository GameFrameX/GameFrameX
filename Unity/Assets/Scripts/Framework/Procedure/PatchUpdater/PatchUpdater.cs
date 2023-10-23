using System;
using UnityEngine;
using YooAsset;

namespace GameFrameX.Procedure
{
    public static class PatchUpdater
    {
        private static bool _isRun = false;

        /// <summary>
        /// 下载器
        /// </summary>
        public static PatchDownloaderOperation Downloader { set; get; }

        /// <summary>
        /// 资源版本
        /// </summary>
        public static long ResourceVersion { set; get; }

        /// <summary>
        /// 开启初始化流程
        /// </summary>
        public static void Run()
        {
            if (_isRun == false)
            {
                _isRun = true;

                Debug.Log("开始补丁更新...");

                // 注意：按照先后顺序添加流程节点
                // FsmComponent.Instance.AddNode(new FsmPatchInit());
                // FsmComponent.Instance.AddNode(new FsmUpdateStaticVersion());
                // FsmComponent.Instance.AddNode(new FsmUpdateManifest());
                // FsmComponent.Instance.AddNode(new FsmCreateDownloader());
                // FsmComponent.Instance.AddNode(new FsmDownloadWebFiles());
                // FsmComponent.Instance.AddNode(new FsmPatchDone());
                // FsmComponent.Instance.Transition(nameof (FsmPatchInit));
            }
            else
            {
                Debug.LogWarning("补丁更新已经正在进行中!");
            }
        }

        /// <summary>
        /// 处理请求操作
        /// </summary>
        public static void HandleOperation(EPatchOperation operation)
        {
            if (operation == EPatchOperation.BeginDownloadWebFiles)
            {
                // FsmComponent.Instance.Transition(nameof (FsmDownloadWebFiles));
            }
            else if (operation == EPatchOperation.TryUpdateStaticVersion)
            {
                // FsmComponent.Instance.Transition(nameof (FsmUpdateStaticVersion));
            }
            else if (operation == EPatchOperation.TryUpdatePatchManifest)
            {
                // FsmComponent.Instance.Transition(nameof (FsmUpdateManifest));
            }
            else if (operation == EPatchOperation.TryDownloadWebFiles)
            {
                // FsmComponent.Instance.Transition(nameof (FsmCreateDownloader));
            }
            else
            {
                throw new NotImplementedException($"{operation}");
            }
        }
    }
}