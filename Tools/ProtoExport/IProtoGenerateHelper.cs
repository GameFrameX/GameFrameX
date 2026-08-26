namespace GameFrameX.ProtoExport
{
    /// <summary>
    /// 协议生成帮助基类
    /// </summary>
    internal interface IProtoGenerateHelper
    {
        /// <summary>
        /// 初始化配置
        /// </summary>
        /// <param name="launcherOptions">启动时配置选项</param>
        void Init(LauncherOptions launcherOptions);

        /// <summary>
        /// 开始生成协议代码
        /// </summary>
        /// <param name="inputPath">包含协议定义的 MessageInfoList 实例</param>
        /// <param name="outputPath">生成代码文件的输出目录</param>
        /// <param name="namespaceName">生成代码所使用的命名空间，默认值为 "GFXHotfix"</param>
        void Run(MessageInfoList inputPath, string outputPath, string namespaceName = "GFXHotfix");

        /// <summary>
        /// 协议生成后的统一后处理逻辑
        /// </summary>
        /// <param name="operationCodeInfo">待处理的协议信息列表</param>
        /// <param name="launcherOptions">启动时配置选项</param>
        void Post(List<MessageInfoList> operationCodeInfo, LauncherOptions launcherOptions);
    }
}
