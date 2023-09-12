using System;
using System.Buffers;

namespace UnityGameFramework.Network.Pipelines.Protocols
{
    public interface IProtoCalReadHelper<TMessage>
    {
        /// <summary>
        /// 读取和解析消息
        /// </summary>
        /// <param name="input"></param>
        /// <param name="consumed"></param>
        /// <param name="examined"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out TMessage message);
    }

    public interface IProtoCalWriteHelper<TMessage>
    {
        /// <summary>
        /// 写入消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="output"></param>
        void WriteMessage(TMessage message, IBufferWriter<byte> output);
    }
}