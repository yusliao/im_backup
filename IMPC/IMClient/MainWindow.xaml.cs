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
using IMClient.Views.Panels;
using IMModels;
using IMClient.Views.ChildWindows;
using IMClient.Helper;
using System.Windows.Media.Animation;

namespace IMClient
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
        public AppendWindow AppendWindowView;

        public MainWindow(IMModels.LoginUser login)
        {
            InitializeComponent();

            this.GotFocus += MainWindow_GotFocus;
            this.Activated += MainWindow_Activated;
            this.Unloaded += MainWindow_Unloaded;
            this.PreviewMouseDown += MainWindow_MouseDown;

            _mainMV = new MainViewModel(this, login);
            this.DataContext = ViewModel = _mainMV;
            _mainMV.MainTitle = login.User.PhoneNumber;
            _mainMV.OnExpandGroupList += _mainMV_OnExpandGroupList;
            _mainMV.OnForceOffline += _mainMV_OnForceOffline;
            _mainMV.OnChangeTrayWindowSize += _mainMV_OnChangeTrayWindowSize;
            _mainMV.OnCloseTrayWindow += _mainMV_OnCloseTrayWindow;
            _mainMV.OnCloseAppendWindow += _mainMV_OnCloseAppendWindow;
            //初始化基本数据
            _mainMV.ChatListVM = new ChatListView().ViewModel as ChatListViewModel;
            _mainMV.ChatListVM.OnFlashIcon += ChatListVM_OnFlashIcon;
            _mainMV.ChatListVM.OnCloseTrayWindow += ChatListVM_OnCloseTrayWindow;
            _mainMV.FriendListVM = new FriendListView().ViewModel as FriendListViewModel;
            _mainMV.GroupListVM = new GroupListView().ViewModel as GroupListViewModel;
            _mainMV.AttentionListVM = new AttentionListView().ViewModel as AttentionListViewModel;
            _mainMV.SearchUserListVM = new UserSearchListView().ViewModel as UserSearchListViewModel;
            _mainMV.SettingListVM = new SettingListView().ViewModel as SettingListViewModel;
            _mainMV.FriendListVM.SearchChatListViewModel = _mainMV.ChatListVM;
            _mainMV.BlacklistVM = new BlacklistViewModel(new BlacklistModel());

            _mainMV.ChatListVM.StrangerMessage.View = new StrangerMessageView();
            _mainMV.ChatListVM.OnSelectedItemEvent += ChatListVM_OnSelectedItemEvent;
            FriendListViewModel.NewFriend.View = new FriendNewApplyView();
            SettingListViewModel.CommonSetting.View = new CommonSettingView();
            SettingListViewModel.Privacy.View = new PrivacyView();
            SettingListViewModel.HelpCenter.View = new HelpCenterView();
            SettingListViewModel.FeedBack.View = new FeedBackView();

            AppData.Current.InitializeDatasAsync();


            _mainMV.NaviCommand.Execute(_mainMV.ChatListVM);

            AppendInitialize();
            _mainMV.ShowTip += (tip) => { Views.MessageTip.ShowTip(tip); };



            this.Loaded += MainWindow_Loaded;
            this.LocationChanged += MainWindow_LocationChanged;
            this.SizeChanged += MainWindow_SizeChanged;
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void ChatListVM_OnSelectedItemEvent()
        {

            //_mainMV.ChatListVM.SelectedItem = chatVM;
            if (!_mainMV.ChatListVM.IsChecked)
            {
                _mainMV.ListViewModel = _mainMV.ChatListVM;
                _mainMV.ChatListVM.IsChecked = true;
            }

        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (AppendWindowView != null)
            {
                if (AppendWindowView.isInTheMainWindow)
                {
                    AppendWindowView.Height = e.NewSize.Height - 60;
                    AppendWindowView.Left = this.Left + e.NewSize.Width - AppendWindowView.Width;
                }
                else
                {
                    AppendWindowView.Left = this.Left + e.NewSize.Width - 10;
                    AppendWindowView.Height = e.NewSize.Height;
                }

            }
        }

        private void _mainMV_OnExpandGroupList(string groupTypeName)
        {
            (_mainMV.GroupListVM.View as GroupListView).ExpandGroupList(groupTypeName);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _mainMV.SettingListVM.LoadDatas();

            this._tempTop = this.Top;
            this._tempLeft = this.Left;
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
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
            }
            ChangeTaskBarAndTray();
        }
        private bool IsUpdateRead;
        void ChangeTaskBarAndTray()
        {
            StartOrStopFlashTaskBar(false);
            
            if (_mainMV != null && _mainMV.ListViewModel == _mainMV.ChatListVM && _mainMV.ChatListVM.SelectedItem != null)
            {
                _mainMV.ChatListVM.IsChangeSelected = false;
                if (_mainMV.ChatListVM.SelectedItem.UnReadCount == 0)
                {
                    return;
                }
                var unReadCount = _mainMV.ChatListVM.SelectedItem.UnReadCount;
                int roomType = _mainMV.ChatListVM.SelectedItem.IsGroup ? 1 : 0;
                if (_mainMV.ChatListVM.SelectedItem.ID > 0)
                {
                    if (!IsUpdateRead)
                    {
                        var tempTask = Task.Run(() =>
                        {
                            IsUpdateRead = true;
                            SDKClient.SDKClient.Instance.UpdateHistoryMsgIsReaded(_mainMV.ChatListVM.SelectedItem.ID, roomType, true, unReadCount);
                        }).ContinueWith(t =>
                        {
                            IsUpdateRead = false;
                        });
                    }
                }
                //SDKClient.SDKClient.Instance.UpdateHistoryMsgIsReaded(_mainMV.ChatListVM.SelectedItem.ID, roomType, true, _mainMV.ChatListVM.SelectedItem.UnReadCount);

                //int count = (_mainMV.ChatListVM.SelectedItem.Model as ChatModel).Messages.Count;
                //for (int i = 0; i < _mainMV.ChatListVM.SelectedItem.UnReadCount; i++)
                //{
                //    if (count - 1 - i >= 0)
                //    {
                //        string msgKey = (_mainMV.ChatListVM.SelectedItem.Model as ChatModel).Messages[count - 1 - i].MsgKey;
                //        SDKClient.SDKClient.Instance.UpdateHistoryMsgIsReaded(msgKey);
                //    }
                //}


                if (_mainMV.ChatListVM.SelectedItem.IsViewLoaded)
                {
                    _mainMV.ChatListVM.SelectedItem.TempUnReadCount = _mainMV.ChatListVM.SelectedItem.UnReadCount;
                    _mainMV.ChatListVM.SelectedItem.UnReadCount = 0;
                    _mainMV.ChatListVM.SelectedItem.HasNewGroupNotice = false;
                    _mainMV.ChatListVM.SelectedItem.noticeMessage.Clear();
                    //_mainMV.ChatListVM.SelectedItem.PopupGroupNoticeWindow();
                }

                _mainMV.ChatListVM.SelectedItem.DisplayAtButton();
                _mainMV.UpdateUnReadMsgCount();
                _mainMV.ChatListVM.TotalUnReadCount = _mainMV.TotalUnReadCount;

                App.Current.Dispatcher.Invoke(() =>
                {
                    if (_mainMV.TotalUnReadCount == 0)
                    {
                        this.OnCloseTrayWindow?.Invoke();
                    }
                    this.OnChangeTrayWindowSize?.Invoke();
                });
            }
        }

        private async void _mainMV_OnForceOffline(string msg = null)
        {
            await SDKClient.SDKClient.Instance.StopAsync();
            App.Current.Dispatcher.Invoke(() =>
            {
                App.IsCancelOperate(null, null);
                ForceOfflineWindow win = new ForceOfflineWindow(msg);
                win.ShowDialog();

                App.MUTEX?.Close();
                App.MUTEX = null;


                (App.Current as App).ApplicationExit(null, null);
                Application.Current?.Shutdown(0);

                string mainProgramPath = string.Format(@"{0}\IMUI.exe", AppDomain.CurrentDomain.BaseDirectory);
                System.Diagnostics.Process.Start(mainProgramPath);

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

            AppendWindowView?.Close();

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
            //if (App.IsCancelOperate("关闭聊天窗口", "您有文件正在传输中，确定终止文件传输吗？") == true)
            //{
            //    return;
            //}
            this.Visibility = Visibility.Hidden;

            AppendWindowView?.Close();
        }

        private void _mainMV_OnCloseAppendWindow()
        {
            AppendWindowView?.Close();
        }

        private void rect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender == this.rectTop2)
            {
                this.btnMax.IsChecked = !this.btnMax.IsChecked;
                SetWindowMaxOrNormal();

                AppendWindowView?.Close();
            }
            else
            {
                MainViewModel.CloseAppend?.Invoke(true);
                this.DragMove();
            }
        }

        double _tempTop;
        double _tempLeft;
        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            double topOffset = this.Top - this._tempTop;
            double leftOffset = this.Left - this._tempLeft;

            this._tempTop = this.Top;
            this._tempLeft = this.Left;
            if (AppendWindowView != null)
            {
                AppendWindowView.Top += topOffset;
                AppendWindowView.Left += leftOffset;
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

        private void miCreateGroupChat_Click(object sender, RoutedEventArgs e)
        {
            if (AppData.MainMV.FriendListVM == null || AppData.MainMV.FriendListVM.Items.Count == 0)
                return;
            var datas = AppData.MainMV.FriendListVM.Items.ToList();

            List<UserModel> source = new List<UserModel>();
            foreach (var d in datas)
            {

                var user = d.Model as UserModel;
                if (user == null)
                    continue;
                if (user.ID == AppData.Current.LoginUser.ID)
                    continue;
                if (user.LinkType == 1 || user.LinkType == 3)
                    continue;
                if (user.ID > 0)
                {
                    source.Add(user);
                    user.FirstChar = Helper.CommonHelper.GetFirstChar(user.DisplayName);
                    user.GroupByChar = Helper.CommonHelper.GetFirstChar(user.DisplayName, true);
                }
            }

            source = source.OrderBy(info => info.DisplayName).ToList();

            var selection = GroupMemberDealWindow.ShowInstance("发起群聊", source);
            if (selection != null)
            {
                AppData.MainMV.GroupListVM.GroupCreateCommand?.Execute(selection);

                //int count = selection.Count();
                ////包括当前用户总共3个人以上才可群聊
                //if (count<2)
                //{
                //    string tip = string.Empty;
                //    if (count == 1) //有一个直接跳到当前好友聊天页面并提示
                //    {
                //        UserModel user = selection.FirstOrDefault();
                //        FriendViewModel friednVM = AppData.MainMV.FriendListVM.Items.FirstOrDefault(info => info.ID == user.ID);
                //        if (friednVM != null)
                //        {
                //            friednVM.JupmToChatCommand?.Execute(user); 
                //        } 
                //    }
                //    _mainMV.TipMessage = "您所邀请群聊的好友未满3人,无法创建群聊！"; 
                //}
                //else  
                //{
                //    int noFriendCount = selection.Count(info => info.LinkDelType != 0);

                //    if (count - noFriendCount<2)
                //    {
                //        _mainMV.TipMessage = "您所邀请群聊的好友中，有人已将您删除或拉黑，未满3人无法创建群聊！";
                //        return;
                //    }

                //    ViewModels.GroupViewModel groupVM = new ViewModels.GroupViewModel(new IMModels.GroupModel(), "我创建的群");
                //    AppData.MainMV.GroupListVM.GroupCreateCommand?.Execute(selection);
                //}
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

        private void DockPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is IMCustomControls.IconStateButton item && item.CommandParameter is IListViewModel target)
            {
                AppData.MainMV.ListViewModel = target;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            tbSearch.Text = string.Empty;
        }

        private void tbSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            border.Background = new SolidColorBrush(Colors.White);
            border.BorderBrush = new SolidColorBrush(Color.FromRgb(222, 223, 223));
            border.BorderThickness = new Thickness(1);
        }

        private void tbSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            border.BorderBrush = new SolidColorBrush(Colors.Transparent);
            border.BorderThickness = new Thickness(0);
            border.Background = new SolidColorBrush(Color.FromRgb(239, 242, 247));
        }
    }
}
