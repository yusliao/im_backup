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
using IMClient.Views.ChildWindows;

namespace IMClient.Views.Panels
{
    /// <summary>
    /// 聊天框View
    /// </summary>
    public partial class GroupView : UserControl,IView
    {
        /// <summary>
        /// 群组信息 View
        /// </summary>
        public GroupView(ViewModel vm)
        {
            InitializeComponent();
            this.DataContext = this.ViewModel = vm;

            this.elpQrCode.MouseLeftButtonUp += ElpQrCode_MouseLeftButtonUp;

        
            Task.Run(() =>
            {
                if(vm.Model is GroupModel group)
                {
                   group.QrCodePath=  SDKClient.SDKClient.Instance.GetQrCodeImg(this.ViewModel.ID.ToString(), "2");
                }
            });
            this.Loaded += FriendView_Loaded;
            this.Unloaded += FriendView_Unloaded;
        } 

        private void FriendView_Loaded(object sender, RoutedEventArgs e)
        { 
            if(this.DataContext is GroupViewModel gvm)
            {
                this.tbRemark.IsHitTestVisible = gvm.IsAdmin || gvm.IsCreator;
            }
            App.Current.MainWindow.MouseDown += MainWindow_MouseDown;
        }

        private void FriendView_Unloaded(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.MouseDown -= MainWindow_MouseDown;
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource != this.tbRemark)
            {
                //FriendViewModel vm = this.DataContext as FriendViewModel;
                //vm.ChangedFriendNickNameCommand.Execute(this.tbNickName.Text);
                this.btnSend.Focus();
            }
        }

        private void ElpQrCode_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel.Model is GroupModel group)
            {
                ChildWindows.ImageScanWindow.ShowScan(group.QrCodePath);
            }
        }

        public ViewModel ViewModel { get; private set; }

        private void btnModifyGroupName_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is GroupViewModel groupVM)
            {
                GroupModel group = groupVM.Model as GroupModel;
                string newValue = EditStringValueWindow.ShowInstance(group.DisplayName, "修改群名称");
                EditStringValueWindow.Win.GroupNameEvent -= EditStringValueWindow_GroupNameEvent;
                EditStringValueWindow.Win.GroupNameEvent += EditStringValueWindow_GroupNameEvent;


            }
        }

        private void EditStringValueWindow_GroupNameEvent(object obj)
        {
            if (this.DataContext is GroupViewModel groupVM)
            {
                GroupModel group = groupVM.Model as GroupModel;
                groupVM.ChangedGroupNameCommand?.Execute(obj);
            }
        }
    }
}
