#if !BESTHTTP_DISABLE_WEBSOCKET && (!UNITY_WEBGL || UNITY_EDITOR)

using BestHTTP.WebSocket.Frames;

namespace BestHTTP.WebSocket.Extensions
{
    public interface IExtension
    {
        /// <summary>
        /// This is the first pass: here we can add headers to the request to initiate an extension negotiation.
        /// </summary>
        /// <param name="request"></param>
        void AddNegotiation(HTTPRequest request);

        /// <summary>
        /// If the websocket upgrade succeded it will call this function to be able to parse the server's negotiation
        /// response. Inside this function the IsEnabled should be set.
        /// </summary>
        bool ParseNegotiation(WebSocketResponse resp);

        /// <summary>
        /// This function should return a new header flag based on the inFlag parameter. The extension should set only the
        /// Rsv1-3 bits in the header.
        /// </summary>
        byte GetFrameHeader(WebSocketFrame writer, byte inFlag);

        /// <summary>
        /// This function will be called to be able to transform the data that will be sent to the server.
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        byte[] Encode(WebSocketFrame writer);

        /// <summary>
        /// This function can be used the decode the server-sent data.
        /// </summary>
        byte[] Decode(byte header, byte[] data, int length);
    }
}

#endif