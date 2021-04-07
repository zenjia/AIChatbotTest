using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AiTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            foreach (var arg in e.Args)
            {
                if (arg.EndsWith(".cmx"))
                {
                    this.Properties["FileName"] = arg;
                     
                    break;
                }
            }

            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("异常类型：" + ex.GetType());
            sb.AppendLine("异常信息：");
            sb.AppendLine(ex.Message);
            sb.AppendLine("");
            sb.AppendLine("StackTrace：");
            sb.AppendLine(ex.StackTrace);

            if (ex.InnerException!=null)
            {
                sb.AppendLine("");
                sb.AppendLine("-----------------InnerException-----------------");
                sb.AppendLine("异常类型：" + ex.InnerException.GetType());
                sb.AppendLine("异常信息：");
                sb.AppendLine(ex.InnerException.Message);
                sb.AppendLine("");
                sb.AppendLine("StackTrace：");
                sb.AppendLine(ex.InnerException.StackTrace);
            }

            MessageBox.Show(sb.ToString(), "UnhandledException");
        }
    }
}
