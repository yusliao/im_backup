using System;
using IMModels;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Collections;
using System.Collections.Generic;
using CSClient.Helper;
using System.Threading.Tasks;
using SDKClient.WebAPI;

namespace CSClient.ViewModels
{
    /// <summary>
    /// 主VM 
    /// </summary>
    public class MainViewModel : ViewModel
    {
        public static Action CloseAppend;
        public Action<string> ShowTip;
        public event Action OnForceOffline;
        public event Action OnChangeTrayWindowSize;
        public event Action OnCloseTrayWindow;

        public bool IsOpenedAppendWindow { get; set; }

        public bool IsAdmin { get; set; }

        public MainViewModel(IView view, LoginUser login) : base(view)
        {
            this.LoginUser = AppData.Current.InitializeLoginer(this, login);
            this.LoginUser.IsOnline = true;

            if (SDKClient.SDKClient.Instance.property.CurrentAccount.CustomProperty.Role == 1)
            {
                IsAdmin = true;
            }

            CSStates = new List<CSState>();
            foreach (SDKClient.SDKProperty.customState css in Enum.GetValues(typeof(SDKClient.SDKProperty.customState)))
            {
                CSState state = new CSState();
                state.Value = (int)css;
                state.Desc = CommonHelper.GetEnumDescription(css);
                state.Icon = string.Empty;
                switch (css)
                {
                    case SDKClient.SDKProperty.customState.working:
                        state.Icon = "pack://application:,,,/CSClient;component/Icons/online.png";
                        break;
                    case SDKClient.SDKProperty.customState.business:
                        state.Icon = "pack://application:,,,/CSClient;component/Icons/busy.png";
                        break;
                    case SDKClient.SDKProperty.customState.left:
                        state.Icon = "pack://application:,,,/CSClient;component/Icons/leave.png";
                        break;
                    default:
                        break;
                }

                CSStates.Add(state);
            }

            if (CSStates.Count > 0)
            {
                CurrentCSState = CSStates[0];
            }
        }

        #region Commands


        private VMCommand _naviCommand;
        /// <summary>
        /// 导航命令
        /// </summary>
        public VMCommand NaviCommand
        {
            get
            {
                if (_naviCommand == null)
                    _naviCommand = new VMCommand(NaviToListView, new Func<object, bool>(o => o != null));
                return _naviCommand;
            }
        }

        private VMCommand _searchUserCommand;
        /// <summary>
        /// 查找用户命令
        /// </summary>
        public VMCommand SearchUserCommand
        {
            get
            {
                if (_searchUserCommand == null)
                    _searchUserCommand = new VMCommand(SearchUser);
                return _searchUserCommand;
            }
        }

        private VMCommand _showBusinessCard;
        /// <summary>
        /// 个人名片命令
        /// </summary> 
        public VMCommand ShowBusinessCard
        {
            get
            {
                if (_showBusinessCard == null)
                    _showBusinessCard = new VMCommand(ShowUserBusinessCard, new Func<object, bool>(o => o != null));
                return _showBusinessCard;
            }
        }

        private VMCommand _logoutCommand;
        /// <summary>
        /// 退出登录
        /// </summary>
        public VMCommand LogoutCommand
        {
            get
            {
                if (_logoutCommand == null)
                    _logoutCommand = new VMCommand(Logout);
                return _logoutCommand;
            }
        }


        #endregion

        public void ChangeTrayWindowSize()
        {
            this.OnChangeTrayWindowSize?.Invoke();
        }

        public void ForceOffline()
        {
            this.OnForceOffline?.Invoke();
        }

        private void NaviToListView(object para)
        {
            if (para is IListViewModel current && this.ListViewModel != current)
            {

                this.ListViewModel = current;
                if (para == this.ChatListVM)
                {
                    this.ChatListVM.SelectedItem?.Acitve();
                    this.SearchTip = "搜索";
                }
                //else if (para == this.FriendListVM)
                //{
                //    this.SearchTip = "搜索通讯录";
                //}
                //else if (para == this.GroupListVM)
                //{
                //    this.SearchTip = "搜索我的群聊";
                //}
                //else if (para == this.AttentionListVM)
                //{
                //    this.SearchTip = "搜索我的关注";
                //}
            }

        }

        private void SearchUser(object para)
        {

        }

