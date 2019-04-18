using System;
using IMModels;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using SDKClient.Model;
using IMClient.Views.Panels;
using System.Windows.Media;
using System.Threading;

namespace IMClient.ViewModels
{
    /// <summary>
    /// 主VM 
    /// </summary>
    public class MainViewModel : ViewModel
    {
        public event Action<string> OnExpandGroupList;
        public static Action<bool> CloseAppend;
        public Action<string> ShowTip;
        public event Action<string> OnForceOffline;
        public event Action OnChangeTrayWindowSize;
        public event Action OnCloseTrayWindow;
        public event Action OnUpdatePrivacySetting;
        public event Action OnCloseAppendWindow;

        public bool IsOpenedAppendWindow { get; set; }

        public MainViewModel(IView view, LoginUser login) : base(view)
        {
            this.LoginUser = AppData.Current.InitializeLoginer(this, login);
            this.LoginUser.IsOnline = true;
            var obj = SDKClient.SDKClient.Instance.ScanNewVersion();
            if (obj.isUpdate)
            {
                SDKClient.SDKClient.Instance.GetLaunchFile();
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
        /// <summary>
        /// 双击定位到某个未读消息会话
        /// </summary>
        public VMCommand JumpChatItemCommand
        {
            get
            {
                return new VMCommand(JumpChatItem);
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
                    _showBusinessCard = new VMCommand(ShowUserInfoCard, new Func<object, bool>(o => o != null));
                return _showBusinessCard;
            }
        }

        #endregion 
        /// <summary>
        /// 打开个人名片
        /// </summary>
        /// <param name="para"></param>
        public void ShowUserInfoCard(object para)
        {
            AppData.MainMV.ShowUserBusinessCard(para);
        }
        public void ChangeTrayWindowSize()
        {
            this.OnChangeTrayWindowSize?.Invoke();
        }

        public void ForceOffline(string msg)
        {
            this.OnForceOffline?.Invoke(msg);
        }

        private void NaviToListView(object para)
        {
            if (para is IListViewModel current && this.ListViewModel != current)
            {

                App.Current.Dispatcher.Invoke(() =>
                {
                    this.ListViewModel = current;
                });
                if (para == this.ChatListVM)
                {
                    this.ChatListVM.SelectedItem?.Acitve();
                    //    this.SearchTip = "搜索聊天列表";
                    //}
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
                }
                this.SearchTip = "搜索";
            }

            this.OnCloseAppendWindow?.Invoke();
        }

        private void SearchUser(object para)
        {

        }
        int clickCount = 0;
        /// <summary>
        /// 双击定位到某个未读消息会话
        /// </summary>
        private void JumpChatItem(object obj)
        {
            var chatVMList = this.ChatListVM.Items.ToList();
            if (chatVMList.Count == 0)
            {
                clickCount = 0;
                return;
            }
            var v = chatVMList.Where(m => m.UnReadCount > 0 && !m.Chat.Chat.IsNotDisturb).Select((chatVM, index) => new { index, chatVM }).OrderByDescending(n => n.chatVM.Chat.LastMsg.SendTime).ToList();

            if (v.Count == 0)
            {
                v = chatVMList.Where(m => m.UnReadCount > 0 && m.Chat.Chat.IsNotDisturb).Select((chatVM, index) => new { index, chatVM }).OrderByDescending(n => n.chatVM.Chat.LastMsg.SendTime).ToList();
                if (v.Count == 0)
                    return;
            }

            if (clickCount > v.Count - 1)
                clickCount = 0;
            var chatItem = v[clickCount].chatVM;
            var objView = AppData.MainMV.ChatListVM.View as ChatListView;

            objView.list.Dispatcher.InvokeAsync(() =>
            {
                objView.list.UpdateLayout();
                Decorator decorator = (Decorator)VisualTreeHelper.GetChild(objView.list, 0);
                ScrollViewer scrollViewer = (ScrollViewer)decorator.Child;
                scrollViewer.ScrollToEnd();
                objView.list.ScrollIntoView(chatItem);
            });
            clickCount++;
            //AppData.MainMV.ChatListVM.SelectedItem = chatItem;
            //AppData.MainMV.ChatListVM.IsChecked = true;
        }

        public void GetUserPrivacySetting(GetUserPrivacySettingPackage pg)
        {
            if (pg == null || pg.data == null)
            {
                return;
            }

            UserModel userModel = AppData.Current.GetUserModel(pg.data.userId);
            userModel.IsReceiveStrangerMessage = pg.data.item.receiveAnonymousMsg;
            this.OnUpdatePrivacySetting?.Invoke();
        }

        public void UpdateUser(UpdateuserPackage pg)
        {
            if (pg == null || pg.data == null)
            {
                return;
            }


            if (pg.data.updateType == (int)UpdateUserOption.修改是否接收陌生信息)
            {
                UserModel userModel = AppData.Current.GetUserModel(AppData.Current.LoginUser.User.ID);
                userModel.IsReceiveStrangerMessage = pg.data.content.Equals("1") ? true : false;
                this.OnUpdatePrivacySetting?.Invoke();
            }
            else if (pg.data.updateType == (int)UpdateUserOption.修改头像)
            {
                UserModel userModel = AppData.Current.GetUserModel(AppData.Current.LoginUser.User.ID);
                var photo = pg.data.content.Split(',');
                if (photo.Length == 2)
                {
                    userModel.HeadImgMD5 = photo[1];
                    IMClient.Helper.ImageHelper.GetFriendFace(userModel.HeadImgMD5, (a) =>
                    {
                        userModel.HeadImg = a;
                    });
                }

            }
            else if (pg.data.updateType == (int)UpdateUserOption.修改可访号)
            {
                if (pg.code == 0)
                {
                    UserModel userModel = AppData.Current.GetUserModel(AppData.Current.LoginUser.User.ID);
                    userModel.KfNum = pg.data.content;
                    userModel.HaveModifiedKfid = 1;

                }
                else if (pg.code == 659)
                {
                    AppData.MainMV.TipMessage = "可访号已被修改！";
                }
                else
                {
                    AppData.MainMV.TipMessage = pg.error;
                }
            }

        }

        /// <summary>
        /// 直接跳跃到对应的群信息界面
        /// </summary>
        /// <param name="group"></param>
        public void JumpToGroupModel(GroupModel group)
        {
            this.GroupListVM.SelectedItem = this.GroupListVM.Items.FirstOrDefault(info => info.Model.ID == group.ID);
            this.ListViewModel = this.GroupListVM;
            this.GroupListVM.IsChecked = true;

            OnExpandGroupList?.Invoke(this.GroupListVM.SelectedItem.GroupTypeName);
            //this.SearchTip = "搜索我的群聊";
        }

        /// <summary>
        /// 直接跳跃到对应的聊天界面
        /// </summary>
        /// <param name="group"></param>
        public void JumpToChatModel(IChat chat, bool isJumpToChat = false, string tipMsg = "", bool isFileAssistant = false)
        {
            bool isGroup = (chat is GroupModel) ? true : false;
            var chatVM = this.ChatListVM.Items.ToList().FirstOrDefault(info => info.Model.ID == chat.ID && info.IsGroup == isGroup);
            if (!this.ChatListVM.ChatVMDic.ContainsKey(chat.ID, isGroup))
            {
                if (chatVM == null)
                {
                    ChatModel chatModel = new ChatModel(chat);

                    chatModel.LastMsg = new MessageModel()
                    {
                        SendTime = DateTime.Now,
                        TipMessage = tipMsg
                    };
                    chatVM = new ChatViewModel(chatModel);

                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (chatVM.IsGroup)
                            chatVM.IsShowGroupNoticeBtn = true;
                        else
                            chatVM.IsShowGroupNoticeBtn = false;
                        this.ChatListVM.Items.Add(chatVM);
                    }));
                }
            }
            if (!isFileAssistant)
            {
                bool isTemporaryChat;
                if (chat is UserModel userModel && (userModel.LinkDelType == 1 || userModel.LinkDelType == 3))
                {
                    chat.TopMostTime = chat.TopMostTime ?? DateTime.MinValue;
                    isTemporaryChat = true;

                    if (AppData.MainMV.ChatListVM.StrangerChatIds.Any(x => x == userModel.ID))
                    {
                        chatVM.CurrentChatType = ChatType.Stranger;
                    }
                    else
                    {
                        chatVM.CurrentChatType = ChatType.Temporary;
                    }

                    int sex = userModel.Sex == "女" ? 0 : 1;
                    ThreadPool.QueueUserWorkItem(async m =>
                    {
                        await SDKClient.SDKClient.Instance.InsertOrUpdateStrangerInfo(userModel.ID, userModel.HeadImgMD5, userModel.Name, sex);
                    });
                }
                else
                {
                    isTemporaryChat = false;
                }

                chatVM.IsTemporaryChat = isTemporaryChat;
                this.ChatListVM.DeleteStrangerItem(chatVM, isTemporaryChat);
                App.Current.Dispatcher.Invoke(() =>
                {
                    AppData.MainMV.ChatListVM.ResetSort();
                });

                if (chatVM.Chat.IsGroup)
                {
                    chatVM.IsShowGroupNoticeBtn = true;
                    SDKClient.SDKClient.Instance.UpdateChatRoomVisibility(chat.ID, 1, true);
                }
                else
                {
                    chatVM.IsShowGroupNoticeBtn = false;
                    SDKClient.SDKClient.Instance.UpdateChatRoomVisibility(chat.ID, 0, true);
                }
            }
            else
            {
                chatVM.IsFileAssistant = true;
                chatVM.CurrentChatType = ChatType.FileAssistant;
                chatVM.IsHideAppendButton = true;
                App.Current.Dispatcher.Invoke(() =>
                {
                    AppData.MainMV.ChatListVM.ResetSort();
                });
                chatVM.IsShowGroupNoticeBtn = false;
                SDKClient.SDKClient.Instance.UpdateChatRoomVisibility(chat.ID, 0, true);
            }
            if (isJumpToChat) return;
            this.ChatListVM.SelectedItem = chatVM;
            this.ListViewModel = this.ChatListVM;
            this.ChatListVM.IsChecked = true;

