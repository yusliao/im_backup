using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Util;

namespace CSClient.Views.ChildWindows
{
    /// <summary>
    /// DetectNewVersionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DetectNewVersionWindow : Window
    {
        public DetectNewVersionWindow(bool hasNewVersion, string version)
        {
            InitializeComponent();

            this.tbHint.Text = hasNewVersion ? "发现新版本！" : "此程序为最新版本！";
            this.pnlHasNewVersion.Visibility = hasNewVersion ? Visibility.Visible : Visibility.Collapsed;
            this.pnlHasNotNewVersion.Visibility = hasNewVersion ? Visibility.Collapsed : Visibility.Visible;
            int v = Math.Abs(version.ToInt() - 65);
            this.tbVersion.Text = string.Format("V1.1.{0}", v);
            this.Owner = App.Current.MainWindow;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ButtonBase btn = e.Source as Button;
            switch (btn.Uid)
            {
                default:
                case "CANCEL":
                    this.DialogResult = false;
                    break;
                case "SURE":
                    this.DialogResult = true;
                    System.Diagnostics.Process.Start(SDKClient.SDKProperty.LaunchObj);

                    (App.Current as App).ApplicationExit(null, null);
                    SDKClient.SDKClient.Instance.StopAsync().ConfigureAwait(false);
                    Application.Current?.Shutdown(0);
                    break;
            }
        }
    }
}
