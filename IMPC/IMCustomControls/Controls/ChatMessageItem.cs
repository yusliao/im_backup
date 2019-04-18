
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
    [TemplatePart(Name = "PART_Content", Type = typeof(SelectableTextBlock))] 
    [System.Windows.Markup.ContentProperty("Content")]
    [System.ComponentModel.DefaultProperty("Content")]
    //[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
    public class ChatItem : Control
    {
        public static DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(ChatItem));
        public static DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(object), typeof(ChatItem));


        public static DependencyProperty HeaderForegroundProperty = DependencyProperty.Register("HeaderForeground", typeof(Brush), typeof(ChatItem),
            new PropertyMetadata(Brushes.Black));
        public static DependencyProperty HeaderFontSizeProperty = DependencyProperty.Register("HeaderFontSize", typeof(double), typeof(ChatItem),
            new PropertyMetadata(12d));


        public static DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(ObservableCollection<Inline>), typeof(ChatItem),
            new PropertyMetadata(OnContentPropertyChanged));

        public static void OnContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ChatItem item = d as ChatItem;

            var oldV = e.OldValue as  ObservableCollection<Inline>;
            var newV = e.NewValue as   ObservableCollection<Inline>;

            if (oldV != null)
            {
                oldV.CollectionChanged -= item.Content_CollectionChanged;
            }

            if (newV != null)
            {
                newV.CollectionChanged += item.Content_CollectionChanged;
            }
            item.UpdateContent(e.NewValue as ObservableCollection<Inline>);
        }

        private void Content_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.UpdateContent(sender as ObservableCollection<Inline>);
        }



        public object Header
        {
            get
            {
                return this.GetValue(HeaderProperty);
            }
            set
            {
                this.SetValue(HeaderProperty, value);
            }
        }

        public double HeaderFontSize
        {
            get
            {
                return (double)this.GetValue(HeaderFontSizeProperty);
            }
            set
            {
                this.SetValue(HeaderFontSizeProperty, value);
            }
        }

        public Brush HeaderForeground
        {
            get
            {
                return (Brush)this.GetValue(HeaderForegroundProperty);
            }
            set
            {
                this.SetValue(HeaderForegroundProperty, value);
            }
        }

        public object Icon
        {
            get
            {
                return this.GetValue(IconProperty);
            }
            set
            {
                this.SetValue(IconProperty, value);
            }
        }

        public ObservableCollection<Inline> Content
        {
            get
            {
                return (ObservableCollection<Inline>)this.GetValue(ContentProperty);
            }
            set
            {
                this.SetValue(ContentProperty, value);
            }
        }

        static ChatItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChatItem), new FrameworkPropertyMetadata(typeof(ChatItem)));
        }


      

        private SelectableTextBlock TBContent = new SelectableTextBlock();
        public ChatItem()
        {
            //this.Content = new ObservableCollection<Inline>();
        }
         

        private void UpdateContent(ObservableCollection<Inline> contents)
        {
            this.TBContent.Text = null;

            if (contents != null)
            {
                this.TBContent.Inlines.AddRange(contents); 
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            SelectableTextBlock tbContent = this.Template.FindName("PART_Content", this) as SelectableTextBlock;
            if (tbContent!=null)
            {
                tbContent.Inlines.AddRange(this.Content);
             
                this.TBContent = tbContent; 
                //this.Content = _content = new ObservableCollection<Inline>();
            }
        }
    }
}
