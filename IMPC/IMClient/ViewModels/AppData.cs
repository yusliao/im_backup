using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using IMClient.Views.ChildWindows;
using IMModels;
using SDKClient.Model;
using SDKClient.Protocol;
using static IMClient.ViewModels.GroupListViewModel;

namespace IMClient.ViewModels
{
    public class AppData
    {



        /// <summary>
        /// 创建群时，等待返回的对象集合
        /// </summary>
        public static Dictionary<string, int[]> CreateGroupWaits = new Dictionary<string, int[]>();

        private static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        public static List<int> DeleteFriendIDItems = new List<int>();
        public static List<PackageInfo> DismissGroupIDItems = new List<PackageInfo>();
        /// <summary>
        /// 自定义复制标签
        /// </summary>
        public const string FlagCopy = "[{COPY}]";

        /// <summary>
        /// 表情标签
        /// </summary>
        public const string FlagEmoje = "[{EM}]";
        /// <summary>
        /// 图片标签
        /// </summary>
        public const string FlagImage = "[{IMG}]";
        /// <summary>
        /// 小视频标签
        /// </summary>
        public const string FlagSmallVideo = "[{SMALLVIDEO}]";
        /// <summary>
        /// 文件标签
        /// </summary>
        public const string FlagFile = "[{FILE}]";
        /// <summary>
        /// @标签
        /// </summary>
        public const string FlagAt = "[{@}]";


        /// <summary>
        /// 当前主VM
        /// </summary>
        public static MainViewModel MainMV { get; private set; }

        public static List<UserModel> TempUserModel { get; set; } = new List<UserModel>();

        private static Hashtable _userHash = new Hashtable();
        /// <summary>
        /// 用户 哈希表 [int,UserModel]
        /// </summary>
        private static Hashtable UserHash { get { return _userHash; } }

        private static Hashtable _groupHash = new Hashtable();
        /// <summary>
        /// 群组 哈希表 [int,GroupModel]
        /// </summary>
        private static Hashtable GroupHash { get { return _groupHash; } }

        private static Hashtable _chatHash = new Hashtable();
        /// <summary>
        /// 聊天 哈希表 [int,ChatModel]
        /// </summary>
        private static Hashtable ChatHash { get { return _chatHash; } }

        private static AppData _current = new AppData();
        public static AppData Current { get { return _current; } }

        public static List<ChatViewModel> MessageModels = new List<ChatViewModel>();
        /// <summary>
        /// 是否是全局搜索
        /// </summary>
        public static bool IsGlobalSearch { set; get; }
        /// <summary>
        /// 以下两个字典用于公告缓存
        /// </summary>
        public static Dictionary<int, GroupNotice> ordinaryGroupNoticeDic = new Dictionary<int, GroupNotice>();
        public static Dictionary<int, GroupNotice> joinGroupNeedKnowDic = new Dictionary<int, GroupNotice>();
        /// <summary>
        /// 以下字典用于反馈缓存
        /// </summary>
        public static Dictionary<int, FeedBackViewModel> feedBackVmDic = new Dictionary<int, FeedBackViewModel>();
        public static GroupNoticeTipWindow tipWindow;


        /// <summary>
        /// 当前登录用户
        /// </summary>
        public LoginUser LoginUser { get; private set; }

