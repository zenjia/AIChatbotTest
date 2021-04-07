using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

namespace AiTest.View
{
    /// <summary>
    /// Interaction logic for LogView.xaml
    /// </summary>
    public partial class LogView : UserControl
    {
        public IList<LogItem> ItemsSource
        {
            get { return (IList<LogItem>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IList<LogItem>), typeof(LogView), new PropertyMetadata(null));

        public LogView()
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

            LogItem item = (sender as FrameworkElement).DataContext as LogItem;

            var view = new LogView();
            view.isInMainWindow = false;
            view.ItemsSource = this.ItemsSource.Where(x => x.ThreadId == item.ThreadId).OrderBy(x => x.TimeStamp).ToArray();

            Window window = new Window();
            window.Title = $"线程{item.ThreadId}测试日志";
            window.Content = view;
            window.ShowDialog();
        }



        private void ListBox_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var item = this.ListBox.SelectedItems.OfType<LogItem>().SingleOrDefault();
            if (item == null)
            {
                return;
            }

            ContextMenu contextMenu = new ContextMenu();
            MenuItem mi = new MenuItem();
            mi.Header = "复制";
            mi.Click += (o, ee) =>
            {
                Clipboard.SetText(item.Message);
                MessageBox.Show("ok. 复制成功！", "提示");
            };
            contextMenu.Items.Add(mi);
            contextMenu.IsOpen = true;

        }
    }
}
