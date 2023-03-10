namespace YooAsset
{
    public class AssetInfo
    {
        private readonly PatchAsset _patchAsset;
        private string _providerGuid;

        /// <summary>
        /// 唯一标识符
        /// </summary>
        internal string GUID
        {
            get
            {
                if (string.IsNullOrEmpty(_providerGuid) == false)
                    return _providerGuid;

                if (AssetType == null)
                    _providerGuid = $"{AssetPath}[null]";
                else
                    _providerGuid = $"{AssetPath}[{AssetType.Name}]";
                return _providerGuid;
            }
        }

        /// <summary>
        /// 身份是否无效
        /// </summary>
        internal bool IsInvalid => _patchAsset == null;

        /// <summary>
        /// 错误信息
        /// </summary>
        internal string Error { private set; get; }

        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath { private set; get; }

        /// <summary>
        /// 资源类型
        /// </summary>
        public System.Type AssetType { private set; get; }


        // 注意：这是一个内部类，严格限制外部创建。
        private AssetInfo()
        {
        }

        internal AssetInfo(PatchAsset patchAsset, System.Type assetType)
        {
            _patchAsset = patchAsset ?? throw new System.Exception("Should never get here !");
            AssetType = assetType;
            AssetPath = patchAsset.AssetPath;
            Error = string.Empty;
        }

        internal AssetInfo(PatchAsset patchAsset)
        {
            _patchAsset = patchAsset ?? throw new System.Exception("Should never get here !");
            AssetType = null;
            AssetPath = patchAsset.AssetPath;
            Error = string.Empty;
        }

        internal AssetInfo(string error)
        {
            _patchAsset = null;
            AssetType = null;
            AssetPath = string.Empty;
            Error = error;
        }
    }
}