namespace AiModel
{
    public class HttpLoginInfo
    {
        public string apiToken { get; } = "";
        public string function_code { get; } = "login";
        public string identify { get; }
        public string identify_code { get; } = "oss";
        public string reserve_param { get; } = "";
        public string success { get; } = "true";
        public string openid { get; }

        public HttpLoginInfo(string _identify, string _openid)
        {
            this.identify = _identify;
            this.openid = _openid;
        }

    }
}