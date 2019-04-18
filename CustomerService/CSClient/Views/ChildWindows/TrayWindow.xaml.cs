using CSClient.ViewModels;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CSClient.Views.ChildWindows
{
    /// <summary>
    /// TrayWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TrayWindow : Window
    {
        public TrayWindow()
        {
            InitializeComponent();
        }

        private void btnIgnoreAllNewMessages_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            AppData.MainMV.ChatListVM.IgnoreAllNewMessages();
        }

        private void border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (App.Current.MainWindow.WindowState == WindowState.Minimized)
            {
                App.Current.MainWindow.WindowState = WindowState.Normal;
            }
            App.Current.MainWindow.Visibility = Visibility.Visible;
            App.Current.MainWindow.Activate();

            AppData.MainMV.ChatListVM.SelectedItem = (sender as Border).DataContext as ChatViewModel;
            if (this.newMsgList.Items.Count == 1)
            {
                this.Hide();
            }
        }
    }
}
