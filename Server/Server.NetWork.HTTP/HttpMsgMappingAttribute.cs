using Server.Extension;

namespace Server.NetWork.HTTP
{
    /// <summary>
    /// Http 消息处理器
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class HttpMsgMappingAttribute : Attribute
    {
        /// <summary>
        /// 原始命令
        /// </summary>
        public string OriginalCmd { get; }

        /// <summary>
        /// 标准化的命令
        /// </summary>
        public string StandardCmd { get; }

        /// <summary>
        /// 处理器命名前缀
        /// </summary>
        public const string HTTPprefix = "Http";

        /// <summary>
        /// 处理器命名后缀
        /// </summary>
        public const string HTTPsuffix = "Handler";

        public HttpMsgMappingAttribute(Type classType)
        {
            if (classType == null)
            {
                throw new ArgumentNullException(nameof(classType));
            }

            var className = classType.Name;

            if (!className.StartsWith(HTTPprefix, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("HttpMsgMapping 必须以Http开头");
            }

            if (!className.EndsWith(HTTPsuffix, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("HttpMsgMapping 必须以Handler结尾");
            }

            OriginalCmd = className.Substring(HTTPprefix.Length, className.Length - HTTPprefix.Length - HTTPsuffix.Length);
            StandardCmd = OriginalCmd.ConvertToSnakeCase();
        }
    }
}