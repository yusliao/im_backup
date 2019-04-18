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
    /// 关注人列表
    /// </summary>
    public partial class AttentionListView : UserControl,IListView
    {
        /// <summary>
        /// 关注人view
        /// </summary>
        public AttentionListView()
        {
            InitializeComponent();
            this.DataContext = this.ViewModel =new AttentionListViewModel(this);
            this.Loaded += View_Loaded;
             
            this.list.PreviewMouseRightButtonDown += (s, e) => { e.Handled = true; };
            //this.searchResults.PreviewMouseRightButtonDown += (s, e) => { e.Handled = true; };
        }

        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.list.SelectedItem != null)
            {
                this.list.ScrollIntoView(this.list.SelectedItem);
            }
        }

        public ViewModel ViewModel { get; private set; }

        public IView GetItemView(ViewModel vm)
        {
            return  new AttentionView(vm);
        } 
    }
}
