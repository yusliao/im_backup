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
    /// 查找用户View
    /// </summary>
    public partial class UserSearchView : UserControl,IView
    {
        /// <summary>
        /// 查找用户View
        /// </summary>
        public UserSearchView(UserSearchListViewModel vm)
        {
            InitializeComponent();
            this.DataContext = this.ViewModel = vm;

            vm.OnResetScrollViewerVerticalOffset += Vm_OnResetScrollViewerVerticalOffset;
            this.sv.PreviewMouseWheel += Sv_PreviewMouseWheel;
            //点击其他地方，输入框不获取光标，若为空则提示输入
            this.PreviewMouseDown += delegate { this.btnSearch.Focus(); };
        }

        private void Vm_OnResetScrollViewerVerticalOffset()
        {
            this.sv.ScrollToTop();
        }

        private void Sv_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(e.Delta > 0)
            {
                return;
            }
            if(this.sv.VerticalOffset != this.sv.ScrollableHeight)
            {
                return;
            }

            if (this.DataContext is UserSearchListViewModel vm)
            {
                vm.GetData();
            }
        }

        public ViewModel ViewModel { get; private set; } 
    }


    /// <summary>
    /// 查找用户View
    /// </summary>
    public class UserSearchListView : UserControl, IListView
    {
        /// <summary>
        /// 查找用户View
        /// </summary>
        public UserSearchListView()
        {
            UserSearchListViewModel vm = new UserSearchListViewModel(this);
            this.DataContext = this.ViewModel = vm; 
            vm.SelectedItem = new UserViewModel(AppData.Current.LoginUser.User) { View = GetItemView(vm) };
        }

        public ViewModel ViewModel { get; private set; }

        public IView GetItemView(ViewModel vm)
        {
            return new UserSearchView(this.ViewModel as UserSearchListViewModel);
        }
    }
}
