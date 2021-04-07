using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AiTest
{
    /// <summary>
    /// Test ThreadGroup with SampleResult support
    /// </summary>
    public abstract class TestThreadGroup : TestThreadGroupBase, ISampleResultListOwner
    {
        private int _sampleId = 0;

        private ITestThreadGroupOwner _owner;
        public SampleResultList SampleResults { get; }

        protected TestThreadGroup(ITestThreadGroupOwner owner)
            : base(owner.GetLog())
        {
            this._owner = owner;
            this.SampleResults = new SampleResultList(this);
        }

        public async Task AddSampleResult(SampleResult sampleResult)
        {
            await this.SampleResults.Add(sampleResult);
        }

        public int GenerateSampleId()
        {
            return Interlocked.Increment(ref this._sampleId);
        }

        public string GetTestSampleResultFileName()
        {
            var path = Path.Combine(this._owner.TestPlanPath, this.Settings.Label);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return Path.Combine(path, "SampleResult.xml");
        }

        public override void Clear()
        {
            this.SampleResults.Clear();

        }

        protected override void Reset()
        {
            base.Reset();
            Clear();
            this._sampleId = 0;
        }


    }
}