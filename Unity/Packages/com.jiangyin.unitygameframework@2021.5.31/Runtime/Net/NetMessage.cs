using GameFramework.Network;

namespace Base.Net
{
    /// <summary>
    /// net message
    /// </summary>
    public class NetMessage
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public int MsgId { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public object Msg { get; set; }

        public NetMessage()
        {
        }

        public NetMessage(MessageObject msg)
        {
            this.Msg = msg;
        }

        public byte[] Serialize()
        {
            try
            {
                return null;
                // var data = MessagePack.MessagePackSerializer.Serialize(Msg);
                // return data;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
                throw;
            }
        }
    }
}