using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMModels
{
    /// <summary>
    /// 消息模型
    /// </summary>
    public class MessageModel : BaseModel
    {
        /// <summary>
        /// 操作线程任务
        /// </summary>
        public System.Threading.CancellationTokenSource OperateTask;

        /// <summary>
        /// 消息加密key
        /// </summary>
        public string MsgKey { get; set; } = Guid.NewGuid().ToString();

        private bool _isSync;
        /// <summary>
        /// 是否是同步消息
        /// </summary>
        public bool IsSync
        {
            get { return _isSync; }
            set { _isSync = value; this.OnPropertyChanged(); }
        }
        private IChat _sender;
        /// <summary>
        /// 消息发送方
        /// </summary>
        public IChat Sender
        {
            get { return _sender; }
            set
            {
                _sender = value;
                if (_sender != null)
                {
                    if (_sender is UserModel user)
                        HeadImg = user.HeadImg;
                    else if (_sender is GroupMember groupMember)
                        HeadImg = groupMember.TargetUser.HeadImg;

                }
                this.OnPropertyChanged();
            }
        }
        private string _headImg;
        /// <summary>
        /// 联系人头像
        /// </summary>
        public string HeadImg
        {
            get { return _headImg; }
            set
            {
                _headImg = value;
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// 消息接收方
        /// </summary>
        public IChat Receiver { get; set; }

        private bool _isMine;
        /// <summary>
        /// 是否我发送的消息（显示从右到左）
        /// </summary>
        public bool IsMine
        {
            get { return _isMine; }
            set { _isMine = value; this.OnPropertyChanged(); }
        }

        private bool _isSending;
        /// <summary>
        /// 是否正在上传
        /// </summary>
        public bool IsSending
        {
            get { return _isSending; }
            set { _isSending = value; this.OnPropertyChanged(); }
        }

        private DateTime _sendTime;
        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime
        {
            get { return _sendTime; }
            set { _sendTime = value; this.OnPropertyChanged(); }
        }

        private string _historySendTime;
        public string HistorySendTime
        {
            get { return _historySendTime; }
            set
            {
                _historySendTime = value;
                this.OnPropertyChanged();
            }
        }
        private bool _showSendTime;
        /// <summary>
        /// 是否显示时间标签
        /// </summary>
        public bool ShowSendTime
        {
            get { return _showSendTime; }
            set { _showSendTime = value; this.OnPropertyChanged(); }
        }


        private MessageType messageType;
        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageType MsgType
        {
            get { return messageType; }
            set { messageType = value; this.OnPropertyChanged(); }
        }


        private string _content;
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content
        {
            get { return _content; }
            set { _content = value; this.OnPropertyChanged(); }
        }

        private string _msgHyperlink;
        /// <summary>
        /// 分享、网站、动态消息的超链接
        /// </summary>
        public string MsgHyperlink
        {
            get { return _msgHyperlink; }
            set
            {
                _msgHyperlink = value;
                this.OnPropertyChanged();
            }
        }

        private string _shareMsgImage;
        /// <summary>
        /// 分享、网站、动态消息的资源图片
        /// </summary>
        public string ShareMsgImage
        {
            get { return _shareMsgImage; }
            set
            {
                _shareMsgImage = value;
                this.OnPropertyChanged();
            }
        }


        private string _actionableContent;
        /// <summary>
        /// 可点击的内容（当MsgType为notification时，比如"你不在对方通讯录内，有事找TA"中的"有事找TA"）
        /// </summary>
        public string ActionableContent
        {
            get { return _actionableContent; }
            set { _actionableContent = value; this.OnPropertyChanged(); }
        }

        private string _tipMessage;

        public string TipMessage
        {
            get { return _tipMessage; }
            set { _tipMessage = value; this.OnPropertyChanged(); }
        }


        private string _contentMD5;
        /// <summary>
        /// 内容MD5
        /// </summary>
        public string ContentMD5
        {
            get { return _contentMD5; }
            set { _contentMD5 = value; this.OnPropertyChanged(); }
        }

        private string _msgBrief;
        /// <summary>
        /// 消息概要
        /// 如：发送者+内容缩略
        /// </summary>
        public string MsgBrief
        {
            get { return _msgBrief; }
            set { _msgBrief = value; this.OnPropertyChanged(); }
        }

        private object _msgSource;
        /// <summary>
        /// 消息源
        /// </summary>
        public object MsgSource
        {
            get { return _msgSource; }
            set { _msgSource = value; this.OnPropertyChanged(); }
        }

        private bool _isAtMeMsg;
        /// <summary>
        /// 是否是@我的消息
        /// </summary>
        public bool IsAtMeMsg
        {
            get { return _isAtMeMsg; }
            set { _isAtMeMsg = value; this.OnPropertyChanged(); }
        }


        /// <summary>
        /// 目标对象
        /// </summary>
        public object Target { get; set; }

        private MessageStates _messageState;
        /// <summary>
        /// 消息状态
        /// </summary>
        public MessageStates MessageState
        {
            get { return _messageState; }
            set
            {
                _messageState = value; this.OnPropertyChanged();

            }
        }
        private string _retractId;
        public string RetractId
        {
            get { return _retractId; }
            set
            {
                _retractId = value; this.OnPropertyChanged();

            }
        }

        private int _isRead = -1;
        /// <summary>
        /// 是否已读
        /// </summary>
        public int IsRead
        {
            get { return _isRead; }
            set
            {
                _isRead = value;
                OnPropertyChanged();
            }
        }
        private bool _isEnabled = true;
        //转发是否禁用
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        private FileResourceModel _resourceModel = new FileResourceModel();
        /// <summary>
        /// 对应的文件资源模型
        /// </summary>
        public FileResourceModel ResourceModel
        {
            get { return _resourceModel; }
            set { _resourceModel = value; this.OnPropertyChanged(); }
        }

        private GroupNoticeModel _noticeModel = new GroupNoticeModel();
        /// <summary>
        /// 对应的公告类型
        /// </summary>
        public GroupNoticeModel NoticeModel
        {
            get { return _noticeModel; }
            set { _noticeModel = value; this.OnPropertyChanged(); }
        }

        private bool _canOperate = true;

        public bool CanOperate
        {
            get { return _canOperate; }
            set { _canOperate = value; this.OnPropertyChanged(); }
        }
        //private SDKClient.SDKProperty.SessionType
        private PersonCardModel _personCardModel = new PersonCardModel();
        /// <summary>
        /// 对应的名片类型
        /// </summary>
        public PersonCardModel PersonCardModel
        {
            get { return _personCardModel; }
            set { _personCardModel = value; this.OnPropertyChanged(); }
        }

    }
}
