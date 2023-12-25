using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Server.NetWork.HTTP
{
    public class HttpResult
    {
        public static readonly HttpResult Success = new HttpResult(HttpStatusCode.Success, "ok");
        public static readonly HttpResult Undefine = new HttpResult(HttpStatusCode.Undefine, "undefine command");

        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            //WriteIndented = true
        };

        public static HttpResult CreateOk(string retMsg = "")
        {
            return new HttpResult(HttpStatusCode.Success, retMsg);
        }

        public static HttpResult CreateErrorParam(string retMsg = "")
        {
            return new HttpResult(HttpStatusCode.ParamErr, retMsg);
        }

        public static HttpResult CreateActionFailed(string retMsg = "")
        {
            return new HttpResult(HttpStatusCode.ActionFailed, retMsg);
        }

        public string Code { get; set; }
        public string Msg { get; set; }
        public Dictionary<string, string> ExtraMap { get; set; }

        public HttpResult(HttpStatusCode retCode = HttpStatusCode.Success, string retMsg = "ok")
        {
            Code = retCode.ToString();
            Msg = retMsg;
        }

        public string Get(string key)
        {
            if (ExtraMap == null)
                return null;
            ExtraMap.TryGetValue(key, out var res);
            return res;
        }

        public void Set(string key, string value)
        {
            if (ExtraMap == null)
                ExtraMap = new Dictionary<string, string>();
            ExtraMap[key] = value;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, GetType(), options);
        }

        public static implicit operator string(HttpResult value)
        {
            return value.ToString();
        }
    }
}