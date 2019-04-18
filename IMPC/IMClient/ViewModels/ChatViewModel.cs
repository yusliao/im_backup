using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using IMModels;
using IMClient.Views.ChildWindows;
using SDKClient.Model;
using System.IO;
using IMClient.Helper;
using Util;
using System.Collections.Concurrent;
using IMClient.Views.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using NLog;
using System.Threading;
using SDKClient.DTO;
using IMClient.Views.Panels;
using System.Windows.Media;
using System.Windows;
using SDKClient;

namespace IMClient.ViewModels
{
    /// <summary>
    /// 聊天项VM
    /// </summary>
    public class ChatViewModel : ViewModel
    {

        /// <summary>
        /// 聊天框最多显示消息数 63
        /// </summary>
        private readonly int MAXCOUNT = 100;
        /// <summary>
        /// 追加信息
        /// </summary>
        public event Action<MessageModel> AppendMessage;
        public event Action OnDisplayMsgHint;
        public event Action<string> ReSendFileEvent;
        /// <summary>
        /// 是否显示右上角"有人@你"按钮
        /// </summary>
        public event Action OnDisplayAtButton;
        public event Action OnResetStrangerMessageListSort;
        public event Action OnDelegateToStrangerMessageView;
        const long MAXFILELENGTH = 1024 * 1024 * 501;

        public event Action OnPopUpNoticeWindow;


        /// <summary>
        /// 来自服务器的未读消息ID集合
        /// </summary>
        public List<string> UnReadServerMsgID = new List<string>();

        public static Logger logger = LogManager.GetCurrentClassLogger();

        private ObservableCollection<ChatViewModel> _strangerMessageList;
        public ObservableCollection<ChatViewModel> StrangerMessageList
        {
            get
            {
                if (_strangerMessageList == null)
                {
                    _strangerMessageList = new ObservableCollection<ChatViewModel>();
                }
                return _strangerMessageList;
            }
            set { _strangerMessageList = value; this.OnPropertyChanged(); }
        }

        public ConcurrentDictionary<string, MessageModel> AtMeDic = new ConcurrentDictionary<string, MessageModel>();

        public Queue<MessageModel> SendingMessages = new Queue<MessageModel>();
        public List<MessageModel> UnReadMsgs = new List<MessageModel>();
        public List<MessageEntity> FirstMessages = new List<MessageEntity>();

        /// <summary>
        /// 处理在线公告增加、删除
        /// </summary>
        public Dictionary<int, MessageModel> noticeMessage = new Dictionary<int, MessageModel>();

        /// <summary>
        /// 处理离线公告增加、删除
        /// </summary>
        public Dictionary<int, MessageModel> offlineNoticeMessage = new Dictionary<int, MessageModel>();

        /// <summary>
        /// 处理在线和离线消息合计
        /// </summary>
        public Dictionary<int, MessageModel> OnlineAndOfflineMessage = new Dictionary<int, MessageModel>();

        public MessageModel currentUnreadGroupNoticeMessage = null;
        /// <summary>
        /// 聊天项VM
        /// </summary>
        /// <param name="view"></param>
        public ChatViewModel(ChatModel model) : base(model)
        {
            Chat = model;
            this.IsGroup = model.IsGroup;
            this.AtUserModel = AppData.Current.LoginUser.User;
            //this.IsShowGroupNoticeBtn = model.IsShowGroupNoticeBtn;
        }

        /// <summary>
        /// 聊天项VM
        /// </summary>
        /// <param name="view"></param>
        public ChatViewModel(ChatModel model, MessageModel last) : this(model)
        {
            (this.Model as ChatModel).LastMsg = last;
        }

        public override string ToString()
        {
            return string.Format("[{2}]{0}---{1}", this.Model.ID, this.Chat.Chat.DisplayName, this.IsGroup ? "Group" : "User");
        }

