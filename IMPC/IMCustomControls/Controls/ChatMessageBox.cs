using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace IMCustomControls
{ 
    public class ChatBox : ItemsControl
    {
        static ChatBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChatBox), new FrameworkPropertyMetadata(typeof(ChatBox)));
        }


        public static DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ChatBox));

        public static DependencyProperty ItemTitlePathProperty = DependencyProperty.Register("ItemTitlePath", typeof(string), typeof(ChatBox));

        public static DependencyProperty ItemValuePathProperty = DependencyProperty.Register("ItemValuePath", typeof(string), typeof(ChatBox));

        public static DependencyProperty ItemDetailTemplateProperty = DependencyProperty.Register("ItemDetailTemplate", typeof(DataTemplate), typeof(ChatBox));

        public static DependencyProperty ItemTitleTemplateProperty = DependencyProperty.Register("ItemTitleTemplate", typeof(DataTemplate), typeof(ChatBox));

        public static DependencyProperty ItemValueTemplateProperty = DependencyProperty.Register("ItemValueTemplate", typeof(DataTemplate), typeof(ChatBox));

        public string Title
        {
            get
            {
                return (string)this.GetValue(TitleProperty);
            }
            set
            {
                this.SetValue(TitleProperty, value);
            }
        }

        public string ItemTitlePath
        {
            get
            {
                return (string)this.GetValue(ItemTitlePathProperty);
            }
            set
            {
                this.SetValue(ItemTitlePathProperty, value);
            }
        }

        public string ItemValuePath
        {
            get
            {
                return (string)this.GetValue(ItemValuePathProperty);
            }
            set
            {
                this.SetValue(ItemValuePathProperty, value);
            }
        }

        public DataTemplate ItemDetailTemplate
        {
            get
            {
                return (DataTemplate)this.GetValue(ItemDetailTemplateProperty);
            }
            set
            {
                this.SetValue(ItemDetailTemplateProperty, value);
            }
        }

        public DataTemplate ItemTitleTemplate
        {
            get
            {
                return (DataTemplate)this.GetValue(ItemTitleTemplateProperty);
            }
            set
            {
                this.SetValue(ItemTitleTemplateProperty, value);
            }
        }

        public DataTemplate ItemValueTemplate
        {
            get
            {
                return (DataTemplate)this.GetValue(ItemValueTemplateProperty);
            }
            set
            {
                this.SetValue(ItemValueTemplateProperty, value);
            }
        }

        //protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        //{ 
        //    base.PrepareContainerForItemOverride(element, item);
        //    var ele = element as VListItemDemo;

        //    {
        //        Binding binding = new Binding();
        //        ele.SetBinding(VListItemDemo.ItemProperty, binding);
        //    }

        //    if (!string.IsNullOrEmpty(this.ItemTitlePath))
        //    {
        //        Binding binding = new Binding(this.ItemTitlePath);
        //        ele.SetBinding(VListItemDemo.TitleProperty, binding);
        //    }

        //    if (!string.IsNullOrEmpty(this.ItemValuePath))
        //    {
        //        Binding binding = new Binding(this.ItemValuePath);
        //        ele.SetBinding(VListItemDemo.ValueProperty, binding);
        //    }

        //    ele.DetailTemplate = this.ItemDetailTemplate;
        //    ele.TitleTemplate = this.ItemTitleTemplate;
        //    ele.ValueTemplate = this.ItemValueTemplate;
        //}

        //protected override DependencyObject GetContainerForItemOverride()
        //{
        //    return new VListItemDemo();
        //}

    } 
}
