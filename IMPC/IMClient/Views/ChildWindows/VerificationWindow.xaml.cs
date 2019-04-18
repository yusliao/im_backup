using IMClient.ViewModels;
using IMModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// VerificationWindow.xaml 的交互逻辑
    /// </summary>
    public partial class VerificationWindow : Window
    {
        public VerificationWindow(UserViewModel userModel)
        {
            InitializeComponent();
            this.Owner = App.Current.MainWindow;
            //completedGrid.Visibility = Visibility.Collapsed;


        }
        private bool dialogResult = true;

        public static bool ShowInstance(UserViewModel userModel)
        {
            VerificationWindow win = new VerificationWindow(userModel);
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            //win.Topmost = true;
            win.DataContext = new SetVerificationViewModel(userModel);
            win.Owner = App.Current.MainWindow;
            //box.tbTitle.Text = title;
            //box.runContent.Text = msg;
            //box.btnSure0.Content = sureText;
            win.ShowDialog();
            return win.dialogResult;
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.dialogResult = false;
            this.Close();
        }

        private void btnSure_Click(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(200);
            this.dialogResult = true;
            this.Close();
        }

        private void btnCompleted_Click(object sender, RoutedEventArgs e)
        {
            //if (AppData.CanInternetAction(""))
            //    completedGrid.Visibility = Visibility.Visible;
        }
    }
}
