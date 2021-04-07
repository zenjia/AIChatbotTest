using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AiTest.Utils;
using AiTest.View;
using AiTest.ViewModel;
using ControlLibrary1.SampleResult;
using IniParser;
using IniParser.Model;
using Microsoft.Win32;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using ViewLibrary.View;
using Path = System.IO.Path;


namespace AiTest
{
    public class UIBlockDetector
    {
        static Timer _timer;
        public UIBlockDetector(int maxFreezeTimeInMilliseconds = 200)
        {
            var sw = new Stopwatch();

            new DispatcherTimer(TimeSpan.FromMilliseconds(10), DispatcherPriority.Send, (sender, args) =>
            {
                lock (sw)
                {
                    sw.Restart();
                }

            }, Application.Current.Dispatcher);

            _timer = new Timer(state =>
            {
                lock (sw)
                {
                    if (sw.ElapsedMilliseconds > maxFreezeTimeInMilliseconds)
                    {
                        //Debugger.Break();
                        // Debugger.Break() or set breakpoint here;
                        // Goto Visual Studio --> Debug --> Windows --> Theads 
                        // and checkup where the MainThread is.
                    }
                }

            }, null, TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(10));

        }

    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _title2;
        public string Title2
        {
            get { return _title2; }
            set
            {
                if (_title2 != value)
                {
                    _title2 = value;
                    OnPropertyChanged();


                }

            }
        }

        private TestPlan _testPlan;
        public TestPlan TestPlan
        {
            get { return _testPlan; }
            set
            {
                if (_testPlan != value)
                {
                    _testPlan = value;
                    OnPropertyChanged();


                }

            }
        }

        private int _startDelay;
        public int StartDelay
        {
            get { return _startDelay; }
            set
            {
                if (_startDelay != value)
                {
                    _startDelay = value;
                    OnPropertyChanged();

                }

            }
        }
        private static string GetAppDataPath()
        {
            String path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appDataPath = Path.Combine(path, "AIChatbotTest");

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            return appDataPath;
        }

        private static string GetSettingFileName()
        {

            var filename = Path.Combine(GetAppDataPath(), "settings.ini");

            return filename;
        }

        public void SaveSettings()
        {
            var parser = new FileIniDataParser();
            IniData data = new IniData(); //parser.ReadFile(configFilename);

            data["path"]["LastTestPlanPath"] = this.TestPlan?.TestPlanPath;

            parser.WriteFile(GetSettingFileName(), data);
        }

        private async Task<bool> TryLoadFromeFile()
        {
            if (!Application.Current.Properties.Contains("FileName"))
            {
                return false;
            }

            var fileName = Application.Current.Properties["FileName"];

            var path = Path.GetDirectoryName(fileName.ToString());

            if (!Directory.Exists(path))
            {
                return false;
            }

            var testPlan = new TestPlan();
            testPlan.LoadTestPlan(path);
            await testPlan.LoadTestData();
            this.TestPlan = testPlan;

            return true;
        }

        public async Task LoadSettings()
        {
            bool bo = await TryLoadFromeFile();
            if (bo)
            {
                return;
            }

            var fileName = GetSettingFileName();
            if (!File.Exists(fileName))
            {
                return;

            }

            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(fileName);
            var lastTestPlanPath = data["path"]["LastTestPlanPath"];

            if (Directory.Exists(lastTestPlanPath))
            {
                var testPlan = new TestPlan();
                testPlan.LoadTestPlan(lastTestPlanPath);
                await testPlan.LoadTestData();
                this.TestPlan = testPlan;

            }


        }