            //this.SearchTip = "搜索聊天列表";
        }
        bool _isAddFriend = false;
        ApplyFriendSource _type;
        string _sourceGroupID;
        string _sourceGroupName;
        public void ShowUserBusinessCard(object para,bool isAddFriend=false,ApplyFriendSource type= ApplyFriendSource.Other, string groupId="",string groupName="")
        {
            _isAddFriend = isAddFriend;
            _type = type;
            _sourceGroupID = groupId;
            _sourceGroupName = groupName;
            if (para is UserModel user)
            {
                if (AppData.MainMV.AttentionListVM.Items.Count > 0)
                {
                    var attentionList = AppData.MainMV.AttentionListVM.Items.ToList();
                    var tempAttention = attentionList.FirstOrDefault(m => m.Model.ID == user.ID);
                    if (tempAttention == null && user.IsAttention)
                        user.IsAttention = false;
                }
                else
                {
                    user.IsAttention = false;
                }
                if (AppData.MainMV.BlacklistVM != null && AppData.MainMV.BlacklistVM.Items.Count > 0)
                {
                    var blackUser = AppData.MainMV.BlacklistVM.Items.ToList().FirstOrDefault(m => m.Model.ID == user.ID);
                    if (blackUser != null && (blackUser.Model as UserModel).AttentionID != 0)
                    {
                        user.IsAttention = true;
                    }
                }
                ShowUserCard(user);
            }
            else if (para is GroupMember gm)
            {
                ShowUserCard(gm.TargetUser);
            }
            else if (para is ChatModel model && model.Chat is UserModel userModel)
            {
                ShowUserCard(userModel);
            }
        }

