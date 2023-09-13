using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace GameFramework.Network
{
    /// <summary>
    /// 协议消息处理器
    /// </summary>
    public static class ProtoMessageIdHandler
    {
        private static readonly BidirectionalDictionary<int, Type> ReqDictionary = new BidirectionalDictionary<int, Type>();
        private static readonly BidirectionalDictionary<int, Type> RespDictionary = new BidirectionalDictionary<int, Type>();

        /// <summary>
        /// 根据消息ID获取请求的类型
        /// </summary>
        /// <param name="messageId">消息ID</param>
        /// <returns>请求的类型</returns>
        public static Type GetReqTypeById(int messageId)
        {
            ReqDictionary.TryGetValue(messageId, out var value);
            return value;
        }

        /// <summary>
        /// 根据类型获取请求消息ID
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>请求消息ID</returns>
        public static int GetReqMessageIdByType(Type type)
        {
            ReqDictionary.TryGetKey(type, out var value);
            return value;
        }

        /// <summary>
        /// 根据消息ID获取响应的类型
        /// </summary>
        /// <param name="messageId">消息ID</param>
        /// <returns>响应的类型</returns>
        public static Type GetRespTypeById(int messageId)
        {
            RespDictionary.TryGetValue(messageId, out var value);
            return value;
        }

        /// <summary>
        /// 根据类型获取响应消息ID
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>响应消息ID</returns>
        public static int GetRespMessageIdByType(Type type)
        {
            RespDictionary.TryGetKey(type, out var value);
            return value;
        }

        /// <summary>
        /// 初始化所有协议对象
        /// </summary>
        public static void Init(Assembly assembly)
        {
            ReqDictionary.Clear();
            RespDictionary.Clear();
            var types = assembly.GetTypes();
            StringBuilder stringBuilder = new StringBuilder(1024);
            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute(typeof(MessageTypeHandlerAttribute));
                if (attribute == null)
                {
                    continue;
                }

                stringBuilder.AppendLine(type.FullName);

                if (attribute is MessageTypeHandlerAttribute messageIdHandler)
                {
                    if (type.IsImplWithInterface(typeof(IRequestMessage)))
                    {
                        // 请求
                        if (!ReqDictionary.TryAdd(messageIdHandler.MessageId, type))
                        {
                            ReqDictionary.TryGetValue(messageIdHandler.MessageId, out var value);
                            throw new GameFrameworkException($"请求Id重复==>当前ID:{messageIdHandler.MessageId},已有ID类型:{value.FullName}");
                        }
                    }
                    else if (type.IsImplWithInterface(typeof(IResponseMessage)))
                    {
                        // 返回
                        if (!RespDictionary.TryAdd(messageIdHandler.MessageId, type))
                        {
                            RespDictionary.TryGetValue(messageIdHandler.MessageId, out var value);
                            throw new GameFrameworkException($"返回Id重复==>当前ID:{messageIdHandler.MessageId},已有ID类型:{value.FullName}");
                        }
                    }
                }
            }

            GameFrameworkLog.Debug(" 注册消息ID类型: " + stringBuilder);
            GameFrameworkLog.Info(" 注册消息ID类型: 结束");
        }
    }
}