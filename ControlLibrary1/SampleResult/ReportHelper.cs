using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ControlLibrary1.SampleResult
{


    public class ReportHelper
    {
        public static string CreateAggregateReport(IEnumerable<AiTest.SampleResult> source)
        {
            StringBuilder sb = new StringBuilder();
            //Label,样本数,均值,中位数,Min,Max,90百分位,95百分位,99百分位,Error
            sb.AppendLine("Label,样本数,平均值,中位数,最小值,最大值,90百分位,95百分位,99百分位,Error");
            foreach (var group in source.GroupBy(sr => sr.Label))
            {
                var arr = group.Select(sr => sr.Elapsed).OrderBy(x => x).ToArray();
                var middleIndex = arr.Length / 2;

                var median = arr[middleIndex];

                var line_90 = arr[(int)(arr.Length * 0.90)];
                var line_95 = arr[(int)(arr.Length * 0.95)];
                var line_99 = arr[(int)(arr.Length * 0.99)];

                sb.AppendLine(
                    $"{group.Key},{arr.Length},{(arr.Sum() / (double)arr.Length):F0},{median},{arr.Min()},{arr.Max()},{line_90},{line_95},{line_99},{group.Count(sr => !sr.IsSucceed)}");
            }

            return sb.ToString();
        }

        class StatItem
        {
            public int Count { get; set; }
            public double Total { get; set; }

            public override string ToString()
            {
                return $"{(this.Total / this.Count):F0}";
            }
        }

        /// <summary>
        /// 获取并发数、样本数、响应时间的序列数据（可用于生成走势图）
        /// </summary>
        /// <param name="source"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static string GetTimeSeriesData(IEnumerable<AiTest.SampleResult> source, int stepInMilliSeconds)
        {
            var start = source.First().TimeStamp;
            var totalSpan = source.Last().TimeStamp.Subtract(start).TotalMilliseconds;

            int ct = (int)(totalSpan / stepInMilliSeconds) + 1;

            var labels = source.Select(x => x.Label).Distinct().ToArray();
            var indexMap = new Dictionary<string, int>();
            for (int i = 0; i < labels.Length; i++)
            {
                indexMap.Add(labels[i], i);
            }

            StatItem[][] arr = new StatItem[ct][];
            int[] maxIndex = new int[labels.Length];

            StatItem[] threadCtArr = new StatItem[ct];
            foreach (var item in source)
            {
                var label = item.Label;
                totalSpan = item.TimeStamp.Subtract(start).TotalMilliseconds;
                int index = (int)(totalSpan / stepInMilliSeconds);

                #region 并发数
                if (threadCtArr[index] == null)
                {
                    threadCtArr[index] = new StatItem();
                }

                threadCtArr[index].Total += item.ActiveThreadCount;
                threadCtArr[index].Count++;
                #endregion

                #region 响应时间
                if (arr[index] == null)
                {
                    arr[index] = new StatItem[labels.Length];
                }

                var i = indexMap[label];

                if (arr[index][i] == null)
                {
                    arr[index][i] = new StatItem();
                }
                arr[index][i].Total += item.Elapsed;
                arr[index][i].Count++;

                maxIndex[i] = index;
                #endregion

            }

            StatItem[] lastArrItem = new StatItem[labels.Length];
            for (var j = 0; j < ct; j++)
            {
                #region 并发数
                if (threadCtArr[j] == null)
                {
                    if (arr[j] != null)
                    {
                        throw new NotImplementedException();
                    }

                    continue;

                    //if (j > 0)
                    //{
                    //    threadCtArr[j] = threadCtArr[j - 1];
                    //}
                    //else
                    //{
                    //    throw new NotImplementedException();
                    //}
                }
                #endregion

                #region 响应时间
                var a = arr[j];

                if (a == null)
                {
                    continue;
                }

                for (int i = 0; i < a.Length; i++)
                {
                    if (j > maxIndex[i])
                    {
                        continue;
                    }

                    if (a[i] == null)
                    {
                        if (lastArrItem[i] != null)
                        {
                            a[i] = lastArrItem[i];
                        }
                    }
                    else
                    {
                        lastArrItem[i] = a[i];
                    }
                }
                #endregion

            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"时间(秒),样本数,平均并发数,{string.Join(",", labels)}");
            for (int i = 0; i < ct; i++)
            {
                var a = arr[i];
                if (a == null)
                {
                    continue;

                }

                var b = threadCtArr[i];

                var t = TimeSpan.FromMilliseconds(stepInMilliSeconds * i);
                sb.AppendLine(string.Format("{0}:{1},{2},{3},{4}", t.Minutes, t.Seconds,
                    b.Count, b,
                    string.Join(",", a.Select(x => x))));
            }


            return sb.ToString();

        }

        //public static string GetActiveThreadAndSampleCountOverTime(IEnumerable<AiTest.SampleResult> source, TimeSpan step)
        //{
        //    var start = source.First().TimeStamp;
        //    var totalSpan = source.Last().TimeStamp.Subtract(start);

        //    int ct = (int)(totalSpan.TotalMilliseconds / step.TotalMilliseconds) + 1;

        //    StatItem[] arr = new StatItem[ct];

        //    foreach (var item in source)
        //    {
        //        totalSpan = item.TimeStamp.Subtract(start);
        //        int index = (int)(totalSpan.TotalMilliseconds / step.TotalMilliseconds);
        //        if (arr[index] == null)
        //        {
        //            arr[index] = new StatItem();
        //        }

        //        arr[index].Total += item.ActiveThreadCount;
        //        arr[index].Count++;
        //    }

        //    // StatItem lastArrItem = null;
        //    for (var j = 0; j < arr.Length; j++)
        //    {
        //        if (arr[j] == null)
        //        {
        //            if (j > 0)
        //            {
        //                arr[j] = arr[j - 1];
        //            }
        //            else
        //            {
        //                throw new NotImplementedException();
        //            }
        //        }

        //    }

        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine("时间(秒),平均并发数,样本数");
        //    for (int i = 0; i < arr.Length; i++)
        //    {
        //        var a = arr[i];

        //        var t = TimeSpan.FromMilliseconds(step.TotalMilliseconds * i);
        //        sb.AppendLine(string.Format("{0}:{1},{2},{3}", t.Minutes, t.Seconds, a, a.Count));
        //    }

        //    return sb.ToString();
        //}

        /// <summary>
        /// 获取响应时间分布数据
        /// </summary>
        /// <param name="source"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static (string detailed, string condensed) GetResponseDistributionData(IEnumerable<AiTest.SampleResult> source, int stepInMillseconds)
        {
            var maxResponseTime = source.Max(x => x.Elapsed);

            int ct = (int)(maxResponseTime / stepInMillseconds) + 1;

            var labels = source.Select(x => x.Label).Distinct().ToArray();
            var indexMap = new Dictionary<string, int>();
            for (int i = 0; i < labels.Length; i++)
            {
                indexMap.Add(labels[i], i);
            }

            int[][] arr = new int[ct][];
            foreach (var item in source)
            {
                var label = item.Label;

                int index = (int)(item.Elapsed / stepInMillseconds);

                #region 响应时间
                if (arr[index] == null)
                {
                    arr[index] = new int[labels.Length];
                }

                var i = indexMap[label];

                arr[index][i]++;

                #endregion

            }


            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"响应时间(毫秒),{string.Join(",", labels)}");
            for (int i = 0; i < ct; i++)
            {
                var a = arr[i];
                if (a == null)
                {
                    continue;

                }

                sb.AppendLine(string.Format("{0}~{1},{2}", i * stepInMillseconds,
                    (i + 1) * stepInMillseconds,
                    string.Join(",", a.Select(x => x == 0 ? string.Empty : x.ToString()))));
            }

            var detailData = sb.ToString();
            #region 精简版

            sb.Clear();
            //截短数据
            for (int i = 4; i < ct; i++)
            {
                if (arr.Length <= i || arr[i] == null)
                {
                    continue;
                }
                 

                for (int j = 0; j < labels.Length; j++)
                {
                    arr[3][j] += arr[i][j];
                }

            }

            //求和
            int[] sums = new int[labels.Length];
            for (int i = 0; i < 4; i++)
            {
                if (arr.Length <= i)
                {
                    break;
                }
                var a = arr[i];

                if (a == null)
                {
                    continue;
                }

                for (int j = 0; j < sums.Length; j++)
                {
                    sums[j] += a[j];
                }
            }


            sb.AppendLine($"响应时间(毫秒),{string.Join(",", labels)}");
            int[] indexArr = Enumerable.Range(0, labels.Length).ToArray();
            for (int i = 0; i < 4; i++)
            {
                if (arr.Length <= i)
                {
                    break;
                }
                var a = arr[i];
                if (a == null)
                {
                    continue;
                }

                string t = i < 3
                    ? $"{i * stepInMillseconds}~{(i + 1) * stepInMillseconds}"
                    : $">{i * stepInMillseconds}";

                sb.AppendLine(string.Format("{0},{1}", t, string.Join(",", indexArr.Select(j => $"{a[j]}({a[j] * 100.0 / sums[j]:F1}%)"))));


            }
            #endregion




            return (detailData, sb.ToString());

        }

        public static void GenerateReportDataFiles(IEnumerable<AiTest.SampleResult> source,
            int step,
            int responseDistributionStep,
            string reportDataPath)
        {
            if (!Directory.Exists(reportDataPath))
            {
                Directory.CreateDirectory(reportDataPath);
            }

            var text = GetTimeSeriesData(source, step);
            var fileName = Path.Combine(reportDataPath, "走势数据.csv");
            File.WriteAllText(fileName, text, Encoding.UTF8);

            text = ReportHelper.CreateAggregateReport(source);
            fileName = Path.Combine(reportDataPath, "聚合报告.csv");
            File.WriteAllText(fileName, text, Encoding.UTF8);

            var tuple = ReportHelper.GetResponseDistributionData(source, responseDistributionStep);
            fileName = Path.Combine(reportDataPath, "响应时间分布.csv");
            File.WriteAllText(fileName, tuple.detailed, Encoding.UTF8);

            fileName = Path.Combine(reportDataPath, "响应时间分布（精简版）.csv");
            File.WriteAllText(fileName, tuple.condensed, Encoding.UTF8);

            MessageBox.Show("ok，操作已完成！", "提示");
            GC.Collect(0);
        }

    }
}
