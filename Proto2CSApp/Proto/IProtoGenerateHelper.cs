namespace Proto2CS.Editor
{
    /// <summary>
    /// 协议生成帮助基类
    /// </summary>
    internal interface IProtoGenerateHelper
    {
        /// <summary>
        /// 开始生成
        /// </summary>
        /// <param name="inputPath">输入目录</param>
        /// <param name="outputPath">输出目录</param>
        /// <param name="namespaceName">命名空间</param>
        /// <param name="isServer">是否是生成服务器</param>
        void Run(string inputPath, string outputPath, string namespaceName = "GFXHotfix", bool isServer = false);
    }
}