using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AiTest.View
{
    /// <summary>
    /// Interaction logic for SampleResultView.xaml
    /// </summary>
    public partial class SampleResultView : UserControl 
    {
         
        public SampleResult SelectedSampleResult
        {
            get { return (SampleResult)GetValue(SelectedSampleResultProperty); }
            set { SetValue(SelectedSampleResultProperty, value); }
        }
        public static readonly DependencyProperty SelectedSampleResultProperty =
            DependencyProperty.Register("SelectedSampleResult", typeof(SampleResult), typeof(SampleResultView), new PropertyMetadata(null));
         

        public IList<SampleResult> ItemsSource
        {
            get { return (IList<SampleResult>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IList<SampleResult>), typeof(SampleResultView), new PropertyMetadata(null));

         
        public SampleResultView()
        {
            InitializeComponent();
        }


        private bool isInMainWindow = true;
        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (!this.isInMainWindow)
            {
                return;
            }

            SampleResult item = (sender as FrameworkElement).DataContext as SampleResult;


            var view = new SampleResultView();
            view.isInMainWindow = false;
            view.ItemsSource = this.ItemsSource.Where(x => x.ThreadId == item.ThreadId).OrderBy(x => x.TimeStamp).ToArray();

            Window window = new Window();
            window.Title = $"线程{item.ThreadId}测试结果";
            window.Content = view;
            window.ShowDialog();


        }
    }
}
