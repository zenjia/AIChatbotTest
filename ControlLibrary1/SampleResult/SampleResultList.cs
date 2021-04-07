using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;
using AiTest.Utils;

namespace AiTest
{
    public interface ISampleResultListOwner
    {
        string GetTestSampleResultFileName();
    }

    public class SampleResultList : BindableBase, ISampleResultList
    {
        private ISampleResultListOwner _owner;
        public ObservableCollection<SampleResult> Items { get; }

        private Dictionary<string, SampleResultStat> _sampleResultStatDic;
        public ObservableCollection<SampleResultStat> SampleResultStats { get; }

        public SampleResultList(ISampleResultListOwner owner)
        {
            this._owner = owner;
            this._sampleResultStatDic = new Dictionary<string, SampleResultStat>();
            this.Items = new ObservableCollection<SampleResult>();
            this.Items.CollectionChanged += Items_CollectionChanged;
            this.SampleResultStats = new ObservableCollection<SampleResultStat>();
        }
         

        /// <summary>
        /// 获取测试起止时间
        /// </summary>
        /// <returns></returns>
        public string GetTestStartEndTime()
        {
            var firstItem = this.Items.First().TimeStamp.AddHours(8);
            var lastItem = this.Items.Last().TimeStamp.AddHours(8);
             


            return $"测试时间：{firstItem:yyyy-MM-dd HH:mm:ss} ~ {lastItem:yyyy-MM-dd HH:mm:ss}";
        }
         

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                Debug.Assert(e.NewItems.Count == 1);
                SampleResult sampleResult = (SampleResult)e.NewItems[0];
                OnItemAdd(sampleResult);
            }
        }

        private void OnItemAdd(SampleResult sampleResult)
        {
 

            if (this._sampleResultStatDic.ContainsKey(sampleResult.Label))
            {
                var item = this._sampleResultStatDic[sampleResult.Label];
                item.SampleCount++;
                if (sampleResult.IsSucceed)
                {
                    int sampleCt = item.SampleCount - item.ErrorCount;
                    item.AverageResponseTime = (item.AverageResponseTime * sampleCt + sampleResult.Elapsed) /
                                               (sampleCt + 1);

                    if (sampleResult.Elapsed > item.MaxResponseTime)
                    {
                        item.MaxResponseTime = sampleResult.Elapsed;
                    }

                    if (sampleResult.Elapsed < item.MinResponseTime)
                    {
                        item.MinResponseTime = sampleResult.Elapsed;
                    }
                }
                else
                {
                    item.ErrorCount++;
                    item.ErrorSampleResults.Add(sampleResult);
                }
            }
            else
            {
                SampleResultStat item = new SampleResultStat();
                item.Label = sampleResult.Label;
                item.SampleCount = 1;

                this._sampleResultStatDic.Add(item.Label, item);
                this.SampleResultStats.Add(item);

                if (sampleResult.IsSucceed)
                {
                    item.MaxResponseTime = sampleResult.Elapsed;
                    item.MinResponseTime = sampleResult.Elapsed;
                    item.AverageResponseTime = sampleResult.Elapsed;
                }
                else
                {
                    item.ErrorCount = 1;
                    item.ErrorSampleResults.Add(sampleResult);
                }
            }
        }

        public async Task Add(SampleResult sampleResult)
        {
            await Application.Current.Dispatcher.InvokeAsync(() => this.Items.Add(sampleResult)
               , DispatcherPriority.Normal);
        }

        public string GetSampleResultFileName()
        {
            return this._owner.GetTestSampleResultFileName();
        }

        public void Save()
        {
            string fileName = GetSampleResultFileName();
            XDocument document = new XDocument(new XElement("root"));

            foreach (var item in this.Items)
            {
                document.Root.Add(item.SaveToXml());
            }

            document.Save(fileName);
             
        }

  
        public void Load()
        {
            string fileName = GetSampleResultFileName();
            this.Items.Clear();
            if (!File.Exists(fileName))
            {

                return;
            }

 
            XDocument document = XDocument.Load(fileName);

            foreach (var element in document.Root.Elements())
            {
                var sr = SampleResult.LoadFromXml(element);
                this.Items.Add(sr);
            }
             
        }

        public void DeleteFile()
        {
            string fileName = GetSampleResultFileName();
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

        }

        public void Clear()
        {
            this.Items.Clear();
            this.SampleResultStats.Clear();
            this._sampleResultStatDic.Clear();

            DeleteFile();
             
        }
    }
}