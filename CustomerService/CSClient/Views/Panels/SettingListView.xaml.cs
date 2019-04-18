using CSClient.ViewModels;
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

namespace CSClient.Views.Panels
{
    /// <summary>
    /// SettingListView.xaml 的交互逻辑
    /// </summary>
    public partial class SettingListView : UserControl, IListView
    {
        public SettingListView()
        {
            InitializeComponent();
            UItaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            SettingListViewModel vm = new SettingListViewModel(this);
            this.DataContext = this.ViewModel = vm;
            this.list.PreviewMouseRightButtonDown += (s, e) => { e.Handled = true; }; 
        }

        public ViewModel ViewModel { get; private set; }

        public TaskScheduler UItaskScheduler { get; private set; }

        public IView GetItemView(ViewModel vm)
        {
            return vm.View;
        }
    }
}
