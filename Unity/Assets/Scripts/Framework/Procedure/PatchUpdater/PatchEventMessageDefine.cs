namespace UnityGameFramework.Procedure
{
    public partial class EventIdType
    {
        public const string YooAssetPatchStatesChange = "YooAssetPatchStatesChange";
        public const string YooAssetFoundUpdateFiles = "YooAssetFoundUpdateFiles";
        public const string YooAssetDownloadProgressUpdate = "YooAssetDownloadProgressUpdate";
        public const string YooAssetStaticVersionUpdateFailed = "YooAssetStaticVersionUpdateFailed";
        public const string YooAssetPatchManifestUpdateFailed = "YooAssetPatchManifestUpdateFailed";

        public const string YooAssetWebFileDownloadFailed = "YooAssetWebFileDownloadFailed";
        // public const string YooAssetPatchStatesChange = "PatchStatesChange";
    }

    public class PatchEventMessageDefine
    {
        /// <summary>
        /// 补丁流程步骤改变
        /// </summary>
        public class PatchStatesChange
        {
            public EPatchStates CurrentStates;
        }

        /// <summary>
        /// 发现更新文件
        /// </summary>
        public class FoundUpdateFiles
        {
            public int TotalCount;
            public long TotalSizeBytes;
        }

        /// <summary>
        /// 下载进度更新
        /// </summary>
        public class DownloadProgressUpdate
        {
            public int TotalDownloadCount;
            public int CurrentDownloadCount;
            public long TotalDownloadSizeBytes;
            public long CurrentDownloadSizeBytes;
        }

        /// <summary>
        /// 资源版本号更新失败
        /// </summary>
        public class StaticVersionUpdateFailed
        {
        }

        /// <summary>
        /// 补丁清单更新失败
        /// </summary>
        public class PatchManifestUpdateFailed
        {
        }

        /// <summary>
        /// 网络文件下载失败
        /// </summary>
        public class WebFileDownloadFailed
        {
            public string FileName;
            public string Error;
        }
    }
}