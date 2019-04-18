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

namespace CSClient.Views.ChildWindows
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

            this.txtHint.Text = string.Format("    您的账号于 {0} 在另一台电脑登录。若非本人操作，您的账号密码可能已泄露，请联系技术管理人员！", DateTime.Now.ToString("HH:mm"));
            //var context = TaskScheduler.FromCurrentSynchronizationContext();
            //Task.Delay(10000).ContinueWith(t =>
            //{
            //    this.Close();
            //}, context);
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
