using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;
using AiTest.Utils;

namespace AiTest
{
    public abstract class TestThreadGroupBase : BindableBase
    {

        private CancellationTokenSource _cts;
         

        private TestGroupSettingsBase _settings;
        public TestGroupSettingsBase Settings
        {
            get { return this._settings; }
            set { SetProperty(ref this._settings, value); }
        }

        public ILog Log { get; }

     

        private int _activeCount = 0;


        public int ActiveCount
        {
            get { return this._activeCount; }
            set { SetProperty(ref this._activeCount, value); }
        }

        protected TestThreadGroupBase(ILog log)
        {
            
            this.Log = log;
        }

        public async Task DecActiveThreadCount()
        {
            await Application.Current.Dispatcher
                .InvokeAsync(() => this.ActiveCount--, DispatcherPriority.Normal);
        }



        protected virtual void Reset()
        {
            this.ActiveCount = 0;
            
        }



        public void Cancel()
        {
            this._cts?.Cancel();
        }

        protected abstract Task DoBegin(CancellationToken token);

        public async Task Begin()
        {
            Reset();

            this._cts = new CancellationTokenSource();
            try
            {
                this._cts.Token.Register(async () => await this.Log.Info(-1, "Test is Canceled!"));

                await Task.Delay(this.Settings.StartDelay, this._cts.Token);

                await DoBegin(this._cts.Token);

            }
            finally
            {
                this._cts.Dispose();
                this._cts = null;
            }

        }

        public void SaveToXml(XElement element)
        {

            this.Settings.SaveToXaml(element);
        }

        public void LoadFromXml(XElement element)
        {

            this.Settings.LoadFromXml(element);
        }



        public virtual void Clear()
        {
            

        }
    }
}