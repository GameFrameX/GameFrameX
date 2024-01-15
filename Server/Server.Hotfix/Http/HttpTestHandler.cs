using Server.NetWork.HTTP;

namespace Server.Hotfix.Http
{
    public class HttpTestRes
    {
        public class Info
        {
            public int Age { get; set; }
            public string Name { get; set; }
        }

        public int A { get; set; }
        public string B { get; set; }

        public Info TestInfo { get; set; }
    }


    /// <summary>
    /// 测试
    /// http://localhost:20001/game/api?command=test
    /// </summary>
    [HttpMsgMapping(typeof(HttpTestHandler))]
    public class HttpTestHandler : BaseHttpHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override Task<string> Action(string ip, string url, Dictionary<string, string> parameters)
        {
            var response = new HttpTestRes
            {
                A = 100,
                B = "hello",
                TestInfo = new HttpTestRes.Info()
            };
            response.TestInfo.Age = 18;
            response.TestInfo.Name = "leeveel";
            return Task.FromResult(HttpResult.Create(response));
        }
    }
}