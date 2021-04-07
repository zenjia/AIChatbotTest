using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Timer = System.Timers.Timer;

namespace AiTest
{
    public class WechatAppThreadGroup : TestThreadGroup, ITestThreadGroup
    { 
        private readonly HttpClient _httpClient = new HttpClient();

        public WechaptAppTestgroupConfig Config
        {
            get { return (WechaptAppTestgroupConfig) this.Settings; }
        }
         
        public HttpClient GetHttpClient()
        {
            return this._httpClient;
        }


        public int GetActiveThreadCount()
        {
            return this.ActiveCount;
        }
         

        public int GetChatRoundCount()
        {
            return this.Config.ChatRoundCount;
        }

        public int GetChatDelay()
        {
            return this.Config.ChatDelay;
        }
         

        public int GetReadTimeOut()
        {
            return this.Config.ReadTimeOut;
        }


        public WechatAppThreadGroup(ITestThreadGroupOwner owner)
        :base(owner)
        {
            this._httpClient.DefaultRequestHeaders.Add("Host", "mlp-int.bmwgroup.com");
            this._httpClient.DefaultRequestHeaders.Add("Referer", @"https://servicewechat.com/wx965de401c0dd3432/1/page-frame.html");
            this._httpClient.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36 MicroMessenger/7.0.9.501 NetType/WIFI MiniProgramEnv/Windows WindowsWechat");

            this.Settings = new WechaptAppTestgroupConfig();
           
 
        }



        protected override void Reset()
        {
   

        }
         

        protected override async Task DoBegin(CancellationToken token)
        {
            var arr = this.Config.OpenIdIdentifyMap.ToArray()
                .Take(this.Config.ThreadCount).ToArray();
 
            List<Task> tasks = new List<Task>();

            List<WechatAppThread> threads = new List<WechatAppThread>();
            try
            {
                foreach (var pair in arr)
                {
                    token.ThrowIfCancellationRequested();
                    WechatAppThread client = new WechatAppThread(this, threads.Count + 1, pair.Value, pair.Key, token);
                    threads.Add(client);
                }

                var delay = this.Config.RampUpDelay;
      
                foreach (var client in threads)
                {
                    token.ThrowIfCancellationRequested();
                     
                    await Task.Delay(delay, token);

                    var t = Task.Run(async () => await client.Begin(),
                        token);

                    tasks.Add(t);
                    this.ActiveCount = tasks.Count;
                }
   
                await Task.WhenAll(tasks);
            }
            finally
            {
                foreach (var client in threads)
                {
                    client.Dispose();
                }
            }
           

            


        }

 


    }
}