using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AiModel;
using Newtonsoft.Json;

namespace AiTest
{
    public class WechatAppThread : TestThreadBase, IDisposable
    {
        private static readonly string Label_Login = "登录";
        private static readonly string Label_Connect = "创建连接";
        private static readonly string Label_Auth = "认证";
        private static readonly string Label_Hello = "你好！";
        private static readonly string Label_Business = "我下期什么时候还款？";
        private static readonly string Label_OK = "好的";
        private static readonly string Label_GoodBye = "再见！";
        private static readonly string Label_Byebye = "没有了";

        private static readonly string authUrl = "https://mlp-int.bmwgroup.com/AIChat/e2_uat/chatpage/api/chat/auth/callback";
        private static readonly Uri serverUri = new Uri("wss://mlp-int.bmwgroup.com:443/AIChat/wsapi/e2_uat/chat/im");

        private readonly string OpenId;
        private readonly string Identify;

        protected readonly ClientWebSocket ws;

        private ITestThreadGroup _owner2 => (ITestThreadGroup)this._owner;
        public WechatAppThread(ITestThreadGroup owner, int threadId, string identify, string openid, CancellationToken cancellationToken)
            : base(owner, threadId, cancellationToken)
        {
            this.ws = new ClientWebSocket();

            this.OpenId = openid;
            this.Identify = identify;

        }

        private async Task DoSample(string label, Func<Task<string>> request,
            Stopwatch sw)
        {
           
            int sampleId = this._owner2.GenerateSampleId();

            sw.Reset();
            sw.Start();
            var timeStamp = DateTimeOffset.Now;
            var activeThreadCt = this._owner2.GetActiveThreadCount();
            SampleResult sampleResult;
            try
            {
                var responseBody = await request();

                sampleResult = new SampleResult(sampleId,
                    activeThreadCt,
                    label, this.ThreadId,
                    timeStamp,
                    sw.ElapsedMilliseconds, true,
                    responseBody, null);

                sampleResult.ThreadHashCode = this.GetHashCode();

                await this._owner2.AddSampleResult(sampleResult);

                sw.Stop();
               

            }
            catch (OperationCanceledException tce)
            {
                throw;
            }
            catch (Exception e)
            {
                string failInfo = string.Format("{0}: {1}", e.GetType(), e.Message);

                sw.Stop();
                sampleResult = new SampleResult(sampleId,
                    activeThreadCt,
                    label, this.ThreadId, timeStamp,
                    sw.ElapsedMilliseconds, false, null, failInfo);
                sampleResult.ThreadHashCode = this.GetHashCode();

                await AddLog($"sampleId={sampleId}，发生异常：{failInfo}", label, LogType.Error);

                await this._owner2.AddSampleResult(sampleResult);
                throw;
            }

        }

 

        private async Task DelayChat(int delayInSecs=-1)
        {
            if (delayInSecs == -1)
            {
                await Task.Delay(this._owner2.GetChatDelay(), this._cancellationToken);
            }
            else
            {
                await Task.Delay(delayInSecs * 100, this._cancellationToken);
            }
        }

        protected override async Task BeginTest()
        {

            Stopwatch sw = new Stopwatch();
            LoginContent loginContent = null;
             
            await DoSample(Label_Login, async () =>
            {
                var tuple = await HttpLogin(this._owner2.GetHttpClient());
                loginContent = tuple.Item1.content;
                return tuple.Item2;
            },
            sw);
              
            await DoSample(Label_Connect, async () => await Connect(),
              sw);
              
            await DoSample(Label_Auth, async () => await Auth(loginContent),
             sw);
             
            for (int i = 0; i < this._owner2.GetChatRoundCount(); i++)
            { 

                await DoSample(Label_Hello, async () => await SendMessage(Label_Hello),
                  sw);
                await DelayChat();
                  
                await DoSample(Label_Business, async () => await SendMessage(Label_Business),
                  sw);
                await DelayChat();
                  
                await DoSample(Label_OK, async () => await SendMessage(Label_OK),
                  sw);
                await DelayChat();
            }
             

            await DoSample(Label_GoodBye, async () => await SendMessage(Label_GoodBye),
              sw);

            await DelayChat(); 

            await DoSample(Label_Byebye, async () => await SendMessage(Label_Byebye, false),
              sw);

            await AddLog("测试结束");

        }

