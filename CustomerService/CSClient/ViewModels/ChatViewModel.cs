using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using IMModels;
using CSClient.Views.ChildWindows;
using SDKClient.Model;
using System.IO;
using CSClient.Helper;
using Util;
using System.Collections.Concurrent;
using CSClient.Views.Controls;

namespace CSClient.ViewModels
{
    /// <summary>
    /// 聊天项VM
    /// </summary>
    public class ChatViewModel : ViewModel
    {
        /// <summary>
        /// 聊天框最多显示消息数 120
        /// </summary>
        private readonly int MAXCOUNT = 120;
        /// <summary>
        /// 追加信息
        /// </summary>
        public event Action<MessageModel> AppendMessage;

        public event Action<string> OnFastReply;

        /// <summary>
        /// 来了新信息
        /// </summary>
        public event Action<MessageModel> NewMessage;
        public event Action<bool> OnStartOrStopSession;

        public event Action OnDisplayMsgHint;
        /// <summary>
        /// 是否显示右上角"有人@你"按钮
        /// </summary>
        public event Action OnDisplayAtButton;

        public ConcurrentDictionary<string, MessageModel> AtMeDic = new ConcurrentDictionary<string, MessageModel>();

        List<MessageModel> _unReadMsgs = new List<MessageModel>();
        /// <summary>
        /// 聊天项VM
        /// </summary>
        /// <param name="view"></param>
        public ChatViewModel(ChatModel model) : base(model)
        {
            Chat = model;
            this.IsGroup = model.IsGroup;
            this.IsShowGroupNoticeBtn = model.IsGroup;
            this.AtUserModel = AppData.Current.LoginUser.User;
        }
        

        /// <summary>
        /// 聊天项VM
        /// </summary>
        /// <param name="view"></param>
        public ChatViewModel(ChatModel model, MessageModel last) : this(model)
        {
            (this.Model as ChatModel).LastMsg = last;
        }

