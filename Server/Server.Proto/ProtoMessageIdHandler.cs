using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Server.Extension;
using Server.Log;
using Server.NetWork.Messages;
using Server.Serialize.Serialize;

namespace Server.Proto
{
    /// <summary>
    /// 协议消息处理器
    /// </summary>
    public static class ProtoMessageIdHandler
    {
        private static readonly BidirectionalDictionary<int, Type> ReqDictionary = new BidirectionalDictionary<int, Type>();
        private static readonly BidirectionalDictionary<int, Type> RespDictionary = new BidirectionalDictionary<int, Type>();
        private static readonly BidirectionalDictionary<int, Type> RequestActorDictionary = new BidirectionalDictionary<int, Type>();
        private static readonly BidirectionalDictionary<int, Type> ResponseActorDictionary = new BidirectionalDictionary<int, Type>();

        #region Actor

        /// <summary>
        /// 根据消息ID获取请求的类型
        /// </summary>
        /// <param name="messageId">消息ID</param>
        /// <returns>请求的类型</returns>
        public static Type GetRequestActorTypeById(int messageId)
        {
            RequestActorDictionary.TryGetValue(messageId, out var value);
            return value;
        }

        /// <summary>
        /// 根据类型获取请求消息ID
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>请求消息ID</returns>
        public static int GetRequestActorMessageIdByType(Type type)
        {
            RequestActorDictionary.TryGetKey(type, out var value);
            return value;
        }


        /// <summary>
        /// 根据消息ID获取响应的类型
        /// </summary>
        /// <param name="messageId">消息ID</param>
        /// <returns>响应的类型</returns>
        public static Type GetResponseActorTypeById(int messageId)
        {
            ResponseActorDictionary.TryGetValue(messageId, out var value);
            return value;
        }

        /// <summary>
        /// 根据类型获取响应消息ID
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>响应消息ID</returns>
        public static int GetResponseActorMessageIdByType(Type type)
        {
            ResponseActorDictionary.TryGetKey(type, out var value);
            return value;
        }

        #endregion


        public static int GetMessageIdByType(Type type)
        {
            if (ReqDictionary.TryGetKey(type, out var value))
            {
                return value;
            }

            if (RespDictionary.TryGetKey(type, out value))
            {
                return value;
            }

            return 0;
        }


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
        public static void Init()
        {
            ReqDictionary.Clear();
            RespDictionary.Clear();
            var assembly = typeof(ProtoMessageIdHandler).Assembly;
            var types = assembly.GetTypes();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute(typeof(MessageTypeHandlerAttribute));
                if (attribute == null)
                {
                    continue;
                }


                // SerializerHelper.Register(type);
                if (attribute is MessageTypeHandlerAttribute messageIdHandler)
                {
                    stringBuilder.AppendLine($"ID:{messageIdHandler.MessageId},类型: {type}");
                    if (type.IsImplWithInterface(typeof(IRequestMessage)))
                    {
                        // 请求
                        if (type.IsImplWithInterface(typeof(IActorRequestMessage)) && messageIdHandler.MessageId > 100_0000)
                        {
                            if (!RequestActorDictionary.TryAdd(messageIdHandler.MessageId, type))
                            {
                                RequestActorDictionary.TryGetValue(messageIdHandler.MessageId, out var value);
                                LogHelper.Error($"请求Id重复==>当前ID:{messageIdHandler.MessageId},已有ID类型:{value.FullName}");
                            }
                        }
                        else
                        {
                            if (!ReqDictionary.TryAdd(messageIdHandler.MessageId, type))
                            {
                                ReqDictionary.TryGetValue(messageIdHandler.MessageId, out var value);
                                LogHelper.Error($"请求Id重复==>当前ID:{messageIdHandler.MessageId},已有ID类型:{value.FullName}");
                            }
                        }
                    }
                    else if (type.IsImplWithInterface(typeof(IResponseMessage)))
                    {
                        // 返回
                        if (type.IsImplWithInterface(typeof(IActorResponseMessage)) && messageIdHandler.MessageId > 100_0000)
                        {
                            if (!ResponseActorDictionary.TryAdd(messageIdHandler.MessageId, type))
                            {
                                ResponseActorDictionary.TryGetValue(messageIdHandler.MessageId, out var value);
                                LogHelper.Error($"返回Id重复==>当前ID:{messageIdHandler.MessageId},已有ID类型:{value.FullName}");
                            }
                        }
                        else
                        {
                            if (!RespDictionary.TryAdd(messageIdHandler.MessageId, type))
                            {
                                RespDictionary.TryGetValue(messageIdHandler.MessageId, out var value);
                                LogHelper.Error($"返回Id重复==>当前ID:{messageIdHandler.MessageId},已有ID类型:{value.FullName}");
                            }
                        }
                    }
                }
            }

            LogHelper.Debug(stringBuilder.ToString());
            LogHelper.Info(" 注册消息ID类型: 结束");
        }
    }
}