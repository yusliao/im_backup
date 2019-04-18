using IMClient.ViewModels;
using IMModels;
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
    /// GroupNoticeTipWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GroupNoticeTipWindow : Window
    {
        public GroupNoticeTipWindow()
        {
            InitializeComponent();
        }
        public GroupNoticeTipWindow(MessageModel msgModel) : this()
        {
            if (AppData.tipWindow != null)
                AppData.tipWindow.Close();
            AppData.tipWindow = new GroupNoticeTipWindow();
            AppData.tipWindow.DataContext = msgModel;
            AppData.tipWindow.MouseLeftButtonDown += TipWin_MouseLeftButtonDown;
            AppData.tipWindow.Closed += TipWin_Closed;
            AppData.tipWindow.Loaded += TipWin_Loaded;
            AppData.tipWindow.Owner = App.Current.MainWindow;
            AppData.tipWindow.ShowDialog();
        }
        public GroupNoticeTipWindow(MessageModel msgModel, bool isClick) : this()
        {
            if (isClick)
            {
                this.DataContext = msgModel;
                this.MouseLeftButtonDown += GroupNoticeTipWindow_MouseLeftButtonDown;
                this.Loaded += GroupNoticeTipWindow_Loaded;
                this.Owner = App.Current.MainWindow;
            }

        }

        private void GroupNoticeTipWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Activate();
        }

        private void GroupNoticeTipWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        public void CloseCurrentTipWindow()
        {
            AppData.tipWindow = null;
        }

        private void TipWin_Loaded(object sender, RoutedEventArgs e)
        {
            AppData.tipWindow.Activate();
        }

        private void TipWin_Closed(object sender, EventArgs e)
        {
            AppData.tipWindow = null;
        }

        private void TipWin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AppData.tipWindow.DragMove();
        }

        private void gridLayout_Click(object sender, RoutedEventArgs e)
        {
            Button btn = e.Source as Button;
            string uid = btn.Uid;
            switch (uid)
            {
                case "Cancel":
                case "Close":
                case "Sure":
                    this.Close();
                    break;

            }
        }

        //private void btnAgain_Click(object sender, RoutedEventArgs e)
        //{
        //    if (this.DataContext is MessageModel messageModel)
        //    {
        //        if (AppData.CanInternetAction())
        //        {
        //            this.grid_Offline.Visibility = Visibility.Collapsed;
        //            this.gridLayout.Visibility = Visibility.Visible;
        //        }
        //        else
        //        {
        //            this.grid_Offline.Visibility = Visibility.Visible;
        //            this.gridLayout.Visibility = Visibility.Collapsed;
        //        }
        //    }

        //}
    }
}
