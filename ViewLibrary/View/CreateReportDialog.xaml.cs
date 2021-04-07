using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AiTest.Utils;

namespace ViewLibrary.View
{
    public class ReportCreatingOptions : BindableBase
    {
        private bool _onlyIncludeMaxThreadCountSamples = false;
        public bool OnlyIncludeMaxThreadCountSamples
        {
            get { return _onlyIncludeMaxThreadCountSamples; }
            set { SetProperty(ref _onlyIncludeMaxThreadCountSamples, value); }
        }


        private int _timeSeriesStepInMilliseconds = 2000;
        public int TimeSeriesStepInMilliseconds
        {
            get { return _timeSeriesStepInMilliseconds; }
            set { SetProperty(ref _timeSeriesStepInMilliseconds, value); }
        }
        private int _rtdDataStepInMilliseconds = 1000;
        public int RtdDataStepInMilliseconds
        {
            get { return this._rtdDataStepInMilliseconds; }
            set { SetProperty(ref this._rtdDataStepInMilliseconds, value); }
        }
    }

    /// <summary>
    /// Interaction logic for CreateReportDialog.xaml
    /// </summary>
    public partial class CreateReportDialog : Window
    {
        public ReportCreatingOptions ReportCreatingOptions { get; }
 
        public bool IsAccepted { get; private set; }
        public CreateReportDialog()
        {
            ReportCreatingOptions = new ReportCreatingOptions();
            InitializeComponent();
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            IsAccepted = true;
            Close();

        }
    }
}
