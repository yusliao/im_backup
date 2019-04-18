using IMClient.Views.Panels;
using IMModels;
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
    public class SendPersonCardViewModel : BaseModel
    {
        private int _chatID;
        private bool _isGroup;
        private bool _isFromCard;//是否点击的UserCard上面的按钮进入
        public DispatcherTimer timer = new DispatcherTimer();
        public UserModel _userModel;
        public SendPersonCardViewModel(int chatID, UserModel userModel, bool isFromCard, bool isGroup = false)
        {
            this._chatID = userModel != null ? userModel.ID : chatID;
            _userModel = userModel;
            this._isGroup = isGroup;
            this._isFromCard = isFromCard;
            LoadData(isFromCard);
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

        private bool _isOnlyShowFriends;
        /// <summary>
        /// 是否只显示好友
        /// </summary>
        public bool IsOnlyShowFriends
        {
            get { return _isOnlyShowFriends; }
            set
            {
                _isOnlyShowFriends = value;
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
            ChatInfo currentSelectItem = obj as ChatInfo;//当前选择的聊天条目
            ChatItems.ToList().ForEach(x => x.IsChatSelected = false);
            List<ChatInfo> tempChatInfos = ChatItems.Where(x => x.ChatID == currentSelectItem.ChatID && x.IsGroup == currentSelectItem.IsGroup).ToList();
            SelectedChatList.Clear();
            currentSelectItem.IsChatSelected = true;
            SelectedChatList.Add(currentSelectItem);
            if (tempChatInfos?.Count > 0)
            {
                tempChatInfos.ForEach(x => x.IsChatSelected = true);
            }
            if (SelectedChatList.Count > 0)
                IsEnabled = true;
            else
                IsEnabled = false;
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
                    if (user == null || user.ID == AppData.Current.LoginUser.ID)
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
            ChatInfo chatinfo = SelectedChatList.FirstOrDefault();

            List<FriendViewModel> listFriend = AppData.MainMV.FriendListVM.Items.ToList();
            List<GroupViewModel> listGroup = AppData.MainMV.GroupListVM.Items.ToList();
            List<ChatViewModel> listChat = AppData.MainMV.ChatListVM.Items.ToList();

            if (_isFromCard)
            {
                var chatvm = AppData.MainMV.ChatListVM.GetChat(chatinfo.ChatID, chatinfo.IsGroup);
                if (chatvm != null)
                {
                    if (_userModel == null) return;
                    //if (!AppData.CanInternetAction())
                    //    return;
                    //else
                    //{
                        var phone = string.Empty;
                        MessageModel msg = new MessageModel()
                        {
                            Sender = AppData.Current.LoginUser.User,
                            SendTime = DateTime.Now,
                            IsMine = true,
                            MsgType = MessageType.usercard,
                            MessageState = MessageStates.Loading,
                            Content = "[个人名片]",
                            TipMessage = "[个人名片]"+_userModel.DisplayName,
                            Target = new List<int>() { chatinfo.ID },
                        };
                        if (!string.IsNullOrEmpty(_userModel.PhoneNumber) && _userModel.PhoneNumber.Length == 11)
                        {
                            phone = _userModel.PhoneNumber.Remove(3, 4);
                            phone = phone.Insert(3, "****");
                        }
                        PersonCardModel pcm = new PersonCardModel()
                        {
                            PhoneNumber = phone,
                            PhotoImg = _userModel.HeadImg,
                            Name = _userModel.DisplayName,
                            UserId = _userModel.ID
                        };
                        msg.ContentMD5 = _userModel.HeadImgMD5;
                        msg.PersonCardModel = pcm;
                        chatvm.SendPersonCardToserver(msg);
                    }
                //}

            }
            else
            {
                ChatViewModel chatvm = AppData.MainMV.ChatListVM.GetChat(this._chatID, this._isGroup);
                FriendViewModel friendViewModel = AppData.MainMV.FriendListVM.Items.ToList().FirstOrDefault(x => x.ID == chatinfo.ChatID);
                //ChatViewModel chatVMNeedSend = AppData.MainMV.ChatListVM.Items.FirstOrDefault(x => x.ID == this._chatID);
                if (friendViewModel.ID == chatvm.ID)
                {
                    AppData.MainMV.TipMessage = "不能给该对象发送名片";
                    return;
                }
                if (chatvm != null && chatvm.Model is ChatModel chatModel)
                {
                    if (friendViewModel.Model is UserModel user)
                    {
                        //if (!AppData.CanInternetAction())
                        //    return;
                        //else
                        //{
                            var phone = string.Empty;
                            MessageModel msg = new MessageModel()
                            {
                                Sender = AppData.Current.LoginUser.User,
                                SendTime = DateTime.Now,
                                IsMine = true,
                                MessageState=MessageStates.Loading,
                                MsgType = MessageType.usercard,
                                Content = "[个人名片]",
                                TipMessage = "[个人名片]"+user.DisplayName,
                                Target = new List<int>() { chatinfo.ID },
                            };
                            if (!string.IsNullOrEmpty(user.PhoneNumber) && user.PhoneNumber.Length == 11)
                            {
                                phone = user.PhoneNumber.Remove(3, 4);
                                phone = phone.Insert(3, "****");
                            }
                            PersonCardModel pcm = new PersonCardModel()
                            {
                                PhoneNumber = phone,
                                PhotoImg = user.HeadImg,
                                Name = user.DisplayName,
                                UserId = user.ID
                            };
                            msg.ContentMD5 = user.HeadImgMD5;
                            msg.PersonCardModel = pcm;
                            chatvm.SendPersonCardToserver(msg);
                        }
                    //}
                }
            }




            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                AppData.MainMV.ChatListVM.ResetSort();
                Thread.Sleep(100);
                AppData.PersonCardWindow?.Close();
            }));


        }
        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="isFromCard">是否点击的是UserCard上面的入口</param>
        private void LoadData(bool isFromCard)
        {
            var tempChatList = AppData.MainMV.ChatListVM.Items.ToList();
            var tempFriendList = AppData.MainMV.FriendListVM.Items.ToList();
            var tempGroupList = AppData.MainMV.GroupListVM.Items.ToList();
            if (!isFromCard)
            {
                IsOnlyShowFriends = true;
            }
            if (tempFriendList?.Count > 0)
            {
                foreach (var friend in tempFriendList)
                {
                    if (friend.ID == -1)
                        continue;
                    var userModel = friend.Model as UserModel;
                    if (userModel == null || userModel.ID == AppData.Current.LoginUser.ID)
                        continue;
                    if (userModel.LinkType == 1 || userModel.LinkType == 3)
                        continue;
                    _chatItems.Add(new ChatInfo
                    {
                        ChatID = friend.ID,
                        IsGroup = false,
                        DisplayName = userModel != null ? userModel.DisplayName : string.Empty,
                        GroupTypeName = isFromCard ? "好友" : "",
                        HeadImg = userModel != null ? userModel.HeadImg : string.Empty
                    });
                }
            }
            if (isFromCard)
            {
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
                    var chatList = tempChatList.OrderByDescending(m => m.Chat.LastMsg.SendTime);
                    foreach (var chat in chatList)
                    {
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
                            if (user == null || user.ID == AppData.Current.LoginUser.ID)
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
}
