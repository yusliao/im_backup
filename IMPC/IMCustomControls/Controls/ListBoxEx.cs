using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace IMCustomControls.Controls
{
    public class ListBoxEx : ListBox
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ListBoxItemEx();
        }
    }

    public class ListBoxItemEx : ListBoxItem
    {
        protected override void OnSelected(System.Windows.RoutedEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            while ((dep != null) && !(dep is ListBoxItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
                return;

            ListBoxItem item = (ListBoxItem)dep;

            if (item.IsSelected)
            {
                item.IsSelected = !item.IsSelected;
                //e.Handled = true;
            }
            base.OnSelected(e);
        }
    }
}
