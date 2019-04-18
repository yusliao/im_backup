using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using IMModels;
using Util;
using System.ComponentModel;
using System.Windows.Data;
using SDKClient.Model;
using IMClient.Views.ChildWindows;
using IMClient.Helper;
using System.Threading;
using SDKClient.DTO;
using SDKClient;
using System.Text.RegularExpressions;
using IMClient.Views.Panels;
using System.Windows.Controls;
using System.Windows.Media;

namespace IMClient.ViewModels
{
    /// <summary>
    /// 聊天列表VM
    /// </summary>
    public class ChatListViewModel : ListViewModel<ChatViewModel>
    {
        public event Action<bool, bool> OnFlashIcon;
        public event Action OnCloseTrayWindow;
        public event Action OnScrollIntoView;
        public event Action OnSelectedItemEvent;
        public Dictionary<int, bool, ChatViewModel> ChatVMDic = new Dictionary<int, bool, ChatViewModel>();
        public bool IsChangeSelected;
        public List<int> StrangerChatIds = new List<int>();

        public static System.Configuration.Configuration ExeConfig = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
        private List<string> StangerMessagsRetractKeys = new List<string>();
        /// <summary>
        /// 消息列表VM
        /// </summary>
        /// <param name="view"></param>
        public ChatListViewModel(IListView view) : base(view)
        {

            System.Threading.ThreadPool.QueueUserWorkItem(async (o) =>
            {
                await LoadHisChatsExAsync();
            });
            //this.Items.Add(StrangerMessage);
        }


        protected override IEnumerable<ChatViewModel> GetSearchResult(string key)
        {
            var tempItems = this.Items.ToList();
            return tempItems.Where(info => !string.IsNullOrEmpty((info.Model as ChatModel).Chat.DisplayName) && (info.Model as ChatModel).Chat.DisplayName.Contains(key));
        }
        //public void Go



        /// <summary>
        /// 判断是否是数字
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static bool InputIsNum(string strText)
        {
            Regex regex = new Regex("^[0 - 9]*$");
            return regex.IsMatch(strText);
        }
        /// <summary>
        /// 判断输入的字符串是否是一个合法的手机号
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsPhoneNo(string str_handset)
        {
            return Regex.IsMatch(str_handset, "^(0\\d{2,3}-?\\d{7,8}(-\\d{3,5}){0,1})|(((13[0-9])|(15([0-3]|[5-9]))|(18[0-9])|(17[0-9])|(14[0-9]))\\d{8})$");
        }

        #region Propertys

        private ChatViewModel _strangerMessage;
        public ChatViewModel StrangerMessage
        {
            get
            {
                if (_strangerMessage == null)
                {
                    UserModel user = new UserModel();
                    user.HeadImg = IMAssets.ImageDeal.StrangerMessageImage;
                    user.ID = -2;
                    user.DisplayName = "粉丝留言";

                    user.TopMostTime = DateTime.MinValue;
                    user.IsTopMost = false;
                    string topTime = string.Empty;
                    if (ExeConfig.AppSettings.Settings["StrangerChatTopTime"] != null)
                        topTime = ExeConfig.AppSettings.Settings["StrangerChatTopTime"].Value;
                    else
                        topTime = DateTime.MinValue.ToString();
                    if (!string.IsNullOrEmpty(topTime) && DateTime.TryParse(topTime, out DateTime dt))
                    {
                        user.TopMostTime = dt;
                        user.IsTopMost = dt == DateTime.MinValue ? false : true;
                    }

                    user.IsNotDisturb = true;
                    string isNotDisturb = string.Empty;
                    if (ExeConfig.AppSettings.Settings["StrangerMessagedoNotDisturb"] != null)
                        isNotDisturb = ExeConfig.AppSettings.Settings["StrangerMessagedoNotDisturb"].Value;
                    else
                        isNotDisturb = "0";

                    if (!string.IsNullOrEmpty(isNotDisturb) && isNotDisturb.Equals("0"))
                    {
                        user.IsNotDisturb = false;
                    }

                    ChatModel chatModel = new ChatModel(user);
                    chatModel.LastMsg = new MessageModel()
                    {
                        SendTime = DateTime.Now,
                    };
                    _strangerMessage = new ChatViewModel(chatModel);
                    _strangerMessage.UnReadCount = 0;
                    _strangerMessage.IsHideAppendButton = false;//隐藏右键菜单的“开启消息提醒”
                }
                return _strangerMessage;
            }
            set
            {
                _strangerMessage = value;
                this.OnPropertyChanged();
            }
        }

        private ChatViewModel _selectedItem;
        /// <summary>
        /// 当前选项
        /// </summary>
        public override ChatViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                this.PriorSelectedItem = _selectedItem;

