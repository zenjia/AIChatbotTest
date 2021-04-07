using System;
using System.Xml;
using System.Xml.Linq;

namespace AiTest
{
    public class SampleResult
    {
        public int ThreadId { get; }
        public int ThreadHashCode { get; set; }
        public int Id { get; }
        public int ActiveThreadCount { get; }
        public string Label { get; }
        public DateTimeOffset TimeStamp { get; }
        public long Elapsed { get; }
        public bool IsSucceed { get; }
        public string Response { get; }
        public string FailMessage { get; }
        public SampleResult(int id, int activeThreadCount, string label, int threadId,
            DateTimeOffset timeStamp, long elapsed,
            bool isSucceed, string response, string failMessage)
        {
            this.Id = id;
            this.ActiveThreadCount = activeThreadCount;
            this.Label = label;
            this.ThreadId = threadId;
            this.TimeStamp = timeStamp;
            this.Elapsed = elapsed;
            this.IsSucceed = isSucceed;
            this.Response = response;
            this.FailMessage = failMessage;
        }

        public override string ToString()
        {
            return $"线程{this.ThreadId}({this.ThreadHashCode}),{this.Id},{this.Label},{this.TimeStamp},{this.Elapsed},{this.IsSucceed}";
        }

        public XElement SaveToXml()
        {
            XElement element = new XElement("item");
            element.SetAttributeValue("ThreadId", this.ThreadId);
            element.SetAttributeValue("Id", this.Id);
            element.SetAttributeValue("ActiveThreadCount", this.ActiveThreadCount);
            element.SetAttributeValue("Label", this.Label);

            element.SetAttributeValue("TimeStamp", this.TimeStamp.ToUnixTimeMilliseconds());
            element.SetAttributeValue("Elapsed", this.Elapsed);
            element.SetAttributeValue("IsSucceed", this.IsSucceed ? "1" : "0");
            element.SetAttributeValue("Response", this.Response);
            element.SetAttributeValue("FailMessage", this.FailMessage);
            if (this.Response != null)
            {
               
                XElement childElement = new XElement("Response");
                childElement.Value = this.Response;
                element.Add(childElement);
            }

            if (this.FailMessage != null)
            {
                XElement childElement = new XElement("FailMessage");
                childElement.Value = this.FailMessage;
                element.Add(childElement);
            }
            return element;
        }

        public static SampleResult LoadFromXml(XElement element)
        {
            var threadIndex = int.Parse(element.Attribute("ThreadId").Value);
            var id = int.Parse(element.Attribute("Id").Value);
            var activeThreadCount = int.Parse(element.Attribute("ActiveThreadCount").Value);
            var elapsed = long.Parse(element.Attribute("Elapsed").Value);

            long milliseconds = long.Parse(element.Attribute("TimeStamp").Value);
            var timeStamp = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);

            var label = element.Attribute("Label").Value; 

            var childElement = element.Element("Response");
            var response = childElement != null ? childElement.Value : null;

            childElement = element.Element("FailMessage");
            var failMessage = childElement != null ? childElement.Value : null;

            var isSucceed = int.Parse(element.Attribute("IsSucceed").Value) == 1 ? true : false;

            return new SampleResult(id, activeThreadCount, label, threadIndex,
                timeStamp, elapsed, isSucceed, response, failMessage);
        }

    }
}