        public static DateTime FileAssistantTopMostTime { get; set; }
        /// <summary>
        /// 初始化当前登录用户
        /// </summary>
        /// <param name="login"></param>
        public LoginUser InitializeLoginer(MainViewModel main, LoginUser login)
        {
            MainMV = main;
            int id = login.User.ID;
            //确保对象唯一性，当前登录用户重新赋值
            UserModel user = GetUserModel(id, login.User);
            user.LinkDelType = -1;

            SetMutex(login.User);
            //在这里可以赋值某些数据，比如以后修改密码等
            return this.LoginUser = new LoginUser(user) { IsSavePassword = login.IsSavePassword };
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        public void InitializeDatasAsync()
        {
            SDKClient.SDKClient.Instance.NewDataRecv += Instance_NewDataRecv;
            SDKClient.SDKClient.Instance.P2PDataRecv += Instance_P2PDataRecv;
            SDKClient.SDKClient.Instance.OffLineMessageEventHandle += Instance_OffLineMessageEventHandle;
            ThreadPool.QueueUserWorkItem(m =>
            {
                SDKClient.SDKClient.Instance.GetUser(this.LoginUser.User.ID);
                SDKClient.SDKClient.Instance.GetUserPrivacySetting(this.LoginUser.User.ID);
                //  SDKClient.SDKClient.Instance.GetContactsList();
                SDKClient.SDKClient.Instance.GetGroupList();
                SDKClient.SDKClient.Instance.GetImDataListIncr();
            });
            ThreadPool.QueueUserWorkItem(m =>
            {
                //   SDKClient.SDKClient.Instance.GetAttentionList();
                //  SDKClient.SDKClient.Instance.GetBlackList();

                //SDKClient.SDKClient.Instance.GetOfflineMessageList();
                SDKClient.SDKClient.Instance.GetThefuckOfflineMessageList();

            });

            SDKClient.SDKClient.Instance.SendFaile += Instance_SendFaile1;
            SDKClient.SDKClient.Instance.SendConfirm += Instance_SendConfirm;
            SDKClient.SDKClient.Instance.ConnState += (s, isOnline) =>
            {
                this.LoginUser.IsOnline = isOnline;
                MainMV.LoginUser.IsOnline = isOnline;
            };
        }

        private void Instance_SendConfirm(object sender, (int roomId, bool isgroup, MessagePackage package, DateTime sendTime) e)
        {
            var room = MainMV.ChatListVM.Items.ToList().FirstOrDefault(info => info.ID == e.roomId && info.IsGroup == e.isgroup);
            if (room != null)
            {
                var msg = room.Chat.Messages.ToList().FirstOrDefault(info => info.MsgKey == e.package.id);
                if (e.package.code != 0)
                {
                    room.AddMessageTip(e.package.error);
                    if (msg != null)
                    {
                        msg.MessageState = MessageStates.Warn;
                        //if (SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden)
                        //    msg.MessageState = MessageStates.Fail;
                    }
                }

                room.IsDefriend();
                if (msg == null)
                {
                    msg = room.UnReadMsgs.ToList().FirstOrDefault(m => m.MsgKey == e.package.id);
                }
                if (msg != null)
                {
                    msg.SendTime = e.sendTime;
                    if ((msg.MessageState == MessageStates.Fail || msg.MessageState == MessageStates.Loading) && msg.MsgType != MessageType.notification)
                        msg.MessageState = MessageStates.Success;
                    else
                    {

                    }
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        MainMV.ChatListVM.ResetSort();
                    });

                }
                else
                {

                }
            }
            else
            {

            }
        }

        /// <summary>
        /// 离线消息推送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Instance_OffLineMessageEventHandle(object sender, SDKClient.DTO.OfflineMessageContext e)
        {
            var log = Util.Helpers.CodeTimer.Time("Instance_OffLineMessageEventHandle", 1, () =>
              {

                  var OfflineMessages = e.context;
                  if (OfflineMessages?.Count == 0)
                      return;
                  foreach (var offlineMsg in OfflineMessages)
                  {
                      MainMV.ChatListVM.HandleOfflineMessage(offlineMsg.Key.Item1, offlineMsg.Key.Item2, offlineMsg.Value);
                  }
              });
            NLog.LogManager.GetCurrentClassLogger().Info(log);
        }
        private void SetMutex(UserModel user)
        {
            System.Threading.Mutex temp;
#if RELEASE

            if (System.Threading.Mutex.TryOpenExisting(user.PhoneNumber, out temp))
            {
                App.ReStart(true);
                return;
            }
            else
            {
                App.MUTEX = new System.Threading.Mutex(true, user.PhoneNumber);
            }
#elif CHECK
            if (System.Threading.Mutex.TryOpenExisting(user.PhoneNumber + "cs", out temp))
            {
                App.ReStart(true);
                return;
            }
            else
            {
                App.MUTEX = new System.Threading.Mutex(true, user.PhoneNumber + "cs");
            }
#elif DEBUG
            if (System.Threading.Mutex.TryOpenExisting(user.PhoneNumber + "kf", out temp))
            {
                App.ReStart(true);
                return;
            }
            else
            {
                App.MUTEX = new System.Threading.Mutex(true, user.PhoneNumber + "kf");
            }
#elif HUIDU
            if (System.Threading.Mutex.TryOpenExisting(user.PhoneNumber + "hd", out temp))
            {
                App.ReStart(true);
                return;
            }
            else
            {
                App.MUTEX = new System.Threading.Mutex(true, user.PhoneNumber + "hd");
            }
#endif
        }