        /// <summary>
        /// 直接跳跃到对应的聊天界面
        /// </summary>
        /// <param name="group"></param>
        public void JumpToChatModel(IChat chat)
        {
            var chatVM = this.ChatListVM.Items.FirstOrDefault(info => info.Model.ID == chat.ID);

            if (chatVM == null)
            {
                ChatModel chatModel = new ChatModel(chat);
                chatModel.LastMsg = new MessageModel()
                {
                    SendTime = DateTime.Now,
                };
                chatVM = new ChatViewModel(chatModel);

                App.Current.Dispatcher.Invoke(() =>
                {
                    this.ChatListVM.Items.Add(chatVM);
                    AppData.MainMV.ChatListVM.ResetSort();
                });
            }

            this.ChatListVM.SelectedItem = chatVM;
            this.ListViewModel = this.ChatListVM;
            this.ChatListVM.IsChecked = true;

            this.SearchTip = "搜索";
        }


        public void ShowUserBusinessCard(object para)
        {
            if (para is UserModel user)
            {
                this.UserBusinessCard = new UserViewModel(user);
                this.IsOpenBusinessCard = false;
                this.IsOpenBusinessCard = true;
                user.IsApplyFriend = false;
                //SDKClient.SDKClient.Instance.GetUser(user.ID);
            }
            else if (para is GroupMember gm)
            {
                gm.TargetUser.IsApplyFriend = false;
                this.UserBusinessCard = new UserViewModel(gm.TargetUser);
                this.IsOpenBusinessCard = false;
                this.IsOpenBusinessCard = true;

                //SDKClient.SDKClient.Instance.GetUser(gm.TargetUser.ID);
            }
        }

        public void Logout(object para)
        {
            if (Views.MessageBox.ShowDialogBox("确定注销当前登录吗？") != true)
            {
                return;
            }

            SDKClient.SDKClient.Instance.SetCustiomServerState(SDKClient.SDKProperty.customState.quit);
            foreach (var item in this.ChatListVM.Items)
            {
                if (item.sessionType == 1)
                    Util.Helpers.Async.Run(async () => await SDKClient.SDKClient.Instance.SendCustiomServerMsg(item.ID.ToString(), item.SessionId, SDKClient.SDKProperty.customOption.over));
            }
            (App.Current as App).ApplicationExit(null, null);
            SDKClient.SDKClient.Instance.StopAsync().ConfigureAwait(false);
            Application.Current?.Shutdown(0);

            string mainProgramPath = string.Format(@"{0}\CSClient.exe", AppDomain.CurrentDomain.BaseDirectory);
            System.Diagnostics.Process.Start(mainProgramPath);
        }

        public void UpdateUnReadMsgCount()
        {
            this.TotalUnReadCount = this.ChatListVM.Items.Where(info => !info.Chat.Chat.IsNotDisturb && (info.Chat.Chat is UserModel || (info.Chat.Chat is GroupModel && !string.IsNullOrEmpty(info.Chat.Chat.DisplayName)))).Sum(info => info.UnReadCount);
            if (this.TotalUnReadCount == 0)
            {
                this.OnCloseTrayWindow?.Invoke();
            }
        }
        #region Propertys

        private QuickReplycontent _commonReply;
        public QuickReplycontent CommonReply
        {
            get { return _commonReply; }
            set { _commonReply = value; this.OnPropertyChanged(); }
        }

        private QuickReplycontent _personalReply;
        public QuickReplycontent PersonalReply
        {
            get { return _personalReply; }
            set { _personalReply = value; this.OnPropertyChanged(); }
        }

        private string _mainTitle;

        public string MainTitle
        {
            get { return _mainTitle; }
            set { _mainTitle = value; this.OnPropertyChanged(); }
        }


        private IListViewModel _listViewModel;

        public IListViewModel ListViewModel
        {
            get { return _listViewModel; }
            set
            {
                if (_listViewModel != null)
                {
                    _listViewModel.IsChecked = false;
                }
                _listViewModel = value;

                if (_listViewModel != null)
                {
                    _listViewModel.IsChecked = true;
                }
                this.OnPropertyChanged();
                this.SearchKey = null;
            }
        }

        private string _searchTip;
        /// <summary>
        /// 查找tip提示
        /// </summary>
        public string SearchTip
        {
            get { return _searchTip; }
            set { _searchTip = value; this.OnPropertyChanged(); }
        }


        private int _unReadCount;
        /// <summary>
        /// 未读消息数总量
        /// </summary>
        public int TotalUnReadCount
        {
            get { return _unReadCount; }
            set { _unReadCount = value; this.OnPropertyChanged(); }
        }


        private LoginUser _loginUser;
        /// <summary>
        /// 当前登录用户
        /// </summary>
        public LoginUser LoginUser
        {
            get { return _loginUser; }
            private set { _loginUser = value; }
        }

        private int _newMessageCount;
        /// <summary>
        /// 新消息数量（未读消息）
        /// </summary>
        public int NewMessageCount
        {
            get { return _newMessageCount; }
            set { _newMessageCount = value; this.OnPropertyChanged(); }
        }

