using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    public class CollectResult
    {
        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName { private set; get; }

        /// <summary>
        /// 资源包名唯一化
        /// </summary>
        public bool UniqueBundleName { private set; get; }

        /// <summary>
        /// 收集的资源信息列表
        /// </summary>
        public List<CollectAssetInfo> CollectAssets { private set; get; }


        public CollectResult(string packageName, bool uniqueBundleName)
        {
            PackageName = packageName;
            UniqueBundleName = uniqueBundleName;
        }

        public void SetCollectAssets(List<CollectAssetInfo> collectAssets)
        {
            CollectAssets = collectAssets;
        }
    }
}