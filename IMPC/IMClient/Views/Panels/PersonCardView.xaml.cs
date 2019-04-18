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
    /// PersonCardView.xaml 的交互逻辑
    /// </summary>
    public partial class PersonCardView : UserControl
    {
        private List<DependencyObject> _listDependencyObj = new List<DependencyObject>();
        public PersonCardView()
        {
            InitializeComponent();
            this.Loaded += PersonCardView_Loaded;
        }

        private void PersonCardView_Loaded(object sender, RoutedEventArgs e)
        {
            GetChildObject<Expander>(this.listsource, "expander");
            if (_listDependencyObj.Count == 1)
                (_listDependencyObj.FirstOrDefault() as Expander).IsExpanded = true;
            else
            {
                Expander expander = _listDependencyObj.FirstOrDefault(x => ((x as Expander).Header as TextBlock).Text.Equals("最近消息")) as Expander;
                if (expander != null)
                    expander.IsExpanded = true;
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