        /// <summary>
        /// 一般是替换为别的类型信息
        /// </summary>
        /// <param name="msg"></param>
        public void UpdateMsg(MessageModel msg)
        {

            int index = Chat.Messages.IndexOf(msg);
            if (index < 0)
            {
                var target = Chat.Messages.FirstOrDefault(info => info.MsgKey == msg.MsgKey);
                if (target == null)
                {
                    return;
                }
                target.MsgType = msg.MsgType;
                target.MessageState = msg.MessageState;
                target.Content = msg.Content;
                msg = target;
                index = Chat.Messages.IndexOf(msg);
            }
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                Chat.Messages.Remove(msg);
                Chat.Messages.Insert(index, msg);
                msg.TipMessage = msg.Content;
                this.SetLastMsg(msg);

            }));
            //this.HideMessage(msg);
        }

        #region public Property

        private bool _isViewLoaded;
        /// <summary>
        /// 视图是否已经加载
        /// </summary>
        public bool IsViewLoaded
        {
            get { return _isViewLoaded; }
            set
            {
                _isViewLoaded = value;
                if (value)
                {

                    this.Acitve();
                    AppData.MainMV.ChatListVM.IsCloseTrayWindow();
                }
            }
        }

        private bool _isTemporaryChat;
        /// <summary>
        /// 是否是临时聊天
        /// </summary>
        public bool IsTemporaryChat
        {
            get { return _isTemporaryChat; }
            set { _isTemporaryChat = value; this.OnPropertyChanged(); }
        }

        /// <summary>
        /// 是否已经激活加载过东西
        /// </summary>
        public bool HasActived { get; set; }

        public ChatModel Chat { get; }

        private string _warningInfo;
        /// <summary>
        /// 没有更多消息
        /// </summary>
        public string WarningInfo
        {
            get { return _warningInfo; }
            set
            {
                _warningInfo = null;
                this.OnPropertyChanged();
                _warningInfo = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isDisplayMsgHint;
        /// <summary>
        /// 是否显示底部的最后一条消息提示
        /// </summary>
        public bool IsDisplayMsgHint
        {
            get { return _isDisplayMsgHint; }
            set { _isDisplayMsgHint = value; this.OnPropertyChanged(); }
        }

        public bool IsGroup { get; }

        public ChatType CurrentChatType { get; set; }

        private ViewModel _targetVM;
        /// <summary>
        /// 实际聊天对象VM，用户VM或组VM
        /// </summary>
        public ViewModel TargetVM
        {
            get { return _targetVM; }
            set { _targetVM = value; this.OnPropertyChanged(); }
        }

        public int TempUnReadCount { get; set; }

        private int _unReadCount;
        /// <summary>
        /// 未读消息数量
        /// </summary>
        public int UnReadCount
        {
            get { return _unReadCount; }
            set
            {
                bool isChanged = value != _unReadCount;
                _unReadCount = value;
                if (isChanged)
                {
                    AppData.MainMV.UpdateUnReadMsgCount();
                    AppData.MainMV.ChatListVM.TotalUnReadCount = AppData.MainMV.TotalUnReadCount;
                }
                this.OnPropertyChanged();
            }
        }
        private string _unReadMsgTip;
        /// <summary>
        /// 聊天框右上角未读消息提示（大于10条时才显示）
        /// </summary>
        public string UnReadMsgTip
        {
            get { return _unReadMsgTip; }
            set { _unReadMsgTip = value; this.OnPropertyChanged(); }
        }
        private bool _isDisplayHistoryMsgButton;
        /// <summary>
        /// 是否显示聊天框右上角未读消息按钮
        /// </summary>
        public bool IsDisplayHistoryMsgButton
        {
            get { return _isDisplayHistoryMsgButton; }
            set { _isDisplayHistoryMsgButton = value; this.OnPropertyChanged(); }
        }

        private bool _isNullMsg;
        /// <summary>
        /// 发送的空信息
        /// </summary>
        public bool IsNullMsg
        {
            get { return _isNullMsg; }
            set { _isNullMsg = value; this.OnPropertyChanged(); }
        }

        private bool _hasAtMsg;
        /// <summary>
        /// 是否有被@的消息
        /// </summary>
        public bool HasAtMsg
        {
            get { return _hasAtMsg; }
            set { _hasAtMsg = value; this.OnPropertyChanged(); }
        }

        private bool _isAllRead;
        /// <summary>
        /// 未读消息是否全部已读
        /// </summary>
        public bool IsAllRead
        {
            get { return _isAllRead; }
            set { _isAllRead = value; }
        }

        private bool _hasNewGroupNotice;
        /// <summary>
        /// 是否有群公告消息
        /// </summary>
        public bool HasNewGroupNotice
        {
            get { return _hasNewGroupNotice; }
            set { _hasNewGroupNotice = value; this.OnPropertyChanged(); }
        }

        private bool _isIgnoreAllMessage = false;
        /// <summary>
        /// 是否忽略全部消息
        /// </summary>
        public bool IsIgnoreAllMessage
        {
            get { return _isIgnoreAllMessage; }
            set { _isIgnoreAllMessage = value; this.OnPropertyChanged(); }
        }

        private bool _isDisplayAtButton;
        /// <summary>
        /// 是否显示聊天框右上角"有人@你"按钮
        /// </summary>
        public bool IsDisplayAtButton
        {
            get { return _isDisplayAtButton; }
            set { _isDisplayAtButton = value; this.OnPropertyChanged(); }
        }

        private UserModel _atUserModel;
        /// <summary>
        /// 被@人对象
        /// </summary>
        public UserModel AtUserModel
        {
            get { return _atUserModel; }
            set { _atUserModel = value; this.OnPropertyChanged(); }
        }

        private bool _isFullPage;
        /// <summary>
        /// 消息是否满页
        /// </summary>
        public bool IsFullPage
        {
            get { return _isFullPage; }
            set { _isFullPage = value; this.OnPropertyChanged(); }
        }

        private bool _isOpenHisMsg;
        /// <summary>
        /// 是否打开消息记录
        /// </summary>
        public bool IsOpenHisMsg
        {
            get { return _isOpenHisMsg; }
            set { _isOpenHisMsg = value; this.OnPropertyChanged(); }
        }

        private bool _isHideAppendButton;
        /// <summary>
        /// 是否隐藏聊天窗右上角的查看群聊/好友详情按钮
        /// </summary>
        public bool IsHideAppendButton
        {
            get { return _isHideAppendButton; }
            set { _isHideAppendButton = value; this.OnPropertyChanged(); }
        }

        private bool _isShowGroupNoticeBtn;
        /// <summary>
        /// 是否显示群公告按钮
        /// </summary>
        public bool IsShowGroupNoticeBtn
        {
            get { return _isShowGroupNoticeBtn; }
            set { _isShowGroupNoticeBtn = value; this.OnPropertyChanged(); }
        }

        private string _searchKeyWord;
        public string SearchKeyWord
        {
            get { return _searchKeyWord; }
            set { _searchKeyWord = value; this.OnPropertyChanged(); }
        }
        private bool _isFileAssistant;
        /// <summary>
        /// 是否是文件助手
        /// </summary>
        public bool IsFileAssistant
        {
            get { return _isFileAssistant; }
            set
            {
                _isFileAssistant = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region Command

        private VMCommand _jumpCommand;
        /// <summary>
        /// 跳转命令(从群名称点击跳转到群信息页面）
        /// </summary> 
        public VMCommand JumpCommand
        {
            get
            {
                if (_jumpCommand == null)
                    _jumpCommand = new VMCommand(MainJupmToNewConent);
                return _jumpCommand;
            }
        }

        private VMCommand _replyCommand;
        /// <summary>
        /// 回复粉丝留言
        /// </summary>
        public VMCommand ReplyCommand
        {
            get
            {
                if (_replyCommand == null)
                    _replyCommand = new VMCommand(ReplyStranger);
                return _replyCommand;
            }
        }

        private VMCommand _deleteStrangerMessageCommand;
        /// <summary>
        /// 删除粉丝留言
        /// </summary>
        public VMCommand DeleteStrangerMessageCommand
        {
            get
            {
                if (_deleteStrangerMessageCommand == null)
                    _deleteStrangerMessageCommand = new VMCommand(DeleteStrangerMessage);
                return _deleteStrangerMessageCommand;
            }
        }

        private VMCommand _ShowBusinessCard;
        /// <summary>
        /// 个人名片命令
        /// </summary> 
        public VMCommand ShowBusinessCard
        {
            get
            {
                if (_ShowBusinessCard == null)
                    _ShowBusinessCard = new VMCommand(ShowUserInfoCard, new Func<object, bool>(o => o != null));
                return _ShowBusinessCard;
            }
        }

        private VMCommand _sendCommand;
        /// <summary>
        /// 发送消息命令
        /// </summary>
        public VMCommand SendCommand
        {
            get
            {
                if (_sendCommand == null)
                    _sendCommand = new VMCommand(SendMsg, new Func<object, bool>(o => o != null));
                return _sendCommand;
            }
        }

        private VMCommand _hideMessage;
        /// <summary>
        /// 删除（暂时处理为隐藏）
        /// </summary>
        public VMCommand HideMessageCommand
        {
            get
            {
                if (_hideMessage == null)
                    _hideMessage = new VMCommand(HideMessage, new Func<object, bool>(o => o != null));
                return _hideMessage;
            }
        }

        private VMCommand _noDisturbCommand;
        /// <summary>
        /// 免打扰设置命令
        /// </summary>
        public VMCommand NoDisturbCommand
        {
            get
            {
                if (_noDisturbCommand == null)
                    _noDisturbCommand = new VMCommand(NoDisturb);
                return _noDisturbCommand;
            }
        }

        private VMCommand _topMostCommand;
        /// <summary>
        /// 聊天置顶命令
        /// </summary>
        public VMCommand TopMostCommand
        {
            get
            {
                if (_topMostCommand == null)
                    _topMostCommand = new VMCommand(TopMost);
                return _topMostCommand;
            }
        }

        private VMCommand _defriendCommand;
        /// <summary>
        /// 移至黑名单命令
        /// </summary>
        public VMCommand DefriendCommand
        {
            get
            {
                if (_defriendCommand == null)
                    _defriendCommand = new VMCommand(Defriend);
                return _defriendCommand;
            }
        }

        private VMCommand _deleteChatCommand;
        /// <summary>
        /// 删除聊天命令
        /// </summary>
        public VMCommand DeleteChatCommand
        {
            get
            {
                if (_deleteChatCommand == null)
                    _deleteChatCommand = new VMCommand(DeleteChat);
                return _deleteChatCommand;
            }
        }

        private VMCommand _modifyGroupNameCommand;
        /// <summary>
        /// 修改群名命令
        /// </summary>
        public VMCommand ModifyGroupNameCommand
        {
            get
            {
                if (_modifyGroupNameCommand == null)
                    _modifyGroupNameCommand = new VMCommand(ModifyGroupName);
                return _modifyGroupNameCommand;
            }
        }

        private VMCommand _clearCommand;
        /// <summary>
        /// 清屏
        /// </summary> 
        public VMCommand ClearCommand
        {
            get
            {
                if (_clearCommand == null)
                    _clearCommand = new VMCommand(Clear);
                return _clearCommand;
            }
        }

        private VMCommand _startTemporaryChatCommand;
        /// <summary>
        /// 有事找TA
        /// </summary>
        public VMCommand StartTemporaryChatCommand
        {
            get
            {
                if (_startTemporaryChatCommand == null)
                    _startTemporaryChatCommand = new VMCommand(StartTemporaryChat);
                return _startTemporaryChatCommand;
            }
        }

        #endregion

        #region CommandMethods
        /// <summary>
        /// 打开个人名片
        /// </summary>
        /// <param name="para"></param>
        public void ShowUserInfoCard(object para)
        {

            AppData.MainMV.ShowUserBusinessCard(para, this.IsGroup, this.IsGroup ? ApplyFriendSource.Group : ApplyFriendSource.Other, this.IsGroup ? this.ID.ToString() : "", this.IsGroup && this.Chat.Chat is GroupModel groupModel ? groupModel.DisplayName : string.Empty);
        }
        private void StartTemporaryChat(object para)
        {
            var list = this.Chat.Messages.Where(x => x.MsgType == MessageType.notification && x.TipMessage == "您不在对方通讯录内，有事找TA");
            if (list != null)
            {
                int count = list.ToList().Count();
                for (int i = 0; i < count; i++)
                {
                    this.Chat.Messages.Remove(list.ToList()[i]);
                    count--;
                    i--;
                }
            }

            if (!this.IsTemporaryChat)
            {
                this.AddMessageTip("已转为临时聊天");
                this.IsTemporaryChat = true;
                this.CurrentChatType = ChatType.Temporary;

                //删除好友
                if (this.Chat.Chat is UserModel userModel)
                {
                    userModel.IsApplyFriend = false;
                    userModel.IsAttention = false;
                    userModel.DisplayName = userModel.Name;

                    SDKClient.SDKClient.Instance.DeleteFriend(userModel.ID);
                }
            }
            else
            {
                this.IsTemporaryChat = true;
                this.CurrentChatType = ChatType.Temporary;
            }
        }

        private void ReplyStranger(object para)
        {
            this.DeleteStrangerMessageItem();

            //string tip = "已转为临时聊天";
            //MessageModel msg = new MessageModel()
            //{
            //    MsgKey = null,
            //    MsgType = MessageType.notification,
            //    Content = tip,
            //    TipMessage = tip,
            //    SendTime = DateTime.Now,
            //};

            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    if (this.Chat.Messages.Count > 0)
                    {
                        var tempMsg = this.Chat.Messages[this.Chat.Messages.Count - 1];
                        if (tempMsg != null)
                        {
                            this.Chat.LastMsg = tempMsg;
                        }
                    }
                    AppData.MainMV.ChatListVM.Items.Add(this);
                }
                catch
                {

                }
            }));
            this.IsTemporaryChat = true;



            foreach (var item in this.Chat.Messages)
            {
                SetOneMsgRead(item.MsgKey);
            }

            //App.Current.Dispatcher.Invoke(() =>
            //{
            //    SetMsgShowTime(msg);
            //    (this.Chat as ChatModel).Messages.Add(msg);
            //    this.AppendMessage?.Invoke(null);
            //});
            this.HasActived = true;
            AppData.MainMV.ChatListVM.SelectedItem = this;
            AppData.MainMV.ChatListVM.IsChecked = true;
            App.Current.Dispatcher.Invoke(() =>
            {
                this.Chat.Chat.TopMostTime = DateTime.MinValue;
                AppData.MainMV.ChatListVM.ResetSort();
            });


        }

        private async void DeleteStrangerMessage(object para)
        {
            this.DeleteStrangerMessageItem();
            await SDKClient.SDKClient.Instance.DeleteHistoryMsg(this.ID, SDKClient.SDKProperty.chatType.chat);
        }

        private void DeleteStrangerMessageItem()
        {
            int index = AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList.IndexOf(this);

            AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList.Remove(this);
            AppData.MainMV.ChatListVM.StrangerMessage.UnReadCount -= this.UnReadCount;
            if (AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList.Count == 0)
            {
                AppData.MainMV.ChatListVM.DeleteChatItem(AppData.MainMV.ChatListVM.StrangerMessage.ID);
            }

            if (AppData.MainMV.ChatListVM.StrangerMessage.Chat.LastMsg != null &&
                AppData.MainMV.ChatListVM.StrangerMessage.Chat.LastMsg.Sender != null &&
                AppData.MainMV.ChatListVM.StrangerMessage.Chat.LastMsg.Sender.ID == this.Chat.ID)
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

        #region Send msg

        private void HideMessage(object para)
        {
            if (this.Model is ChatModel chat && para is MessageModel msg)
            {
                SDKClient.SDKClient.Instance.UpdateHistoryMsgIsHidden(msg.MsgKey);

                var taraget = chat.Messages.ToList().FirstOrDefault(info => info.MsgKey == msg.MsgKey);

                int index = chat.Messages.IndexOf(taraget);

                if (index == 0) //第一条
                {
                    if (chat.Messages.Count > 1)
                    {
                        chat.Messages[1].ShowSendTime = true;
                    }
                    else
                    {
                        this.SetLastMsg(new MessageModel() { SendTime = DateTime.Now }, false);
                    }

                }
                else if (index == chat.Messages.Count - 1) //最后一条
                {
                    if (chat.Messages.Count > 1)
                    {
                        this.SetLastMsg(chat.Messages[chat.Messages.Count - 2], true, true);
                    }
                    else
                    {
                        this.SetLastMsg(null);
                    }
                    //if (chat.Messages.Count == 1)//只有一条时
                    //{
                    //    this.SetLastMsg(null, "", false);
                    //}
                    //else
                    //{
                    //    this.SetLastMsg(null, "", false);
                    //}
                }
                else
                {
                    var per = chat.Messages[index - 1];
                    var next = chat.Messages[index + 1];

                    if ((next.SendTime - per.SendTime).TotalMinutes >= 5)
                    {
                        next.ShowSendTime = true;
                    }
                }

                chat.Messages.Remove(taraget);

                if (msg.Sender.ID != AppData.Current.LoginUser.ID)
                {
                    try
                    {
                        if (msg.MsgType == MessageType.img && msg.Content.Contains(SDKClient.SDKProperty.imgPath))
                        {
                            if (File.Exists(msg.Content))
                            {
                                File.Delete(msg.Content);
                            }
                        }
                        else if (msg.MsgType == MessageType.file || msg.MsgType == MessageType.onlinefile)
                        {
                            if (msg.ResourceModel != null &&
                                msg.ResourceModel.FullName.Contains(SDKClient.SDKProperty.filePath) &&
                                File.Exists(msg.ResourceModel.FullName))
                            {
                                File.Delete(msg.ResourceModel.FullName);
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }

        StringBuilder _strB = new StringBuilder();

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="para"></param>
        private void SendMsg(object para)
        {
            IsFoward = false;
            //if (AppData.CanInternetAction())
            //{
            if (!this.CanSendMsg())
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    AppData.MainMV.ChatListVM.ResetSort();
                    AppData.MainMV.ChatListVM.ScrollIntoView();
                });
                return;
            }

            List<int> atUserIds = new List<int>();

            FlowDocument doc = para as FlowDocument;
            if (doc != null)
            {
                _strB.Clear();
                bool isNullOrEmpty = true;
                int blockCount = 1;
                var noText = false;
                foreach (var root in doc.Blocks)
                {
                    if (!(root is Paragraph))
                    {
                        break;
                    }

                    foreach (var item in ((Paragraph)root).Inlines)
                    {
                        if (item is InlineUIContainer cn && cn.Child != null)
                        {
                            isNullOrEmpty = false;
                            string value = string.Empty;
                            if (cn.Child.Uid.StartsWith(AppData.FlagEmoje))
                            {
                                value = cn.Child.Uid.Replace(AppData.FlagEmoje, string.Empty);
                                //_strB.Append(GetEmojiName(value));
                                _strB.Append(value);
                            }
                            else if (cn.Child.Uid.StartsWith(AppData.FlagAt))
                            {
                                atUserIds.Add(cn.Child.Uid.Split('|')[2].ToInt());
                                _strB.Append(cn.Child.Uid.Split('|')[1] + " ");
                            }
                            else if (cn.Child.Uid.StartsWith(AppData.FlagImage))
                            {
                                if (!string.IsNullOrEmpty(_strB.ToString().Trim()))
                                {
                                    SendTextMsgToServer(_strB.ToString(), atUserIds);
                                    _strB.Clear();
                                    atUserIds.ToList().Clear();
                                }

                                value = cn.Child.Uid.Replace(AppData.FlagImage, string.Empty);
                                SendImageMsgToServer(value);
                                noText = true;
                            }
                            else if (cn.Child.Uid.StartsWith(AppData.FlagSmallVideo))
                            {
                                if (!string.IsNullOrEmpty(_strB.ToString().Trim()))
                                {
                                    SendTextMsgToServer(_strB.ToString(), atUserIds);
                                    _strB.Clear();
                                    atUserIds.ToList().Clear();
                                }

                                value = cn.Child.Uid.Replace(AppData.FlagSmallVideo, string.Empty);
                                if (value.Split('|').Length == 3)
                                {
                                    SendSmallVideoMsgToServer(value.Split('|')[0], value.Split('|')[1], value.Split('|')[2].ToInt());
                                    noText = true;
                                }
                            }
                            else if (cn.Child.Uid.StartsWith(AppData.FlagFile))
                            {
                                if (!string.IsNullOrEmpty(_strB.ToString().Trim()))
                                {
                                    SendTextMsgToServer(_strB.ToString(), atUserIds);
                                    _strB.Clear();
                                    atUserIds.ToList().Clear();
                                }

                                value = cn.Child.Uid.Replace(AppData.FlagFile, string.Empty);
                                if (value.Split('|').Length > 1)
                                {
                                    SendFileMsgToServerAsync(value.Split('|')[0], value.Split('|')[1]);

                                }
                                else
                                {
                                    SendFileMsgToServerAsync(value);
                                }
                                noText = true;
                            }
                        }
                        else if (item is Run run)
                        {
                            //文本
                            _strB.Append(run.Text);
                        }
                        else
                        {
                            item.ToString();
                        }
                    }

                    if (doc.Blocks.Count > 1 && blockCount >= 1 && blockCount < doc.Blocks.Count)
                    {
                        if (_strB.Length > 0)
                        {
                            _strB.Append("\r\n");
                        }
                    }

                    blockCount++;
                }

                string result = _strB.ToString();
                if (!string.IsNullOrEmpty(result))
                {
                    //if (result.Length > 2000)
                    //{
                    //    this.AddMessageTip("单条消息不能超过2000！");
                    //    this.AppendMessage.Invoke(null);
                    //    return;
                    //}
                    var strMsg = result.Trim();
                    if (!noText && (string.IsNullOrEmpty(result.Replace("\r\n", "")) || string.IsNullOrEmpty(strMsg)))
                    {
                        this.IsNullMsg = false;
                        this.IsNullMsg = true;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(strMsg))
                        {
                            SendTextMsgToServer(result, atUserIds);
                            this.AppendMessage.Invoke(null);
                        }
                    }
                }
                else if (isNullOrEmpty)
                {
                    this.IsNullMsg = false;
                    this.IsNullMsg = true;
                }
                else
                {
                    this.AppendMessage.Invoke(null);
                }
                App.Current.Dispatcher.Invoke(() =>
                {
                    AppData.MainMV.ChatListVM.ResetSort();
                    AppData.MainMV.ChatListVM.ScrollIntoView();
                });
            }
            doc.Blocks.Clear();

            atUserIds.ToList().Clear();
            //}
            _strB.Clear();
        }

        private bool CanSendMsg(bool isAddTip = false)
        {
            bool canSend = true;
            string tip = string.Empty;
            string actionableContent = null;
            if (Chat.Chat is GroupModel group && !AppData.MainMV.GroupListVM.Items.ToList().Any(g => g.ID == this.ID))
            {
                tip = "该群组已经被解散或您已被管理员请出群组！";
                canSend = false;
            }
            else if (Chat.Chat is UserModel user)
            {
                if (user.LinkType >= 2)
                {
                    if (!this.IsTemporaryChat)
                    {
                        tip = "对方拒绝接受您的消息！";
                        //canSend = false;
                    }
                }
                else if (user.LinkDelType == 2)
                {
                    if (!this.IsTemporaryChat)
                    {
                        tip = "您不在对方通讯录内，";
                        actionableContent = "有事找TA";
                        //canSend = false;
                    }
                }
            }
            if (!canSend || isAddTip)
            {
                if (!string.IsNullOrEmpty(tip))
                {
                    AddMessageTip(tip, actionableContent: actionableContent);
                    this.AppendMessage?.Invoke(null);
                }
            }
            return canSend;
        }

        /// <summary>
        /// 获取要发送的Emoji名
        /// </summary>
        /// <param name="str">相对路径的值</param>
        /// <returns></returns>
        private string GetEmojiName(string str)
        {
            foreach (var item in IM.Emoje.EmojeBox.EmojeKeys)
            {
                if (item.Name.ToString().Equals(str))
                {
                    return item.Name;
                }
            }
            return string.Empty;
        }
        private bool IsFoward = false;
        /// <summary>
        /// 转发、重新发送
        /// </summary>
        /// <param name="msg"></param>
        public void ReSend(MessageModel msg, bool isFoward = false)
        {
            IsFoward = isFoward;
            bool isReSend = false;
            if (CanSendMsg() && (msg.Sender == AppData.Current.LoginUser.User || isFoward))
            {
                if (!isFoward)
                {
                    if (!string.IsNullOrEmpty(msg.MsgKey) && !msg.MsgKey.Contains("-"))
                    {
                        msg.MessageState = MessageStates.Loading;
                        if (msg.MsgType == MessageType.file &&msg.ResourceModel!=null &&msg.ResourceModel.FileState!=FileStates.Completed)
                        {
                            ReSend(msg.MsgKey);
                            return;
                        }
                        SDKClient.SDKClient.Instance.RetrySendMessageByMsgId(msg.MsgKey);
                        return;
                    }

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (!(this.Model as ChatModel).Messages.Remove(msg))
                        {
                            isReSend = true;
                            return;
                        }
                    });
                }
                if (isReSend) return;
                var user = Chat.Chat as UserModel;
                string msgId = string.Empty;
                switch (msg.MsgType)
                {
                    default:
                        break;
                    case MessageType.file:
                        if (isFoward)
                        {
                            string destId = this.Model.ID.ToString();
                            var groupName = this.IsGroup && this.Model is GroupModel groupModel ? groupModel.DisplayName : string.Empty;
                            SDKClient.SDKProperty.chatType chatType = this.IsGroup ?
            SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;
                            var imgMD5 = string.Empty;
                            if (!string.IsNullOrEmpty(msg.ResourceModel.SmallKey))
                                imgMD5 = msg.ResourceModel.SmallKey;
                            else if (!string.IsNullOrEmpty(msg.ResourceModel.PreviewKey))
                                imgMD5 = msg.ResourceModel.PreviewKey;

                            if (!IsFileAssistant && user != null && !SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden && (user.LinkDelType > 1 || user.LinkType > 1))
                            {
                                SendFileMsgToServerAsync(msg.ContentMD5, "", msgId, msg);
                                return;
                            }
                            msgId = SDKClient.SDKClient.Instance.SendFileMessage(msg.Content, destId, msg.ResourceModel.Key, msg.ResourceModel.Length, chatType, groupName, 0, 0, imgMD5);
                            SendFileMsgToServerAsync(msg.ContentMD5, "", msgId, msg);
                        }
                        else
                        {
                            SendFileMsgToServerAsync(msg.Content);
                        }
                        break;
                    case MessageType.txt:
                    case MessageType.addfriendaccepted:
                    case MessageType.bigtxt:
                        if (!isFoward)
                            this.SendTextMsgToServer(msg.Content, msg.Target as List<int>);
                        else
                            this.SendTextMsgToServer(msg.Content);
                        break;
                    case MessageType.usercard:
                        this.SendPersonCardToserver(msg, true);
                        break;
                    case MessageType.invitejoingroup:

                        break;
                    case MessageType.foreigndyn:
                        SendForeignDynMsgToServer(msg);
                        break;
                    case MessageType.img:
                        if (isFoward)
                        {
                            string destId = this.Model.ID.ToString();
                            var groupName = this.IsGroup && this.Chat.Chat is GroupModel groupModel ? groupModel.DisplayName : string.Empty;
                            SDKClient.SDKProperty.chatType chatType = this.IsGroup ?
            SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;
                            if (!IsFileAssistant && user != null && !SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden && (user.LinkDelType > 1 || user.LinkType > 1))
                            {
                                this.SendImageMsgToServer(msg.Content, msg, true, msgId);
                                return;
                            }

                            msgId = SDKClient.SDKClient.Instance.SendImgMessage(msg.Content, destId, msg.ResourceModel.Key, msg.ResourceModel.SmallKey, chatType, null, groupName);
                            this.SendImageMsgToServer(msg.Content, msg, true, msgId);
                            return;
                        }
                        this.SendImageMsgToServer(msg.Content, msg, isFoward);

                        break;
                    case MessageType.smallvideo:
                    case MessageType.video:
                        if (isFoward)
                        {
                            string destId = this.Model.ID.ToString();
                            int Width = 0;
                            int Height = 0;
                            using (var source = new System.Drawing.Bitmap(msg.ResourceModel.PreviewImagePath))
                            {
                                Width = source.Width;
                                Height = source.Height;
                            }
                            SDKClient.SDKProperty.chatType chatType = this.IsGroup ?
           SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;
                            var groupName = this.IsGroup && this.Model is GroupModel groupModel ? groupModel.DisplayName : string.Empty;

                            if (!IsFileAssistant && user != null && !SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden && (user.LinkDelType > 1 || user.LinkType > 1))
                            {
                                //msg.MsgKey = msgId;
                                this.SendSmallVideoMsgToServer(msg.Content, msg.ResourceModel.PreviewImagePath, msg.ResourceModel.RecordTime, isFoward ? msg : null);
                                return;
                            }
                            else
                            {
                                msgId = SDKClient.SDKClient.Instance.SendSmallVideoMessage(msg.Content, destId, msg.ResourceModel.RecordTime.ToString(),
                                  msg.ResourceModel.Key, msg.ResourceModel.PreviewKey, Width, Height, msg.ResourceModel.Length, chatType, groupName);
                            }

                        }
                        this.SendSmallVideoMsgToServer(msg.Content, msg.ResourceModel.PreviewImagePath, msg.ResourceModel.RecordTime, isFoward ? msg : null, msgId);
                        break;
                }
            }
        }

        /// <summary>
        /// 重发文件
        /// </summary>
        public void ReSend(string msgID)
        {
            ReSendFileEvent?.Invoke(msgID);
        }
        private void SendTextMsgToServer(string content, List<int> userIds = null)
        {
            if (content == "\r\n")
            {
                return;
            }

            string destId = this.Model.ID.ToString();

            SDKClient.SDKProperty.chatType msgType = this.IsGroup ?
                SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;

            string groupName = this.IsGroup ? this.Chat.Chat.DisplayName : null;

            MessageModel msg = new MessageModel()
            {
                Sender = AppData.Current.LoginUser.User,
                SendTime = DateTime.Now,
                IsMine = true,
                MsgType = MessageType.txt,
                Content = content,
                TipMessage = content,
                MessageState = MessageStates.Loading,
                Target = userIds?.ToList(),
            };

            if (IsFileAssistant)
            {

                if (AppData.CanInternetAction(""))
                {
                    Task.Run(() =>
                    {
                        msg.MsgKey = SDKClient.SDKClient.Instance.Sendtxt(content, destId, null, msgType, groupName, SDKClient.SDKProperty.SessionType.FileAssistant, null, (msgId) =>
                        {
                            msg.MsgKey = msgId;
                        });
                    });
                }
                else
                {
                    SaveSendingMessages(msg);
                }

                this.AddMessage(msg);
                return;
            }
            var user = Chat.Chat as UserModel;

            if (SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden)
            {
                msg.MessageState = MessageStates.Warn;
            }
            if (user != null && !this.IsTemporaryChat && user.LinkDelType == 3)
            {
                this.IsTemporaryChat = true;
            }
            if (user != null && !SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden && ((!this.IsTemporaryChat && user.LinkDelType > 1) || user.LinkType > 1))
            {
                msg.MessageState = MessageStates.Warn;
            }
            this.AddMessage(msg);
            if (this.IsTemporaryChat)
            {
                SDKClient.SDKProperty.SessionType sessionType = SDKClient.SDKProperty.SessionType.SenderLeavingChat;
                message.ReceiverInfo recverInfo = new message.ReceiverInfo()
                {
                    userName = user.Name,
                    photo = user.HeadImgMD5,
                };
                Task.Run(() =>
                {
                    if (AppData.CanInternetAction(""))
                    {
                        msg.MsgKey = SDKClient.SDKClient.Instance.Sendtxt(content, destId, this.IsGroup ? userIds : null, msgType, groupName, sessionType, recverInfo, (msgID) =>
                        {
                            msg.MsgKey = msgID;
                        });
                    }
                    else
                    {
                        SaveSendingMessages(msg);
                        return;
                    }
                    if (user.LinkDelType >= 1)
                    {
                        if (AppData.MainMV.AttentionListVM != null && AppData.MainMV.AttentionListVM.Items.Count > 0)
                        {
                            var attention = AppData.MainMV.AttentionListVM.Items.ToList().FirstOrDefault(m => m.Model.ID == user.ID);
                            if (attention != null)
                                return;
                        }
                        if (AppData.MainMV.BlacklistVM != null && AppData.MainMV.BlacklistVM.Items.Count > 0)
                        {
                            var blackUser = AppData.MainMV.BlacklistVM.Items.ToList().FirstOrDefault(m => m.Model.ID == user.ID);
                            if (blackUser != null && (blackUser.Model as UserModel).AttentionID != 0)
                            {
                                return;
                            }
                        }
                        if (user.LinkType < 2)
                        {
                            SDKClient.SDKClient.Instance.AddAttention(AppData.Current.LoginUser.User.ID, user.ID);
                        }
                    }

                });

                string msgContent = "已转为临时聊天";
                if (AppData.MainMV.ChatListVM.StrangerChatIds.Any(x => x == this.ID))
                {
                    AppData.MainMV.ChatListVM.StrangerChatIds.Remove(this.ID);

                    if (!IsExistNotificationByContent(msgContent) && user.LinkType != 2)
                        //AddMessageTip(msgContent);
                        this.AppendMessage?.Invoke(null);
                }
                //else
                //{
                //    if ((user.LinkDelType == 1 || user.LinkDelType == 3) && user.LinkType != 2)
                //    {
                //        if (!IsExistNotificationByContent(msgContent))
                //            AddMessageTip(msgContent);
                //    }
                //}
            }
            else
            {
                if (user != null && !SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden && (user.LinkDelType > 1 || user.LinkType > 1))
                {
                    CanSendMsg(true);
                    return;
                }
                Task.Run(() =>
               {
                   if (AppData.CanInternetAction(""))
                   {

                       if (IsFileAssistant)
                       {
                           msg.MsgKey = SDKClient.SDKClient.Instance.Sendtxt(content, destId, null, msgType, groupName, SDKClient.SDKProperty.SessionType.FileAssistant, null, (msgID) =>
                           {
                               msg.MsgKey = msgID;
                           });
                       }
                       else
                       {
                           msg.MsgKey = SDKClient.SDKClient.Instance.Sendtxt(content, destId, this.IsGroup ? userIds : null, msgType, groupName, SDKProperty.SessionType.CommonChat, null, (msgID) =>
                           {
                               msg.MsgKey = msgID;
                           });
                       }
                   }
                   else
                   {
                       SaveSendingMessages(msg);
                   }
               });
            }
        }

        /// <summary>
        /// 转发动态消息
        /// </summary>
        /// <param name="messageModel"></param>
        private void SendForeignDynMsgToServer(MessageModel messageModel)
        {

            string destId = this.Model.ID.ToString();

            SDKClient.SDKProperty.chatType msgType = this.IsGroup ?
                SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;

            string groupName = this.IsGroup ? this.Chat.Chat.DisplayName : null;

            var msg = new MessageModel
            {
                Sender = AppData.Current.LoginUser.User,
                SendTime = DateTime.Now,
                IsMine = true,
                MsgType = MessageType.foreigndyn,
                Content = messageModel.Content,
                TipMessage = "[链接]" + messageModel.Content,
                MsgHyperlink = messageModel.MsgHyperlink,
                MessageState = MessageStates.Loading,
                ShareMsgImage = messageModel.ShareMsgImage,
                MsgSource = messageModel.MsgSource,
            };

            if (IsFileAssistant)
            {

                if (AppData.CanInternetAction(""))
                {
                    Task.Run(() =>
                    {
                       msg.MsgKey = SDKClient.SDKClient.Instance.SendForeignDynMsg(messageModel.MsgSource, destId, msgType, groupName, SDKClient.SDKProperty.SessionType.FileAssistant, null, (msgID) =>
                       {
                           msg.MsgKey = msgID;
                       });
                    });
                }
                else
                {
                    SaveSendingMessages(msg);
                }

                this.AddMessage(msg);
                return;
            }
            var user = Chat.Chat as UserModel;

            if (SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden)
            {
                msg.MessageState = MessageStates.Warn;
            }
            if (user != null && !this.IsTemporaryChat && user.LinkDelType == 3)
            {
                this.IsTemporaryChat = true;
            }
            if (user != null && !SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden && ((!this.IsTemporaryChat && user.LinkDelType > 1) || user.LinkType > 1))
            {
                msg.MessageState = MessageStates.Warn;
            }
            this.AddMessage(msg);
            if (this.IsTemporaryChat)
            {
                SDKClient.SDKProperty.SessionType sessionType = SDKClient.SDKProperty.SessionType.SenderLeavingChat;
                message.ReceiverInfo recverInfo = new message.ReceiverInfo()
                {
                    userName = user.Name,
                    photo = user.HeadImgMD5,
                };
                Task.Run(async () =>
                {

                    if (AppData.CanInternetAction(""))
                    {
                        msg.MsgKey = SDKClient.SDKClient.Instance.SendForeignDynMsg(messageModel.MsgSource, destId, msgType, groupName, sessionType, recverInfo, (msgID) =>
                        {
                            msg.MsgKey = msgID;
                        });
                    }
                    else
                    {
                        SaveSendingMessages(msg);
                        return;
                    }
                    if (user.LinkDelType >= 1)
                    {
                        if (AppData.MainMV.AttentionListVM != null && AppData.MainMV.AttentionListVM.Items.Count > 0)
                        {
                            var attention = AppData.MainMV.AttentionListVM.Items.FirstOrDefault(m => m.Model.ID == user.ID);
                            if (attention != null)
                                return;
                        }
                        SDKClient.SDKClient.Instance.AddAttention(AppData.Current.LoginUser.User.ID, user.ID);
                    }

                });

                string msgContent = "已转为临时聊天";
                if (AppData.MainMV.ChatListVM.StrangerChatIds.Any(x => x == this.ID))
                {
                    AppData.MainMV.ChatListVM.StrangerChatIds.Remove(this.ID);

                    if (!IsExistNotificationByContent(msgContent) && user.LinkType != 2)
                        //AddMessageTip(msgContent);
                        this.AppendMessage?.Invoke(null);
                }
                //else
                //{
                //    if ((user.LinkDelType == 1 || user.LinkDelType == 3) && user.LinkType != 2)
                //    {
                //        if (!IsExistNotificationByContent(msgContent))
                //            AddMessageTip(msgContent);
                //    }
                //}
            }
            else
            {
                if (user != null && !SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden && (user.LinkDelType > 1 || user.LinkType > 1))
                {
                    CanSendMsg(true);
                    return;
                }
                Task.Run(()=>
                {
                    if (AppData.CanInternetAction(""))
                    {
                        if (IsFileAssistant)
                        {
                            msg.MsgKey =  SDKClient.SDKClient.Instance.SendForeignDynMsg(messageModel.MsgSource, destId, msgType, groupName, SDKClient.SDKProperty.SessionType.FileAssistant, null, (msgID) =>
                            {
                                msg.MsgKey = msgID;
                            });
                        }
                        else
                        {
                            msg.MsgKey =  SDKClient.SDKClient.Instance.SendForeignDynMsg(messageModel.MsgSource, destId, msgType, groupName,SDKProperty.SessionType.CommonChat, null, (msgID) =>
                            {
                                msg.MsgKey = msgID;
                            });
                        }
                    }
                    else
                    {
                        SaveSendingMessages(msg);
                        return;
                    }
                });
            }
        }
        public void SaveSendingMessages(MessageModel msg)
        {
            if (!AppData.MessageModels.Exists(m => m.ID == this.ID))
                AppData.MessageModels.Add(this);
            var sendingMsg = SendingMessages.FirstOrDefault(m => m.MsgKey == msg.MsgKey);
            if (sendingMsg == null)
                SendingMessages.Enqueue(msg);
            Task.Delay(300000).ContinueWith((t) =>
            {
                //300000
                //TODO:检测消息状态
                //消息未发送成功，更改为发送失败；同时从全局未发送消息队列中剔除该消息
                //消息发送成功，返回
                if (msg.MessageState == MessageStates.Loading || (msg.MsgType == MessageType.file && msg.MessageState == MessageStates.None))
                {
                    msg.MessageState = MessageStates.Fail;
                    if (AppData.MessageModels.Count > 0)
                        SendingMessages.Dequeue();
                }

            });
        }
        /// <summary>
        /// 重发等待发送的消息
        /// </summary>
        public void ReSendWaitMsg()
        {
            if (SendingMessages.Count > 0)
            {
                foreach (var msg in SendingMessages)
                {
                    ReSend(msg);
                }
                SendingMessages.Clear();
            }

        }
        public void SendInviteJoinGroupToserver(MessageModel messageModel)
        {
            string destId = this.Model.ID.ToString();
            SDKClient.SDKProperty.chatType chatType = this.IsGroup ?
                SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;
            string groupName = this.IsGroup ? this.Chat.Chat.DisplayName : null;
            var msg = messageModel;
            //if (isFoward)
            //{
            var photoKey = msg.ContentMD5;
            msg = new MessageModel()
            {
                Sender = AppData.Current.LoginUser.User,
                SendTime = DateTime.Now,
                IsMine = true,
                MsgType = MessageType.usercard,
                Content = "[个人名片]",
                TipMessage = "[个人名片]" + messageModel.PersonCardModel.Name,
                Target = new List<int>() { this.Model.ID },
            };
            PersonCardModel pcm = new PersonCardModel()
            {
                PhoneNumber = messageModel.PersonCardModel.PhoneNumber,
                PhotoImg = messageModel.PersonCardModel.PhotoImg,
                Name = messageModel.PersonCardModel.Name,
                UserId = messageModel.PersonCardModel.UserId
            };
            msg.ContentMD5 = photoKey;
            msg.PersonCardModel = pcm;
            //}
            if (SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden)
            {
                msg.MessageState = MessageStates.Warn;
            }
            if (AppData.CanInternetAction())
            {
                if (this.IsGroup)
                {
                    if (!this.CanSendMsg())
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            AppData.MainMV.ChatListVM.ResetSort();
                            AppData.MainMV.ChatListVM.ScrollIntoView();
                        });
                        return;
                    }
                }
            }

            UserModel user = this.Chat.Chat as UserModel;
            if (IsFileAssistant)
            {

                Task.Run(() =>
                {
                    msg.MsgKey = SDKClient.SDKClient.Instance.SendPersonCard(msg.PersonCardModel.Name, msg.ContentMD5, msg.PersonCardModel.PhoneNumber, msg.PersonCardModel.UserId, destId, this.IsGroup ? msg.Target as List<int> : null, chatType, groupName, SDKProperty.SessionType.CommonChat, null, (msgID) =>
                      {
                          msg.MsgKey = msgID;
                      }); ;
                });

                this.AddMessage(msg);
                return;
            }
            if (user != null && !this.IsTemporaryChat && user.LinkDelType == 3)
            {
                this.IsTemporaryChat = true;
            }
            if (user != null && !SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden && ((!this.IsTemporaryChat && user.LinkDelType > 1) || user.LinkType > 1))
            {
                msg.MessageState = MessageStates.Warn;
            }
            if (this.IsTemporaryChat)
            {
                msg.MessageState = MessageStates.Warn;
                this.AddMessage(msg);
                string tip = "不能给陌生人发此类消息";
                AddMessageTip(tip);
                this.AppendMessage?.Invoke(null);
            }
            else
            {
                this.AddMessage(msg);
                if (user != null && !SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden && (user.LinkDelType > 1 || user.LinkType > 1))
                {
                    CanSendMsg(true);
                    return;
                }
                Task.Run(() =>
                {
                    msg.MsgKey = SDKClient.SDKClient.Instance.SendPersonCard(msg.PersonCardModel.Name, msg.ContentMD5, msg.PersonCardModel.PhoneNumber, msg.PersonCardModel.UserId, destId, this.IsGroup ? msg.Target as List<int> : null, chatType, groupName, SDKProperty.SessionType.CommonChat, null, (msgID) =>
                      {
                          msg.MsgKey = msgID;
                      });
                });
            }
        }
        /// <summary>
        /// 发送个人名片
        /// </summary>
        /// <param name="messageModel"></param>
        /// <param name="isFoward"></param>
        public void SendPersonCardToserver(MessageModel messageModel, bool isFoward = false)
        {
            string destId = this.Model.ID.ToString();
            SDKClient.SDKProperty.chatType chatType = this.IsGroup ?
                SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;
            string groupName = this.IsGroup ? this.Chat.Chat.DisplayName : null;
            var msg = messageModel;
            if (isFoward)
            {
                var photoKey = msg.ContentMD5;
                msg = new MessageModel()
                {
                    Sender = AppData.Current.LoginUser.User,
                    SendTime = DateTime.Now,
                    IsMine = true,
                    MessageState = MessageStates.Loading,
                    MsgType = MessageType.usercard,
                    Content = "[个人名片]",
                    TipMessage = "[个人名片]" + messageModel.PersonCardModel.Name,
                    Target = new List<int>() { this.Model.ID },
                };
                PersonCardModel pcm = new PersonCardModel()
                {
                    PhoneNumber = messageModel.PersonCardModel.PhoneNumber,
                    PhotoImg = messageModel.PersonCardModel.PhotoImg,
                    Name = messageModel.PersonCardModel.Name,
                    UserId = messageModel.PersonCardModel.UserId
                };
                msg.ContentMD5 = photoKey;
                msg.PersonCardModel = pcm;
            }
            if (SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden)
            {
                msg.MessageState = MessageStates.Warn;
            }
            if (AppData.CanInternetAction(""))
            {
                if (this.IsGroup)
                {
                    if (!this.CanSendMsg())
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            AppData.MainMV.ChatListVM.ResetSort();
                            AppData.MainMV.ChatListVM.ScrollIntoView();
                        });
                        return;
                    }
                }
            }

            UserModel user = this.Chat.Chat as UserModel;
            if (IsFileAssistant)
            {

                if (AppData.CanInternetAction(""))
                {
                    Task.Run(() =>
                    {
                        msg.MsgKey = SDKClient.SDKClient.Instance.SendPersonCard(msg.PersonCardModel.Name, msg.ContentMD5, msg.PersonCardModel.PhoneNumber, msg.PersonCardModel.UserId, destId, this.IsGroup ? msg.Target as List<int> : null, chatType, groupName, SDKProperty.SessionType.CommonChat, null, (msgID) =>
                          {
                              msg.MsgKey = msgID;
                          });
                    });

                }
                else
                {
                    SaveSendingMessages(msg);
                }
                this.AddMessage(msg);
                return;
            }
            if (user != null && !this.IsTemporaryChat && user.LinkDelType == 3)
            {
                this.IsTemporaryChat = true;
            }
            if (user != null && !SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden && ((!this.IsTemporaryChat && user.LinkDelType > 1) || user.LinkType > 1))
            {
                msg.MessageState = MessageStates.Warn;
            }
            if (this.IsTemporaryChat)
            {
                msg.MessageState = MessageStates.Warn;
                this.AddMessage(msg);
                string tip = "不能给陌生人发此类消息";
                AddMessageTip(tip);
                this.AppendMessage?.Invoke(null);
            }
            else
            {
                this.AddMessage(msg);
                if (user != null && !SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden && (user.LinkDelType > 1 || user.LinkType > 1))
                {
                    CanSendMsg(true);
                    return;
                }
                if (AppData.CanInternetAction(""))
                {
                    Task.Run(() =>
                    {
                        msg.MsgKey = SDKClient.SDKClient.Instance.SendPersonCard(msg.PersonCardModel.Name, msg.ContentMD5, msg.PersonCardModel.PhoneNumber, msg.PersonCardModel.UserId, destId, this.IsGroup ? msg.Target as List<int> : null, chatType, groupName, SDKProperty.SessionType.CommonChat, null, (msgID) =>
                          {
                              msg.MsgKey = msgID;
                          });
                    });
                }
                else
                {
                    SaveSendingMessages(msg);
                }
            }
        }

        public void IsDefriend()
        {
            var tempItems = AppData.MainMV.BlacklistVM.Items.ToList();
            if (tempItems.Any(x => x.ID == this.ID))
            {
                AddMessageTip("您已将对方拉入黑名单");
                this.AppendMessage?.Invoke(null);
            }
        }

        public bool IsExistNotificationByContent(string content)
        {
            var tempMessages = (this.Model as ChatModel).Messages.ToList();
            if (tempMessages.FirstOrDefault(x => x.MsgType == MessageType.notification && x.Content.Equals(content)) != null)
            {
                return true;
            }
            return false;
        }

        public void SendSmallVideoMsgToServer(string videoPath, string videoPreviewImagePath, int recordTime, MessageModel messageModel = null, string newMsgkey = "")
        {
            string destId = this.Model.ID.ToString();

            SDKClient.SDKProperty.chatType msgType = this.IsGroup ?
                SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;

            int groupID = this.IsGroup ? this.Model.ID : 0;
            List<int> userIds = this.IsGroup ? new List<int>() : null;

            FileResourceModel file = new FileResourceModel(videoPath);
            file.PreviewImagePath = videoPreviewImagePath;
            file.RecordTime = recordTime;
            MessageModel msg = new MessageModel()
            {
                Sender = AppData.Current.LoginUser.User,
                SendTime = DateTime.Now,
                IsMine = true,
                MsgType = MessageType.smallvideo,
                ResourceModel = file,
                Content = videoPath,
                MessageState = MessageStates.Loading,
            };
            if (messageModel == null)
                msg.MsgKey = string.Empty;
            msg.TipMessage = "[小视频]";
            msg.IsSending = true;
            msg.ResourceModel.ID = 5201314;
            if (messageModel != null)
            {
                if (SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden)
                    msg.MessageState = MessageStates.Warn;
                else
                {
                    if (!string.IsNullOrEmpty(newMsgkey))
                        msg.MsgKey = newMsgkey;
                    msg.ResourceModel = messageModel.ResourceModel;
                    msg.Content = messageModel.Content;
                    msg.ContentMD5 = messageModel.ContentMD5;
                    msg.MessageState = MessageStates.Success;
                    msg.IsSending = false;
                }
            }
            var user = Chat.Chat as UserModel;
            if (!IsFileAssistant && user != null && !this.IsTemporaryChat && user.LinkDelType == 3)
            {
                this.IsTemporaryChat = true;
            }
            if (!IsFileAssistant && user != null && !SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden && (user.LinkDelType > 1 || user.LinkType > 1))
            {
                msg.MessageState = MessageStates.Warn;
                msg.IsSending = false;
                this.AddMessage(msg);
                if (this.IsTemporaryChat)
                {
                    string tip = "不能给陌生人发此类消息";
                    AddMessageTip(tip);
                    this.AppendMessage?.Invoke(null);
                }
                else
                {
                    CanSendMsg(true);
                }
                return;
            }
            this.AddMessage(msg);
            if (user != null && user.LinkType == 1)
            {
                AddMessageTip("您已将对方拉入黑名单");
                this.AppendMessage?.Invoke(null);
            }
        }

        [Obsolete]
        public void ReloadSmallVideo()
        {
            try
            {
                for (int i = 0; i < (this.Model as ChatModel).Messages.Count; i++)
                {
                    if ((this.Model as ChatModel).Messages[i].MsgType == MessageType.smallvideo || (this.Model as ChatModel).Messages[i].MsgType == MessageType.video)
                    {
                        if ((this.Model as ChatModel).Messages[i].MessageState == MessageStates.Fail)
                        {
                            (this.Model as ChatModel).Messages[i].MessageState = MessageStates.Loading;
                            MessageHelper.LoadVideoPreviewImage((this.Model as ChatModel).Messages[i]);
                            MessageHelper.LoadVideoContent((this.Model as ChatModel).Messages[i]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex.StackTrace);
            }
        }

        private void SendImageMsgToServer(string imagePath, MessageModel msg = null, bool isFoward = false, string newMsgKey = "")
        {
            string destId = this.Model.ID.ToString();

            SDKClient.SDKProperty.chatType msgType = this.IsGroup ?
                SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;

            int groupID = this.IsGroup ? this.Model.ID : 0;
            List<int> userIds = this.IsGroup ? new List<int>() : null;
            MessageModel messageModel = new MessageModel()
            {
                Sender = AppData.Current.LoginUser.User,

                IsMine = true,
                MsgType = MessageType.img,
                //MsgKey = string.Empty,
                Content = imagePath,
                TipMessage = "[图片]",
            };
            messageModel.SendTime = DateTime.Now;
            messageModel.ResourceModel.Key = imagePath.Split('\\').LastOrDefault();
            messageModel.OperateTask = new System.Threading.CancellationTokenSource();

            if (msg != null && isFoward)
            {
                if (!string.IsNullOrEmpty(newMsgKey))
                    messageModel.MsgKey = newMsgKey;
                messageModel.MessageState = MessageStates.Success;
                messageModel.ContentMD5 = msg.ContentMD5;
                messageModel.ResourceModel = msg.ResourceModel;
            }
            string groupName = this.IsGroup ? this.Chat.Chat.DisplayName : null;

            if (!this.IsTemporaryChat)
            {
                var user = Chat.Chat as UserModel;
                if (!IsFileAssistant && user != null && !this.IsTemporaryChat && user.LinkDelType == 3)
                {
                    this.IsTemporaryChat = true;
                }
                if (!IsFileAssistant && user != null && !SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden && (user.LinkDelType > 1 || user.LinkType > 1))
                {
                    messageModel.MessageState = MessageStates.Warn;
                    this.AddMessage(messageModel);
                    CanSendMsg(true);
                    return;
                }
                if (!isFoward)
                {
                    messageModel.MessageState = MessageStates.Loading;
                    if (AppData.CanInternetAction(""))
                    {
                        Task.Run(async () => await SDKClient.SDKClient.Instance.SendImgMessage(imagePath, (m) =>
                        {
                            //App.Current.Dispatcher.Invoke(() =>
                            //{

                            if (SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden)
                            {
                                messageModel.MessageState = MessageStates.Warn;
                            }
                            else
                            {
                                if (m.isSuccess == 1)
                                {

                                    messageModel.MessageState = MessageStates.Loading;
                                    messageModel.MsgKey = m.msgId;
                                    messageModel.ResourceModel.Key = m.imgMD5;
                                    messageModel.ResourceModel.SmallKey = m.smallId;
                                }
                                else
                                {
                                    messageModel.MessageState = MessageStates.Fail;
                                }
                            }
                            //});
                        }, destId, msgType, groupID, messageModel.OperateTask.Token, groupName: groupName));
                    }
                    else
                    {
                        SaveSendingMessages(messageModel);
                    }

                }
                this.AddMessage(messageModel);
                //if (Chat.Chat is UserModel user && user.LinkType == 1)
                //{
                //    AddMessageTip("您已将对方拉入黑名单");
                //    this.AppendMessage?.Invoke(null);
                //}

                if (AppData.MainMV.ChatListVM.StrangerChatIds.Any(x => x == this.ID))
                {
                    AppData.MainMV.ChatListVM.StrangerChatIds.Remove(this.ID);
                    //AddMessageTip("已转为临时聊天");
                    this.AppendMessage?.Invoke(null);
                }

            }
            else
            {
                messageModel.MessageState = MessageStates.Warn;
                this.AddMessage(messageModel);

                string tip = "不能给陌生人发此类消息";
                AddMessageTip(tip);
                this.AppendMessage?.Invoke(null);
            }

        }

        private async Task SendFileMsgToServerAsync(string filePath, string thumbnailPath = "", string newMsgKey = "", MessageModel msgModel = null)
        {
            string destId = this.Model.ID.ToString();

            SDKClient.SDKProperty.chatType msgType = this.IsGroup ?
                SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;

            int groupID = this.IsGroup ? this.Model.ID : 0;
            List<int> userIds = this.IsGroup ? new List<int>() : null;
            if (!string.IsNullOrEmpty(filePath))
            {
                if (File.Exists(filePath))
                {
                    FileInfo fileInfo = new System.IO.FileInfo(filePath);
                    if (fileInfo.Length > MAXFILELENGTH)
                    {
                        if (IsFileAssistant)
                        {
                            AppData.MainMV.TipMessage = $"不支持发送大于500M文件！";
                            return;
                        }
                        else
                        {
                            bool isOnLine = await SDKClient.SDKClient.Instance.GetUserPcOnlineInfo(this.ID);
                            if (!isOnLine)
                            {
                                AppData.MainMV.TipMessage = $"对方已离线，不支持发送大于500M文件！";
                                return;
                            }
                        }
                    }
                }
            }
            FileResourceModel file = new FileResourceModel(filePath);
            file.FileImg = Helper.FileHelper.GetFileImage(filePath, true);
            var msg = new MessageModel()
            {
                Sender = AppData.Current.LoginUser.User,
                SendTime = DateTime.Now,
                IsMine = true,
                MsgType = MessageType.file,
                Content = filePath,
                ResourceModel = file,
            };
            if (!string.IsNullOrEmpty(thumbnailPath))
                msg.ResourceModel.PreviewImagePath = thumbnailPath;
            if (msgModel != null)
            {
                if (!string.IsNullOrEmpty(newMsgKey))
                    msg.MsgKey = newMsgKey;
                msg.ContentMD5 = msgModel.ContentMD5;
                msg.ResourceModel = msgModel.ResourceModel;
                msg.ResourceModel.DBState = 2;
                msg.Content = msgModel.Content;
                msg.MessageState = msgModel.MessageState;
            }

            msg.TipMessage = "[文件]";
            if (msgModel == null)
                msg.IsSending = true;

            var user = Chat.Chat as UserModel;
            if (!IsFileAssistant && user != null && !this.IsTemporaryChat && user.LinkDelType == 3)
            {
                this.IsTemporaryChat = true;
            }
            if (!IsFileAssistant && user != null && !SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden && (user.LinkDelType > 1 || user.LinkType > 1))
            {
                msg.MessageState = MessageStates.Warn;
                msg.IsSending = false;
                this.AddMessage(msg);
                if (this.IsTemporaryChat)
                {
                    string tip = "不能给陌生人发此类消息";
                    AddMessageTip(tip);
                    this.AppendMessage?.Invoke(null);
                }
                else
                {
                    CanSendMsg(true);
                }
                return;
            }
            if (SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden)
            {
                msg.MessageState = MessageStates.Warn;
            }
            this.AddMessage(msg);
            //this.AddMessage(msg);

            //if (Chat.Chat is UserModel user && user.LinkType == 1)
            //{
            //    AddMessageTip("您已将对方拉入黑名单");
            //    this.AppendMessage?.Invoke(null);
            //}
        }

        /// <summary>
        /// 离线上传文件
        /// </summary>
        /// <param name="msg"></param>
        public void SendOfflineFile(MessageModel msg, System.Threading.CancellationTokenSource operate, Action<string> callback)
        {
            if (IsFoward) return;
            if (msg == null || !File.Exists(msg.Content))
            {
                return;
            }
            string destId = this.Model.ID.ToString();
            msg.ResourceModel.FileState = FileStates.SendOffline;
            SDKClient.SDKProperty.chatType chatType = this.IsGroup ?
              SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;
            SDKClient.SDKProperty.MessageType messageType = msg.MsgType == MessageType.file ? SDKClient.SDKProperty.MessageType.file : SDKClient.SDKProperty.MessageType.smallvideo;
            var tension = System.IO.Path.GetExtension(msg.Content).ToLower();
            string ImageFilter = @"图片|*.gif;*.jpeg;*.jpg;*.png;*.bmp";
            bool isImage = false;
            var imgFullName = string.Empty;
            if (ImageFilter.Contains(tension))
            {
                imgFullName = msg.Content;
            }
            else if (App.VideoFilter.Contains(tension) && !string.IsNullOrEmpty(msg.ResourceModel.PreviewImagePath))
            {
                imgFullName = msg.ResourceModel.PreviewImagePath;
            }
            var tempMsgID = string.Empty;
            if (!msg.MsgKey.Contains('-'))
            {
                tempMsgID = msg.MsgKey;
            }
            string groupName = this.IsGroup ? this.Chat.Chat.DisplayName : null;

            if (!this.IsTemporaryChat)
            {
                Task.Run(async () =>
                {
                    await
                    SDKClient.SDKClient.Instance.SendFileMessage(msg.Content,
                    (size, length) =>
                    {
                        msg.ResourceModel.Length = length;
                        msg.ResourceModel.CompleteLength = size;
                    },
                    (result) =>
                    {
                        if (operate.Token.IsCancellationRequested)
                        {
                            callback?.Invoke("TokenCancel");
                            return;
                        }

                        msg.IsSending = false;
                        msg.ContentMD5 = result.fileMD5;
                        msg.ResourceModel.Key = result.fileMD5;

                        if (result.isSuccess == 1)
                        {
                            msg.ResourceModel.CompleteLength = msg.ResourceModel.Length;
                            var msgId = result.func?.Invoke();
                            msg.ResourceModel.PreviewKey = result.imgId;
                            msg.MsgKey = msgId;
                            msg.ResourceModel.FileState = FileStates.Completed;
                            msg.MessageState = MessageStates.Success;
                            callback?.Invoke(null);
                        }
                        else
                        {
                            var msgId = result.msgId;
                            if (!string.IsNullOrEmpty(msgId))
                                msg.MsgKey = msgId;
                            if (msg.MessageState != MessageStates.Fail)
                            {
                                if (msg.MsgType == MessageType.file)
                                {
                                    //msg.MsgType = MessageType.notification;

                                    msg.ResourceModel.FileState = FileStates.Fail;
                                    msg.MessageState = MessageStates.Fail;
                                    msg.ContentMD5 = msg.Content;
                                    switch (result.errorState)
                                    {
                                        case SDKProperty.ErrorState.Cancel:
                                        case SDKProperty.ErrorState.OutOftheControl:
                                            string tip = result.errorState.Description();
                                            string size = Helper.FileHelper.FileSizeToString(msg.ResourceModel.Length);
                                            msg.Content = $"{tip}，您中断了离线文件\"{msg.ResourceModel.FileName}\"({size})的发送";
                                            msg.TipMessage = msg.Content;
                                            callback?.Invoke(msg.Content);
                                            return;
                                    }
                                    callback?.Invoke(null);
                                }
                            }
                        }

                        if (AppData.MainMV.ChatListVM.StrangerChatIds.Any(x => x == this.ID))
                        {

                            AppData.MainMV.ChatListVM.StrangerChatIds.Remove(this.ID);

                            AddMessageTip("已转为临时聊天");
                            this.AppendMessage?.Invoke(null);
                        }
                    },
                    (process) =>
                    {
                        try
                        {
                            msg.ResourceModel.CompleteLength += process;
                            Console.WriteLine(process);
                        }
                        catch (Exception ex)
                        {
                            ex.Message.ToString();
                        }
                    },
                    destId, chatType, operate.Token, messageType, groupName: groupName, imgFullName: imgFullName, msgId:tempMsgID).ConfigureAwait(false);

                    msg.CanOperate = true;
                });
            }
            else
            {
                if (msg.MessageState != MessageStates.Warn)
                {
                    msg.MessageState = MessageStates.Warn;
                    msg.ResourceModel.FileState = FileStates.Fail;

                    string tip = "不能给陌生人发此类消息";
                    AddMessageTip(tip);
                    this.AppendMessage?.Invoke(null);
                }
            }
        }

        /// <summary>
        /// 在线发送文件
        /// </summary> 
        public void SendOnlineFile(ChatViewModel chatVM, MessageModel msg, System.Threading.CancellationTokenSource operate, Action<bool> callBack)
        {
            if (msg == null || !File.Exists(msg.Content))
            {
                return;
            }

            if (!this.IsTemporaryChat)
            {
                msg.ResourceModel.FileState = FileStates.SendOnline;
                Task.Run(() =>
                {
                    msg.MsgKey = SDKClient.SDKClient.Instance.SendOnlineFile(this.Model.ID, msg.Content,
                         (size) =>
                         {
                             msg.ResourceModel.Length = size;
                         },
                          (result) =>
                          {
                              if (operate.Token.IsCancellationRequested)
                              {
                                  return;
                              }
                              msg.IsSending = false;
                              msg.ContentMD5 = result.imgMD5;
                              msg.ResourceModel.Key = result.imgMD5;

                              if (result.isSuccess == 1)
                              {
                                  msg.ResourceModel.CompleteLength = msg.ResourceModel.Length;
                                  msg.ResourceModel.FileState = FileStates.Completed;

                                  MessageModel sMsg = chatVM.AddMessageTip(result.notifyPackage.Content, msgId: result.notifyPackage.MsgId);
                                  sMsg.MessageState = MessageStates.Success;

                                  callBack?.Invoke(true);
                              }
                              else
                              {
                                  if (msg.MessageState == MessageStates.Fail)
                                  {
                                      return;
                                  }
                                  msg.MsgType = MessageType.notification;

                                  msg.ResourceModel.FileState = FileStates.Fail;
                                  msg.MessageState = MessageStates.Fail;
                                  msg.ContentMD5 = msg.Content;

                                  string size = Helper.FileHelper.FileSizeToString(msg.ResourceModel.Length);
                                  msg.Content = result.notifyPackage.Content;

                                  if (result.notifyPackage.Error.Contains("文件不存在"))
                                  {
                                      AppData.MainMV.TipMessage = "源文件被修改或删除";
                                  }
                                  callBack?.Invoke(false);
                              }

                              if (AppData.MainMV.ChatListVM.StrangerChatIds.Any(x => x == this.ID))
                              {
                                  AppData.MainMV.ChatListVM.StrangerChatIds.Remove(this.ID);
                                  AddMessageTip("已转为临时聊天");
                                  this.AppendMessage?.Invoke(null);
                              }
                          },
                           (process) =>
                           {
                               try
                               {
                                   msg.ResourceModel.CompleteLength = process;
                                   Console.WriteLine(process);
                               }
                               catch (Exception ex)
                               {
                                   ex.Message.ToString();
                               }
                           }, operate.Token);

                    msg.CanOperate = true;
                }).ConfigureAwait(false);
            }
            else
            {
                if (msg.MessageState != MessageStates.Warn)
                {
                    msg.MessageState = MessageStates.Warn;
                    msg.ResourceModel.FileState = FileStates.Fail;

                    string tip = "不能给陌生人发此类消息";
                    AddMessageTip(tip);
                    this.AppendMessage?.Invoke(null);
                }
            }
        }

        public void SendSmallVideoFile(MessageModel msg, System.Threading.CancellationTokenSource operate, Action<bool> callback)
        {
            if (IsFoward) return;
            if (msg == null || !File.Exists(msg.Content))
            {
                return;
            }

            string destId = this.Model.ID.ToString();
            SDKClient.SDKProperty.chatType chatType = this.IsGroup ? SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;
            SDKClient.SDKProperty.MessageType messageType = msg.MsgType == MessageType.file ? SDKClient.SDKProperty.MessageType.file : SDKClient.SDKProperty.MessageType.smallvideo;

            string groupName = this.IsGroup ? this.Chat.Chat.DisplayName : null;

            if (!this.IsTemporaryChat)
            {
                Task.Run(async () =>
                {
                    await SDKClient.SDKClient.Instance.SendSmallVideoMessage(msg.Content, msg.ResourceModel.RecordTime.ToString(), msg.ResourceModel.PreviewImagePath,
                    (size, length) =>
                    {
                        //msg.ResourceModel.Length = size;
                    },
                    (result) =>
                    {
                        if (operate.Token.IsCancellationRequested)
                        {
                            return;
                        }
                        msg.IsSending = false;
                        msg.ContentMD5 = result.videoMD5;
                        msg.ResourceModel.Key = result.videoMD5;
                        msg.ResourceModel.PreviewKey = result.imgId;
                        if (result.isSuccess == 1)
                        {
                            msg.MessageState = MessageStates.Success;
                            msg.ResourceModel.CompleteLength = msg.ResourceModel.Length;
                            msg.ResourceModel.FileState = FileStates.Completed;
                            msg.MsgKey = result.msgId;

                            callback?.Invoke(true);
                        }
                        else
                        {
                            if (msg.MessageState != MessageStates.Fail)
                            {
                                msg.ResourceModel.FileState = FileStates.Fail;
                                msg.MessageState = MessageStates.Fail;
                                msg.ContentMD5 = msg.Content;
                            }
                        }

                        if (AppData.MainMV.ChatListVM.StrangerChatIds.Any(x => x == this.ID))
                        {
                            AppData.MainMV.ChatListVM.StrangerChatIds.Remove(this.ID);
                            //AddMessageTip("已转为临时聊天");
                            this.AppendMessage?.Invoke(null);
                        }
                    },
                    (process) =>
                    {
                        try
                        {
                            msg.ResourceModel.CompleteLength = process;
                        }
                        catch (Exception ex)
                        {
                            ex.Message.ToString();
                        }
                    },
                   destId, chatType, operate.Token, messageType, groupName: groupName).ConfigureAwait(false);
                    msg.CanOperate = true;
                    if (SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden)
                        msg.MessageState = MessageStates.Warn;
                });
            }
            else
            {
                msg.MessageState = MessageStates.Warn;

                string tip = "不能给陌生人发此类消息";
                AddMessageTip(tip);
                this.AppendMessage?.Invoke(null);
            }
        }

        #endregion

        #region 撤回
        public void SendWithDrawMsg(MessageModel msg)
        {
            if (!AppData.CanInternetAction())
            {
                return;
            }
            App.Current.Dispatcher.Invoke(() =>
            {
                //如果撤回时，对方还在上传，结束上传Task
                if (msg != null && msg.OperateTask != null)
                {
                    msg.OperateTask.Cancel();
                    msg.OperateTask = null;
                }
                var tempMessages = (this.Model as ChatModel).Messages.ToList();
                var tempMsg = tempMessages.FirstOrDefault(m => m.MsgKey == msg.MsgKey);
                int index = tempMessages.IndexOf(tempMsg);
                if (tempMsg != null)
                    (this.Model as ChatModel).Messages.Remove(tempMsg);

                string tip = "您撤回了一条消息";

                MessageModel messageModel = new MessageModel();
                messageModel.MsgKey = msg.MsgKey;// string.Empty;
                messageModel.MsgType = MessageType.notification;
                messageModel.Content = messageModel.TipMessage = tip;
                messageModel.SendTime = msg.SendTime;// DateTime.Now;
                messageModel.RetractId = tempMsg.MsgKey;
                //SetMsgShowTime(messageModel);
                messageModel.ShowSendTime = msg.ShowSendTime;
                (this.Model as ChatModel).Messages.Insert(index, messageModel);

                this.SetLastMsg(messageModel, false);

                SDKClient.SDKProperty.chatType messageType = this.IsGroup ? SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;
                if (!string.IsNullOrEmpty(msg.MsgKey))
                {
                    if (this.IsTemporaryChat)
                    {
                        var user = Chat.Chat as UserModel;
                        message.ReceiverInfo recverInfo = new message.ReceiverInfo()
                        {
                            userName = user.Name,
                            photo = user.HeadImgMD5,
                        };
                        SDKClient.SDKProperty.SessionType sessionType = (this.CurrentChatType == ChatType.Temporary_Replied) ? SDKClient.SDKProperty.SessionType.temporaryChat : SDKClient.SDKProperty.SessionType.SenderLeavingChat;

                        SDKClient.SDKClient.Instance.SendRetractMessage(msg.MsgKey, this.ID.ToString(), messageType, this.IsGroup ? this.ID : 0, sessionType: sessionType, recverInfo: recverInfo);
                    }
                    else
                    {
                        SDKClient.SDKClient.Instance.SendRetractMessage(msg.MsgKey, this.ID.ToString(), messageType, this.IsGroup ? this.ID : 0);
                    }
                }
            });
        }

        public void ReceiveWithDrawMsg(MessageModel msg, string displayName, bool isSync, bool isUnRead, SDKClient.SDKProperty.RetractType rType)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                //如果撤回时，对方还在下载，结束下载Task
                if (msg != null && msg.OperateTask != null)
                {
                    msg.OperateTask.Cancel();
                    msg.OperateTask = null;
                }

                string tip = "对方撤回了一条消息";
                var tempMsgs = (this.Model as ChatModel).Messages.ToList();
                var tempMsg = tempMsgs.FirstOrDefault(m => m.MsgKey == msg.RetractId);
                MessageModel messageModel = new MessageModel();
                messageModel.MsgKey = string.Empty;
                messageModel.MsgType = MessageType.notification;
                messageModel.RetractId = msg.RetractId;
                messageModel.SendTime = msg.SendTime;
                if (msg.MessageState == MessageStates.Fail)
                {
                    return; //已经是失败状态，不再提示
                }
                if (isUnRead && this.UnReadCount > 0 && !isSync)
                {
                    if (this.UnReadCount > 0)
                    {
                        this.UnReadCount--;
                    }
                }
                if (rType != SDKClient.SDKProperty.RetractType.Normal)//是否在线
                {
                    msg.MsgType = MessageType.notification;
                    string size = Helper.FileHelper.FileSizeToString(tempMsg.ResourceModel.Length);
                    if (tempMsg.ResourceModel != null && tempMsg.ResourceModel.RefInfo != null)
                    {
                        switch (rType)
                        {
                            case SDKClient.SDKProperty.RetractType.TargetEndOnlineRetract:
                                tip = $"对方取消了\"{ tempMsg.ResourceModel.FileName}\"({size})的发送，发送失败。";
                                break;
                            case SDKClient.SDKProperty.RetractType.OnlineToOffline:
                                tip = string.Format("对方已取消在线文件\"{0}\"({1})的发送，已转为离线发送。", tempMsg.ResourceModel.FileName, size);
                                break;
                            case SDKClient.SDKProperty.RetractType.SourceEndOnlineRetract:
                                tip = string.Format("对方取消了\"{0}\"({1})的发送，文件传输失败。", tempMsg.ResourceModel.FileName, size);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        tip = $"对方取消了\"{ tempMsg.ResourceModel.FileName}\"({size})的接收，文件传输失败。";
                    }
                    messageModel.MessageState = MessageStates.Fail;
                    messageModel.MsgKey = msg.MsgKey;

                    //msg.MessageState = MessageStates.Fail;
                    //msg.MsgType = MessageType.notification;
                    msg.Content = tip;
                    msg.MessageState = MessageStates.Fail;
                    var list = Views.Controls.FileChatItem.AcioningItems.ToList();
                    foreach (var item in list)
                    {
                        if (item.DataContext is MessageModel target && target.MsgKey == msg.MsgKey)
                        {
                            item.Cancel(true);
                            break;
                        }
                    }

                    var list1 = Views.Controls.SmallVideo.AcioningItems.ToList();
                    foreach (var item in list1)
                    {
                        if (item.DataContext is MessageModel target && target.MsgKey == msg.MsgKey)
                        {
                            item.Cancel(true);
                            break;
                        }
                    }
                }
                else
                {
                    if (Chat.IsGroup)
                    {
                        tip = "[" + displayName + "] 撤回了一条消息";
                    }
                    if (isSync || (msg.Sender != null && msg.Sender.ID == AppData.Current.LoginUser.ID))
                    {
                        tip = "您撤回了一条消息";
                    }
                    msg.MsgType = MessageType.notification;
                    msg.Content = tip;
                }

                messageModel.Content = tip;
                //var tempMessages = (this.Model as ChatModel).Messages.ToList();
                //int index = tempMessages.IndexOf(msg);
                //if (index >= 0)
                //{

                int index = 0;
                if (tempMsg != null)
                {
                    index = tempMsgs.IndexOf(tempMsg);
                    (this.Model as ChatModel).Messages.Remove(tempMsg);
                }
                SetMsgShowTime(messageModel);
                var tempRetractMsg = tempMsgs.FirstOrDefault(m => m.MsgKey == msg.MsgKey);
                if (tempRetractMsg == null)
                {
                    if (index != 0)
                        (this.Model as ChatModel).Messages.Insert(index, msg);
                    else
                    {

                        //index = (this.Model as ChatModel).Messages.Count;
                        var tempMessages = (this.Model as ChatModel).Messages;
                        int lastIndex = tempMessages.Count - 1;

                        if (lastIndex > -1)
                        {
                            for (int i = lastIndex; i > -1; i--)
                            {
                                if (tempMessages[i].SendTime <= msg.SendTime)
                                {
                                    var tempMessage = Chat.Messages.ToList().FirstOrDefault(n => n.MsgKey == msg.MsgKey);
                                    if (tempMessage != null) break;
                                    Chat.Messages.Insert(i + 1, msg);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            (this.Model as ChatModel).Messages.Add(msg);
                        }
                    }
                }
                else
                {
                    //(this.Model as ChatModel).Messages.Add(msg);
                }
                //}



                AppData.MainMV.UpdateUnReadMsgCount();
                AppData.MainMV.ChatListVM.TotalUnReadCount = AppData.MainMV.TotalUnReadCount;
                if (AppData.MainMV.ChatListVM.TotalUnReadCount == 0)
                {
                    AppData.MainMV.ChatListVM.CloseTrayWindow();
                }
                else
                {
                    AppData.MainMV.ChatListVM.FlashIcon(this);
                }


                if (msg != null && msg.Sender != null && msg.Sender.ID != AppData.Current.LoginUser.ID)
                {
                    if (msg.ResourceModel != null && msg.ResourceModel.FullName != null &&
                        msg.ResourceModel.FullName.Contains(SDKClient.SDKProperty.filePath) &&
                        File.Exists(msg.ResourceModel.FullName))
                    {
                        try
                        {
                            File.Delete(msg.ResourceModel.FullName);
                        }
                        catch
                        {

                        }
                    }
                }

                if (msg.IsAtMeMsg)
                {
                    if (this.AtMeDic.Count > 0)
                    {
                        this.AtMeDic.TryRemove(msg.MsgKey, out msg);
                        if (this.AtMeDic.Count == 0)
                        {
                            this.IsDisplayAtButton = false;
                            this.HasAtMsg = false;
                        }
                    }

                    if (this.UnReadMsgs.Any(x => x.IsAtMeMsg))
                    {
                        this.HasAtMsg = false;
                    }
                }

                messageModel.TipMessage = tip;
                this.SetLastMsg(messageModel, false);
            });
        }
        #endregion

        /// <summary>
        /// 修改群名
        /// </summary>
        /// <param name="para"></param>
        void ModifyGroupName(object para)
        {
            ChatModel chatModel = this.Model as ChatModel;
            if (!this.IsGroup)
            {
                return;
            }

            GroupViewModel groupVM = AppData.MainMV.GroupListVM.Items.FirstOrDefault(i => i.ID == this.ID);
            if (!groupVM.IsCreator && !groupVM.IsAdmin)
            {
                return;
            }

            GroupModel group = (GroupModel)chatModel.Chat;
            string newValue = EditStringValueWindow.ShowInstance(group.DisplayName, "修改群名称");
            EditStringValueWindow.Win.GroupNameEvent -= EditStringValueWindow_GroupNameEvent;
            EditStringValueWindow.Win.GroupNameEvent += EditStringValueWindow_GroupNameEvent;

            //string value = string.Format("{0}", newValue).Trim();
            //if (group != null && value != group.DisplayName)
            //{
            //    //若为空
            //    if (string.IsNullOrEmpty(value))
            //    {
            //        group.DisplayName = group.DisplayName;

            //    }
            //    else if (AppData.CanInternetAction())
            //    {
            //        group.DisplayName = value;
            //        SDKClient.SDKClient.Instance.UpdateGroup(group.ID, SDKClient.Model.SetGroupOption.修改群名称, group.DisplayName);
            //    }
            //    else //网络已断开
            //    {
            //        group.DisplayName = group.DisplayName;

            //        //IMUI.View.V2.MessageTip.ShowTip("修改群名称失败", IMUI.View.V2.TipTypes.Error);
            //        //(sender as TextBox).Text = groupModel.Name;  
            //    }
            //}
        }
        private void EditStringValueWindow_GroupNameEvent(object obj)
        {
            //ObservableCollection<object> o = obj as ObservableCollection<object>;
            //if (o == null)
            //    return;
            string para = (string)obj;
            ChatModel chatModel = this.Model as ChatModel;
            GroupModel group = (GroupModel)chatModel.Chat;

            //TextBox txbBox = o[0] as TextBox;
            //TextBlock textBlock = (o[1] as Border).Child as TextBlock;
            //Border bord_Tip = o[1] as Border;
            string value = string.Format("{0}", para).Trim();
            int index = 0;
            bool isFirstBadWord = SDKClient.SDKProperty.stringSearchEx.FindFirst(value, out index);
            bool isContainsBadWord = SDKClient.SDKProperty.stringSearchEx.ContainsAny(value);
            List<string> badWordLi = SDKClient.SDKProperty.stringSearchEx.FindAll(value);
            List<string> goodWordLi = value.Split(badWordLi.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> badWordLiOperate = new List<string>();
            StringBuilder stringBuilder = new StringBuilder(value);
            foreach (string child in badWordLi)
            {
                badWordLiOperate.Add("|" + child + "|");
            }
            foreach (string child1 in badWordLi)
            {
                foreach (string child2 in badWordLiOperate)
                {
                    if (child1.Equals(child2.Replace("|", string.Empty)))
                        stringBuilder.Replace(child1, child2);
                }
            }
            List<string> resultList = new List<string>(stringBuilder.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
            if (isFirstBadWord || isContainsBadWord)
            {
                //txbBox.Visibility = System.Windows.Visibility.Collapsed;
                //bord_Tip.Visibility = Visibility.Visible;
                //textBlock.Inlines.Clear();

                //BrushConverter brushConverter = new BrushConverter();
                //Brush brush = (Brush)brushConverter.ConvertFromString("#FEE057");
                //foreach (string child in resultList)
                //{
                //    if (badWordLi.Contains(child))
                //    {
                //        textBlock.Inlines.Add(new Run(child) { Background = brush });
                //    }
                //    else
                //    {
                //        textBlock.Inlines.Add(new Run(child));
                //    }
                //}
                StringBuilder sb = new StringBuilder();
                badWordLi.ForEach(x => sb.Append("\"" + x + "\"" + "、"));
                string result = sb.ToString().TrimEnd('、');
                if (IMClient.Views.MessageBox.ShowDialogBox(string.Format("【群名】中包含敏感词{0}，请修改后再试", result), isCancelShow: false))
                {
                    //bord_Tip.Visibility = Visibility.Collapsed;
                    //txbBox.Visibility = Visibility.Visible;
                    //txbBox.Focus();
                }
                //else
                //{
                //    bord_Tip.Visibility = Visibility.Collapsed;
                //    txbBox.Visibility = Visibility.Visible;
                //    txbBox.Focus();
                //}
            }
            else
            {
                if (group != null && value != group.DisplayName)
                {
                    //若为空
                    if (string.IsNullOrEmpty(value))
                    {
                        group.DisplayName = group.DisplayName;
                    }
                    else if (AppData.CanInternetAction())
                    {
                        group.DisplayName = value;
                        SDKClient.SDKClient.Instance.UpdateGroup(group.ID, SDKClient.Model.SetGroupOption.修改群名称, group.DisplayName);
                    }
                    else //网络已断开
                    {
                        group.DisplayName = group.DisplayName;
                    }
                }
            }
        }
        /// <summary>
        /// 删除聊天
        /// </summary>
        /// <param name="para"></param>
        async void DeleteChat(object para)
        {
            ChatModel chatModel = this.Model as ChatModel;

            //bool? result = App.IsCancelOperate("删除聊天", "您有文件正在传输中，确定终止文件传输吗？", chatModel.ID);
            //if (result == true)
            //{
            //    return;
            //}
            if (!IMClient.Views.MessageBox.ShowDialogBox("删除后，将清空该聊天的消息记录"))
            {
                return;
            }
            try
            {
                if (chatModel.ID == -2)
                {
                    //粉丝留言
                    for (int i = 0; i < AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList.Count; i++)
                    {
                        AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList[i].Chat.Messages.Clear();
                        await SDKClient.SDKClient.Instance.DeleteHistoryMsg(AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList[i].Chat.ID, SDKClient.SDKProperty.chatType.chat);
                    }
                    AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList.Clear();
                    AppData.MainMV.ChatListVM.StrangerMessage.UnReadCount = 0;
                    AppData.MainMV.ChatListVM.DeleteChatItem(this.ID);
                    AppData.MainMV.ChatListVM.StrangerChatIds.Clear();
                }
                else
                {
                    App.CancelFileOperate(chatModel.ID);
                    int roomType = this.IsGroup ? 1 : 0;

                    // SDKClient.SDKClient.Instance.UpdateHistoryMsgIsReaded(this.Model.ID, roomType, true, this.UnReadCount, true);
                    SDKClient.SDKClient.Instance.UpdateChatRoomVisibility(this.Model.ID, roomType, false);

                    //删除聊天条目 
                    AppData.MainMV.ChatListVM.DeleteChatItem(this.ID, this.IsGroup);
                    await SDKClient.SDKClient.Instance.DeleteHistoryMsg(chatModel.ID, roomType == 1 ? SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat);
                    Chat.Messages.Clear();
                    if (AppData.MainMV.ChatListVM.SearchVisibility == System.Windows.Visibility.Visible)
                    {
                        AppData.MainMV.ChatListVM.Search(AppData.MainMV.SearchKey);
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex.StackTrace);
            }
        }

        /// <summary>
        /// 主页跳转到群信息页面
        /// </summary>
        /// <param name="para"></param>
        private void MainJupmToNewConent(object para)
        {
            ChatModel model = this.Model as ChatModel;
            if (model != null && model.Chat is GroupModel)
            {
                AppData.MainMV.JumpToGroupModel(model.Chat as GroupModel);
            }
        }

        /// <summary>
        /// 免打扰
        /// </summary> 
        private async void NoDisturb(object para)
        {
            if (!AppData.CanInternetAction())
            {
                return;
            }

            ChatModel chat = this.Model as ChatModel;
            int id = chat.Chat.ID;

            chat.Chat.IsNotDisturb = !chat.Chat.IsNotDisturb;
            string content = chat.Chat.IsNotDisturb ? "1" : "0";

            if (chat.IsGroup)
            {
                SDKClient.SDKClient.Instance.UpdateUserSetsInGroup(id, 3, content);
            }
            else
            {
                if (AppData.MainMV.FriendListVM.Items.ToList().Any(x => x.ID == id))
                {
                    SDKClient.SDKClient.Instance.UpdateFriendSet(SDKClient.Model.UpdateFriendSetPackage.FriendSetOption.设置免打扰, content, id);
                }
                else
                {
                    int isNotdisturb = chat.Chat.IsNotDisturb ? 1 : 0;
                    if (id != -2)
                    {
                        await SDKClient.SDKClient.Instance.SetStrangerDoNotDisturb(AppData.Current.LoginUser.User.ID, id, isNotdisturb);
                    }
                    else
                    {
                        if (ChatListViewModel.ExeConfig.AppSettings.Settings["StrangerMessagedoNotDisturb"] != null)
                        {
                            ChatListViewModel.ExeConfig.AppSettings.Settings.Remove("StrangerMessagedoNotDisturb");
                            ChatListViewModel.ExeConfig.AppSettings.Settings.Add("StrangerMessagedoNotDisturb", content);
                            ChatListViewModel.ExeConfig.Save();
                        }
                        else
                        {
                            ChatListViewModel.ExeConfig.AppSettings.Settings.Add("StrangerMessagedoNotDisturb", content);
                            ChatListViewModel.ExeConfig.Save();
                        }


                        AppData.MainMV.ChatListVM.SetStrangerDisturb(chat.Chat.IsNotDisturb);

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            AppData.MainMV.ChatListVM.IsCloseTrayWindow(false);
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 聊天置顶
        /// </summary>
        private async void TopMost(object para)
        {
            if (!IsFileAssistant)
            {
                if (!AppData.CanInternetAction())
                {
                    MessageModel msg = new MessageModel()
                    {
                        Content = "网络已中断",
                        SendTime = DateTime.Now,
                        TipMessage = "网络已中断"
                    };
                    AppData.MainMV.ChatListVM.Items.ToList().First().AppendMessage(msg);
                    return;
                }
            }


            ChatModel chat = this.Model as ChatModel;
            int id = chat.Chat.ID;

            chat.Chat.TopMostTime = (chat.Chat.TopMostTime.HasValue && chat.Chat.TopMostTime.Value == DateTime.MinValue) ?
                DateTime.Now : DateTime.MinValue;
            chat.Chat.IsTopMost = (chat.Chat.TopMostTime == DateTime.MinValue) ? false : true;
            string content = (chat.Chat.TopMostTime.Value != DateTime.MinValue) ? "1" : "0";
            if (IsFileAssistant)
            {
                SDKClient.SDKClient.Instance.UpdateAccountTopMostTime(chat.Chat.TopMostTime);

                App.Current.Dispatcher.Invoke(() =>
                {
                    AppData.MainMV.ChatListVM.ResetSort();
                });
                return;
            }
            if (chat.IsGroup)
            {
                //if (chat.Chat.TopMostTime.HasValue && chat.Chat.TopMostTime.Value != DateTime.MinValue)
                //{
                //    SDKClient.SDKClient.Instance.UpdateUserSetsInGroup(id, 2, content);
                //}
                //else
                //{
                //    SDKClient.SDKClient.Instance.UpdateUserSetsInGroup(id, 2, content);
                //}
                SDKClient.SDKClient.Instance.UpdateUserSetsInGroup(id, 2, content);
            }
            else
            {
                //if (chat.Chat.TopMostTime.HasValue && chat.Chat.TopMostTime.Value != DateTime.MinValue)
                //{
                //    SDKClient.SDKClient.Instance.UpdateFriendSet(SDKClient.Model.UpdateFriendSetPackage.FriendSetOption.设置是否消息置顶, content, id);
                //}
                //else
                //{
                //    SDKClient.SDKClient.Instance.UpdateFriendSet(SDKClient.Model.UpdateFriendSetPackage.FriendSetOption.设置是否消息置顶, content, id);
                //}


                if (AppData.MainMV.FriendListVM.Items.ToList().Any(x => x.ID == id))
                {
                    SDKClient.SDKClient.Instance.UpdateFriendSet(SDKClient.Model.UpdateFriendSetPackage.FriendSetOption.设置是否消息置顶, content, id);
                }
                else
                {
                    if (id != -2)
                    {
                        await SDKClient.SDKClient.Instance.SetStrangerChatTopTime(id, chat.Chat.TopMostTime.Value);
                    }
                    else
                    {
                        string value = chat.Chat.IsTopMost ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss");
                        if (ChatListViewModel.ExeConfig.AppSettings.Settings["StrangerChatTopTime"] != null)
                        {
                            ChatListViewModel.ExeConfig.AppSettings.Settings.Remove("StrangerChatTopTime");
                            ChatListViewModel.ExeConfig.AppSettings.Settings.Add("StrangerChatTopTime", value);
                            ChatListViewModel.ExeConfig.Save();
                        }
                        else
                        {
                            ChatListViewModel.ExeConfig.AppSettings.Settings.Add("StrangerChatTopTime", value);
                            ChatListViewModel.ExeConfig.Save();
                        }

                    }
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        AppData.MainMV.ChatListVM.ResetSort();
                    });
                }
            }
        }

        private void Defriend(object para)
        {
            if (!AppData.CanInternetAction())
            {
                return;
            }

            ChatModel chat = this.Model as ChatModel;
            UserModel user = chat.Chat as UserModel;

            if (user.IsDefriend)
            {
                AppData.MainMV.BlacklistVM.UpdateFriendRelation(user.ID, 0);
            }
            else
            {
                string tip = $"确定将{user.DisplayName}移至黑名单？";
                var result = Views.MessageBox.ShowDialogBox(tip);
                if (!result)
                {
                    return;
                }
                user.IsApplyFriend = false;
                SDKClient.SDKClient.Instance.UpdateFriendRelation(1, user.ID);
            }
            user.IsDefriend = !user.IsDefriend;
        }

        /// <summary>
        /// 清屏
        /// </summary>
        /// <param name="para"></param>
        private void Clear(object para)
        {
            bool? result = App.IsCancelOperate("清空屏幕", "您有文件正在传输中，确定终止文件传输吗？", -1, false, this.ID);
            if (result == true) //取消，不做操作
            {

            }
            else  //继续操作，则直接执行
            {
                this.Chat.Messages.Clear();
                this.SetLastMsg(new MessageModel() { SendTime = DateTime.Now }, false);
                this.IsDisplayMsgHint = false;
            }
        }
        #endregion

        /// <summary>
        /// 设置最新消息
        /// </summary>
        /// <param name="msg">最新消息条目</param>
        /// <param name="tip">消息内容tip</param>
        /// <param name="isAddTimeMsg">是否加一条时间消息</param>
        public void SetLastMsg(MessageModel msg, bool isAddTimeMsg = true, bool isDelMsg = false)
        {
            if (msg != null)
            {
                if (isAddTimeMsg)
                {
                    SetMsgShowTime(msg);
                }

                IChat chat = msg.Sender;

                if (this.IsGroup && chat != null)
                {
                    UserModel user = AppData.Current.GetUserModel(chat.ID);
                    GroupMember member = user.GetInGroupMember((this.Model as ChatModel).Chat as GroupModel);
                    chat = member;
                }

            }
            if (msg != null && Chat.LastMsg != null && !isDelMsg)
            {
                if (string.IsNullOrEmpty(msg.RetractId))
                    Chat.LastMsg = msg;
                else
                {
                    if (string.IsNullOrEmpty(Chat.LastMsg.Content))
                    {
                        Chat.LastMsg = msg;
                    }
                    else if (msg.MsgType == MessageType.notification
                        && Chat.LastMsg.SendTime <= msg.SendTime
                        && !string.IsNullOrEmpty(msg.RetractId)
                        && msg.RetractId == Chat.LastMsg.MsgKey)
                    {
                        Chat.LastMsg = msg;
                    }

                    //Chat.LastMsg.TipMessage = string.Empty;
                    //Chat.LastMsg.SendTime=DateTime
                }
            }
            else if (msg != null)
            {
                Chat.LastMsg = msg;
                if (string.IsNullOrEmpty(Chat.LastMsg.TipMessage))
                {
                    Chat.LastMsg.TipMessage = msg.Content;
                }
            }
            //if (Chat.LastMsg.Sender == null || Chat.LastMsg.MsgType == MessageType.notification)
            //{

            //}
        }

        private void PopUpNotice(List<MessageModel> messageModels)
        {
            this.IsHasPopup = true;
            if (messageModels.Any(x => x.NoticeModel.NoticeTitle.Equals("入群须知")))
            {
                var targetData = messageModels.FirstOrDefault(x => x.NoticeModel.NoticeTitle.Equals("入群须知"));
                MessageModel msg = new MessageModel()
                {
                    Sender = AppData.Current.LoginUser.User,
                    SendTime = targetData.SendTime,
                    MsgType = MessageType.addgroupnotice,
                    IsMine = true,
                    Content = targetData.Content,
                };
                GroupNoticeModel gmtc = new GroupNoticeModel()
                {
                    NoticeTitle = targetData.NoticeModel.NoticeTitle,
                    //GroupMId = (groupVM.Model as GroupModel).ID
                };
                msg.NoticeModel = gmtc;
                msg.TipMessage = string.Format("{0}：{1}", msg.Sender.DisplayName, msg.NoticeModel.NoticeTitle);
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (AppData.tipWindow != null)
                    {
                        AppData.tipWindow.Close();
                        Views.ChildWindows.GroupNoticeTipWindow win = new GroupNoticeTipWindow(msg, true);
                        win.Owner = App.Current.MainWindow;
                        win.ShowDialog();
                    }
                    else
                        AppData.tipWindow = new Views.ChildWindows.GroupNoticeTipWindow(msg);
                }));
            }
            else
            {
                var targetData = messageModels.OrderByDescending(x => x.SendTime).First();
                MessageModel msg = new MessageModel()
                {
                    Sender = AppData.Current.LoginUser.User,
                    SendTime = targetData.SendTime,
                    MsgType = MessageType.addgroupnotice,
                    IsMine = true,
                    Content = targetData.Content,
                };
                GroupNoticeModel gmtc = new GroupNoticeModel()
                {
                    NoticeTitle = targetData.NoticeModel.NoticeTitle,
                    //GroupMId = (groupVM.Model as GroupModel).ID
                };
                msg.NoticeModel = gmtc;
                msg.TipMessage = string.Format("{0}：{1}", msg.Sender.DisplayName, msg.NoticeModel.NoticeTitle);
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (AppData.tipWindow != null)
                    {
                        AppData.tipWindow.Close();
                        Views.ChildWindows.GroupNoticeTipWindow win = new GroupNoticeTipWindow(msg, true);
                        win.Owner = App.Current.MainWindow;
                        win.ShowDialog();
                    }
                    else
                        AppData.tipWindow = new Views.ChildWindows.GroupNoticeTipWindow(msg);
                }));

            }
        }

        /// <summary>
        /// 是否已经弹出来过
        /// </summary>
        public bool IsHasPopup = false;
        private bool IsUpdateRead = false;
        /// <summary>
        /// 活跃状态，即当前显示
        /// </summary>
        public void Acitve()
        {
            this.HasAtMsg = false;
            this.HasNewGroupNotice = false;
            this.IsAllRead = true;
            if (this.IsGroup)
            {
                var groupVM = AppData.MainMV.GroupListVM.Items.ToList().FirstOrDefault(info => info.ID == this.ID);
                if (groupVM != null && (this.noticeMessage.Count > 0 || this.offlineNoticeMessage.Count > 0))
                {
                    if (!this.IsIgnoreAllMessage)
                    {
                        if (AppData.MainMV.ChatListVM.IsChangeSelected)
                        {
                            List<MessageModel> onLineNoticeMessage = this.noticeMessage.Values.ToList();//未读的群公告
                            List<MessageModel> offLineNoticeMessage = this.offlineNoticeMessage.Values.ToList();//未读的离线群公告
                            if (onLineNoticeMessage.Count > 0)
                            {
                                PopUpNotice(onLineNoticeMessage);
                            }
                            else if (offLineNoticeMessage.Count > 0)
                            {
                                PopUpNotice(offLineNoticeMessage);
                            }
                            this.offlineNoticeMessage.Clear();
                            this.noticeMessage.Clear();
                            this.OnlineAndOfflineMessage.Clear();
                        }
                    }
                }
                if (groupVM == null)//已经退出的群（被踢出）
                {
                    groupVM = new GroupViewModel(AppData.Current.GetGroupModel(this.ID), "已经退出的群");
                    this.IsHideAppendButton = true;
                    this.IsShowGroupNoticeBtn = false;
                    this.HasNewGroupNotice = false;
                    AppData.MainMV.GroupListVM.DissolveGroups.Add(groupVM);
                }
                ThreadPool.QueueUserWorkItem(o =>
                {
                    TryGetGroupInfo(groupVM);
                });
            }
            var unReadCount = UnReadCount;
            //ThreadPool.QueueUserWorkItem(o =>
            //{
            if (!IsUpdateRead)
            {
                var tempTask = Task.Run(() =>
                {
                    IsUpdateRead = true;
                    SDKClient.SDKClient.Instance.UpdateHistoryMsgIsReaded(this.ID, this.IsGroup ? 1 : 0, true, unReadCount);
                }).ContinueWith(t =>
                {
                    IsUpdateRead = false;
                });
            }
            //});
            if (UnReadServerMsgID.Count > 0)
            {
                //SDKClient.SDKClient.Instance.SendSyncMsgStatus(this.ID, UnReadServerMsgID.Count, UnReadServerMsgID.LastOrDefault(),
                //    this.IsGroup ? SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat);

                UnReadServerMsgID.Clear();
            }
            var tempChatMsgs = Chat.Messages.ToList();
            if (!this.HasActived)
            {
                if (this.IsGroup)
                {
                    var groupVM = AppData.MainMV.GroupListVM.Items.ToList().FirstOrDefault(info => info.ID == this.ID);

                    if (groupVM != null)
                    {
                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            //获取申请列表
                            SDKClient.SDKClient.Instance.GetJoinGroupList(this.ID);
                        });
                    }
                }
                if (tempChatMsgs.Count > 0)
                {
                    foreach (var msg in tempChatMsgs)
                    {
                        UnReadMsgs.Add(msg);
                    }
                }

                App.Current.Dispatcher.Invoke(new Action(() =>
               {
                   this.Chat.Messages.Clear();

               }));
                //加载最新历史消息
                this.LoadHisMessages(false);
                this.HasActived = true;
                //UnReadCount = 0;
            }
            else
            {
                this.IsDisplayAtButton = false;

                var newMsgs = UnReadMsgs.OrderBy(info => info.SendTime).ToList();
                UnReadMsgs.Clear();
                int showCount = newMsgs.Count + tempChatMsgs.Count;
                //大于最大数值，先清空 只显示新消息
                if (newMsgs.Count >= MAXCOUNT)
                {
                    int count = newMsgs.Count;
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Chat.Messages.Clear();
                    });
                    newMsgs = newMsgs.Skip(count - MAXCOUNT).Take(MAXCOUNT).ToList();
                }
                else if (showCount > MAXCOUNT)
                {
                    //允许旧的继续呈现数量
                    int oldCount = MAXCOUNT - newMsgs.Count;

                    if (newMsgs.Count > 0)
                    {
                        while (Chat.Messages.Count > oldCount)
                        {
                            if (Chat.Messages.Count > 0)
                                Chat.Messages.RemoveAt(0);
                        }
                        newMsgs = newMsgs.ToList();
                    }
                }
                var sendDatetime = newMsgs != null && newMsgs.Count > 0 ? newMsgs[0].SendTime : DateTime.Now;
                if (this.UnReadCount > 0)
                    AddNewMessageFlag(this.UnReadCount, sendDatetime);

                UnReadCount = 0;

                //int tempCount = Chat.Messages.ToList().Count;
                foreach (var m in newMsgs)
                {
                    var tempMsg = tempChatMsgs.ToList().FirstOrDefault(n => n.MsgKey != null && n.MsgKey == m.MsgKey);
                    if (tempMsg != null) continue;
                    //App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                    //{

                    //}));

                    //System.Threading.Thread.CurrentThread.Join(1);
                    App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                    {
                        SetMsgShowTime(m);
                        //if (!string.IsNullOrEmpty(m.Content))
                        Chat.Messages.Add(m);
                        if (m.MsgType == MessageType.notification && (m.Content == ConstString.BecomeFriendsTip || m.Content == "已经是好朋友，开始聊天吧"))
                        {

                            var tempStrangerMsg = newMsgs.Where(sm => sm.SendTime < m.SendTime).ToList();

                            if (tempStrangerMsg.Count > 0)
                                AddMessageTipEx("以上为打招呼内容", m.SendTime, m.MsgKey);
                            // AddMessageTipEx("已经成为好友，开始聊天吧", m.SendTime, m.MsgKey);
                        }

                        //AddNewMessageFlag(this.UnReadCount, sendDatetime);
                    }));
                    if (UnReadCount > 0)
                        UnReadCount = 0;
                }

                if (newMsgs.Count > 0)
                {
                    this.OnDisplayAtButton?.Invoke();
                    this.OnDisplayMsgHint?.Invoke();
                }
                if (tempChatMsgs?.Count > 0)
                {
                    var tempMsgsCount = tempChatMsgs.Where(m => m.ShowSendTime).Count();
                    if (tempMsgsCount <= 0)
                        Chat.Messages[0].ShowSendTime = true;
                }

                //过程中若有更新的消息，则递归
                if (UnReadMsgs.Count > 0)
                {
                    this.Acitve();
                }
            }
        }

        /// <summary>
        /// 添加一条“以下为新消息”的提示信息
        /// </summary>
        public void AddNewMessageFlag(int unReadCount, DateTime dateTime)
        {
            if (unReadCount >= 10)
            {
                if (unReadCount > 99)
                {
                    this.UnReadMsgTip = "99+条消息";
                }
                else
                {
                    this.UnReadMsgTip = string.Format("{0}条消息", unReadCount);
                }

                this.IsDisplayHistoryMsgButton = true;
            }
            else
            {
                this.IsDisplayHistoryMsgButton = false;
                return;
            }

            var msg = Chat.Messages.ToList().FirstOrDefault(x => x.MsgType == MessageType.notification && x.Content == ConstString.FollowingIsNewMessage);
            if (msg != null)
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Chat.Messages.Remove(msg);
                }));
            }


            var hintMsg = new MessageModel();
            hintMsg.MsgType = MessageType.notification;
            hintMsg.Content = ConstString.FollowingIsNewMessage;
            hintMsg.SendTime = dateTime;
            hintMsg.MsgKey = string.Empty;

            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                Chat.Messages.Add(hintMsg);
                //this.AppendMessage?.Invoke(null);
            }));
        }

        public void Unload()
        {
            if (this.Model is ChatModel chat && chat.Messages.Count > 20)
            {
                for (int i = 0; i < chat.Messages.Count - 20; i = 0)
                {
                    chat.Messages.RemoveAt(0);
                }
            }
        }

        Task _loadHisTask;
        bool IsReading = false;

        /// <summary>
        /// 加载历史消息
        /// </summary>
        /// <param name="isForward">是否向前滚动追加</param>
        public void LoadHisMessages(bool isForward = false)
        {
            var msgs = (this.Model as ChatModel).Messages;
            //if (_loadHisTask != null && _loadHisTask.Status != TaskStatus.RanToCompletion)
            //{
            //    return;
            //}
            if (IsReading)
                return;
            if (msgs.Count >= MAXCOUNT)
            {
                this.IsFullPage = true;
                return;
            }
            _loadHisTask = Task.Run(() =>
            {
                IsReading = true;
                List<SDKClient.DB.messageDB> datas = null;
                MessageModel top = null;
                int queryMsgCount = 0;
                top = msgs.Where(x => !string.IsNullOrEmpty(x.MsgKey)).FirstOrDefault();
                if (isForward)
                {
                    if (top != null)
                    {
                        SDKClient.SDKProperty.chatType chatType = this.IsGroup ? SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;
                        datas = SDKClient.SDKClient.Instance.GetHistoryMsg(this.Model.ID, top.MsgKey, 10, chatType: chatType);
                    }

                    if (datas == null || datas.Count == 0)
                    {
                        WarningInfo = "没有更多消息";
                        return;
                    }
                }
                else
                {
                    queryMsgCount = TempUnReadCount == 0 ? UnReadCount : TempUnReadCount;//默认加载全部未读消息
                    TempUnReadCount = 0;
                    UnReadCount = 0;
                    var newMsgs = UnReadMsgs.OrderBy(info => info.SendTime).ToList();
                    var sendDatetime = newMsgs != null && newMsgs.Count > 0 ? newMsgs[0].SendTime : DateTime.Now;
                    if (queryMsgCount >= 0 && queryMsgCount <= 6)
                    {
                        queryMsgCount = 6;//没有未读消息或者未读消息数<=6条时，加载最近的6条历史消息
                    }
                    else if (queryMsgCount >= MAXCOUNT)
                    {
                        AddNewMessageFlag(queryMsgCount, sendDatetime);
                        queryMsgCount = MAXCOUNT;//未读消息>=300条时，加载最近的300条未读消息
                    }
                    else
                    {
                        AddNewMessageFlag(queryMsgCount, sendDatetime);
                    }
                    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                    stopwatch.Start();
                    if (FirstMessages.Count == 0)
                    {
                        SDKClient.SDKProperty.chatType chatType = this.IsGroup ? SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;
                        datas = SDKClient.SDKClient.Instance.GetHistoryMsg(this.Model.ID, queryMsgCount, chatType: chatType);
                    }
                    stopwatch.Stop();
                    long time = stopwatch.ElapsedMilliseconds;
                    System.Diagnostics.Debug.WriteLine("获取历史消息耗时：" + time);
                }
                if (datas != null && datas.Count > 0)
                {
                    var tempDatas = datas.Where(m => m.optionRecord == 0).ToList();
                    if (tempDatas?.Count > 0)
                    {
                        if (!IsUpdateRead)
                        {
                            var tempTask = Task.Run(() =>
                            {
                                IsUpdateRead = true;
                                SDKClient.SDKClient.Instance.UpdateHistoryMsgIsReaded(this.ID, this.IsGroup ? 1 : 0, true, tempDatas.Count);
                            }).ContinueWith(t =>
                            {
                                IsUpdateRead = false;
                            });
                        }
                    }
                }

                int fromId;
                if (FirstMessages.Count == 0)
                {
                    System.Diagnostics.Stopwatch stopwatch1 = new System.Diagnostics.Stopwatch();
                    stopwatch1.Start();
                    if (this.IsFileAssistant)
                    {
                        datas = datas.Where(m => m.sessionType == (int)SDKClient.SDKProperty.SessionType.FileAssistant).ToList();
                    }

                    foreach (var m in datas)
                    {
                        if (int.TryParse(m.from, out fromId) && fromId > 0)
                        {

                            #region 消息类型
                            MessageType type;
                            try
                            {
                                type = (MessageType)Enum.Parse(typeof(MessageType), m.msgType.ToLower());
                            }
                            catch
                            {
                                //Views.MessageBox.ShowBox("收到未知消息类型：" + m.msgType);
                                continue;
                            }
                            #endregion

                            IChat sender;

                            object target = null;
                            bool isMine = AppData.Current.LoginUser.User.ID == fromId;
                            if (isMine) //我发送的消息，统一放在右边
                            {
                                sender = AppData.Current.LoginUser.User;
                            }
                            else if (this.IsGroup) //群成员发送的消息，要现实的是成员在  我给好友备注的昵称>群成员自己设置的昵称>群成员名称
                            {
                                UserModel user = AppData.Current.GetUserModel(fromId);
                                GroupModel group = AppData.Current.GetGroupModel(m.roomId);
                                sender = user.GetInGroupMember(group);
                                if (string.IsNullOrEmpty(user.Name))
                                {
                                    SDKClient.SDKClient.Instance.GetGroupMember(AppData.Current.LoginUser.ID, group.ID, fromId);
                                }
                            }
                            else //单聊好友发送的消息,只显示头像，不显示昵称（聊天框已有昵称显示）
                            {
                                sender = AppData.Current.GetUserModel(fromId);
                            }

                            bool isFailureGroupCard = false;
                            if (type == MessageType.invitejoingroup)
                            {
                                target = LoadGroupCard(m.Source, fromId);
                                UserModel user = AppData.Current.GetUserModel(m.roomId);
                                if (user != null && (user.LinkDelType == 2 || user.LinkType >= 2))
                                {
                                    isFailureGroupCard = true;
                                }
                                if (target == null)
                                {
                                    continue;
                                }
                            }

                            MessageModel msg = new MessageModel()
                            {
                                MsgKey = m.msgId,
                                Sender = sender,
                                SendTime = m.msgTime,
                                IsMine = isMine,
                                MsgType = type,
                                Target = target,
                            };
                            if ((m.optionRecord & (int)SDKClient.SDKProperty.MessageState.sendfaile) == (int)SDKClient.SDKProperty.MessageState.sendfaile ||
                                (m.optionRecord & (int)SDKClient.SDKProperty.MessageState.cancel) == (int)SDKClient.SDKProperty.MessageState.cancel
                                || (m.optionRecord & (int)SDKClient.SDKProperty.MessageState.sending) == (int)SDKClient.SDKProperty.MessageState.sending
                            )
                            {
                                msg.MessageState = MessageStates.Fail;
                            }
                            if (isFailureGroupCard)
                            {
                                msg.MessageState = MessageStates.Fail;
                            }
                            if (m.Source != null)
                            {
                                MessagePackage pak = Util.Helpers.Json.ToObject<SDKClient.Model.MessagePackage>(m.Source);
                                if (pak != null)
                                    msg.IsSync = pak.syncMsg == 1 ? true : false;
                            }
                            if (!isForward)
                            {
                                if (!string.IsNullOrEmpty(m.TokenIds))
                                {
                                    string[] arr = m.TokenIds.Split(',');
                                    foreach (var id in arr)
                                    {
                                        if (id == ConstString.AtAllId.ToString() || id == AppData.Current.LoginUser.ID.ToString())
                                        {
                                            msg.IsAtMeMsg = true;
                                            var tempUnReadMessage = this.UnReadMsgs.ToList().FirstOrDefault(n => n.MsgKey == msg.MsgKey);
                                            if (tempUnReadMessage != null)
                                            {
                                                this.AtMeDic.TryAdd(m.msgId, msg);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            msg.ResourceModel = new FileResourceModel()
                            {
                                Key = m.resourceId,
                                SmallKey = m.resourcesmallId,
                                Length = m.fileSize,
                                FileName = Path.GetFileName(m.fileName),
                                //如果文件名包含绝对路径，那么不从manjinba/file路径中文件
                                FullName = m.fileName,
                                //fileState 0：未开始，1：下载中，2：已完成,3:取消，4：异常
                                DBState = m.fileState,
                            };

                            if (type == MessageType.redenvelopesreceive || (type == MessageType.redenvelopessendout && msg.IsMine))////不显示对方接收的红包和自己发送的红包消息
                                continue;

                            switch (type)
                            {
                                case MessageType.addfriendaccepted:
                                    if (!string.IsNullOrEmpty(m.Source) && m.Source == AppData.Current.LoginUser.ID.ToString())
                                    {
                                        if (SDKClient.SDKClient.Instance.property.FriendApplyList != null && SDKClient.SDKClient.Instance.property.FriendApplyList.Count > 0)
                                        {
                                            var friendApply = SDKClient.SDKClient.Instance.property.FriendApplyList.FirstOrDefault(a => a.userId == m.roomId);
                                            if (friendApply != null)
                                            {

                                                if (string.IsNullOrEmpty(friendApply.applyRemark))
                                                {
                                                    msg.Sender = AppData.Current.GetUserModel(m.roomId);
                                                    msg.MsgType = MessageType.notification;
                                                    msg.TipMessage = msg.Content = m.content;
                                                    break;
                                                }
                                                else
                                                {
                                                    msg.Sender = AppData.Current.GetUserModel(m.roomId);
                                                    msg.IsMine = false;
                                                    msg.MsgType = MessageType.notification;
                                                    msg.Content = m.content;
                                                    //msg.MsgKey = friendApply.msgId;
                                                    msg.TipMessage = m.content;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    msg.MsgType = MessageType.notification;
                                    msg.TipMessage = msg.Content = m.content;
                                    break;
                                case MessageType.foreigndyn:
                                    var foreigndynPackage = Util.Helpers.Json.ToObject<SDKClient.Model.MessagePackage>(m.Source);
                                    if (foreigndynPackage == null) return;
                                    string dynContent = foreigndynPackage.data.body.text;
                                    string msgHyperlink = foreigndynPackage.data.body.url;
                                    string msgstr = foreigndynPackage.data.body.img;
                                    if (string.IsNullOrEmpty(msgstr))
                                    {
                                        msg.ShareMsgImage = IMAssets.ImageDeal.NewsDefaultIcon;
                                    }
                                    else
                                    {
                                        if (FileHelper.IsUrlRegex(msgstr))
                                            msg.ShareMsgImage = msgstr;
                                        else
                                        {
                                            msg.ResourceModel = new FileResourceModel { SmallKey = msgstr };
                                            msg.ShareMsgImage = IMAssets.ImageDeal.NewsDefaultIcon;
                                        }
                                    }
                                    msg.MsgSource = m.Source;
                                    msg.MsgHyperlink = msgHyperlink;

                                    msg.Content = string.IsNullOrEmpty(dynContent) ? msgHyperlink : dynContent;
                                    //msg.ShareMsgImage=
                                    msg.TipMessage = "[链接]" + dynContent;
                                    //链接消息，请在手机端查看";
                                    break;
                                case MessageType.img:
                                    msg.TipMessage = "[图片]";
                                    msg.Content = m.fileName;
                                    break;
                                case MessageType.file:
                                    msg.TipMessage = "[文件]";
                                    msg.Content = m.fileName;
                                    break;
                                case MessageType.onlinefile:
                                    if (File.Exists(m.content))
                                    {
                                        msg.Content = m.content;
                                        msg.MsgType = MessageType.file;
                                        msg.ResourceModel.FullName = msg.Content;
                                    }
                                    else
                                    {
                                        var package1 = Util.Helpers.Json.ToObject<SDKClient.Model.MessagePackage>(m.Source);
                                        var body = package1.data.body;

                                        //string onlineName = Path.GetFileName($"{package.data.body.fileName}");
                                        //string onlinePath = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.filePath, onlineName);
                                        //onlinePath = FileHelper.GetFileName(onlinePath, 1);
                                        //onlineName = Path.GetFileName(onlinePath);

                                        string onlinePath = m.fileName;
                                        string onlineName = Path.GetFileName(onlinePath);

                                        FileResourceModel onlineFile = new FileResourceModel()
                                        {
                                            Key = body.id,
                                            Length = body.fileSize,
                                            FileName = onlineName,
                                            //如果文件名包含绝对路径，那么不从manjinba/file路径中文件
                                            FullName = onlinePath,
                                            RefInfo = new SDKClient.Model.OnlineFileBody()
                                            {
                                                id = body.id,
                                                fileSize = body.fileSize,
                                                fileName = onlineName,
                                                Port = body.Port,
                                                IP = body.IP,
                                            },
                                            DBState = m.fileState,
                                        };
                                        msg.ResourceModel = onlineFile;
                                        msg.MsgType = MessageType.file;
                                        msg.Content = onlinePath;
                                    }
                                    msg.TipMessage = "[文件]";
                                    break;
                                case MessageType.invitejoingroup:
                                    msg.MsgSource = Util.Helpers.Json.ToObject<SDKClient.Model.InviteJoinGroupPackage>(m.Source);
                                    var targetgroup = LoadGroupCard(m.Source, fromId);
                                    msg.Target = targetgroup;
                                    msg.TipMessage = "[群名片]";
                                    break;
                                case MessageType.audio:
                                    msg.TipMessage = "[语音]";
                                    msg.Content = msg.IsMine ? "您发送了一条语音消息，请在手机端查看" : "对方发送语音消息，请在手机端查看";
                                    break;
                                case MessageType.smallvideo:
                                case MessageType.video:
                                    #region video
                                    msg.TipMessage = "[小视频]";
                                    var videoPackage = Util.Helpers.Json.ToObject<SDKClient.Model.MessagePackage>(m.Source);
                                    var videoBody = videoPackage.data.body;
                                    string videoPath = m.fileName;
                                    string videoName = Path.GetFileName(videoPath);

                                    FileResourceModel video = new FileResourceModel()
                                    {
                                        Key = videoBody.id,
                                        PreviewKey = videoBody.previewId,
                                        Length = videoBody.fileSize,
                                        FileName = videoName,
                                        FullName = videoPath,
                                        RecordTime = videoBody.recordTime,
                                        DBState = m.fileState,
                                    };
                                    video.PreviewImagePath = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, video.PreviewKey);
                                    msg.ResourceModel = video;
                                    msg.Content = video.FullName;
                                    #endregion
                                    break;
                                case MessageType.redenvelopesreceive:
                                    msg.MsgType = MessageType.notification;
                                    msg.TipMessage = msg.Content = msg.IsMine ? "您领取了一个红包，请在手机端查看" : "[有人领取了您的红包，请在手机端查看]";
                                    break;
                                case MessageType.redenvelopessendout:
                                    msg.MsgType = MessageType.notification;
                                    msg.TipMessage = msg.Content = "[您有新红包，请在手机上查看]";
                                    break;
                                case MessageType.bigtxt:
                                    msg.MsgType = MessageType.bigtxt;
                                    msg.Content = m.content;
                                    msg.TipMessage = m.content;
                                    break;
                                case MessageType.setmemberpower:
                                    var p = Util.Helpers.Json.ToObject<SetMemberPowerPackage>(m.Source);
                                    this.ShowAddMemberPowerMsg(p, isForward, false);
                                    continue;
                                case MessageType.dismissgroup:

                                    var dismissGroupPackage = Util.Helpers.Json.ToObject<DismissGroupPackage>(m.Source);
                                    this.ShowDismissMsg(dismissGroupPackage, isForward);
                                    continue;
                                case MessageType.exitgroup:
                                    this.AddMessageTip(m.content, m.msgTime, isForward, m.msgId);
                                    continue;
                                case MessageType.notification:
                                    msg.TipMessage = msg.Content = m.content;
                                    int result = m.optionRecord & (int)SDKClient.SDKProperty.MessageState.cancel;
                                    //if (!msg.Content.Contains("被取消群管理员"))
                                    //{ }
                                    //else
                                    //{
                                    //    if (msg.Content.Contains("失败") ||
                                    //        msg.Content.Contains("取消") ||
                                    //        msg.Content.Contains("异常") ||
                                    //        msg.Content.Contains("中断"))
                                    //    {
                                    //        msg.MessageState = MessageStates.Fail;
                                    //    }
                                    //    else if (msg.Content.Contains("成功") && msg.Content.Contains("文件"))
                                    //    {
                                    //        msg.MessageState = MessageStates.Success;
                                    //    }
                                    //}

                                    if (!string.IsNullOrEmpty(m.fileName))
                                    {
                                        if (msg.Content.Contains("失败") ||
                                                msg.Content.Contains("取消") ||
                                                msg.Content.Contains("异常") ||
                                                msg.Content.Contains("中断"))
                                        {
                                            msg.MessageState = MessageStates.Fail;
                                            msg.TipMessage = msg.Content = msg.Content;
                                            break;
                                        }
                                        else if (msg.Content.Contains("成功") && msg.Content.Contains("文件"))
                                        {
                                            msg.MessageState = MessageStates.Success;
                                            msg.TipMessage = msg.Content = msg.Content;
                                            break;
                                        }
                                    }
                                    if (msg.Content.Contains("文件"))
                                    {
                                        if (msg.Content.Contains("失败") ||
                                                msg.Content.Contains("取消") ||
                                                msg.Content.Contains("异常") ||
                                                msg.Content.Contains("中断"))
                                            msg.MessageState = MessageStates.Fail;
                                    }
                                    if (msg.Content.Contains("撤回"))
                                    {
                                        msg.MessageState = MessageStates.None;
                                    }
                                    break;
                                case MessageType.addgroupnotice:
                                    MessagePackage package = Util.Helpers.Json.ToObject<SDKClient.Model.MessagePackage>(m.Source);
                                    GroupNoticeModel gmtc = new GroupNoticeModel()
                                    {
                                        NoticeTitle = package.data.body.title,
                                        NoticeId = package.data.body.noticeId,
                                        GroupNoticeType = package.data.body.type == 0 ? "普通群公告" : "入群须知",
                                        GroupNoticeContent = package.data.body.content
                                    };
                                    msg.Content = package.data.body.content;
                                    msg.TipMessage = package.data.body.title;
                                    if (this.Chat.Chat is GroupModel gModel)
                                    {
                                        int groupId = gModel.ID;
                                        gmtc.GroupMId = groupId;
                                    }
                                    msg.NoticeModel = gmtc;
                                    msg.SendTime = package.data.body.publishTime ?? DateTime.Now;
                                    GroupViewModel groupViewModel = AppData.MainMV.GroupListVM.Items.FirstOrDefault(hhhh => hhhh.ID == this.ID);
                                    if (groupViewModel != null)
                                        this.IsShowGroupNoticeBtn = true;
                                    else
                                        this.IsShowGroupNoticeBtn = false;
                                    if (!noticeMessage.ContainsKey(msg.NoticeModel.NoticeId))
                                        if (AppData.MainMV.ChatListVM.SelectedItem != this)
                                            noticeMessage.Add(msg.NoticeModel.NoticeId, msg);
                                    break;
                                case MessageType.deletegroupnotice:
                                    MessagePackage packagee = Util.Helpers.Json.ToObject<SDKClient.Model.MessagePackage>(m.Source);
                                    int noticeId = packagee.data.body.noticeId;
                                    var dgn = UnReadMsgs.FirstOrDefault(k => k.NoticeModel != null && k.NoticeModel.NoticeId == noticeId);
                                    if (dgn != null)
                                        dgn.NoticeModel.IsHasDelete = true;
                                    else
                                    {
                                        var item = this.Chat.Messages.FirstOrDefault(y => !string.IsNullOrEmpty(y.NoticeModel.NoticeTitle) && y.NoticeModel.NoticeId == noticeId);
                                        if (item != null)
                                            item.NoticeModel.IsHasDelete = true;
                                    }
                                    int i = UnReadMsgs.Count(y => !string.IsNullOrEmpty(y.NoticeModel.NoticeTitle) && y.NoticeModel.NoticeId == noticeId && !y.NoticeModel.IsHasDelete);
                                    this.HasNewGroupNotice = i > 0 ? true : false;
                                    if (noticeMessage.ContainsKey(noticeId))
                                        noticeMessage.Remove(noticeId);
                                    return;
                                case MessageType.retract:
                                    msg.TipMessage = msg.Content = m.content;
                                    msg.MsgType = MessageType.notification;
                                    if (msg.Content.Contains("撤回"))
                                    {
                                        msg.MessageState = MessageStates.None;
                                    }

                                    if (msg.Content.Contains("失败") ||
                                            msg.Content.Contains("取消") ||
                                            msg.Content.Contains("异常") ||
                                            msg.Content.Contains("中断"))
                                    {
                                        msg.MessageState = MessageStates.Fail;
                                    }
                                    else if (msg.Content.Contains("成功") && msg.Content.Contains("文件"))
                                    {
                                        msg.MessageState = MessageStates.Success;
                                    }
                                    break;
                                case MessageType.usercard:
                                    MessagePackage packageMSG = Util.Helpers.Json.ToObject<SDKClient.Model.MessagePackage>(m.Source);
                                    PersonCardModel pcm = new PersonCardModel()
                                    {
                                        Name = packageMSG.data.body.name,
                                        PhotoImg = packageMSG.data.body.photo,
                                        PhoneNumber = packageMSG.data.body.phone,
                                        UserId = packageMSG.data.body.userId
                                    };
                                    string imgPath = packageMSG.data.body.photo;
                                    var imageFullPath = IMClient.Helper.ImageHelper.GetFriendFace(imgPath, (a) =>
                                    {
                                        msg.PersonCardModel.PhotoImg = a;
                                    });
                                    pcm.PhotoImg = imageFullPath;
                                    msg.ContentMD5 = packageMSG.data.body.photo;
                                    msg.Content = "[个人名片]";
                                    msg.TipMessage = "[个人名片]" + pcm.Name;
                                    msg.PersonCardModel = pcm;
                                    break;
                                default:
                                    msg.TipMessage = msg.Content = m.content;
                                    break;
                            }
                            if (string.IsNullOrEmpty(msg.TipMessage.Trim()) && string.IsNullOrEmpty(msg.Content.Trim()))
                                continue;
                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                var tempMessage = (this.Model as ChatModel).Messages.ToList().FirstOrDefault(x => x.MsgKey == m.msgId);
                                if (tempMessage == null)//重复消息
                                {
                                    if (isForward)
                                    {
                                        SetMsgShowTime(msg, false);
                                        if (msg.Sender != null || msg.MsgType == MessageType.notification)
                                        {
                                            var tempUnReadMessages = this.UnReadMsgs.ToList();
                                            var tempUnReadMessage = tempUnReadMessages.FirstOrDefault(n => n.MsgKey == msg.MsgKey);

                                            if (tempUnReadMessage != null)
                                            {
                                                tempUnReadMessage.ShowSendTime = msg.ShowSendTime;
                                                (this.Model as ChatModel).Messages.Insert(0, tempUnReadMessage);
                                            }
                                            else
                                            {
                                                (this.Model as ChatModel).Messages.Insert(0, msg);
                                            }

                                            if (msg.MsgType == MessageType.notification && (msg.Content == ConstString.BecomeFriendsTip || msg.Content == "已经是好朋友，开始聊天吧"))
                                            {
                                                var tempStrangerMsg = datas.Where(sm => sm.msgTime < msg.SendTime).ToList();
                                                if (tempStrangerMsg.Count > 0 && !string.IsNullOrEmpty(m.Source) && m.Source == AppData.Current.LoginUser.ID.ToString())
                                                    AddMessageTipEx("以上为打招呼内容", msg.SendTime, msg.MsgKey);
                                                else if (tempStrangerMsg.Count == 0)
                                                {
                                                    msg.ShowSendTime = true;
                                                }
                                                //AddMessageTipEx("已经成为好友，开始聊天吧", msg.SendTime, msg.MsgKey);

                                            }

                                            this.AppendMessage?.Invoke(top);
                                        }
                                    }
                                    else
                                    {
                                        if (m == datas.Last())
                                        {
                                            this.SetLastMsg(msg);
                                            AppData.MainMV.ChatListVM.ResetSort();
                                        }

                                        SetMsgShowTime(msg);
                                        if (msg.Sender != null || msg.MsgType == MessageType.notification)
                                        {
                                            var tempUnReadMessages = this.UnReadMsgs.ToList();
                                            var tempUnReadMessage = tempUnReadMessages.FirstOrDefault(n => n.MsgKey == msg.MsgKey);
                                            if (tempUnReadMessage != null)
                                            {
                                                tempUnReadMessage.ShowSendTime = msg.ShowSendTime;
                                                (this.Model as ChatModel).Messages.Add(tempUnReadMessage);
                                            }
                                            else
                                            {
                                                if (msg.MsgType == MessageType.notification && msg.Content.Contains("您不在对方通讯录内"))
                                                {
                                                    string actionableContent = null;
                                                    actionableContent = "有事找TA";
                                                    AddMessageTip("您不在对方通讯录内，", actionableContent: actionableContent);
                                                }
                                                else
                                                {
                                                    (this.Model as ChatModel).Messages.Add(msg);
                                                }
                                            }
                                            //if (!string.IsNullOrEmpty(m.Source) && m.Source == AppData.Current.LoginUser.ID.ToString())
                                            //{

                                            if (msg.MsgType == MessageType.notification && (msg.Content == ConstString.BecomeFriendsTip || msg.Content == "已经是好朋友，开始聊天吧"))
                                            {
                                                var tempStrangerMsg = datas.Where(sm => sm.msgTime < msg.SendTime).ToList();
                                                if (tempStrangerMsg.Count > 0 && !string.IsNullOrEmpty(m.Source) && m.Source == AppData.Current.LoginUser.ID.ToString())
                                                    AddMessageTipEx("以上为打招呼内容", msg.SendTime, msg.MsgKey);
                                                else if (tempStrangerMsg.Count == 0)
                                                {
                                                    msg.ShowSendTime = true;
                                                }
                                                //AddMessageTipEx("已经成为好友，开始聊天吧", msg.SendTime, msg.MsgKey);
                                                //AddMessageTip("以上为打招呼内容", msg.SendTime, false, "", "", msg.Sender as UserModel);
                                                //AddMessageTip("已经成为好友，开始聊天吧", msg.SendTime, false, "", "", msg.Sender as UserModel);
                                            }
                                            //}
                                            this.AppendMessage?.Invoke(null);
                                            this.OnDisplayAtButton?.Invoke();
                                        }
                                    }
                                }
                                //AddNewMessageFlag(queryMsgCount, sendDatetime);
                            }));
                        }
                    }
                    if (datas?.Count == 0)
                    {
                        var tempMsgLst = UnReadMsgs.OrderByDescending(m => m.SendTime).OrderByDescending(m => m.ID).Take(MAXCOUNT).ToList();
                        foreach (var tempMsg in tempMsgLst)
                        {
                            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                            {
                                SetMsgShowTime(tempMsg, false);
                                if (tempMsg.Sender != null)
                                {
                                    (this.Model as ChatModel).Messages.Insert(0, tempMsg);
                                    this.AppendMessage?.Invoke(null);
                                }
                                //AddNewMessageFlag(queryMsgCount, sendDatetime);
                            }));
                        }
                    }
                    UnReadMsgs.Clear();
                }
                else
                {
                    var tempFirstMessages = FirstMessages.Take(MAXCOUNT).ToList();
                    System.Diagnostics.Stopwatch stopwatch1 = new System.Diagnostics.Stopwatch();
                    stopwatch1.Start();
                    if (UnReadMsgs.Count == 0)
                    {
                        foreach (var msg in tempFirstMessages)
                        {
                            var msgEntity = new MessageModel();
                            ChatMsgFomat(msg, ref msgEntity, true);
                            if (msgEntity.TipMessage != null && msgEntity.Content != null)
                            {
                                if (string.IsNullOrEmpty(msgEntity.TipMessage.Trim()) && string.IsNullOrEmpty(msgEntity.Content.Trim()))
                                    continue;
                            }
                            App.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                            {
                                SetMsgShowTime(msgEntity, false);
                                if (msgEntity.Sender != null)
                                {
                                    var tempMsgs = (this.Model as ChatModel).Messages.ToList();
                                    if (!tempMsgs.Any(m => m.MsgKey == msg.MsgId))
                                        (this.Model as ChatModel).Messages.Insert(0, msgEntity);
                                    //if (!string.IsNullOrEmpty(msg.Source) && msg.Source == AppData.Current.LoginUser.ID.ToString())
                                    //{
                                    if (msgEntity.MsgType == MessageType.notification && (msgEntity.Content == ConstString.BecomeFriendsTip || msgEntity.Content == "已经是好朋友，开始聊天吧"))
                                    {
                                        var tempStrangerMsg = tempFirstMessages.Where(sm => sm.MsgTime < msgEntity.SendTime).ToList();
                                        if (tempStrangerMsg.Count > 0 && !string.IsNullOrEmpty(msg.Source) && msg.Source == AppData.Current.LoginUser.ID.ToString())
                                            AddMessageTipEx("以上为打招呼内容", msgEntity.SendTime, msgEntity.MsgKey);
                                        else if (tempStrangerMsg.Count == 0)
                                        {
                                            msgEntity.ShowSendTime = true;
                                        }

                                        //AddMessageTipEx("已经成为好友，开始聊天吧", msgEntity.SendTime, msgEntity.MsgKey);
                                    }
                                    //}
                                    this.AppendMessage?.Invoke(null);
                                    if (this.UnReadCount > 0)
                                        UnReadCount = 0;
                                    if (!isForward)
                                    {
                                        this.OnDisplayAtButton?.Invoke();
                                    }
                                }
                            }));
                        }
                    }
                    else
                    {
                        var tempMsgList = UnReadMsgs.ToList();
                        var tempRetracts = tempMsgList.Where(m => !string.IsNullOrEmpty(m.RetractId)).ToList();
                        if (tempRetracts?.Count > 0)
                        {
                            foreach (var retractMsg in tempRetracts)
                            {
                                var tempMsg = tempFirstMessages.FirstOrDefault(m => m.MsgId == retractMsg.RetractId);
                                if (tempMsg != null)
                                {
                                    retractMsg.SendTime = tempMsg.MsgTime;
                                    tempFirstMessages.Remove(tempMsg);
                                }
                            }
                        }
                        if (tempMsgList.Count < tempFirstMessages.Count)
                        {
                            foreach (var msg in tempFirstMessages)
                            {
                                var tempMsg = tempMsgList.FirstOrDefault(m => m.MsgKey == msg.MsgId);
                                if (tempMsg == null)
                                {
                                    var msgEntity = new MessageModel();
                                    ChatMsgFomat(msg, ref msgEntity, true);
                                    if (msgEntity.TipMessage != null && msgEntity.Content != null)
                                    {
                                        if (string.IsNullOrEmpty(msgEntity.TipMessage.Trim()) && string.IsNullOrEmpty(msgEntity.Content.Trim()))
                                            continue;
                                    }
                                    tempMsgList.Add(msgEntity);
                                }
                            }
                        }
                        var tempMsgLst = tempMsgList.OrderByDescending(m => m.SendTime).ToList();
                        if (tempMsgList.Count > MAXCOUNT)
                        {
                            tempMsgLst = tempMsgLst.Take(MAXCOUNT).ToList();
                        }
                        var tempCount = 0;
                        foreach (var msg in tempMsgLst)
                        {
                            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                            {
                                SetMsgShowTime(msg, false);
                                if (msg.Sender != null)
                                {
                                    var tempMsgs = (this.Model as ChatModel).Messages.ToList();
                                    var isNewMessage = tempMsgs.FirstOrDefault(x => x.MsgType == MessageType.notification && x.Content == ConstString.FollowingIsNewMessage);
                                    if (!tempMsgs.Any(m => m.MsgKey == msg.MsgKey))
                                    {
                                        //if (!string.IsNullOrEmpty(msg.Content))
                                        //{
                                        if (isNewMessage != null && tempCount < queryMsgCount)
                                        {
                                            (this.Model as ChatModel).Messages.Insert(1, msg);
                                        }
                                        else
                                        {
                                            (this.Model as ChatModel).Messages.Insert(0, msg);
                                        }
                                        //}
                                        if (msg.MsgType == MessageType.notification && (msg.Content == ConstString.BecomeFriendsTip || msg.Content == "已经是好朋友，开始聊天吧"))
                                        {
                                            var tempStrangerMsg = tempMsgLst.Where(sm => sm.SendTime < msg.SendTime).ToList();
                                            if (tempStrangerMsg.Count > 0)
                                                AddMessageTipEx("以上为打招呼内容", msg.SendTime, msg.MsgKey);
                                            else if (tempStrangerMsg.Count == 0)
                                            {
                                                msg.ShowSendTime = true;
                                            }
                                            //AddMessageTipEx("已经成为好友，开始聊天吧", msg.SendTime, msg.MsgKey);
                                        }
                                    }
                                    //if (!string.IsNullOrEmpty(msg.) && msg.Source == AppData.Current.LoginUser.ID.ToString())
                                    //{

                                    //}
                                    this.AppendMessage?.Invoke(null);
                                    if (this.UnReadCount > 0)
                                        UnReadCount = 0;
                                    if (!isForward)
                                    {
                                        this.OnDisplayAtButton?.Invoke();
                                    }
                                }
                                tempCount++;
                            }));

                        }

                        UnReadMsgs.Clear();
                    }
                    FirstMessages.Clear();
                    stopwatch1.Stop();
                    System.Diagnostics.Debug.WriteLine("所有消息展示耗时：" + stopwatch1.ElapsedMilliseconds);
                }
            }).ContinueWith(t =>
            {
                IsReading = false;
            });
        }

        private void TryGetGroupInfo(GroupViewModel groupVM)
        {
            if (groupVM == null)
            {
                return;
            }

            if (groupVM.ShowMembers == null || groupVM.ShowMembers.Count == 0)
            {
                groupVM.GetGroupMemberList();
            }
            if (groupVM.Model is GroupModel group && string.IsNullOrEmpty(group.DisplayName))
            {
                SDKClient.SDKClient.Instance.GetGroup(this.ID);
            }
        }
        /// <summary>
        /// 添加设置成员权限的通知
        /// </summary>
        /// <param name="package"></param>
        private void ShowAddMemberPowerMsg(SetMemberPowerPackage package, bool isForward = false, bool isNewMessage = true)
        {
            string info = string.Empty;
            int index = 0;
            foreach (var item in package.data.userIds)
            {
                UserModel user = AppData.Current.GetUserModel(item);
                var group = this.Chat.Chat as GroupModel;
                var sender = user.GetInGroupMember(group);

                if (string.IsNullOrEmpty(user.Name))
                {
                    SDKClient.SDKClient.Instance.GetUser(item);
                }

                if (package.data.type == "admin")
                {
                    if (item == AppData.MainMV.LoginUser.ID)
                    {
                        info = "你成为群管理员";
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(sender.DisplayName))
                        {
                            info = $"[{package.data.userNames[index]}] 成为群管理员";
                        }
                        else
                        {
                            info = string.Format("[{0}] 成为群管理员", sender.DisplayName);
                        }
                    }
                }
                else
                {
                    if (item == AppData.MainMV.LoginUser.ID)
                    {
                        info = "你被取消群管理员";
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            var groupVM = AppData.MainMV.GroupListVM.Items.ToList().FirstOrDefault(i => i.ID == package.data.groupId);
                            if (groupVM != null)
                            {
                                groupVM.ApplyUsers.Clear();
                                groupVM.IsJoinGroupApply = false;
                            }
                        });
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(sender.DisplayName))
                        {
                            info = $"[{package.data.userNames[index]}] 被取消群管理员";
                        }
                        else
                        {
                            info = string.Format("[{0}] 被取消群管理员", sender.DisplayName);
                        }
                    }
                }
                index++;
                this.AddMessageTip(info, package.time, isForward, package.id);
            }
        }

        private void ShowDismissMsg(DismissGroupPackage package, bool isForward = false)
        {
            //群成员收到解散群的包时 
            this.AddMessageTip("该群已经被解散！", package.time, isForward, package.id);
        }

        public void LoadHisMessage(SDKClient.DB.messageDB m, int fromId, bool isForward = false)
        {
            if (fromId > 0)
            {
                #region 消息类型
                MessageType type;
                try
                {
                    type = (MessageType)Enum.Parse(typeof(MessageType), m.msgType.ToLower());
                }
                catch
                {
                    //Views.MessageBox.ShowBox("收到未知消息类型：" + m.msgType);
                    return;
                }
                #endregion

                IChat sender;

                object target = null;
                bool isMine = AppData.Current.LoginUser.User.ID == fromId;
                if (isMine) //我发送的消息，统一放在右边
                {
                    sender = AppData.Current.LoginUser.User;
                }
                else if (this.IsGroup) //群成员发送的消息，要现实的是成员在  我给好友备注的昵称>群成员自己设置的昵称>群成员名称
                {
                    UserModel user = AppData.Current.GetUserModel(fromId);
                    GroupModel group = AppData.Current.GetGroupModel(m.roomId);
                    sender = user.GetInGroupMember(group);// AppData.Current.GetUserModel(fromId);
                }
                else //单聊好友发送的消息,只显示头像，不显示昵称（聊天框已有昵称显示）
                {
                    sender = AppData.Current.GetUserModel(fromId);
                }
                if (type == MessageType.invitejoingroup)
                {
                    target = LoadGroupCard(m.Source, fromId);
                    if (target == null)
                    {
                        return;
                    }
                }
                var msgId = m.msgId;
                if (!isForward)
                {
                    if (type == MessageType.invitejoingroup)
                    {
                        var package = Util.Helpers.Json.ToObject<SDKClient.Model.InviteJoinGroupPackage>(m.Source);
                        if (package.data.targetGroupId == 0)
                        {
                            var userId = isForward ? package.to : package.from;
                            msgId = package.id + package.to + "single";
                        }
                        else
                        {
                            msgId = package.id + package.data.targetGroupId + "group";
                        }
                    }
                }
                MessageModel msg = new MessageModel()
                {
                    MsgKey = msgId,
                    Sender = sender,
                    SendTime = m.msgTime,
                    IsMine = isMine,
                    Content = m.content,
                    MsgType = type,
                    Target = target,
                };

                string info = string.Empty;
                switch (type)
                {
                    case MessageType.img:
                        info = "[图片]";
                        break;
                    case MessageType.file:
                        info = "[文件]";

                        FileResourceModel file = new FileResourceModel()
                        {
                            Key = m.resourceId,
                            SmallKey = m.resourcesmallId,
                            Length = m.fileSize,
                            FileName = m.fileName,
                            //如果文件名包含绝对路径，那么不从manjinba/file路径中文件
                            FullName = m.fileName,
                            //fileState 0：未开始，1：下载中，2：已完成,3:取消，4：异常
                            DBState = m.fileState,
                        };
                        msg.ResourceModel = file;
                        break;
                    case MessageType.invitejoingroup:
                        msg.MsgSource = Util.Helpers.Json.ToObject<SDKClient.Model.InviteJoinGroupPackage>(m.Source);
                        info = "[群名片]";
                        break;
                    case MessageType.audio:
                        info = "[语音]";
                        msg.Content = msg.IsMine ? "您发送了一条语音消息，请在手机端查看" : "对方发送语音消息，请在手机端查看";
                        break;
                    case MessageType.video:
                        info = "[小视频]";
                        msg.Content = msg.IsMine ? "您发送了一条视频消息，请在手机端查看" : "对方发送视频消息，请在手机端查看";
                        break;
                    case MessageType.redenvelopesreceive:
                        info = "[红包消息]";
                        msg.Content = msg.IsMine ? "您领取了一个红包，请在手机端查看" : "[有人领取了您的红包，请在手机端查看]";
                        break;
                    case MessageType.redenvelopessendout:
                        info = "[红包消息]";
                        msg.Content = msg.IsMine ? "[您发送了一条红包消息，请在手机端查看]" : "[您有新红包，请在手机上查看]";
                        break;
                    case MessageType.addgroupnotice:
                        info = "[群公告]";
                        break;
                    default:
                        info = msg.Content;
                        break;
                }


                msg.TipMessage = info;
                if ((this.Model as ChatModel).Messages != null && AppData.MainMV.ChatListVM.SelectedItem == this)
                {
                    this.SetLastMsg(msg);

                    this.SetOneMsgRead(m.msgId);

                    SetMsgShowTime(msg);
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        //var tempMessages = (this.Model as ChatModel).Messages.ToList();
                        //var tempMessage = tempMessages.FirstOrDefault(model => model.MsgKey == m.msgId);
                        //if (tempMessage != null) return;
                        (this.Model as ChatModel).Messages.Add(msg);
                        this.OnDisplayMsgHint?.Invoke();
                        //NewMessage.Invoke(this.LastMsg);

                        if (GotViewIsFocus != null && !GotViewIsFocus.Invoke())
                        {
                            this.UnReadCount++;
                        }
                    }));
                }
                else
                {
                    this.SetLastMsg(msg, false);
                    //var tempUnReadMsgs = UnReadMsgs.ToList();
                    //var tempMessage = tempUnReadMsgs.FirstOrDefault(model => model.MsgKey == m.msgId);
                    //if (tempMessage != null) return;
                    if (!isMine)
                        msg.IsRead = 0;
                    UnReadMsgs.Add(msg);
                    //this.UnReadCount = _unReadMsgs.Count;
                    this.UnReadCount++;
                }

                if ((this.Model as ChatModel).Chat is GroupModel groupModel && groupModel.IsTopMost)
                {
                    return;
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    AppData.MainMV.ChatListVM.ResetSort();
                });
            }
        }

        private GroupModel LoadGroupCard(string srouceData, int fromID)
        {
            try
            {
                var p = Util.Helpers.Json.ToObject<SDKClient.Model.InviteJoinGroupPackage>(srouceData);
                GroupModel model = AppData.Current.GetGroupModel(p.data.groupId);
                model.DisplayName = p.data.groupName;
                model.GroupRemark = p.data.groupIntroduction;
                model.HeadImg = IMClient.Helper.ImageHelper.GetFriendFace(p.data.groupPhoto, (s) => model.HeadImg = s);
                model.HeadImgMD5 = p.data.groupPhoto;
                model.AppendID = fromID;
                return model;
            }
            catch
            {
                return null;
            }
        }

        public async void SetOneMsgRead(string msgID, bool sx = true)
        {
            if (sx)
            {
                await SDKClient.SDKClient.Instance.SendSyncMsgStatus(this.ID, 1, msgID,
                  this.IsGroup ? SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat);
            }

            SDKClient.SDKClient.Instance.UpdateHistoryMsgIsReaded(msgID);
        }

        /// <summary>
        /// 新消息
        /// </summary>
        /// <param name="package"></param>
        /// <param name="fromID"></param>
        public void ReceiveNewMessage(SDKClient.Model.MessagePackage package, int fromID, bool isStrangerMsg = false)
        {

            var tempMessages = (this.Model as ChatModel).Messages.ToList();
            if (tempMessages.FirstOrDefault(m => m.MsgKey == package.id) != null)//重复消息
            {
                return;
            }
            ChatViewModel chatViewModel = AppData.MainMV.ChatListVM.Items.ToList().FirstOrDefault(x => x.ID == this.ID);
            if (chatViewModel != null)
            {
                if (chatViewModel.IsGroup)
                    chatViewModel.IsShowGroupNoticeBtn = true;
            }

            //信息类型            
            MessageType type;
            try
            {

                type = Util.Helpers.Enum.Parse<MessageType>(package.data.subType.ToLower());
            }
            catch
            {
                return;
            }

            IChat sender;

            if (package.data.groupInfo != null)
            {
                GroupModel group = AppData.Current.GetGroupModel(package.data.groupInfo.groupId);
                UserModel user = AppData.Current.GetUserModel(fromID);
                var groupVM = AppData.MainMV.GroupListVM.Items.ToList().FirstOrDefault(g => g.ID == group.ID);
                //ThreadPool.QueueUserWorkItem(o =>
                //{
                //    if (group.Members != null && !group.Members.Any(m => m.ID == fromID)) //若发送人不在群中（断线过程中加入的人）
                //    {
                //        groupVM?.GetGroupMemberList(true);
                //    }
                //    TryGetGroupInfo(groupVM);
                //});
                sender = user.GetInGroupMember(group);

                if (string.IsNullOrEmpty(sender.DisplayName))
                {
                    sender.DisplayName = package.data.senderInfo.userName;
                    //SDKClient.SDKClient.Instance.GetGroupMember(AppData.Current.LoginUser.ID, group.ID, fromID);
                }
            }
            else
            {
                //if (!chatViewModel.IsFileAssistant)
                //{
                sender = AppData.Current.GetUserModel(fromID);
                if (string.IsNullOrEmpty(sender.DisplayName))
                {
                    ThreadPool.QueueUserWorkItem(m =>
                    {
                        SDKClient.SDKClient.Instance.GetUser(fromID);
                    });
                }
                if (type == MessageType.invitejoingroup)
                {
                    object target = LoadGroupCard(package.data.body, fromID);
                    if (target == null)
                    {
                        return;
                    }
                }
                //}
                //else
                //{
                //    sender = chatViewModel.Chat.Chat;
                //}
            }

            MessageModel msg = new MessageModel()
            {
                Sender = sender,
                SendTime = package.time ?? DateTime.Now,
                IsMine = fromID == AppData.Current.LoginUser.User.ID,
                MsgType = type,
                Content = package.data.body.text,
                IsSync = package.syncMsg == 1 ? true : false,
                MsgKey = package.id,
                MsgSource = package.data.body,
            };

            if (type != MessageType.retract && package.data.tokenIds != null && package.data.tokenIds.Count > 0 && package.syncMsg != 1)
            {
                foreach (var item in package.data.tokenIds)
                {
                    if (item == ConstString.AtAllId || item == AppData.Current.LoginUser.ID)
                    {
                        if ((this.Model as ChatModel).Messages != null && AppData.MainMV.ChatListVM.SelectedItem == this)
                        {
                            this.HasAtMsg = false;
                        }
                        else
                        {
                            this.HasAtMsg = true;
                        }
                        msg.IsAtMeMsg = true;
                        this.AtMeDic.TryAdd(package.id, msg);
                        break;
                    }
                }
            }

            if (type == MessageType.redenvelopesreceive || (type == MessageType.redenvelopessendout && msg.IsMine))////不显示对方接收的红包和自己发送的红包消息
                return;
            var body = package.data.body;
            string info = string.Empty;
            switch (type)
            {
                case MessageType.foreigndyn:
                    string dynContent = package.data.body.text;
                    string msgHyperlink = package.data.body.url;
                    string msgstr = package.data.body.img;
                    if (string.IsNullOrEmpty(msgstr))
                    {
                        msg.ShareMsgImage = IMAssets.ImageDeal.NewsDefaultIcon;
                    }
                    else
                    {
                        if (FileHelper.IsUrlRegex(msgstr))
                            msg.ShareMsgImage = msgstr;
                        else
                        {
                            msg.ResourceModel = new FileResourceModel { SmallKey = msgstr };
                            msg.ShareMsgImage = IMAssets.ImageDeal.NewsDefaultIcon;
                        }
                    }
                    msg.MsgHyperlink = msgHyperlink;
                    msg.MsgSource = package.data.body;
                    msg.Content = string.IsNullOrEmpty(dynContent) ? msgHyperlink : dynContent;
                    //msg.ShareMsgImage=
                    info = "[链接]" + dynContent;
                    //链接消息，请在手机端查看";
                    break;
                case MessageType.img:
                    msg.ResourceModel.Key = body.id;
                    msg.ResourceModel.SmallKey = body.smallId;
                    info = "[图片]";
                    break;
                case MessageType.onlinefile:

                    string onlineName = Path.GetFileName($"{package.data.body.fileName}");
                    string onlinePath = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.filePath, onlineName);
                    onlinePath = FileHelper.GetFileName(onlinePath, 1);
                    onlineName = Path.GetFileName(onlinePath);

                    FileResourceModel onlineFile = new FileResourceModel()
                    {
                        Key = body.id,
                        Length = body.fileSize,
                        FileName = onlineName,
                        //如果文件名包含绝对路径，那么不从manjinba/file路径中文件
                        FullName = onlinePath,
                        RefInfo = new SDKClient.Model.OnlineFileBody()
                        {
                            id = body.id,
                            fileSize = body.fileSize,
                            fileName = onlineName,
                            Port = body.Port,
                            IP = body.IP,
                        }
                    };
                    msg.ResourceModel = onlineFile;
                    msg.MsgType = MessageType.file;
                    info = "[文件]";
                    break;
                case MessageType.file:
                    string fileName = Path.GetFileName($"{package.data.body.fileName}");
                    string filePath = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.filePath, fileName);
                    filePath = FileHelper.GetFileName(filePath, 1);
                    fileName = filePath.Split('\\').LastOrDefault();

                    FileResourceModel file = new FileResourceModel()
                    {
                        Key = body.id,
                        SmallKey = body.resourcesmallId,
                        Length = body.fileSize,
                        FileName = fileName,
                        //如果文件名包含绝对路径，那么不从manjinba/file路径中文件
                        FullName = filePath,
                        PreviewKey = body.img,
                    };
                    msg.ResourceModel = file;
                    info = "[文件]";
                    break;
                case MessageType.invitejoingroup:
                    msg.MsgSource = package;
                    info = "[群名片]";
                    break;
                case MessageType.audio:
                    info = "[语音]";
                    msg.Content = msg.IsMine ? "您发送了一条语音消息，请在手机端查看" : "对方发送语音消息，请在手机端查看";
                    break;
                case MessageType.smallvideo:
                case MessageType.video:
                    info = "[小视频]";
                    string videoName = Path.GetFileName($"{package.data.body.fileName}");
                    if (string.IsNullOrEmpty(videoName))
                    {
                        videoName = package.data.body.id;
                    }

                    string videoPath = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.filePath, videoName);
                    if (package.syncMsg == 1)
                    {
                        videoPath = FileHelper.GetFileName(videoPath, 1);
                        videoName = videoPath.Split('\\').LastOrDefault();
                    }

                    FileResourceModel video = new FileResourceModel()
                    {
                        Key = body.id,
                        PreviewKey = body.previewId,
                        Length = body.fileSize,
                        FileName = videoName,
                        FullName = videoPath,
                        RecordTime = body.recordTime,
                    };
                    video.PreviewImagePath = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, video.PreviewKey);
                    msg.ResourceModel = video;
                    msg.Content = video.FullName;
                    break;
                case MessageType.redenvelopesreceive:
                    msg.MsgType = MessageType.notification;
                    info = msg.Content = msg.IsMine ? "您领取了一个红包，请在手机端查看" : "[有人领取了您的红包，请在手机端查看]";
                    break;
                case MessageType.redenvelopessendout:
                    msg.MsgType = MessageType.notification;
                    info = msg.Content = "[您有新红包，请在手机上查看]";
                    break;

                case MessageType.addgroupnotice:
                    GroupNoticeModel groupNotice = new GroupNoticeModel()
                    {
                        NoticeTitle = package.data.body.title,
                        NoticeId = package.data.body.noticeId,
                        GroupNoticeType = package.data.body.type == 0 ? "普通群公告" : "入群须知",
                        GroupNoticeContent = package.data.body.content
                    };
                    msg.Content = package.data.body.content;
                    msg.SendTime = package.data.body.publishTime ?? DateTime.Now;
                    msg.NoticeModel = groupNotice;
                    info = msg.NoticeModel.NoticeTitle;
                    if (this.Chat.Chat is GroupModel gModel)
                    {
                        int groupId = gModel.ID;
                        msg.NoticeModel.GroupMId = groupId;
                        this.HasNewGroupNotice = true;
                    }
                    if (!noticeMessage.ContainsKey(msg.NoticeModel.NoticeId))
                        noticeMessage.Add(msg.NoticeModel.NoticeId, msg);
                    if (noticeMessage.Count > 0)
                    {
                        if (noticeMessage.Values.Any(x => x.NoticeModel.GroupNoticeType.Equals("入群须知")))
                        {
                            MessageModel message1 = noticeMessage.Values.FirstOrDefault(x => x.NoticeModel.GroupNoticeType.Equals("入群须知"));
                            this.currentUnreadGroupNoticeMessage = message1;
                        }
                        else
                        {
                            MessageModel message1 = noticeMessage.Values.OrderByDescending(x => x.SendTime).FirstOrDefault();
                            this.currentUnreadGroupNoticeMessage = message1;
                        }
                    }
                    break;
                case MessageType.deletegroupnotice:
                    int noticeId = package.data.body.noticeId;
                    var dgn = UnReadMsgs.FirstOrDefault(m => m.NoticeModel.NoticeId == noticeId);
                    if (dgn != null)
                        dgn.NoticeModel.IsHasDelete = true;
                    else
                    {
                        var item = this.Chat.Messages.ToList().FirstOrDefault(y => !string.IsNullOrEmpty(y.NoticeModel.NoticeTitle) && y.NoticeModel.NoticeId == noticeId);
                        if (item != null)
                            item.NoticeModel.IsHasDelete = true;
                    }
                    if (noticeMessage.ContainsKey(noticeId))
                        noticeMessage.Remove(noticeId);
                    if (noticeMessage.Count > 0)
                    {
                        foreach (KeyValuePair<int, MessageModel> child in noticeMessage)
                        {
                            if (!OnlineAndOfflineMessage.ContainsKey(child.Key))
                                OnlineAndOfflineMessage.Add(child.Key, child.Value);
                        }
                    }
                    if (offlineNoticeMessage.Count > 0)
                    {
                        foreach (KeyValuePair<int, MessageModel> child in offlineNoticeMessage)
                        {
                            if (!OnlineAndOfflineMessage.ContainsKey(child.Key))
                                OnlineAndOfflineMessage.Add(child.Key, child.Value);
                        }
                    }
                    if (this.UnReadCount > 0 && OnlineAndOfflineMessage.Count > 0)
                        this.HasNewGroupNotice = true;
                    else
                        this.HasNewGroupNotice = false;
                    return;
                case MessageType.usercard:
                    PersonCardModel pcm = new PersonCardModel();
                    pcm.Name = package.data.body.name;
                    string imgPath = package.data.body.photo;
                    var imageFullPath = IMClient.Helper.ImageHelper.GetFriendFace(imgPath, (a) =>
                    {
                        msg.PersonCardModel.PhotoImg = a;
                    });
                    pcm.PhotoImg = imageFullPath;
                    msg.ContentMD5 = package.data.body.photo;
                    //pcm.PhotoImg = package.data.body.photo;
                    pcm.PhoneNumber = package.data.body.phone;
                    pcm.UserId = package.data.body.userId;
                    msg.PersonCardModel = pcm;

                    info = "[个人名片]" + pcm.Name;
                    break;
                case MessageType.bigtxt:
                    var msgID = string.Empty;
                    if (package.data != null && package.data.body != null)
                        msgID = package.data.body.partName;
                    BigBody bb = new BigBody()
                    {
                        partName = package.data.body.partName,
                        partOrder = package.data.body.partOrder,
                        partTotal = package.data.body.partTotal,
                        text = package.data.body.text
                    };
                    if (package.data != null && package.data.body != null)
                        msg.MsgKey = msgID;
                    int index = bb.partOrder;
                    if (index == 0)
                    {
                        BigtxtHelper.AddBigtxtMsg(bb, s =>
                        {
                            //大文本合包完成后，在回调中单独处理后续逻辑
                            var tempMsgs = (this.Model as ChatModel).Messages.ToList();
                            if (tempMsgs.FirstOrDefault(m => m.MsgKey == package.id) != null ||
                            UnReadMsgs.FirstOrDefault(m => m.MsgKey == package.id) != null) //重复消息
                            {
                                return;
                            }

                            msg.Content = s;
                            info = msg.Content;
                            AddMessage(msg, info, package);
                            return;
                        });
                    }
                    else
                    {
                        BigtxtHelper.AddBigtxtMsg(bb, null);
                    }
                    return;
                case MessageType.retract:
                    string retractId = package.data.body.retractId;
                    msg.RetractId = retractId;
                    DateTime dateTime = DateTime.Now;
                    bool isAdd = false;
                    if (isStrangerMsg)
                    {
                        ReceiveWithDrawStrangerMsg(retractId, fromID);
                    }
                    else
                    {
                        var target = Chat.Messages.FirstOrDefault(t => t != null && t.MsgKey != null && t.MsgKey == retractId);
                        if (target == null)
                        {
                            target = this.UnReadMsgs.FirstOrDefault(t => t != null && t.MsgKey != null && t.MsgKey == retractId);
                            if (target == null)
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    if (this.FirstMessages != null && this.FirstMessages.Count > 0)
                                    {
                                        var tempTarget = this.FirstMessages.FirstOrDefault(t => t != null && t.MsgId != null && t.MsgId == retractId);
                                        if (tempTarget != null)
                                        {
                                            isAdd = true;
                                            dateTime = tempTarget.MsgTime;
                                            if (package.data.groupInfo != null)
                                            {
                                                var tempUnReadMsgs = this.FirstMessages != null && this.FirstMessages.Count > 0 ? this.FirstMessages.Where(n => n.TokenIds != null && n.MsgType != nameof(MessageType.retract)
                                                && n.TokenIds.Contains(AppData.Current.LoginUser.ID.ToString())).ToList() : null;
                                                if (tempUnReadMsgs == null || tempUnReadMsgs?.Count == 0)
                                                {
                                                    if (this.AtMeDic != null && this.AtMeDic.Count > 0)
                                                    {
                                                        var tempAtMsg = this.AtMeDic[retractId];
                                                        if (tempAtMsg != null)
                                                            this.AtMeDic.TryRemove(retractId, out tempAtMsg);
                                                    }
                                                    this.IsDisplayAtButton = false;
                                                    this.HasAtMsg = false;
                                                }
                                                //}
                                            }
                                            this.FirstMessages.Remove(tempTarget);
                                        }
                                    }
                                });
                                //if (Chat.LastMsg != null && Chat.LastMsg.MsgKey != null && Chat.LastMsg.MsgKey == retractId)
                                //{

                                //}
                                //else
                                //{
                                //    return;
                                //}
                            }
                            else
                            {
                                dateTime = target.SendTime;
                                isAdd = true;
                                App.Current.Dispatcher.Invoke(() =>
                                       {

                                           this.UnReadMsgs.Remove(target);
                                           if (this.FirstMessages != null && this.FirstMessages.Count > 0)
                                           {
                                               var tempTarget = this.FirstMessages.FirstOrDefault(t => t != null && t.MsgId != null && t.MsgId == retractId);
                                               if (tempTarget != null)
                                               {
                                                   this.FirstMessages.Remove(tempTarget);
                                               }
                                           }
                                           if (package.data.groupInfo != null)
                                           {
                                               var tempUnReadMsgs = this.UnReadMsgs.Where(n => n.IsAtMeMsg).ToList();


                                               var tempFirstMessages = this.FirstMessages != null && this.FirstMessages.Count > 0 ? this.FirstMessages.Where(n => n.TokenIds != null && n.MsgType != nameof(MessageType.retract)
                                               && n.TokenIds.Contains(AppData.Current.LoginUser.ID.ToString())).ToList() : null;
                                               int count = tempUnReadMsgs != null ? tempUnReadMsgs.Count : 0;
                                               count += tempFirstMessages != null ? tempFirstMessages.Count : 0;
                                               if (count == 0)
                                               {
                                                   if (this.AtMeDic != null && this.AtMeDic.Count > 0)
                                                   {
                                                       var tempAtMsg = this.AtMeDic[retractId];
                                                       if (tempAtMsg != null)
                                                           this.AtMeDic.TryRemove(retractId, out tempAtMsg);
                                                   }
                                                   this.IsDisplayAtButton = false;
                                                   this.HasAtMsg = false;
                                               }

                                           }

                                           //});

                                       });
                            }
                        }

                        SDKClient.SDKProperty.RetractType rType = package.data.body.retractType ?? SDKClient.SDKProperty.RetractType.Normal;
                        MessageModel retractMsg = Chat.Messages.ToList().FirstOrDefault(x => x != null && x.MsgKey != null && x.MsgKey.Equals(retractId));
                        string tip = string.Empty;
                        switch (rType)
                        {
                            default:
                            case SDKClient.SDKProperty.RetractType.Normal:
                                tip = "对方撤回了一条消息";
                                break;
                            case SDKClient.SDKProperty.RetractType.OfflineToOnline:
                                tip = "对方已取消离线文件传输，已转为在线发送";
                                break;
                            case SDKClient.SDKProperty.RetractType.TargetEndOnlineRetract:
                                tip = "对方取消了在线文件的接收";
                                if (retractMsg != null)
                                {
                                    var tempFileMsg = Views.Controls.FileChatItem.AcioningItems.FirstOrDefault(m => m.ChatViewModel != null && m._targetModel.MsgKey == retractMsg.MsgKey);
                                    if (tempFileMsg != null)
                                        Views.Controls.FileChatItem.AcioningItems.Remove(tempFileMsg);
                                }
                                    ;
                                break;
                            case SDKClient.SDKProperty.RetractType.OnlineToOffline:
                                tip = "对方已取消在线文件传输，已转为离线发送";
                                break;
                        }
                        if (rType != SDKClient.SDKProperty.RetractType.Normal)
                        {
                            if (target != null)
                            {
                                string size = Helper.FileHelper.FileSizeToString(target.ResourceModel.Length);
                                if (target.ResourceModel != null && target.ResourceModel.RefInfo != null)
                                {
                                    switch (rType)
                                    {
                                        case SDKClient.SDKProperty.RetractType.TargetEndOnlineRetract:
                                            tip = $"对方取消了\"{ target.ResourceModel.FileName}\"({size})的发送，发送失败。";
                                            break;
                                        case SDKClient.SDKProperty.RetractType.OnlineToOffline:
                                            tip = string.Format("对方已取消在线文件\"{0}\"({1})的发送，已转为离线发送。", target.ResourceModel.FileName, size);
                                            break;
                                        case SDKClient.SDKProperty.RetractType.SourceEndOnlineRetract:
                                            tip = string.Format("对方取消了\"{0}\"({1})的发送，文件传输失败。", target.ResourceModel.FileName, size);
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                else
                                {
                                    tip = $"对方取消了\"{ target.ResourceModel.FileName}\"({size})的接收，文件传输失败。";
                                }
                            }
                        }

                        else
                        {
                            if ((this.Model as ChatModel).IsGroup)
                            {
                                tip = "[" + sender.DisplayName + "]" + "撤回了一条消息";
                            }

                            if (package.syncMsg == 1 || msg.Sender.ID == AppData.Current.LoginUser.ID)
                            {
                                tip = "您撤回了一条消息";
                            }
                        }


                        if (retractMsg != null)
                        {
                            bool isUnread = true;
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                if (AppData.MainMV.ChatListVM.SelectedItem == this)
                                {
                                    if (AppData.MainMV.ListViewModel == AppData.MainMV.ChatListVM)
                                    {
                                        if (GotViewIsFocus != null && !GotViewIsFocus.Invoke())
                                        {

                                        }
                                        else
                                        {
                                            isUnread = false;
                                        }
                                    }
                                }
                                else
                                {
                                    isUnread = false;
                                }
                                ReceiveWithDrawMsg(msg, sender.DisplayName, package.syncMsg == 1 ? true : false, isUnread, rType);
                            });


                        }
                        else
                        {
                            retractMsg = this.UnReadMsgs.ToList().FirstOrDefault(x => x != null && x.MsgKey != null && x.MsgKey.Equals(retractId));
                            if (retractMsg != null)
                            {
                                msg.MsgType = MessageType.notification;
                                msg.TipMessage = tip;
                                dateTime = retractMsg.SendTime;
                                if (rType != SDKClient.SDKProperty.RetractType.Normal)
                                {
                                    msg.MessageState = MessageStates.Fail;
                                }
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    this.UnReadMsgs.Remove(retractMsg);
                                    var tempUnReadMsgs = this.UnReadMsgs.ToList();
                                    var tempUnreadMsg = tempUnReadMsgs.FirstOrDefault(m => m.MsgKey == msg.MsgKey);
                                    if (tempUnreadMsg == null)
                                    {
                                        this.UnReadMsgs.Add(msg);

                                    }
                                    AppData.MainMV.ChatListVM.ResetSort();
                                });

                                //ReceiveWithDrawMsg(msg, sender.DisplayName, package.syncMsg == 1 ? true : false, true, rType);
                            }
                            else
                            {

                                MessageModel messageModel = new MessageModel();
                                messageModel.MsgKey = msg.MsgKey;
                                messageModel.MsgType = MessageType.notification;
                                messageModel.SendTime = dateTime;
                                messageModel.RetractId = retractId;
                                messageModel.Sender = sender;
                                if (rType != SDKClient.SDKProperty.RetractType.Normal)
                                {
                                    messageModel.MessageState = MessageStates.Fail;
                                }

                                //App.Current.Dispatcher.Invoke(() =>
                                //{
                                //    if (this.UnReadCount > 0)
                                //    {
                                //        int count = this.UnReadMsgs.Where(m => m.MsgType != MessageType.notification && m.Sender != null && m.Sender.ID != AppData.Current.LoginUser.ID).Count();
                                //        if (!this.HasActived)
                                //        {
                                //            if (this.FirstMessages != null && this.FirstMessages.Count > 0)
                                //            {
                                //                if (count == 0)
                                //                    count = this.FirstMessages.Where(m => m.MsgType != nameof(MessageType.notification) && m.OptionRecord == 0).Count();
                                //                else
                                //                {
                                //                    count += this.FirstMessages.Where(m => m.MsgType != nameof(MessageType.retract) && m.MsgType != nameof(MessageType.notification) && m.OptionRecord == 0).Count();
                                //                }
                                //            }
                                //        }

                                //        this.UnReadCount = count;
                                //    }

                                //    AppData.MainMV.UpdateUnReadMsgCount();
                                AppData.MainMV.ChatListVM.TotalUnReadCount = AppData.MainMV.TotalUnReadCount;
                                if (AppData.MainMV.ChatListVM.TotalUnReadCount == 0)
                                {
                                    AppData.MainMV.ChatListVM.CloseTrayWindow();
                                }
                                else
                                {
                                    AppData.MainMV.ChatListVM.FlashIcon(this);
                                }
                                //});
                                messageModel.TipMessage = messageModel.Content = tip;
                                if (isAdd)
                                {
                                    this.UnReadMsgs.Add(messageModel);
                                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        AppData.MainMV.ChatListVM.ResetSort();
                                    });
                                }
                                this.SetLastMsg(messageModel, false);
                            }
                        }
                        SetOneMsgRead(msg.MsgKey, false);
                    }
                    return;
                default:
                    info = msg.Content;
                    break;
            }

            if (isStrangerMsg)
            {
                AddStrangerMessage(msg, info);
            }
            else
            {
                AddMessage(msg, info, package);
            }
        }

        public void ReceiveWithDrawStrangerMsg(string retractId, int fromId)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                string tip = "对方撤回了一条消息";
                MessageModel messageModel = new MessageModel();
                messageModel.MsgKey = string.Empty;
                messageModel.MsgType = MessageType.notification;
                messageModel.SendTime = DateTime.Now;
                messageModel.Content = messageModel.TipMessage = tip;
                this.SetLastMsg(messageModel, false);
                if (AppData.MainMV.ChatListVM.StrangerMessage.UnReadCount > 0)
                {
                    AppData.MainMV.ChatListVM.StrangerMessage.UnReadCount--;
                }

                var chatVM = this.StrangerMessageList.FirstOrDefault(x => x.ID == fromId);
                if (chatVM != null)
                {
                    MessageModel retractMsg = chatVM.Chat.Messages.ToList().FirstOrDefault(x => x != null && x.MsgKey != null && x.MsgKey.Equals(retractId));
                    if (retractMsg != null)
                    {
                        int index = chatVM.Chat.Messages.IndexOf(retractMsg);
                        if (index >= 0)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                chatVM.Chat.Messages.Remove(retractMsg);
                                chatVM.SetMsgShowTime(messageModel);
                                chatVM.Chat.Messages.Insert(index, messageModel);
                            });
                        }
                    }
                    if (chatVM.UnReadCount > 0)
                    {
                        chatVM.UnReadCount--;
                    }
                }
            });
        }

        public event Func<bool> GotViewIsFocus;

        public bool GetViewIsFocus()
        {
            if (GotViewIsFocus == null)
            {
                return false;
            }

            return !GotViewIsFocus.Invoke();
        }

        public void AddMessage(MessageModel msg)
        {
            this.SetLastMsg(msg);
            SetMsgShowTime(msg);
            App.Current.Dispatcher.Invoke(() =>
            {
                while (Chat.Messages.Count > MAXCOUNT)
                {
                    Chat.Messages.RemoveAt(0);
                    //Chat.Messages.RemoveAt(0);
                    //Chat.Messages.RemoveAt(0);
                }
                if (this.IsViewLoaded && Chat.Messages != null && AppData.MainMV.ChatListVM.SelectedItem == this)
                {
                    var tempMessages = this.Chat.Messages.ToList();
                    var tempNonNullContentMsgList = tempMessages.Where(m => !string.IsNullOrEmpty(m.Content)).ToList();
                    var index = 0;
                    if (tempNonNullContentMsgList != null && tempNonNullContentMsgList.Count > 0)
                        index = tempMessages.Where(m => !string.IsNullOrEmpty(m.Content)).ToList().FindIndex(m => m.Content.Contains("已经成为好友，开始聊天吧"));
                    if (index > 0)
                    {
                        var tempMsg = tempMessages[index];
                        this.Chat.Messages.Remove(tempMsg);
                        this.Chat.Messages.Insert(index, tempMsg);
                    }
                    this.Chat.Messages.Add(msg);
                    if (this.View is ChatView chatview)
                    {
                        chatview.chatBox.UpdateLayout();
                        if (chatview.chatBox.IsVerticalScrollBarAtBottom)
                            chatview.chatBox.ScallToEnd();
                        else
                            chatview.chatBox.ScallToCurrent(msg);
                    }
                }
                else
                {
                    if (UnReadMsgs.ToList().FirstOrDefault(m => m.MsgKey == msg.MsgKey) == null)
                    {
                        UnReadMsgs.Add(msg);
                        UnReadServerMsgID.Add(msg.MsgKey);
                    }
                }

            });
        }

        #region 离线消息处理
        /// <summary>
        /// 消息格式化
        /// </summary>
        /// <param name="message"></param>
        /// <param name="msgModel"></param>
        /// <returns></returns>
        public string ChatMsgFomat(MessageEntity message, ref MessageModel msgModel, bool isFirst = false, bool isOffline = false)
        {
            MessageType type;
            var info = string.Empty;
            try
            {
                if (message.MsgType != null)
                    type = (MessageType)Enum.Parse(typeof(MessageType), message.MsgType.ToLower());
                else
                    return info;
            }
            catch
            {
                return info;
            }
            var fromID = 0;
            int.TryParse(message.From, out fromID);

            IChat sender;

            bool isFailureGroupCard = false;
            object Target = null;
            if (message.RoomType == 0)
            {
                //单聊
                sender = AppData.Current.GetUserModel(fromID);
                if (type == MessageType.invitejoingroup)
                {
                    GroupModel model = null;
                    if (message.Data == null)
                    {
                        model = LoadGroupCard(message.Source, fromID);
                    }
                    else
                    {
                        int tempGroupId = message.Data.groupId;
                        model = AppData.Current.GetGroupModel(tempGroupId);
                        model.DisplayName = message.Data.groupName;
                        model.GroupRemark = message.Data.groupIntroduction;
                        string tempGroupPhoto = message.Data.groupPhoto;
                        model.HeadImg = IMClient.Helper.ImageHelper.GetFriendFace(tempGroupPhoto, (s) => model.HeadImg = s);
                        model.AppendID = fromID;
                    }
                    UserModel user = AppData.Current.GetUserModel(message.RoomId);
                    Target = model;
                    if (user != null && (user.LinkDelType == 2 || user.LinkType >= 2))
                    {
                        isFailureGroupCard = true;
                    }
                    if (Target == null)
                    {
                        return info;
                    }
                }
            }
            else
            {
                //群聊
                GroupModel group = AppData.Current.GetGroupModel(message.RoomId);
                UserModel user = AppData.Current.GetUserModel(fromID);
                sender = user.GetInGroupMember(group);

                if (string.IsNullOrEmpty(sender.DisplayName))
                {
                    sender.DisplayName = message.SenderName;
                    //SDKClient.SDKClient.Instance.GetGroupMember(AppData.Current.LoginUser.ID, group.ID, fromID);
                }
            }

            var msg = new MessageModel()
            {
                Sender = sender,
                SendTime = message.MsgTime,
                IsMine = fromID == AppData.Current.LoginUser.User.ID,
                MsgType = type,
                Content = message.Content,
                IsSync = message.OptionRecord == 1 ? true : false,
                MsgKey = message.MsgId,
                MsgSource = message.Data,
                Target = Target
            };
            if (isFailureGroupCard || (message.OptionRecord & (int)SDKClient.SDKProperty.MessageState.sending) == (int)SDKClient.SDKProperty.MessageState.sending)
            {
                msg.MessageState = MessageStates.Fail;
            }

            if (type == MessageType.file || type == MessageType.onlinefile)
            {
                msg.ResourceModel = new FileResourceModel()
                {
                    Key = message.ResourceId,
                    SmallKey = message.ResourcesmallId,
                    Length = message.FileSize,
                    FileName = Path.GetFileName(message.FileName),
                    //如果文件名包含绝对路径，那么不从manjinba/file路径中文件
                    FullName = message.FileName,
                    //fileState 0：未开始，1：下载中，2：已完成,3:取消，4：异常
                    DBState = message.FileState,
                };
            }

            if (type != MessageType.retract && !string.IsNullOrEmpty(message.TokenIds))
            {
                foreach (var item in message.TokenIds.Split(','))
                {
                    if (item == ConstString.AtAllId.ToString() || item == AppData.Current.LoginUser.ID.ToString())
                    {
                        if ((this.Model as ChatModel).Messages != null && AppData.MainMV.ChatListVM.SelectedItem == this)
                        {
                            this.HasAtMsg = false;
                        }
                        else
                        {
                            this.HasAtMsg = true;
                        }
                        msg.IsAtMeMsg = true;
                        if (message.OptionRecord == 0)
                        {
                            this.AtMeDic.TryAdd(message.MsgId, msg);
                            break;
                        }
                    }
                }
            }

            if (type != MessageType.retract && ((message.OptionRecord & (int)SDKClient.SDKProperty.MessageState.sendfaile) == (int)SDKClient.SDKProperty.MessageState.sendfaile ||
               (message.OptionRecord & (int)SDKClient.SDKProperty.MessageState.cancel) == (int)SDKClient.SDKProperty.MessageState.cancel))
            {
                msg.MessageState = MessageStates.Fail;
            }

            switch (type)
            {
                case MessageType.addfriendaccepted:
                    if (!string.IsNullOrEmpty(message.Source) && message.Source == AppData.Current.LoginUser.ID.ToString())
                    {
                        if (SDKClient.SDKClient.Instance.property.FriendApplyList != null && SDKClient.SDKClient.Instance.property.FriendApplyList.Count > 0)
                        {
                            var friendApply = SDKClient.SDKClient.Instance.property.FriendApplyList.FirstOrDefault(a => a.userId == message.RoomId);
                            if (friendApply != null)
                            {

                                if (string.IsNullOrEmpty(friendApply.applyRemark))
                                {
                                    msg.MsgType = MessageType.notification;
                                    msg.TipMessage = msg.Content = msg.Content;
                                    break;
                                }
                                else
                                {
                                    msg.Sender = AppData.Current.GetUserModel(message.RoomId);
                                    var content = msg.Content;
                                    msg.MsgType = MessageType.notification;
                                    msg.IsMine = false;
                                    //msg.MsgKey = friendApply.msgId;
                                    msg.Content = content;
                                    msg.TipMessage = content;
                                    //msg.Content = "";
                                    break;
                                }
                            }
                        }
                    }
                    msg.MsgType = MessageType.notification;
                    msg.TipMessage = msg.Content;
                    break;
                case MessageType.foreigndyn:
                    string dynContent = message.Data.text;
                    string msgHyperlink = message.Data.url;
                    string imgStr = message.Data.img;
                    if (string.IsNullOrEmpty(imgStr))
                        msg.ShareMsgImage = IMAssets.ImageDeal.NewsDefaultIcon;
                    else
                    {
                        if (FileHelper.IsUrlRegex(imgStr))
                            msg.ShareMsgImage = imgStr;
                        else
                        {
                            msg.ResourceModel = new FileResourceModel { SmallKey = imgStr };
                            msg.ShareMsgImage = IMAssets.ImageDeal.NewsDefaultIcon;
                        }
                    }
                    msg.MsgSource = message.Source;
                    msg.MsgHyperlink = msgHyperlink;

                    msg.Content = string.IsNullOrEmpty(dynContent) ? msgHyperlink : dynContent;
                    //msg.ShareMsgImage=
                    msg.TipMessage = info = "[链接]" + dynContent;
                    //链接消息，请在手机端查看";
                    break;
                case MessageType.img:
                    msg.ResourceModel.Key = message.ResourceId;
                    msg.ResourceModel.SmallKey = message.ResourcesmallId;
                    msg.TipMessage = info = "[图片]";
                    msg.Content = message.FileName;
                    break;
                case MessageType.onlinefile:
                    if (isFirst)
                    {
                        if (File.Exists(message.FileName))
                        {

                            msg.Content = message.FileName;
                            //msg.MsgType = MessageType.file;
                            msg.ResourceModel.FullName = msg.Content;
                        }
                        else
                        {
                            //var package1 = Util.Helpers.Json.ToObject<SDKClient.Model.MessagePackage>(m.Source);
                            //var body = package1.data.body;


                            string onlinePath = message.FileName;
                            string onlineName = Path.GetFileName(onlinePath);

                            FileResourceModel onlineFile = new FileResourceModel()
                            {
                                Key = message.MsgId,
                                Length = message.FileSize,
                                FileName = onlineName,
                                //如果文件名包含绝对路径，那么不从manjinba/file路径中文件
                                FullName = onlinePath,
                                RefInfo = new SDKClient.Model.OnlineFileBody()
                                {
                                    id = message.MsgId,
                                    fileSize = message.FileSize,
                                    fileName = onlineName,
                                    Port = message.Data.Port,
                                    IP = message.Data.IP,
                                },
                                DBState = message.FileState,
                            };
                            msg.ResourceModel = onlineFile;
                            msg.Content = onlinePath;
                            //msg.MsgType = MessageType.file;
                        }
                        //msg.TipMessage = "[文件]";
                    }
                    msg.MsgType = MessageType.file;
                    msg.TipMessage = info = "[文件]";
                    break;
                case MessageType.file:
                    info = "[文件]";
                    msg.TipMessage = "[文件]";
                    msg.Content = message.FileName;
                    break;
                case MessageType.invitejoingroup:

                    msg.MsgSource = Util.Helpers.Json.ToObject<SDKClient.Model.InviteJoinGroupPackage>(message.Source);
                    var targetgroup = LoadGroupCard(message.Source, fromID);
                    msg.Target = targetgroup;
                    info = "[群名片]";
                    msg.TipMessage = "[群名片]";
                    break;
                case MessageType.audio:
                    msg.TipMessage = info = "[语音]";
                    msg.Content = msg.IsMine ? "您发送了一条语音消息，请在手机端查看" : "对方发送语音消息，请在手机端查看";
                    break;
                case MessageType.smallvideo:
                case MessageType.video:
                    msg.TipMessage = info = "[小视频]";
                    var videoPackage = Util.Helpers.Json.ToObject<SDKClient.Model.MessagePackage>(message.Source);
                    var videoBody = videoPackage.data.body;
                    string videoPath = message.FileName;
                    string videoName = Path.GetFileName(videoPath);

                    FileResourceModel video = new FileResourceModel()
                    {
                        Key = videoBody.id,
                        PreviewKey = videoBody.previewId,
                        Length = videoBody.fileSize,
                        FileName = videoName,
                        FullName = videoPath,
                        RecordTime = videoBody.recordTime,
                        DBState = message.FileState
                    };
                    video.PreviewImagePath = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, video.PreviewKey);
                    msg.ResourceModel = video;
                    msg.Content = video.FullName;
                    break;
                case MessageType.redenvelopesreceive:
                    msg.MsgType = MessageType.notification;
                    msg.TipMessage = info = msg.Content = msg.IsMine ? "您领取了一个红包，请在手机端查看" : "[有人领取了您的红包，请在手机端查看]";
                    break;
                case MessageType.redenvelopessendout:
                    msg.MsgType = MessageType.notification;
                    msg.TipMessage = info = msg.Content = "[您有新红包，请在手机上查看]";
                    break;
                case MessageType.addgroupnotice:
                    info = message.Data?.title;
                    RecieveGroupNotice(msg, message);
                    break;
                case MessageType.deletegroupnotice:
                    int noticeId = message.Data?.noticeId;
                    GroupNoticeModel gmTc = new GroupNoticeModel()
                    {
                        IsHasDelete = true,
                        NoticeTitle = message.Data?.title,
                        NoticeId = message.Data?.noticeId,
                        GroupNoticeContent = "已删除"
                    };

                    msg.NoticeModel = gmTc;
                    msg.Content = gmTc.GroupNoticeContent;
                    if (offlineNoticeMessage.ContainsKey(noticeId))
                        offlineNoticeMessage.Remove(noticeId);
                    if (offlineNoticeMessage.Count > 0)
                    {
                        foreach (KeyValuePair<int, MessageModel> child in offlineNoticeMessage)
                        {
                            try
                            {
                                if (!OnlineAndOfflineMessage.ContainsKey(child.Key))
                                    OnlineAndOfflineMessage.Add(child.Key, child.Value);
                            }
                            catch
                            { }
                        }
                    }
                    if (noticeMessage.Count > 0)
                    {
                        foreach (KeyValuePair<int, MessageModel> child in noticeMessage)
                        {
                            try
                            {
                                if (!OnlineAndOfflineMessage.ContainsKey(child.Key))
                                    OnlineAndOfflineMessage.Add(child.Key, child.Value);
                            }
                            catch { }
                        }
                    }
                    this.HasNewGroupNotice = OnlineAndOfflineMessage.Count > 0 ? true : false;
                    this.SetLastMsg(msg, false);
                    msg.TipMessage = message.Data?.title;
                    return "";
                case MessageType.bigtxt:
                    var msgID = string.Empty;
                    if (message.Data != null)
                        msgID = message.Data.partName;
                    BigBody bb = new BigBody()
                    {
                        partName = message.Data?.partName,
                        partOrder = message.Data?.partOrder,
                        partTotal = message.Data?.partTotal,
                        text = message.Data?.text
                    };
                    if (message.Data != null)
                        msg.MsgKey = msgID;
                    int index = bb.partOrder;
                    if (index == 0)
                    {
                        BigtxtHelper.AddBigtxtMsg(bb, s =>
                        {
                            //大文本合包完成后，在回调中单独处理后续逻辑
                            var tempMessages = (this.Model as ChatModel).Messages.ToList();
                            if (tempMessages.FirstOrDefault(m => m.MsgKey == message.MsgId) != null ||
                        UnReadMsgs.FirstOrDefault(m => m.MsgKey == message.MsgId) != null) //重复消息
                            {
                                return;
                            }

                            msg.Content = s;
                            info = bb.text;
                            msg.TipMessage = msg.Content;
                            //AddMessage(msg, info, package);
                        });
                        break;
                    }
                    else
                    {
                        BigtxtHelper.AddBigtxtMsg(bb, null);
                    }
                    return msg.TipMessage = info;
                case MessageType.retract:
                    string retractId = message.Data?.retractId;
                    msg.RetractId = retractId;
                    SDKClient.SDKProperty.RetractType rType = message.Data?.retractType ?? SDKClient.SDKProperty.RetractType.Normal;
                    var target = Chat.Messages.ToList().FirstOrDefault(t => !string.IsNullOrEmpty(t.MsgKey) && t.MsgKey == retractId);
                    if (target == null)
                    {
                        target = UnReadMsgs.ToList().FirstOrDefault(t => t != null && t.MsgKey != null && t.MsgKey == retractId);
                        if (target == null)
                        {
                            if (Chat.LastMsg != null && Chat.LastMsg.MsgKey != null && Chat.LastMsg.MsgKey == retractId)
                            {

                                msg.MsgType = MessageType.notification;
                                if (rType == SDKClient.SDKProperty.RetractType.Normal)
                                {
                                    msg.MessageState = MessageStates.None;
                                }
                                else
                                {
                                    msg.MessageState = MessageStates.Fail;
                                }
                                msg.TipMessage = info = message.Content;
                                msgModel = msg;
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    this.UnReadMsgs.Add(msg);
                                });
                                return message.Content;
                            }
                            else
                            {
                                msg.TipMessage = info = message.Content;
                                msg.MsgType = MessageType.notification;
                                if (rType == SDKClient.SDKProperty.RetractType.Normal)
                                {
                                    msg.MessageState = MessageStates.None;
                                }
                                else
                                {
                                    msg.MessageState = MessageStates.Fail;
                                }
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    bool isUnread = true;
                                    if (AppData.MainMV.ChatListVM.SelectedItem == this)
                                    {
                                        if (AppData.MainMV.ListViewModel == AppData.MainMV.ChatListVM)
                                        {
                                            if (GotViewIsFocus != null && !GotViewIsFocus.Invoke())
                                            {

                                            }
                                            else
                                            {
                                                isUnread = false;
                                            }
                                        }
                                        ReceiveWithDrawMsg(msg, message.SenderName, message.OptionRecord == 1 ? true : false, isUnread, rType);
                                        return;
                                    }
                                });

                                var tempFirstMessages = this.FirstMessages?.Count > 0 ? this.FirstMessages.ToList() : null;

                                var tempMessage = tempFirstMessages?.FirstOrDefault(m => m.MsgId == retractId);
                                if (this.IsGroup)
                                {
                                    var tempUnReadMsgs = this.UnReadMsgs.ToList().Where(n => n.IsAtMeMsg).ToList();


                                    var tempMessages = this.FirstMessages != null && this.FirstMessages.Count > 0 ? this.FirstMessages.Where(n => n.TokenIds != null && n.MsgType != nameof(MessageType.retract)
                                    && n.TokenIds.Contains(AppData.Current.LoginUser.ID.ToString())).ToList() : null;
                                    int count = tempUnReadMsgs != null ? tempUnReadMsgs.Count : 0;
                                    count += tempMessages != null ? tempMessages.Count : 0;
                                    if (count == 0)
                                    {
                                        if (this.AtMeDic != null && this.AtMeDic.Count > 0)
                                        {
                                            var tempAtMsg = this.AtMeDic[retractId];
                                            if (tempAtMsg != null)
                                                this.AtMeDic.TryRemove(retractId, out tempAtMsg);
                                        }
                                        this.IsDisplayAtButton = false;
                                        this.HasAtMsg = false;
                                    }
                                }
                                if (isOffline)
                                {
                                    if (tempMessage != null)
                                    {
                                        msg.SendTime = tempMessage.MsgTime;
                                        this.FirstMessages.Remove(tempMessage);
                                    }
                                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        var tempUnReadMsgs = this.UnReadMsgs.ToList();
                                        var tempUnreadMsg = tempUnReadMsgs.FirstOrDefault(m => m.MsgKey == msg.MsgKey);
                                        if (tempUnreadMsg == null)
                                        {
                                            this.UnReadMsgs.Add(msg);
                                        }
                                        AppData.MainMV.ChatListVM.ResetSort();
                                    });

                                }
                                msgModel = msg;
                                break;
                            }
                        }
                        else
                        {


                        }
                    }

                    MessageModel retractMsg = Chat.Messages.ToList().FirstOrDefault(x => x != null && x.MsgKey != null && x.MsgKey.Equals(retractId));
                    string tip = string.Empty;

                    switch (rType)
                    {
                        default:
                        case SDKClient.SDKProperty.RetractType.Normal:
                            tip = "对方撤回了一条消息";
                            break;
                        case SDKClient.SDKProperty.RetractType.OfflineToOnline:
                            tip = "对方已取消离线文件传输，已转为在线发送";
                            break;
                        case SDKClient.SDKProperty.RetractType.TargetEndOnlineRetract:
                            tip = "对方取消了在线文件的接收";
                            break;
                        case SDKClient.SDKProperty.RetractType.OnlineToOffline:
                            tip = "对方已取消在线文件传输，已转为离线发送";
                            break;
                    }
                    if ((this.Model as ChatModel).IsGroup)
                    {
                        tip = "[" + message.SenderName + "]" + "撤回了一条消息";
                    }

                    if (message.OptionRecord == 1 || msg.Sender.ID == AppData.Current.LoginUser.ID)
                    {
                        tip = "您撤回了一条消息";
                    }
                    if (retractMsg != null)
                    {
                        msg.TipMessage = tip;
                        msg.MsgType = MessageType.notification;
                        msgModel = msg;
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            bool isUnread = true;
                            if (AppData.MainMV.ChatListVM.SelectedItem == this)
                            {
                                if (AppData.MainMV.ListViewModel == AppData.MainMV.ChatListVM)
                                {
                                    if (GotViewIsFocus != null && !GotViewIsFocus.Invoke())
                                    {

                                    }
                                    else
                                    {
                                        isUnread = false;
                                    }
                                }
                                ReceiveWithDrawMsg(msg, message.SenderName, message.OptionRecord == 1 ? true : false, isUnread, rType);
                                return;
                            }
                            else
                            {
                                isUnread = false;
                                //App.Current.Dispatcher.Invoke(() =>
                                //{

                                var tempUnReadMsgs = this.UnReadMsgs.ToList();
                                var tempMsgs = (this.Model as ChatModel).Messages.ToList();
                                var tempMsg = tempMsgs.FirstOrDefault(m => m.MsgKey == retractId);
                                if (tempMsg != null)
                                {
                                    msg.SendTime = tempMsg.SendTime;
                                    (this.Model as ChatModel).Messages.Remove(tempMsg);
                                }
                                var tempUnreadMsg = tempUnReadMsgs.FirstOrDefault(m => m.MsgKey == msg.MsgKey);
                                if (tempUnreadMsg == null)
                                {
                                    this.UnReadMsgs.Add(msg);
                                    //App.Current.Dispatcher.Invoke(() =>
                                    //{

                                    //});
                                }

                                //});
                            }
                            AppData.MainMV.ChatListVM.ResetSort();

                            //msgModel.MsgType = MessageType.notification;

                        });

                    }
                    else
                    {
                        retractMsg = this.UnReadMsgs.ToList().FirstOrDefault(x => x != null && x.MsgKey != null && x.MsgKey.Equals(retractId));
                        if (retractMsg != null)
                        {
                            msg.MsgType = MessageType.notification;
                            msg.TipMessage = tip;
                            App.Current.Dispatcher.Invoke(() =>
                            {

                                this.UnReadMsgs.Remove(retractMsg);
                                var tempUnReadMsgs = this.UnReadMsgs.ToList();
                                if (this.IsGroup)
                                {
                                    var unReadMsgs = tempUnReadMsgs.Where(n => n.IsAtMeMsg).ToList();
                                    if (unReadMsgs?.Count == 0)
                                    {
                                        if (this.AtMeDic != null && this.AtMeDic.Count > 0)
                                        {
                                            var tempAtMsg = this.AtMeDic[retractId];
                                            if (tempAtMsg != null)
                                                this.AtMeDic.TryRemove(retractId, out tempAtMsg);
                                        }
                                        this.IsDisplayAtButton = false;
                                        this.HasAtMsg = false;
                                    }
                                }
                                var tempUnreadMsg = tempUnReadMsgs.FirstOrDefault(m => m.MsgKey == msg.MsgKey);
                                if (tempUnreadMsg == null)
                                {
                                    this.UnReadMsgs.Add(msg);
                                    AppData.MainMV.ChatListVM.ResetSort();
                                }
                            });

                        }
                        else
                        {
                            AppData.MainMV.UpdateUnReadMsgCount();
                            AppData.MainMV.ChatListVM.TotalUnReadCount = AppData.MainMV.TotalUnReadCount;
                            if (AppData.MainMV.ChatListVM.TotalUnReadCount == 0)
                            {
                                AppData.MainMV.ChatListVM.CloseTrayWindow();
                            }
                            else
                            {
                                AppData.MainMV.ChatListVM.FlashIcon(this);
                            }
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                this.UnReadMsgs.Add(msg);
                                AppData.MainMV.ChatListVM.ResetSort();
                            });
                            this.SetLastMsg(msg, false);
                        }
                    }
                    SetOneMsgRead(msg.MsgKey, false);
                    msg.TipMessage = tip;
                    break;
                case MessageType.notification:
                    if (!string.IsNullOrEmpty(message.FileName))
                    {
                        if (msg.Content.Contains("失败") ||
                                msg.Content.Contains("取消") ||
                                msg.Content.Contains("异常") ||
                                msg.Content.Contains("中断"))
                        {
                            msg.MessageState = MessageStates.Fail;
                            msg.TipMessage = msg.Content = message.Content;
                            break;
                        }
                        else if (msg.Content.Contains("成功") && msg.Content.Contains("文件"))
                        {
                            msg.MessageState = MessageStates.Success;
                            msg.TipMessage = msg.Content = message.Content;
                            break;
                        }
                    }
                    if (msg.Content.Contains("文件"))
                    {
                        if (msg.Content.Contains("失败") ||
                                msg.Content.Contains("取消") ||
                                msg.Content.Contains("异常") ||
                                msg.Content.Contains("中断"))
                            msg.MessageState = MessageStates.Fail;
                    }

                    if ((message.OptionRecord & ((int)SDKClient.SDKProperty.MessageState.cancel)) == (int)SDKClient.SDKProperty.MessageState.cancel)
                    {
                        var targetMsgModel = this.Chat.Messages.FirstOrDefault(x => x != null && x.MsgKey != null && x.MsgKey.Equals(message.MsgId));
                        if (targetMsgModel != null)
                        {
                            var tempMessages = (this.Model as ChatModel).Messages.ToList();
                            int msgIndex = tempMessages.IndexOf(targetMsgModel);
                            if (msgIndex >= 0)
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    this.Chat.Messages.Remove(targetMsgModel);
                                });
                            }
                        }
                    }
                    else
                    {
                        msg.TipMessage = info = msg.Content;
                    }

                    SDKClient.SDKProperty.RetractType typeRetract = message.Data?.retractType ?? SDKClient.SDKProperty.RetractType.Normal;

                    if (typeRetract == SDKClient.SDKProperty.RetractType.Normal)
                    {
                        msg.MessageState = MessageStates.None;
                    }
                    else
                    {
                        msg.MessageState = MessageStates.Fail;
                    }
                    if (msg.Content.Contains("文件"))
                    {
                        if (msg.Content.Contains("失败") ||
                                msg.Content.Contains("取消") ||
                                msg.Content.Contains("异常") ||
                                msg.Content.Contains("中断"))
                            msg.MessageState = MessageStates.Fail;
                    }
                    break;
                case MessageType.usercard:
                    PersonCardModel pcm = new PersonCardModel()
                    {
                        Name = message.Data.name,
                        PhoneNumber = message.Data.phone,
                        PhotoImg = message.Data.photo,
                        UserId = message.Data.userId
                    };
                    string imgPath = message.Data.photo;
                    var imageFullPath = IMClient.Helper.ImageHelper.GetFriendFace(imgPath, (a) =>
                     {
                         msg.PersonCardModel.PhotoImg = a;
                     });
                    pcm.PhotoImg = imageFullPath;
                    msg.ContentMD5 = message.Data.photo;
                    msg.PersonCardModel = pcm;
                    msg.TipMessage = "[个人名片]" + pcm.Name;
                    msg.Content = "[个人名片]";
                    break;
                default:
                    msg.TipMessage = info = msg.Content;
                    break;

            }
            msgModel = msg;
            return info;
        }

        private void RecieveGroupNotice(MessageModel message, MessageEntity entity)
        {
            GroupNoticeModel groupNotice = new GroupNoticeModel()
            {
                NoticeTitle = entity.Data?.title,
                NoticeId = entity.Data?.noticeId,
                GroupNoticeContent = entity.Data?.content
            };
            message.Content = entity.Data?.content;
            message.SendTime = entity.Data?.publishTime ?? DateTime.Now;
            message.NoticeModel = groupNotice;
            message.TipMessage = message.NoticeModel.NoticeTitle;
            if (this.Chat.Chat is GroupModel gModel)
            {
                int groupId = gModel.ID;
                message.NoticeModel.GroupMId = groupId;
            }
            this.HasNewGroupNotice = entity.OptionRecord == 0 ? true : false;
            if (this.HasNewGroupNotice)
            {
                if (!offlineNoticeMessage.ContainsKey(message.NoticeModel.NoticeId))
                {
                    offlineNoticeMessage.Add(message.NoticeModel.NoticeId, message);
                }
            }
        }


        /// <summary>
        /// 离线消息增加到消息集合
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="info"></param>
        /// <param name="message"></param>
        public void AddMessage(MessageModel msg, string info, MessageEntity message, bool isOffline = false)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (UnReadServerMsgID.Count > 0 && UnReadServerMsgID.Exists(m => m == msg.MsgKey))
                        return;
                    if ((this.IsViewLoaded) && Chat.Messages != null && AppData.MainMV.ChatListVM.SelectedItem == this)
                    {
                        msg.TipMessage = info;
                        SetMsgShowTime(msg);
                        var tempMessages = Chat.Messages.ToList();
                        if (tempMessages.Count > 0)
                        {
                            var tempMessage = tempMessages[0];
                            if (tempMessage.SendTime <= msg.SendTime && tempMessages.Count > MAXCOUNT)
                            {
                                Chat.Messages.RemoveAt(0);
                                Chat.Messages.RemoveAt(0);
                            }
                            else if (tempMessage.SendTime > msg.SendTime && tempMessages.Count > MAXCOUNT)
                            {
                                return;
                            }
                        }



                        int lastIndex = tempMessages.Count - 1;

                        if (lastIndex > -1)
                        {
                            for (int i = lastIndex; i > -1; i--)
                            {
                                if (tempMessages[i].SendTime <= msg.SendTime)
                                {
                                    var tempMsg = Chat.Messages.ToList().FirstOrDefault(n => n.MsgKey == msg.MsgKey);
                                    if (tempMsg != null) break;
                                    Chat.Messages.Insert(i + 1, msg);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            var tempMsg = Chat.Messages.ToList().FirstOrDefault(n => n.MsgKey == msg.MsgKey);
                            if (tempMsg != null) return;
                            Chat.Messages.Add(msg);
                        }

                        //this.OnDisplayMsgHint?.Invoke();

                        if (AppData.MainMV.ListViewModel == AppData.MainMV.ChatListVM)
                        {
                            if ((GotViewIsFocus != null && !GotViewIsFocus.Invoke()) || !App.Current.MainWindow.IsActive)
                            {
                                var fromId = 0;
                                int.TryParse(message.From, out fromId);
                                if (message.OptionRecord == 1 && fromId == AppData.Current.LoginUser.ID)//自己发送的同步消息
                                {
                                    SetOneMsgRead(message.MsgId);
                                }
                                else
                                {
                                    UnReadServerMsgID.Add(msg.MsgKey);
                                    //AppData.MainMV.ChatListVM.FlashIcon(this);
                                }
                            }
                            else
                            {
                                this.OnDisplayAtButton?.Invoke();
                                SetOneMsgRead(message.MsgId);
                            }
                        }
                        else
                        {
                            var fromId = 0;
                            int.TryParse(message.From, out fromId);
                            if (message != null && message.OptionRecord == 1 && fromId == AppData.Current.LoginUser.ID)//自己发送的同步消息
                            {
                                // SetOneMsgRead(package.id);
                            }
                            else
                            {
                                if (message.OptionRecord == 0)
                                {
                                    UnReadServerMsgID.Add(msg.MsgKey);
                                }
                            }
                        }
                    }
                    else
                    {
                        var tempUnReadMsgs = UnReadMsgs.ToList();
                        if (!string.IsNullOrEmpty(msg.RetractId))
                        {
                            var tempUnReadMsg = UnReadMsgs.FirstOrDefault(m => m.MsgKey == msg.RetractId);
                            if (tempUnReadMsg != null)
                                UnReadMsgs.Remove(tempUnReadMsg);
                        }
                        var tempMsg = UnReadMsgs.FirstOrDefault(m => m.RetractId == msg.MsgKey);
                        if (tempMsg != null)
                            return;
                        msg.IsRead = message.OptionRecord;
                        msg.TipMessage = info;
                        if (!isOffline)
                            this.SetLastMsg(msg, false);
                        if (UnReadMsgs.FirstOrDefault(m => m.MsgKey == msg.MsgKey) == null)
                            UnReadMsgs.Add(msg);
                        UnReadServerMsgID.Add(msg.MsgKey);
                        //if (this.UnReadCount > 0)
                        //{
                        //    int count = this.UnReadMsgs.Where(m => m.MsgType != MessageType.notification && m.Sender != null && m.Sender.ID != AppData.Current.LoginUser.ID).Count();
                        //    if (!this.HasActived)
                        //    {
                        //        if (count == 0)
                        //            count = this.FirstMessages.Where(m => m.MsgType != nameof(MessageType.notification) && m.OptionRecord == 0).Count();
                        //        else
                        //        {
                        //            count += this.FirstMessages.Where(m => m.MsgType != nameof(MessageType.retract) && m.MsgType != nameof(MessageType.notification) && m.OptionRecord == 0).Count();
                        //        }
                        //    }

                        //    this.UnReadCount = count;
                        //}
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("ChatViewModel:聊天消息写入聊天框" + ex.Message + ex.StackTrace);
                }
            }));

            if ((this.Model as ChatModel).Chat is GroupModel groupModel && groupModel.IsTopMost)
            {
                return;
            }
        }
        #endregion
        public void AddStrangerMessage(MessageModel msg, string info, string retractId = "")
        {
            msg.TipMessage = info;
            this.SetLastMsg(msg);

            if (!msg.IsSync)
            {
                msg.IsRead = 0;
                this.UnReadCount++;
            }
            var tempStrangerMsgs = this.StrangerMessageList.ToList();
            if (tempStrangerMsgs.Any(x => x.ID == msg.Sender.ID))
            {
                var vm = tempStrangerMsgs.FirstOrDefault(x => x.ID == msg.Sender.ID);
                (vm.Model as ChatModel).LastMsg = msg;
                vm.CurrentChatType = ChatType.Stranger;
                var tempMessages = (vm.Model as ChatModel).Messages.ToList();
                if (tempMessages.FirstOrDefault(m => m.MsgKey == msg.MsgKey) == null)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (!msg.IsSync)
                        {
                            vm.UnReadCount++;
                        }
                        if (msg.MsgType == MessageType.retract && string.IsNullOrEmpty(retractId))
                        {
                            msg.MsgType = MessageType.notification;
                            if (!msg.IsSync)
                            {
                                if (this.UnReadCount > 0)
                                    this.UnReadCount--;
                                if (vm.UnReadCount > 0)
                                    vm.UnReadCount--;
                            }
                        }

                        vm.SetMsgShowTime(msg);
                        (vm.Model as ChatModel).Messages.Add(msg);

                    });
                }

            }
            else
            {
                msg.Sender.IsNotDisturb = AppData.MainMV.ChatListVM.StrangerMessage.Chat.Chat.IsNotDisturb;
                ChatModel model = new ChatModel(msg.Sender);
                ChatViewModel vm = new ChatViewModel(model, msg);
                vm.CurrentChatType = ChatType.Stranger;
                App.Current.Dispatcher.Invoke(() =>
                {
                    if (!msg.IsSync)
                    {
                        vm.UnReadCount++;
                    }
                    if (msg.MsgType == MessageType.retract && string.IsNullOrEmpty(retractId))
                    {
                        msg.MsgType = MessageType.notification;
                        if (!msg.IsSync)
                        {
                            if (this.UnReadCount > 0)
                                this.UnReadCount--;
                            if (vm.UnReadCount > 0)
                                vm.UnReadCount--;
                        }
                    }
                    var tempStrangerMsgList = this.StrangerMessageList.ToList();
                    if (!tempStrangerMsgList.Any(x => x.ID == msg.Sender.ID))
                    {
                        vm.SetMsgShowTime(msg);
                        model.Messages.Add(msg);
                        this.StrangerMessageList.Insert(0, vm);
                    }
                    else
                    {
                        var strangerMsgVm = tempStrangerMsgList.FirstOrDefault(x => x.ID == msg.Sender.ID);
                        (strangerMsgVm.Model as ChatModel).LastMsg = msg;
                        strangerMsgVm.CurrentChatType = ChatType.Stranger;
                        var tempMessages = (strangerMsgVm.Model as ChatModel).Messages.ToList();
                        if (tempMessages.FirstOrDefault(m => m.MsgKey == msg.MsgKey) == null)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                if (!msg.IsSync)
                                {
                                    strangerMsgVm.UnReadCount++;
                                }
                                if (msg.MsgType == MessageType.retract && string.IsNullOrEmpty(retractId))
                                {
                                    msg.MsgType = MessageType.notification;
                                    if (!msg.IsSync)
                                    {
                                        if (this.UnReadCount > 0)
                                            this.UnReadCount--;
                                        if (strangerMsgVm.UnReadCount > 0)
                                            strangerMsgVm.UnReadCount--;
                                    }
                                }

                                strangerMsgVm.SetMsgShowTime(msg);
                                (strangerMsgVm.Model as ChatModel).Messages.Add(msg);
                            });
                        }
                    }
                });
            }

            this.OnDelegateToStrangerMessageView?.Invoke();
            this.OnResetStrangerMessageListSort?.Invoke();

            App.Current.Dispatcher.Invoke(() =>
            {
                AppData.MainMV.ChatListVM.ResetSort();
            });

            if (msg.MsgType == MessageType.retract && !string.IsNullOrEmpty(retractId))
            {
                ReceiveWithDrawStrangerMsg(retractId, msg.Sender.ID);
            }
        }

        private void AddMessage(MessageModel msg, string info, MessagePackage package)
        {
            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                try
                {
                    bool isReplied = false;
                    if (this.IsTemporaryChat && this.CurrentChatType == ChatType.Temporary && package.syncMsg != 1)
                    {
                        isReplied = true;
                        this.CurrentChatType = ChatType.Temporary_Replied;
                    }
                    if (this.View is ChatView chatview)
                    {
                        if (chatview.chatBox.IsVerticalScrollBarAtBottom)
                            chatview.chatBox.ScallToEnd();//如果滚轮在最底部，收到新消息就滚动到最底部，否则不管
                                                          //其他情况不用管

                    }


                    if (this.IsViewLoaded && Chat.Messages != null && AppData.MainMV.ChatListVM.SelectedItem == this)
                    {
                        msg.TipMessage = info;
                        this.SetLastMsg(msg);

                        SetMsgShowTime(msg);
                        while (Chat.Messages.Count > MAXCOUNT)
                        {
                            Chat.Messages.RemoveAt(0);
                            //Chat.Messages.RemoveAt(0);
                        }

                        int lastIndex = Chat.Messages.Count - 1;

                        if (lastIndex > -1)
                        {
                            for (int i = lastIndex; i > -1; i--)
                            {
                                if (Chat.Messages[i].SendTime <= msg.SendTime)
                                {
                                    var tempMessages = Chat.Messages.ToList();
                                    var tempMsg = tempMessages.FirstOrDefault(m => m.MsgKey == msg.MsgKey);
                                    if (tempMsg != null) break;
                                    Chat.Messages.Insert(i + 1, msg);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            var tempMessages = Chat.Messages.ToList();
                            var tempMsg = tempMessages.FirstOrDefault(m => m.MsgKey == msg.MsgKey);
                            if (tempMsg != null) return;
                            Chat.Messages.Add(msg);
                        }

                        if (isReplied)
                        {
                            string content = "已转为临时聊天";
                            if (!IsExistNotificationByContent(content))
                            {
                                AddMessageTip(content);
                                SDKClient.SDKClient.Instance.DeleteFriend(this.Chat.Chat.ID);
                            }
                        }

                        this.OnDisplayMsgHint?.Invoke();

                        if (AppData.MainMV.ListViewModel == AppData.MainMV.ChatListVM)
                        {
                            if ((GotViewIsFocus != null && !GotViewIsFocus.Invoke()) || !App.Current.MainWindow.IsActive)
                            {
                                if (package != null && package.syncMsg == 1 && package.from.ToInt() == AppData.Current.LoginUser.ID)//自己发送的同步消息
                                {
                                    SetOneMsgRead(package.id);
                                }
                                else
                                {
                                    this.UnReadCount++;
                                    UnReadServerMsgID.Add(msg.MsgKey);
                                    AppData.MainMV.ChatListVM.FlashIcon(this);
                                }
                            }
                            else
                            {
                                this.OnDisplayAtButton?.Invoke();
                                SetOneMsgRead(package.id);
                            }
                        }
                        else
                        {
                            if (package != null && package.syncMsg == 1 && package.from.ToInt() == AppData.Current.LoginUser.ID)//自己发送的同步消息
                            {
                                // SetOneMsgRead(package.id);
                            }
                            else
                            {
                                if (package.read == 0 && package.from.ToInt() != AppData.Current.LoginUser.ID)
                                {
                                    this.UnReadCount++;
                                    UnReadServerMsgID.Add(msg.MsgKey);
                                    AppData.MainMV.ChatListVM.FlashIcon(this);
                                }


                            }
                        }
                    }
                    else
                    {
                        IsAllRead = false;
                        msg.IsRead = 0;
                        msg.TipMessage = info;
                        this.SetLastMsg(msg, false);
                        if (UnReadMsgs.ToList().FirstOrDefault(m => m.MsgKey == msg.MsgKey) == null)
                            UnReadMsgs.Add(msg);

                        if (package != null && package.syncMsg == 1 && package.from.ToInt() == AppData.Current.LoginUser.ID)//自己发送的同步消息
                        {
                            // SetOneMsgRead(package.id);
                        }
                        else
                        {
                            if (package.read == 0 && package.from.ToInt() != AppData.Current.LoginUser.ID)
                            {
                                this.UnReadCount++;
                                AppData.MainMV.ChatListVM.FlashIcon(this);
                            }
                        }
                        UnReadServerMsgID.Add(msg.MsgKey);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("ChatViewModel:聊天消息写入聊天框" + ex.Message + ex.StackTrace);
                }
                if ((this.Model as ChatModel).Chat is GroupModel groupModel && groupModel.IsTopMost)
                {
                    return;
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    AppData.MainMV.ChatListVM.ResetSort();
                });
            }));
        }

        public void SetScrollOffset()
        {
            this.AppendMessage?.Invoke(null);
        }

        public void DisplayHint()
        {
            this.OnDisplayMsgHint?.Invoke();
        }

        public void DisplayAtButton()
        {
            this.OnDisplayAtButton?.Invoke();
        }

        public void PopupGroupNoticeWindow()
        {
            this.OnPopUpNoticeWindow?.Invoke();
        }

        ///// <summary>
        ///// 添加时间标签
        ///// </summary>
        ///// <param name="currentMsg">当前消息</param>
        ///// <param name="compareMsg">需要相邻比较的消息</param> 
        //private void AddTimeTip(MessageModel currentMsg, MessageModel compareMsg,bool isForword=false)
        //{ 
        //    ChatModel chat = this.Model as ChatModel;

        //    //-1 不显示，0 时间往前插入，1 时间往后追加
        //    int index = -1;

        //    MessageModel msg;
        //    //无比较，直接添加
        //    if (compareMsg == null)
        //    {
        //        index = 1; 
        //    }
        //    else
        //    { 
        //        double spanMinite = (currentMsg.SendTime - compareMsg.SendTime).TotalMinutes;

        //        if (spanMinite >= 5)
        //        {
        //            index = 1;
        //        }
        //        else  if(spanMinite<0) //向前追加消息
        //        {
        //            if (spanMinite <=-5) //插入新的时间
        //            {
        //                index = 0;
        //            }
        //            else  //向前的消息间隔不超过五分钟-替换
        //            {
        //                compareMsg.SendTime = currentMsg.SendTime;
        //            }
        //        } 
        //    }

        //    if (index>=0)
        //    {
        //        msg = new MessageModel()
        //        {
        //            SendTime = currentMsg.SendTime,
        //            MsgType = MessageType.notification,
        //            Content = GetTimeFromatString(currentMsg.SendTime), 
        //        };
        //        //if (index == 1)
        //        //{
        //        //    if (chat.Messages.Count > 0 && chat.Messages.LastOrDefault().MsgType == MessageType.notification)
        //        //    {
        //        //        msg.ToString();
        //        //        return;
        //        //    }
        //        //}
        //        App.Current.Dispatcher.Invoke(new Action(() =>
        //        {
        //            if (isForword)
        //            {
        //                chat.Messages.Insert(0, msg);
        //            }
        //            else
        //            { 
        //                chat.Messages.Add(msg); 
        //            }
        //        })); 
        //    } 
        //}

        public MessageModel AddMessageTip(string tip, DateTime? dt = null, bool isForward = false, string msgId = null, string actionableContent = null, UserModel userModel = null)
        {
            MessageModel msg = new MessageModel()
            {
                //MsgKey = string.IsNullOrEmpty(msgId) == true ? "[提示]" + Guid.NewGuid().ToString() : msgId,
                MsgKey = msgId,
                MsgType = MessageType.notification,
                Content = tip,
                ActionableContent = actionableContent,
                TipMessage = tip + actionableContent,
                SendTime = dt ?? DateTime.Now,
                Sender = userModel != null ? userModel : AppData.Current.LoginUser.User,
            };
            if (!isForward)//新消息
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.SetLastMsg(msg);
                    if (this.HasActived)
                    {
                        if (AppData.MainMV.ChatListVM.SelectedItem == this)
                        {
                            var tempMessages = (this.Model as ChatModel).Messages.ToList();
                            var tempMessage = tempMessages.FirstOrDefault(m => !string.IsNullOrEmpty(m.MsgKey) && m.MsgKey == msg.MsgKey && m.MsgType == msg.MsgType);
                            if (tempMessage == null)
                            {
                                SetMsgShowTime(msg);
                                (this.Model as ChatModel).Messages.Add(msg);
                            }
                        }
                        else
                        {
                            var tempUnReadMsgs = UnReadMsgs.ToList();
                            var tempMsg = tempUnReadMsgs.FirstOrDefault(m => !string.IsNullOrEmpty(m.MsgKey) && m.MsgKey == msg.MsgKey && m.MsgType == msg.MsgType);
                            if (tempMsg == null && !msg.IsMine)
                            {
                                msg.IsRead = 0;
                                UnReadMsgs.Add(msg);
                            }
                        }
                    }
                    else
                    {
                        //msg.IsRead = 0;
                        var tempUnReadMsgs = UnReadMsgs.ToList();
                        var tempMsg = tempUnReadMsgs.FirstOrDefault(m => !string.IsNullOrEmpty(m.MsgKey) && m.MsgKey == msg.MsgKey && m.MsgType == msg.MsgType);
                        if (tempMsg == null)
                            UnReadMsgs.Add(msg);
                    }
                    AppData.MainMV.ChatListVM.ResetSort();
                }));

            }
            else//上翻
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    SetMsgShowTime(msg, false);
                    ChatModel chat = this.Model as ChatModel;
                    chat.Messages.Insert(0, msg);
                }));
            }
            return msg;
        }

        public void AddMessageTipEx(string tip, DateTime? sendTime = null, string msgId = "")

        {

            MessageModel msg = new MessageModel()
            {
                //MsgKey = "[提示]" + Guid.NewGuid().ToString(),
                MsgType = MessageType.notification,
                Content = tip,
                TipMessage = tip,
                SendTime = sendTime == null ? DateTime.Now : sendTime.Value
            };
            if (string.IsNullOrEmpty(msgId))
                SetMsgShowTime(msg);
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                ChatModel chat = this.Model as ChatModel;
                if (string.IsNullOrEmpty(msgId))
                    this.SetLastMsg(msg);
                if (sendTime == null)
                    chat.Messages.Add(msg);
                else
                {
                    var tempMsg = chat.Messages.ToList().FirstOrDefault(m => m.MsgKey == msgId);
                    int index = 0;
                    if (tempMsg != null)
                        index = chat.Messages.ToList().IndexOf(tempMsg);
                    if (chat.Messages.Count == 0)
                        chat.Messages.Insert(0, msg);
                    else
                        chat.Messages.Insert(index, msg);
                    //chat.Messages.Add(msg);
                }
            }));
        }

        /// <summary>
        /// 设置条目是否显示时间值
        /// </summary>
        /// <param name="msg">信息题目</param>
        /// <param name="isAdd">是否追加，true 追加，false 往前插入</param>
        public void SetMsgShowTime(MessageModel msg, bool isAdd = true)
        {
            var tempMessages = Chat.Messages.ToList();
            if (msg.MsgType == MessageType.notification && msg.Content == ConstString.BecomeFriendsTip || msg.Content == "已经是好朋友，开始聊天吧")
            {
                return;
            }

            //if (tempMessages.Count > 0)
            //{
            //    var tempNotificationMsg = tempMessages.FirstOrDefault(m => m.MsgType == MessageType.notification && msg.Content == ConstString.BecomeFriendsTip || msg.Content == "已经是好朋友，开始聊天吧");
            //    if (tempNotificationMsg != null)
            //    {
            //        //double interval = (msg.SendTime - last.SendTime).TotalMinutes;
            //        if (tempMessages[0] == tempNotificationMsg)
            //        {
            //            tempNotificationMsg.ShowSendTime = true;
            //        }
            //        else
            //        {
            //            tempNotificationMsg.ShowSendTime = false;
            //        }
            //    }
            //}

            if (tempMessages.Count == 0)
            {
                msg.ShowSendTime = true;
                return;
            }
            else if (tempMessages.Count == 1 && tempMessages.Exists(m => m.Content == ConstString.FollowingIsNewMessage))
            {
                msg.ShowSendTime = true;
                return;
            }

            if (isAdd)
            {
                var last = tempMessages.LastOrDefault(m => m.Content != ConstString.FollowingIsNewMessage);
                double interval = (msg.SendTime - last.SendTime).TotalMinutes;
                if (interval >= 5)
                {
                    msg.ShowSendTime = true;
                }
                else
                {
                    msg.ShowSendTime = false;
                }
            }
            else
            {
                var first = tempMessages.FirstOrDefault(m => m.Content != ConstString.FollowingIsNewMessage);
                double interval = (first.SendTime - msg.SendTime).TotalMinutes;
                msg.ShowSendTime = true;
                if (interval < 5)
                {
                    first.ShowSendTime = false;
                }
            }
        }

        /// <summary>
        /// 设置所有消息为已读
        /// </summary>
        public void SetAllRead()
        {
            this.HasAtMsg = false;
            this.HasNewGroupNotice = false;
            this.UnReadCount = 0;
            this.IsAllRead = true;
            this.UnReadMsgs = this.UnReadMsgs.Select(m => { m.IsRead = 1; return m; }).ToList();
            AppData.MainMV.UpdateUnReadMsgCount();
            AppData.MainMV.ChatListVM.TotalUnReadCount = AppData.MainMV.TotalUnReadCount;
            if (AppData.MainMV.ChatListVM.TotalUnReadCount == 0)
            {
                AppData.MainMV.ChatListVM.CloseTrayWindow(false);
            }
            else
            {
                AppData.MainMV.ChatListVM.FlashIcon(this);
            }
        }

    }
    /// <summary>
    /// 全局搜索数据对象
    /// </summary>
    public class GlobalSearchChatModel : ChatViewModel
    {
        public new ChatModel Chat { get; }
        public new bool IsGroup { get; }
        public GlobalSearchChatModel(ChatModel model) : base(model)
        {
            Chat = model;
            this.IsGroup = model.IsGroup;
            this.AtUserModel = AppData.Current.LoginUser.User;
        }
        private string _keyWord;
        /// <summary>
        /// 搜索关键字
        /// </summary>
        public string KeyWord
        {
            get { return _keyWord; }
            set { _keyWord = value; this.OnPropertyChanged(); }
        }
        /// <summary>
        /// 分组类型
        /// </summary>
        public SearchGroupType GroupType { get; set; }
        /// <summary>
        /// 是否显示电话号码
        /// </summary>
        public bool IsShowPhoneNum { get; set; }
        /// <summary>
        /// 搜索的扩展内容
        /// </summary>
        public string SearchExContent { get; set; }
        /// <summary>
        /// 分组类型
        /// </summary>
        public string GroupTypeName { get; set; }
    }
    public enum SearchGroupType
    {
        Contact = 0,
        Group = 1,
        Black = 2
    }

    public enum ChatType
    {
        /// <summary>
        /// 正常聊天
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 粉丝留言
        /// </summary>
        Stranger,
        /// <summary>
        /// 被回复过的临时聊天
        /// </summary>
        Temporary_Replied,
        /// <summary>
        /// 本人发起的临时聊天(等待回复)
        /// </summary>
        Temporary,
        /// <summary>
        /// 文件小助手
        /// </summary>
        FileAssistant
    }
}
