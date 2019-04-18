using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace IM.Emoje.Controls
{
    public class ClickItemListBox:ListBox
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ClickItem();
        }
    }

    public class ClickItem : ListBoxItem
    {
        protected override void OnSelected(RoutedEventArgs e)
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
