using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using IMModels;
using SDKClient.Model;
using Util;
using IMClient.Helper;

namespace IMClient.ViewModels
{
    /// <summary>
    /// 群组列表VM
    /// </summary>
    public class GroupListViewModel : ListViewModel<GroupViewModel>
    {
        /// <summary>
        /// 已经退出的群
        /// </summary>
        public List<GroupViewModel> DissolveGroups { get; }

        /// <summary>
        /// 群组列表VM
        /// </summary>
        /// <param name="view"></param>
        public GroupListViewModel(IListView view) : base(view)
        {
            DissolveGroups = new List<GroupViewModel>();
        }

        private VMCommand _groupCreateCommand;

        /// <summary>
        /// 创建群组命令
        /// </summary> 
        public VMCommand GroupCreateCommand
        {
            get
            {
                if (_groupCreateCommand == null)
                    _groupCreateCommand = new VMCommand(GroupCreate);
                return _groupCreateCommand;
            }
        }

        /// <summary>
        /// 创建群
        /// </summary> 
        private void GroupCreate(object para)
        {
            if (para is IEnumerable<UserModel> users)
            {
                if (AppData.CanInternetAction())
                {
                    //允许发送邀请的好友id列表
                    var ids = users.Where(info => info.LinkDelType == 0 && info.LinkType == 0).Select(info => info.ID).ToList();
                    //非正常好友关系的id列表
                    var failIDs = users.Where(info => info.LinkDelType != 0 || info.LinkType != 0).Select(info => info.ID).ToArray();

                    //两个以上 加上 当前登录用户 共3个人以上才可以群聊
                    if (ids.Count > 1)
                    {
                        List<string> heads = users.Where(info => info.LinkDelType == 0).Select(info => info.HeadImg).ToList();
                        //加入当前用户的ID
                        heads.Insert(0, AppData.Current.LoginUser.User.HeadImg);

                        string groupHead = Helper.ImageHelper.JoinImage(heads.ToArray(), 300, 300);

                        string guid = SDKClient.SDKClient.Instance.CreateGroup(ids, groupHead);

                        if (failIDs.Length > 0) //等待建群成功返回数据后处理
                        {
                            AppData.CreateGroupWaits.Add(guid, failIDs);
                        }
                    }
                    else
                    {
                        if (ids.Count == 1)
                        {
                            ChatViewModel cVM = AppData.MainMV.ChatListVM.GetChat(ids[0]);

                            AppData.MainMV.ChatListVM.SelectedItem = cVM;
                            AppData.MainMV.ListViewModel = AppData.MainMV.ChatListVM;
                            AppData.MainMV.ChatListVM.IsChecked = true;

                            //AppData.MainMV.JumpToChatModel(cVM.Chat.Chat);
                        }
                        AppData.MainMV.TipMessage = "您所邀请群聊的好友中，有人已将您删除或拉黑，未满3人无法创建群聊！";
                    }
                }
            }
        }
        #region 搜索
        protected override IEnumerable<GroupViewModel> GetSearchResult(string key)
        {
            return this.Items.ToList().Where(info => (info.Model as GroupModel).DisplayName.Contains(key));
        }
        //protected override IEnumerable<GroupViewModel> GetSearchContactResult(string key)
        //{
        //    return null;
        //}

        //protected override IEnumerable<GroupViewModel> GetSearchBlackResult(string key)
        //{
        //    return null;
        //}
        //protected override IEnumerable<GroupViewModel> GetSearchGroupResult(string key)
        //{
        //    return null;
        //}
        #endregion
        public void DealPackageResult(GroupActions action, SDKClient.Model.PackageInfo pg, bool isForward = false)
        {
            if (pg == null)
            {
                return;
            }

            switch (action)
            {
                case GroupActions.Create:
                    DealCreate(pg as CreateGroupComponsePackage);
                    break;
                case GroupActions.Invite:
                    DealInvite(pg as InviteJoinGroupPackage);
                    break;
                case GroupActions.JoinApply:
                    DealJoinApply(pg as JoinGroupPackage);
                    break;
                case GroupActions.JoinAccepted:
                    DealJoinAccepted(pg as JoinGroupAcceptedPackage);
                    break;
                case GroupActions.Load:
                    DealLoadDatas(pg as GetGroupListPackage);
                    break;
                case GroupActions.UpdateInfo:
                    DealUpdateInfo(pg as UpdateGroupPackage);
                    break;
                case GroupActions.Expel:
                    break;
                case GroupActions.Exit:
                    if (pg.code == 0 && pg is ExitGroupPackage exit)
                    {
                        this.DealExit(exit, isForward);
                    }
                    break;
                case GroupActions.Dismiss:
                    if (pg.code == 0 && pg is DismissGroupPackage dismiss)
                    {
                        this.DealDismiss(dismiss, isForward);
                    }
                    break;
                case GroupActions.UpdateMemberInfo:

                    if (pg is GetGroupMemberPackage gmPG)
                    {
                        if (gmPG.code != 0)
                        {
                            return;
                        }

                        var user = AppData.Current.GetUserModel(gmPG.data.user.userId);
                        var group = AppData.Current.GetGroupModel(gmPG.data.user.groupId);
                        var gm = user.GetInGroupMember(group);
                        if (gm != null)
                        {
                            user.Name = gmPG.data.user.userName;
                            if (string.IsNullOrEmpty(gmPG.data.user.memoInGroup))
                            {
                                //gm.NickNameInGroup = gmPG.data.user.userName;
                            }
                            else
                            {
                                gm.NickNameInGroup = gmPG.data.user.memoInGroup;
                            }
                        }

                    }
                    else
                    {
                        this.DealUpdateMemberInfo(pg as UpdateUserSetsInGroupPackage);
                    }
                    break;
                case GroupActions.SetMemberPower:
                    if (pg.code == 0 && pg is SetMemberPowerPackage smp)
                    {
                        this.AddMemberPowerMessage(smp, isForward);
                    }
                    break;
            }
        }

        #region private Methods

