using System;

namespace GameFrameX.Network
{
#if ENABLE_MESSAGEPACK
    /// <summary>
    /// 消息对象标签
    /// </summary>
    public class MessagePackageObjectAttribute : MessagePack.MessagePackObjectAttribute
    {
        public MessagePackageObjectAttribute() : base(true)
        {
        }
    }
#elif ENABLE_PROTOBUF
    /// <summary>
    /// 消息对象标签
    /// </summary>
    public class MessagePackageObjectAttribute : Attribute
    {
        public MessagePackageObjectAttribute() : base()
        {
        }
    }
#else
    /// <summary>
    /// 消息对象标签
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class MessagePackageObjectAttribute
    {
        public MessagePackageObjectAttribute() { }
    }
#endif
}