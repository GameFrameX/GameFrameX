using GameFrameX.Runtime;
using YooAsset;

namespace GameFrameX.Asset
{
    public partial class AssetManager
    {
        private class RemoteServices : IRemoteServices
        {
            public string HostServer { get; }
            public string FallbackHostServer { get; }

            public RemoteServices(string hostServer, string fallbackHostServer)
            {
                HostServer = hostServer;
                FallbackHostServer = fallbackHostServer;
            }

            public string GetRemoteMainURL(string fileName)
            {
                return HostServer + fileName;
            }

            public string GetRemoteFallbackURL(string fileName)
            {
                return FallbackHostServer + fileName;
            }
        }

        /// <summary>
        /// 内置文件查询服务类
        /// </summary>
        private class QueryStreamingAssetsFileServices : IBuildinQueryServices
        {
            public bool Query(string packageName, string fileName)
            {
                // 注意：使用了BetterStreamingAssets插件，使用前需要初始化该插件！
                string buildinFolderName = PathHelper.AppResPath;
                return BetterStreamingAssets.FileExists($"{buildinFolderName}/{fileName}");
            }
        }
    }
}