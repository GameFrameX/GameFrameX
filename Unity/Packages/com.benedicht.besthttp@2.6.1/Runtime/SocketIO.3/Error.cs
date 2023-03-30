#if !BESTHTTP_DISABLE_SOCKETIO

namespace BestHTTP.SocketIO3
{
    public class Error
    {
        public string message;

        public Error() { }

        public Error(string msg)
        {
            this.message = msg;
        }

        public override string ToString()
        {
            return this.message;
        }
    }
}

#endif
