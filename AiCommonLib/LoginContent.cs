using System.Collections.Generic;

namespace AiModel
{
    public class LoginContent
    {
        public string loginStatus { get; set; }
        public string loginType { get; set; }
        public List<string> contracts { get; set; }
        public string session_id { get; set; }
        public string openid { get; set; }
    }
}