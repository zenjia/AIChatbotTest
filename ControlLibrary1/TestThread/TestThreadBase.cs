using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace AiTest
{
    public abstract class TestThreadBase 
    {
        protected readonly int ThreadId;

        

        protected readonly ITestThreadGroupBase _owner;

        protected readonly CancellationToken _cancellationToken;

        protected TestThreadBase(ITestThreadGroupBase owner, int threadId, CancellationToken cancellationToken)
        {
            
            
            this._owner = owner;
            this._cancellationToken = cancellationToken;
            this.ThreadId = threadId;

        }

        protected void CheckIfCancelled()
        {
            this._cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="label">sample label</param>
        /// <returns></returns>
        protected string Decorate(string message, string label = null)
        {
            if (label == null)
            {
                return message;
            }
            else
            {
                return $"\"{label}\": {message}";
            }

        }

        protected async Task AddLog(string message, string label = null, LogType t = LogType.Info)
        {
            var msg = Decorate(message, label);
            switch (t)
            {
                case LogType.Info:
                    await this._owner.Log.Info(this.ThreadId, msg);
                    break;
                case LogType.Warn:
                    await this._owner.Log.Warn(this.ThreadId, msg);
                    break;
                case LogType.Error:
                    await this._owner.Log.Error(this.ThreadId, msg);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(t), t, null);
            }

        }

        protected abstract Task BeginTest();
 
        public async Task Begin()
        {
            try
            {
                await BeginTest();
            }
            catch (Exception e)
            {
                Exception ex = e is AggregateException ? e.InnerException : e;
                var t = ex?.GetType();
                if (t == typeof(OperationCanceledException) || t == typeof(TaskCanceledException))
                {
                    await AddLog("测试已取消：" + ex?.GetType() + ":" + ex?.Message, null, LogType.Warn);
                }
                else
                {
                    await AddLog("测试异常中断：" + ex?.GetType() + ":" + ex?.Message, null, LogType.Error);
                }

            }
            finally
            {
                await this._owner.DecActiveThreadCount();
            }
        }


    }
}