        private void Instance_SendFaile1(object sender, (int roomId, bool isgroup, string msgId) e)
        {
            var room = MainMV.ChatListVM.Items.ToList().FirstOrDefault(info => info.ID == e.roomId && info.IsGroup == e.isgroup);
            if (room != null)
            {
                MessageModel msg = room.Chat.Messages.ToList().FirstOrDefault(info => info.MsgKey == e.msgId);
                if (msg != null)
                {
                    if (msg.MessageState != MessageStates.Warn)
                    {
                        msg.MessageState = MessageStates.Fail;
                    }
                }
                else
                {

                }
            }
        }


        #region 哈希表数据获取

        /// <summary>
        /// 获取用户模型
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns></returns>
        public UserModel GetUserModel(int id, UserModel user = null)
        {
            Hashtable ht = Hashtable.Synchronized(AppData.UserHash);
            UserModel model = null;
            if (ht.ContainsKey(id))
            {
                model = ht[id] as UserModel;
            }
            else
            {
                if (user != null)
                    model = user;
                else
                    model = new UserModel() { ID = id, TopMostTime = DateTime.MinValue, HeadImg = IMAssets.ImageDeal.DefaultHeadImage };
                lock (ht.SyncRoot)
                {
                    ht.Add(id, model);
                }
            }

            return model;
        }

        /// <summary>
        /// 获取群组模型
        /// </summary>
        /// <param name="id">群ID</param>
        /// <returns></returns>
        public GroupModel GetGroupModel(int id)
        {
            Hashtable ht = Hashtable.Synchronized(AppData.GroupHash);

            GroupModel model = null;
            if (ht.ContainsKey(id))
            {
                model = ht[id] as GroupModel;
            }
            else
            {
                model = new GroupModel() { ID = id, HeadImg = IMAssets.ImageDeal.DefaultGroupHeadImage, TopMostTime = DateTime.MinValue };

                lock (ht.SyncRoot)
                {
                    ht.Add(id, model);
                }
            }
            return model;
        }

        /// <summary>
        /// 获取聊天模型
        /// </summary>
        /// <param name="chat">聊天对象</param>
        /// <returns></returns>
        public ChatModel GetChatViewModel(IChat chat)
        {
            Hashtable ht = Hashtable.Synchronized(AppData.ChatHash);
            ChatModel model = null;

            string key = (chat is GroupModel ? "g" : "u") + chat.ID;
            if (ht.Contains(key))
            {
                model = ht[key] as ChatModel;
            }
            else
            {
                model = new ChatModel(chat);
                lock (ht.SyncRoot)
                {
                    ht.Add(key, model);
                }
            }

            return model;
        }

        #endregion


        /// <summary>
        /// 判断是否可网络操作
        /// </summary>
        /// <param name="errorInfo">网络错误时提示内容</param>
        /// <returns></returns>
        public static bool CanInternetAction(string errorInfo = "当前无网络，请检查网络设置！")
        {
            if (MainMV.LoginUser.IsOnline && SDKClient.SDKClient.Instance.IsConnected)
            {
                MainMV.LoginUser.IsOnline = true;
                return true;
            }
            else
            {
                MainMV.LoginUser.IsOnline = false;
                if (!string.IsNullOrEmpty(errorInfo))
                    MainMV.TipMessage = errorInfo;
                return false;
            }
        }

