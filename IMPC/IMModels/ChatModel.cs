using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace IMModels
{
    /// <summary>
    /// 聊天对象模型
    /// </summary>
    public class ChatModel : BaseModel
    {
        /// <summary>
        /// 聊天对象
        /// </summary>
        /// <param name="chat"></param>
        public ChatModel(IChat chat)
        {
            if (chat != null)
            {
                this.ID = chat.ID;                
                this.Chat = chat;
                IsGroup = chat is GroupModel;
                this.IsShowGroupNoticeBtn = chat is GroupModel;
                this.Messages = new ObservableCollection<MessageModel>();
            }
        }
        
        /// <summary>
        /// 全盘替换消息
        /// </summary>
        /// <param name="news"></param>
        public void ReplaceMsgs(IEnumerable<MessageModel> news)
        {
            this.Messages = new ObservableCollection<MessageModel>(news);
        }

        /// <summary>
        /// 聊天对象
        /// </summary>
        public IChat Chat { get; }

        private ObservableCollection<MessageModel> _messages;
        /// <summary>
        /// 聊天内容集合
        /// </summary>
        public ObservableCollection<MessageModel> Messages
        {
            get { return _messages; }
            set { _messages = value; this.OnPropertyChanged(); } 
        }



        private MessageModel _lastMsg;
        /// <summary>
        /// 最后收到的信息
        /// </summary>
        public MessageModel LastMsg
        {
            get { return _lastMsg; }
            set { _lastMsg = value; this.OnPropertyChanged(); }
        }

        private bool _isGroup;
        /// <summary>
        /// 是否组
        /// </summary>
        public bool IsGroup
        {
            get { return _isGroup; }
            set { _isGroup = value; this.OnPropertyChanged(); }
        }

        private bool _isShowGroupNoticeBtn;
        /// <summary>
        /// 是否显示群公告按钮
        /// </summary>
        public bool IsShowGroupNoticeBtn
        {
            get { return _isShowGroupNoticeBtn; }
            set { _isShowGroupNoticeBtn = value;this.OnPropertyChanged(); }
        }

        //private bool _isNotDisturb;
        ///// <summary>
        ///// 是否免打扰
        ///// </summary>
        //public bool IsNotDisturb
        //{
        //    get { return _isNotDisturb; }
        //    set { _isNotDisturb = value; this.OnPropertyChanged(); }
        //}

        //private bool _isTopMost;
        ///// <summary>
        ///// 是否置顶
        ///// </summary>
        //public bool IsTopMost
        //{
        //    get { return _isTopMost; }
        //    set { _isTopMost = value; this.OnPropertyChanged(); }
        //}

        //private DateTime? _topMostTime;
        ///// <summary>
        ///// 置顶时间
        ///// </summary>
        //public DateTime? TopMostTime
        //{
        //    get { return _topMostTime; }
        //    set { _topMostTime = value; this.OnPropertyChanged(); }
        //}

        private int _newMessageCount;
        /// <summary>
        /// 新消息数量（未读消息）
        /// </summary>
        public int NewMessageCount
        {
            get { return _newMessageCount; }
            set { _newMessageCount = value; this.OnPropertyChanged(); }
        }
        
    }
}
