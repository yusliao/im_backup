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
    /// MessageBoxEx.xaml 的交互逻辑
    /// </summary>
    public partial class MessageBoxEx : Window
    {
        public MessageBoxEx()
        {
            InitializeComponent();
            this.Owner = App.Current.MainWindow;
        }
        public MessageBoxEx(string msg, string title = "提示", string sureText = "确定", bool isCancelShow = true):this()
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Topmost = true;
            this.tbTitle.Text = title;
            this.runContent.Text = msg;
            this.btnSure0.Content = sureText;
            if (!isCancelShow)
                this.btnCancel.Visibility = Visibility.Collapsed;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
        private bool dialogResult = false;
        private void Grid_Click(object sender, RoutedEventArgs e)
        {
            Button btn = e.Source as Button;
            string uid = btn.Uid;
            switch (uid)
            {
                case "CLOSE":
                case "CANCEL":
                    this.dialogResult = false;
                    this.Close();
                    break;
                case "SURE":
                    this.dialogResult = true;
                    this.Close();
                    break;
                default:
                    break;
            }
        }
        public static bool ShowInstance(string msg, string title = "提示", string sureText = "确定", bool isCancelShow = true)
        {
            MessageBoxEx box = new MessageBoxEx();
            box.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            box.Topmost = true;
            box.tbTitle.Text = title;
            box.runContent.Text = msg;
            box.btnSure0.Content = sureText;
            if (!isCancelShow)
                box.btnCancel.Visibility = Visibility.Collapsed;
            box.ShowDialog();
            return box.dialogResult;
        }

        public static void ShowMsg(string msg, string title = "提示")
        {
            MessageBoxEx box = new MessageBoxEx();
            box.tbTitle.Text = title;
            box.runContent.Text = msg;

            box.btnCancel.Visibility = box.btnSure0.Visibility = Visibility.Collapsed;
            box.btnSure1.Visibility = Visibility.Visible;
            box.ShowDialog();
        }
    }
}
