#if !BESTHTTP_DISABLE_WEBSOCKET && (!UNITY_WEBGL || UNITY_EDITOR)

namespace BestHTTP.WebSocket.Frames
{
    public enum WebSocketFrameTypes : byte
    {
        /// <summary>
        /// A fragmented message's first frame's contain the type of the message(binary or text), all consecutive frame of that message must be a Continuation frame.
        /// Last of these frame's Fin bit must be 1.
        /// </summary>
        /// <example>For a text message sent as three fragments, the first fragment would have an opcode of 0x1 (text) and a FIN bit clear,
        /// the second fragment would have an opcode of 0x0 (Continuation) and a FIN bit clear,
        /// and the third fragment would have an opcode of 0x0 (Continuation) and a FIN bit that is set.</example>
        Continuation        = 0x0,
        Text                = 0x1,
        Binary              = 0x2,
        //Reserved1         = 0x3,
        //Reserved2         = 0x4,
        //Reserved3         = 0x5,
        //Reserved4         = 0x6,
        //Reserved5         = 0x7,

        /// <summary>
        /// The Close frame MAY contain a body (the "Application data" portion of the frame) that indicates a reason for closing,
        /// such as an endpoint shutting down, an endpoint having received a frame too large, or an endpoint having received a frame that
        /// does not conform to the format expected by the endpoint.
        /// As the data is not guaranteed to be human readable, clients MUST NOT show it to end users.
        /// </summary>
        ConnectionClose     = 0x8,

        /// <summary>
        /// The Ping frame contains an opcode of 0x9. A Ping frame MAY include "Application data".
        /// </summary>
        Ping                = 0x9,

        /// <summary>
        /// A Pong frame sent in response to a Ping frame must have identical "Application data" as found in the message body of the Ping frame being replied to.
        /// </summary>
        Pong                = 0xA,
        //Reserved6         = 0xB,
        //Reserved7         = 0xC,
        //Reserved8         = 0xD,
        //Reserved9         = 0xE,
        //Reserved10        = 0xF,
    }
}

#endif