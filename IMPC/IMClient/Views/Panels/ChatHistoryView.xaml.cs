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
using IMClient.ViewModels;

namespace IMClient.Views.Panels
{
    /// <summary>
    /// ChatHistoryView.xaml 的交互逻辑
    /// </summary>
    public partial class ChatHistoryView : UserControl
    {
        /// <summary>
        /// 历史消息框
        /// </summary>
        public static Helper.FlowDocumentEx HisMsgTarget;

        ChatHistoryViewModel _hisVM;
        SDKClient.SDKProperty.MessageType _msgType = SDKClient.SDKProperty.MessageType.all;

        public ChatHistoryView(IMModels.ChatModel chat)
        {
            InitializeComponent();
            this.DataContext = _hisVM = new ChatHistoryViewModel(chat);

            this.rictBox.SizeChanged += RictBox_SizeChanged;
            this.sv.PreviewMouseWheel += Sv_PreviewMouseWheel;

            DataObject.AddCopyingHandler(this.rictBox, (s, e) => { e.CancelCommand(); e.Handled = true; DoCopy(); });

            this.Loaded += ChatHistoryView_Loaded;

            this.flowDoc.MouseUp += FlowDoc_PreviewMouseUp;
            this.Unloaded += ChatHistoryView_Unloaded;
        }

        private void ChatHistoryView_Unloaded(object sender, RoutedEventArgs e)
        {
            HisMsgTarget = null;
        }

