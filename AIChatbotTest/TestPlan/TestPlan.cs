using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AiTest.ViewModel
{
    public class TestPlan : TestPlanBase
    {
        private WechatAppThreadGroup _wechatAppThreadGroup;
        public WechatAppThreadGroup WechatAppThreadGroup
        {
            get { return this._wechatAppThreadGroup; }
            private set { SetProperty(ref this._wechatAppThreadGroup, value); }
        }

        public TestPlan()
        {

        }

        public void CreateDefaultThreadGroup()
        {
            if (this._threadGroups.Any())
            {
                throw new NotImplementedException();
            }

            this.WechatAppThreadGroup = new WechatAppThreadGroup(this);
            this._threadGroups.Add(this.WechatAppThreadGroup);
        }

        protected override TestThreadGroupBase LoadThreadGroupsFromXml(XElement element)
        {
            WechatAppThreadGroup threadGroup = new WechatAppThreadGroup(this);
            threadGroup.LoadFromXml(element);
            this.WechatAppThreadGroup = threadGroup;
            return threadGroup;
        }

        protected override async Task DoLoadTestData()
        {
            await base.DoLoadTestData();

            foreach (var threadGroup in this._threadGroups.OfType<TestThreadGroup>())
            {
                threadGroup.SampleResults.Load();
            }


        }

        protected override void DoSaveTestData()
        {
            base.DoSaveTestData();
            foreach (var threadGroup in this._threadGroups.OfType<TestThreadGroup>())
            {
                threadGroup.SampleResults.Save();
            }
        }

   

    }
}
