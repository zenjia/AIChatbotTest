namespace AiModel
{
    public class AuthRequestInfo
    {
        
        public class Data
        {
            public string openid { get; internal set; }
            public string session_id { get; internal set; }
            public string reg_no { get; internal set; }
        }

        public string msg_id { get; }
        public string @event { get; } = "399:1";
        public Data data { get; }

        public AuthRequestInfo(string open_id, string session_id, string reg_no)
        {
            this.data = new Data();
            this.data.openid = open_id;
            this.data.session_id = session_id;
            this.data.reg_no = reg_no;
            this.msg_id = string.Format("{0}-331b-4ca6-89c2-5a12ca73b003", ThreadSafeRandom.Next(10000000, 99999999));
        }
    }
}