        private void FlowDoc_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Button btn)
            {
                switch (btn.Uid)
                {
                    case "Cancel":
                    case "Delete":
                        App.Current.MainWindow.Activate();
                        break;
                    default:
                        break;

                }
            }
        }

        private void ChatHistoryView_Loaded(object sender, RoutedEventArgs e)
        {
            HisMsgTarget = this.flowDoc;
            this.Loaded -= ChatHistoryView_Loaded;
            this.calendar.DisplayDateEnd = DateTime.Now;
            this.rbtnAll.IsChecked = true;
        }

        private void Sv_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0 && this.sv.VerticalOffset == 0)
            {
                LoadDatas(_msgType, false);
                this.isScroll = true;
            }
        }
        bool isScroll;

        private void RictBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double vOffset = e.NewSize.Height - e.PreviousSize.Height;

            vOffset += this.sv.VerticalOffset;
            this.sv.ScrollToVerticalOffset(vOffset);
            if (this.calendar.SelectedDate != null)
            {
                if (!isScroll)
                {
                    this.sv.ScrollToHome();
                }
                else
                    return;
            }
        }


        private void DoCopy()
        {
            this.cmMenu.IsOpen = false;
            Task.Delay(200).ContinueWith(t =>
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    var datas = Helper.RichTextBoxHelper.GetSelectionItems(this.rictBox);
                    if (datas.Count > 0)
                    {
                        Helper.MessageHelper.SetRichTextBoxSelectionToClipboard(datas);
                    }
                }));

            });

        }

        private void Type_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rbtn)
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    switch (rbtn.Uid)
                    {
                        case "All":
                            _msgType = SDKClient.SDKProperty.MessageType.all;
                            break;
                        case "Image|Video":
                            _msgType = SDKClient.SDKProperty.MessageType.imgandvideo;
                            break;
                        case "File":
                            _msgType = SDKClient.SDKProperty.MessageType.file;
                            break;
                    }
                    LoadDatas(_msgType, true);
                }));
                this.calendar.SelectedDate = null;
            }
        }

        private void calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.calendar.SelectedDate == null)
            {
                return;
            }
            this.isScroll = false;
            LoadDatas(_msgType, true, this.calendar.SelectedDate);
            //this.calendar.SelectedDate = null;
        }

        private void LoadDatas(SDKClient.SDKProperty.MessageType type, bool isReset, DateTime? date = null, int count = 20)
        {
            this.aniLoading.Begin();
            grid_LoadWithoutContent.Visibility = Visibility.Collapsed;
            if (isReset)
            {
                this.rictBox.SizeChanged -= RictBox_SizeChanged;
                try
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        this.flowDoc.Blocks.Clear();
                    });
                }
                catch
                {

                }
            }
            _hisVM.LoadPreviousMessages((items) =>
            {
                if (isReset)
                {
                    this.rictBox.SizeChanged -= RictBox_SizeChanged;
                    try
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            this.flowDoc.Blocks.Clear();
                        });
                    }
                    catch
                    {

                    }
                }
                if (items?.Count > 0)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        bool isSearch = date != null ? true : false;
                        this.aniLoading.Stop();

                        this.flowDoc.InsertPrevious(items, isReset, isSearch);
                        this.sv.Visibility = Visibility.Visible;
                        this.grid_LoadWithoutContent.Visibility = Visibility.Collapsed;
                        this.grid_SearchWithoutContent.Visibility = Visibility.Collapsed;
                        if (isReset)
                        {
                            this.rictBox.SizeChanged += RictBox_SizeChanged;
                        }
                        this.ptbtnCalendar.IsChecked = false;
                    }));
                }
                else
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        this.aniLoading?.Stop();
                        if (date != null)
                        {
                            this.sv.Visibility = Visibility.Collapsed;
                            this.grid_LoadWithoutContent.Visibility = Visibility.Collapsed;
                            this.grid_SearchWithoutContent.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            this.sv.Visibility = Visibility.Collapsed;
                            this.grid_LoadWithoutContent.Visibility = Visibility.Visible;
                            this.grid_SearchWithoutContent.Visibility = Visibility.Collapsed;
                        }
                    }));
                }
                this.Dispatcher.Invoke(new Action(() =>
                {
                    if (this.flowDoc.Blocks.Count > 0)
                    {
                        this.aniLoading?.Stop();
                        this.sv.Visibility = Visibility.Visible;
                        this.grid_LoadWithoutContent.Visibility = Visibility.Collapsed;
                        this.grid_SearchWithoutContent.Visibility = Visibility.Collapsed;
                        if (date != null)
                            this.sv.ScrollToHome();
                    }
                }));



            }, type, date, isReset, count);


        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!Views.MessageBox.ShowDialogBox("确定清空此聊天消息？"))
            {
                return;
            }

            bool? result = App.IsCancelOperate("清空聊天记录", "您有文件正在传输中，确定终止文件传输吗？", _hisVM.ID);
            if (result == true)
            {
                return;
            }
            this.pbtnMore.IsChecked = false;
            aniLoading.Begin();
            Task.Delay(500).ContinueWith(t =>
            {
                _hisVM.ClearAllCommand.Execute(null);
                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.flowDoc.Blocks.Clear();
                    aniLoading.Stop();
                }));

                string fileRoot = SDKClient.SDKClient.Instance.property.CurrentAccount.filePath;
                if (System.IO.Directory.Exists(fileRoot))
                {
                    System.IO.Directory.Delete(fileRoot, true);
                }
            });
            this.sv.Visibility = Visibility.Collapsed;
            this.grid_LoadWithoutContent.Visibility = Visibility.Visible;
            this.grid_SearchWithoutContent.Visibility = Visibility.Collapsed;
        }

        private void calendar_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var target = e.OriginalSource as FrameworkElement;

            while (target != null)
            {
                if (target is System.Windows.Controls.Primitives.CalendarDayButton item)
                {
                    this.ptbtnCalendar.IsChecked = false;
                    break;
                }
                else
                {
                    target = VisualTreeHelper.GetParent(target) as FrameworkElement;
                }
            }

        }

        public List<T> GetChildObjects<T>(DependencyObject obj) where T : FrameworkElement
        {
            DependencyObject child = null;
            List<T> childList = new List<T>();

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);

                if (child is T)
                {
                    childList.Add((T)child);
                }
                childList.AddRange(GetChildObjects<T>(child));
            }
            return childList;
        }

        private void btn_BackToAll_Click(object sender, RoutedEventArgs e)
        {
            List<RadioButton> radioButtons = GetChildObjects<RadioButton>(this.dp_TypesCheck);
            RadioButton tb_Target = null;
            foreach (RadioButton child in radioButtons)
            {
                if (child.IsChecked == true)
                    tb_Target = child;
            }
            switch (tb_Target.Uid)
            {
                case "All":
                    _msgType = SDKClient.SDKProperty.MessageType.all;
                    break;
                case "Image|Video":
                    _msgType = SDKClient.SDKProperty.MessageType.imgandvideo;
                    break;
                case "File":
                    _msgType = SDKClient.SDKProperty.MessageType.file;
                    break;
                default:
                    break;
            }
            LoadDatas(_msgType, true);
            this.calendar.SelectedDate = null;
        }
    }
}
