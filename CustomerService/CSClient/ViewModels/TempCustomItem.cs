using IMModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSClient.ViewModels
{
    public class TempCustomItem:ViewModel
    {
       
        public event Action<bool> OnStartOrStopSession;

        public event Action OnDisplayMsgHint;

        /// <summary>
        /// 聊天项VM
        /// </summary>
        /// <param name="view"></param>
        public TempCustomItem(CustomItem model) : base(model)
        {
            Chat = model;
        }

        /// <summary>
        /// 聊天项VM
        /// </summary>
        /// <param name="view"></param>
        public TempCustomItem(CustomItem model, MessageModel last) : this(model)
        {
            LastMsg = last;
        }
                 

        #region public Property
        private MessageModel _lastMsg;
        /// <summary>
        /// 最后的消息
        /// </summary>
        public MessageModel LastMsg
        {
            get { return _lastMsg; }
            set { _lastMsg = value; this.OnPropertyChanged(); }
        }


        public CustomItem Chat { get; }

        private int _unReadCount;
        /// <summary>
        /// 未读消息数量
        /// </summary>
        public int UnReadCount
        {
            get { return _unReadCount; }
            set
            {
                _unReadCount = value; this.OnPropertyChanged();
               
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

        private bool _isShowGroupNoticeBtn;
        /// <summary>
        /// 是否显示群公告按钮
        /// </summary>
        public bool IsShowGroupNoticeBtn
        {
            get { return _isShowGroupNoticeBtn; }
            set { _isShowGroupNoticeBtn = value; this.OnPropertyChanged(); }
        }

        private bool _isSessionEnd;
        /// <summary>
        /// 会话是否已结束（界面做标识）
        /// </summary>
        public bool IsSessionEnd
        {
            get { return _isSessionEnd; }
            set
            {
                _isSessionEnd = value; this.OnPropertyChanged();

            }
        }

        private bool _isDisplayStartSession;
        public bool IsDisplayStartSession
        {
            get { return _isDisplayStartSession; }
            set { _isDisplayStartSession = value; this.OnPropertyChanged(); }
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
        private VMCommand _preemptChatCommand;
        /// <summary>
        /// 抢占聊天命令
        /// </summary>
        public VMCommand PreemptChatCommand
        {
            get
            {
                if (_preemptChatCommand == null)
                    _preemptChatCommand = new VMCommand(JupmToChat);
                return _preemptChatCommand;
            }
        }


        private void JupmToChat(object para)
        {
            if (this.Model is CustomItem user && user.ID > 0)
            {
                CustomItem item = this.Model as CustomItem;
                AppData.MainMV.JumpToChatModel(item);
                System.Threading.ThreadPool.QueueUserWorkItem( o =>
                {
                    SDKClient.SDKClient.Instance.SendCSSyncMsgStatus(item.ID,item.HeadImg,item.DisplayName);
                });

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
                    _ShowBusinessCard = new VMCommand(AppData.MainMV.ShowUserBusinessCard, new Func<object, bool>(o => o != null));
                return _ShowBusinessCard;
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

      

        #endregion

        #region CommandMethods

        #region Send msg

        private void HideMessage(object para)
        {
            if (this.Model is ChatModel chat && para is MessageModel msg)
            {
                SDKClient.SDKClient.Instance.UpdateHistoryMsgIsHidden(msg.MsgKey);

                var taraget = chat.Messages.FirstOrDefault(info => info.MsgKey == msg.MsgKey);

                int index = chat.Messages.IndexOf(taraget);

                if (index == 0) //第一条
                {
                    if (chat.Messages.Count > 1)
                    {
                        chat.Messages[1].ShowSendTime = true;
                    }
                }
                else if (index > 0 && index != chat.Messages.Count - 1) //不是最后一条
                {
                    var per = chat.Messages[index - 1];
                    var next = chat.Messages[index + 1];

                    if ((next.SendTime - per.SendTime).TotalMinutes >= 5)
                    {
                        next.ShowSendTime = true;
                    }
                }
                chat.Messages.Remove(taraget);
            }
        }

        StringBuilder _strB = new StringBuilder();

        

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
        #endregion

       

   

        /// <summary>
        /// 删除聊天
        /// </summary>
        /// <param name="para"></param>
        void DeleteChat(object para)
        {
            

            //chatModel.Messages.Clear();
            //删除聊天条目
            AppData.MainMV.HisChatListVM.Items.Remove(this);
            
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
                //AppData.MainMV.JumpToGroupModel(model.Chat as GroupModel);
            }
        }

        /// <summary>
        /// 免打扰
        /// </summary> 
        private void NoDisturb(object para)
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
                if (chat.Chat.IsNotDisturb)
                {
                    SDKClient.SDKClient.Instance.UpdateUserSetsInGroup(id, 3, content);
                }
                else
                {
                    SDKClient.SDKClient.Instance.UpdateUserSetsInGroup(id, 3, content);
                }
            }
            else
            {
                if (chat.Chat.IsNotDisturb)
                {
                    SDKClient.SDKClient.Instance.UpdateFriendSet(SDKClient.Model.UpdateFriendSetPackage.FriendSetOption.设置免打扰, content, id);
                }
                else
                {
                    SDKClient.SDKClient.Instance.UpdateFriendSet(SDKClient.Model.UpdateFriendSetPackage.FriendSetOption.设置免打扰, content, id);
                }
            }
        }

        /// <summary>
        /// 聊天置顶
        /// </summary>
        private void TopMost(object para)
        {
            if (!AppData.CanInternetAction())
            {
                return;
            }

            ChatModel chat = this.Model as ChatModel;
            int id = chat.Chat.ID;

            chat.Chat.TopMostTime = (chat.Chat.TopMostTime.HasValue && chat.Chat.TopMostTime.Value == DateTime.MinValue) ?
                DateTime.Now : DateTime.MinValue;
            chat.Chat.IsTopMost = (chat.Chat.TopMostTime == DateTime.MinValue) ? false : true;
            string content = (chat.Chat.TopMostTime.Value != DateTime.MinValue) ? "1" : "0";

            if (chat.IsGroup)
            {
                if (chat.Chat.TopMostTime.HasValue && chat.Chat.TopMostTime.Value != DateTime.MinValue)
                {
                    SDKClient.SDKClient.Instance.UpdateUserSetsInGroup(id, 2, content);
                }
                else
                {
                    SDKClient.SDKClient.Instance.UpdateUserSetsInGroup(id, 2, content);
                }
            }
            else
            {
                if (chat.Chat.TopMostTime.HasValue && chat.Chat.TopMostTime.Value != DateTime.MinValue)
                {
                    SDKClient.SDKClient.Instance.UpdateFriendSet(SDKClient.Model.UpdateFriendSetPackage.FriendSetOption.设置是否消息置顶, content, id);
                }
                else
                {
                    SDKClient.SDKClient.Instance.UpdateFriendSet(SDKClient.Model.UpdateFriendSetPackage.FriendSetOption.设置是否消息置顶, content, id);
                }
            }
        }

        #endregion

       

       
        /// <summary>
        /// 添加一条“以下为新消息”的提示信息
        /// </summary>
       

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

        

        
        public void StartOrStopSession(bool isStart)
        {
            this.IsSessionEnd = !isStart;
            this.OnStartOrStopSession?.Invoke(isStart);
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

        

        
       
    }
}
