using IMClient.ViewModels;
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

namespace IMClient.Views 
{
    /// <summary>
    /// LogoutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MessageBox : Window
    {
        private MessageBox()
        {
            InitializeComponent();

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
                case "CLOSE":
                case "CANCEL":
                    this.DialogResult = false;
                    break;
                case "SURE":
                    this.DialogResult = true; 
                    break;
            }
        }

        /// <summary>
        /// 显示信息提示框
        /// </summary>
        /// <param name="msg">提示内容</param>
        /// <param name="title">提示标题</param>
        /// <returns></returns>
        public static bool ShowDialogBox(string msg,string title="提示",string sureText="确定",bool isCancelShow=true)
        {
            MessageBox box = new MessageBox();
            box.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            //box.Topmost = true;
            box.tbTitle.Text = title;
            box.runContent.Text = msg;
            box.btnSure0.Content = sureText;
            box.Owner = App.Current.MainWindow;
            if (!isCancelShow)
                box.btnCancel.Visibility = Visibility.Collapsed;
            return box.ShowDialog() == true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg">提示内容</param>
        /// <param name="type">提示内容的类别</param>
        /// <param name="title"></param>
        /// <param name="sureText"></param>
        /// <param name="isCancelShow"></param>
        /// <returns></returns>
        public static bool ShowDialogBoxEx(string msg,string type, string title = "提示", string sureText = "确定", bool isCancelShow = true)
        {
            MessageBox box = new MessageBox();
            box.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            box.Topmost = true;
            box.tbTitle.Text = title;
            box.runContent.Text =string.Format(msg,type);
            box.btnSure0.Content = sureText;
            box.Owner = App.Current.MainWindow;
            if (!isCancelShow)
                box.btnCancel.Visibility = Visibility.Collapsed;
            return box.ShowDialog() == true;
        }

        /// <summary>
        /// 显示信息提示框
        /// </summary>
        /// <param name="msg">提示内容</param>
        /// <param name="title">提示标题</param>
        /// <returns></returns>
        public static void ShowBox(string msg, string title = "提示")
        {
            MessageBox box = new MessageBox();
            box.tbTitle.Text = title;
            box.runContent.Text = msg;

            box.btnCancel.Visibility = box.btnSure0.Visibility = Visibility.Collapsed;
            box.btnSure1.Visibility = Visibility.Visible;
            box.ShowDialog();
        }
    }
}