        /// <summary>
        /// https://stackoverflow.com/questions/11178220/is-httpclient-safe-to-use-concurrently
        /// </summary>
        /// <param name="httpClient">reusing HttpClient instances as much as possible</param>
        /// <returns></returns>
        public async Task<(LoginResult, string)> HttpLogin(HttpClient httpClient)
        {
            await AddLog("正在Https登录...", Label_Login);

            HttpLoginInfo httpLoginInfo = new HttpLoginInfo(this.Identify, this.OpenId);

            var json = JsonConvert.SerializeObject(httpLoginInfo);

            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(authUrl, data, this._cancellationToken);

            string jsonStr = await response.Content.ReadAsStringAsync();

            if (jsonStr.StartsWith("{\"type\": \"continue_login"))
            {
                throw new AITestException("重复登录！");
            }


            /*{"result": "success", "content":
              {"loginStatus": "success", "loginType": "PHONENOCON", "session_id": "e83d9eda-7503-11eb-97de-920ed07a473b",
               "openid": "b06fbfa6-c3f6-4070-8ff8-c2b9974e387e"}}*/

            int j = jsonStr.IndexOf("}}") + 2;
            string jsonStr2 = j == jsonStr.Length ? jsonStr : jsonStr.Substring(0, j);

            LoginResult loginResult;
            try
            {
                loginResult = JsonConvert.DeserializeObject<LoginResult>(jsonStr2);
            }
            catch (Exception e)
            {
                await AddLog($"解析Json数据失败：{jsonStr}", Label_Login, LogType.Error);
                throw;
            }

            if (loginResult == null)
            {
                throw new NotImplementedException($"Http登录返回null：{jsonStr}");
            }

            if (loginResult.content.contracts?.Any() != true)
            {
                throw new AITestException($"Http登录返回数据缺少合同信息！");
            }

            string responseText = $"response headers: {response.Headers}{Environment.NewLine}{jsonStr}";

            await AddLog("Https登录成功！", Label_Login);

            return (loginResult, responseText);

        }

        public async Task<string> Connect()
        {
            await AddLog("Connecting..", Label_Connect);
            await this.ws.ConnectAsync(serverUri, this._cancellationToken);

            if (this.ws.State != WebSocketState.Open)
            {
                throw new AITestConnectFailedException($"Can not Connected to {serverUri}!!!");
            }

            await AddLog("Connected!", Label_Connect);
            return Decorate("connected!");

        }

        public async Task<string> Auth(LoginContent loginContent)
        {
            await AddLog("正在发送认证信息..", Label_Auth);

            AuthRequestInfo authRequest = new AuthRequestInfo(loginContent.openid, loginContent.session_id, loginContent.contracts.First());

            var msg = JsonConvert.SerializeObject(authRequest);
            await this.ws.SendTextAsync(msg, this._cancellationToken);

            //Trace.WriteLine("msg="+ msg);

            var reply = await WaitForReplyMessage(Label_Auth, x =>
            {
                if (x.StartsWith("{\"event\": \"11") && x.Contains("\\u60a8\\u7684\\u8eab\\u4efd\\u8ba4\\u8bc1\\u5df2\\u5931\\u6548\\uff0c\\u8bf7\\u9000\\u51fa\\u91cd\\u65b0\\u767b\\u5f55"))
                {
                    throw new AITestAuthFailedException("身份认证已经失效，需要重新登录！");
                }
            });

            await AddLog("认证成功！", Label_Auth);
            return StringHelper.Unicode2String(reply);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="loopCt"></param>
        /// <param name="timeout"></param>
        /// <returns>超时或循环超过10次，则返回false</returns>
        private async Task<string> WaitForReplyMessage(string lable, Action<string> msgPreProcessor = null)
        {
            string response = null;

            CheckIfCancelled();
            int loopCt = 0;
            while (true)
            {
                loopCt++;
                if (loopCt >= 10)
                {
                    throw new AITestException("读取AI回复超出最大循环次数(10)!");
                }

                using (var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(this._cancellationToken))
                {
                    tokenSource.CancelAfter(this._owner2.GetReadTimeOut());
                    try
                    {
                        response = await this.ws.ReceiveMessageAsync(tokenSource.Token);
                    }
                    catch (OperationCanceledException e)
                    {
                        if (!this._cancellationToken.IsCancellationRequested)
                        {
                            //await AddLog("等待AI回复超时！", lable, LogType.Warn);
                            throw new AITestReadTimeoutException("等待AI回复超时！");
                        }

                        throw;
                    }


                }


                msgPreProcessor?.Invoke(response);

                //if (lable.Contains("认证"))
                //{
                //    Trace.WriteLine("msg=" + response);

                //}
               
                if (response != null && response.StartsWith("{\"event\": \"101:"))
                {
                    return response;
                }

            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="waitForResponse"></param>
        /// <returns>response body</returns>
        public async Task<string> SendMessage(string msg, bool waitForResponse = true)
        {
            await AddLog("正在发送信息", msg);
            OutGoingMessage goingMessage = new OutGoingMessage(msg);

            await this.ws.SendTextAsync(JsonConvert.SerializeObject(goingMessage), this._cancellationToken);

            //await AddLog("信息发送完毕！", msg);

            if (!waitForResponse)
            {
                return null;
            }

            //await AddLog("正在等待AI回复...", msg);
            var reply = await WaitForReplyMessage(msg);
            await AddLog("收到AI回复！", msg);

            return StringHelper.Unicode2String(reply);

        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ws?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}