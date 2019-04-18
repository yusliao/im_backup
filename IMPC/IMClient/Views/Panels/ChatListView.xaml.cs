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
using System.ComponentModel;

namespace IMClient.Views.Panels
{
    /// <summary>
    /// 聊天列表
    /// </summary>
    public partial class ChatListView : UserControl, IListView
    {
        public ChatListView()
        {
            InitializeComponent();
            this.DataContext = this.ViewModel = new ChatListViewModel(this);
            (this.DataContext as ChatListViewModel).OnScrollIntoView += ChatListView_OnScrollIntoView;
            this.Loaded += View_Loaded;
            this.list.PreviewMouseRightButtonDown += (s, e) => { e.Handled = true; };
            //this.searchResults.PreviewMouseRightButtonDown += (s, e) => { e.Handled = true; };
        }

        private void ChatListView_OnScrollIntoView()
        {
            ScrollIntoView();
        }

        private void ScrollIntoView()
        {
            if (this.list.SelectedItem != null)
            {
                this.list.ScrollIntoView(this.list.SelectedItem);
            }
        }

        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollIntoView();
        }

        public ViewModel ViewModel { get; private set; }

        public IView GetItemView(ViewModel vm)
        {
            return new ChatView(vm);
        }


        private void Item_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is Grid grid && grid.ContextMenu != null)
            {
                grid.ContextMenu.IsOpen = false;
            }
        }

    }
}
