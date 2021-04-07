using System.Diagnostics;
using System.Windows;
using System.Xml.Linq;
using AiTest.Utils;

namespace AiTest
{
    public class TestGroupSettingsBase : BindableBase
    {
        private bool _isDataChanged;
        public bool IsDataChanged
        {
            get { return this._isDataChanged; }
            set
            {
                if (SetProperty(ref this._isDataChanged, value))
                {
 
                }

            }
        }

        private string _label = "线程组";
        public string Label
        {
            get { return _label; }
            set
            {
                if (SetProperty(ref this._label, value))
                {
                    this.IsDataChanged = true;
                }
            }
        }

        private int _startDelay;
        public int StartDelay
        {
            get { return _startDelay; }
            set
            {
                if (SetProperty(ref this._startDelay, value))
                {
                    this.IsDataChanged = true;
                }
            }
        }

        private int _threadCount = 50;
        public int ThreadCount
        {
            get { return this._threadCount; }
            set
            {
                if (SetProperty(ref this._threadCount, value))
                {
                    this.IsDataChanged = true;
                }

            }
        }

        private int _rampUpDelay = 500;//线程间隔500毫秒启动
        public int RampUpDelay
        {
            get { return this._rampUpDelay; }
            set
            {
                if (SetProperty(ref this._rampUpDelay, value))
                {
                    this.IsDataChanged = true;
                }
            }
        }

        public void SaveToXaml(XElement element)
        {
            DoSaveToXaml(element);
            this.IsDataChanged = false;
        }

        public virtual void LoadFromXml(XElement element)
        {

            DoLoadFromXaml(element);
            this.IsDataChanged = false;
        }

        protected virtual void DoSaveToXaml(XElement element)
        {
            element.SetAttributeValue("Label", this.Label);
            element.SetAttributeValue("threadCount", this.ThreadCount);
            element.SetAttributeValue("rampUpDelay", this.RampUpDelay);
            element.SetAttributeValue("startDelay", this.StartDelay);
             
        }

        protected virtual void DoLoadFromXaml(XElement element)
        {
            this.Label = element.Attribute("Label").Value;
            this.ThreadCount = (int)element.Attribute("threadCount");
            this.RampUpDelay = (int)element.Attribute("rampUpDelay");
            this.StartDelay = (int)element.Attribute("startDelay");


        }
    }
}