using IMModels;
using SDKClient.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

namespace IMClient.ViewModels
{
    public class FowardViewModel : BaseModel
    {
        private int _chatID;
        private string _msgID;
        private bool _isGroup;
        private bool _isInviteJoin;
        public DispatcherTimer timer = new DispatcherTimer();
        public FowardViewModel(int chatID, string msgID, bool isGroup = false, bool isInviteJoin = false)
        {
            _chatID = chatID;
            _msgID = msgID;
            _isGroup = isGroup;
            _isInviteJoin = isInviteJoin;
            LoadData();
        }
        #region 属性
        private ObservableCollection<ChatInfo> _chatItems = new ObservableCollection<ChatInfo>();
        /// <summary>
        /// 好友，群组，最新消息条目集合
        /// </summary>
        public ObservableCollection<ChatInfo> ChatItems

        {
            get
            {
                return _chatItems;
            }
            set
            {
                _chatItems = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<ChatInfo> _selectedChatList = new ObservableCollection<ChatInfo>();
        /// <summary>
        /// 选中聊天室的集合
        /// </summary>
        public ObservableCollection<ChatInfo> SelectedChatList
        {
            get { return _selectedChatList; }
            set
            {
                _selectedChatList = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<ChatInfo> _SearchChatList = new ObservableCollection<ChatInfo>();
        /// <summary>
        /// 搜索结果集合
        /// </summary>
        public ObservableCollection<ChatInfo> SearchChatList
        {
            get
            {
                return _SearchChatList;
            }
            set
            {
                _SearchChatList = value;
                OnPropertyChanged();
            }
        }

        private bool _hasSearchChatData;
        /// <summary>
        /// 是否有搜索结果
        /// </summary>
        public bool HasSearchChatData
        {
            get { return _hasSearchChatData; }
            set
            {
                _hasSearchChatData = value;
                OnPropertyChanged();
            }
        }

        private bool _hasResult;
        /// <summary>
        /// 是否有搜索结果
        /// </summary>
        public bool HasResult
        {
            get
            {
                return _hasResult;
            }
            set
            {
                _hasResult = value;
                OnPropertyChanged();
            }
        }
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }
        private int _chatCount;
        /// <summary>
        /// 选中个数
        /// </summary>
        public int ChatCount
        {
            get { return _chatCount; }
            set
            {
                _chatCount = value;
                OnPropertyChanged();
            }
        }
        private string _searchContactName;
        /// <summary>
        /// 搜索
        /// </summary>
        public string SearchContactName
        {
            get { return _searchContactName; }
            set
            {
                _searchContactName = value;
                OnPropertyChanged();
            }
        }

        private ChatInfo _SelectedChatItem;
        public ChatInfo SelectedChatItem
        {
            get { return _SelectedChatItem; }
            set
            {
                _SelectedChatItem = value;

                OnPropertyChanged();
            }
        }
        private bool isMsgExist;
        /// <summary>
        /// 消息是否存在
        /// </summary>
        public bool IsMsgExist
        {
            get { return isMsgExist; }
            set
            {
                isMsgExist = value;
                OnPropertyChanged();
            }
        }


        private bool isNoData;
        /// <summary>
        /// 是否有联系人数据
        /// </summary>
        public bool IsNoData
        {
            get { return isNoData; }
            set
            {
                isNoData = value;
                OnPropertyChanged();
            }
        }

        private bool _isMoreContact;
        public bool IsMoreContact
        {
            get { return _isMoreContact; }
            set
            {
                _isMoreContact = value;
                OnPropertyChanged();
            }
        }
        private string _fowardTips;
        /// <summary>
        /// 提示
        /// </summary>
        private string FowardTips
        {
            get { return _fowardTips; }
            set
            {
                _fowardTips = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #region 命令
        /// <summary>
        /// 删除选中聊天室命令
        /// </summary>
        public VMCommand RemoveSelectChat
        {
            get
            {
                return new VMCommand(RemoveSelectedChat);
            }
        }

        public VMCommand SelectedChatItemCommand
        {
            get
            {
                return new VMCommand(SelectedChatItemChanged);
            }
        }
        /// <summary>
        /// 确认选中聊天室命令
        /// </summary>
        public VMCommand SureSelectedChatCommand
        {
            get
            {
                return new VMCommand(SureSelectedChat);
            }
        }
        /// <summary>
        /// 根据关键字查询命令
        /// </summary>
        public VMCommand QueryConditionChanged
        {
            get { return new VMCommand(QueryCondition); }
        }
        /// <summary>
        ///删除搜索关键字
        /// </summary>
        public VMCommand DeleteCommand
        {
            get { return new VMCommand(DeleteSearcheStr); }
        }
        #endregion
        #region 方法
        /// <summary>
        /// 选择要转发的聊天室
        /// </summary>
        /// <param name="obj"></param>
        private void SelectedChatItemChanged(object obj)
        {

            var tempChatInfo = obj as ChatInfo;
            var tempSelectedItem = SelectedChatList.FirstOrDefault(m => m.ChatID == tempChatInfo.ChatID && m.IsGroup == tempChatInfo.IsGroup);
            var tempChatItems = ChatItems.Where(m => m.ChatID == tempChatInfo.ChatID && m.IsGroup == tempChatInfo.IsGroup)?.ToList();
            if (tempSelectedItem == null)
            {
                if (SelectedChatList.Count == 9)
                {
                    //IsMsgExist = true;
                    //FowardTips = "最多只能选择9个联系人";

                    IsMoreContact = true;
                    timer.Tick += new EventHandler(Timer_Tick);
                    timer.Interval = new TimeSpan(0, 0, 1);
                    timer.Start();

                    return;
                }
                SelectedChatList.Add(tempChatInfo);
                if (tempChatItems?.Count > 0)
                {
                    foreach (var chatItem in tempChatItems)
                    {
                        chatItem.IsChatSelected = true;
                    }
                }
            }
            else
            {
                SelectedChatList.Remove(tempSelectedItem);
                if (tempChatItems?.Count > 0)
                {
                    foreach (var chatItem in tempChatItems)
                    {
                        chatItem.IsChatSelected = false;
                    }
                }
            }
            if (SelectedChatList.Count > 0)
                IsEnabled = true;
            else
                IsEnabled = false;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            IsMoreContact = false;
            timer.Stop();
        }

        /// <summary>
        /// 删除关键字搜索
        /// </summary>
        /// <param name="obj"></param>
        private void DeleteSearcheStr(object obj)
        {
            SearchContactName = string.Empty;
        }
        /// <summary>
        ///  根据关键字查询好友、群组
        /// </summary>
        /// <param name="obj"></param>
        private void QueryCondition(object obj)
        {

            if (string.IsNullOrWhiteSpace(SearchContactName))
            {
                HasSearchChatData = false;
                HasResult = false;
                return;
            }
            SearchChatList.Clear();
            var tempFriendList = AppData.MainMV.FriendListVM.Items.ToList();
            var tempGroupList = AppData.MainMV.GroupListVM.Items.ToList();
            string key = this.SearchContactName.Trim();
            var searchs = this.ChatItems.Where(info => info.DisplayName.Contains(key))?.ToList();
            if (searchs?.Count == 0)
            {
                HasResult = true;
                return;
            }
            else
            {
                HasResult = false;
            }
            foreach (var chatItem in searchs)
            {
                if (chatItem.IsGroup)
                {
                    var group = tempGroupList.FirstOrDefault(m => m.ID == chatItem.ChatID && chatItem.IsGroup);
                    if (group == null)
                        continue;
                }
                else
                {
                    var user = tempFriendList.FirstOrDefault(m => m.ID == chatItem.ChatID && !chatItem.IsGroup);
                    if (user != null)
                    {
                        var userModel = user.Model as UserModel;
                        if (userModel.LinkType == 1 || userModel.LinkType == 3)
                            continue;
                    }
                    if (user == null)
                        continue;
                }
                if (SearchChatList.Count > 0)
                {
                    var tempChat = SearchChatList.FirstOrDefault(m => m.ChatID == chatItem.ChatID && m.IsGroup == chatItem.IsGroup);
                    if (tempChat == null)
                        SearchChatList.Add(chatItem);
                }
                else
                {
                    SearchChatList.Add(chatItem);
                }
            }
            if (SearchChatList.Count > 0)
                HasSearchChatData = true;

        }
        /// <summary>
        /// 删除选中的聊天室
        /// </summary>
        /// <param name="obj"></param>
        private void RemoveSelectedChat(object obj)
        {
            var tempChatInfo = obj as ChatInfo;
            var tempSelectedItem = SelectedChatList.FirstOrDefault(m => m.ChatID == tempChatInfo.ChatID && m.IsGroup == tempChatInfo.IsGroup);
            if (tempSelectedItem != null)
            {
                SelectedChatList.Remove(tempSelectedItem);
                var tempChatItems = ChatItems.Where(m => m.ChatID == tempChatInfo.ChatID && m.IsGroup == tempChatInfo.IsGroup)?.ToList();
                if (tempChatItems?.Count > 0)
                {
                    foreach (var chatItem in tempChatItems)
                    {
                        chatItem.IsChatSelected = false;
                    }
                }
            }
            if (SelectedChatList?.Count == 0)
                IsEnabled = false;

        }
        /// <summary>
        /// 确认选中聊天室转发
        /// </summary>
        /// <param name="obj"></param>
        private void SureSelectedChat(object obj)
        {
            var tempChatList = AppData.MainMV.ChatListVM.Items.ToList();

            var chatVM = tempChatList.FirstOrDefault(m => m.ID == _chatID && m.IsGroup == _isGroup);
            MessageModel message = null;
            if (chatVM != null && chatVM.Model is ChatModel chatModel)
            {
                var tempMessages = chatModel.Messages.ToList();
                message = tempMessages.FirstOrDefault(m => m.MsgKey == _msgID);
            }
            if (message == null)//判断消息是否已经被删除
            {
                //转发失败，源消息被删除
                // FowardTips = "转发失败，源消息已被删除";
                IsMsgExist = true;
                return;
            }
            if (!AppData.CanInternetAction())
            {
                return;
            }

            if (_isInviteJoin)
            {
                var ids = new List<int>();
                var groups = new List<int>();

                foreach (var chatItem in SelectedChatList)
                {
                    if (chatItem.IsGroup)
                    {
                        groups.Add(chatItem.ChatID);
                    }
                    else
                    {
                        ids.Add(chatItem.ChatID);
                    }
                }
                InviteJoinGroupPackage.Data data = new InviteJoinGroupPackage.Data();

                //var package = Util.Helpers.Json.ToObject<SDKClient.Model.InviteJoinGroupPackage>(message.MsgSource);
                if (message.Target is GroupModel group)
                {
                    data.groupId = group.ID;
                    data.groupIntroduction = group.GroupRemark;
                    data.groupName = group.DisplayName;
                    data.groupPhoto = group.HeadImgMD5;
                }
                if (message.MsgSource is InviteJoinGroupPackage invite)
                {
                    data.InviteUserId = invite.data.InviteUserId;
                    data.inviteUserName = invite.data.inviteUserName;
                    data.inviteUserPhoto = invite.data.inviteUserPhoto;
                }
                data.userIds = ids;
                data.targetGroupIds = groups;
                var msgId = SDKClient.SDKClient.Instance.InviteJoinGroup(data,true);
                //InviteJoinGroupPackage package = new InviteJoinGroupPackage();
                foreach (var chatItem in SelectedChatList)
                {
                    var tempChat = AppData.MainMV.ChatListVM.GetChat(chatItem.ChatID, chatItem.IsGroup, "", chatItem.ChatID == AppData.Current.LoginUser.ID);
                    var str = chatItem.IsGroup ? "group" : "single";
                    var strMsgId = msgId + chatItem.ChatID + str;
                    InviteJoinGroupPackage package = new InviteJoinGroupPackage()
                    {
                        id = msgId,
                        from = AppData.Current.LoginUser.ID.ToString(),
                        to = chatItem.ChatID.ToString(),
                        time = DateTime.Now,
                        data = data,
                    };
                    SDKClient.DB.messageDB msgDB = new SDKClient.DB.messageDB()
                    {
                        from = AppData.Current.LoginUser.ID.ToString(),
                        to = chatItem.ChatID.ToString(),
                        msgTime = DateTime.Now,
                        msgId = strMsgId,

                        optionRecord = 1,
                        roomId = chatItem.ChatID,
                        Source = Util.Helpers.Json.ToJson(package),
                        roomType = 0
                    };
                    msgDB.content = "群名片";
                    msgDB.msgType = nameof(MessageType.invitejoingroup);
                    tempChat.LoadHisMessage(msgDB, AppData.Current.LoginUser.ID, true);

                    SDKClient.SDKClient.Instance.AppendLocalData_InviteJoinGroupPackage(package, SDKClient.SDKProperty.MessageState.sendfaile, true).ConfigureAwait(true);
                }
            }
            else
            {
                foreach (var chatItem in SelectedChatList)
                {
                    var tempChat = AppData.MainMV.ChatListVM.GetChat(chatItem.ChatID, chatItem.IsGroup, "", chatItem.ChatID == AppData.Current.LoginUser.ID);
                    if (tempChat != null)
                    {
                        tempChat.Chat.LastMsg.TipMessage = message.TipMessage;
                        //if (_isInviteJoin)
                        //{
                        //    tempChat.SendInviteJoinGroupToserver();
                        //}
                        //else
                        tempChat.ReSend(message, true);
                    }
                }
            }


            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                AppData.MainMV.ChatListVM.ResetSort();
                Thread.Sleep(100);
                AppData.FowardWindow?.Close();
            }));


        }
        /// <summary>
        /// 初始化数据
        /// </summary>
        private void LoadData()
        {
            var tempChatList = AppData.MainMV.ChatListVM.Items.ToList();
            var tempFriendList = AppData.MainMV.FriendListVM.Items.ToList();
            var tempGroupList = AppData.MainMV.GroupListVM.Items.ToList();

            if (tempFriendList?.Count > 0)
            {
                foreach (var friend in tempFriendList)
                {
                    if (friend.ID == -1)
                        continue;
                    var userModel = friend.Model as UserModel;
                    if (userModel == null)
                        continue;
                    if (userModel.LinkType == 1 || userModel.LinkType == 3)
                        continue;
                    _chatItems.Add(new ChatInfo
                    {
                        ChatID = friend.ID,
                        IsGroup = false,
                        DisplayName = userModel != null ? userModel.DisplayName : string.Empty,
                        GroupTypeName = "好友",
                        HeadImg = userModel != null ? userModel.HeadImg : string.Empty
                    });
                }
            }
            if (tempGroupList?.Count > 0)
            {
                foreach (var group in tempGroupList)
                {
                    _chatItems.Add(new ChatInfo
                    {
                        ChatID = group.ID,
                        IsGroup = true,
                        DisplayName = group.Model is GroupModel groupModel ? groupModel.DisplayName : string.Empty,
                        GroupTypeName = "群组",
                        HeadImg = group.Model is GroupModel model ? model.HeadImg : string.Empty
                    });
                }
            }
            if (tempChatList?.Count > 0)
            {
                var chatList = tempChatList.Where(m => m.Chat.LastMsg != null).OrderByDescending(m => m.Chat.LastMsg.SendTime).ToList();
                foreach (var chat in chatList)
                {
                    //var chat = tempChatList[i];
                    var chatView = chat.Model as ChatModel;

                    if (chatView == null)
                        continue;
                    if (chatView.IsGroup)
                    {
                        var group = tempGroupList.FirstOrDefault(m => m.ID == chat.ID && chatView.IsGroup);
                        if (group == null)
                            continue;
                    }
                    else
                    {
                        var user = tempFriendList.FirstOrDefault(m => m.ID == chat.ID && !chatView.IsGroup);
                        if (user != null)
                        {
                            var userModel = user.Model as UserModel;
                            if (userModel.LinkType == 1 || userModel.LinkType == 3)
                                continue;
                        }
                        if (user == null)
                            continue;
                    }
                    if (chat.CurrentChatType == ChatType.Stranger || chat.CurrentChatType == ChatType.Temporary || chat.CurrentChatType == ChatType.Temporary_Replied)
                        continue;
                    var displayName = string.Empty;
                    var headImg = string.Empty;
                    if (chatView.Chat is GroupModel model)
                    {
                        displayName = model.DisplayName;
                        headImg = model.HeadImg;
                    }
                    else if (chatView.Chat is UserModel userModel)
                    {
                        displayName = userModel.DisplayName;
                        headImg = userModel.HeadImg;
                    }
                    _chatItems.Add(new ChatInfo
                    {
                        ChatID = chat.ID,
                        IsGroup = chatView.Chat is GroupModel,
                        DisplayName = displayName,
                        RecentlyChatMsg = chatView.LastMsg.TipMessage,
                        GroupTypeName = "最近消息",
                        IsRecentlyChatMsg = true,
                        HeadImg = headImg
                    });
                }
            }
            if (_chatItems?.Count > 0)
            {
                ResetGroupBy();
            }
            else
            {
                IsNoData = true;
            }
        }
        /// <summary>
        /// 重置排序
        /// </summary>
        private void ResetGroupBy()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                ICollectionView cv = CollectionViewSource.GetDefaultView(this.ChatItems);
                if (cv == null)
                {
                    return;
                }
                //cv.SortDescriptions.Clear();
                //cv.SortDescriptions.Add(new SortDescription("GroupTypeName", ListSortDirection.Ascending));
                //cv.SortDescriptions.Add(new SortDescription("ID", ListSortDirection.Descending));

                cv.GroupDescriptions.Clear();
                cv.GroupDescriptions.Add(new PropertyGroupDescription("GroupTypeName"));
            });
        }
        #endregion
    }
    public class ChatInfo : BaseModel
    {
        private int _chatID;
        /// <summary>
        /// 聊天室ID
        /// </summary>
        public int ChatID
        {
            get { return _chatID; }
            set
            {
                this._chatID = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// 头像
        /// </summary>
        public string HeadImg { get; set; }
        /// <summary>
        ///  显示名称
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string RecentlyChatMsg { get; set; }
        /// <summary>
        /// 最近消息时间
        /// </summary>
        public DateTime MsgDateTime { get; set; }
        /// <summary>
        /// 是否是最近消息
        /// </summary>
        public bool IsRecentlyChatMsg { get; set; } = false;
        private bool isChatSelected;
        /// <summary>
        /// 是否被选中
        /// </summary>
        public bool IsChatSelected
        {
            set
            {
                this.isChatSelected = value;
                OnPropertyChanged();
            }
            get { return this.isChatSelected; }
        }
        /// <summary>
        /// 分组类型
        /// </summary>
        public string GroupTypeName { get; set; }

        /// <summary>
        /// 是否是群组
        /// </summary>
        public bool IsGroup { get; set; }


    }
}
