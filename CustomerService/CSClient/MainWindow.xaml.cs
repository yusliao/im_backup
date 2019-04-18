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
using CSClient.ViewModels;
using CSClient.Views.Panels;
using IMModels;
using CSClient.Views.ChildWindows;
using CSClient.Helper;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;

namespace CSClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IView
    {
        public event Action<bool, bool> OnFlashIcon;
        public event Action OnCloseTrayWindow;
        public event Action OnChangeTrayWindowSize;

        MainViewModel _mainMV;

        public MainWindow(IMModels.LoginUser login)
        {
            InitializeComponent();
            UItaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.GotFocus += MainWindow_GotFocus;
            this.Activated += MainWindow_Activated;
            this.PreviewMouseDown += MainWindow_MouseDown;

            _mainMV = new MainViewModel(this, login);
            _mainMV.MainTitle = login.User.PhoneNumber;

            _mainMV.OnForceOffline += _mainMV_OnForceOffline;
            _mainMV.OnChangeTrayWindowSize += _mainMV_OnChangeTrayWindowSize;
            _mainMV.OnCloseTrayWindow += _mainMV_OnCloseTrayWindow;
            //初始化基本数据
            _mainMV.ChatListVM = new ChatListView().ViewModel as ChatListViewModel;
            _mainMV.ChatListVM.OnFlashIcon += ChatListVM_OnFlashIcon;
            _mainMV.ChatListVM.GotWindowIsActive += ChatListVM_GotWindowIsActive;
            _mainMV.ChatListVM.OnCloseTrayWindow += ChatListVM_OnCloseTrayWindow;

            _mainMV.HisChatListVM = new TempChatListView().ViewModel as TempChatListViewModel;
            _mainMV.ChatHistoryListVM = new HistoryChatListView().ViewModel as HistoryChatListViewModel;
           // _mainMV.HisChatListVM.GotWindowIsActive += ChatListVM_GotWindowIsActive;
           // _mainMV.HisChatListVM.OnCloseTrayWindow += ChatListVM_OnCloseTrayWindow;

            _mainMV.SettingListVM = new SettingListView().ViewModel as SettingListViewModel;
            Task.Run(async () =>
            {
                _mainMV.CommonReply =await SDKClient.SDKClient.Instance.GetQuickReplycontext(1);
                _mainMV.PersonalReply =await SDKClient.SDKClient.Instance.GetQuickReplycontext(2);
            }).ContinueWith(t=>
            {
                _mainMV.SettingListVM.LoadDatas();
            },UItaskScheduler);
            
          

            AppData.Current.InitializeDatas();

            this.DataContext = ViewModel = _mainMV;
            _mainMV.NaviCommand.Execute(_mainMV.ChatListVM);

            AppendInitialize();
            _mainMV.ShowTip += (tip) => { Views.MessageTip.ShowTip(tip); };
        }

        private void _mainMV_OnCloseTrayWindow()
        {
            ChatListVM_OnCloseTrayWindow();
        }

        private void ChatListVM_OnCloseTrayWindow()
        {
            this.OnCloseTrayWindow?.Invoke();
        }

        private void _mainMV_OnChangeTrayWindowSize()
        {
            this.OnChangeTrayWindowSize?.Invoke();
        }

        private bool ChatListVM_GotWindowIsActive()
        {
            if (this.WindowState == WindowState.Minimized || this.Visibility == Visibility.Hidden)
            {
                return false;
            }

            return this.IsActive;
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChangeTaskBarAndTray();
        }

        private void MainWindow_GotFocus(object sender, RoutedEventArgs e)
        {
            ChangeTaskBarAndTray();
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            ChangeTaskBarAndTray();
        }

        void ChangeTaskBarAndTray()
        {
            StartOrStopFlashTaskBar(false);
            if (_mainMV.ListViewModel == _mainMV.ChatListVM && _mainMV.ChatListVM.SelectedItem != null)
            {
                int count = (_mainMV.ChatListVM.SelectedItem.Model as ChatModel).Messages.Count;
                for (int i = 0; i < _mainMV.ChatListVM.SelectedItem.UnReadCount; i++)
                {
                    string msgKey = (_mainMV.ChatListVM.SelectedItem.Model as ChatModel).Messages[count - 1 - i].MsgKey;
                    SDKClient.SDKClient.Instance.UpdateHistoryMsgIsReaded(msgKey);
                }
                _mainMV.ChatListVM.SelectedItem.UnReadCount = 0;
                _mainMV.UpdateUnReadMsgCount();
                _mainMV.ChatListVM.TotalUnReadCount = _mainMV.TotalUnReadCount;
                if (_mainMV.TotalUnReadCount == 0)
                {
                    this.OnCloseTrayWindow?.Invoke();
                }
                this.OnChangeTrayWindowSize?.Invoke();
            }
        }

        private void _mainMV_OnForceOffline()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                ForceOfflineWindow win = new ForceOfflineWindow();
                win.ShowDialog();
                (App.Current as App).ApplicationExit(null, null);
                SDKClient.SDKClient.Instance.StopAsync().ConfigureAwait(false);
                Application.Current?.Shutdown(0);
            });
        }

        private void StartOrStopFlashTrayIcon(bool isFlash, bool isDisplayMainWindow = true)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                this.OnFlashIcon?.Invoke(isFlash, isDisplayMainWindow);
            });
        }

        private void StartOrStopFlashTaskBar(bool isFlash)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                if (isFlash)
                {
                    //var isTrue = FlashWindowHelper.FlashWindowEx(this, FlashType.FLASHW_TIMERNOFG);
                    //if (isTrue == false)
                    //{
                    //    //如果窗口未激活，那么就停止闪烁，高亮
                    //    FlashWindowHelper.FlashWindowEx(this, FlashType.FLASHW_TIMER);
                    //}

                    FlashWindowHelper.FlashWindowEx(this, FlashType.FLASHW_TIMER);
                }
                else
                {
                    FlashWindowHelper.StopFlashingWindow(this);
                }
            });
        }

        private void ChatListVM_OnFlashIcon(bool isFlash, bool isDisplayMainWindow = true)
        {
            StartOrStopFlashTrayIcon(isFlash, isDisplayMainWindow);
            StartOrStopFlashTaskBar(isFlash);
        }

        private void AppendInitialize()
        {
            var hotKey = new Helper.HotKey(ModifierKeys.Control | ModifierKeys.Alt, System.Windows.Forms.Keys.A, this);
            //hotKey.HotKeyPressed += (k) => Console.Beep();
            hotKey.HotKeyPressed += (k) =>
            {
                string value = Views.Controls.ClipWindow.ShowClip();

                //通过快捷键截完图后，如果当前有选中的聊天框，则把截图添加到当前聊天框里的输入框
                if (_mainMV.ChatListVM.SelectedItem == null)
                {
                    return;
                }
                if (_mainMV.ChatListVM.SelectedItem.View is ChatView chatView)
                {
                    chatView.msgEditor.AddImageItem(value);
                }
            };

            App.Current.Exit += delegate { hotKey.Dispose(); };


            this.MaxHeight = Helper.PrimaryScreen.MaxAreaHeight + 7;
            this.MaxWidth = Helper.PrimaryScreen.MaxAreaWidth + 7;
            this.StateChanged += MainWindow_StateChanged;

            this.ppBusinessCard.Topmost = true;
        }

        public ViewModel ViewModel { get; private set; }

        public TaskScheduler UItaskScheduler { get; private set; }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.btnMax.IsChecked = true;
                this.gridLayout.Margin = new Thickness();
                this.stkpState.Margin = new Thickness(0, 5, 0, 0);
            }
            else
            {
                this.btnMax.IsChecked = false;
                this.stkpState.Margin = new Thickness();
                this.gridLayout.Margin = new Thickness(10);
            }

            if (this.WindowState == WindowState.Minimized)
            {
                this.Content = null;
            }
            else
            {
                this.Content = this.gridLayout;

                ChangeTaskBarAndTray();
            }
        }

        private void btnMax_Click(object sender, RoutedEventArgs e)
        {
            SetWindowMaxOrNormal();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (App.IsCancelOperate("关闭聊天窗口", "您有文件正在传输中，确定终止文件传输吗？") == true)
            {
                return;
            }
            this.Visibility = Visibility.Hidden;

        }

        private void rect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender == this.rectTop)
            {
                this.btnMax.IsChecked = !this.btnMax.IsChecked;
                SetWindowMaxOrNormal();
            }
            else
            {
                MainViewModel.CloseAppend?.Invoke();
                this.DragMove();
            }
        }

        private void SetWindowMaxOrNormal()
        {
            if (this.btnMax.IsChecked == true)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void Size_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (e.Source is System.Windows.Controls.Primitives.Thumb thumb)
            {
                double x = e.HorizontalChange, y = e.VerticalChange;
                double width = this.Width, height = this.Height, left = this.Left, top = this.Top;
                switch (thumb.Uid)
                {
                    case "Left":
                        width += -x;
                        left -= -x;
                        break;
                    case "Right":
                        width += x;
                        break;
                    case "Top":
                        top -= -y;
                        height += -y;
                        break;
                    case "Bottom":
                        height += y;
                        break;
                    case "LeftTop":
                        width += -x;
                        left -= -x;
                        top -= -y;
                        height += -y;
                        break;
                    case "LeftBottom":
                        width += -x;
                        left -= -x;
                        height += y;
                        break;
                    case "RightTop":
                        width += x;
                        top -= -y;
                        height += -y;
                        break;
                    case "RightBottom":
                        width += x;
                        height += y;
                        break;
                }

                if (width < this.MinWidth)
                {
                    width = this.MinWidth;
                    left = this.Left;
                }

                if (height < this.MinHeight)
                {
                    height = this.MinHeight;
                    top = this.Top;
                }

                this.Width = width;
                this.Height = height;
                this.Left = left;
                this.Top = top;
            }
        }

        private void ListBoxItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.btnState.IsChecked = false;
            this.popupState.IsOpen = false;

            ListBoxItem item = sender as ListBoxItem;
            _mainMV.CurrentCSState = item.DataContext as CSState;

            if(_mainMV.CurrentCSState.Value == (int)SDKClient.SDKProperty.customState.quit)
            {
                _mainMV.Logout(null);
            }
        }

        private void DockPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is IMCustomControls.IconStateButton item && item.CommandParameter is IListViewModel target)
            {
                AppData.MainMV.ListViewModel = target;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tbSearch_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void miCreateGroupChat_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            tbSearch.Text = string.Empty;
        }
    }
}
