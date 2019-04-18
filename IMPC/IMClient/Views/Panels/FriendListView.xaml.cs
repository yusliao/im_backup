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
using IMClient.Helper;

namespace IMClient.Views.Panels
{
    /// <summary>
    /// 好友列表
    /// </summary>
    public partial class FriendListView : UserControl, IListView
    {
        //List<char> _chars = new List<char>();

        public FriendListView()
        {
            InitializeComponent();
            this.DataContext = this.ViewModel = new FriendListViewModel(this);

            this.Loaded += View_Loaded;
            //LoadChars();

            this.list.PreviewMouseRightButtonDown += (s, e) => { e.Handled = true; };
            //this.searchResults.PreviewMouseRightButtonDown += (s, e) => { e.Handled = true; };
        }

        //private void LoadChars()
        //{ 
        //    for(int i = 0; i < 26; i++)
        //    {
        //        _chars.Add((char) (65+i));
        //    }
        //    _chars.Add('#');

        //    this.listChar.ItemsSource = _chars;
        //}

        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.list.SelectedItem != null)
            {
                this.list.ScrollIntoView(this.list.SelectedItem);
            }
            if (AppData.MainMV.FriendListVM.ApplyUserListVM.Items?.Count < SDKClient.SDKClient.Instance.property.FriendApplyList.Count)
                AppData.MainMV.FriendListVM.ApplyUserListVM.LoadDatas();
        }

        public ViewModel ViewModel { get; private set; }

        public IView GetItemView(ViewModel vm)
        {
            return new FriendView(vm);
        }

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is FrameworkElement fe && fe.DataContext is char c)
            {
                var vm = this.ViewModel as FriendListViewModel;

                var group = vm.GroupViews.FirstOrDefault(info => info.Name.Equals(c));

                if (group != null)
                {
                    //实际上用Invoke里面的内容就可以
                    //但因为当group的子项数量跨度大于当前窗口可容纳高度时，
                    // 且是从底部往上滚动时，会造成未滚动到当前char位置，只是滚动到group最后一个子项的位置
                    //
                    this.list.ScrollIntoView(vm.GroupViews[0]);

                    Task.Run(new Action(() =>
                    {
                        System.Threading.Thread.CurrentThread.Join(1);
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            this.list.ScrollIntoView(group);

                            if (group.Items.FirstOrDefault() is FriendViewModel friend)
                            {
                                friend.IsActive = false;
                                friend.IsActive = true;
                            }
                        }));
                    }));

                }
            }
        }
    }
}
