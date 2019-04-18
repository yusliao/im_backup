using System;
using System.Collections;
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
using CSClient.ViewModels;
using IMModels;
using System.Windows.Controls.Primitives;

namespace CSClient.Views.Controls
{
    /// <summary>
    /// 聊天盒子（消息呈现面板）
    /// </summary>
    public partial class ChatBox : UserControl
    {
        public event Action OnHideHistoryMsgButton;
        public event Action<IChat> OnDisplayAtSomeone;

        StackPanel _itemPanel;
        ChatViewModel _chatVM;

        IMCustomControls.SelectableTextBlock _targetSTB;

        public ChatBox()
        {
            InitializeComponent();
            this.Loaded += ChatBox_Loaded;
            this.sv.PreviewMouseWheel += Sv_PreviewMouseWheel;
            this.sv.PreviewMouseDown += Sv_PreviewMouseDown;

            this.sv.KeyUp += Sv_KeyUp;
            this.Unloaded += ChatBox_Unloaded;

            this.lsb.Loaded += Lsb_Loaded;
        }

        private void Lsb_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ChatViewModel cvm)
            {
                int count = cvm.AtMeDic.Count;
                for (int i = 0; i < count; i++)
                {
                    MessageModel msgModel = cvm.AtMeDic.ElementAt(i).Value;

                    bool b = this.IsItemInVisualArea(msgModel);
                    if (b)
                    {
                        cvm.AtMeDic.TryRemove(msgModel.MsgKey, out msgModel);
                        count--;
                        i--;
                    }
                }
                if (cvm.AtMeDic.Count == 0)
                {
                    cvm.IsDisplayAtButton = false;
                }
                else
                {
                    cvm.IsDisplayAtButton = true;
                    cvm.IsDisplayHistoryMsgButton = false;
                }
            }
        }

        private void ChatBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ChatViewModel)
            {
                _chatVM = this.DataContext as ChatViewModel;
                _chatVM.OnDisplayMsgHint -= _chatVM_OnDisplayMsgHint;
                _chatVM.OnDisplayMsgHint += _chatVM_OnDisplayMsgHint;
            }
        }

        private void _chatVM_OnDisplayMsgHint()
        {
            if (sv.VerticalOffset == sv.ScrollableHeight)
            {
                sv.ScrollToEnd();
            }
            else
            {
                _chatVM.IsDisplayMsgHint = true;
            }
        }

        private void ChatBox_Unloaded(object sender, RoutedEventArgs e)
        {
            //_chatVM?.Unload();
        }

        private void itemPanel_Loaded(object sender, RoutedEventArgs e)
        {
            //_itemPanel = sender as VirtualizingStackPanel;
            _itemPanel = sender as StackPanel;            
        }

        private void Sv_KeyUp(object sender, KeyEventArgs e)
        {
            //if (_targetSTB != null && Keyboard.Modifiers == ModifierKeys.Control)
            //{
            //    if (e.Key == Key.C)
            //        _targetSTB.Copy();
            //    else if (e.Key == Key.A)
            //        _targetSTB.SelectAll();
            //}
        }

        private void Sv_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject os)
            {
                var parent = LogicalTreeHelper.GetParent(os);

                while (parent != null && !(parent is IMCustomControls.SelectableTextBlock))
                {
                    parent = LogicalTreeHelper.GetParent(parent);
                }

                _targetSTB = parent as IMCustomControls.SelectableTextBlock;
            }
        }

        private void Sv_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0 && this.sv.VerticalOffset == 0)
            {
                _chatVM.LoadHisMessages(true);
            }

            if (sv.VerticalOffset == sv.ScrollableHeight)
            {
                _chatVM.IsDisplayMsgHint = false;
            }
            else
            {
                this.OnHideHistoryMsgButton?.Invoke();
            }

            this.DisplayOrHideAtButton();
        }

        private void DisplayOrHideAtButton()
        {
            int count = _chatVM.AtMeDic.Count;
            for (int i = 0; i < count; i++)
            {
                MessageModel msgModel = _chatVM.AtMeDic.ElementAt(i).Value;
                bool b = this.IsItemInVisualArea(msgModel);
                if (b)
                {
                    _chatVM.AtMeDic.TryRemove(msgModel.MsgKey, out msgModel);
                    count--;
                    i--;
                }
            }

            if (_chatVM.AtMeDic.Count == 0)
            {
                _chatVM.IsDisplayAtButton = false;
            }
        }

        /// <summary>
        /// 判断Item是否在可视区域
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool IsItemInVisualArea(MessageModel msgModel)
        {
            try
            {
                this.lsb.UpdateLayout();
                foreach (MessageModel c in this.lsb.Items)
                {
                    if (c.MsgKey.Equals(msgModel.MsgKey))
                    {
                        var item = (FrameworkElement)this.lsb.ItemContainerGenerator.ContainerFromItem(c);
                        if (item == null)
                        {
                            return true;
                        }
                        bool b = IsChildVisibleInParent(item, this.sv);
                        return b;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }            
        }

        private bool IsChildVisibleInParent(FrameworkElement child, FrameworkElement parent)
        {
            try
            {
                var childTransform = child.TransformToAncestor(parent);
                var childRectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), child.RenderSize));
                var ownerRectangle = new Rect(new Point(0, 0), parent.RenderSize);
                return ownerRectangle.IntersectsWith(childRectangle);
            }
            catch
            {
                return false;
            }
        }

        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(ChatBox), new PropertyMetadata(OnItemsSourcePropertyChanged));

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ChatBox target = d as ChatBox;
            target.lsb.ItemsSource = e.NewValue as IEnumerable;
        }

        public void ScallToHome()
        {
            this.sv.ScrollToHome();
        }
        public void ScallToCurrent(MessageModel content = null)
        {
            if (content == null)
            {
                this.sv.ScrollToEnd();
                if (_chatVM != null)
                {
                    _chatVM.IsDisplayMsgHint = false;
                }
            }
            else
            {
                if (_itemPanel != null)
                {
                    int index = _chatVM.Chat.Messages.IndexOf(content);

                    if (_itemPanel.Children.Count <= index)
                    {
                        index = _itemPanel.Children.Count - 1;
                    }
                    UIElement view = _itemPanel.Children[index];

                    Point pos = view.TranslatePoint(new Point(), _itemPanel);
                    this.sv.ScrollToVerticalOffset(pos.Y - 60);

                    if (pos.Y >= sv.ScrollableHeight)
                    {
                        _chatVM.IsDisplayMsgHint = false;
                    }
                }
            }
        }

        private void ccTB_DoCopyAction(IMCustomControls.SelectableTextBlock sender)
        {
            //List<string> list = new List<string>();
            StringBuilder sb = new StringBuilder();
            foreach (Inline item in sender.Inlines)
            {
                //if (sender.Selection.Contains(item.ContentStart) && sender.Selection.Contains(item.ContentEnd))
                {
                    if (item is Run run)
                    {
                        //list.Add(run.Text);
                        sb.Append(run.Text);
                    }
                    else if (item is InlineUIContainer uic && uic.Child is Image chatIMG)
                    {
                        //list.Add(string.Format("{0}{1}{0}", AppData.FlagEmoje, chatIMG.Uid));

                        sb.Append(chatIMG.Uid);
                    }
                    else
                    {

                    }
                }
            }

            Clipboard.SetDataObject(sb.ToString());
        }

        private void ccTB_DoWithdrawAction(IMCustomControls.SelectableTextBlock sender)
        {
            if (this.DataContext is ChatViewModel chatVM && sender.DataContext is MessageModel msgModel)
            {
                chatVM.SendWithDrawMsg(msgModel);
            }
        }

        private void ccTB_DoDeleteAction(IMCustomControls.SelectableTextBlock sender)
        {
            if (this.DataContext is ChatViewModel chatVM && sender.DataContext is MessageModel msgModel)
            {
                chatVM.HideMessageCommand.Execute(msgModel);
            }
        }

        private void ccMsgHint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _chatVM.IsDisplayMsgHint = false;
            sv.ScrollToBottom();
            this.DisplayOrHideAtButton();
        }

        private void miAt_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if (mi.DataContext is MessageModel msgModel)
            {
                this.OnDisplayAtSomeone?.Invoke(msgModel.Sender);
            }
        }

    

        private void pathResend_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(sender is Path path)
            {
                Message_ReSend(path.DataContext as MessageModel);
            }
        }

        private void Message_ReSend(MessageModel msg)
        {
            if (msg != null)
            {
                _chatVM?.ReSend(msg); 
            }
        }
    }
}