        /// <summary>
        /// 接收到数据
        /// </summary> 
        private void Instance_NewDataRecv(object sender, SDKClient.Model.PackageInfo e)
        {
            switch (e.apiId)
            {
                case ProtocolBase.GetContactsListCode://获取联系人列表 
                    MainMV.FriendListVM.LoadDatas(e as SDKClient.Model.GetContactsListPackage);
                    break;
                case ProtocolBase.GetAttentionListCode://获取关注人列表
                    MainMV.AttentionListVM.LoadDatas(e as SDKClient.Model.GetAttentionListPackage);
                    break;
                case ProtocolBase.SetStrangerDoNotDisturbCode://设置粉丝留言免打扰
                    SetStrangerDoNotDisturbPackage pck = e as SetStrangerDoNotDisturbPackage;
                    if (pck != null && pck.data != null)
                    {
                        UserModel userModel = AppData.Current.GetUserModel(pck.data.strangerId);
                        userModel.IsNotDisturb = pck.data.isNotdisturb == 1 ? true : false;
                    }
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        MainMV.ChatListVM.IsCloseTrayWindow(false);
                    });
                    break;
                case ProtocolBase.SysNotifyCode://通知消息
                    SysNotifyPackage snp = e as SysNotifyPackage;
                    ReceivedSysNotifyResponse(snp);
                    break;
                case ProtocolBase.AddAttentionCode://关注陌生人
                                                   //MainMV.AttentionListVM.LoadDatas(e as SDKClient.Model.GetAttentionListPackage);
                                                   //break;
                case ProtocolBase.DeleteAttentionUserCode://取消关注 
                    if (e.apiId == ProtocolBase.AddAttentionCode)
                    {
                        var package = e as AddAttentionPackage;
                        var tempAttentions = MainMV.SearchUserListVM.Items.ToList();
                        UserViewModel tempUser = null;
                        if (package.data != null && package.data.strangerId > 0)
                        {
                            tempUser = tempAttentions.FirstOrDefault(m => m.ID == package.data.strangerId);
                        }
                        if (e.code == -101)
                        {
                            return;
                        }
                        if (!string.IsNullOrEmpty(e.error))
                        {
                            if (tempUser != null && tempUser.IsAuto)
                                AppData.MainMV.TipMessage = e.error;
                            if (tempUser != null && e.code == 654)
                            {
                                var user = tempUser.Model as UserModel;
                                if (AppData.MainMV.BlacklistVM != null && AppData.MainMV.BlacklistVM.Items.Count > 0)
                                {
                                    var blackUser = AppData.MainMV.BlacklistVM.Items.ToList().FirstOrDefault(m => m.Model.ID == user.ID);
                                    if (blackUser != null && (blackUser.Model as UserModel).AttentionID == 0)
                                    {
                                        user.IsAttention = true;
                                        user.AttentionID = package.data.strangerId;
                                    }
                                    else if (blackUser == null)
                                    {
                                        user.IsAttention = false;
                                    }
                                }
                                else
                                {

                                    user.IsAttention = false;
                                }
                            }
                        }
                        else
                        {
                            //AppData.MainMV.TipMessage = "关注成功！";
                            if (tempUser != null && tempUser.IsAuto)
                            {
                                tempUser.AttentionResult();
                                tempUser.IsAuto = false;
                            }
                            if (tempUser != null)
                            {
                                var user = tempUser.Model as UserModel;
                                user.IsAttention = true;
                            }
                        }
                        SDKClient.SDKClient.Instance.GetAttentionList();
                    }
                    else
                    {
                        var package = e as DeleteAttentionUserPackage;
                        if (package.data != null && package.data.strangerLinkId > 0)
                        {
                            UserModel model = AppData.MainMV.AttentionListVM.Items.ToList().FirstOrDefault(x => (x.Model as UserModel).AttentionID == package.data.strangerLinkId).Model as UserModel;
                            model.IsAttention = false;
                        }
                        //取消关注，直接重新获取关注列表
                        SDKClient.SDKClient.Instance.GetAttentionList();
                    }


                    break;
                case ProtocolBase.GetgroupListCode: //获取群（元数据）列表 
                    MainMV.GroupListVM.DealPackageResult(GroupActions.Load, e);
                    break;
                case ProtocolBase.GetUserCode: //获取用户资料                    
                    GetUserInfo(e as GetUserPackage);
                    break;
                case ProtocolBase.messageCode://消息
                    if (e is SDKClient.Model.MessagePackage msgPackage)
                    {
                        MainMV.ChatListVM.ReceiveMsg(msgPackage);
                    }
                    break;
                case ProtocolBase.GetGroupCode:
                    GetGroupInfo(e as GetGroupPackage);
                    break;
                case ProtocolBase.GetgroupMemberCode:// 获取的那个群成员资料
                    MainMV.GroupListVM.DealPackageResult(GroupActions.UpdateMemberInfo, e);
                    break;
                case ProtocolBase.GetgroupMemberListCode://获取群成员列表 
                    GetGroupMemberListPackage gmlPG = e as GetGroupMemberListPackage;
                    if (gmlPG == null || gmlPG.code != 0 || gmlPG.data == null || gmlPG.data.items == null)
                    {
                        return;
                    }
                    var list = AppData.MainMV.GroupListVM.Items.ToList();
                    GroupViewModel groupVM = list.FirstOrDefault(x => x.ID == gmlPG.data.groupId);

