using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
using IMModels;

namespace IMClient.Views.ChildWindows
{
    /// <summary>
    /// 创建群 的交互逻辑
    /// </summary>
    public partial class GroupMemberDealWindow : Window
    {
        IEnumerable<UserModel> _datas;
        private GroupMemberDealWindow(IEnumerable<UserModel> datas)
        {
            InitializeComponent();
            this.Top = -100;
            this.Owner = App.Current.MainWindow;

            this.listSource.ItemsSource =_datas= datas;
            
            UpdateGroupBy();
            this.Loaded += FriendView_Loaded;
            this.Unloaded += FriendView_Unloaded;
            //ViewModels.MainViewModel.CloseAppend?.Invoke();
        }

        private void FriendView_Loaded(object sender, RoutedEventArgs e)
        {
            this.MouseDown += MainWindow_MouseDown;
        }

        private void FriendView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.MouseDown -= MainWindow_MouseDown;
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource != this.tbKey)
            {
                //FriendViewModel vm = this.DataContext as FriendViewModel;
                //vm.ChangedFriendNickNameCommand.Execute(this.tbNickName.Text);
                this.btnCancel.Focus();
            }
        }

        private void UpdateGroupBy()
        {
            ICollectionView cv = CollectionViewSource.GetDefaultView(this.listSource.ItemsSource);
            if (cv == null)
            {
                return;
            }
            
            cv.SortDescriptions.Clear();
            cv.SortDescriptions.Add(new SortDescription("FirstChar", ListSortDirection.Ascending));
            cv.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));

            cv.GroupDescriptions.Clear();
            cv.GroupDescriptions.Add(new PropertyGroupDescription("GroupByChar")); 
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button btn && btn.DataContext != null)
            {
                this.listSource.SelectedItems.Remove(btn.DataContext);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ButtonBase btn = e.Source as Button;
            switch (btn.Uid)
            {
                default:
                case "CANCEL":
                    this.DialogResult = false;
                    App.Current.MainWindow.Focus();
                    break;
                case "SURE":
                    this.DialogResult = true;
                    break;
            }
        }

        private void tbKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.tbKey.Text))
            {
                this.listSearch.Visibility = Visibility.Collapsed;
                return;
            }

            string key = this.tbKey.Text.Trim();
            var searchs = this._datas.Where(info => info.DisplayName.Contains(key));
            this.listSearch.ItemsSource = searchs;
            this.listSearch.Visibility = Visibility.Visible;
            //this.listSearch.Visibility = searchs.Count() > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 启动
        /// </summary> 
        /// <returns></returns>
        public static IEnumerable<UserModel> ShowInstance(string title, IEnumerable<UserModel> source)
        {
            GroupMemberDealWindow win = new GroupMemberDealWindow(source);
            win.Title = title;
            win.tbTitle.Text = title; 
            bool? result = win.ShowDialog();
            List<UserModel> results = new List<UserModel>();
            foreach (var user in source)
            {
                user.IsLock = false;
                if (user.IsSelected)
                {
                    user.IsSelected = false;
                    results.Add(user);
                }
            }

            if (result == true)
            { 
                return results;
            }
            else
            {
                return null;
            }
        }

        private void listSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.btnConfirm.IsEnabled = this.listSource.SelectedItems.Count > 0 ? true : false;
        }
    }
}
