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
    /// FowardView.xaml 的交互逻辑
    /// </summary>
    public partial class FowardView : UserControl
    {
        private List<DependencyObject> _listDependencyObj = new List<DependencyObject>();
        public FowardView()
        {
            InitializeComponent();
            this.Loaded += FowardView_Loaded;
        }

        private void FowardView_Loaded(object sender, RoutedEventArgs e)
        {
            GetChildObject<Expander>(this.listSource, "expander");
            foreach (var item in _listDependencyObj)
            {
                if ((item as Expander).Header is TextBlock tb && tb.Text.Equals("最近消息"))
                {
                    (item as Expander).IsExpanded = true;
                }
            }
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
