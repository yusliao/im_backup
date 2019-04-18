using IMClient.ViewModels;
using IMClient.Views.ChildWindows;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IMClient.Views.Panels
{
    /// <summary>
    /// PrivacyView.xaml 的交互逻辑
    /// </summary>
    public partial class PrivacyView : UserControl, IView
    {
        public PrivacyView()
        {
            InitializeComponent();

            PrivacyViewModel vm = new PrivacyViewModel(new PrivacyModel());
            this.DataContext = this.ViewModel = vm;
        }

        public ViewModel ViewModel { get; private set; }

        private void ListBoxItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.btnState.IsChecked = false;
            this.popupState.IsOpen = false;

            if (!AppData.CanInternetAction())
            {
                return;
            }

            ListBoxItem item = sender as ListBoxItem;
            (this.DataContext as PrivacyViewModel).SelectedReceiveMode = item.DataContext as string;
            (this.DataContext as PrivacyViewModel).ReceiveStrangerMessageMode.Clear();

            string content = string.Empty;
            if((this.DataContext as PrivacyViewModel).SelectedReceiveMode.Equals("接收"))
            {
                (this.DataContext as PrivacyViewModel).ReceiveStrangerMessageMode.Add("不接收");
                AppData.Current.LoginUser.User.IsReceiveStrangerMessage = true;
                content = "1";
            }
            else
            {
                (this.DataContext as PrivacyViewModel).ReceiveStrangerMessageMode.Add("接收");
                AppData.Current.LoginUser.User.IsReceiveStrangerMessage = false;
                content = "0";
            }

            SDKClient.SDKClient.Instance.UpdateMyself(SDKClient.Model.UpdateUserOption.修改是否接收陌生信息, content);
        }

        private void btnOpenBlacklist_Click(object sender, RoutedEventArgs e)
        {
            BlacklistWindow win = new BlacklistWindow();
            win.Owner = App.Current.MainWindow;
            win.DataContext = AppData.MainMV.BlacklistVM;
            win.ShowDialog();
        }
    }
}
