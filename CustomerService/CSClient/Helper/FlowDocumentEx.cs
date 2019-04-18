using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CSClient.Views.Controls;
using IMModels;

namespace CSClient.Helper
{
    public class FlowDocumentEx:  FlowDocument
    {
       static readonly FontFamily _FontFamily = new FontFamily("微软雅黑");
        static readonly Brush _Foreground = new SolidColorBrush(Colors.Black) {Opacity=0.9 };// App.Current.Resources["HightLightBackground"] as Brush;
       static readonly Brush _ActiveBrush = App.Current.Resources["SelectedBackground"] as Brush;
        public FlowDocumentEx()
        { 
        }
 
        [Bindable(true)]  
        public List<MessageModel> Items
        {
            get { return (List<MessageModel>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
         
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(List<MessageModel>), typeof(FlowDocumentEx), 
                new FrameworkPropertyMetadata(OnItemsPropertyChanged));
         
        private static void OnItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FlowDocumentEx doc = d as FlowDocumentEx;
            doc.RefreshContent();
        }



        public Paragraph ActiveItem
        {
            get { return (Paragraph)GetValue(ActiveItemItemProperty); }
            set { SetValue(ActiveItemItemProperty, value); }
        }
         
        public static readonly DependencyProperty ActiveItemItemProperty =
            DependencyProperty.Register("ActiveItem", typeof(Paragraph), typeof(FlowDocumentEx), new PropertyMetadata(OnActiveItemPropertyChanged));
        private static void OnActiveItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.OldValue is Paragraph oldPa)
            {
                oldPa.Background = null; ;
            }

            if (e.NewValue is Paragraph newPa)
            {
                newPa.Background = _ActiveBrush;
            }
        }
         

        private void RefreshContent()
        { 
            this.Blocks.Clear();
            if(this.Items!=null)
            {
                int count = this.Items.Count;
                Task.Run(() =>
                {
                    for (int i = 0; i < count; i++)
                    {
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            Paragraph pa = GetMsgBlock(this.Items[i]);
                            this.FillContent(pa, this.Items[i]);
                            //pa.Inlines.Add(new LineBreak());
                            this.Blocks.Add(pa);
                        }));
                    System.Threading.Thread.CurrentThread.Join(1);
                }
                });

            }
        }

        public void InsertPrevious(List<MessageModel> previous, bool isReset = false)
        { 
            if (isReset || this.Items == null || this.Items.Count == 0)
            {
                this.SetValue(FlowDocumentEx.ItemsProperty, previous);
            }
            else
            {
                this.Items.InsertRange(0, previous);
                for (int i = previous.Count-1; i >0 ; i--)
                {
                    var msg = previous[i];

                    Paragraph pa = GetMsgBlock(msg);
                    this.FillContent(pa, msg); 
                    this.Blocks.InsertBefore(this.Blocks.FirstBlock, pa);
                }
            }
        }

        
        private Paragraph GetMsgBlock(MessageModel msg)
        {
            Paragraph pa = new Paragraph()
            { 
                TextAlignment=TextAlignment.Left,
                DataContext = msg,
                Margin=new Thickness(0),
                Padding=new Thickness(0),
            };
            Binding bind = new Binding("Sender.DisplayName")
            {
                Mode = BindingMode.OneWay,
            };
            Run run = new Run() { FontSize = 12, Foreground = _Foreground, FontFamily= _FontFamily };
            run.SetBinding(Run.TextProperty, bind);
            pa.Inlines.Add(run);

            pa.Inlines.Add(new Run("  "));

            bind = new Binding("SendTime")
            {
                Mode = BindingMode.OneTime,
                StringFormat = "yyyy-MM-dd HH:mm:ss",
            };

            run = new Run() { FontSize = 12, Foreground = Brushes.DarkGray, FontFamily = _FontFamily };
            run.SetBinding(Run.TextProperty, bind);
            pa.Inlines.Add(run);

            pa.Inlines.Add("\r\n");

            pa.MouseEnter += Pa_MouseEnter;
            return pa;
        }

        private void Pa_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //Paragraph pa = sender as Paragraph;
            //this.ActiveItem = pa;
        }

        private void FillContent(Paragraph pa, MessageModel msg)
        {
            switch (msg.MsgType)
            {
                case MessageType.txt:
                case MessageType.bigtxt:
                    Helper.MessageHelper.ContentInlines = null;
                    var inlines= CSClient.Helper.MessageHelper.GetRichContent(msg.Content,false);
                    foreach(Inline inl in inlines)
                    {
                        inl.BaselineAlignment = BaselineAlignment.Bottom;
                        inl.FontSize = 14;
                        inl.FontFamily = _FontFamily;
                        pa.Inlines.Add(inl);

                        if(inl is InlineUIContainer uic)
                        {
                            uic.Child.Uid = ViewModels.AppData.FlagEmoje + uic.Child.Uid;
                        }
                    }
                    break;
                case MessageType.img:
                    InlineUIContainer imgC = new InlineUIContainer();

                    ChatImage chatImg = new ChatImage(msg.Content, new Size(240, 135)) { DataContext = msg,HasContexMenu=false };
                    imgC.Child = chatImg;
                    Binding bind = new Binding("Content") {Mode=BindingMode.OneWay };
                    chatImg.SetBinding(ChatImage.ImagePathProperty, bind);
                   
                    bind = new Binding("MessageState") { Mode = BindingMode.OneWay };
                    chatImg.SetBinding(ChatImage.StateProperty, bind);

                    CSClient.Helper.MessageHelper.LoadImgContent(msg);
                    pa.Inlines.Add(imgC); 
                    imgC.PreviewMouseLeftButtonDown += ImgC_MouseLeftButtonUp;
                    break;
                case MessageType.file:
                case MessageType.onlinefile:
                    InlineUIContainer uicFile = new InlineUIContainer();
                    FileChatItem chatFile = new FileChatItem() { DataContext = msg,IsMainView=false };
                  
                    chatFile.IsInDocument = true;
                    //msg.ResourceModel.FileState = FileStates.Completed;
                    uicFile.Child = chatFile;
                    Binding bindF = new Binding("Content") { Mode = BindingMode.OneWay };
                    chatFile.SetBinding(FileChatItem.FullNameProperty, bindF);

                    Binding bindState = new Binding("ResourceModel.FileState") { Mode = BindingMode.TwoWay };
                    chatFile.SetBinding(FileChatItem.FileStateProperty, bindState);

                    //CSClient.Helper.MessageHelper.LoadFileContent(msg);
                    pa.Inlines.Add(uicFile);
                    break;
                default:
                    //pa.Inlines.Clear(); 
                    //pa.Inlines.Add(new Run(msg.Content)
                    //{
                    //    FontSize = 14,
                    //    Background = Brushes.Gray,
                    //    Foreground = Brushes.LightGray,
                    //    FontFamily = _FontFamily,
                    //});
                    //pa.TextAlignment = TextAlignment.Center;
                    pa.Inlines.Add(new Run(msg.Content)
                    {
                        FontSize = 14, 
                        FontFamily = _FontFamily,
                    });
                    
                    break;
            } 
            pa.Inlines.Add("\r\n");
        }
         
        private void ImgC_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        { 
            //if (sender is  ChatImage chatImg)
            //{
            //    Views.ChildWindows.AppendWindow.AutoClose = false;
            //    chatImg.ScanImage();
            //    Views.ChildWindows.AppendWindow.AutoClose = true;
            //}
            e.Handled = true;
        }

        
     
    }
}
