using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMClient.Helper;
using IMModels;
using SDKClient.Model;
namespace IMClient.ViewModels
{
    public class ChatHistoryViewModel : ViewModel
    {
        ChatModel _targetChat;

        private List<MessageModel> _hisMessages;
        /// <summary>
        /// 聊天记录VM
        /// </summary>
        /// <param name="view"></param>
        public ChatHistoryViewModel(ChatModel model) : base(model)
        {
            _targetChat = model;
            _hisMessages = new List<MessageModel>();
        }

        #region private Method

        //public IList<MessageModel> LoadDefaultHistoryMsg()
        //{
        //    return   _hisMessages = _targetChat.Messages.Where(info => !info.IsTimeValue).ToList();

        //}

        bool isLoading = false;
        string currentMsgKdy = string.Empty;
        /// <summary>
        /// 往前加载数据
        /// </summary>
        /// <param name="count">增加加载数量</param>
        public void LoadPreviousMessages(Action<List<MessageModel>> completed,
            SDKClient.SDKProperty.MessageType msgType = SDKClient.SDKProperty.MessageType.all,
            DateTime? date = null, bool isReset = false, int count = 20)
        {
            if (isLoading)
            {
                completed?.Invoke(null);
                return;
            }

            Task.Delay(300).ContinueWith((t) =>
            {
                bool isForword = false;
                isLoading = true;
                if (isReset)
                {

                    _hisMessages.Clear();
                }
                MessageModel topMost = _hisMessages.FirstOrDefault();
                List<SDKClient.DB.messageDB> datas;
                SDKClient.SDKProperty.chatType chatType = _targetChat.IsGroup ? SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;

                if (topMost != null)
                {
                    if (currentMsgKdy != topMost.MsgKey)
                    {
                        isForword = true;
                        currentMsgKdy = topMost.MsgKey;
                        datas = SDKClient.SDKClient.Instance.GetHistoryMsg(this.Model.ID, topMost.MsgKey, count, date, messageType: msgType, chatType: chatType);
                    }
                    else
                    {
                        completed?.Invoke(null);
                        isLoading = false;
                        return;
                    }
                }
                else
                {
                    datas = SDKClient.SDKClient.Instance.GetHistoryMsg(this.Model.ID, count, date, messageType: msgType, chatType: chatType);
                }


                List<MessageModel> previous = new List<MessageModel>();
                int fromId;
                if (this.Model.ID == AppData.Current.LoginUser.ID)
                {
                    datas = datas.Where(m => m.sessionType == (int)SDKClient.SDKProperty.SessionType.FileAssistant).ToList();
                }
                for (int i = 0; i < datas.Count; i++)
                {
                    var m = datas[i];
                    //if (_hisMessages.Any(old => old.MsgKey == m.msgId))
                    //{
                    //    continue; ;
                    //}
                    //System.Threading.Thread.CurrentThread.Join(5);
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
                        bool isMine = AppData.Current.LoginUser.User.ID == fromId;
                        if (_targetChat.IsGroup) //群成员发送的消息，要现实的是成员在  我给好友备注的昵称>群成员自己设置的昵称>群成员名称
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
                                type = MessageType.txt;

                            }
                        }

                        //FileResourceModel file = new FileResourceModel()
                        //{
                        //    Length = m.fileSize,
                        //    FileName = m.fileName,
                        //    Key = m.resourceId, 
                        //    SmallKey = m.resourcesmallId,
                        //};

                        MessageModel msg = new MessageModel()
                        {
                            MsgKey = m.msgId,
                            Sender = sender,
                            SendTime = m.msgTime,
                            MsgType = type,
                            IsMine = isMine,
                            //Content = m.content,
                            //ResourceModel = file, 
                        };

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

                        string info = string.Empty;
                        switch (type)
                        {
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
                                msg.MsgHyperlink = msgHyperlink;
                                msg.MsgSource = m.Source;
                                msg.Content = string.IsNullOrEmpty(dynContent) ? msgHyperlink : dynContent;
                                //msg.ShareMsgImage=
                                msg.TipMessage = "[链接]" + dynContent;
                                //链接消息，请在手机端查看";
                                break;
                            case MessageType.img:
                                info = "[图片]";
                                msg.Content = m.fileName;

                                if (AppData.MainMV.ChatListVM.SelectedItem is ChatViewModel chat)
                                {
                                    var target = chat.Chat.Messages.FirstOrDefault(old => old.MsgKey == msg.MsgKey);
                                    if (target != null)
                                    {
                                        target.Sender = sender;
                                        msg = target;
                                        msg.MessageState = target.MessageState;
                                    }
                                }
                                break;
                            case MessageType.file:

                                info = "[文件]";
                                msg.Content = m.fileName;
                                break;
                            case MessageType.onlinefile:

                                if (File.Exists(m.content))
                                {
                                    msg.Content = m.content;
                                    msg.MsgType = MessageType.file;
                                }
                                else
                                {
                                    var package = Util.Helpers.Json.ToObject<SDKClient.Model.MessagePackage>(m.Source);
                                    var body = package.data.body;

                                    //string onlineName = Path.GetFileName($"{package.data.body.fileName}");
                                    //string onlinePath = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.filePath, onlineName);
                                    //onlinePath = Helper.FileHelper.GetFileName(onlinePath, 1);
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
                                        DBState = m.fileState
                                    };

                                    msg.ResourceModel = onlineFile;
                                    msg.MsgType = MessageType.file;
                                    msg.Content = onlinePath;
                                }
                                info = "[文件]";
                                break;
                            case MessageType.invitejoingroup:
                                //info = "[群名片]";
                                info = msg.Content = "[群名片]";
                                break;
                            case MessageType.audio:
                                info = "[语音]";
                                msg.Content = msg.IsMine ? "您发送了一条语音消息，请在手机端查看" : "对方发送语音消息，请在手机端查看";
                                break;
                            case MessageType.smallvideo:
                            case MessageType.video:
                                info = "[小视频]";
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
                                };
                                video.PreviewImagePath = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, video.PreviewKey);
                                msg.ResourceModel = video;
                                msg.Content = videoPath;