        private ChatListViewModel _chatListVM;
        /// <summary>
        /// 聊天列表VM
        /// </summary>
        public ChatListViewModel ChatListVM
        {
            get { return _chatListVM; }
            set { _chatListVM = value; this.OnPropertyChanged(); }
        }
        private TempChatListViewModel _hischatListVM;
        /// <summary>
        /// 聊天列表VM
        /// </summary>
        public TempChatListViewModel HisChatListVM
        {
            get { return _hischatListVM; }
            set { _hischatListVM = value; this.OnPropertyChanged(); }
        }

        private HistoryChatListViewModel _chatHistoryListVM;
        /// <summary>
        /// 历史消息记录列表
        /// </summary>
        public HistoryChatListViewModel ChatHistoryListVM
        {
            get { return _chatHistoryListVM; }
            set { _chatHistoryListVM = value; this.OnPropertyChanged(); }
        }

        private SettingListViewModel _settingListVM;
        /// <summary>
        /// "更多"列表VM
        /// </summary>
        public SettingListViewModel SettingListVM
        {
            get { return _settingListVM; }
            set { _settingListVM = value; this.OnPropertyChanged(); }
        }

        private bool _isOpenBusinessCard;
        /// <summary>
        /// 是否打开个人名片
        /// </summary>
        public bool IsOpenBusinessCard
        {
            get { return _isOpenBusinessCard; }
            set { _isOpenBusinessCard = value; this.OnPropertyChanged(); }
        }

        private UserViewModel _userBusinessCard;
        /// <summary>
        /// 个人名片VM
        /// </summary>
        public UserViewModel UserBusinessCard
        {
            get { return _userBusinessCard; }
            private set { _userBusinessCard = value; this.OnPropertyChanged(); }
        }


        private string _searchKey;
        /// <summary>
        /// 搜索键值
        /// </summary>
        public string SearchKey
        {
            get { return _searchKey; }
            set
            {
                _searchKey = value; this.OnPropertyChanged();
                if (this.ListViewModel != null)
                {
                    this.ListViewModel.Search(_searchKey);
                }
            }
        }

        private string _tipMessage;
        /// <summary>
        /// 提示信息
        /// </summary>
        public string TipMessage
        {
            get { return _tipMessage; }
            set
            {
                _tipMessage = value;
                //this.OnPropertyChanged();

                if (!string.IsNullOrEmpty(value) && ShowTip != null)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        ShowTip.Invoke(value);
                    });
                }
                //if (!string.IsNullOrEmpty(value))
                //{
                //    this.ShowTipMessage = false;
                //    System.Threading.Tasks.Task.Run(() =>
                //    {
                //        System.Threading.Thread.CurrentThread.Join(10);
                //        this.ShowTipMessage = true;
                //    });
                //}
            }
        }

        //private bool _showTipMessage;
        ///// <summary>
        ///// 显示提示信息
        ///// </summary>
        //public bool ShowTipMessage
        //{
        //    get { return _showTipMessage; }
        //    private set { _showTipMessage = value; this.OnPropertyChanged(); }
        //}

        private CSState _currentCSState;

        public CSState CurrentCSState
        {
            get { return _currentCSState; }
            set
            {
                _currentCSState = value;
                this.OnPropertyChanged();

                SDKClient.SDKProperty.customState state = SDKClient.SDKProperty.customState.working;
                switch (value.Value)
                {
                    case (int)SDKClient.SDKProperty.customState.business:
                        state = SDKClient.SDKProperty.customState.business;
                        break;
                    case (int)SDKClient.SDKProperty.customState.left:
                        state = SDKClient.SDKProperty.customState.left;
                        break;
                    case (int)SDKClient.SDKProperty.customState.working:
                    default:
                        break;
                }

                var result = SDKClient.SDKClient.Instance.SetCustiomServerState(state);
                if (!result)
                {
                    //业务要求 ，状态上传失败，就一直上传
                    System.Threading.ThreadPool.QueueUserWorkItem(o =>
                    {
                        while (!result)
                        {
                            result = SDKClient.SDKClient.Instance.SetCustiomServerState(state);
                            System.Threading.Thread.Sleep(100);
                        }
                    });
                }
            }
        }

        private List<CSState> _csStates;

        public List<CSState> CSStates
        {
            get { return _csStates; }
            set { _csStates = value; this.OnPropertyChanged(); }
        }


        #endregion
    }

    public class CSState
    {
        public int Value { get; set; }
        public string Desc { get; set; }
        public string Icon { get; set; }
    }
}