                    if (groupVM != null)
                    {
                        App.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            groupVM.LoadMembers(gmlPG.data.items);
                        }));
                    }
                    else
                    {
                        //groupVM=AppData.MainMV.GroupListVM.DissolveGroups.FirstOrDefault(x => x.ID == gmlPG.data.groupId);
                        //groupVM?.LoadMembers(gmlPG.data.items);
                    }
                    break;
                case ProtocolBase.SearchNewFriendCode://搜索好友 
                    //MainMV.SearchUserListVM.LoadData(e as SearchNewFriendPackage);
                    break;
                case ProtocolBase.SetMemberPowerCode://设置成员权限
                    MainMV.GroupListVM.DealPackageResult(GroupActions.SetMemberPower, e as SetMemberPowerPackage);
                    break;
                case ProtocolBase.AddFriendCode: //添加好友消息 
                    var applyPG = e as AddFriendPackage;
                    if (applyPG.code == 0 && applyPG.from.Equals(AppData.Current.LoginUser.ID.ToString()))
                    {
                        return;
                    }

                    if (applyPG != null && applyPG.code == 0 && applyPG.data != null)
                    {
                        MainMV.FriendListVM.ApplyUserListVM.ReceiveApply(applyPG, false, false, null);
                    }
                    break;
                case ProtocolBase.AddFriendAcceptedCode: //好友申请通过
                    MainMV.FriendListVM.ApplyUserListVM.PackageDeal(e as AddFriendAcceptedPackage);
                    break;
                case ProtocolBase.CreateGroupCode: //创建群组  
                    MainMV.GroupListVM.DealPackageResult(GroupActions.Create, e);
                    break;
                case ProtocolBase.DismissGroupCode://解散群组  
                    MainMV.GroupListVM.DealPackageResult(GroupActions.Dismiss, e);
                    break;
                case ProtocolBase.ExitGroupCode://退出群组  
                    MainMV.GroupListVM.DealPackageResult(GroupActions.Exit, e);
                    break;
                case ProtocolBase.InviteJoinGroupCode://邀请入群
                    MainMV.GroupListVM.DealPackageResult(GroupActions.Invite, e);
                    break;
                case ProtocolBase.JoinGroupCode://入群申请 
                    MainMV.GroupListVM.DealPackageResult(GroupActions.JoinApply, e);
                    break;
                case ProtocolBase.JoinGroupAcceptedCode://通过入群申请 
                    MainMV.GroupListVM.DealPackageResult(GroupActions.JoinAccepted, e);
                    break;
                case ProtocolBase.UpdateGroupCode://更新群（元数据）信息  
                    MainMV.GroupListVM.DealPackageResult(GroupActions.UpdateInfo, e);
                    break;
                case ProtocolBase.UpdateUserSetsInGroupCode://成员更新群信息  
                    MainMV.GroupListVM.DealPackageResult(GroupActions.UpdateMemberInfo, e);
                    break;
                case ProtocolBase.UpdateUserCode://更新个人信息
                    MainMV.UpdateUser(e as UpdateuserPackage);
                    break;
                case ProtocolBase.UpdateFriendSetCode://更新用户备注
                    MainMV.FriendListVM.UpdateFriendSet(e as UpdateFriendSetPackage);
                    break;
                case ProtocolBase.GetUserPrivacySettingCode://
                    MainMV.GetUserPrivacySetting(e as GetUserPrivacySettingPackage);
                    break;
                case ProtocolBase.GetOfflineMessageListCode://离线消息
                    MainMV.ChatListVM.LoadOfflineMsgs(null);
                    break;
                case ProtocolBase.GetFriendApplyListCode://获取好友申请列表
                    //GetFriendApplyList(e as GetFriendApplyListPackage);
                    break;
                case ProtocolBase.GetBlackListCode://获取黑名单列表
                    MainMV.BlacklistVM.LoadData(e as GetBlackListPackage);
                    break;
                case ProtocolBase.MessageConfirmCode://确认消息                    
                    break;
                case ProtocolBase.authCode:
                    ReceivedAuthResponse(e as AuthPackage);
                    break;
                case ProtocolBase.loginCode:
                    ReceivedloginResponse(e as LoginPackage);
                    break;
                case ProtocolBase.DeleteFriendCode://删除好友
                    if (e is DeleteFriendPackage delFriendPG)
                    {
                        if (delFriendPG == null || delFriendPG.data == null || delFriendPG.code != 0)
                        {
                        }
                        else
                        {
                            int operateID = Convert.ToInt32(delFriendPG.from);
                            int deleteID = delFriendPG.data.friendId;
                            int type = delFriendPG.data.type;
                            bool isSync = delFriendPG.syncMsg == 1 ? true : false;
                            bool isAutoDel = type == 1 ? true : false;
                            MainMV.FriendListVM.DealDeleteFriend(operateID, deleteID, isSync, isAutoDel);
                        }
                    }
                    break;
                case ProtocolBase.UpdateFriendRelationCode://更新好友关系（拉黑或取消拉黑）
                    MainMV.FriendListVM.UpdateFriendRelation(e as UpdateFriendRelationPackage);
                    break;
                case ProtocolBase.SyncMsgStatusCode:
                    if (e is SyncMsgStatusPackage syncMsgState)
                    {
                        int chatID = 0;

                        if (syncMsgState.data.partnerId > 0)
                        {
                            chatID = syncMsgState.data.partnerId;
                        }
                        else if (syncMsgState.data.groupId > 0)
                        {
                            chatID = syncMsgState.data.groupId;
                        }
                        else
                        {
                            return;
                        }

                        var chatVM = MainMV.ChatListVM.Items.ToList().FirstOrDefault(info => info.ID == chatID);
                        if (chatVM == null)
                        {
                            chatVM = MainMV.ChatListVM.Items.ToList().FirstOrDefault(m => m.ID < 0);
                            if (chatVM == null)
                                return;
                        }
                        if (chatVM != null)
                        {
                            if (chatVM.StrangerMessageList != null && chatVM.StrangerMessageList.Count > 0)
                            {
                                var tempStrangerMsgsList = chatVM.StrangerMessageList.ToList();
                                var strangerMsg = tempStrangerMsgsList.FirstOrDefault(m => m.ID == chatID);
                                if (strangerMsg != null)
                                {
                                    strangerMsg.IsAllRead = true;
                                    var model = strangerMsg.Model as ChatModel;
                                    if (model != null && model.Messages != null)
                                    {
                                        foreach (var msg in model.Messages)
                                        {
                                            //if (msg.IsRead == 0)
                                            msg.IsRead = 1;
                                        }
                                    }

                                    strangerMsg.UnReadCount = 0;
                                    //strangerMsg
                                    //strangerMsg.
                                }

                            }
                            chatVM.SetAllRead();
                        }
                    }
                    break;
                case SDKClient.Protocol.ProtocolBase.ForceExitCode: //手机端强制PC端退出
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {

                        SDKClient.SDKClient.Instance.SendLogout(SDKClient.SDKProperty.LogoutModel.Logout_kickout);

                        App.Logger.Error("收到强制退出命令：", e.ToString());

                        App.ReStart();
                    }));
                    break;


                default:
                    break;
            }
        }

        private void ReceivedSysNotifyResponse(SysNotifyPackage snp)
        {
            if (snp.code == 0 && snp.data.type == 1)
            {
                ///
                ///  101 –帐户封停
                ///   102 –帐户解封
                ///   103-禁止发消息 
                ///   104-解禁发消息
                ///   105-禁止发动态
                /// 106解禁发动态
                /// 107 用户登录官网通知
                /// 108用户退出官网通知
                /// <summary>
                /// 301-下单成功302-支付成功303-已发货304-被分享人开店305-店主获得收益
                /// </summary>
                switch (snp.data.subType)
                {
                    case 101:
                        fengjinBody body = new fengjinBody() { disableTime = snp.data.body.disableTime, userId = snp.data.body.userId };
                        long days = body.disableTime / (24 * 60 * 60);
                        long hours = (body.disableTime % (24 * 60 * 60)) / (60 * 60);
                        long minutes = ((body.disableTime % (24 * 60 * 60)) % (60 * 60)) / 60;
                        string str = string.Empty;
                        if (days > 0)
                            str = string.Format("该账号已封停，剩余解封时间：{0}天{1}小时，若有疑问请联系客服：{2}", days, hours, SDKClient.Protocol.GlobalConfig.CompanyPhone);
                        else
                            str = string.Format("该账号已封停，剩余解封时间：{0}小时{1}分钟，若有疑问请联系客服：{2}", hours, minutes, SDKClient.Protocol.GlobalConfig.CompanyPhone);
                        MainMV.ForceOffline(str);
                        break;
                    case 102:
                        break;
                    case 103:
                        break;
                    case 104:
                        break;
                    case 107:
                        break;
                    case 108:
                        break;
                    default:
                        break;
                }
            }
        }

        private void Instance_P2PDataRecv(object sender, SDKClient.P2P.P2PPackage e)
        {
            switch (e.PackageCode)
            {
                case SDKClient.P2P.P2PPakcageState.none:
                    break;
                case SDKClient.P2P.P2PPakcageState.cancel:
                    break;
                case SDKClient.P2P.P2PPakcageState.complete:
                    break;
                case SDKClient.P2P.P2PPakcageState.@continue:
                    break;
                case SDKClient.P2P.P2PPakcageState.stop:
                    break;
            }
        }

        void ReceivedAuthResponse(AuthPackage packageInfo)
        {
            if (packageInfo.code == 0 && packageInfo.syncMsg != 1)
            {
                DeleteFriendIDItems.Clear();
                DismissGroupIDItems.Clear();
                if (MessageModels.Count > 0)
                {
                    foreach (var msg in MessageModels)
                    {
                        msg.ReSendWaitMsg();
                    }
                    MessageModels.Clear();
                }
                ThreadPool.QueueUserWorkItem(m =>
                {
                    SDKClient.SDKClient.Instance.GetUser(this.LoginUser.User.ID);
                    SDKClient.SDKClient.Instance.GetUserPrivacySetting(this.LoginUser.User.ID);
                    //  SDKClient.SDKClient.Instance.GetContactsList();
                    SDKClient.SDKClient.Instance.GetGroupList();
                    SDKClient.SDKClient.Instance.GetImDataListIncr();
                });
                ThreadPool.QueueUserWorkItem(m =>
                {
                    //SDKClient.SDKClient.Instance.GetAttentionList();
                    //SDKClient.SDKClient.Instance.GetBlackList();
                    SDKClient.SDKClient.Instance.GetThefuckOfflineMessageList();
                });

            }
            else if (packageInfo.code == 0 && packageInfo.syncMsg == 1)
            {
                AppData.MainMV.ChatListVM?.AddFileAssistantItem();
            }
            else if (packageInfo.code == 403)
            {
                //下线通知
                MainMV.ForceOffline(null);
            }
        }
        void ReceivedloginResponse(LoginPackage packageInfo)
        {

            if (packageInfo.code != 0)
            {
                //弹窗显示错误信息
                //MainMV.ForceOffline(packageInfo.error);
                ForceOfflineWindow win = new ForceOfflineWindow(packageInfo.error);
                win.ShowDialog();
            }
        }

        /// <summary>
        /// 获取用户信息
        /// 刷新对象
        /// </summary>
        /// <param name="pg"></param>
        private void GetUserInfo(GetUserPackage pg)
        {
            if (pg == null || pg.code != 0 || pg.data == null || pg.data.user == null)
            {
                return;
            }

            var data = pg.data.user;

            if (UserHash.ContainsKey(data.userId))
            {
                UserModel user = UserHash[data.userId] as UserModel;
                var oldName = user.Name;
                if (string.IsNullOrEmpty(user.NickName) && user.DisplayName != data.userName)
                    oldName = user.Name;
                user.Name = data.userName;
                user.KfNum = data.kfId;
                if (user.LinkDelType == 1 || user.LinkDelType == 3)
                {
                    if (!string.IsNullOrEmpty(user.KfNum) && user.KfNum.Length > 4)
                    {
                        var kfNum = user.KfNum.Remove(user.KfNum.Length - 4, 4);

                        user.KfNum = kfNum.Insert(kfNum.Length, "****");
                    }
                }
                else
                {
                    user.KfNum = data.kfId;
                }

                user.DisplayName = string.IsNullOrEmpty(user.NickName) ? user.Name : user.NickName;
                if (data.userId == AppData.Current.LoginUser.ID)
                {
                    user.HaveModifiedKfid = data.haveModifiedKfid;
                }
                user.Sex = data.sex.ToString();
                user.PhoneNumber = data.mobile;
                user.Area = string.Format("{0} {1}", data.areaAName, data.areaBName);
                user.HeadImgMD5 = data.photo;

                App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                {
                    var isHeadImgUpdate = true;
                    if (!user.HeadImg.Equals(IMAssets.ImageDeal.DefaultHeadImage))
                    {
                        if (File.Exists(user.HeadImg))
                        {
                            var info = new FileInfo(user.HeadImg);
                            if (info.Name == data.photo)
                            {
                                isHeadImgUpdate = false;
                            }
                        }
                    }

                    if (isHeadImgUpdate)
                        user.HeadImg = IMClient.Helper.ImageHelper.GetFriendFace(data.photo, 
                            (s) =>
                            {
                                try
                                {
                                    App.Current.Dispatcher.Invoke(() => user.HeadImg = s);
                                }
                                catch (Exception)
                                {

                                    throw;
                                }
                            
                            });
                }));

                if (user.ID == LoginUser.ID)
                {
                    this.LoginUser.User.DisplayName = user.DisplayName;
                    MainMV.MainTitle = $"{user.Name}({user.PhoneNumber})";
                    App.UpdateTray(user);
                }
                else
                {
                    if (oldName != user.DisplayName)
                    {
                        AppData.MainMV.FriendListVM.UpdateFriendSort(user.ID);
                    }
                }
            }
        }

        /// <summary>
        /// 获取群信息
        /// 刷新群对象
        /// </summary>
        /// <param name="package"></param>
        private void GetGroupInfo(GetGroupPackage package)
        {
            if (package == null || package.code != 0 || package.data == null || package.data.item == null)
            {
                return;
            }

            var data = package.data.item;
            if (GroupHash.ContainsKey(data.groupId))
            {
                GroupModel group = GroupHash[data.groupId] as GroupModel;

                group.GroupRemark = data.groupIntroduction;
                group.DisplayName = data.groupName;
                //group.IsNotDisturb = data.doNotDisturb;
                //group.TopMostTime = data.groupTopTime.HasValue ? data.groupTopTime.Value : DateTime.MinValue;
                //group.IsTopMost = (group.TopMostTime == DateTime.MinValue) ? false : true;

                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    group.HeadImg = IMClient.Helper.ImageHelper.GetFriendFace(data.groupPhoto, (s) => group.HeadImg = s);
                }));
                group.HeadImgMD5 = data.groupPhoto;
                group.OwnerID = data.groupOwnerId;
            }
        }
        public static FowardWindow FowardWindow;
        public static void FowardMsg(int chatID, string msgID, bool isGroup = false, bool isInviteJoin = false)
        {
            FowardWindow = new FowardWindow(chatID, msgID, isGroup, isInviteJoin);
            FowardWindow.Owner = Application.Current.MainWindow;
            FowardWindow.ShowDialog();
        }

        public static SendPersonCardWindow PersonCardWindow;
        /// <summary>
        /// 发送名片
        /// </summary>
        /// <param name="chatId">聊天Id</param>
        /// <param name="isFromCard">是否是从名片上点击发送（有两个地方可发送名片，逻辑不一样，聊天窗口发送时为false，从个人卡片为true）</param>
        /// <param name="isGroup">是否为群</param>
        public static void SendPersonCard(int chatId, UserModel userModel, bool isFromCard, bool isGroup)
        {
            PersonCardWindow = new SendPersonCardWindow(chatId, userModel, isFromCard, isGroup);
            PersonCardWindow.Owner = App.Current.MainWindow;
            PersonCardWindow.ShowDialog();
        }

    }
}
