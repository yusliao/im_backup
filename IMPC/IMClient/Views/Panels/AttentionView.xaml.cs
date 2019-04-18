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
    /// 关注人View
    /// </summary>
    public partial class AttentionView : UserControl, IView
    {
        UserModel userModel = null;
        /// <summary>
        ///关注人View
        /// </summary>
        public AttentionView(ViewModel vm)
        {
            InitializeComponent();
            AttentionVIewModel avm = vm as AttentionVIewModel;

            userModel = avm.Model as UserModel;

            this.DataContext = this.ViewModel = new UserViewModel(userModel);
            
            this.Loaded += AttentionView_Loaded;
            this.elpHead.MouseLeftButtonUp += ElpHead_MouseLeftButtonUp;
        }

        private void AttentionView_Loaded(object sender, RoutedEventArgs e)
        {
            if (userModel!=null && string.IsNullOrEmpty(userModel.PhoneNumber))
            {
                if (userModel.ID != AppData.Current.LoginUser.ID)
                    SDKClient.SDKClient.Instance.GetUser(userModel.ID);
            }
        }

        private void ElpHead_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ChildWindows.ImageScanWindow.ShowScan(this.ibHead.ImageSource);
        }

        public ViewModel ViewModel { get; private set; }

        private void btnApplyFriend_Click(object sender, RoutedEventArgs e)
        {
            if (!AppData.CanInternetAction())
            {
                return;
            }
            System.Windows.Controls.Primitives.ToggleButton tb = sender as System.Windows.Controls.Primitives.ToggleButton;
            if (tb.IsChecked.HasValue && tb.IsChecked.Value)
            {
                (tb.DataContext as UserViewModel).ApplyFriend(null);
            }
        }
    }
}
