using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMModels;
using SDKClient.Model;
namespace CSClient.ViewModels
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
        /// <summary>
        /// 往前加载数据
        /// </summary>
        /// <param name="count">增加加载数量</param>
        public void LoadPreviousMessages(Action<List<MessageModel>> completed,
            SDKClient.SDKProperty.MessageType msgType = SDKClient.SDKProperty.MessageType.all,
            DateTime? date = null, bool isReset = false, int count = 30)
        {
            if (isLoading)
            {
                completed?.Invoke(null);
                return;
            }

            Task.Run(() =>
            {
                bool isForword = false;
                isLoading = true;
                if (isReset)
                {
                    _hisMessages.Clear();
                }
                MessageModel topMost = _hisMessages.FirstOrDefault();
                List<SDKClient.DTO.MessageEntity> datas;
                //SDKClient.SDKProperty.chatType chatType = _targetChat.IsGroup ? SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;

                if (topMost != null)
                {
                    isForword = true;
                    //datas = SDKClient.SDKClient.Instance.GetHistoryMsg(this.Model.ID, topMost.MsgKey, count, date, messageType: msgType, chatType: chatType);
                    DateTime sendTime = topMost.SendTime;
                    if (sendTime == null)
                    {
                        sendTime = DateTime.Now;
                    }
                    datas = SDKClient.SDKClient.Instance.GetHistoryMsgEntity(this.Model.ID, topMost.MsgKey, 10, sendTime, messageType: msgType);
                }
                else
                {
                    //datas = SDKClient.SDKClient.Instance.GetHistoryMsg(this.Model.ID, count, date, messageType: msgType, chatType: chatType);
                    datas = SDKClient.SDKClient.Instance.GetHistoryMsgEntity(this.Model.ID, string.Empty, 10, date, messageType: msgType);
                }

                List<MessageModel> previous = new List<MessageModel>();
                int fromId;
                for (int i = 0; i < datas.Count; i++)
                {
                    var m = datas[i];
                    //if (_hisMessages.Any(old => old.MsgKey == m.msgId))
                    //{
                    //    continue; ;
                    //}
                    //System.Threading.Thread.CurrentThread.Join(5);
                    if (int.TryParse(m.From, out fromId) && fromId > 0)
                    {
                        #region 消息类型
                        MessageType type;
                        try
                        {
                            type = (MessageType)Enum.Parse(typeof(MessageType), m.MsgType.ToLower());
                        }
                        catch
                        {
                            //Views.MessageBox.ShowBox("收到未知消息类型：" + m.msgType);
                            continue;
                        }
                        #endregion

                        IChat sender;
                        bool isMine = AppData.Current.LoginUser.User.ID == fromId;
                        //if (_targetChat.IsGroup) //群成员发送的消息，要现实的是成员在  我给好友备注的昵称>群成员自己设置的昵称>群成员名称
                        //{
                        //    UserModel user = AppData.Current.GetUserModel(fromId);
                        //    GroupModel group = AppData.Current.GetGroupModel(m.RoomId);
                        //    sender = user.GetInGroupMember(group);
                        //}
                        //else //单聊好友发送的消息,只显示头像，不显示昵称（聊天框已有昵称显示）
                        //{
                            sender = AppData.Current.GetUserModel(fromId);
                            if (string.IsNullOrEmpty(sender.DisplayName))
                            {
                                sender.DisplayName = m.SenderName;
                            }
                            if (type == MessageType.invitejoingroup)
                            {
                                type = MessageType.txt;
                            }
                        //}

                        MessageModel msg = new MessageModel()
                        {
                            MsgKey = m.MsgId,
                            Sender = sender,
                            SendTime = m.MsgTime,
                            IsMine = fromId != this.Model.ID,
                            MsgType = type,
                        };

                        msg.ResourceModel = new FileResourceModel()
                        {
                            Key = m.ResourceId,
                            SmallKey = m.ResourcesmallId,
                            Length = m.FileSize,
                            FileName = Path.GetFileName(m.FileName),
                            //如果文件名包含绝对路径，那么不从manjinba/file路径中文件
                            FullName = m.FileName,
                        };

                        if (type == MessageType.redenvelopesreceive||type==MessageType.retract || (type == MessageType.redenvelopessendout && msg.IsMine))////不显示对方接收的红包和自己发送的红包消息
                            continue;

                        string info = string.Empty;
                        switch (type)
                        {
                            case MessageType.img:
                                info = "[图片]";
                                msg.Content = m.FileName;

                                if (AppData.MainMV.ChatListVM.SelectedItem is ChatViewModel chat)
                                {
                                    var target = chat.Chat.Messages.FirstOrDefault(old => old.MsgKey == msg.MsgKey);
                                    if (target != null)
                                    {
                                        target.Sender = sender;
                                        msg = target;
                                    }
                                }
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
                                }
                                else
                                {
                                    var package = Util.Helpers.Json.ToObject<SDKClient.Model.MessagePackage>(m.Source);
                                    var body = package.data.body;

                                    //string onlineName = Path.GetFileName($"{package.data.body.fileName}");
                                    //string onlinePath = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.filePath, onlineName);
                                    //onlinePath = Helper.FileHelper.GetFileName(onlinePath, 1);
                                    //onlineName = Path.GetFileName(onlinePath);

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
                                        },
                                        DBState = m.FileState
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
                                string videoPath = m.FileName;
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
                                info = msg.Content = m.Content;
                                break;
                            case MessageType.retract:
                                
                                msg.MsgType = MessageType.notification;
                                info = msg.Content = msg.IsMine ? "您撤回了一条消息" : "对方撤回了一条消息";
                                break;
                            case MessageType.bigtxt:
                                break;
                            //case MessageType.onlinefile:

                            //    break;
                            //case MessageType.setmemberpower:
                            //    var p = Util.Helpers.Json.ToObject<SetMemberPowerPackage>(m.Source);
                            //    msg.Sender = new UserModel() { DisplayName = "系统消息" };
                            //    msg.MsgType = MessageType.notification;

                            //    int index = 0;
                            //    foreach (var item in p.data.userIds)
                            //    {
                            //        UserModel user = AppData.Current.GetUserModel(item);
                            //        var group = this._targetChat.Chat as GroupModel;
                            //        var member = user.GetInGroupMember(group);

                            //        if (string.IsNullOrEmpty(user.Name))
                            //        {
                            //            SDKClient.SDKClient.Instance.GetUser(item);
                            //        }

                            //        if (p.data.type == "admin")
                            //        {
                            //            if (item == AppData.MainMV.LoginUser.ID)
                            //            {
                            //                msg.Content = "你成为群管理员";
                            //            }
                            //            else
                            //            {
                            //                if (string.IsNullOrEmpty(member.DisplayName))
                            //                {
                            //                    msg.Content = $"[{p.data.userNames[index]}] 成为群管理员";
                            //                }
                            //                else
                            //                {
                            //                    msg.Content = string.Format("[{0}] 成为群管理员", member.DisplayName);
                            //                }
                            //            }
                            //            member.IsManager = true;
                            //        }
                            //        else
                            //        {
                            //            if (item == AppData.MainMV.LoginUser.ID)
                            //            {
                            //                msg.Content = "你被取消群管理员";
                            //            }
                            //            else
                            //            {
                            //                if (string.IsNullOrEmpty(member.DisplayName))
                            //                {
                            //                    msg.Content = $"[{p.data.userNames[index]}] 被取消群管理员";
                            //                }
                            //                else
                            //                {
                            //                    msg.Content = string.Format("[{0}] 被取消群管理员", member.DisplayName);
                            //                }
                            //            }
                            //            member.IsManager = false;
                            //        }
                            //        index++;
                            //    }
                            //    break;
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
                                    NoticeId = noticeData.noticeId
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
                                msg.Sender = new UserModel() { DisplayName = "系统消息" };
                                msg.MsgType = MessageType.notification;
                                info = msg.Content = m.Content;
                                break;
                        }
                        //if (isForword)
                        //{
                        //    previous.Insert(0, msg);
                        //}
                        //else
                        {
                            previous.Add(msg);
                        }
                    }
                }
                previous.Reverse();
                //if (isForword)
                //{
                //    previous.Reverse();
                //}


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