        public void ShowUserCard(UserModel user)
        {
            if (user.LinkDelType == 2)
            {
                if (this.FriendListVM.Items.ToList().Any(x => x.ID == user.ID))
                {
                    ShowDeleteFriendWindow(user);
                }
                else
                {
                    ShowUserBusinessCard(user);
                }
            }
            else
            {
                ShowUserBusinessCard(user);
            }
        }

        public void ShowUserBusinessCard(UserModel user)
        {
            this.UserBusinessCard = new UserViewModel(user);
            this.IsOpenBusinessCard = false;
            this.IsOpenBusinessCard = true;
            this.UserBusinessCard.IsAddFriend = _isAddFriend;
            this.UserBusinessCard.ApplyFriendSourceType = _type;
            this.UserBusinessCard.SourceGroupID = _sourceGroupID;
            this.UserBusinessCard.SourceGroupName = _sourceGroupName;
            user.IsApplyFriend = false;
            if (this.LoginUser.IsOnline)
                SDKClient.SDKClient.Instance.GetUser(user.ID);
        }

        private void ShowDeleteFriendWindow(UserModel user)
        {
            var result = Views.MessageBox.ShowDialogBox("对方已将你删除，点击确定则将好友从列表中移出！");
            if (result && AppData.CanInternetAction())
            {
                user.IsApplyFriend = false;
                user.IsAttention = false;
                user.NickName = null;
                user.DisplayName = user.Name;

                SDKClient.SDKClient.Instance.DeleteFriend(user.ID);
            }
        }

