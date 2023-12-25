namespace Server.Core.Net.Http
{
    /// <summary>
    /// Http 消息
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class HttpMsgMapping : Attribute
    {
        public string Cmd { get; }

        public HttpMsgMapping(string cmd)
        {
            this.Cmd = cmd;
        }
    }
}