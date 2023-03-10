namespace YooAsset.Editor
{
    public class CollectCommand
    {
        /// <summary>
        /// 构建模式
        /// </summary>
        public EBuildMode BuildMode { private set; get; }


        public CollectCommand(EBuildMode buildMode)
        {
            BuildMode = buildMode;
        }
    }
}