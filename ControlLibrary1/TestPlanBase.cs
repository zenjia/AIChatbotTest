using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using AiTest.Utils;

namespace AiTest.ViewModel
{
    public abstract class TestPlanBase : BindableBase, ITestThreadGroupOwner, ISimpleLogOwner
    {
         
        protected ObservableCollection<TestThreadGroupBase> _threadGroups = new ObservableCollection<TestThreadGroupBase>();

        public static readonly string DefaultTestPlanFileName = "Test Plan";

        private string _testPlanPath;
        public string TestPlanPath
        {
            get { return this._testPlanPath; }
            set
            {
                if (SetProperty(ref this._testPlanPath, value))
                {
                    this.TestPlanFileName = Path.Combine(this.TestPlanPath, $"{DefaultTestPlanFileName}.cmx");

                }

            }
        }

        public ILog GetLog()
        {
            return this.Log;
        }

        private string _testPlanFileName;
        public string TestPlanFileName
        {
            get { return _testPlanFileName; }
            set { SetProperty(ref this._testPlanFileName, value); }
        }

        public bool IsDataChanged
        {
            get
            {
                return this._threadGroups.Any(x => x.Settings.IsDataChanged);

            }
        }

        public SimpleLog Log { get; }

        protected TestPlanBase()
        {

            this.Log = new SimpleLog(this);
        }

        protected abstract TestThreadGroupBase LoadThreadGroupsFromXml(XElement element);

        public void LoadTestPlan(string testPlanPath)
        {
            this.TestPlanPath = testPlanPath;
            this._threadGroups.Clear();

            var fileName = this.TestPlanFileName;
            if (!File.Exists(fileName))
            {
                return;
            }
            XDocument document = XDocument.Load(fileName);

            XElement threadGroupNode = document.Root.Element("threadGroups");

            foreach (var element in threadGroupNode.Elements())
            {
                TestThreadGroupBase thg = LoadThreadGroupsFromXml(element);

                this._threadGroups.Add(thg);
            }

        }
         
        protected virtual async Task DoLoadTestData()
        {
            await this.Log.Load();
        }

        public async Task LoadTestData()
        {
            try
            {
                await DoLoadTestData();
 
            }
            catch (Exception e)
            {
                MessageBox.Show($"加载TestData失败：{e.GetType()}: {e.Message}", "出错了");
                throw;
            }

        }
         
        protected virtual void DoSaveTestData()
        {
            this.Log.Save();
        }

        public void SaveTestData()
        {
            DoSaveTestData();
        }

        public void SaveTestPlan()
        {
            XDocument document = new XDocument(new XElement("root"));

            XElement threadGroupNode = new XElement("threadGroups");
            document.Root.Add(threadGroupNode);

            foreach (var threadGroup in this._threadGroups)
            {
                XElement element = new XElement("threadGroup");

                threadGroup.SaveToXml(element);

                threadGroupNode.Add(element);
            }

            document.Save(this.TestPlanFileName);
        }

        public void Clear()
        {
            this.Log.Clear();
            foreach (var threadGroup in this._threadGroups)
            {
                threadGroup.Clear();
            }
 
        }

        public async Task Begin()
        {
            Clear();
            try
            {
                foreach (var threadGroup in this._threadGroups)
                {
                    await threadGroup.Begin();
                }
            }
            finally
            {
                SaveTestData();

            }

        }

        public void Cancel()
        {
            foreach (var threadGroup in this._threadGroups)
            {
                threadGroup.Cancel();
            }

        }
    }
}