using System;
using System.Collections.Generic;
using System.Reflection;
using Protocol;
using UnityGameFramework.Runtime;

namespace Net
{
    public class HotfixMessageIdHandler
    {
        private static readonly BidirectionalDictionary<int, Type> ReqDictionary = new BidirectionalDictionary<int, Type>();
        private static readonly BidirectionalDictionary<int, Type> RespDictionary = new BidirectionalDictionary<int, Type>();

        public static Type GetReqTypeById(int messageId)
        {
            ReqDictionary.TryGetValue(messageId, out var value);
            return value;
        }

        public static int GetReqMessageIdByType(Type type)
        {
            ReqDictionary.TryGetKey(type, out var value);
            return value;
        }

        public static Type GetRespTypeById(int messageId)
        {
            RespDictionary.TryGetValue(messageId, out var value);
            return value;
        }

        public static int GetRespMessageIdByType(Type type)
        {
            RespDictionary.TryGetKey(type, out var value);
            return value;
        }

        public static void Init(Assembly assembly)
        {
            ReqDictionary.Clear();
            RespDictionary.Clear();
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute(typeof(MessageTypeHandler));
                if (attribute != null)
                {
                    Log.Info(" 注册消息ID类型: " + type);

                    if (attribute is MessageTypeHandler messageIdHandler)
                    {
                        if (type.IsImplWithInterface(typeof(IRequestMessage)))
                        {
                            // 请求
                            if (!ReqDictionary.TryAdd(messageIdHandler.MessageId, type))
                            {
                                ReqDictionary.TryGetValue(messageIdHandler.MessageId, out var value);
                                Log.Error($"请求Id重复==>当前ID:{messageIdHandler.MessageId},已有ID类型:{value.FullName}");
                            }
                        }
                        else if (type.IsImplWithInterface(typeof(IResponseMessage)))
                        {
                            // 返回
                            if (!RespDictionary.TryAdd(messageIdHandler.MessageId, type))
                            {
                                RespDictionary.TryGetValue(messageIdHandler.MessageId, out var value);
                                Log.Error($"返回Id重复==>当前ID:{messageIdHandler.MessageId},已有ID类型:{value.FullName}");
                            }
                        }
                    }
                }
            }

            Log.Info(" 注册消息ID类型: 结束");
        }
    }
}