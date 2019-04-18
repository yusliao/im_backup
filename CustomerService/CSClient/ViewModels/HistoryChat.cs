using IMModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSClient.ViewModels
{
    public class HistoryChat : ViewModel
    {

        public HistoryChat(HistoryChatItem model) : base(model)
        {
            Chat = model;
        }
        public HistoryChatItem Chat { get; }
        private VMCommand _jumpCommand;
        public VMCommand JumpCommand
        {
            get
            {
                return new VMCommand(JupmToChat);
            }
        }
        private void JupmToChat(object para)
        {

            HistoryChatItem item = this.Model as HistoryChatItem;
            AppData.MainMV.JumpToChatModel(item);

        }
        public bool HasActived { get; private set; }
        private ObservableCollection<MessageModel> _messages;
        public ObservableCollection<MessageModel> Messages
        {
            get { return _messages; }
            set { _messages = value; this.OnPropertyChanged(); }
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

        //public void Acitve()
        //{

        //    if (!this.HasActived)
        //    {
        //        App.Current.Dispatcher.Invoke(new Action(() =>
        //        {
        //            this.Messages.Clear();
        //        }));
        //        this.HasActived = true;
        //    }
        //    else
        //    {
        //    }
        //}
        //Task _loadHisTask;
        //public void LoadHisMessages(bool isForward = false)
        //{
        //    var msgs = (this.Model as ChatModel).Messages;
        //    if (_loadHisTask != null && _loadHisTask.Status != TaskStatus.RanToCompletion)
        //    {
        //        return;
        //    }
        //    _loadHisTask = Task.Run(() =>
        //    {
        //        List<SDKClient.DTO.MessageEntity> datas;
        //        MessageModel top = null;

        //        top = msgs.Where(x => !string.IsNullOrEmpty(x.MsgKey)).FirstOrDefault();
        //        if (isForward)
        //        {
        //            //datas = SDKClient.SDKClient.Instance.GetHistoryMsg(this.Model.ID, top.MsgKey, 10);

        //            if (msgs.Count > 1 && top != null)
        //            {
        //                DateTime sendTime = top.SendTime;
        //                if (sendTime == null)
        //                {
        //                    sendTime = DateTime.Now;
        //                }
        //                datas = SDKClient.SDKClient.Instance.GetHistoryMsgEntity(this.Model.ID, top.MsgKey, 10, sendTime);
        //            }
        //            else
        //            {
        //                datas = SDKClient.SDKClient.Instance.GetHistoryMsgEntity(this.Model.ID, string.Empty, 10, DateTime.Now);
        //            }

        //            if (datas.Count == 0)
        //            {
        //                WarningInfo = "没有更多消息";
        //                return;
        //            }
        //        }
        //        else
        //        {

        //        }

        //        int fromId;
        //        foreach (var m in datas)
        //        {
        //            System.Threading.Thread.CurrentThread.Join(5);
        //            if (int.TryParse(m.From, out fromId) && fromId >= 0)
        //            {
        //                if ((this.Model as ChatModel).Messages.Any(x => x.MsgKey == m.MsgId))//重复消息
        //                {
        //                    continue;
        //                }

        //                #region 消息类型
        //                MessageType type;
        //                try
        //                {
        //                    type = (MessageType)Enum.Parse(typeof(MessageType), m.MsgType.ToLower());
        //                }
        //                catch
        //                {
        //                    continue;
        //                }
        //                if (type == MessageType.redenvelopesreceive || type == MessageType.retract)////不显示对方接收的红包和自己发送的红包消息
        //                    continue;
        //                #endregion

        //                IChat sender;

        //                object target = null;
        //                // AppData.Current.LoginUser.User.ID == fromId;
        //                //if (isMine) //我发送的消息，统一放在右边
        //                //{
        //                //    sender = AppData.Current.LoginUser.User;
        //                //}
        //                //else


        //                    //sender= AppData.Current.GetUserModel(fromId);
        //                    var user = AppData.Current.GetUserModel(fromId);
        //                    //if (string.IsNullOrEmpty(user.DisplayName))
        //                    //{
        //                    //    user.DisplayName = user.PhoneNumber;
        //                    //}

        //                    //if(fromId != this.Model.ID)
        //                    //{

        //                    //}

        //                    sender = user;
        //                    //if (type == MessageType.invitejoingroup)
        //                    //{
        //                    //    target = LoadGroupCard(m.Source, fromId);
        //                    //    if (target == null)
        //                    //    {
        //                    //        continue;
        //                    //    }
        //                    //}


        //                MessageModel msg = new MessageModel()
        //                {
        //                    MsgKey = m.MsgId,
        //                    Sender = sender,
        //                    SendTime = m.MsgTime,
        //                    IsMine = fromId != this.Model.ID,
        //                    MsgType = type,
        //                    Target = target,
        //                };
        //                if (msg.IsMine)
        //                {
        //                    msg.Sender.HeadImg = AppData.Current.LoginUser.User.HeadImg;
        //                }

        //                if (!isForward)
        //                {
        //                    if (!string.IsNullOrEmpty(m.TokenIds))
        //                    {
        //                        string[] arr = m.TokenIds.Split(',');

        //                    }
        //                }
        //                msg.ResourceModel = new FileResourceModel()
        //                {
        //                    Key = m.ResourceId,
        //                    SmallKey = m.ResourcesmallId,
        //                    Length = m.FileSize,
        //                    FileName = Path.GetFileName(m.FileName),
        //                    //如果文件名包含绝对路径，那么不从manjinba/file路径中文件
        //                    FullName = m.FileName,
        //                };
        //                string info = string.Empty;
        //                switch (type)
        //                {
        //                    case MessageType.img:
        //                        info = "[图片]";
        //                        break;
        //                    case MessageType.file:
        //                        info = "[文件]";
        //                        msg.Content = m.FileName;
        //                        break;
        //                    case MessageType.onlinefile:

        //                        if (File.Exists(m.Content))
        //                        {

        //                            msg.Content = m.Content;
        //                            msg.MsgType = MessageType.file;
        //                            msg.ResourceModel.FullName = msg.Content;
        //                        }
        //                        else
        //                        {
        //                            var package = Util.Helpers.Json.ToObject<SDKClient.Model.MessagePackage>(m.Data);
        //                            var body = package.data.body;

        //                            string onlinePath = m.FileName;
        //                            string onlineName = Path.GetFileName(onlinePath);

        //                            FileResourceModel onlineFile = new FileResourceModel()
        //                            {
        //                                Key = body.id,
        //                                Length = body.fileSize,
        //                                FileName = onlineName,
        //                                //如果文件名包含绝对路径，那么不从manjinba/file路径中文件
        //                                FullName = onlinePath,
        //                                RefInfo = new SDKClient.Model.OnlineFileBody()
        //                                {
        //                                    id = body.id,
        //                                    fileSize = body.fileSize,
        //                                    fileName = onlineName,
        //                                    Port = body.Port,
        //                                    IP = body.IP,
        //                                }
        //                            };
        //                            msg.ResourceModel = onlineFile;
        //                            msg.MsgType = MessageType.file;
        //                        }
        //                        info = "[文件]";
        //                        break;
        //                    case MessageType.invitejoingroup:
        //                        info = "[群名片]";
        //                        break;
        //                    case MessageType.audio:
        //                        info = "[语音]";
        //                        msg.Content = msg.IsMine ? "您发送了一条语音消息，请在手机端查看" : "对方发送语音消息，请在手机端查看";
        //                        break;
        //                    case MessageType.video:
        //                        info = "[视频]";
        //                        msg.Content = msg.IsMine ? "您发送了一条视频消息，请在手机端查看" : "对方发送视频消息，请在手机端查看";
        //                        break;
        //                    case MessageType.redenvelopesreceive:
        //                        msg.MsgType = MessageType.notification;
        //                        info = msg.Content = msg.IsMine ? "您领取了一个红包，请在手机端查看" : "[有人领取了您的红包，请在手机端查看]";
        //                        break;
        //                    case MessageType.redenvelopessendout:
        //                        msg.MsgType = MessageType.notification;
        //                        info = msg.Content = "[您有新红包，请在手机上查看]";
        //                        break;
        //                    case MessageType.addgroupnotice:
        //                        info = "[群公告]";
        //                        msg.MsgType = MessageType.notification;
        //                        break;
        //                    case MessageType.bigtxt:
        //                        break;

        //                    case MessageType.notification:
        //                        info = msg.Content = m.Content;
        //                        int result = m.OptionRecord & (int)SDKClient.SDKProperty.MessageState.cancel;
        //                        //if (msg.Content != null && result == 4)
        //                        //{
        //                        //    msg.MessageState = MessageStates.Fail;
        //                        //}

        //                        if (msg.Content.Contains("失败"))
        //                        {
        //                            msg.MessageState = MessageStates.Fail;
        //                        }
        //                        else if (msg.Content.Contains("成功"))
        //                        {
        //                            msg.MessageState = MessageStates.Success;
        //                        }
        //                        break;
        //                    case MessageType.goods:
        //                        info = "[商品链接]";
        //                        msg.MsgType = MessageType.txt;
        //                        msg.Content = m.Data.afterurl;
        //                        break;
        //                    case MessageType.order:
        //                        info = "[订单链接]";
        //                        msg.MsgType = MessageType.txt;
        //                        msg.Content = m.Data.afterurl;
        //                        break;
        //                    case MessageType.custom:
        //                        info = "[商品链接]";
        //                        msg.MsgType = MessageType.txt;
        //                        msg.Content = m.Data.afterurl;
        //                        break;
        //                    default:
        //                        info = msg.Content = m.Content;
        //                        break;
        //                }
        //                App.Current.Dispatcher.Invoke(new Action(() =>
        //                {
        //                    if (isForward)
        //                    {
        //                        SetMsgShowTime(msg, false);
        //                        msgs.Insert(0, msg);
        //                        this.AppendMessage?.Invoke(top);
        //                    }
        //                    else
        //                    {
        //                        if (m == datas.Last())
        //                        {
        //                            this.SetLastMsg(msg, info);
        //                            App.Current.Dispatcher.Invoke(() =>
        //                            {
        //                                AppData.MainMV.ChatListVM.ResetSort();
        //                            });
        //                        }

        //                        SetMsgShowTime(msg);
        //                        msgs.Add(msg);
        //                        this.AppendMessage?.Invoke(null);

        //                    }
        //                }));
        //            }
        //        }

        //        if (!isForward)
        //        {
        //            App.Current.Dispatcher.Invoke(() =>
        //            {
        //                this.OnDisplayAtButton?.Invoke();
        //            });
        //        }
        //    });
        //}

    }
}