                if (_selectedItem != value)
                {

                    _selectedItem?.AtMeDic.Clear();

                    _selectedItem = value;
                    IsChangeSelected = true;
                    if (value == null) //当为空时，可以设置默认界面，暂不处理
                    {

                    }
                    else
                    {

                        var item = this.Items.ToList().FirstOrDefault(m => m.ID == _selectedItem.ID && m.IsGroup == _selectedItem.IsGroup);

                        if (item != null)
                            _selectedItem = item;
                        else
                        {
                            if (_selectedItem.Chat != null)
                            {
                                if (_selectedItem.IsFileAssistant)
                                    AppData.MainMV.JumpToChatModel(_selectedItem.Chat.Chat, false, "", true);
                                else
                                    AppData.MainMV.JumpToChatModel(_selectedItem.Chat.Chat);
                            }
                        }
                        if (_selectedItem.View == null)
                        {
                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                SelectedItem.View = View.GetItemView(_selectedItem);
                            }));
                        }
                        if (!this.IsChecked)
                        {
                            IsChecked = true;
                            if (AppData.MainMV.ListViewModel != this)
                                AppData.MainMV.ListViewModel = this;
                        }
                        else
                        {

                        }
                        if (AppData.IsGlobalSearch)
                        {
                            var objView = AppData.MainMV.ChatListVM.View as ChatListView;

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                objView.list.UpdateLayout();
                                Decorator decorator = (Decorator)VisualTreeHelper.GetChild(objView.list, 0);
                                ScrollViewer scrollViewer = (ScrollViewer)decorator.Child;
                                scrollViewer.ScrollToEnd();
                                objView.list.ScrollIntoView(_selectedItem);
                            });
                            AppData.IsGlobalSearch = false;
                        }
                    }
                }

                this.OnPropertyChanged();
            }
        }

        public void IsCloseTrayWindow(bool isActive = false)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (AppData.MainMV.ListViewModel != this)
                {
                    AppData.MainMV.ListViewModel = this;
                }

                AppData.MainMV.UpdateUnReadMsgCount();
                TotalUnReadCount = AppData.MainMV.TotalUnReadCount;
                if (TotalUnReadCount == 0)
                {
                    this.OnCloseTrayWindow?.Invoke();
                }
                else
                {
                    this.OnFlashIcon?.Invoke(true, true);
                }
                AppData.MainMV.ChangeTrayWindowSize();
                if (isActive)
                {
                    App.Current.MainWindow.Activate();
                }
            });
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

        #endregion

        public void CloseTrayWindow(bool isDisplayMainWindow = true)
        {
            this.OnCloseTrayWindow?.Invoke();
            this.OnFlashIcon?.Invoke(false, false);
        }

        public void IgnoreAllNewMessages()
        {
            var tempItems = this.Items.ToList();
            foreach (var item in tempItems)
            {
                if (item.UnReadCount > 0 && !item.Chat.Chat.IsNotDisturb)
                {
                    item.IsIgnoreAllMessage = true;
                    item.HasNewGroupNotice = false;
                    item.HasAtMsg = false;
                    var unReadCount = item.UnReadCount;
                    //ThreadPool.QueueUserWorkItem(o =>
                    //{
                    SDKClient.SDKClient.Instance.UpdateHistoryMsgIsReaded(item.ID, item.IsGroup ? 1 : 0, true, unReadCount);
                    //});
                    item.UnReadCount = 0;
                    if (item.UnReadMsgs != null && item.UnReadMsgs.Count > 0)
                        item.UnReadMsgs = item.UnReadMsgs.Select(m =>
                        {
                            if (m.IsRead == 0)
                                m.IsRead = 1; return m;
                        }).ToList();
                    //item.Acitve();
                }
                //if (item.UnReadCount > 0)
                //{

                //}

            }

            foreach (var chat in this.StrangerMessage.StrangerMessageList)
            {
                chat.UnReadCount = 0;
                foreach (var item in chat.Chat.Messages)
                {
                    chat.SetOneMsgRead(item.MsgKey);
                }
            }

            AppData.MainMV.UpdateUnReadMsgCount();
            TotalUnReadCount = AppData.MainMV.TotalUnReadCount;
            this.OnFlashIcon?.Invoke(false, false);
        }

        /// <summary>
        /// 加载本地历史消息
        /// </summary>
        private async Task LoadHisChatsExAsync()
        {
            var task = await SDKClient.SDKClient.Instance.GetRoomContextList();


            //    App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            //{
            await Task.Run(async () =>
            {

                foreach (var item in task)
                {
                    try
                    {
                        IChat chat = null;
                        IChat lastSender;
                        int fromID = item.LastMessage.From.ToInt();
                        int toId = item.LastMessage.To.ToInt();
                        bool isMine = AppData.Current.LoginUser.User.ID == fromID;

                        if (item.ChatType == SDKClient.SDKProperty.chatType.chat)//单聊
                        {
                            if (item.LastMessage.SessionType != (int)SDKClient.SDKProperty.SessionType.FileAssistant && item.RoomId == AppData.Current.LoginUser.ID)
                            {
                                continue;//把自己移除聊天列表
                            }
                            if (AppData.DeleteFriendIDItems.Count > 0)
                            {
                                var deleteFriend = AppData.DeleteFriendIDItems.FirstOrDefault(m => m == item.RoomId);
                                if (deleteFriend != 0)
                                {
                                    AppData.DeleteFriendIDItems.Remove(deleteFriend);
                                    continue;
                                }
                            }

                            var tempMessage = Util.Helpers.Json.ToObject<MessagePackage>(item.LastMessage.Source);
                            if (tempMessage != null && tempMessage.data != null && tempMessage.data.type == nameof(SDKProperty.chatType.groupChat))
                            {
                                GroupModel group = AppData.Current.GetGroupModel(item.RoomId);
                                chat = group;

                                UserModel user = AppData.Current.GetUserModel(fromID);
                                lastSender = user.GetInGroupMember(group);

                                switch (item.LastMessage.MsgType.ToLower())
                                {
                                    case nameof(MessageType.notification):
                                    case nameof(MessageType.setmemberpower):
                                    case nameof(MessageType.exitgroup):
                                    case nameof(MessageType.dismissgroup):
                                    case nameof(MessageType.invitejoingroup):
                                        lastSender = null;
                                        break;
                                    case nameof(MessageType.addgroupnotice):
                                        lastSender = user;
                                        break;
                                    case nameof(MessageType.deletegroupnotice):
                                        lastSender = user;
                                        break;
                                    default:
                                        if (string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(lastSender.DisplayName))
                                        {
                                            lastSender.DisplayName = item.LastMessage.SenderName;
                                        }

                                        break;
                                }
                                SDKClient.SDKClient.Instance.GetGroupMember(AppData.Current.LoginUser.ID, group.ID, fromID);
                            }
                            else
                            {
                                if (item.LastMessage != null && item.LastMessage.SessionType == (int)SDKClient.SDKProperty.SessionType.FileAssistant && item.RoomId == AppData.Current.LoginUser.ID)
                                {
                                    UserModel user = new UserModel()
                                    {
                                        ID = AppData.Current.LoginUser.ID,
                                        DisplayName = "文件小助手",
                                        HeadImg = IMAssets.ImageDeal.FileAssistantIcon,
                                        TopMostTime = DateTime.MinValue
                                    };
                                    lastSender = chat = user;
                                }
                                else
                                {
                                    UserModel user = AppData.Current.GetUserModel(item.RoomId);
                                    if (string.IsNullOrEmpty(user.DisplayName) && item.RoomId != 0)
                                        SDKClient.SDKClient.Instance.GetUser(item.RoomId);
                                    lastSender = chat = user;
                                }
                            }
                        }
                        else if (item.ChatType == SDKClient.SDKProperty.chatType.groupChat)//群聊
                        {
                            GroupModel group = AppData.Current.GetGroupModel(item.RoomId);
                            chat = group;

                            UserModel user = AppData.Current.GetUserModel(fromID);
                            lastSender = user.GetInGroupMember(group);

                            switch (item.LastMessage.MsgType.ToLower())
                            {
                                case nameof(MessageType.notification):
                                case nameof(MessageType.setmemberpower):
                                case nameof(MessageType.exitgroup):
                                case nameof(MessageType.dismissgroup):
                                case nameof(MessageType.invitejoingroup):
                                    lastSender = null;
                                    break;
                                case nameof(MessageType.addgroupnotice):
                                    lastSender = user;
                                    break;
                                case nameof(MessageType.deletegroupnotice):
                                    lastSender = user;
                                    break;
                                default:
                                    if (string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(lastSender.DisplayName))
                                    {
                                        lastSender.DisplayName = item.LastMessage.SenderName;
                                    }

                                    break;
                            }
                            SDKClient.SDKClient.Instance.GetGroupMember(AppData.Current.LoginUser.ID, group.ID, fromID);
                        }
                        else
                        {
                            continue;
                        }

                        var last = new MessageModel()
                        {
                            SendTime = item.LastMessage.MsgTime,
                            Content = item.LastMessage.Content,
                            Sender = lastSender,
                            IsMine = isMine,
                            MsgKey = item.LastMessage.MsgId,
                        };
                        if (item.LastMessage.SessionType == (int)SDKClient.SDKProperty.SessionType.FileAssistant)
                        {
                            last.SendTime = DateTime.Now;
                            last.HistorySendTime = item.LastMessage.SessionType == (int)SDKClient.SDKProperty.SessionType.FileAssistant ? item.LastMessage.MsgTime.ToString() : string.Empty;
                        }

                        MessageType type;
                        try
                        {
                            type = (MessageType)Enum.Parse(typeof(MessageType), item.LastMessage.MsgType.ToLower());
                        }
                        catch
                        {
                            continue;
                        }

                        if (type == MessageType.redenvelopesreceive || (type == MessageType.redenvelopessendout && isMine))//不显示对方接收的红包和自己发送的红包消息
                            continue;

                        switch (type)
                        {
                            case MessageType.img:
                                last.Content = "[图片]";
                                break;
                            case MessageType.file:
                            case MessageType.onlinefile:
                                last.Content = "[文件]";
                                break;
                            case MessageType.invitejoingroup:
                                last.MsgSource = Util.Helpers.Json.ToObject<SDKClient.Model.InviteJoinGroupPackage>(item.LastMessage.Source);
                                last.Content = "[群名片]";
                                break;
                            case MessageType.audio:
                                last.Content = "[语音]";
                                break;
                            case MessageType.smallvideo:
                            case MessageType.video:
                                last.Content = "[小视频]";
                                break;
                            //case MessageType.bigtxt:
                            //    last.Content = "[大文本]";
                            //    break;
                            case MessageType.exitgroup:
                            case MessageType.dismissgroup:
                                break;
                            case MessageType.addgroupnotice:
                                string title = item.LastMessage.Data.title;
                                last.Content = title;
                                break;
                            case MessageType.deletegroupnotice:
                                string title1 = item.LastMessage.Data.title;
                                last.Content = title1;
                                break;
                            case MessageType.usercard:
                                string userName = item.LastMessage.Data.name;
                                string cardcontent = item.LastMessage.Content;
                                last.Content = cardcontent + userName;
                                break;
                            default:
                                string content = item.LastMessage.Content;
                                last.Content = content;
                                break;
                        }
                        last.TipMessage = last.Content;

                        //App.Current.Dispatcher.Invoke(new Action(async () =>
                        //{
                        if (item.LastMessage.SessionType == (int)SDKClient.SDKProperty.SessionType.ReceiverLeavingChat)
                        {
                            SDKClient.SDKClient.Instance.GetUser(chat.ID);
                            item.StrangerMsgList.Reverse();
                            foreach (var strangerMsg in item.StrangerMsgList)
                            {
                                MessageModel msgModel = new MessageModel
                                {
                                    SendTime = strangerMsg.MsgTime,
                                    Content = strangerMsg.Content,
                                    Sender = lastSender,
                                    IsMine = false,
                                    MsgKey = strangerMsg.MsgId,
                                    IsSync = strangerMsg.OptionRecord == 1 ? true : false,
                                    MsgType = (MessageType)Enum.Parse(typeof(MessageType), strangerMsg.MsgType.ToLower()),
                                    TipMessage = strangerMsg.Content,
                                };
                                if (!ExitStrangerMsgRetract(strangerMsg.MsgId))
                                    LoadStrangerChat(chat.ID, msgModel);
                                else if (last.Content == strangerMsg.Content && last.MsgKey == strangerMsg.MsgId)
                                {
                                    if (lastSender.ID == AppData.Current.LoginUser.ID)
                                    {
                                        last.TipMessage = "您撤回了一条消息";
                                    }
                                    else
                                    {
                                        last.TipMessage = "对方撤回了一条消息";
                                    }

                                }

                            }
                            var friend = AppData.MainMV.FriendListVM.Items.ToList().FirstOrDefault(info => info.ID == chat.ID);
                            if (friend != null)
                            {
                                var chatListVM = this.Items.ToList().FirstOrDefault(m => m.ID == chat.ID);
                                DeleteStrangerItem(chatListVM, false);
                            }
                        }
                        else
                        {
                            var isGroup = item.ChatType == SDKClient.SDKProperty.chatType.groupChat;
                            var tempItem = Items.ToList().FirstOrDefault(m => m.ID == chat.ID && m.IsGroup == isGroup);
                            if (ChatVMDic.ContainsKey(chat.ID, isGroup) && tempItem != null)
                            {
                                //tempItem.UnReadCount += item.UnReadCount;
                                //tempItem.FirstMessages = item.PreloadLists;
                                if (tempItem.UnReadMsgs != null && tempItem.UnReadMsgs.Count > 0)
                                {
                                    var tempUnReadMsgs = tempItem.UnReadMsgs.ToList().OrderByDescending(m => m.SendTime).ToList();
                                    var tempUnReadMsg = tempUnReadMsgs[0];
                                    if (tempItem.Chat?.LastMsg != null)
                                    {
                                        if (tempItem != null && tempItem.Chat.LastMsg.MsgKey != tempUnReadMsg.MsgKey && tempItem.Chat.LastMsg.SendTime <= tempUnReadMsg.SendTime)
                                        {
                                            if (!string.IsNullOrEmpty(tempItem.Chat.LastMsg.RetractId))
                                            {
                                                tempItem.Chat.LastMsg.TipMessage = tempUnReadMsg.TipMessage;
                                                //item.Chat.LastMsg.SendTime = tempUnReadMsg.SendTime;
                                            }
                                        }
                                    }
                                    var tempRetracts = tempUnReadMsgs.Where(m => !string.IsNullOrEmpty(m.RetractId)).ToList();
                                    if (tempRetracts?.Count > 0)
                                    {
                                        foreach (var retractMsg in tempRetracts)
                                        {
                                            var tempMsg = tempItem.FirstMessages.FirstOrDefault(m => m.MsgId == retractMsg.RetractId);
                                            if (tempMsg != null)
                                            {
                                                retractMsg.SendTime = tempMsg.MsgTime;
                                                tempItem.FirstMessages.Remove(tempMsg);
                                            }

                                        }
                                    }
                                }
                                //foreach (var unRead in item.UnReadList)
                                //{
                                //    MessageModel messageModel = new MessageModel();
                                //    var msgEntity = new MessageModel();
                                //    tempItem.ChatMsgFomat(unRead, ref msgEntity, true);
                                //    if (!string.IsNullOrEmpty(unRead.TokenIds) && unRead.MsgType != null && unRead.MsgType == nameof(MessageType.retract))
                                //    {
                                //        messageModel.IsAtMeMsg = true;
                                //    }
                                //    if (unRead.MsgType != nameof(MessageType.deletegroupnotice))
                                //        tempItem.UnReadMsgs.Add(messageModel);
                                //}


                            }
                            var chatVM = tempItem;
                            var model = tempItem != null ? tempItem.Model as ChatModel : null;

                            if (chatVM == null)
                            {
                                model = AppData.Current.GetChatViewModel(chat);
                                chatVM = new ChatViewModel(model, last);
                                App.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    if (AppData.DismissGroupIDItems.Count > 0)
                                    {
                                        var dismissGroup = AppData.DismissGroupIDItems.FirstOrDefault((m) =>
                                        {
                                            if (m is DismissGroupPackage dis && dis.data.groupId == chat.ID && dis.data.ownerId == AppData.Current.LoginUser.ID)
                                            {
                                                return true;
                                            }
                                            else if (m is ExitGroupPackage exit && exit.data.groupId == chat.ID && exit.data.userIds.Contains(AppData.Current.LoginUser.ID))
                                            {
                                                return true;
                                            }
                                            return false;
                                        });
                                        //.FirstOrDefault(m => m.data.groupId == chat.ID);

                                        if (dismissGroup != null)
                                        {
                                            return;
                                        }
                                    }
                                    if (!ChatVMDic.ContainsKey(chat.ID, model.IsGroup))
                                    {
                                        ChatVMDic.Add(chat.ID, model.IsGroup, chatVM);
                                        Items.Add(chatVM);
                                    }
                                }));
                            }
                            if (item.LastMessage != null && item.LastMessage.SessionType == (int)SDKClient.SDKProperty.SessionType.FileAssistant && item.RoomId == AppData.Current.LoginUser.ID)
                            {
                                chatVM.IsFileAssistant = true;
                                chatVM.IsHideAppendButton = true;
                                var currentUesr = SDKClient.SDKClient.Instance.GetAccount();
                                (chatVM.Chat.Chat as UserModel).TopMostTime = currentUesr.TopMostTime ?? DateTime.MinValue;
                                if (currentUesr.TopMostTime != null && currentUesr.TopMostTime.Value != DateTime.MinValue)
                                    (chatVM.Chat.Chat as UserModel).IsTopMost = true;
                            }
                            chatVM.FirstMessages = item.PreloadLists;

                            if (item.LastMessage.SessionType != (int)SDKClient.SDKProperty.SessionType.FileAssistant && item.LastMessage.SessionType != (int)SDKClient.SDKProperty.SessionType.CommonChat)
                            {
                                //var isCommonChat = true;
                                //if (AppData.MainMV.FriendListVM.Items.Count > 0)
                                //{
                                //    var tempFriendListItems = AppData.MainMV.FriendListVM.Items.ToList();
                                //    var friendItem = tempFriendListItems.FirstOrDefault(m => m.Model is UserModel user && user.ID == chatVM.Chat.Chat.ID);
                                //    if (friendItem != null)
                                //        isCommonChat = false;
                                //}
                                chatVM.IsTemporaryChat = true;
                                var userId = chatVM.Chat.Chat.ID;
                                var s = await SDKClient.SDKClient.Instance.GetStranger(userId);
                                if (s != null)
                                {
                                    (chatVM.Chat.Chat as UserModel).TopMostTime = s.ChatTopTime ?? DateTime.MinValue;
                                    if (s.ChatTopTime != null && s.ChatTopTime.Value != DateTime.MinValue)
                                        (chatVM.Chat.Chat as UserModel).IsTopMost = true;
                                    if (!string.IsNullOrEmpty(s.HeadImgMD5))
                                        (chatVM.Chat.Chat as UserModel).HeadImgMD5 = s.HeadImgMD5;
                                    if (!string.IsNullOrEmpty(s.NickName))
                                        (chatVM.Chat.Chat as UserModel).DisplayName = s.NickName;
                                    (chatVM.Chat.Chat as UserModel).IsNotDisturb = s.doNotDisturb == 1 ? true : false;
                                    (chatVM.Chat.Chat as UserModel).HeadImg = IMClient.Helper.ImageHelper.GetFriendFace((chatVM.Chat.Chat as UserModel).HeadImgMD5, (a) =>
                                    {
                                        (chatVM.Chat.Chat as UserModel).HeadImg = a;
                                    });
                                }
                                var friend = AppData.MainMV.FriendListVM.Items.ToList().FirstOrDefault(info => info.ID == userId);
                                if (friend == null)
                                {

                                    if (item.LastMessage.SessionType == (int)SDKClient.SDKProperty.SessionType.SenderLeavingChat)
                                    {
                                        chatVM.CurrentChatType = ChatType.Temporary_Replied;

                                    }
                                    else
                                    {
                                        chatVM.CurrentChatType = ChatType.Temporary;
                                    }
                                }
                                else
                                {
                                    chatVM.IsTemporaryChat = false;

                                }
                            }

                            if (string.IsNullOrEmpty(chatVM.Chat.Chat.DisplayName))
                            {
                                SDKClient.SDKClient.Instance.GetGroup(chat.ID);
                            }
                            if (type == MessageType.dismissgroup)
                            {
                                chatVM.IsHideAppendButton = true;
                                chatVM.IsShowGroupNoticeBtn = false;
                                chatVM.HasNewGroupNotice = false;
                            }
                            if (type == MessageType.exitgroup)
                            {
                                if (item.LastMessage.Data == null)
                                {

                                }
                                else
                                {
                                    var str = Util.Helpers.Json.ToJson(item.LastMessage.Data.userIds);
                                    List<int> userIds = Util.Helpers.Json.ToObject<List<int>>(str);
                                    if (userIds.Contains(AppData.Current.LoginUser.User.ID))
                                    {
                                        chatVM.IsHideAppendButton = true;
                                        chatVM.IsShowGroupNoticeBtn = false;
                                        chatVM.HasNewGroupNotice = false;
                                    }
                                }
                            }

                            if (chatVM.IsGroup)
                                chatVM.IsShowGroupNoticeBtn = true;
                            else
                                chatVM.IsShowGroupNoticeBtn = false;
                            if (item.UnReadCount > 0)
                                chatVM.UnReadCount += item.UnReadCount;
                            foreach (var unRead in item.UnReadList)
                            {
                                var unMsg = chatVM.UnReadMsgs.FirstOrDefault(m => m.MsgType == MessageType.notification && !string.IsNullOrEmpty(m.RetractId) && m.RetractId == unRead.MsgId);
                                if (unMsg != null)
                                    continue;
                                var msgEntity = new MessageModel();
                                chatVM.ChatMsgFomat(unRead, ref msgEntity, true);
                                msgEntity.IsRead = unRead.OptionRecord;
                                if (!string.IsNullOrEmpty(unRead.TokenIds) && unRead.MsgType != null && unRead.MsgType == nameof(MessageType.retract))
                                {
                                    msgEntity.IsAtMeMsg = true;
                                }
                                if (unRead.MsgType != nameof(MessageType.deletegroupnotice))
                                    chatVM.UnReadMsgs.Add(msgEntity);
                            }
                            if (item.UnReadCount > 0 && (model.Chat is UserModel || (model.Chat is GroupModel && !string.IsNullOrEmpty(model.Chat.DisplayName))))
                            {
                                if (item.IsCallMe)
                                {
                                    chatVM.HasAtMsg = true;
                                }

                                if (!model.Chat.IsNotDisturb)
                                {
                                    this.OnFlashIcon?.Invoke(true, true);
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                    //}));
                }

                AppData.MainMV.ChatListVM.SetStrangerDisturb(this.StrangerMessage.Chat.Chat.IsNotDisturb);
                AddFileAssistantItem();


            });
            //}));
            App.Current.Dispatcher.Invoke(() =>
            {
                this.ResetSort();
            });
            AppData.MainMV.UpdateUnReadMsgCount();
            TotalUnReadCount = AppData.MainMV.TotalUnReadCount;
        }
        /// <summary>
        /// 添加文件小助手条目
        /// </summary>
        public void AddFileAssistantItem()
        {
            var tempChatList = this.Items.ToList();
            var fileAssistantItem = tempChatList.FirstOrDefault(m => m.IsFileAssistant);
            if (fileAssistantItem == null)
            {
                var tempChat = GetChat(AppData.Current.LoginUser.ID, false, "", true);
                tempChat.IsFileAssistant = true;
                tempChat.IsHideAppendButton = true;
                var currentUesr = SDKClient.SDKClient.Instance.GetAccount();
                (tempChat.Chat.Chat as UserModel).TopMostTime = currentUesr.TopMostTime ?? DateTime.MinValue;
                if (currentUesr.TopMostTime != null && currentUesr.TopMostTime.Value != DateTime.MinValue)
                    (tempChat.Chat.Chat as UserModel).IsTopMost = true;

            }
        }

        /// <summary>
        /// 创建粉丝留言条目
        /// </summary>
        private void CreateStrangerMessageItem()
        {
            if (!AppData.Current.LoginUser.User.IsReceiveStrangerMessage)
            {
                return;
            }


            App.Current.Dispatcher.Invoke(new Action(() =>
             {
                 if (!this.Items.ToList().Contains(StrangerMessage))
                 {
                     this.Items.Add(StrangerMessage);
                 }
             }));

        }

        private void LoadStrangerChat(int chatId, MessageModel msg, string retractId = "")
        {
            CreateStrangerMessageItem();
            if (!StrangerChatIds.Any(x => x == chatId))
            {
                StrangerChatIds.Add(chatId);
            }
            StrangerMessage.AddStrangerMessage(msg, msg.TipMessage, retractId);

        }

        /// <summary>
        /// 加载离线消息
        /// </summary>
        /// <param name="datas">消息源</param>
        public void LoadOfflineMsgs(List<dynamic> datas)
        {
            foreach (var d in datas)
            {
                SDKClient.Model.MessagePackage pg = new SDKClient.Model.MessagePackage();

                string value = string.Format("{0}", d);

                value.ToString();
            }
        }

        /// <summary>
        /// 收到新消息
        /// </summary>
        /// <param name="package"></param>
        public void ReceiveMsg(SDKClient.Model.MessagePackage package)
        {
            if (package == null || package.code != 0 || package.data == null)
            {
                if (package != null && package.code == 500 && package.data.type == "groupChat")
                {
                    var vm = AppData.MainMV.GroupListVM.Items.FirstOrDefault(g => g.ID == package.data.groupInfo.groupId);
                    if (vm != null)
                    {
                        var myself = AppData.Current.LoginUser.User.GetInGroupMember((vm.Model as GroupModel));
                        (vm.Model as GroupModel).Members.Remove(myself);
                    }
                }

                int fromId = package.from.ToInt();
                int toId = package.to.ToInt();
                if (package != null && package.code != 0)
                {
                    int chatId = toId;
                    var chatViewModel = GetChat(chatId);
                    var msgModel = chatViewModel.Chat.Messages.FirstOrDefault(x => x.MsgKey == package.id);
                    if (msgModel != null)
                    {
                        string tip = string.Empty;
                        switch (package.code)
                        {
                            case 635:
                                msgModel.MessageState = MessageStates.Warn;
                                tip = "对方还没回应你哦~再等一等";
                                chatViewModel.AddMessageTip(tip);
                                break;
                            case 643:
                                msgModel.MessageState = MessageStates.Warn;
                                tip = "对方拒绝接收粉丝留言！";
                                chatViewModel.AddMessageTip(tip);
                                break;
                            case 648:
                                var userModel = chatViewModel.Chat.Chat as UserModel;
                                if (userModel.LinkType == 0)
                                    userModel.LinkType = 2;
                                else if (userModel.LinkType == 1)
                                    userModel.LinkType = 3;

                                tip = "对方拒绝接受您的消息！";

                                var newMsgModel = new MessageModel();
                                newMsgModel.MsgType = MessageType.notification;
                                newMsgModel.TipMessage = newMsgModel.Content = tip;
                                newMsgModel.SendTime = DateTime.Now;
                                newMsgModel.MsgKey = msgModel.MsgKey;
                                newMsgModel.MessageState = MessageStates.Warn;

                                int index = chatViewModel.Chat.Messages.IndexOf(msgModel);
                                if (index >= 0)
                                {
                                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        msgModel.MessageState = MessageStates.Warn;
                                        //chatViewModel.Chat.Messages.Remove(msgModel);
                                        chatViewModel.SetMsgShowTime(newMsgModel);
                                        if (chatViewModel.Chat.Messages.Any(x => x.Content.Contains("已转为临时聊天")))
                                        {
                                            MessageModel targetModel = chatViewModel.Chat.Messages.Last(x => x.Content.Contains("已转为临时聊天"));
                                            if (targetModel != null)
                                                chatViewModel.Chat.Messages.Remove(targetModel);
                                        }
                                        chatViewModel.Chat.Messages.Insert(index + 1, newMsgModel);
                                        //chatViewModel.SetLastMsg(newMsgModel);
                                    });
                                }

                                break;
                            default:
                                msgModel.MessageState = MessageStates.Warn;
                                tip = package.error;
                                chatViewModel.AddMessageTip(tip);
                                break;
                        }

                        chatViewModel.SetScrollOffset();
                    }
                }
                return;
            }

            int from = package.from.ToInt();
            int to = package.to.ToInt();
            int chatID = from;
            string chatPhoto = IMAssets.ImageDeal.DefaultHeadImage;

            if (package.data.groupInfo != null)
            {
                chatID = package.data.groupInfo.groupId;
                chatPhoto = package.data.groupInfo.groupPhoto;
            }
            else
            {
                if (package.syncMsg == 1)
                {
                    //同步消息
                    var friendVM = AppData.MainMV.FriendListVM.Items.FirstOrDefault(x => x.ID == to);
                    if (friendVM == null)
                    {
                        if (package.data.receiverInfo != null)
                            chatPhoto = package.data.receiverInfo.photo ?? IMAssets.ImageDeal.DefaultHeadImage;
                    }
                    else
                        chatPhoto = (friendVM.Model as UserModel).HeadImgMD5;
                    chatID = to;
                }
                else
                {
                    chatPhoto = package.data.senderInfo.photo ?? IMAssets.ImageDeal.DefaultHeadImage;

                    //var tempFriendListItems = AppData.MainMV.FriendListVM.Items.ToList();
                    var friendVM = AppData.MainMV.FriendListVM.Items.ToList().FirstOrDefault(x => x.ID == from);
                    if (friendVM == null)
                    {
                        var tempItems = this.Items.ToList();
                        if (tempItems.Any(info => info.ID == chatID && info.IsGroup == false))
                        {
                            //临时聊天
                        }
                        else
                        {
                            //粉丝留言
                            if (package.data.chatType == (int)SDKProperty.SessionType.ReceiverLeavingChat)
                            {
                                CreateStrangerMessageItem();
                                ThreadPool.QueueUserWorkItem(m =>
                                {
                                    SDKClient.SDKClient.Instance.GetUser(from);
                                });
                                if (!StrangerChatIds.Any(x => x == from))
                                {
                                    StrangerChatIds.Add(from);
                                }
                                StrangerMessage.ReceiveNewMessage(package, from, true);
                                ThreadPool.QueueUserWorkItem(m =>
                                {
                                    SDKClient.SDKClient.Instance.UpdateChatRoomVisibility(from, 0, true);
                                });
                                return;
                            }
                        }
                    }

                    chatID = from;
                }
            }
            var isFileAssistant = false;
            if (package.data.chatType == (int)SDKProperty.SessionType.FileAssistant)
            {
                isFileAssistant = true;
            }
            var chatVM = GetChat(chatID, package.data.type == nameof(SDKClient.SDKProperty.chatType.groupChat), package.data.subType.ToLower(), isFileAssistant);

            if (chatVM.IsGroup)
                chatVM.IsShowGroupNoticeBtn = true;
            else
            {
                if (!isFileAssistant)
                {
                    var photo = chatPhoto.Split(',');
                    if (photo.Length == 2)
                    {
                        chatPhoto = photo[1];
                    }
                    if ((string.IsNullOrEmpty((chatVM.Chat.Chat as UserModel).HeadImg) && !string.IsNullOrEmpty(chatPhoto)) ||
                        chatPhoto != (chatVM.Chat.Chat as UserModel).HeadImg && chatPhoto != IMAssets.ImageDeal.DefaultHeadImage)
                    {

                        IMClient.Helper.ImageHelper.GetFriendFace(chatPhoto, (a) =>
                        {
                            (chatVM.Chat.Chat as UserModel).HeadImg = a;
                        });
                    }
                }
            }
            if (package.data.type == nameof(SDKClient.SDKProperty.chatType.chat))
            {
                if (package.data.chatType == (int)SDKProperty.SessionType.temporaryChat)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        DeleteStrangerItem(chatVM, true);
                    });

                    if (!chatVM.IsTemporaryChat)
                    {
                        chatVM.IsTemporaryChat = true;
                        if (chatVM.CurrentChatType != ChatType.Temporary_Replied)
                        {
                            chatVM.CurrentChatType = ChatType.Temporary;

                            if (package.syncMsg != 1)
                            {
                                string content = "已转为临时聊天";
                                if (!chatVM.IsExistNotificationByContent(content))
                                    chatVM.AddMessageTip(content);
                            }
                        }
                    }
                    var tempFriendListItems = AppData.MainMV.FriendListVM.Items.ToList();
                    var friendItem = tempFriendListItems.FirstOrDefault(m => m.Model is UserModel user1 && user1.ID == chatVM.Chat.Chat.ID && user1.LinkDelType == 2);
                    if (friendItem != null)//对方删除了我
                    {
                        SDKClient.SDKClient.Instance.DeleteFriend(friendItem.ID, 1);
                        tempFriendListItems.Remove(friendItem);
                    }
                }
                else if (package.data.chatType == (int)SDKProperty.SessionType.CommonChat)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        DeleteStrangerItem(chatVM, false);
                    });
                    chatVM.IsTemporaryChat = false;
                }
            }

            chatVM.ReceiveNewMessage(package, from);
        }
        /// <summary>
        /// 接受离线消息处理
        /// </summary>
        public async void HandleOfflineMessage(int chatID, bool isGroup, IList<MessageEntity> offlineMessages)
        {

            var tempOfflineMsgs = offlineMessages.Where(m => m.MsgType != null
               && m.MsgType.ToLower() != nameof(MessageType.deletegroupnotice)).OrderBy(m => m.MsgTime).ToList();
            //&&(m.MsgType.ToLower() != nameof(MessageType.notification)&& m.MsgType.ToLower() != nameof(MessageType.invitejoingroup)
            var isFileAssistant = false;
            if (tempOfflineMsgs != null && tempOfflineMsgs.Count > 0)
            {
                if (tempOfflineMsgs[0].SessionType == (int)SDKProperty.SessionType.FileAssistant)
                {
                    isFileAssistant = true;
                }
            }
            var chatVM = GetChat(chatID, isGroup, "", isFileAssistant);
            bool isDismissGroup = false;
            PackageInfo dismissGroup = null;
            if (isGroup)
            {
                dismissGroup = AppData.DismissGroupIDItems.FirstOrDefault((m) =>
                {
                    if (m is DismissGroupPackage dis && dis.data.groupId == chatID)
                    {
                        return true;
                    }
                    else if (m is ExitGroupPackage exit && exit.data.groupId == chatID && exit.data.userIds.Contains(AppData.Current.LoginUser.ID))
                    {
                        return true;
                    }
                    return false;
                });
                //AppData.DismissGroupIDItems.FirstOrDefault(m => m.data.groupId == chatID);
                if (dismissGroup != null)
                {
                    if (dismissGroup is DismissGroupPackage dis && dis.data.ownerId == AppData.MainMV.LoginUser.ID)
                    {
                        //AppData.DismissGroupIDItems.Remove(dismissGroup);
                        if (chatVM != null)
                        {
                            DeleteChatItem(chatID, isGroup);
                        }
                        await SDKClient.SDKClient.Instance.DeleteHistoryMsg(chatID, SDKClient.SDKProperty.chatType.groupChat);
                        return;
                    }
                    else
                    {
                        isDismissGroup = true ;
                    }
                }
            }
            MessageModel msgModel = new MessageModel();
            if (tempOfflineMsgs.Count > 0)
            {
                if (!isDismissGroup)
                {
                    var lastOfflineMessage = tempOfflineMsgs[tempOfflineMsgs.Count - 1];

                    if (isGroup && string.IsNullOrEmpty(chatVM.Chat.Chat.DisplayName))
                    {
                        int fromId = 0;
                        int.TryParse(lastOfflineMessage.From, out fromId);
                        SDKClient.SDKClient.Instance.GetGroupMember(AppData.Current.LoginUser.ID, chatID, fromId);
                    }
                    if (lastOfflineMessage.SessionType == (int)SDKClient.SDKProperty.SessionType.temporaryChat)
                    {
                        bool isCommonChat = true;
                        if (AppData.MainMV.FriendListVM.Items.Count > 0)//判断消息发送者是否存在好友列表，如果存在不显示“临时”
                        {
                            var tempFriendListItems = AppData.MainMV.FriendListVM.Items.ToList();

                            var friendItem = tempFriendListItems.FirstOrDefault(m => m.Model is UserModel user && user.ID == chatVM.Chat.Chat.ID && user.LinkDelType == 0);
                            if (friendItem != null)//判定双方互为好友
                            {
                                isCommonChat = false;
                            }
                            else
                            {
                                StrangerEntity s = await SDKClient.SDKClient.Instance.GetStranger(chatVM.Chat.Chat.ID);
                                if (s != null)
                                {
                                    (chatVM.Chat.Chat as UserModel).TopMostTime = s.ChatTopTime ?? DateTime.MinValue;
                                    if (s.ChatTopTime != null && s.ChatTopTime.Value != DateTime.MinValue)
                                        (chatVM.Chat.Chat as UserModel).IsTopMost = true;
                                    (chatVM.Chat.Chat as UserModel).HeadImgMD5 = s.HeadImgMD5;
                                    (chatVM.Chat.Chat as UserModel).DisplayName = s.NickName;
                                    (chatVM.Chat.Chat as UserModel).IsNotDisturb = s.doNotDisturb == 1 ? true : false;
                                    (chatVM.Chat.Chat as UserModel).HeadImg = IMClient.Helper.ImageHelper.GetFriendFace(s.HeadImgMD5, (a) =>
                                    {
                                        (chatVM.Chat.Chat as UserModel).HeadImg = a;
                                    });
                                }
                                else
                                {
                                    var package = Util.Helpers.Json.ToObject<SDKClient.Model.MessagePackage>(lastOfflineMessage.Source);
                                    if (package.data != null && package.data.senderInfo != null)
                                    {
                                        (chatVM.Chat.Chat as UserModel).DisplayName = package.data.senderInfo.userName;
                                        (chatVM.Chat.Chat as UserModel).HeadImg = IMClient.Helper.ImageHelper.GetFriendFace(package.data.senderInfo.photo, (a) =>
                                        {
                                            (chatVM.Chat.Chat as UserModel).HeadImg = a;
                                        });

                                    }
                                }
                                friendItem = tempFriendListItems.FirstOrDefault(m => m.Model is UserModel user1 && user1.ID == chatVM.Chat.Chat.ID && user1.LinkDelType == 2);
                                if (friendItem != null)//对方删除了我
                                {
                                    SDKClient.SDKClient.Instance.DeleteFriend(friendItem.ID, 1);
                                    tempFriendListItems.Remove(friendItem);
                                }
                            }
                        }
                        DeleteStrangerItem(chatVM, isCommonChat);
                        chatVM.IsTemporaryChat = isCommonChat;

                    }
                    else if (lastOfflineMessage.SessionType == (int)SDKClient.SDKProperty.SessionType.ReceiverLeavingChat)//粉丝留言
                    {
                        var tempFriends = AppData.MainMV.FriendListVM.Items.ToList();
                        if (!tempFriends.Any(x => x.ID == chatID))
                        {
                            this.DeleteChatItem(chatID, isGroup);
                            UserModel user = AppData.Current.GetUserModel(lastOfflineMessage.RoomId);
                            SDKClient.SDKClient.Instance.GetUser(chatID);
                            foreach (var strangerMsg in offlineMessages)
                            {
                                var strangerMsgModel = new MessageModel
                                {
                                    SendTime = strangerMsg.MsgTime,
                                    Content = strangerMsg.Content,
                                    Sender = user,
                                    IsMine = false,
                                    MsgKey = strangerMsg.MsgId,
                                    MsgType = (MessageType)Enum.Parse(typeof(MessageType), strangerMsg.MsgType.ToLower()),
                                    TipMessage = strangerMsg.Content,
                                };
                                if (strangerMsg.SessionType == (int)SDKClient.SDKProperty.SessionType.ReceiverLeavingChat)
                                {
                                    string retractId = string.Empty;
                                    if (strangerMsgModel.MsgType == MessageType.retract)
                                    {
                                        retractId = strangerMsg.Data.retractId;
                                        StangerMessagsRetractKeys.Add(retractId);
                                    }
                                    LoadStrangerChat(chatID, strangerMsgModel, retractId);
                                }
                            }
                            return;
                        }
                        else
                        {
                            DeleteStrangerItem(chatVM, false);
                        }
                    }
                    else
                    {
                        if (!isGroup)
                        {
                            DeleteStrangerItem(chatVM, false);
                        }
                    }
                    chatVM.ChatMsgFomat(lastOfflineMessage, ref msgModel, false, true);

                    tempOfflineMsgs = tempOfflineMsgs.Where(m => m.OptionRecord == 0).ToList();
                    chatVM.UnReadCount += tempOfflineMsgs.Where(x => x.MsgType.ToLower() != nameof(MessageType.deletegroupnotice)).Count() + chatVM.UnReadCount;
                    chatVM.SetLastMsg(msgModel);
                    chatVM.Chat.LastMsg.TipMessage = msgModel.TipMessage;
                }
                else
                {
                    chatVM.AddMessageTip("该群已经被解散！", dismissGroup.time, false, dismissGroup.id);
                    chatVM.IsHideAppendButton = true;
                    chatVM.IsShowGroupNoticeBtn = false;
                    chatVM.HasNewGroupNotice = false;
                    //AppData.DismissGroupIDItems.Remove(dismissGroup);
                }
                if (chatVM.IsFileAssistant)
                {
                    if (string.IsNullOrEmpty(chatVM.Chat.LastMsg.HistorySendTime))
                    {
                        var sendTime = chatVM.Chat.LastMsg.SendTime.ToString();
                        chatVM.Chat.LastMsg.HistorySendTime = sendTime;
                        chatVM.Chat.LastMsg.SendTime = DateTime.Now;
                    }
                    else
                    {
                        DateTime dt;
                        DateTime.TryParse(string.Format("{0}", chatVM.Chat.LastMsg.HistorySendTime), out dt);
                        if (dt != chatVM.Chat.LastMsg.SendTime)
                        {
                            var sendTime = chatVM.Chat.LastMsg.SendTime.ToString();
                            chatVM.Chat.LastMsg.HistorySendTime = sendTime;
                            chatVM.Chat.LastMsg.SendTime = DateTime.Now;
                        }

                    }
                }
                AppData.MainMV.UpdateUnReadMsgCount();
                TotalUnReadCount = AppData.MainMV.TotalUnReadCount;
                if (TotalUnReadCount > 0)
                    AppData.MainMV.ChatListVM.FlashIcon(chatVM);
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.ResetSort(true);
                }));
            }
            foreach (MessageEntity chatMsg in offlineMessages)
            {
                MessageModel tempMsgModel = new MessageModel();
                var tempMessages = (chatVM.Model as ChatModel).Messages.ToList();

                if (chatMsg.MsgType == nameof(MessageType.retract))
                {
                    string retractId = chatMsg.Data?.retractId;
                    var tempMsg = tempMessages.FirstOrDefault(m => m.MsgKey == retractId);
                    //if (tempMsg != null)
                    //{
                    //    App.Current.Dispatcher.Invoke(() =>
                    //    {
                    //        (chatVM.Model as ChatModel).Messages.Remove(tempMsg);
                    //    });
                    //}
                    if (chatVM.UnReadMsgs.Count > 0)
                    {
                        var tempUnReadMessages = chatVM.UnReadMsgs.ToList();
                        var tempUnReadMsg = tempUnReadMessages.FirstOrDefault(m => m.MsgKey == retractId);
                        if (tempUnReadMsg != null)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                chatVM.UnReadMsgs.Remove(tempMsg);
                            });
                        }
                    }


                }

                if (tempMessages != null && tempMessages.Any(m => !string.IsNullOrEmpty(m.MsgKey) && m.MsgKey == chatMsg.MsgId))//重复消息
                {
                    continue;
                }
                if (chatMsg.MsgType == nameof(MessageType.deletegroupnotice))
                {
                    chatVM.ChatMsgFomat(chatMsg, ref tempMsgModel, true, true);
                    continue;
                }

                if (chatMsg.MsgType == nameof(MessageType.retract))
                {
                    if (msgModel.MsgKey == chatMsg.MsgId)
                        continue;
                    else
                    {
                        chatVM.ChatMsgFomat(chatMsg, ref tempMsgModel, true, true);
                        continue;
                    }
                    //else
                    //    tempMsgModel = msgModel;
                }
                else
                {
                    chatVM.ChatMsgFomat(chatMsg, ref tempMsgModel, true, true);
                    //if (chatMsg.MsgType == nameof(MessageType.retract) && string.IsNullOrEmpty(strMsg))
                    //    continue;
                }
                //if (chatMsg.MsgType == nameof(MessageType.invitejoingroup))
                //    continue;
                tempMsgModel.IsRead = chatMsg.OptionRecord;
                chatVM.AddMessage(tempMsgModel, tempMsgModel.TipMessage, chatMsg, true);
            }
            if (chatVM.IsGroup)
                chatVM.IsShowGroupNoticeBtn = true;
            else
                chatVM.IsShowGroupNoticeBtn = false;

            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.ResetSort();
            }));
            //this.OnDisplayMsgHint?.Invoke();

        }
        /// <summary>
        /// 粉丝留言是否撤销
        /// </summary>
        /// <param name="msgKey"></param>
        /// <returns></returns>
        public bool ExitStrangerMsgRetract(string msgKey)
        {
            if (StangerMessagsRetractKeys.Exists(m => m == msgKey))
                return true;
            return false;
        }
        public void FlashIcon(ChatViewModel chatVM, bool isSync = false)
        {
            if (isSync)
            {
                return;
            }

            AppData.MainMV.UpdateUnReadMsgCount();
            TotalUnReadCount = AppData.MainMV.TotalUnReadCount;
            AppData.MainMV.ChangeTrayWindowSize();

            if (!chatVM.Chat.Chat.IsNotDisturb && chatVM.UnReadCount > 0)
            {
                this.OnFlashIcon?.Invoke(true, false);
            }
        }

        public ChatViewModel GetChat(int chatID, bool isGroup = false, string subType = "", bool isFileAssistant = false)
        {
            var tempItems = this.Items.ToList();
            var chatVM = tempItems.FirstOrDefault(info => info.ID == chatID && info.IsGroup == isGroup);
            if (ChatVMDic.ContainsKey(chatID, isGroup) && chatVM != null)
                return chatVM;
            if (chatVM == null)
            {
                IChat chat;

                if (isGroup)
                {
                    chat = AppData.Current.GetGroupModel(chatID);
                }
                else
                {
                    if (!isFileAssistant)
                        chat = AppData.Current.GetUserModel(chatID);
                    else
                    {
                        UserModel user = new UserModel()
                        {
                            ID = AppData.Current.LoginUser.ID,
                            DisplayName = "文件小助手",
                            HeadImg = IMAssets.ImageDeal.FileAssistantIcon,
                            TopMostTime = DateTime.MinValue
                        };
                        chat = user;
                    }

                }

                ChatModel chatModel = null;
                if (isFileAssistant)
                {
                    chatModel = new ChatModel(chat);
                }
                else
                {
                    chatModel = AppData.Current.GetChatViewModel(chat);
                }
                chatModel.LastMsg = new MessageModel()
                {
                    SendTime = DateTime.Now,
                };
                chatVM = new ChatViewModel(chatModel);
                if (isFileAssistant)
                {
                    var userModel = chatVM.Chat.Chat as UserModel;
                    userModel.HeadImg = chat.HeadImg;
                    chatVM.IsFileAssistant = true;
                    chatVM.IsHideAppendButton = true;
                }
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tempItems = this.Items.ToList();
                    if (!ChatVMDic.ContainsKey(chatID, isGroup))
                    {
                        ChatVMDic.Add(chat.ID, isGroup, chatVM);
                        var tempChatVM = tempItems.FirstOrDefault(info => info.ID == chatID && info.IsGroup == isGroup);
                        if (tempChatVM == null)
                        {
                            try
                            {
                                if (subType == "deletegroupnotice" && !this.Items.Contains(chatVM)) { }
                                else
                                    this.Items.Add(chatVM);
                            }
                            catch
                            { }
                        }
                    }
                    if (!this.Items.Contains(chatVM))
                    {
                        var chatViewmodel = tempItems.FirstOrDefault(info => info.ID == chatID && info.IsGroup == isGroup);
                        if (chatViewmodel == null)
                        {
                            if (subType == "deletegroupnotice")
                            { }
                            else
                                this.Items.Add(chatVM);
                        }
                    }
                }));
                ThreadPool.QueueUserWorkItem(o =>
                {
                    int roomType = isGroup ? 1 : 0;
                    SDKClient.SDKClient.Instance.UpdateChatRoomVisibility(chatID, roomType, true);
                });
            }

            return chatVM;
        }

        public void DeleteStrangerItem(ChatViewModel chatVM, bool isTemporaryChat)
        {
            try
            {
                var tempStrangerMessageList = AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList.Count > 0 ? AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList.ToList() : null;
                var tempChatList = AppData.MainMV.ChatListVM.Items.Count > 0 ? AppData.MainMV.ChatListVM.Items.ToList() : null;
                if (tempStrangerMessageList != null && tempStrangerMessageList.Any(x => x.ID == chatVM.ID))
                {
                    //开启新的聊天条目时，如果粉丝留言条目里面有来自该用户的消息，则从粉丝留言列表里面删除掉
                    if (isTemporaryChat)
                    {
                        chatVM.IsTemporaryChat = true;
                        chatVM.CurrentChatType = ChatType.Temporary;
                    }
                    else
                    {
                        chatVM.IsTemporaryChat = false;
                        chatVM.CurrentChatType = ChatType.Normal;
                    }
                    if (AppData.MainMV.ChatListVM.StrangerChatIds.Any(x => x == chatVM.ID))
                    {
                        AppData.MainMV.ChatListVM.StrangerChatIds.Remove(chatVM.ID);
                    }
                    int index = AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList.IndexOf(chatVM);

                    var strangerVM = AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList.FirstOrDefault(x => x.ID == chatVM.ID);
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList.Remove(strangerVM);
                    });

                    AppData.MainMV.ChatListVM.StrangerMessage.UnReadCount -= strangerVM.UnReadCount;
                    if (AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList.Count == 0)
                    {
                        AppData.MainMV.ChatListVM.DeleteChatItem(AppData.MainMV.ChatListVM.StrangerMessage.ID);
                    }

                    if (AppData.MainMV.ChatListVM.StrangerMessage.Chat.LastMsg != null &&
                    AppData.MainMV.ChatListVM.StrangerMessage.Chat.LastMsg.Sender != null &&
                    AppData.MainMV.ChatListVM.StrangerMessage.Chat.LastMsg.Sender.ID == chatVM.Chat.ID)
                    {
                        if (index > 0)
                        {
                            ChatViewModel previousStrangerVM = AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList[index - 1];
                            AppData.MainMV.ChatListVM.StrangerMessage.Chat.LastMsg = previousStrangerVM.Chat.LastMsg;
                        }
                        else
                        {
                            if (AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList.Count > 0)
                            {
                                ChatViewModel previousStrangerVM = AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList[0];
                                AppData.MainMV.ChatListVM.StrangerMessage.Chat.LastMsg = previousStrangerVM.Chat.LastMsg;
                            }
                            else
                            {
                                AppData.MainMV.ChatListVM.StrangerMessage.Chat.LastMsg = null;
                            }
                        }
                    }
                }
                else if (tempChatList != null && tempChatList.Any(x => x.ID == chatVM.ID))
                {
                    chatVM.IsTemporaryChat = isTemporaryChat;
                }
            }
            catch (Exception)
            {

            }
        }
        int unStrangerReadCount = 0;
        /// <summary>
        /// 重置排序
        /// </summary>
        public void ResetSort(bool isOffline = false)
        {
            //App.Current.Dispatcher.Invoke(new Action(() =>
            //{
            var tempItems = this.Items.ToList();
            if (!isOffline)
            {
                foreach (var item in tempItems)
                {
                    if (item.UnReadMsgs != null && item.UnReadMsgs.Count > 0)
                    {
                        var tempUnReadMsgs = item.UnReadMsgs.ToList().OrderByDescending(m => m.SendTime).ToList();
                        var tempUnReadMsg = tempUnReadMsgs[0];
                        if (item.Chat?.LastMsg != null)
                        {
                            if (item != null && item.Chat.LastMsg.MsgKey != tempUnReadMsg.MsgKey && item.Chat.LastMsg.SendTime <= tempUnReadMsg.SendTime)
                            {
                                if (!string.IsNullOrEmpty(item.Chat.LastMsg.RetractId))
                                {
                                    item.Chat.LastMsg.TipMessage = tempUnReadMsg.TipMessage;
                                    //item.Chat.LastMsg.SendTime = tempUnReadMsg.SendTime;
                                }
                            }
                        }

                    }
                    if (this.SelectedItem != item)
                    {
                        if (item.UnReadMsgs != null && item.UnReadMsgs.Count > 0)
                        {
                            int count = item.UnReadMsgs.ToList().Where(m => m.IsRead == 0 && m.Sender != null && m.Sender.ID != AppData.Current.LoginUser.ID).Count();
                            item.UnReadCount = count;
                        }
                    }
                    if (item.IsGroup)
                    {
                        continue;
                    }
                    else
                    {
                        var tempStrangerMessageList = StrangerMessage.StrangerMessageList.ToList();
                        var tempTemporaryChat = tempStrangerMessageList.FirstOrDefault(m => m.ID == item.ID);
                        if (tempTemporaryChat != null && item.IsTemporaryChat)
                        {
                            var tempModel = tempTemporaryChat.Model as ChatModel;
                            var tempTemporaryModel = item.Model as ChatModel;
                            if (tempModel?.LastMsg != null && tempTemporaryModel?.LastMsg != null)
                            {
                                if (tempModel.LastMsg.SendTime < tempTemporaryModel.LastMsg.SendTime)
                                {
                                    StrangerMessage.StrangerMessageList.Remove(tempTemporaryChat);
                                    var strangerModel = StrangerMessage.Model as ChatModel;
                                    if (strangerModel != null)
                                    {
                                        var tempStrangerModel = StrangerMessage.StrangerMessageList[0];
                                        if (tempStrangerModel != null)
                                        {
                                            strangerModel.LastMsg = (tempStrangerModel.Model as ChatModel).LastMsg;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    var friend = AppData.MainMV.FriendListVM.Items.ToList().FirstOrDefault(info => info.ID == item.ID);
                    if (friend != null)
                    {
                        DeleteStrangerItem(item, item.IsTemporaryChat);
                    }
                }
                if (AppData.MainMV.FriendListVM.Items.Count > 2)
                {
                    var tempFriendListItems = AppData.MainMV.FriendListVM.Items.ToList();
                    var tempBlackListItems = AppData.MainMV.BlacklistVM.Items.ToList();
                    for (int i = 0; i < tempItems.Count; i++)
                    {
                        if (tempItems[i].ID < 0) continue;
                        if (tempItems[i].IsFileAssistant) continue;
                        if (tempItems[i].IsGroup)
                        {
                            continue;
                        }

                        var tempFriend = tempFriendListItems.FirstOrDefault(m => m.ID == tempItems[i].ID);
                        var tempBlackFriend = tempBlackListItems.FirstOrDefault(m => m.ID == tempItems[i].ID);
                        if (tempFriend == null)
                        {
                            if (tempBlackFriend != null && tempBlackFriend.Model is UserModel userModel)
                            {
                                if (userModel.LinkDelType == 0 || userModel.LinkDelType == 2)
                                    continue;
                            }
                            if (!tempItems[i].IsTemporaryChat)
                            {
                                if (tempItems[i].ID > 0)
                                    AppData.MainMV.ChatListVM.DeleteChatItem(tempItems[i].ID);
                            }
                            continue;
                        }
                    }
                }

                if (StrangerMessage != null && StrangerMessage.StrangerMessageList != null && StrangerMessage.StrangerMessageList.Count > 0)
                {
                    var tempStrangerMessageList = StrangerMessage.StrangerMessageList.ToList();

                    foreach (var stranger in tempStrangerMessageList)
                    {
                        var model = stranger.Model as ChatModel;
                        if (model != null)
                        {
                            var tempMessages = model.Messages.ToList();
                            var count = tempMessages.Count(m => !m.IsSync && m.IsRead == 0 && m.MsgType != MessageType.notification && m.MsgType != MessageType.retract);
                            stranger.UnReadCount = count;
                            unStrangerReadCount += count;
                        }
                    }
                    StrangerMessage.UnReadCount = unStrangerReadCount;
                    unStrangerReadCount = 0;
                }
            }

            var my = tempItems.FirstOrDefault(info => info.ID == AppData.Current.LoginUser.ID && !info.IsFileAssistant);
            if (my != null)
            {
                this.Items.Remove(my);
            }

            //}));
            if (this.Items.Count <= 1)
                return;
            ICollectionView cv = CollectionViewSource.GetDefaultView(this.Items);
            if (cv == null)
            {
                return;
            }

            cv.SortDescriptions.Clear();
            cv.SortDescriptions.Add(new SortDescription("Model.Chat.TopMostTime", ListSortDirection.Descending));
            cv.SortDescriptions.Add(new SortDescription("Model.LastMsg.SendTime", ListSortDirection.Descending));
        }

        public void ScrollIntoView()
        {
            OnScrollIntoView?.Invoke();
        }

        /// <summary>
        /// 删除条目
        /// </summary>
        /// <param name="chatID"></param>
        /// <param name="isGroup"></param>
        public void DeleteChatItem(int chatID, bool isGroup = false)
        {
            var tempItems = this.Items.ToList();
            var chatVM = tempItems.FirstOrDefault(info => info.ID == chatID && info.IsGroup == isGroup);
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (chatVM != null)
                    {
                        var tuple2 = Tuple.Create(chatID, isGroup);
                        ChatVMDic.Remove(tuple2);
                        Items.Remove(chatVM);
                    }
                    //更新未读消息数量
                    AppData.MainMV.UpdateUnReadMsgCount();
                    this.TotalUnReadCount = AppData.MainMV.TotalUnReadCount;
                }
                catch
                {

                }
            }));
        }

        public async void SetStrangerDisturb(bool isNotdisturb)
        {
            int value = isNotdisturb ? 1 : 0;
            for (int i = 0; i < AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList.Count; i++)
            {
                AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList[i].Chat.Chat.IsNotDisturb = isNotdisturb;
                int strangerId = AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList[i].Chat.Chat.ID;
                await SDKClient.SDKClient.Instance.SetStrangerDoNotDisturb(AppData.Current.LoginUser.User.ID, strangerId, value);
            }
        }
    }
}
