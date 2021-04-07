using System.Collections.Generic;
using AiTest.Utils;

namespace AiTest
{
    public class SampleResultStat: BindableBase
    {
        private int _sampleCount;
        public int SampleCount
        {
            get { return this._sampleCount; }
            set { SetProperty(ref this._sampleCount, value); }
        }

        private string _label;
        public string Label
        {
            get { return this._label; }
            set { SetProperty(ref this._label, value); }
        }
 
        private double _averageResponseTime;
        public double AverageResponseTime
        {
            get { return this._averageResponseTime; }
            set { SetProperty(ref this._averageResponseTime, value); }
        }

        private double _minResponseTime;
        public double MinResponseTime
        {
            get { return this._minResponseTime; }
            set { SetProperty(ref this._minResponseTime, value); }
        }

        private double _maxResponseTime;
        public double MaxResponseTime
        {
            get { return this._maxResponseTime; }
            set { SetProperty(ref this._maxResponseTime, value); }
        }

        private int _errorCount;
        public int ErrorCount
        {
            get { return this._errorCount; }
            set { SetProperty(ref this._errorCount, value); }
        }

        public List<SampleResult> ErrorSampleResults { get; }

        public SampleResultStat()
        {
            this.ErrorSampleResults = new List<SampleResult>();
        }
    }
}