        #region LoadGroupList
        /// <summary>
        /// 处理加载群列表
        /// </summary> 
        private void DealLoadDatas(GetGroupListPackage package)
        {
            if (package == null || package.code != 0 || package.data == null || package.data.items == null)
            {
                return;
            }
            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                var owners = UpdateGroupInfo(package.data.items.ownerGroup, GroupType.Owner, "我创建的群");
                var admins = UpdateGroupInfo(package.data.items.adminGroup, GroupType.Admin, "我管理的群");
                var joins = UpdateGroupInfo(package.data.items.joinGroup, GroupType.Join, "我加入的群");

                List<GroupViewModel> datas = new List<GroupViewModel>();
                datas.AddRange(owners);
                datas.AddRange(admins);
                datas.AddRange(joins);

                this.Items = new ObservableCollection<GroupViewModel>(datas);
                ResetGroupBy();

                AppData.MainMV.ChatListVM.ResetSort();
                AppData.MainMV.ChatListVM.IsCloseTrayWindow();
                AppData.MainMV.UpdateUnReadMsgCount();
                AppData.MainMV.ChatListVM.TotalUnReadCount = AppData.MainMV.TotalUnReadCount;
            }));
        }

        private List<GroupViewModel> UpdateGroupInfo(List<SDKClient.Model.group> items, GroupType groupType, string groupTypeName)
        {
            List<GroupViewModel> datas = new List<GroupViewModel>(items.Count);

            foreach (var item in items)
            {
                GroupModel group = AppData.Current.GetGroupModel(item.groupId);
                group.GroupType = groupType;
                group.GroupRemark = item.groupIntroduction;
                group.DisplayName = item.groupName;
                group.MenbersCount = item.groupNumMember;
                //App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                //{
                var isHeadImgUpdate = true;
                if (!group.HeadImg.Equals(IMAssets.ImageDeal.DefaultGroupHeadImage))
                {
                    if (File.Exists(group.HeadImg))
                    {
                        var info = new FileInfo(group.HeadImg);
                        if (info.Name == item.groupPhoto)
                        {
                            isHeadImgUpdate = false;
                        }
                    }
                }

                if (isHeadImgUpdate)
                    group.HeadImg = IMClient.Helper.ImageHelper.GetFriendFace(item.groupPhoto, (s) => group.HeadImg = s);
                //}));
                group.HeadImgMD5 = item.groupPhoto;
                group.OwnerID = item.groupOwnerId;
                group.JoinAuthType = item.joinAuthType;

                group.IsNotDisturb = item.doNotDisturb;

                group.TopMostTime = item.groupTopTime.HasValue ? item.groupTopTime.Value : DateTime.MinValue;
                group.IsTopMost = (group.TopMostTime == DateTime.MinValue) ? false : true;
                datas.Add(new GroupViewModel(group, groupTypeName));
            }

            //App.Current.Dispatcher.Invoke(new Action(() =>
            //{
            //    AppData.MainMV.ChatListVM.ResetSort();
            //    AppData.MainMV.ChatListVM.IsCloseTrayWindow();
            //}));
            return datas;
        }

        [Obsolete]
        private void GetOtherGroupInfo()
        {
            var chats = AppData.MainMV.ChatListVM.Items.ToList();
            foreach (var item in chats)
            {
                if (item.IsGroup)
                {
                    var groupVM = this.Items.FirstOrDefault(info => info.ID == item.ID);
                    if (item.IsGroup && groupVM == null)
                    {
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            try
                            {
                                AppData.MainMV.ChatListVM.Items.Remove(item);
                            }
                            catch { }
                        }));
                        ////if (groupVM.IsCreator)
                        ////{
                        ////    AppData.MainMV.ChatListVM.Items.Remove(item);
                        ////}
                        ////else
                        //{ 
                        //    SDKClient.SDKClient.Instance.GetGroup(item.ID);
                        //}
                    }
                }
            }
        }

        /// <summary>
        /// 重置排序
        /// </summary>
        private void ResetGroupBy()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                ICollectionView cv = CollectionViewSource.GetDefaultView(this.Items);
                if (cv == null)
                {
                    return;
                }
                cv.SortDescriptions.Clear();
                cv.SortDescriptions.Add(new SortDescription("GroupTypeName", ListSortDirection.Ascending));
                cv.SortDescriptions.Add(new SortDescription("ID", ListSortDirection.Descending));

                cv.GroupDescriptions.Clear();
                cv.GroupDescriptions.Add(new PropertyGroupDescription("GroupTypeName"));
            });
        }

        #endregion
        /// <summary>
        /// 添加设置成员权限的通知
        /// </summary>
        /// <param name="package"></param>
        private void AddMemberPowerMessage(SetMemberPowerPackage package, bool isForward = false, bool isNewMessage = true)
        {
            GroupViewModel groupVM;

            groupVM = this.Items.ToList().FirstOrDefault(i => i.ID == package.data.groupId);
            if (groupVM == null)
                return;
            groupVM.GetGroupMemberList(true);

            GroupModel group = AppData.Current.GetGroupModel(package.data.groupId);

            var chat = AppData.MainMV.ChatListVM.GetChat(group.ID, true);
            var has = (chat.Model as ChatModel).Messages.FirstOrDefault(m => m.MsgKey == package.id);
            if (has != null) //重复消息
            {
                return;
            }
            if (!chat.IsShowGroupNoticeBtn)
                chat.IsShowGroupNoticeBtn = true;
            string info = string.Empty;
            int index = 0;
            foreach (var item in package.data.userIds)
            {
                UserModel user = AppData.Current.GetUserModel(item);
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
                    sender.IsManager = true;
                    if (item == AppData.Current.LoginUser.ID)
                    {
                        groupVM.IsAdmin = true;
                        groupVM.GroupTypeName = "我管理的群";
                        group.GroupType = GroupType.Admin;
                        groupVM.IsCanOpera = true;
                        groupVM.IsCreator = true;
                    }
                }
                else
                {
                    if (item == AppData.MainMV.LoginUser.ID)
                    {
                        info = "你被取消群管理员";
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            groupVM.ApplyUsers.Clear();
                            groupVM.IsJoinGroupApply = false;
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
                    sender.IsManager = false;
                    if (item == AppData.Current.LoginUser.ID)
                    {
                        groupVM.IsAdmin = false;
                        groupVM.GroupTypeName = "我加入的群";
                        group.GroupType = GroupType.Join;
                        groupVM.IsCanOpera = false;
                        groupVM.IsCreator = false;
                    }
                }
                if (groupVM.HasMoreMembers)
                    groupVM.ShowMoreMembers = false;
                index++;
                chat.AddMessageTip(info, package.time, isForward, package.id);
                //groupVM.InitializeData();
                //chat.IsShowGroupNoticeBtn = true;
            }

            this.ResetGroupBy();
        }

        /// <summary>
        /// 处理-创建结果
        /// </summary>
        /// <param name="pg"></param>
        private void DealCreate(CreateGroupComponsePackage pg)
        {
            if (pg == null || pg.code != 0 || pg.data == null)
            {
                AppData.MainMV.TipMessage = "创建群失败！";
            }
            else
            {
                int fromID = int.Parse(pg.from);

                GroupViewModel groupVM = this.Items.FirstOrDefault(info => info.ID == pg.data.groupId);
                if (groupVM == null) //该群未存在
                {
                    GroupModel group = AppData.Current.GetGroupModel(pg.data.groupId);
                    SDKClient.SDKClient.Instance.GetGroup(pg.data.groupId);
                    group.OwnerID = fromID;
                    string groupChar = fromID == AppData.Current.LoginUser.ID ? "我创建的群" : "我加入的群";

                    groupVM = new GroupViewModel(group, groupChar);
                    //var chatVModel = AppData.MainMV.ChatListVM.Items.FirstOrDefault(info => info.ID == group.ID && info.IsGroup == true);
                    //if (chatVModel != null)
                    //    chatVModel.IsShowGroupNoticeBtn = true;

                    App.Current.Dispatcher.Invoke(new Action(async () =>
                    {
                        this.Items.Add(groupVM);
                        this.ResetGroupBy();
                        this.SelectedItem = groupVM;

                        if (fromID == AppData.Current.LoginUser.ID) //我创建的群
                        {
                            //var dismissGroup = AppData.DismissGroupIDItems.FirstOrDefault(m => m.data.groupId == pg.data.groupId);
                            var dismissGroup = AppData.DismissGroupIDItems.FirstOrDefault((m) =>
                             {
                                 if (m is DismissGroupPackage dis && dis.data.groupId == pg.data.groupId && dis.data.ownerId == AppData.Current.LoginUser.ID)
                                 {
                                     return true;
                                 }
                                 else if (m is ExitGroupPackage exit && exit.data.groupId == pg.data.groupId && exit.data.userIds.Contains(AppData.Current.LoginUser.ID))
                                 {
                                     return true;
                                 }
                                 return false;
                             });
                            if (dismissGroup != null)
                            {
                                return;
                            }

                            if (pg.syncMsg == null || pg.syncMsg == 0)
                                groupVM.JupmToChatCommand?.Execute(groupVM);
                            else
                            {
                                StringBuilder sb = new StringBuilder();
                                foreach (var item in pg.data.items)
                                {
                                    sb.Append(item.userName);
                                    sb.Append("、");
                                }
                                sb.Remove(sb.Length - 1, 1);
                                var tipMsg = $"[{sb.ToString()}] 进入群聊";
                                AppData.MainMV.JumpToChatModel(group, true, tipMsg);
                            }
                            if (AppData.CreateGroupWaits.ContainsKey(pg.id))
                            {
                                var fails = AppData.CreateGroupWaits[pg.id];

                                string from = AppData.Current.LoginUser.ID.ToString();
                                string to = string.Empty;

                                InviteJoinGroupPackage.Data data = new InviteJoinGroupPackage.Data();
                                data.groupId = group.ID;
                                data.groupIntroduction = group.GroupRemark;
                                data.groupName = group.DisplayName;
                                data.groupPhoto = group.HeadImg;
                                data.InviteUserId = AppData.Current.LoginUser.User.ID;
                                data.inviteUserName = AppData.Current.LoginUser.User.Name;
                                data.inviteUserPhoto = AppData.Current.LoginUser.User.HeadImg;
                                data.userIds = new List<int>();

                                foreach (int id in fails)
                                {
                                    var target = AppData.MainMV.ChatListVM.GetChat(id);
                                    data.InviteUserId = id;
                                    to = id.ToString();
                                    InviteJoinGroupPackage package = new InviteJoinGroupPackage()
                                    {
                                        id = SDKClient.SDKProperty.RNGId,
                                        from = from,
                                        to = to,
                                        time = DateTime.Now,
                                        data = data,
                                    };

                                    await SDKClient.SDKClient.Instance.AppendLocalData_InviteJoinGroupPackage(package);

                                    ChatViewModel chatVM = AppData.MainMV.ChatListVM.GetChat(id);
                                    MessageModel msg = new MessageModel()
                                    {
                                        MsgKey = package.id,
                                        Sender = AppData.Current.LoginUser.User,
                                        SendTime = package.time.Value,
                                        IsMine = true,
                                        MsgType = MessageType.invitejoingroup,
                                        Target = group,
                                        MessageState = MessageStates.Fail,
                                    };
                                    if (target.Chat.Chat is UserModel user && user.LinkType >= 2)
                                    {
                                        await SDKClient.SDKClient.Instance.AppendLocalData_NotifyMessage(from, to, "对方拒绝接受你的消息！", id,
                                        SDKClient.SDKProperty.MessageType.notification);

                                        chatVM.Chat.Messages.Add(msg);
                                        chatVM.AddMessageTipEx("对方拒绝接受你的消息！");
                                    }
                                    else
                                    {
                                        await SDKClient.SDKClient.Instance.AppendLocalData_NotifyMessage(from, to, "您不在对方通讯录内", id,
                                        SDKClient.SDKProperty.MessageType.notification);

                                        chatVM.Chat.Messages.Add(msg);
                                        string actionableContent = null;
                                        actionableContent = "有事找TA";
                                        chatVM.AddMessageTip("您不在对方通讯录内，", actionableContent: actionableContent);
                                    }
                                }
                            }
                        }
                        else
                        {

                        }

                    }));

                    //获取群成员
                    groupVM.GetGroupMemberList(true);
                }
            }
        }

        /// <summary>
        /// 处理-邀请结果
        /// </summary> 
        public void DealInvite(InviteJoinGroupPackage package)
        {
            if (package == null || package.data == null || package.code != 0)
            {
                if (package.code == 614)
                    AppData.MainMV.TipMessage = "抱歉，该群已经被解散！";
                else if (package.code == 619)
                    AppData.MainMV.TipMessage = package.error;
                else
                    AppData.MainMV.TipMessage = "入群邀请发送失败！";
                return;
            }

            int from = -1;

            if (!int.TryParse(package.from, out from) || from < 0 || AppData.Current.LoginUser.ID == from)
            {
                return;
            }
            var isGroup = false;
            var msgId = package.id;
            if (package.data.targetGroupId == 0)
            {
                msgId = package.id + package.to + "single";
            }
            else
            {
                isGroup = true;
                msgId = package.id + package.data.targetGroupId + "group";
            }
            int roomId = 0;
            int.TryParse(package.from, out roomId);

            if (!isGroup)
            {
                //邀请人是否我的好友
                var tempFriendList = AppData.MainMV.FriendListVM.Items.ToList();
                FriendViewModel friendVM = tempFriendList.FirstOrDefault(info => info.ID == roomId);
                if (friendVM == null)
                {
                    return;
                }
            }

            //var msgId = package.id;
            //var isGroup = false;
            //if (package.data.targetGroupId == 0)
            //{
            //    msgId = package.id + package.data.InviteUserId + "single";
            //}
            //else
            //{
            //    isGroup = true;
            //    msgId = package.id + package.data.targetGroupId + "group";
            //}
            //邀请人和我有聊天记录 
            var chatVM = AppData.MainMV.ChatListVM.GetChat(isGroup ? package.data.targetGroupId : roomId, isGroup);

            //if (chatVM == null) //未有聊天条目
            //{
            //    ChatModel chat = AppData.Current.GetChatViewModel(friendVM.Model as UserModel);
            //    chatVM = new ChatViewModel(chat);

            //    App.Current.Dispatcher.Invoke(new Action(() =>
            //    {
            //        var tempItems = AppData.MainMV.ChatListVM.Items.ToList();
            //        var tempChatVM = tempItems.FirstOrDefault(info => info.ID == package.data.InviteUserId && !info.IsGroup);
            //        if (tempChatVM == null)
            //            AppData.MainMV.ChatListVM.Items.Add(chatVM);
            //    }));
            //}


            SDKClient.DB.messageDB msgDB = new SDKClient.DB.messageDB()
            {
                from = package.from,
                to = package.to,
                msgTime = package.time.Value,
                msgId = msgId,

                optionRecord = 1,
                roomId = isGroup ? package.data.groupId : package.data.InviteUserId,
                Source = Util.Helpers.Json.ToJson(package),
                roomType = 0
            };
            msgDB.content = "群名片";
            msgDB.msgType = package.api;

            chatVM.LoadHisMessage(msgDB, from);

            //闪烁托盘和任务栏图标
            AppData.MainMV.ChatListVM.FlashIcon(chatVM);
        }


        /// <summary>
        /// 处理-入群申请被批准
        /// </summary>
        /// <param name="package"></param>
        private async void DealJoinAccepted(JoinGroupAcceptedPackage package)
        {
            if (package == null || package.code != 0 || package.data == null)
            {
                return;
            }

            switch (package.data.auditStatus)
            {
                case 1://同意加群申请
                    //var groupVM = this.Items.FirstOrDefault(info => info.ID == package.data.groupId);
                    GroupViewModel groupVM;
                    if (AppData.Current.LoginUser.ID == package.data.memberId)
                    {
                        //获取群信息
                        SDKClient.SDKClient.Instance.GetGroup(package.data.groupId);
                        //被邀请人收到消息后，创建一个新群
                        var group = AppData.Current.GetGroupModel(package.data.groupId);
                        group.GroupType = GroupType.Join;
                        groupVM = new GroupViewModel(group, "我加入的群");

                        var chat = AppData.Current.GetChatViewModel(group);

                        var chatVM = AppData.MainMV.ChatListVM.GetChat(group.ID, true);

                        if (package.syncMsg == 1)
                        {
                            App.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                if (chatVM != null)
                                {
                                    chatVM.IsHideAppendButton = false;
                                    chatVM.IsShowGroupNoticeBtn = true;
                                    string tip = "你进入群聊";

                                    chatVM.AddMessageTip(tip, package.time, false, package.id);
                                }
                                var tempGroupVM = this.Items.ToList().FirstOrDefault(info => info.ID == package.data.groupId);
                                if (tempGroupVM == null)
                                    this.Items.Add(groupVM);
                                AppData.MainMV.ChatListVM.ResetSort();
                                ResetGroupBy();
                                groupVM.GetGroupMemberList(true);
                            }));
                        }
                        else
                        {
                            App.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                if (chatVM != null)
                                {
                                    //chatVM = new ChatViewModel(chat);
                                    //AppData.MainMV.ChatListVM.Items.Add(chatVM);
                                    AppData.MainMV.ChatListVM.ResetSort();
                                    if (package.syncMsg == 1)
                                        AppData.MainMV.ChatListVM.SelectedItem = chatVM;
                                }

                                chatVM.IsHideAppendButton = false;
                                chatVM.IsShowGroupNoticeBtn = true;
                                var tempGroupVM = this.Items.ToList().FirstOrDefault(info => info.ID == package.data.groupId);

                                if (tempGroupVM == null)
                                    this.Items.Add(groupVM);
                                ResetGroupBy();

                                if (chatVM != null)
                                {
                                    string tip = "你进入群聊";
                                    chatVM.AddMessageTip(tip, package.time, false, package.id);
                                }
                                groupVM.GetGroupMemberList(true);
                            }));
                            if (AppData.MainMV.ChatListVM.SelectedItem == chatVM)
                            {
                                if (chatVM.IsGroup)
                                {
                                    GroupViewModel gvModel = chatVM.TargetVM as GroupViewModel;
                                    var entityData = await SDKClient.SDKClient.Instance.GetJoinGroupNotice(groupVM.Model.ID);
                                    if (entityData != null)
                                    {
                                        MessageModel msg = new MessageModel()
                                        {
                                            Sender = AppData.Current.LoginUser.User,
                                            SendTime = entityData.publishTime ?? DateTime.Now,
                                            MsgType = MessageType.addgroupnotice,
                                            IsMine = true,
                                            Content = entityData.content,
                                        };
                                        GroupNoticeModel gnModel = new GroupNoticeModel()
                                        {
                                            NoticeTitle = entityData.title,
                                            GroupMId = (groupVM.Model as GroupModel).ID
                                        };
                                        msg.NoticeModel = gnModel;
                                        msg.TipMessage = string.Format("{0}：{1}", msg.Sender.DisplayName, msg.NoticeModel.NoticeTitle);
                                        App.Current.Dispatcher.Invoke(new Action(() =>
                                        {
                                            if (AppData.tipWindow != null)
                                                AppData.tipWindow.Activate();
                                            else
                                                AppData.tipWindow = new Views.ChildWindows.GroupNoticeTipWindow(msg);
                                        }));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //其他群成员收到消息后，在群里添加一个新成员                        
                        groupVM = this.Items.ToList().FirstOrDefault(info => info.ID == package.data.groupId);
                        if (groupVM == null)
                            return;
                        groupVM.GetGroupMemberList(true);

                        //查找是已有聊天框
                        var chat = AppData.MainMV.ChatListVM.GetChat(package.data.groupId, true);

                        if (chat != null)
                        {
                            string tip = string.Format("[{0}] 进入群聊", package.data.userName);
                            chat.AddMessageTip(tip, package.time, false, package.id);
                        }

                        //若我是群主或者管理员
                        if (groupVM.IsAdmin || groupVM.IsCreator)
                        {
                            var target = groupVM.ApplyUsers.FirstOrDefault(info => info.ID == package.data.memberId);
                            if (target != null)
                            {
                                await SDKClient.SDKClient.Instance.DeleteJoinGroupRecord(package.data.groupId, package.data.memberId);
                                App.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    groupVM.ApplyUsers.Remove(target);
                                    if (groupVM.ApplyUsers.Count == 0)
                                        groupVM.IsJoinGroupApply = false;
                                }));
                            }
                        }
                        if (groupVM.HasMoreMembers)
                            groupVM.ShowMoreMembers = false;
                    }
                    break;
                case 2://拒绝加群申请
                    break;
                case 3://忽略加群申请
                    break;
                default:
                    break;
            }
        }
        /// <summary> 
        /// 处理- 入群申请
        /// </summary>
        /// <param name="package"></param>
        private async void DealJoinApply(JoinGroupPackage package)
        {
            if (package == null || package.code != 0 || package.data == null)
            {
                switch (package.code)
                {
                    case 614:
                        AppData.MainMV.TipMessage = "抱歉，该群已经被解散！";
                        break;
                    case 619:
                        AppData.MainMV.TipMessage = package.error;
                        break;
                    default:
                        AppData.MainMV.TipMessage = "发送入群申请失败！";
                        break;
                }
                return;
            }
            int from = int.Parse(package.from);

            var groupVM = AppData.MainMV.GroupListVM.Items.FirstOrDefault(x => x.ID == package.data.groupId);
            GroupModel group;

            string hint = string.Format("[{0}] 申请进入群聊", package.data.userName);

            if (package.data.userId == AppData.Current.LoginUser.User.ID) //我是申请人，收到批复回包
            {
                if (package.data.isAccepted) //被同意
                {
                    if (groupVM == null)
                    {
                        group = AppData.Current.GetGroupModel(package.data.groupId);
                        group.GroupType = GroupType.Join;
                        groupVM = new GroupViewModel(group, "我加入的群");
                        SDKClient.SDKClient.Instance.GetGroup(package.data.groupId);

                        var chatVM = AppData.MainMV.ChatListVM.GetChat(package.data.groupId, true);

                        //if (chatVM == null) //有过群聊，被踢出，又进入
                        //{
                        //    ChatModel chat = AppData.Current.GetChatViewModel(group);
                        //    chatVM = new ChatViewModel(chat);

                        //    App.Current.Dispatcher.Invoke(new Action(() =>
                        //    {
                        //        var tempItems = AppData.MainMV.ChatListVM.Items.ToList();
                        //        var tempChatVM = tempItems.FirstOrDefault(info => info.ID == package.data.groupId);
                        //        if (tempChatVM == null)
                        //            AppData.MainMV.ChatListVM.Items.Add(chatVM);
                        //    }));
                        //}

                        chatVM.IsHideAppendButton = false;
                        chatVM.AddMessageTip("你进入群聊", package.time, false, package.id);
                        chatVM.IsShowGroupNoticeBtn = true;
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            this.Items.Add(groupVM);
                            this.ResetGroupBy();
                            AppData.MainMV.ChatListVM.ResetSort();
                            if (package.syncMsg == null || package.syncMsg == 0)
                                AppData.MainMV.ChatListVM.SelectedItem = chatVM;

                        }));
                        if (AppData.MainMV.ChatListVM.SelectedItem == chatVM)
                        {
                            if (chatVM.IsGroup)
                            {
                                GroupViewModel gvModel = chatVM.TargetVM as GroupViewModel;
                                var entityData = await SDKClient.SDKClient.Instance.GetJoinGroupNotice(chatVM.ID);
                                if (entityData != null)
                                {
                                    MessageModel msg = new MessageModel()
                                    {
                                        Sender = AppData.Current.LoginUser.User,
                                        SendTime = entityData.publishTime ?? DateTime.Now,
                                        MsgType = MessageType.addgroupnotice,
                                        IsMine = true,
                                        Content = entityData.content,
                                    };
                                    GroupNoticeModel gnModel = new GroupNoticeModel()
                                    {
                                        NoticeTitle = entityData.title,
                                        GroupMId = (groupVM.Model as GroupModel).ID
                                    };
                                    msg.NoticeModel = gnModel;
                                    msg.TipMessage = string.Format("{0}：{1}", msg.Sender.DisplayName, msg.NoticeModel.NoticeTitle);
                                    App.Current.Dispatcher.Invoke(new Action(() =>
                                    {
                                        if (AppData.tipWindow != null)
                                        {
                                            AppData.tipWindow.Close();
                                            Views.ChildWindows.GroupNoticeTipWindow win = new Views.ChildWindows.GroupNoticeTipWindow(msg, true);
                                            win.Owner = App.Current.MainWindow;
                                            win.ShowDialog();
                                        }
                                        else
                                            AppData.tipWindow = new Views.ChildWindows.GroupNoticeTipWindow(msg);
                                    }));
                                }
                            }
                        }
                    }
                    else
                    {
                        var chatVM = AppData.MainMV.ChatListVM.GetChat(package.data.groupId, true);
                        chatVM.IsHideAppendButton = false;
                        chatVM.AddMessageTip("你进入群聊", package.time, false, package.id);
                        chatVM.IsShowGroupNoticeBtn = true;
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            AppData.MainMV.ChatListVM.ResetSort();
                        }));
                    }
                }
                else //被拒绝 
                {

                    //AppData.MainMV.TipMessage = string.Format("申请加入群聊[{0}]被拒绝！", package.data.groupId); 
                }
            }
            else if (groupVM != null)
            {
                if (groupVM.IsCreator || groupVM.IsAdmin)
                {
                    //我是群主或管理员
                    UserModel user = AppData.Current.GetUserModel(package.data.userId);
                    group = groupVM.Model as GroupModel;
                    if (group.Members != null && group.Members.Any(info => info.ID == package.data.userId)) //若成员已经在群里面
                    {
                        await SDKClient.SDKClient.Instance.DeleteJoinGroupRecord(package.data.groupId, package.data.userId);
                        return;
                    }

                    UserApplyModel apply = groupVM.ApplyUsers.FirstOrDefault(info => info.ID == package.data.userId);
                    if (apply != null) //已有申请
                    {
                        apply.ApplyTime = package.time == null ? DateTime.Now : package.time.Value;
                        apply.InviteUserId = package.data.InviteUserId;
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            int index = groupVM.ApplyUsers.IndexOf(apply);
                            groupVM.ApplyUsers.Move(index, 0);
                            groupVM.IsJoinGroupApply = true;
                        });
                    }
                    else
                    {
                        apply = new UserApplyModel(user);

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            var tempApplyUser = groupVM.ApplyUsers.ToList().FirstOrDefault(m => m.ID == apply.ID);
                            if (tempApplyUser == null)
                            {
                                groupVM.ApplyUsers.Insert(0, apply);
                                groupVM.IsJoinGroupApply = true;
                            }
                        });
                    }


                    apply.ApplyTime = package.time == null ? DateTime.Now : package.time.Value;
                    apply.InviteUserId = package.data.InviteUserId;

                    if (package.data.isAccepted)
                    {
                        groupVM.GetGroupMemberList(true);
                        hint = string.Format("[{0}] 进入群聊", package.data.userName);
                        await SDKClient.SDKClient.Instance.DeleteJoinGroupRecord(package.data.groupId, package.data.userId);
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            if (apply != null)
                            {
                                groupVM.ApplyUsers.Remove(apply);
                                if (groupVM.ApplyUsers.Count == 0)
                                    groupVM.IsJoinGroupApply = false;
                            }
                        });
                    }

                    if (user.LinkDelType == 1 || user.LinkDelType == 3) //若不在我的好友列表，则获取用户信息
                    {
                        SDKClient.SDKClient.Instance.GetUser(user.ID);
                    }

                    ChatViewModel chatViewModel = AppData.MainMV.ChatListVM.GetChat(package.data.groupId, true);
                    int fromID = 0;
                    int.TryParse(package.from, out fromID);
                    UserModel userModel = null;
                    if (fromID != AppData.Current.LoginUser.ID)
                    {
                        userModel = AppData.Current.GetUserModel(fromID);
                    };
                    chatViewModel.AddMessageTip(hint, package.time, false, package.id, "", userModel);


                    if (AppData.MainMV.ChatListVM.SelectedItem != null && AppData.MainMV.ChatListVM.SelectedItem.ID == chatViewModel.ID)
                    {
                        if (AppData.MainMV.ListViewModel == AppData.MainMV.ChatListVM)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                if (chatViewModel.GetViewIsFocus())
                                {
                                    chatViewModel.UnReadCount++;
                                    AppData.MainMV.ChatListVM.FlashIcon(chatViewModel);
                                }
                            });
                        }
                        else
                        {
                            chatViewModel.UnReadCount++;
                            AppData.MainMV.ChatListVM.FlashIcon(chatViewModel);
                        }
                    }
                    else
                    {
                        chatViewModel.UnReadCount++;
                        AppData.MainMV.ChatListVM.FlashIcon(chatViewModel);
                    }
                    AppData.MainMV.UpdateUnReadMsgCount();
                    AppData.MainMV.ChatListVM.TotalUnReadCount = AppData.MainMV.TotalUnReadCount;

                    if ((chatViewModel.Model as ChatModel).Chat is GroupModel groupModel && groupModel.IsTopMost)
                    {
                        return;
                    }

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        AppData.MainMV.ChatListVM.ResetSort();
                    });
                }
            }
        }

        private void DealUpdateInfo(UpdateGroupPackage package)
        {
            if (package.code != 0)
            {
                //AppData.MainMV.TipMessage = package.error;
                return;
            }
            GroupViewModel groupVM = AppData.MainMV.GroupListVM.Items.ToList().FirstOrDefault(info => info.ID == package.data.groupId);

            if (groupVM == null)
            {
                return;
            }

            GroupModel group = groupVM.Model as GroupModel;
            string msgTip = string.Empty;

            switch (package.data.setType)
            {
                case (int)SetGroupOption.修改群名称:
                    group.DisplayName = package.data.content;
                    msgTip = $"群名称修改为：[{package.data.content}]";
                    break;
                case (int)SetGroupOption.修改群头像:
                    group.HeadImgMD5 = package.data.content;
                    group.HeadImg = Helper.ImageHelper.GetFriendFace(package.data.content, (s) =>
                    {
                        group.HeadImg = s;
                    });

                    msgTip = "[群头像已修改]";
                    return;//不需要显示 该条信息
                case (int)SetGroupOption.修改群简介:
                    group.GroupRemark = package.data.content;
                    msgTip = $"修改群简介：[{package.data.content}]";
                    break;
                case (int)SetGroupOption.设置入群验证方式://1- 管理员审批入群 2- 自由入群 3- 密码入群(数字+逗号+密码)
                    group.JoinAuthType = package.data.content.ToInt();
                    switch (package.data.content)
                    {
                        default:
                        case "1":
                            msgTip = "入群方式修改为: [管理员审批入群]";
                            groupVM.IsJoinGroupNeedCheck = true;
                            break;
                        case "2":
                            msgTip = "入群方式修改为: [自由入群]";
                            groupVM.IsJoinGroupNeedCheck = false;
                            break;
                        case "3":
                            msgTip = "入群方式修改为: [密码入群]";
                            break;
                    }
                    break;
                default:
                    break;
            }

            ChatViewModel chatVM = AppData.MainMV.ChatListVM.GetChat(package.data.groupId, true);
            if (chatVM != null && !string.IsNullOrEmpty(msgTip))
            {
                chatVM.AddMessageTip(msgTip);
                if (chatVM.IsGroup)
                    chatVM.IsShowGroupNoticeBtn = true;
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                AppData.MainMV.ChatListVM.ResetSort();
            });
        }

        private void DealExit(ExitGroupPackage package, bool isForward = false)
        {
            if (package == null || package.code != 0 || package.data == null)
            {
                return;
            }
            if (package.data.userIds.Contains(AppData.Current.LoginUser.ID) && package.data.adminId == 0)
            {
                if (!AppData.DismissGroupIDItems.Exists(m => m.id == package.id))
                    AppData.DismissGroupIDItems.Add(package);
            }
            GroupViewModel groupVM = this.Items.FirstOrDefault(x => x.ID == package.data.groupId);
            if (groupVM == null)
            {
                return;
            }

            GroupModel group = groupVM.Model as GroupModel;
            if (!groupVM.IsCreator && !groupVM.IsAdmin && package.data.userIds[0] != AppData.Current.LoginUser.ID)
                return;
            var chatVM = AppData.MainMV.ChatListVM.GetChat(group.ID, true);
            if (chatVM == null)
            {

                return;
            }

            string prefix = string.Empty;
            string suffix = package.data.adminId == 0 ? "退出群聊" : "被移出群聊";
            string tip = string.Empty;

            if (package.data.userIds.Count == 1 && package.data.userIds[0] == AppData.Current.LoginUser.ID)
            {
                //只有一个人被T或者退群并且是我自己时，提示消息前面不用显示昵称
                prefix = package.data.adminId == 0 ? "我" : "你";
                tip = string.Format("{0}{1}", prefix, suffix);

                App.Current.Dispatcher.Invoke(() =>
                {
                    if (package.data.adminId == 0)
                    {
                        if (!AppData.DismissGroupIDItems.Exists(m => m.id == package.id))
                            AppData.DismissGroupIDItems.Add(package);
                        //删除对应的消息列表项
                        AppData.MainMV.ChatListVM.DeleteChatItem(chatVM.ID, true);
                        //AppData.MainMV.UpdateUnReadMsgCount();
                        //AppData.MainMV.ChatListVM.TotalUnReadCount = AppData.MainMV.TotalUnReadCount;
                    }
                    else
                    {
                        chatVM.AddMessageTip(tip, package.time, isForward, package.id);
                        groupVM.GetGroupMemberList(true);
                    }
                    chatVM.IsHideAppendButton = true;
                    chatVM.IsShowGroupNoticeBtn = false;
                    chatVM.HasNewGroupNotice = false;
                    groupVM.ApplyUsers.Clear();
                    groupVM.IsJoinGroupApply = false;
                    AppData.Current.LoginUser.User.RemoveFromGroup(group);
                    //删除群组列表中的项
                    this.Items.Remove(groupVM);
                });

                App.CancelFileOperate(chatVM.ID);
            }
            else
            {
                foreach (var userID in package.data.userIds)
                {
                    var tempUser = AppData.Current.GetUserModel(userID);
                    tempUser.RemoveFromGroup(group);

                    if (userID == AppData.Current.LoginUser.ID)
                    {
                        prefix = package.data.adminId == 0 ? "我" : "你";
                        tip = string.Format("{0}{1}", prefix, suffix);

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            if (package.data.adminId == 0)
                            {
                                //删除对应的消息列表项
                                AppData.MainMV.ChatListVM.DeleteChatItem(chatVM.ID, true);
                                //AppData.MainMV.UpdateUnReadMsgCount();
                                //AppData.MainMV.ChatListVM.TotalUnReadCount = AppData.MainMV.TotalUnReadCount;
                            }
                            chatVM.IsHideAppendButton = true;
                            chatVM.IsShowGroupNoticeBtn = false;
                            chatVM.HasNewGroupNotice = false;
                            groupVM.IsJoinGroupApply = false;
                            groupVM.ApplyUsers.Clear();
                            //删除群组列表中的项
                            this.Items.Remove(groupVM);
                        });
                        App.CancelFileOperate(chatVM.ID);
                        chatVM.AddMessageTip(tip, package.time, isForward, package.id);
                        groupVM.GetGroupMemberList(true);

                        return;
                    }
                }

                foreach (var item in package.data.userNames)
                {
                    prefix += item + "、";
                }
                prefix = prefix.TrimEnd('、');

                tip = string.Format("[{0}] {1}", prefix, suffix);

                if (groupVM.IsCreator || groupVM.IsAdmin)
                {
                    chatVM.AddMessageTip(tip, package.time, isForward, package.id);
                }
                groupVM.GetGroupMemberList(true);
                if (groupVM.HasMoreMembers)
                    groupVM.ShowMoreMembers = false;
            }
        }

        private void DealDismiss(DismissGroupPackage package, bool isForward = false)
        {
            GroupViewModel model = this.Items.FirstOrDefault(x => x.ID == package.data.groupId);
            //if (model == null)
            //{
            //    return;
            //}

            GroupModel group;
            if (model != null)
                group = model.Model as GroupModel;
            if (!AppData.DismissGroupIDItems.Exists(m => m.id == package.id))
                AppData.DismissGroupIDItems.Add(package);
            App.Current.Dispatcher.Invoke(() =>
            {
                if (model != null)
                    this.Items.Remove(model);

                if (package.data.ownerId == AppData.MainMV.LoginUser.ID)
                {
                    //群主收到解散群的包时， 删除对应的消息列表项
                    var chatVM = AppData.MainMV.ChatListVM.Items.FirstOrDefault(info => info.ID == package.data.groupId && info.IsGroup == true);
                    if (chatVM != null)
                    {
                        chatVM.IsShowGroupNoticeBtn = false;
                        chatVM.HasNewGroupNotice = false;
                        AppData.MainMV.ChatListVM.DeleteChatItem(chatVM.ID, true);
                        App.CancelFileOperate(chatVM.ID);
                    }
                    //else
                    //{

                    //}

                    IListViewModel listVM = AppData.MainMV.ListViewModel;
                    AppData.MainMV.ListViewModel = listVM;
                }
                else
                {
                    //群成员收到解散群的包时 

                    var chatVM = AppData.MainMV.ChatListVM.GetChat(package.data.groupId, true);
                    if (chatVM == null)
                        return;
                    chatVM.AddMessageTip("该群已经被解散！", package.time, isForward, package.id);
                    chatVM.IsHideAppendButton = true;
                    chatVM.IsShowGroupNoticeBtn = false;
                    chatVM.HasNewGroupNotice = false;
                    App.CancelFileOperate(chatVM.ID);
                }
            });
        }

        /// <summary>
        /// 更新群信息
        /// </summary>
        /// <param name="pg"></param>
        private void DealUpdateMemberInfo(UpdateUserSetsInGroupPackage pg)
        {
            GroupModel group = AppData.Current.GetGroupModel(pg.data.groupId);

            if (pg.code != 0)
            {
                //设置出错，回滚
                switch (pg.data.setType)
                {
                    case 1://我的群昵称

                        break;
                    case 2://置顶   
                        if (pg.data.userId == AppData.MainMV.LoginUser.ID)
                        {
                            group.TopMostTime = pg.data.content.Equals("1") ? DateTime.MinValue : DateTime.Now;
                            if (pg.data.content.Equals("1"))
                            {
                                group.IsTopMost = true;
                            }
                            else
                            {
                                group.IsTopMost = false;
                            }
                        }
                        break;
                    case 3://消息免打扰
                        if (pg.data.userId == AppData.MainMV.LoginUser.ID)
                            group.IsNotDisturb = pg.data.content.Equals("1") ? false : true;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (pg.data.setType)
                {
                    case 1://群昵称设置
                        UserModel user = AppData.Current.GetUserModel(pg.data.userId);
                        GroupMember member = user.GetInGroupMember(group, false);
                        if (member != null)
                        {
                            member.NickNameInGroup = pg.data.content;
                        }
                        if (user.ID == AppData.MainMV.LoginUser.ID)
                        {
                            group.MyNickNameInGroup = pg.data.content;
                        }
                        break;
                    case 2://置顶
                        group.TopMostTime = pg.data.content.Equals("1") ? DateTime.Now : DateTime.MinValue;
                        if (pg.data.content.Equals("1"))
                        {
                            group.IsTopMost = true;
                        }
                        else
                        {
                            group.IsTopMost = false;
                        }
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            AppData.MainMV.ChatListVM.ResetSort();
                        }));
                        break;
                    case 3://消息免打扰
                        if (pg.data.userId == AppData.MainMV.LoginUser.ID)
                        {
                            if (pg.data.content.Equals("1"))
                            {
                                group.IsNotDisturb = true;
                            }
                            else
                            {
                                group.IsNotDisturb = false;
                            }

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                AppData.MainMV.ChatListVM.IsCloseTrayWindow(false);
                            });
                        }
                        break;
                    default:
                        break;
                }
            }
            #endregion

        }


        public enum GroupActions
        {
            /// <summary>
            /// 创建
            /// </summary>
            Create = 0,
            /// <summary>
            /// 邀请
            /// </summary>
            Invite = 1,
            /// <summary>
            /// 申请加入
            /// </summary>
            JoinApply = 2,
            /// <summary>
            /// 申请被批准
            /// </summary>
            JoinAccepted = 3,
            /// <summary>
            /// 加载
            /// </summary>
            Load = 4,
            /// <summary>
            /// 更改群信息
            /// </summary>
            UpdateInfo = 5,
            /// <summary>
            /// 剔除
            /// </summary>
            Expel = 6,
            /// <summary>
            ///  退出
            /// </summary>
            Exit = 7,
            /// <summary>
            /// 解散
            /// </summary>
            Dismiss = 8,
            /// <summary>
            /// 成员更新信息
            /// </summary>
            UpdateMemberInfo = 9,
            /// <summary>
            /// 设置群成员权限
            /// </summary>
            SetMemberPower = 10,
        }
    }
}

