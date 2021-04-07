using System;

namespace AiModel
{
    public class OutGoingMessage
    {
        public class Data
        {
            public string msg { get; set; }
            public int ref_num { get; set; }
        }

        public string msg_id { get; }
        public string @event { get; } = "501:0";
        public string rpc_id { get; }
        public Data data { get; }

        public OutGoingMessage(string msg)
        {
            this.data = new Data();
            this.data.msg = msg;
            this.data.ref_num = DateTimeOffset.UtcNow.Millisecond;

            this.msg_id = string.Format("{0}-f5f9-4edc-8d43-3ed5b1ac7417", ThreadSafeRandom.Next(10000000, 99999999));
            this.rpc_id = string.Format("{0}-0792-4bbb-a71f-529c5f6e7c76", ThreadSafeRandom.Next(10000000, 99999999));
        }
    }
}