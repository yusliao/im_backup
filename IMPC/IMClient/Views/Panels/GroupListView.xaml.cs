using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using IMClient.Views.ChildWindows;
using IMModels;

namespace IMClient.Views.Panels
{
    /// <summary>
    /// 群组列表
    /// </summary>
    public partial class GroupListView : UserControl, IListView
    {
        private string _groupTypeName;
        private List<DependencyObject> _listDependencyObj = new List<DependencyObject>();

        public GroupListView()
        {
            InitializeComponent();
            this.DataContext = this.ViewModel = new GroupListViewModel(this);
            this.Loaded += View_Loaded;
            this.list.PreviewMouseRightButtonDown += (s, e) => { e.Handled = true; };
           // this.searchResults.PreviewMouseRightButtonDown += (s, e) => { e.Handled = true; };
        }

        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.list.SelectedItem != null)
            {
                this.list.ScrollIntoView(this.list.SelectedItem);
            }

            if (!string.IsNullOrEmpty(_groupTypeName))
            {
                GetChildObject<Expander>(this.list, "expander");
                foreach (var item in _listDependencyObj)
                {
                    if ((item as Expander).Header is TextBlock tb && tb.FindName("name") is Run run && run.Text.Equals(this._groupTypeName))
                    {
                        (item as Expander).IsExpanded = true;
                    }
                }
                _groupTypeName = string.Empty;
            }
        }

        public IView GetItemView(ViewModel vm)
        {
            return new GroupView(vm);
        }
        public ViewModel ViewModel { get; private set; }
        public GroupViewModel tempGroupViewModel;
        private void miChangeGroupName_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.DataContext is GroupViewModel groupVM)
            {
                tempGroupViewModel = groupVM;
                GroupModel group = groupVM.Model as GroupModel;
                
                string newValue = EditStringValueWindow.ShowInstance(group.DisplayName, "修改群名称");
                EditStringValueWindow.Win.GroupNameEvent -= EditStringValueWindow_GroupNameEvent;
                EditStringValueWindow.Win.GroupNameEvent += EditStringValueWindow_GroupNameEvent;

            }
        }
        private void EditStringValueWindow_GroupNameEvent(object obj)
        {
            GroupModel group = tempGroupViewModel.Model as GroupModel;
            tempGroupViewModel.ChangedGroupNameCommand?.Execute(obj);
        }

        public void ExpandGroupList(string groupTypeName)
        {
            _groupTypeName = groupTypeName;
        }

        public void GetChildObject<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            DependencyObject child = null;

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);
                if (child is T && (((T)child).Name == name | string.IsNullOrEmpty(name)))
                {
                    _listDependencyObj.Add(child);
                }
                else
                {
                    GetChildObject<T>(child, name);
                }
            }
        }
    }
}
