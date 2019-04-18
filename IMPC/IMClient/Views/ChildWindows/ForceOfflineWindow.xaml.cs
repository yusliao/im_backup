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

namespace IMClient.Views.ChildWindows
{
    /// <summary>
    /// ForceOfflineWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ForceOfflineWindow : Window
    {
        public ForceOfflineWindow()
        {
            InitializeComponent();

            this.Owner = App.Current.MainWindow;

            
            //var context = TaskScheduler.FromCurrentSynchronizationContext();
            //Task.Delay(10000).ContinueWith(t =>
            //{
            //    this.Close();
            //}, context);
        }
        public ForceOfflineWindow(string msg):this()
        {
            if (string.IsNullOrEmpty(msg))
                this.txtHint.Text = string.Format("    您的账号于 {0} 在另一台电脑登录。若非本人操作，您的账号密码可能已泄露，请在手机移动端更改您的登录密码。", DateTime.Now.ToString("HH:mm"));
            else
                this.txtHint.Text = msg;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