        public void UpdateUnReadMsgCount()
        {
            this.TotalUnReadCount = this.ChatListVM.Items.ToList().Where(info => !info.Chat.Chat.IsNotDisturb && (info.Chat.Chat is UserModel || (info.Chat.Chat is GroupModel && !string.IsNullOrEmpty(info.Chat.Chat.DisplayName)))).Sum(info => info.UnReadCount);

            if (this.TotalUnReadCount == 0)
            {
                this.OnCloseTrayWindow?.Invoke();
            }
        }

        #region Propertys

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
                this.SearchKey = string.Empty ;
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

        private FriendListViewModel _friendListVM;
        /// <summary>
        /// 好友列表VM
        /// </summary>
        public FriendListViewModel FriendListVM
        {
            get { return _friendListVM; }
            set { _friendListVM = value; this.OnPropertyChanged(); }
        }

        private GroupListViewModel _groupListVM;
        /// <summary>
        /// 群组列表VM
        /// </summary>
        public GroupListViewModel GroupListVM
        {
            get { return _groupListVM; }
            set
            {
                _groupListVM = value; this.OnPropertyChanged();
            }
        }

        private AttentionListViewModel _attentionListVM;
        /// <summary>
        /// 关注列表VM
        /// </summary>
        public AttentionListViewModel AttentionListVM
        {
            get { return _attentionListVM; }
            set { _attentionListVM = value; this.OnPropertyChanged(); }
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

        private UserSearchListViewModel _searchUserListVM;
        /// <summary>
        /// 查找用户列表VM
        /// </summary>
        public UserSearchListViewModel SearchUserListVM
        {
            get { return _searchUserListVM; }
            set { _searchUserListVM = value; this.OnPropertyChanged(); }
        }

        private BlacklistViewModel _blacklistVM;
        /// <summary>
        /// 黑名单列表
        /// </summary>
        public BlacklistViewModel BlacklistVM
        {
            get { return _blacklistVM; }
            set { _blacklistVM = value; this.OnPropertyChanged(); }
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
            get
            {
                if (_searchKey != null)
                    return _searchKey.Replace(" ", "");
                return _searchKey;
            }
            set
            {
                _searchKey = value;

                if (this.ChatListVM != null)
                {

                    if (_searchKey != null && !string.IsNullOrEmpty(_searchKey.Trim()))
                    {
                        this.IsSearchVisibility = true;
                        this.ChatListVM.Search(_searchKey.Replace(" ", ""));
                    }
                    else
                    {
                        this.ChatListVM.Search(_searchKey);
                        this.IsSearchVisibility = false;
                    }

                }
                this.OnPropertyChanged();

            }
        }
        private bool _isSearchVisibility;
        /// <summary>
        /// 搜索结果面板是否可见
        /// </summary>
        public bool IsSearchVisibility
        {
            get { return _isSearchVisibility; }
            set
            {
                _isSearchVisibility = value;
                this.OnPropertyChanged();

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

        #endregion
    }
}
