#if !BESTHTTP_DISABLE_SOCKETIO

namespace BestHTTP.SocketIO
{
    public sealed class Error
    {
        public SocketIOErrors Code { get; private set; }
        public string Message { get; private set; }

        public Error(SocketIOErrors code, string msg)
        {
            this.Code = code;
            this.Message = msg;
        }

        public override string ToString()
        {
            return string.Format("Code: {0} Message: \"{1}\"", this.Code.ToString(), this.Message);
        }
    }
}

#endif