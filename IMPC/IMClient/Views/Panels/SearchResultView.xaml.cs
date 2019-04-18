using IMClient.ViewModels;
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
    /// SearchResultView.xaml 的交互逻辑
    /// </summary>
    public partial class SearchResultView : UserControl, IListView
    {
        public ViewModel ViewModel { get; private set; }

        public IView GetItemView(ViewModel vm)
        {
            return new ChatView(vm);
        }


        public SearchResultView()
        {
            InitializeComponent();
            this.Loaded += SearchResultView_Loaded;
           
        }

        private void SearchResultView_Loaded(object sender, RoutedEventArgs e)
        {
           // var objView = AppData.MainMV.ChatListVM.View as ChatListView;
            //this.DataContext = AppData.MainMV.ChatListVM;
        }

        private void searchResults_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
        private void Item_Unloaded(object sender, RoutedEventArgs e)
        {
            //if (sender is Grid grid && grid.ContextMenu != null)
            //{
            //    grid.ContextMenu.IsOpen = false;
            //}
        }

        /// <summary>
        /// 滚动滚轮方式解决滚动条不能滚动的问题，已用Behavior方式解决，此种方式也可
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;
            MouseWheelEventArgs eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = UIElement.MouseWheelEvent;
            eventArg.Source = sender;
            sv.RaiseEvent(eventArg);
        }

        private void searchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        
    }
}
