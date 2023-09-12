using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Network;
using UnityEngine;
using UnityGameFramework.Network.Pipelines.Protocols;
using AddressFamily = System.Net.Sockets.AddressFamily;

namespace UnityGameFramework.Network.Pipelines
{
    public class NetManager
    {
        private const float DISPATCH_MAX_TIME = 0.06f; //每一帧最大的派发事件时间，超过这个时间则停止派发，等到下一帧再派发
        public const int ConnectEvt = 101; //连接成功
        public const int DisconnectEvt = 102; //连接断开
        public static readonly NetManager Singleton = new NetManager();
        private NetChannel channel { get; set; }
        ConcurrentQueue<NetMessage> msgQueue = new ConcurrentQueue<NetMessage>();
        public int Port { private set; get; }
        public string Host { private set; get; }
        IProtoCalWriteHelper<MessageObject> _protoCalWriteHelper;
        IProtoCalReadHelper<MessageObject> _protoCalReadHelper;

        public void Init()
        {
            _protoCalWriteHelper = new ClientProtocolWriteHelper();
            _protoCalReadHelper = new ClientProtocolReadHelper();
        }

        public void Send(MessageObject msg)
        {
            channel?.Write(msg);
        }

        public async void Connect(string host, int port, int timeOut = 5000)
        {
            Host = host;
            Port = port;
            try
            {
                ClearAllMsg();
                var ipType = AddressFamily.InterNetwork;
                (ipType, host) = Utility.Net.GetIPv6Address(host, port);

                var context = await new SocketConnection(ipType, host, port).StartAsync(timeOut);

                if (context != null)
                {
                    Debug.Log($"Connected to {context.LocalEndPoint}");
                    // OnConnected(NetCode.Success);
                    // channel = new NetChannel(context, _protocol, OnRevice, OnDisConnected);
                    channel = new NetChannel(context, _protoCalWriteHelper, _protoCalReadHelper, OnRevice, () => { });
                    _ = channel.StartReadMsgAsync();
                }
                else
                {
                    // OnConnected(NetCode.Fail);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                // OnConnected(NetCode.Fail);
            }
        }


        /*private void OnConnected(NetCode code)
        {
            var rMsg = new NetMessage();
            rMsg.MsgId = ConnectEvt;
            rMsg.Msg = code;
            msgQueue.Enqueue(rMsg);
        }

        public void OnDisConnected()
        {
            var rMsg = new NetMessage();
            rMsg.MsgId = DisconnectEvt;
            rMsg.Msg = NetCode.Closed;
            msgQueue.Enqueue(rMsg);
        }*/

        public void OnRevice(MessageObject msg)
        {
            msgQueue.Enqueue(new NetMessage(msg));
        }

        public void Close(bool triggerCloseEvt)
        {
            channel?.Abort(triggerCloseEvt);
            channel = null;
            ClearAllMsg();
        }

        public void ClearAllMsg()
        {
            msgQueue = new ConcurrentQueue<NetMessage>();
        }


        public void Update(float maxTime = DISPATCH_MAX_TIME)
        {
            float curTime = UnityEngine.Time.realtimeSinceStartup;
            float endTime = curTime + maxTime;
            while (curTime < endTime)
            {
                if (msgQueue.IsEmpty)
                    return;

                if (!msgQueue.TryDequeue(out var msg))
                    break;

                if (msg == null)
                    return;

#if UNITY_EDITOR
                var innerMsg = ((NetMessage)msg).Msg;
                var msgName = innerMsg != null ? innerMsg.GetType().FullName : "";
                Debug.Log($"开始处理网络事件 {msg.MsgId}  {msgName}");
#endif

                try
                {
                    // evt.dispatchEvent(msg.MsgId, msg.Msg);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }

                curTime = UnityEngine.Time.realtimeSinceStartup;
                if (!ignoreCodeList.Contains(msg.MsgId))
                    ResCode++;
            }
        }

        /// <summary>上次接收消失时间</summary>
        public float HandMsgTime { get; private set; }

        /// <summary>收到的消息计数 和服务器对不上则应该断线重连</summary>
        public int ResCode { get; private set; }

        List<int> ignoreCodeList = new List<int>();

        public void ResetResCode(int code = 0)
        {
            ResCode = code;
        }

        /// <summary>
        /// 心跳等无关逻辑的消息可忽略
        /// </summary>
        public void AddIgnoreCode(int msgId)
        {
            if (!ignoreCodeList.Contains(msgId))
                ignoreCodeList.Add(msgId);
        }
    }
}