        public void FastReply(string content)
        {
            this.OnFastReply?.Invoke(content);
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
                this.SetLastMsg(msg, msg.Content);

            }));
            //this.HideMessage(msg);
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

        /// <summary>
        /// 是否已经激活加载过东西
        /// </summary>
        public bool HasActived { get; private set; }

        public string SessionId { get; set; }
        /// <summary>
        /// 会话类型：0 结束，1我的会话，2别人的会话
        /// </summary>
        public int sessionType { get; set; }
        public ChatModel Chat { get; }

        private CustomUserModel _customUserModel;
        /// <summary>
        /// 客户模型
        /// </summary>
       public CustomUserModel CustomUserModel
        {
            get { return _customUserModel; }
            set { _customUserModel = value; this.OnPropertyChanged(); }
        }


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

        private ViewModel _targetVM;
        /// <summary>
        /// 实际聊天对象VM，用户VM或组VM
        /// </summary>
        public ViewModel TargetVM
        {
            get { return _targetVM; }
            set { _targetVM = value; this.OnPropertyChanged(); }
        }

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
                if (value != _unReadCount)
                {
                    AppData.MainMV.UpdateUnReadMsgCount();
                }
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
                    _jumpCommand = new VMCommand(MainJupmToShopUrl);
                return _jumpCommand;
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
        /// 发送消息
        /// </summary>
        /// <param name="para"></param>
        private async void SendMsg(object para)
        {
            AppData.TestStart();

            if (AppData.CanInternetAction())
            {
                if (!await this.CanSendMsg())
                {
                    return;
                }

                List<int> _atUserIds = new List<int>();
                FlowDocument doc = para as FlowDocument;
                if (doc != null)
                {
                    _strB.Clear();
                    bool isNullOrEmpty = true;
                    int blockCount = 1;
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
                                    _atUserIds.Add(cn.Child.Uid.Split('|')[2].ToInt());
                                    _strB.Append(cn.Child.Uid.Split('|')[1] + " ");
                                }
                                else if (cn.Child.Uid.StartsWith(AppData.FlagImage))
                                {
                                    if (!string.IsNullOrEmpty(_strB.ToString()))
                                    {
                                        SendTextMsgToServer(_strB.ToString(), _atUserIds);
                                        _strB.Clear();
                                        _atUserIds.ToList().Clear();
                                    }

                                    value = cn.Child.Uid.Replace(AppData.FlagImage, string.Empty);
                                    SendImageMsgToServer(value);
                                }
                                else if (cn.Child.Uid.StartsWith(AppData.FlagFile))
                                {
                                    if (!string.IsNullOrEmpty(_strB.ToString()))
                                    {
                                        SendTextMsgToServer(_strB.ToString(), _atUserIds);
                                        _strB.Clear();
                                        _atUserIds.ToList().Clear();
                                    }

                                    value = cn.Child.Uid.Replace(AppData.FlagFile, string.Empty);
                                    AppData.MainMV.ShowTip("暂不支持发送文件格式的消息");
                                    // SendFileMsgToServer(value);
                                }
                            }
                            else if (item is Run run)
                            {
                                //文本
                                _strB.Append(run.Text);
                            }
                        }

                        if (doc.Blocks.Count > 1 && blockCount >= 1 && blockCount < doc.Blocks.Count)
                        {
                            _strB.Append("\r\n");
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
                        if (string.IsNullOrEmpty(result.Replace("\r\n", "")))
                        {
                            this.IsNullMsg = false;
                            this.IsNullMsg = true;
                        }
                        else
                        {
                            SendTextMsgToServer(result, _atUserIds);

                            this.AppendMessage.Invoke(null);
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

                    AppData.MainMV.ChatListVM.ResetSort();
                }
                doc.Blocks.Clear();
                _atUserIds.ToList().Clear();
            }
            _strB.Clear();

            AppData.TestStop("发送完毕");
        }

        private async Task<bool> CanSendMsg()
        {
            bool canSend = true;
            if (sessionType == 2)
            {
                AppData.MainMV.ShowTip("当前用户正在和其他客服进行沟通中");
                canSend = false;

            }
            else if(sessionType==0)
            {
                //Task.Run(async () =>
                //{

                    var r = await SDKClient.SDKClient.Instance.SendCustiomExchangeMsg(this.Model.ID.ToString(), SDKClient.SDKClient.Instance.property.CurrentAccount.CustomProperty.ServicerId ?? 0);
                    if (!r)
                    {
                        sessionType = 2;
#if !Release
                        this.AddMessageTip("申请会话失败，该用户被别人占用了", isSetLastMsg: false);
                        canSend = false;
#endif
                    }
                    else
                    {
#if !Release
                        this.AddMessageTip("申请会话成功，等待用户接入",isSetLastMsg:false);
#endif
                        sessionType = 1;
                    }
                    //});
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

        /// <summary>
        /// 重新发送
        /// </summary>
        /// <param name="msg"></param>
        public void ReSend(MessageModel msg)
        {
            if (CanSendMsg().Result && msg.Sender == AppData.Current.LoginUser.User)
            {
                this.Chat.Messages.Remove(msg);
                switch (msg.MsgType)
                {
                    default:
                        break;
                    case MessageType.txt:
                        this.SendTextMsgToServer(msg.Content, msg.Target as List<int>);
                        break;
                    case MessageType.img:
                        this.SendImageMsgToServer(msg.Content);
                        break;
                }
            }
        }

        public void SendTextMsgToServer(string content, List<int> userIds = null)
        {
            if (content == "\r\n")
            {
                return;
            }

            string destId = this.Model.ID.ToString();// this.IsGroupChat ? this.Group.Id.ToString() : this.Friend.Id.ToString();

            SDKClient.SDKProperty.chatType msgType = this.IsGroup ?
                SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;

            int groupID = this.IsGroup ? this.Model.ID : 0;

            MessageModel msg = new MessageModel()
            {
                Sender = AppData.Current.LoginUser.User,
                SendTime = DateTime.Now,
                IsMine = true,
                MsgType = MessageType.txt,
                Content = content,
                //P_fileName = m.fileName,
                //P_resourceId = m.resourceId,
                //P_fileSize = m.fileSize,
                //P_resourcesmallId = m.resourcesmallId,
                Target = userIds?.ToList(),
            };
            this.SetLastMsg(msg, msg.Content);
            SetMsgShowTime(msg);
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                (this.Model as ChatModel).Messages.Add(msg);
            }));


            Task.Run(async () => msg.MsgKey = SDKClient.SDKClient.Instance.Sendtxt(content, destId, this.IsGroup ? userIds : null, msgType));
        }


        private void SendImageMsgToServer(string imagePath)
        {
            string destId = this.Model.ID.ToString();// this.IsGroupChat ? this.Group.Id.ToString() : this.Friend.Id.ToString();

            SDKClient.SDKProperty.chatType msgType = this.IsGroup ?
                SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;

            int groupID = this.IsGroup ? this.Model.ID : 0;
            List<int> userIds = this.IsGroup ? new List<int>() : null;

            MessageModel msg = new MessageModel()
            {
                Sender = AppData.Current.LoginUser.User,
                SendTime = DateTime.Now,
                IsMine = true,
                MsgType = MessageType.img,
                MsgKey = string.Empty,
                Content = imagePath,
            };
            msg.ResourceModel.Key = imagePath.Split('\\').LastOrDefault();

            msg.OperateTask = new System.Threading.CancellationTokenSource();

            Task.Run(async () => await SDKClient.SDKClient.Instance.SendImgMessage(imagePath, (m) =>
            {
                if (m.isSuccess == 1)
                {
                    msg.MessageState = MessageStates.Success;
                    //msg.MsgKey = m.imgMD5;
                    msg.MsgKey = m.msgId;
                    msg.ResourceModel.Key = m.imgMD5;
                    msg.ResourceModel.SmallKey = m.smallId;
                }
                else
                {
                    msg.MessageState = MessageStates.Fail;
                }

            }, destId, msgType, groupID, msg.OperateTask.Token));

            this.SetLastMsg(msg, "[图片]");
            SetMsgShowTime(msg);
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                (this.Model as ChatModel).Messages.Add(msg);
            }));
        }

        private void SendFileMsgToServer(string filePath)
        {
            string destId = this.Model.ID.ToString();// this.IsGroupChat ? this.Group.Id.ToString() : this.Friend.Id.ToString();

            SDKClient.SDKProperty.chatType msgType = this.IsGroup ?
                SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;

            int groupID = this.IsGroup ? this.Model.ID : 0;
            List<int> userIds = this.IsGroup ? new List<int>() : null;

            FileResourceModel file = new FileResourceModel(filePath);
            file.DBState = 0;
            file.FileImg = Helper.FileHelper.GetFileImage(filePath, false);// WindowsThumbnailProvider.GetFileThumbnail(filePath); 
            MessageModel msg = new MessageModel()
            {
                Sender = AppData.Current.LoginUser.User,
                SendTime = DateTime.Now,
                IsMine = true,
                MsgType = MessageType.file,
                Content = filePath,
                ResourceModel = file,
            };


            this.SetLastMsg(msg, "[文件]");
            SetMsgShowTime(msg);

            msg.IsSending = true;

            //UpLoadFile(msg);

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                (this.Model as ChatModel).Messages.Add(msg);
            }));
            if (!this.IsGroup)
            {
                this.WarningInfo = "如果对方长时间未接收，建议您发送离线文件";
            }
        }

        /// <summary>
        /// 离线上传文件
        /// </summary>
        /// <param name="msg"></param>
        public void SendOfflineFile(MessageModel msg, System.Threading.CancellationTokenSource operate, Action<bool> callback)
        {
            if (msg == null || !File.Exists(msg.Content))
            {
                return;
            }
            string destId = this.Model.ID.ToString();
            msg.ResourceModel.FileState = FileStates.SendOffline;
            SDKClient.SDKProperty.chatType chatType = this.IsGroup ?
              SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;
            var tension = System.IO.Path.GetExtension(msg.ResourceModel.PreviewImagePath).ToLower();
            string ImageFilter = @"图片|*.gif;*.jpeg;*.jpg;*.png;*.bmp";
            bool isImage = false;
            if (ImageFilter.Contains(tension))
                isImage = true;
            SDKClient.SDKClient.Instance.SendFileMessage(msg.Content,null,
                (result) =>
                {
                    if (operate.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    msg.IsSending = false;
                    //msg.ResourceModel.FileName = result.imgMD5;
                    msg.ContentMD5 = result.fileMD5;
                    msg.ResourceModel.Key = result.msgId;
                    msg.ResourceModel.PreviewKey = result.imgId;
                    if (result.isSuccess == 1)
                    {
                        msg.ResourceModel.CompleteLength = msg.ResourceModel.Length;
                        msg.ResourceModel.FileState = FileStates.Completed;
                        msg.MsgKey = result.imgId;

                        callback?.Invoke(true);
                    }
                    else
                    {
                        if (msg.MessageState != MessageStates.Fail)
                        {
                            msg.MsgType = MessageType.notification;

                            msg.ResourceModel.FileState = FileStates.Fail;
                            msg.MessageState = MessageStates.Fail;
                            msg.ContentMD5 = msg.Content;

                            string tip = result.errorState.Description();//.SafeString();

                            //switch (result.errorState)
                            //{
                            //    case SDKClient.SDKProperty.ErrorState.AppError:
                            //        tip = "程序异常";
                            //        break;
                            //    case SDKClient.SDKProperty.ErrorState.Cancel:
                            //        tip = "操作取消";
                            //        break;
                            //    case SDKClient.SDKProperty.ErrorState.NetworkException:
                            //        tip = "网络异常";
                            //        break;
                            //    case SDKClient.SDKProperty.ErrorState.OutOftheControl:
                            //        tip = "文件大小超过限制";
                            //        break;
                            //    case SDKClient.SDKProperty.ErrorState.ServerException:
                            //        tip = "服务器异常";
                            //        break;
                            //}
                            string size = Helper.FileHelper.FileSizeToString(msg.ResourceModel.Length);
                            //msg.Content = $"文件服务器连接异常，您取消了\"{msg.ResourceModel.FileName}\"({size})的发送，文件传输失败；";
                            msg.Content = $"{tip}，您中断了离线文件\"{msg.ResourceModel.FileName}\"({size})的发送";
                            //fileVM.State = FileState.UploadFailed;
                            //msg.ResultVisibility = true;
                            //msg.Result = 0;
                            //msg.msgId = o.imgId; 

                            callback?.Invoke(false);
                        }

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
                },
               destId, chatType, operate.Token, imgFullName: isImage ? msg.ResourceModel.PreviewImagePath : string.Empty);

            msg.CanOperate = true;
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
                          //msg.ResourceModel.FileName = result.imgMD5;
                          msg.ContentMD5 = result.imgMD5;
                          msg.ResourceModel.Key = result.imgId;

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
                              //fileVM.State = FileState.UploadFailed;
                              //msg.ResultVisibility = true;
                              //msg.Result = 0;
                              //msg.msgId = o.imgId; 

                              callBack?.Invoke(false);
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

                int index = (this.Model as ChatModel).Messages.IndexOf(msg);

                (this.Model as ChatModel).Messages.Remove(msg);

                string tip = "您撤回了一条消息";

                MessageModel messageModel = new MessageModel();
                messageModel.MsgKey = string.Empty;
                messageModel.MsgType = MessageType.notification;
                messageModel.Content = tip;
                messageModel.SendTime = DateTime.Now;
                (this.Model as ChatModel).Messages.Insert(index, messageModel);

                this.SetLastMsg(messageModel, tip, false);

                SDKClient.SDKProperty.chatType messageType = this.IsGroup ? SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;
                if (!string.IsNullOrEmpty(msg.MsgKey))
                {
                    SDKClient.SDKClient.Instance.SendRetractMessage(msg.MsgKey, this.ID.ToString(), messageType, this.IsGroup ? this.ID : 0);
                }
            });
        }

        public void ReceiveWithDrawMsg(MessageModel msg, string displayName, bool isSync, bool isUnRead, bool? isMediaResource = null)
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

                MessageModel messageModel = new MessageModel();
                messageModel.MsgKey = string.Empty;
                messageModel.MsgType = MessageType.notification;
                messageModel.SendTime = DateTime.Now;

                if (isMediaResource == true)//是否在线
                {
                    if (msg.MessageState == MessageStates.Fail)
                    {
                        return; //已经是失败状态，不再提示
                    }
                    string size = Helper.FileHelper.FileSizeToString(msg.ResourceModel.Length);
                    if (msg.ResourceModel != null && msg.ResourceModel.RefInfo != null)
                    {
                        tip = $"对方取消了\"{ msg.ResourceModel.FileName}\"({size})的发送，文件传输失败。";
                    }
                    else
                    {
                        tip = $"对方取消了\"{ msg.ResourceModel.FileName}\"({size})的接收，文件传输失败。";
                    }
                    messageModel.MessageState = MessageStates.Fail;
                    messageModel.MsgKey = msg.MsgKey;

                    msg.MessageState = MessageStates.Fail;
                    msg.MsgType = MessageType.notification;
                    msg.Content = tip;

                    var list = Views.Controls.FileChatItem.AcioningItems.ToList();
                    foreach (var item in list)
                    {
                        if (item.DataContext is MessageModel target && target.MsgKey == msg.MsgKey)
                        {
                            item.Cancel(true);
                        }
                    }
                }
                else
                {
                    if ((this.Model as ChatModel).IsGroup)
                    {
                        tip = "[" + displayName + "]" + "撤回了一条消息";
                    }
                    if (isSync)
                    {
                        tip = "您撤回了一条消息";
                    }
                    msg.MsgType = MessageType.notification;
                    msg.Content = tip;
                }

                messageModel.Content = tip;

                if (!isUnRead)
                {
                    int index = (this.Model as ChatModel).Messages.IndexOf(msg);
                    (this.Model as ChatModel).Messages.Remove(msg);

                    (this.Model as ChatModel).Messages.Insert(index, messageModel);
                }

                if (this.UnReadCount > 0 && !isSync)
                {
                    this.UnReadCount--;
                }

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

                this.AtMeDic.TryRemove(msg.MsgKey, out msg);
                if (this.AtMeDic.Count == 0)
                {
                    this.IsDisplayAtButton = false;
                    this.HasAtMsg = false;
                }

                this.SetLastMsg(messageModel, tip, false);
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
            if (chatModel.Chat is UserModel)
            {
                return;
            }
            else
            {
                GroupModel group = (GroupModel)chatModel.Chat;
                if (group.OwnerID == AppData.MainMV.LoginUser.ID)
                {
                    string newValue = EditStringValueWindow.ShowInstance(group.DisplayName, "修改群名称");

                    string value = string.Format("{0}", newValue).Trim();
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

                            //IMUI.View.V2.MessageTip.ShowTip("修改群名称失败", IMUI.View.V2.TipTypes.Error);
                            //(sender as TextBox).Text = groupModel.Name;  
                        }
                    }
                }
                else
                {
                    return;
                }
            }
        }

        /// <summary>
        /// 删除聊天
        /// </summary>
        /// <param name="para"></param>
        void DeleteChat(object para)
        {
            ChatModel chatModel = this.Model as ChatModel;

            bool? result = App.IsCancelOperate("删除聊天", "您有文件正在传输中，确定终止文件传输吗？", chatModel.ID);
            if (result == true)
            {
                return;
            }

            if (chatModel.Chat is UserModel)
            {
                UserModel userModel = chatModel.Chat as UserModel;
                SDKClient.SDKClient.Instance.UpdateHistoryMsgIsReaded(userModel.ID, 0, true);
                SDKClient.SDKClient.Instance.UpdateChatRoomVisibility(userModel.ID, 0, false);
                if(sessionType==1)
                   Task.Run(async()=>await SDKClient.SDKClient.Instance.SendCustiomServerMsg(this.ID.ToString(), this.SessionId, SDKClient.SDKProperty.customOption.over));
            }

            //chatModel.Messages.Clear();
            //删除聊天条目
            AppData.MainMV.ChatListVM.Items.Remove(this);
            //更新未读消息数量
            AppData.MainMV.UpdateUnReadMsgCount();
        }

        /// <summary>
        /// 主页跳转到群信息页面
        /// </summary>
        /// <param name="para"></param>
        private void MainJupmToShopUrl(object para)
        {
            ChatModel model = this.Model as ChatModel;
            if (model != null&&!string.IsNullOrEmpty(this.CustomUserModel.ShopBackUrl))
            {
                System.Diagnostics.Process.Start(this.CustomUserModel.ShopBackUrl);
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
        /// 设置最新消息
        /// </summary>
        /// <param name="msg">最新消息条目</param>
        /// <param name="tip">消息内容tip</param>
        /// <param name="isAddTimeMsg">是否加一条时间消息</param>
        public void SetLastMsg(MessageModel msg, string tip, bool isAddTimeMsg = true)
        {
            try
            {
                if (isAddTimeMsg)
                {
                    SetMsgShowTime(msg);
                }
                if (this.IsDisplayStartSession && !string.IsNullOrEmpty(msg.Content) && msg.Content.Equals("接入聊天"))
                {
                    return;
                }
                IChat chat = msg.Sender;

                if (this.IsGroup && chat != null)
                {
                    UserModel user = AppData.Current.GetUserModel(chat.ID);
                    GroupMember member = user.GetInGroupMember((this.Model as ChatModel).Chat as GroupModel);
                    chat = member;
                }
                Chat.LastMsg = new MessageModel()
                {
                    Sender = chat,
                    Content = tip,
                    SendTime = msg.SendTime,
                    MsgType = msg.MsgType,
                };
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 活跃状态，即当前显示
        /// </summary>
        public void Acitve()
        {
            this.HasAtMsg = false;
            this.IsDisplayStartSession = false;

            if (!this.HasActived)
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.Chat.Messages.Clear();
                }));

                if (_unReadMsgs.Count == 0)
                {
                    this.LoadHisMessages();
                }
                else
                {
                    this.IsDisplayAtButton = false;

                    List<MessageModel> newMsgs = _unReadMsgs.ToList();
                    _unReadMsgs.Clear();
                    int showCount = newMsgs.Count + Chat.Messages.Count;
                    //大于最大数值，先清空 只显示新消息
                    if (newMsgs.Count >= MAXCOUNT)
                    {
                        int count = newMsgs.Count;
                        Chat.Messages.Clear();

                        newMsgs = newMsgs.Skip(count - MAXCOUNT).Take(MAXCOUNT).ToList();
                    }
                    else if (showCount > MAXCOUNT)
                    {
                        //允许旧的继续呈现数量
                        int oldCount = MAXCOUNT - newMsgs.Count;

                        while (Chat.Messages.Count > oldCount)
                        {
                            Chat.Messages.RemoveAt(0);
                        }
                        newMsgs = newMsgs.ToList();
                    }
                    AddNewMessageFlag();

                    foreach (var m in newMsgs)
                    {
                        SetMsgShowTime(m);
                        (this.Model as ChatModel).Messages.Add(m);
                    }

                    if (newMsgs.Count > 0)
                    {
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            this.OnDisplayMsgHint?.Invoke();
                        }));
                    }

                    UnReadCount = 0;
                }

                this.HasActived = true;
            }
            else
            {
                this.IsDisplayAtButton = false;

                List<MessageModel> newMsgs = _unReadMsgs.ToList();
                _unReadMsgs.Clear();
                int showCount = newMsgs.Count + Chat.Messages.Count;
                //大于最大数值，先清空 只显示新消息
                if (newMsgs.Count >= MAXCOUNT)
                {
                    int count = newMsgs.Count;
                    Chat.Messages.Clear();

                    newMsgs = newMsgs.Skip(count - MAXCOUNT).Take(MAXCOUNT).ToList();
                }
                else if (showCount > MAXCOUNT)
                {
                    //允许旧的继续呈现数量
                    int oldCount = MAXCOUNT - newMsgs.Count;

                    while (Chat.Messages.Count > oldCount)
                    {
                        Chat.Messages.RemoveAt(0);
                    }
                    newMsgs = newMsgs.ToList();
                }
                AddNewMessageFlag();

                foreach (var m in newMsgs)
                {
                    SetMsgShowTime(m);
                    (this.Model as ChatModel).Messages.Add(m);
                }

                if (newMsgs.Count > 0)
                {
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        this.OnDisplayMsgHint?.Invoke();
                    }));
                }


                UnReadCount = 0;

                //过程中若有更新的消息，则递归
                if (_unReadMsgs.Count > 0)
                {
                    this.Acitve();
                }
            }

            int roomType = this.IsGroup ? 1 : 0;
            SDKClient.SDKClient.Instance.UpdateHistoryMsgIsReaded(this.ID, roomType, true);

            _unReadMsgs.Clear();
        }

        /// <summary>
        /// 添加一条“以下为新消息”的提示信息
        /// </summary>
        public void AddNewMessageFlag()
        {
            if (this.UnReadCount >= 10)
            {
                this.UnReadMsgTip = string.Format("{0}条消息", this.UnReadCount);
                this.IsDisplayHistoryMsgButton = true;
            }
            else
            {
                this.IsDisplayHistoryMsgButton = false;
                return;
            }

            var msg = Chat.Messages.FirstOrDefault(x => x.MsgType == MessageType.notification && x.Content == ConstString.FollowingIsNewMessage);
            if (msg != null)
            {
                (this.Model as ChatModel).Messages.Remove(msg);
            }

            var hintMsg = new MessageModel();
            hintMsg.MsgType = MessageType.notification;
            hintMsg.Content = ConstString.FollowingIsNewMessage;
            hintMsg.SendTime = DateTime.Now;
            hintMsg.MsgKey = string.Empty;

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Chat.Messages.Add(hintMsg);
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

        /// <summary>
        /// 加载历史消息
        /// </summary>
        /// <param name="isForward">是否向前滚动追加</param>
        public void LoadHisMessages(bool isForward = false)
        {
            var msgs = (this.Model as ChatModel).Messages;
            if (_loadHisTask != null && _loadHisTask.Status != TaskStatus.RanToCompletion)
            {
                return;
            }

            if (msgs.Count >= MAXCOUNT)
            {
                this.IsFullPage = true;
                return;
            }
            _loadHisTask = Task.Run(() =>
            {
                List<SDKClient.DTO.MessageEntity> datas;
                MessageModel top = null;

                top = msgs.Where(x => !string.IsNullOrEmpty(x.MsgKey)).FirstOrDefault();
                if (isForward)
                {
                    //datas = SDKClient.SDKClient.Instance.GetHistoryMsg(this.Model.ID, top.MsgKey, 10);

                    if (msgs.Count > 1 && top != null)
                    {
                        DateTime sendTime = top.SendTime;
                        if (sendTime == null)
                        {
                            sendTime = DateTime.Now;
                        }
                        datas = SDKClient.SDKClient.Instance.GetHistoryMsgEntity(this.Model.ID, top.MsgKey, 10, sendTime);
                    }
                    else
                    {
                        datas = SDKClient.SDKClient.Instance.GetHistoryMsgEntity(this.Model.ID, string.Empty, 10, DateTime.Now);
                    }

                    if (datas.Count == 0)
                    {
                        WarningInfo = "没有更多消息";
                        return;
                    }
                }
                else
                {
                    int queryMsgCount = this.UnReadCount;//默认加载全部未读消息
                    if (this.UnReadCount >= 0 && this.UnReadCount <= 6)
                    {
                        queryMsgCount = 6;//没有未读消息或者未读消息数<=6条时，加载最近的6条历史消息
                    }
                    else if (this.UnReadCount >= MAXCOUNT)
                    {
                        AddNewMessageFlag();
                        queryMsgCount = MAXCOUNT;//未读消息>=300条时，加载最近的300条未读消息
                    }
                    else
                    {
                        AddNewMessageFlag();
                    }
                    //datas = SDKClient.SDKClient.Instance.GetHistoryMsg(this.Model.ID, queryMsgCount);
                    //datas = SDKClient.SDKClient.Instance.GetHistoryMsgEntity(this.Model.ID, queryMsgCount);
                    datas = new List<SDKClient.DTO.MessageEntity>();
                    UnReadCount = 0;
                }

                int fromId;
                foreach (var m in datas)
                {
                    System.Threading.Thread.CurrentThread.Join(5);
                    if (int.TryParse(m.From, out fromId) && fromId >= 0)
                    {
                        if ((this.Model as ChatModel).Messages.Any(x => x.MsgKey == m.MsgId))//重复消息
                        {
                            continue;
                        }

                        #region 消息类型
                        MessageType type;
                        try
                        {
                            type = (MessageType)Enum.Parse(typeof(MessageType), m.MsgType.ToLower());
                        }
                        catch
                        {
                            continue;
                        }
                        if (type == MessageType.redenvelopesreceive || type == MessageType.retract)////不显示对方接收的红包和自己发送的红包消息
                            continue;
                        #endregion

                        IChat sender;

                        object target = null;
                        // AppData.Current.LoginUser.User.ID == fromId;
                        //if (isMine) //我发送的消息，统一放在右边
                        //{
                        //    sender = AppData.Current.LoginUser.User;
                        //}
                        //else

                        if (this.IsGroup) //群成员发送的消息，要现实的是成员在  我给好友备注的昵称>群成员自己设置的昵称>群成员名称
                        {
                            UserModel user = AppData.Current.GetUserModel(fromId);
                            GroupModel group = AppData.Current.GetGroupModel(m.RoomId);
                            sender = user.GetInGroupMember(group);
                            if (string.IsNullOrEmpty(user.Name))
                            {
                                SDKClient.SDKClient.Instance.GetGroupMember(AppData.Current.LoginUser.ID, group.ID, fromId);
                            }
                        }
                        else //单聊好友发送的消息,只显示头像，不显示昵称（聊天框已有昵称显示）
                        {
                            //sender= AppData.Current.GetUserModel(fromId);
                            var user = AppData.Current.GetUserModel(fromId);
                            //if (string.IsNullOrEmpty(user.DisplayName))
                            //{
                            //    user.DisplayName = user.PhoneNumber;
                            //}

                            //if(fromId != this.Model.ID)
                            //{

                            //}

                            sender = user;
                            //if (type == MessageType.invitejoingroup)
                            //{
                            //    target = LoadGroupCard(m.Source, fromId);
                            //    if (target == null)
                            //    {
                            //        continue;
                            //    }
                            //}
                        }

                        MessageModel msg = new MessageModel()
                        {
                            MsgKey = m.MsgId,
                            Sender = sender,
                            SendTime = m.MsgTime,
                            IsMine = fromId != this.Model.ID,
                            MsgType = type,
                            Target = target,
                        };
                        if (msg.IsMine)
                        {
                            msg.Sender.HeadImg = AppData.Current.LoginUser.User.HeadImg;
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
                                        if (this._unReadMsgs.Any(x => x.MsgKey.Equals(m.MsgId)))
                                        {
                                            this.AtMeDic.TryAdd(m.MsgId, msg);
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        msg.ResourceModel = new FileResourceModel()
                        {
                            Key = m.ResourceId,
                            SmallKey = m.ResourcesmallId,
                            Length = m.FileSize,
                            FileName = Path.GetFileName(m.FileName),
                            //如果文件名包含绝对路径，那么不从manjinba/file路径中文件
                            FullName = m.FileName,
                        };
                        string info = string.Empty;
                        switch (type)
                        {
                            case MessageType.img:
                                info = "[图片]";
                                break;
                            case MessageType.file:
                                info = "[文件]";
                                msg.Content = m.FileName;
                                break;
                            case MessageType.onlinefile:

                                if (File.Exists(m.Content))
                                {

                                    msg.Content = m.Content;
                                    msg.MsgType = MessageType.file;
                                    msg.ResourceModel.FullName = msg.Content;
                                }
                                else
                                {
                                    var package = Util.Helpers.Json.ToObject<SDKClient.Model.MessagePackage>(m.Data);
                                    var body = package.data.body;

                                    string onlinePath = m.FileName;
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
                                        }
                                    };
                                    msg.ResourceModel = onlineFile;
                                    msg.MsgType = MessageType.file;
                                }
                                info = "[文件]";
                                break;
                            case MessageType.invitejoingroup:
                                info = "[群名片]";
                                break;
                            case MessageType.audio:
                                info = "[语音]";
                                msg.Content = msg.IsMine ? "您发送了一条语音消息，请在手机端查看" : "对方发送语音消息，请在手机端查看";
                                break;
                            case MessageType.video:
                                info = "[视频]";
                                msg.Content = msg.IsMine ? "您发送了一条视频消息，请在手机端查看" : "对方发送视频消息，请在手机端查看";
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
                                info = "[群公告]";
                                msg.MsgType = MessageType.notification;
                                break;
                            case MessageType.bigtxt:
                                break;
                            case MessageType.setmemberpower:
                                var p = Util.Helpers.Json.ToObject<SetMemberPowerPackage>(m.Data);
                                this.ShowAddMemberPowerMsg(p, isForward, false);
                                continue;
                            case MessageType.dismissgroup:

                                var dismissGroupPackage = Util.Helpers.Json.ToObject<DismissGroupPackage>(m.Data);
                                this.ShowDismissMsg(dismissGroupPackage, isForward);
                                continue;
                            case MessageType.exitgroup:
                                this.AddMessageTip(m.Content, m.MsgTime, isForward, m.MsgId);
                                continue;
                            case MessageType.notification:
                                info = msg.Content = m.Content;
                                int result = m.OptionRecord & (int)SDKClient.SDKProperty.MessageState.cancel;
                                //if (msg.Content != null && result == 4)
                                //{
                                //    msg.MessageState = MessageStates.Fail;
                                //}

                                if (msg.Content.Contains("失败"))
                                {
                                    msg.MessageState = MessageStates.Fail;
                                }
                                else if (msg.Content.Contains("成功"))
                                {
                                    msg.MessageState = MessageStates.Success;
                                }
                                break;
                            case MessageType.goods:
                                info = "[商品链接]";
                                msg.MsgType = MessageType.txt;
                                msg.Content = m.Data.afterurl;
                                break;
                            case MessageType.order:
                                info = "[订单链接]";
                                msg.MsgType = MessageType.txt;
                                msg.Content = m.Data.afterurl;
                                break;
                            case MessageType.custom:
                                info = "[商品链接]";
                                msg.MsgType = MessageType.txt;
                                msg.Content = m.Data.afterurl;
                                break;
                            default:
                                info = msg.Content = m.Content;
                                break;
                        }
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            if (isForward)
                            {
                                SetMsgShowTime(msg, false);
                                msgs.Insert(0, msg);
                                this.AppendMessage?.Invoke(top);
                            }
                            else
                            {
                                if (m == datas.Last())
                                {
                                    this.SetLastMsg(msg, info);
                                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        AppData.MainMV.ChatListVM.ResetSort();
                                    });
                                }

                                SetMsgShowTime(msg);
                                msgs.Add(msg);
                                this.AppendMessage?.Invoke(null);

                            }
                        }));
                    }
                }

                if (!isForward)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        this.OnDisplayAtButton?.Invoke();
                    });
                }
            });
        }

        public void StartOrStopSession(bool isStart)
        {
            this.IsSessionEnd = !isStart;
            this.OnStartOrStopSession?.Invoke(isStart);
        }

        /// <summary>
        /// 添加设置成员权限的通知
        /// </summary>
        /// <param name="package"></param>
        private void ShowAddMemberPowerMsg(SetMemberPowerPackage package, bool isForward = false, bool isNewMessage = true)
        {
            string info = string.Empty;
            foreach (var item in package.data.userIds)
            {
                UserModel user = AppData.Current.GetUserModel(item);
                var group = this.Chat.Chat as GroupModel;
                var sender = user.GetInGroupMember(group);

                if (string.IsNullOrEmpty(user.Name))
                {
                    //SDKClient.SDKClient.Instance.GetUser(item);
                }

                if (package.data.type == "admin")
                {
                    if (item == AppData.MainMV.LoginUser.ID)
                    {
                        info = "你成为群管理员";
                    }
                    else
                    {
                        info = string.Format("[{0}] 成为群管理员", sender.DisplayName);
                    }
                }
                else
                {
                    if (item == AppData.MainMV.LoginUser.ID)
                    {
                        info = "你被取消群管理员";
                    }
                    else
                    {
                        info = string.Format("[{0}] 被取消群管理员", sender.DisplayName);
                    }
                }

                this.AddMessageTip(info, package.time, isForward, package.id);
            }
        }

        private void ShowDismissMsg(DismissGroupPackage package, bool isForward = false)
        {
            //群成员收到解散群的包时 
            this.AddMessageTip("该群已经被解散！", package.time, isForward, package.id);
        }

        public void LoadHisMessage(SDKClient.DB.messageDB m, int fromId)
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

                    if (type == MessageType.invitejoingroup)
                    {
                        target = LoadGroupCard(m.Source, fromId);
                        if (target == null)
                        {
                            return;
                        }
                    }
                }



                MessageModel msg = new MessageModel()
                {
                    MsgKey = m.msgId,
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
                        };
                        msg.ResourceModel = file;
                        break;
                    case MessageType.invitejoingroup:
                        info = "[群名片]";
                        break;
                    case MessageType.audio:
                        info = "[语音]";
                        msg.Content = msg.IsMine ? "您发送了一条语音消息，请在手机端查看" : "对方发送语音消息，请在手机端查看";
                        break;
                    case MessageType.video:
                        info = "[视频]";
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
                        msg.MsgType = MessageType.notification;
                        info = "[群公告]";
                        break;
                    default:
                        info = msg.Content;
                        break;
                }


                if ((this.Model as ChatModel).Messages != null && AppData.MainMV.ChatListVM.SelectedItem == this)
                {
                    this.SetLastMsg(msg, info);

                    SDKClient.SDKClient.Instance.UpdateHistoryMsgIsReaded(m.msgId);

                    SetMsgShowTime(msg);
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
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
                    this.SetLastMsg(msg, info, false);

                    _unReadMsgs.Add(msg);
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
                model.HeadImg = CSClient.Helper.ImageHelper.GetFriendFace(p.data.groupPhoto, (s) => model.HeadImg = s);
                model.AppendID = fromID;
                return model;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 新消息
        /// </summary>
        /// <param name="package"></param>
        /// <param name="fromID"></param>
        public void ReceiveNewMessage(SDKClient.Model.MessagePackage package, int fromID)
        {
            if ((this.Model as ChatModel).Messages.Any(m => m.MsgKey == package.id))//重复消息
            {
                return;
            }

            //信息类型            
            MessageType type;
            try
            {
                type = (MessageType)Enum.Parse(typeof(MessageType), package.data.subType.ToLower());
            }
            catch
            {
                //Views.MessageBox.ShowBox("收到未知消息类型：" + package.data.subType);
                return;
            }

            IChat sender;

            //if (package.data.groupInfo != null)
            //{
            //    //GroupModel group = AppData.Current.GetGroupModel(package.data.groupInfo.groupId);
            //    //UserModel user = AppData.Current.GetUserModel(fromID);

            //    //if (group.Members != null && !group.Members.Any(m => m.ID == fromID)) //若发送人不在群中（断线过程中加入的人）
            //    //{
            //    //    var groupVM = AppData.MainMV.GroupListVM.Items.FirstOrDefault(g => g.ID == group.ID);
            //    //    groupVM?.GetGroupMemberList(true);
            //    //}
            //    //sender = user.GetInGroupMember(group);

            //    //if (string.IsNullOrEmpty(sender.DisplayName))
            //    //{
            //    //    sender.DisplayName = package.data.senderInfo.userName;
            //    //    //SDKClient.SDKClient.Instance.GetUser(fromID); 
            //    //    SDKClient.SDKClient.Instance.GetGroupMember(AppData.Current.LoginUser.ID, group.ID, fromID);
            //    //}
            //}
            //else
            
            sender = AppData.Current.GetUserModel(fromID);
            if (sender != null && package.data.senderInfo != null && (string.IsNullOrEmpty(sender.HeadImg) || sender.HeadImg.Equals(ImagePathHelper.DefaultUserHead)))
            {
                this.Chat.Chat.HeadImg = sender.HeadImg = package.data.senderInfo.photo ?? ImagePathHelper.DefaultUserHead;
            }
            if (type == MessageType.invitejoingroup)
            {
                object target = LoadGroupCard(package.data.body, fromID);
                if (target == null)
                {
                    return;
                }
            }
            

            MessageModel msg = new MessageModel()
            {
                Sender = sender,
                SendTime = package.time ?? DateTime.Now,
                IsMine = fromID == AppData.Current.LoginUser.User.ID,
                MsgType = type,
                Content = package.data.body.text,
                //P_fileName = package.data.body.fileName,
                //P_resourceId = package.data.body.id,
                //P_fileSize = package.data.body.fileSize,
                MsgKey = package.id,
                MsgSource = package.data.body,
                //P_resourcesmallId = m.resourcesmallId,
            };

            if (package.data.tokenIds != null && package.data.tokenIds.Count > 0)
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
            if (type == MessageType.eval)
            {
                msg.Content = "[对方已评价]";
                AddMessage(msg, "[对方已评价]", package);
                this.SendTextMsgToServer("感谢您的评价");
                return;
            }
            var body = package.data.body;
            string info = string.Empty;
            switch (type)
            {
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
                    //public int fileSize { get; set; }
                    //public string id { get; set; }
                    //public string fileName { get; set; }
                    //public string IP { get; set; }
                    //public int Port { get; set; }

                    //string onlinePath = FileHelper.GetFileName(System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.filePath, onlineName));

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
                    //string path = FileHelper.GetFileName(System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.filePath, fileName));

                    //FileResourceModel file = new FileResourceModel(path);

                    //msg.ResourceModel.Key = body.id;
                    //msg.ResourceModel.Length = body.fileSize;
                    //msg.ResourceModel.SmallKey = body.resourcesmallId;

                    FileResourceModel file = new FileResourceModel()
                    {
                        Key = body.id,
                        SmallKey = body.resourcesmallId,
                        Length = body.fileSize,
                        FileName = fileName,
                        //如果文件名包含绝对路径，那么不从manjinba/file路径中文件
                        FullName = filePath,
                    };
                    msg.ResourceModel = file;

                    info = "[文件]";
                    break;
                case MessageType.invitejoingroup:
                    info = "[群名片]";
                    break;
                case MessageType.audio:
                    info = "[语音]";
                    msg.Content = msg.IsMine ? "您发送了一条语音消息，请在手机端查看" : "对方发送语音消息，请在手机端查看";
                    break;
                case MessageType.video:
                    info = "[视频]";
                    msg.Content = msg.IsMine ? "您发送了一条视频消息，请在手机端查看" : "对方发送视频消息，请在手机端查看";
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
                    msg.MsgType = MessageType.notification;
                    info = "[群公告]";
                    break;
                case MessageType.bigtxt:
                    BigBody bb = new BigBody()
                    {
                        partName = package.data.body.partName,
                        partOrder = package.data.body.partOrder,
                        partTotal = package.data.body.partTotal,
                        text = package.data.body.text
                    };

                    int index = bb.partOrder;
                    if (index == 0)
                    {
                        BigtxtHelper.AddBigtxtMsg(bb, s =>
                        {
                            //大文本合包完成后，在回调中单独处理后续逻辑
                            if ((this.Model as ChatModel).Messages.Any(m => m.MsgKey == package.id) ||
                                _unReadMsgs.Any(m => m.MsgKey == package.id)) //重复消息
                            {
                                return;
                            }

                            msg.Content = s;
                            info = bb.text;
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
                    if (Chat.LastMsg != null && Chat.LastMsg.MsgKey == retractId)
                    {
                        return;
                    }
                    bool? isMediaResource = package.data.body.isMediaResource;
                    MessageModel retractMsg = (this.Model as ChatModel).Messages.ToList().FirstOrDefault(x => x != null && x.MsgKey != null && x.MsgKey.Equals(retractId));

                    if (retractMsg != null)
                    {
                        ReceiveWithDrawMsg(retractMsg, sender.DisplayName, package.syncMsg == 1 ? true : false, false, isMediaResource);
                    }
                    else
                    {

                        retractMsg = this._unReadMsgs.ToList().FirstOrDefault(x => x != null && x.MsgKey != null && x.MsgKey.Equals(retractId));
                        if (retractMsg != null)
                        {
                            ReceiveWithDrawMsg(retractMsg, sender.DisplayName, package.syncMsg == 1 ? true : false, true, isMediaResource);
                        }
                        else
                        {
                            string tip = "对方撤回了一条消息";


                            if (isMediaResource == true)
                            {
                                tip = $"对方取消了在线文件的发送";
                            }

                            this.HasAtMsg = false;
                            MessageModel messageModel = new MessageModel();
                            messageModel.MsgKey = string.Empty;
                            messageModel.MsgType = MessageType.notification;
                            messageModel.SendTime = DateTime.Now;

                            if ((this.Model as ChatModel).IsGroup)
                            {
                                tip = sender.DisplayName + "撤回了一条消息";
                            }

                            if (package.syncMsg == 1)
                            {
                                tip = "您撤回了一条消息";
                            }

                            if (this.UnReadCount > 0)
                            {
                                this.UnReadCount--;
                            }

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

                            this.SetLastMsg(messageModel, tip, false);
                        }
                    }
                    return;
                case MessageType.goods:
                    msg.MsgType = MessageType.txt;
                    string goodsAfterurl = package.data.body.afterurl;

                    //GoodsBody gb = new GoodsBody()
                    //{
                    //    goodsid = package.data.body.goodsid,
                    //    goodspic = package.data.body.goodspic,
                    //    goodsname = package.data.body.goodsname,
                    //    goodsprice = package.data.body.goodsprice,
                    //    mfronturl = package.data.body.mfronturl,
                    //    afterurl = package.data.body.afterurl
                    //};
                    info = "[商品链接]";
                    msg.Content = goodsAfterurl;
                    break;
                case MessageType.order:
                    msg.MsgType = MessageType.txt;
                    string orderAfterurl = package.data.body.afterurl;
                    //OrderBody ob = new OrderBody()
                    //{
                    //    orderno = package.data.body.orderno,
                    //    ordertime = package.data.body.ordertime,
                    //    h5fronturl = package.data.body.h5fronturl,
                    //    mfronturl = package.data.body.mfronturl,
                    //    afterurl = package.data.body.afterurl,
                    //    goodspic = package.data.body.goodspic,
                    //    goodsname = package.data.body.goodsname,
                    //    goodsprice = package.data.body.goodsprice,
                    //    skuname = package.data.body.skuname
                    //};
                    info = "[订单链接]";
                    msg.Content = orderAfterurl;
                    break;
                case MessageType.custom:
                    msg.MsgType = MessageType.txt;
                    string customAfterurl = package.data.body.afterurl;

                    //CustomBody cb = new CustomBody()
                    //{
                    //    customno = package.data.body.customno,
                    //    customtime = package.data.body.customtime,
                    //    fronturl = package.data.body.fronturl,
                    //    afterurl = package.data.body.afterurl,
                    //    goodspic = package.data.body.goodspic,
                    //    goodsname = package.data.body.goodsname,
                    //    goodsprice = package.data.body.goodsprice,
                    //    skuname = package.data.body.skuname
                    //};
                    info = "[商品链接]";
                    msg.Content = customAfterurl;
                    break;
                default:
                    info = msg.Content;
                    break;
            }

            AddMessage(msg, info, package);
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

        private void AddMessage(MessageModel msg, string info, MessagePackage package)
        {
            if ((this.Model as ChatModel).Messages != null && AppData.MainMV.ChatListVM.SelectedItem == this)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.SetLastMsg(msg, info);

                    SetMsgShowTime(msg);
                    if (Chat.Messages.Count > MAXCOUNT)
                    {
                        Chat.Messages.RemoveAt(0);
                        Chat.Messages.RemoveAt(0);
                    }
                    Chat.Messages.Add(msg);
                    this.OnDisplayMsgHint?.Invoke();
                    this.OnDisplayAtButton?.Invoke();
                    //NewMessage.Invoke(this.LastMsg);

                    if (AppData.MainMV.ListViewModel == AppData.MainMV.ChatListVM)
                    {
                        if (GotViewIsFocus != null && !GotViewIsFocus.Invoke())
                        {
                            if (package != null && package.syncMsg == 1 && package.from.ToInt() == AppData.Current.LoginUser.ID)//自己发送的同步消息
                            {
                                SDKClient.SDKClient.Instance.UpdateHistoryMsgIsReaded(package.id);
                            }
                            else
                            {
                                this.UnReadCount++;

                                AppData.MainMV.ChatListVM.FlashIcon(this);
                            }
                        }
                        else
                        {
                            SDKClient.SDKClient.Instance.UpdateHistoryMsgIsReaded(package.id);
                        }
                    }
                    else
                    {
                        if (package != null && package.syncMsg == 1 && package.from.ToInt() == AppData.Current.LoginUser.ID)//自己发送的同步消息
                        {
                            SDKClient.SDKClient.Instance.UpdateHistoryMsgIsReaded(package.id);
                        }
                        else
                        {
                            this.UnReadCount++;

                            AppData.MainMV.ChatListVM.FlashIcon(this);
                        }
                    }

                }));
            }
            else
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.SetLastMsg(msg, info, false);

                    _unReadMsgs.Add(msg);

                    if (package != null && package.syncMsg == 1 && package.from.ToInt() == AppData.Current.LoginUser.ID)//自己发送的同步消息
                    {
                        SDKClient.SDKClient.Instance.UpdateHistoryMsgIsReaded(package.id);
                    }
                    else
                    {
                        this.UnReadCount++;

                        AppData.MainMV.ChatListVM.FlashIcon(this);
                    }
                }));
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

        public void AppendMsg()
        {
            this.AppendMessage?.Invoke(null);
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

        public MessageModel AddMessageTip(string tip, DateTime? dt = null, bool isForward = false, string msgId = null,bool isSetLastMsg = true)
        {

            MessageModel msg = new MessageModel()
            {
                //MsgKey = string.IsNullOrEmpty(msgId) == true ? "[提示]" + Guid.NewGuid().ToString() : msgId,
                MsgKey = msgId,
                MsgType = MessageType.notification,
                Content = tip,
                SendTime = dt ?? DateTime.Now,
            };
            if (!isForward)//新消息
            {
                if(isSetLastMsg)
                    this.SetLastMsg(msg, tip);

                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                   
                       
                    SetMsgShowTime(msg);
                    (this.Model as ChatModel).Messages.Add(msg);
                       
                    
                    AppData.MainMV.ChatListVM.ResetSort();
                }));
            }
            else//上翻
            {
                //System.Windows.MessageBox.Show("上翻");
                SetMsgShowTime(msg, false);
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ChatModel chat = this.Model as ChatModel;
                    chat.Messages.Insert(0, msg);
                }));
            }
            return msg;
        }

        public void AddMessageTipEx(string tip)
        {
            MessageModel msg = new MessageModel()
            {
                //MsgKey = "[提示]" + Guid.NewGuid().ToString(),
                MsgType = MessageType.notification,
                Content = tip,
                SendTime = DateTime.Now,
            };
            SetMsgShowTime(msg);
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                ChatModel chat = this.Model as ChatModel;
                this.SetLastMsg(msg, tip);
                chat.Messages.Add(msg);
            }));
        }

        /// <summary>
        /// 设置条目是否显示时间值
        /// </summary>
        /// <param name="msg">信息题目</param>
        /// <param name="isAdd">是否追加，true 追加，false 往前插入</param>
        private void SetMsgShowTime(MessageModel msg, bool isAdd = true)
        {
            if (Chat.Messages.Count == 0)
            {
                msg.ShowSendTime = true;
                return;
            }
            if (isAdd)
            {
                MessageModel last = Chat.Messages.LastOrDefault();

                //double interval = (msg.SendTime - last.SendTime).TotalMinutes;
                //if (interval >= 5)
                //{
                //    msg.ShowSendTime = true;
                //}
                msg.ShowSendTime = true;
            }
            else
            {
                MessageModel first = Chat.Messages.FirstOrDefault();

                double interval = (first.SendTime - msg.SendTime).TotalMinutes;
                msg.ShowSendTime = true;
                //if (interval < 5)
                //{
                //    first.ShowSendTime = false;
                //}
                first.ShowSendTime = true;
            }
        }

    }
}
