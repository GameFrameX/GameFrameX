using System.Security.Cryptography;
using System.Text;
using Server.Log;
using Server.Setting;

namespace Server.NetWork.HTTP
{
    public abstract class BaseHttpHandler
    {
        /// <summary>
        /// 是否使用内部验证方式
        /// </summary>
        public virtual bool IsCheckSign => false;

        public static string GetStringSign(string str)
        {
            //取md5
            var data = Encoding.UTF8.GetBytes(str);
            byte[] md5Bytes = MD5.Create().ComputeHash(data);
            string md5 = BitConverter.ToString(md5Bytes).Replace("-", "").ToLower();

            int checkCode1 = 0; //校验码
            int checkCode2 = 0;
            foreach (var t in md5)
            {
                if (t >= 'a')
                    checkCode1 += t;
                else
                    checkCode2 += t;
            }

            md5 = checkCode1 + md5 + checkCode2;

            return md5;
        }

        public string CheckSign(Dictionary<string, string> paramMap)
        {
            // if (!IsCheckSign || GlobalSettings.IsDebug)
            if (!IsCheckSign)
            {
                return "";
            }

            //内部验证
            if (!paramMap.ContainsKey("token") || !paramMap.ContainsKey("timestamp"))
            {
                LogHelper.Error("http命令未包含验证参数");
                return HttpResult.Create(HttpStatusCode.Illegal, "http命令未包含验证参数");
            }

            var sign = paramMap["token"];
            var time = paramMap["timestamp"];
            long.TryParse(time, out long timeTick);
            var span = new TimeSpan(Math.Abs(DateTime.Now.Ticks - timeTick));
            if (span.TotalMinutes > 5) //5分钟内有效
            {
                LogHelper.Error("http命令已过期");
                return HttpResult.Create(HttpStatusCode.Illegal, "http命令已过期");
            }

            var str = 21001 + time;
            if (sign == GetStringSign(str))
            {
                return "";
            }
            else
            {
                return HttpResult.Create(HttpStatusCode.Illegal, "命令验证失败");
            }
        }

        public abstract Task<string> Action(string ip, string url, Dictionary<string, string> paramMap);
    }
}