        private bool _canBegin = true;
        public bool CanBegin
        {
            get { return this._canBegin; }
            set
            {
                if (this._canBegin != value)
                {
                    this._canBegin = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _canCancel;
        public bool CanCancel
        {
            get { return this._canCancel; }
            set
            {
                if (this._canCancel != value)
                {
                    this._canCancel = value;
                    OnPropertyChanged();
                }

            }
        }

        private TimeSpan _timeEllapsed;
        public TimeSpan TimeElapsed
        {
            get { return this._timeEllapsed; }
            set
            {
                if (this._timeEllapsed != value)
                {
                    this._timeEllapsed = value;
                    OnPropertyChanged();
                }

            }
        }

        private DispatcherTimer timer = new DispatcherTimer();
        public MainWindow()
        {
            this.Loaded += MainWindow_Loaded;
            InitializeComponent();

            timer.Interval = new TimeSpan(0, 0, 1);
            this.timer.Tick += Timer_Tick;


        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            await LoadSettings();

            if (!FileAssociation.IsAssociated(".cmx"))
            {
                //System.Reflection.Assembly.GetExecutingAssembly().Location
                //Process.GetCurrentProcess().MainModule.FileName
                var exeFileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var exePath = Path.GetDirectoryName(exeFileName);
                var iconFileName = Path.Combine(exePath, "favicon.ico");
                FileAssociation.Associate(".cmx",
                    "AiTest",
                    "cmx File",
                    iconFileName,
                    exeFileName);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.TimeElapsed = this.TimeElapsed.Add(timer.Interval);
        }


        protected override void OnClosing(CancelEventArgs e)
        {

            base.OnClosing(e);
            if (this.TestPlan != null && this.TestPlan.IsDataChanged)
            {

                var ret = MessageBox.Show($"当前测试计划的设置已经改变，是否保存？", "提示", MessageBoxButton.YesNoCancel);
                switch (ret)
                {
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        return;

                    case MessageBoxResult.Yes:
                        this.TestPlan.SaveTestPlan();
                        //this.TestPlan.SaveTestData();
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }

            SaveSettings();


        }

        private async void Begin_OnClick(object sender, RoutedEventArgs e)
        {
            //timer2 = new UIBlockDetector(100);
           

            this.CanBegin = false;
            this.CanCancel = true;

            await Task.Delay(this.StartDelay * 60 * 1000);

            this.TimeElapsed = new TimeSpan(0, 0, 0);
            this.timer.Start();
            try
            {
                await this.TestPlan.Begin();
                SystemSounds.Beep.Play();
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("操作已取消！");
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("异常类型：" + ex.GetType());
                sb.AppendLine("异常信息：");
                sb.AppendLine(ex.Message);
                sb.AppendLine("");
                sb.AppendLine("StackTrace：");
                sb.AppendLine(ex.StackTrace);
                MessageBox.Show(sb.ToString(), "出错了");
            }
            finally
            {
                this.CanBegin = true;
                this.CanCancel = false;
                this.timer.Stop();

                GC.Collect(0);

            }


        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.TestPlan.Cancel();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region 文件操作
        /// <summary>
        /// 
        /// </summary>
        /// <returns>false-canceled</returns>
        private bool SaveCurrentTestPlan()
        {
            if (this.TestPlan == null || !this.TestPlan.IsDataChanged)
            {
                return true;
            }

            var ret = MessageBox.Show("当前测试计划数据已改变，是否保存？", "提示", MessageBoxButton.YesNoCancel);

            switch (ret)
            {
                case MessageBoxResult.Cancel:
                    return false;

                case MessageBoxResult.Yes:
                    this.TestPlan.SaveTestPlan();
                    //this.TestPlan.SaveTestData();
                    break;
                case MessageBoxResult.No:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            return true;
        }

        private void BtnSelectUserDataFile_OnClick(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog openFileDialog = new VistaOpenFileDialog();
            openFileDialog.Title = "选择测试用数据文件";
            if (openFileDialog.ShowDialog(this) == true)
            {
                var selectedFile = openFileDialog.FileName;
                try
                {

                    this.TestPlan.WechatAppThreadGroup.Config.UpdateUserDataFileName(openFileDialog.FileName);
                }
                catch (Exception exception)
                {
                    MessageBox.Show("非法csv文件：" + selectedFile);
                }

            }

        }

        private void BtnNewTestPlan_OnClick(object sender, RoutedEventArgs e)
        {
            if (!SaveCurrentTestPlan())
            {
                return;
            }

            VistaFolderBrowserDialog browserDialog = new VistaFolderBrowserDialog();
            browserDialog.Description = "选择测试计划文件夹";
            if (browserDialog.ShowDialog() == true)
            {
                if (File.Exists(Path.Combine(browserDialog.SelectedPath, $"{TestPlanBase.DefaultTestPlanFileName}.cmx")))
                {
                    var ret = MessageBox.Show("测试计划文件已存在，是否覆盖？", "提示", MessageBoxButton.OKCancel);

                    if (ret == MessageBoxResult.Cancel)
                    {
                        MessageBox.Show("操作已取消！");
                        return;
                    }
                }

                TestPlan testPlan = new TestPlan();
                testPlan.CreateDefaultThreadGroup();
                testPlan.TestPlanPath = browserDialog.SelectedPath;
                this.TestPlan = testPlan;
            }

        }

        private async void BtnLoadTestPlan_OnClick(object sender, RoutedEventArgs e)
        {
            if (!SaveCurrentTestPlan())
            {
                return;
            }

            VistaFolderBrowserDialog browserDialog = new VistaFolderBrowserDialog();
            browserDialog.Description = "选择测试计划文件夹";
            if (browserDialog.ShowDialog() == true)
            {
                if (!File.Exists(Path.Combine(browserDialog.SelectedPath, $"{TestPlanBase.DefaultTestPlanFileName}.cmx")))
                {
                    var ret = MessageBox.Show("测试计划文件不存在，是否新建？", "提示", MessageBoxButton.OKCancel);

                    if (ret == MessageBoxResult.Cancel)
                    {
                        MessageBox.Show("操作已取消！");
                        return;
                    }

                }

                this.TestPlan = new TestPlan();
                this.TestPlan.LoadTestPlan(browserDialog.SelectedPath);
                await this.TestPlan.LoadTestData();
            }
        }

        private void BtnSaveTestPlanAs_OnClick(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog browserDialog = new VistaFolderBrowserDialog();
            browserDialog.Description = "选择测试计划文件夹";
            if (browserDialog.ShowDialog() == true)
            {
                this.TestPlan.TestPlanPath = browserDialog.SelectedPath;
                this.TestPlan.SaveTestPlan();
                this.TestPlan.SaveTestData();
            }
        }

        private void BtnSaveTestPlan_OnClick(object sender, RoutedEventArgs e)
        {
            this.TestPlan.SaveTestPlan();
            this.TestPlan.SaveTestData();
        }

        #endregion

        private void BtnClearTestData_OnClick(object sender, RoutedEventArgs e)
        {
            this.TestPlan.Clear();
        }

        private void SaveTextToFile(string text, string suggestedFileName)
        {
            VistaSaveFileDialog saveFileDialog = new VistaSaveFileDialog();
            saveFileDialog.DefaultExt = ".csv";
            saveFileDialog.Title = "选择文件";
            saveFileDialog.FileName = suggestedFileName;

            if (saveFileDialog.ShowDialog() == true)
            {
                var fileName = saveFileDialog.FileName;
                File.WriteAllText(fileName, text, Encoding.UTF8);
                MessageBox.Show($"OK! 文件已保存到{fileName}", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
         
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Button btn = (sender as Button);
            btn.ContextMenu.IsOpen = true;
        }


        public LogType[] LogTypes { get; } = new LogType[] { LogType.Info, LogType.Warn, LogType.Error };


        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            SampleResultStat item = (sender as FrameworkElement).DataContext as SampleResultStat;

            if (item.ErrorSampleResults.Any())
            {


                var view = new SampleResultView();

                view.ItemsSource = item.ErrorSampleResults.ToArray();

                Window window = new Window();
                window.Title = $"Error Sample Results{item.Label}）";
                window.Content = view;
                window.ShowDialog();
            }

        }

        private void btnOpenFolder_OnClick(object sender, RoutedEventArgs e)
        {
            Process process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = this.TestPlan.TestPlanPath;
            process.Start();

        }

        private void NCopyTestTime_OnClick(object sender, RoutedEventArgs e)
        {
            if (!this.TestPlan.WechatAppThreadGroup.SampleResults.Items.Any())
            {
                MessageBox.Show("无法进行此操作：无测试结果数据！", "提示");
                return;
            }

            var text = this.TestPlan.WechatAppThreadGroup.SampleResults.GetTestStartEndTime();

            Clipboard.SetText(text);

            MessageBox.Show($"已复制到粘贴板：{text}", "提示");
        }

        private void NCreateReportData_OnClick(object sender, RoutedEventArgs e)
        {
            if (!this.TestPlan.WechatAppThreadGroup.SampleResults.Items.Any())
            {
                MessageBox.Show("无法进行此操作：无测试结果数据！", "提示");
                return;
            }

          
             
            CreateReportDialog dialog = new CreateReportDialog();

            dialog.ShowDialog();
            if (!dialog.IsAccepted)
            {
                MessageBox.Show("操作已取消！", "提示");
                return;
            }

            ReportCreatingOptions options = dialog.ReportCreatingOptions;

            IEnumerable<SampleResult> items = this.TestPlan.WechatAppThreadGroup.SampleResults.Items;
            if (options.OnlyIncludeMaxThreadCountSamples)
            {
                var maxThreadCount = items.Max(x=>x.ActiveThreadCount);
                items = items.Where(x => x.ActiveThreadCount == maxThreadCount).ToArray();
            }

            var reportDataPath = Path.Combine(this.TestPlan.TestPlanPath, "report");
            
            int i = 2;
            while (Directory.Exists(reportDataPath))
            {
                reportDataPath = Path.Combine(this.TestPlan.TestPlanPath, $"report_{i}");
                i++;
            }
              
            ReportHelper.GenerateReportDataFiles(items,
                options.TimeSeriesStepInMilliseconds, options.RtdDataStepInMilliseconds, reportDataPath );

            Process process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = reportDataPath;
            process.Start();

            GC.Collect(0);
        }
    }
}
