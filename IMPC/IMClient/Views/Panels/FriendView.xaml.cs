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
using IMClient.ViewModels;
using IMModels;

namespace IMClient.Views.Panels
{
    /// <summary>
    /// 好友View
    /// </summary>
    public partial class FriendView : UserControl, IView
    {
        /// <summary>
        /// 好友信息View
        /// </summary>
        public FriendView(ViewModel vm)
        {
            InitializeComponent();
            this.DataContext = this.ViewModel = vm;
            if (vm.Model is UserModel userModel && string.IsNullOrEmpty(userModel.KfNum))
            {
                if (userModel.ID != AppData.Current.LoginUser.ID)
                    SDKClient.SDKClient.Instance.GetUser(userModel.ID);
            }
            this.tbNickName.KeyDown += TbNickName_KeyDown;

            this.Loaded += FriendView_Loaded;
            this.Unloaded += FriendView_Unloaded;
        }

        private void FriendView_Loaded(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.MouseDown += MainWindow_MouseDown;
        }

        private void FriendView_Unloaded(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.MouseDown -= MainWindow_MouseDown;
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource != this.tbNickName)
            {
                //FriendViewModel vm = this.DataContext as FriendViewModel;
                //vm.ChangedFriendNickNameCommand.Execute(this.tbNickName.Text);
                this.btnSend.Focus();
            }
        }

        private void TbNickName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //FriendViewModel vm = this.DataContext as FriendViewModel;
                //vm.ChangedFriendNickNameCommand.Execute(this.tbNickName.Text);

                this.btnSend.Focus();
            }
        }

        public ViewModel ViewModel { get; private set; }

        private void Ellipse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ChildWindows.ImageScanWindow.ShowScan(this.ibHead.ImageSource);
        }


    }
}