                                //if (AppData.MainMV.ChatListVM.SelectedItem is ChatViewModel chatVM)
                                //{
                                //    var target = chatVM.Chat.Messages.FirstOrDefault(old => old.MsgKey == msg.MsgKey);
                                //    if (target != null)
                                //    {
                                //        target.Sender = sender;
                                //        msg = target;
                                //        msg.ResourceModel = target.ResourceModel;
                                //    }
                                //}
                                break;
                            case MessageType.txt:

                                info = msg.Content = m.content;
                                break;
                            case MessageType.bigtxt:
                                msg.Content = m.content;
                                info = m.content;
                                break;
                            //case MessageType.onlinefile:

                            //    break;
                            case MessageType.setmemberpower:
                                var p = Util.Helpers.Json.ToObject<SetMemberPowerPackage>(m.Source);
                                msg.Sender = new UserModel() { DisplayName = "系统消息" };
                                msg.MsgType = MessageType.notification;

                                int index = 0;
                                foreach (var item in p.data.userIds)
                                {
                                    UserModel user = AppData.Current.GetUserModel(item);
                                    var group = this._targetChat.Chat as GroupModel;
                                    var member = user.GetInGroupMember(group);

                                    if (string.IsNullOrEmpty(user.Name))
                                    {
                                        SDKClient.SDKClient.Instance.GetUser(item);
                                    }

                                    if (p.data.type == "admin")
                                    {
                                        if (item == AppData.MainMV.LoginUser.ID)
                                        {
                                            msg.Content = "你成为群管理员";
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(member.DisplayName))
                                            {
                                                msg.Content = $"[{p.data.userNames[index]}] 成为群管理员";
                                            }
                                            else
                                            {
                                                msg.Content = string.Format("[{0}] 成为群管理员", member.DisplayName);
                                            }
                                        }
                                        member.IsManager = true;
                                    }
                                    else
                                    {
                                        if (item == AppData.MainMV.LoginUser.ID)
                                        {
                                            msg.Content = "你被取消群管理员";
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(member.DisplayName))
                                            {
                                                msg.Content = $"[{p.data.userNames[index]}] 被取消群管理员";
                                            }
                                            else
                                            {
                                                msg.Content = string.Format("[{0}] 被取消群管理员", member.DisplayName);
                                            }
                                        }
                                        member.IsManager = false;
                                    }
                                    index++;
                                }
                                break;
                            //case MessageType.dismissgroup:

                            //    var dismissGroupPackage = Util.Helpers.Json.ToObject<DismissGroupPackage>(m.Source);
                            //    this.ShowDismissMsg(dismissGroupPackage, isForward);
                            //    continue;
                            case MessageType.addgroupnotice:
                                var noticePackage = Util.Helpers.Json.ToObject<SDKClient.Model.MessagePackage>(m.Source);
                                var noticeData = noticePackage.data.body;
                                GroupNoticeModel groupNoticeModel = new GroupNoticeModel()
                                {
                                    NoticeTitle = noticeData.title,
                                    NoticeId = noticeData.noticeId,
                                    GroupNoticeContent = noticeData.content
                                };
                                msg.Content = noticeData.content;
                                msg.SendTime = noticeData.publishTime ?? DateTime.Now;
                                msg.NoticeModel = groupNoticeModel;
                                info = msg.NoticeModel.NoticeTitle;
                                if (this.Model is ChatModel chatModell)
                                {
                                    if (chatModell.IsGroup)
                                    {
                                        GroupModel groupModell = chatModell.Chat as GroupModel;
                                        int groupId = groupModell.ID;
                                        msg.NoticeModel.GroupMId = groupId;
                                    }
                                }
                                break;
                            case MessageType.usercard:
                                MessagePackage packageMSG = Util.Helpers.Json.ToObject<SDKClient.Model.MessagePackage>(m.Source);
                                string userName = packageMSG.data.body.name;
                                string userPhoto = packageMSG.data.body.photo;
                                string userPhone = packageMSG.data.body.phone;
                                int userId = packageMSG.data.body.userId;
                                PersonCardModel pcm = new PersonCardModel()
                                {
                                    Name = userName,
                                    PhotoImg = userPhoto,
                                    PhoneNumber = userPhone,
                                    UserId = userId
                                };
                                ///string imgPath = packageMSG.data.body.photo;
                                var imageFullPath = IMClient.Helper.ImageHelper.GetFriendFace(userPhoto, (a) =>
                                {
                                    //msg.PersonCardModel.PhotoImg = a;
                                });
                                pcm.PhotoImg = imageFullPath;
                                msg.ContentMD5 = userPhoto;
                                msg.Content = "[个人名片]";
                                msg.TipMessage = "[个人名片]" + pcm.Name;
                                msg.PersonCardModel = pcm;
                                break;
                            default:
                                msg.Sender = new UserModel() { DisplayName = "系统消息" };
                                msg.MsgType = MessageType.notification;
                                info = msg.Content = m.content;
                                break;
                        }
                        //if (isForword)
                        //{
                        //    previous.Insert(0, msg);
                        //}
                        //else
                        {
                            var tempMsg = previous.ToList().FirstOrDefault(n => n.MsgKey == msg.MsgKey);
                            if (tempMsg == null)
                                previous.Add(msg);
                        }
                    }
                }

                if (isForword)
                {
                    previous.Reverse();
                }


                _hisMessages.InsertRange(0, previous);
                completed?.Invoke(previous);
                isLoading = false;
            });
        }

        #endregion

        #region Command

        private VMCommand _clearAllCommand;
        /// <summary>
        /// 跳转命令
        /// </summary> 
        public VMCommand ClearAllCommand
        {
            get
            {
                if (_clearAllCommand == null)
                    _clearAllCommand = new VMCommand(ClearAll);
                return _clearAllCommand;
            }
        }



        #region command method

        private async void ClearAll(object para)
        {
            if (this.Model is ChatModel chat)
            {
                if (chat.IsGroup)
                    await SDKClient.SDKClient.Instance.DeleteHistoryMsg(chat.ID, SDKClient.SDKProperty.chatType.groupChat);
                else
                    await SDKClient.SDKClient.Instance.DeleteHistoryMsg(chat.ID, SDKClient.SDKProperty.chatType.chat);
            }
        }

        #endregion


        #endregion
    }
}
