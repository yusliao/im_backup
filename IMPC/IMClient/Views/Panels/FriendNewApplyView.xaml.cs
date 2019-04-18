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
    /// 新的好友申请View
    /// </summary>
    public partial class FriendNewApplyView : UserControl, IListView
    {
        FriendApplyListViewModel _applyListVM;
        /// <summary>
        /// 新的好友申请View
        /// </summary>
        public FriendNewApplyView()
        {
            InitializeComponent(); 
            this.DataContext = this.ViewModel =AppData.MainMV.FriendListVM.ApplyUserListVM= _applyListVM = new FriendApplyListViewModel(this);
            this.Loaded += FriendNewApplyView_Loaded;
            this.Unloaded += FriendNewApplyView_Unloaded;
        }

        private void FriendNewApplyView_Unloaded(object sender, RoutedEventArgs e)
        {
            _applyListVM.UpdateApplyCount(true);
        }

        private void FriendNewApplyView_Loaded(object sender, RoutedEventArgs e)
        {
            //_applyListVM.LoadDatas();
            _applyListVM.UpdateApplyCount(true);
        }

        public ViewModel ViewModel { get; private set; }

        public IView GetItemView(ViewModel vm)
        {
            return null;
        }
    } 
}
