using System.IO;

namespace GameFrameX.Network
{
    public sealed class DefaultPacketSendBodyHandler : IPacketSendBodyHandler, IPacketHandler
    {
        public bool Handler(byte[] messageBodyBuffer, MemoryStream cachedStream, Stream destination)
        {
            cachedStream.Write(messageBodyBuffer, 0, messageBodyBuffer.Length);
            cachedStream.WriteTo(destination);
            return true;
        }
    }
}