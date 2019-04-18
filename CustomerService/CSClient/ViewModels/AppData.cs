using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMModels;
using SDKClient.Model;
using SDKClient.Protocol;
using CSClient.Helper;
using SDKClient.WebAPI;

namespace CSClient.ViewModels
{
    public class AppData
    {
        private static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        public static void TestStart()
        {
            sw.Reset();
            sw.Start();
        }

        public static void TestStop(string appendInfo)
        {
            sw.Stop();

            Console.WriteLine("【{0}】{1}", appendInfo, sw.ElapsedMilliseconds);
        }

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


        /// <summary>
        /// 当前登录用户
        /// </summary>
        public LoginUser LoginUser { get; private set; }
        /// <summary>
        /// 初始化当前登录用户
        /// </summary>
        /// <param name="login"></param>
        public LoginUser InitializeLoginer(MainViewModel main, LoginUser login)
        {
            MainMV = main;
            int id = login.User.ID;
            //确保对象唯一性，当前登录用户重新赋值
            UserModel user = GetUserModel(id);
            user.PhoneNumber = login.User.PhoneNumber;
            user.LinkDelType = -1;
            user.HeadImg = login.User.HeadImg;
            //在这里可以赋值某些数据，比如以后修改密码等
            return this.LoginUser = new LoginUser(user) { IsSavePassword = login.IsSavePassword };
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        public void InitializeDatas()
        {
            SDKClient.SDKClient.Instance.NewDataRecv += Instance_NewDataRecv;
            SDKClient.SDKClient.Instance.P2PDataRecv += Instance_P2PDataRecv;
            //SDKClient.SDKClient.Instance.GetContactsList();
            //SDKClient.SDKClient.Instance.GetGroupList();
            //SDKClient.SDKClient.Instance.GetAttentionList();
            //SDKClient.SDKClient.Instance.GetUser(this.LoginUser.User.ID);
            // SDKClient.SDKClient.Instance.GetOfflineMessageList();
            SDKClient.SDKClient.Instance.SendFaile += Instance_SendFaile1;

            SDKClient.SDKClient.Instance.ConnState += (s, isOnline) =>
            {
                this.LoginUser.IsOnline = isOnline;
            };
        }


        private void Instance_SendFaile1(object sender, (int roomId, bool isgroup, string msgId) e)
        {
            var room = MainMV.ChatListVM.Items.FirstOrDefault(info => info.ID == e.roomId && info.IsGroup == e.isgroup);

            if (room != null)
            {
                MessageModel msg = room.Chat.Messages.FirstOrDefault(info => info.MsgKey == e.msgId);
                if (msg != null)
                {
                    if (msg.MessageState != MessageStates.Warn)
                    {
                        msg.MessageState = MessageStates.Fail;
                    }
                }
            }
        }


        #region 哈希表数据获取

        /// <summary>
        /// 获取用户模型
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns></returns>
        public UserModel GetUserModel(int id)
        {
            Hashtable ht = Hashtable.Synchronized(AppData.UserHash);
            UserModel model = null;
            if (ht.ContainsKey(id))
            {
                model = ht[id] as UserModel;
            }
            else
            {
                model = new UserModel() { ID = id, HeadImg = ImagePathHelper.DefaultUserHead };
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
            if (ht.Contains(chat.ID))
            {
                model = ht[chat.ID] as ChatModel;
            }
            else
            {
                model = new ChatModel(chat);
                lock (ht.SyncRoot)
                {
                    ht.Add(chat.ID, model);
                }
            }

            return model;
        }

        public ChatModel GetChatViewModelEx(IChat chat)
        {
            Hashtable ht = Hashtable.Synchronized(AppData.ChatHash);
            ChatModel model = null;
            if (ht.Contains(chat.DisplayName))
            {
                model = ht[chat.DisplayName] as ChatModel;
            }
            else
            {
                model = new ChatModel(chat);
                lock (ht.SyncRoot)
                {
                    ht.Add(chat.DisplayName, model);
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
            if (SDKClient.SDKClient.Instance.IsConnected)
            {
                return true;
            }
            else
            {
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
                    //MainMV.FriendListVM.LoadDatas(e as SDKClient.Model.GetContactsListPackage);
                    break;
                case ProtocolBase.GetAttentionListCode://获取关注人列表
                    //MainMV.AttentionListVM.LoadDatas(e as SDKClient.Model.GetAttentionListPackage);
                    break;
                case ProtocolBase.AddAttentionCode://关注陌生人
                                                   //MainMV.AttentionListVM.LoadDatas(e as SDKClient.Model.GetAttentionListPackage);
                                                   //break;
                case ProtocolBase.DeleteAttentionUserCode://取消关注 
                    //取消关注，直接重新获取关注列表
                    SDKClient.SDKClient.Instance.GetAttentionList();
                    break;
                case ProtocolBase.GetgroupListCode: //获取群（元数据）列表 
                    //MainMV.GroupListVM.LoadDatas(e as SDKClient.Model.GetGroupListPackage);
                    //MainMV.GroupListVM.DealPackageResult(GroupActions.Load, e);
                    break;
                case ProtocolBase.GetUserCode: //获取用户资料                    
                    GetUserInfo(e as GetUserPackage);
                    break;
                
                case ProtocolBase.messageCode://消息
                    MainMV.ChatListVM.ReceiveMsg(e as SDKClient.Model.MessagePackage);
                    //ReceivedMessage(e as MessagePackage);
                    break;
                case ProtocolBase.GetGroupCode:
                    GetGroupInfo(e as GetGroupPackage);
                    break;
                case ProtocolBase.GetgroupMemberCode:// 获取的那个群成员资料
                    //MainMV.GroupListVM.DealPackageResult(GroupActions.UpdateMemberInfo, e);
                    break;
                case ProtocolBase.GetgroupMemberListCode://获取群成员列表 

                    GetGroupMemberListPackage gmlPG = e as GetGroupMemberListPackage;
                    if (gmlPG == null || gmlPG.code != 0 || gmlPG.data == null || gmlPG.data.items == null)
                    {
                        return;
                    }

                    break;
                case ProtocolBase.SearchNewFriendCode://搜索好友 
                    //MainMV.SearchUserListVM.LoadData(e as SearchNewFriendPackage);
                    break;
                case ProtocolBase.SetMemberPowerCode://设置成员权限 
                    break;
                case ProtocolBase.AddFriendCode: //添加好友消息 
                    var applyPG = e as AddFriendPackage;
                    if (applyPG.code == 0 && applyPG.from.Equals(AppData.Current.LoginUser.ID.ToString()))
                    {
                        return;
                    }

                    break;
                case ProtocolBase.AddFriendAcceptedCode: //好友申请通过
                    //MainMV.FriendListVM.ApplyUserListVM.PackageDeal(e as AddFriendAcceptedPackage);
                    break;
                case ProtocolBase.CreateGroupCode: //创建群组   
                    break;
                case ProtocolBase.DismissGroupCode://解散群组   
                    break;
                case ProtocolBase.ExitGroupCode://退出群组   
                    break;
                case ProtocolBase.InviteJoinGroupCode://邀请入群 
                    break;
                case ProtocolBase.JoinGroupCode://入群申请  
                    break;
                case ProtocolBase.JoinGroupAcceptedCode://通过入群申请  
                    break;
                case ProtocolBase.UpdateGroupCode://更新群（元数据）信息   
                    break;
                case ProtocolBase.UpdateUserSetsInGroupCode://成员更新群信息   
                    break;
                case ProtocolBase.UpdateUserCode://更新个人信息
                                                 // UpdateUser(e as UpdateuserPackage);
                    break;
                case ProtocolBase.UpdateFriendSetCode://更新用户备注 
                    break;
                case ProtocolBase.GetOfflineMessageListCode://离线消息
                    MainMV.ChatListVM.LoadOfflineMsgs(null);
                    break;
                case ProtocolBase.GetFriendApplyListCode://获取好友申请列表
                    //GetFriendApplyList(e as GetFriendApplyListPackage);
                    break;
                case ProtocolBase.GetBlackListCode://获取黑名单列表
                    //GetBlackList(e as GetBlackListPackage);
                    break;
                case ProtocolBase.MessageConfirmCode://确认消息                    
                    break;
                case ProtocolBase.authCode:
                    ReceivedAuthResponse(e as AuthPackage);
                    break;
                case ProtocolBase.DeleteFriendCode://删除好友
                    if (e is DeleteFriendPackage delFriendPG)
                    {
                    }
                    break;
                case ProtocolBase.UpdateFriendRelationCode://更新好友关系（拉黑或取消拉黑）
                    //MainMV.FriendListVM.UpdateFriendRelation(e as UpdateFriendRelationPackage);
                    break;
                case ProtocolBase.CustomServiceCode:
                    MainMV.ChatListVM.SetSession(e as CustomServicePackage);
                    MainMV.HisChatListVM.CustomService(e as CustomServicePackage);
                   // MainMV.ChatHistoryListVM.CustomService(e as CustomServicePackage);
                    break;
                case ProtocolBase.CustomExchangeCode:
                    MainMV.ChatListVM.CustomExchange(e as CustomExchangePackage);
                    MainMV.HisChatListVM.CustomExchange(e as CustomExchangePackage);
                    break;
                case ProtocolBase.SysNotifyCode:
                    MainMV.ChatListVM.EndSession(e as SysNotifyPackage);
                    break;
                case ProtocolBase.CSSyncMsgStatusCode:
                    MainMV.ChatListVM.CSSyncMsgStatus(e as CSSyncMsgStatusPackage);
                    MainMV.HisChatListVM.CSSyncMsgStatus(e as CSSyncMsgStatusPackage);
                    break;
                default:
                    break;
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
            if (packageInfo.code == 0)
            {
               // SDKClient.SDKClient.Instance.GetOfflineMessageList();
            }
            else if (packageInfo.code == 403)
            {
                //下线通知
                MainMV.ForceOffline();
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
                user.Name = data.userName;

                user.DisplayName = string.IsNullOrEmpty(user.NickName) ? user.Name : user.NickName;

                user.Sex = data.sex.ToString();
                user.PhoneNumber = data.mobile;
                user.Area = string.Format("{0} {1}", data.areaAName, data.areaBName);

                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    user.HeadImg = CSClient.Helper.ImageHelper.GetFriendFace(data.photo, (s) => user.HeadImg = s);
                }));
                
                if (user.ID == LoginUser.ID)
                {
                    this.LoginUser.User.DisplayName = user.DisplayName;
                    MainMV.MainTitle = $"{user.Name}({user.PhoneNumber})";
                    App.UpdateTray(user);
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
                //model.Name = model.NickName = group.groupName ?? string.Empty; 
                group.DisplayName = data.groupName;
                //model.PersonalizedSignature = group.groupIntroduction;

                //group.IsNotDisturb = data.doNotDisturb;
                //group.TopMostTime = data.groupTopTime.HasValue ? data.groupTopTime.Value : DateTime.MinValue;
                //group.IsTopMost = (group.TopMostTime == DateTime.MinValue) ? false : true;

                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    group.HeadImg = CSClient.Helper.ImageHelper.GetFriendFace(data.groupPhoto, (s) => group.HeadImg = s);
                }));
                group.HeadImgMD5 = data.groupPhoto;
                group.OwnerID = data.groupOwnerId;
            }
        }
    }
}
