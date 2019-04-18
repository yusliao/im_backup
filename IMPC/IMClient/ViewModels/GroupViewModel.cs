using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using IMModels;
using SDKClient.Model;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Threading;

namespace IMClient.ViewModels
{
    public class GroupViewModel : ViewModel
    {
        public GroupViewModel(GroupModel model, string groupTypeName) : base(model)
        {
            this.GroupTypeName = groupTypeName;
            this.IsAdmin = model.GroupType == GroupType.Admin;
            this.IsCreator = model.OwnerID == AppData.Current.LoginUser.User.ID || this.IsAdmin ? true : false;
            this.IsCanOpera = model.OwnerID == AppData.Current.LoginUser.User.ID || this.IsAdmin ? true : false;
            this.IsJoinGroupNeedCheck = model.JoinAuthType == 1 ? true : false;
        }

        #region Propertys

        private string _groupTypeName;
        /// <summary>
        /// 组类型名称
        /// </summary>
        public string GroupTypeName
        {
            get { return _groupTypeName; }
            set { _groupTypeName = value; this.OnPropertyChanged(); }
        }

        private bool _isCreator;
        /// <summary>
        /// 当前登录用户是否创建者
        /// </summary>
        public bool IsCreator { get { return _isCreator; } set { _isCreator = value; this.OnPropertyChanged(); } }

        private bool _isAdmin;
        /// <summary>
        /// 当前登录用户是否是该群管理员
        /// </summary>
        public bool IsAdmin { get { return _isAdmin; } set { _isAdmin = value; this.OnPropertyChanged(); } }

        private bool _hasMoreMembers;
        /// <summary>
        /// 是否有更多成员
        /// </summary>
        public bool HasMoreMembers
        {
            get { return _hasMoreMembers; }
            set { _hasMoreMembers = value; this.OnPropertyChanged(); }
        }

        private bool _showMoreMembers;
        /// <summary>
        /// 是否显示更多成员
        /// </summary>
        public bool ShowMoreMembers
        {
            get { return _showMoreMembers; }
            set
            {
                if (_showMoreMembers != value)
                {
                    _showMoreMembers = value;
                    this.OnPropertyChanged();

                    CalShowMembers(value);
                }
            }
        }


        private ObservableCollection<GroupMember> _showMembers;
        /// <summary>
        /// 显示的成员(有时候只显示部分）
        /// </summary>
        public ObservableCollection<GroupMember> ShowMembers
        {
            get { return _showMembers; }
            set { _showMembers = value; this.OnPropertyChanged(); }
        }

        private ObservableCollection<GroupNotice> _allNotice;
        /// <summary>
        /// 所有群公告
        /// </summary>
        public ObservableCollection<GroupNotice> AllNotice
        {
            get
            {
                if (_allNotice == null)
                    _allNotice = new ObservableCollection<GroupNotice>();
                return _allNotice;
            }
        }

        private bool _iscanOpera;
        /// <summary>
        /// 是否可发布群公告、删除群公告
        /// </summary>
        public bool IsCanOpera
        {
            get { return _iscanOpera; }
            set { _iscanOpera = value; this.OnPropertyChanged(); }
        }

        private bool _isShowEmptyNotice;
        /// <summary>
        /// 群主是否显示第一条空模板入群须知
        /// </summary>
        public bool IsShowEmptyNotice
        {
            get { return _isShowEmptyNotice; }
            set { _isShowEmptyNotice = value; this.OnPropertyChanged(); }
        }

        private bool _isShowEmptyNoticeOrdinary;
        /// <summary>
        /// 普通群成员展示空模板群公告
        /// </summary>
        public bool IsShowEmptyNoticeOrdinary
        {
            get { return _isShowEmptyNoticeOrdinary; }
            set { _isShowEmptyNoticeOrdinary = value; this.OnPropertyChanged(); }
        }

        private ObservableCollection<UserApplyModel> _applyUsers;
        /// <summary>
        /// 入群申请用户列表
        /// </summary>
        public ObservableCollection<UserApplyModel> ApplyUsers
        {
            get
            {
                if (_applyUsers == null)
                {
                    _applyUsers = new ObservableCollection<UserApplyModel>();
                }
                return _applyUsers;
            }
        }

        private bool _isJoinGroupApply;
        /// <summary>
        /// 是否有入群申请
        /// </summary>
        public bool IsJoinGroupApply
        {
            get { return _isJoinGroupApply; }
            set
            {
                _isJoinGroupApply = value;
                this.OnPropertyChanged();
            }
        }


        private bool _isJoinGroupNeedCheck;
        /// <summary>
        /// 是否需要入群审核
        /// </summary>
        public bool IsJoinGroupNeedCheck
        {
            get { return _isJoinGroupNeedCheck; }
            set
            {
                _isJoinGroupNeedCheck = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region Commands

        private VMCommand _searchGroupMemberCommand;
        public VMCommand SearchGroupMemeberCommand
        {
            get
            {
                if (_searchGroupMemberCommand == null)
                    _searchGroupMemberCommand = new VMCommand(SearchGroupMember);
                return _searchGroupMemberCommand;
            }
        }
        private void SearchGroupMember(object para)
        {
            Dictionary<string, object> paraDic = para as Dictionary<string, object>;
            if (paraDic == null)
            {
                return;
            }
            GroupModel model1 = this.Model as GroupModel;
            string searchStr = paraDic["context1"].ToString().Replace(" ", "");
            if (!string.IsNullOrEmpty(searchStr))
            {
                this.HasMoreMembers = false;

                if (model1 != null)
                {
                    var groupViewModel = AppData.MainMV.GroupListVM.Items.ToList().FirstOrDefault(info => info.ID == this.ID);
                    List<GroupMember> results = new List<GroupMember>();
                    model1.Members.Where(x => x.DisplayName.Contains(searchStr)).ToList().ForEach(x => results.Add(x));
                    ObservableCollection<GroupMember> showmembers = new ObservableCollection<GroupMember>()
                    {
                         GroupMember.CreateEmpty(0),
                    };

                    if (groupViewModel.IsCreator)
                    {
                        showmembers.Add(GroupMember.CreateEmpty(-1));
                    }
                    results.ForEach(x => showmembers.Add(x));
                    //tgBtn.IsChecked=true;//展开全部
                    //tgBtn.Visibility = System.Windows.Visibility.Collapsed;

                    groupViewModel.ShowMembers = showmembers;

                    //ObservableCollection<GroupMember> oc = new ObservableCollection<GroupMember>();
                    //List<GroupMember> li = new List<GroupMember>();
                    //GroupModel groupModel = model1.Chat as GroupModel;
                    //groupModel.Members.Where(x => x.DisplayName.Contains(searchStr)).ToList().ForEach(x => oc.Add(x));
                    //groupModel.Members = oc;

                    //if (groupViewModel!=null)
                    //{
                    //    groupViewModel.ShowMembers.Where(x => x.DisplayName.Contains(searchStr)).ToList().ForEach(y => li.Add(y));
                    //    groupViewModel.ShowMembers = li;
                    //}
                }
            }
            else
            {
                InitializeData();
            }
        }



        private VMCommand _jumpToChatCommand;
        /// <summary>
        /// 开始聊天命令
        /// </summary> 
        public VMCommand JupmToChatCommand
        {
            get
            {
                if (_jumpToChatCommand == null)
                    _jumpToChatCommand = new VMCommand(action => { AppData.MainMV.JumpToChatModel(this.Model as GroupModel); });
                return _jumpToChatCommand;
            }
        }

        private VMCommand _groupInviteMembersCommand;
        /// <summary>
        /// 邀请我的朋友加入群命令
        /// </summary> 
        public VMCommand GroupInviteMembersCommand
        {
            get
            {
                if (_groupInviteMembersCommand == null)
                    _groupInviteMembersCommand = new VMCommand(GroupInviteMembers);
                return _groupInviteMembersCommand;
            }
        }

        private VMCommand _groupRemoveMembersCommand;
        /// <summary>
        /// 踢人命令
        /// </summary> 
        public VMCommand GroupRemoveMembersCommand
        {
            get
            {
                if (_groupRemoveMembersCommand == null)
                    _groupRemoveMembersCommand = new VMCommand(GroupRemoveMembers);
                return _groupRemoveMembersCommand;
            }
        }

        public void InitializeData(bool isSort = false)
        {
            var group = this.Model as GroupModel;
            this.HasMoreMembers = group.Members == null || group.Members.Count > _normalCount;
            //if (isSort)
            //{

            //}
            this.CalShowMembers(this.ShowMoreMembers);
        }

        private VMCommand _joinGroupCommand;
        /// <summary>
        /// 加入群组命令
        /// </summary> 
        public VMCommand JoinGroupCommand
        {
            get
            {
                if (_joinGroupCommand == null)
                    _joinGroupCommand = new VMCommand(JoinGroup);
                return _joinGroupCommand;
            }
        }

        private VMCommand _exitGroupCommand;
        /// <summary>
        /// 退出/解散群组命令
        /// </summary> 
        public VMCommand ExitGroupCommand
        {
            get
            {
                if (_exitGroupCommand == null)
                    _exitGroupCommand = new VMCommand(ExitGroup);
                return _exitGroupCommand;
            }
        }

        private VMCommand _ShowBusinessCard;
        /// <summary>
        /// 显示个人名片命令
        /// </summary> 
        public VMCommand ShowBusinessCard
        {
            get
            {
                if (_ShowBusinessCard == null)
                    _ShowBusinessCard = new VMCommand(ShowUserInfoCard, new Func<object, bool>(o => o != null));
                return _ShowBusinessCard;
            }
        }


        private VMCommand _changedGroupNameCommand;
        /// <summary>
        /// 修改群名称命令
        /// </summary> 
        public VMCommand ChangedGroupNameCommand
        {
            get
            {
                if (_changedGroupNameCommand == null)
                    _changedGroupNameCommand = new VMCommand(ChangedGroupName, new Func<object, bool>(o => o != null));
                return _changedGroupNameCommand;
            }
        }

        private VMCommand _changedGroupRemarkCommand;
        /// <summary>
        /// 修改群介绍命令
        /// </summary> 
        public VMCommand ChangedGroupRemarkCommand
        {
            get
            {
                if (_changedGroupRemarkCommand == null)
                    _changedGroupRemarkCommand = new VMCommand(ChangedGroupRemark, new Func<object, bool>(o => o != null));
                return _changedGroupRemarkCommand;
            }
        }

        private VMCommand _changedMyNickNameInGroupCommand;
        /// <summary>
        /// 修改我在群里的昵称命令
        /// </summary> 
        public VMCommand ChangedMyNickNameInGroupCommand
        {
            get
            {
                if (_changedMyNickNameInGroupCommand == null)
                    _changedMyNickNameInGroupCommand = new VMCommand(ChangedMyNickNameInGroup, new Func<object, bool>(o => o != null));
                return _changedMyNickNameInGroupCommand;
            }
        }


        private VMCommand _applyAcceptCommand;
        /// <summary>
        /// 同意入群申请
        /// </summary> 
        public VMCommand ApplyAcceptCommand
        {
            get
            {
                if (_applyAcceptCommand == null)
                    _applyAcceptCommand = new VMCommand(ApplyAccept, new Func<object, bool>(o => o != null));
                return _applyAcceptCommand;
            }
        }

        private VMCommand _applyRejectCommand;
        /// <summary>
        /// 拒绝（丢弃）入群申请
        /// </summary> 
        public VMCommand ApplyRejectCommand
        {
            get
            {
                if (_applyRejectCommand == null)
                    _applyRejectCommand = new VMCommand(ApplyReject, new Func<object, bool>(o => o != null));
                return _applyRejectCommand;
            }
        }

        private VMCommand _rightButtonContextMenuCommand;
        /// <summary>
        /// 群主右键设置、取消管理员
        /// </summary>
        public VMCommand RightButtonContextMenuCommand
        {
            get
            {
                if (_rightButtonContextMenuCommand == null)
                    _rightButtonContextMenuCommand = new VMCommand(RightButtonContextMenu, new Func<object, bool>(o => o != null));
                return _rightButtonContextMenuCommand;
            }
        }

        private VMCommand _joinGroupNeedCheckedCommand;
        public VMCommand JoinGroupNeedCheckedCommand
        {
            get
            {
                if (_joinGroupNeedCheckedCommand == null)
                    _joinGroupNeedCheckedCommand = new VMCommand(JoinGroupNeedCheck);
                return _joinGroupNeedCheckedCommand;
            }
        }


        #region CommandMethods

        /// <summary>
        /// 打开个人名片
        /// </summary>
        /// <param name="para"></param>
        public void ShowUserInfoCard(object para)
        {
            AppData.MainMV.ShowUserBusinessCard(para,true,ApplyFriendSource.Group,this.ID.ToString(),(this.Model as GroupModel).GroupName);
        }
        /// <summary>
        /// 设置入群是否需要审核
        /// </summary>
        /// <param name="obj"></param>
        private void JoinGroupNeedCheck(object obj)
        {
            GroupViewModel groupVM = obj as GroupViewModel;
            if (groupVM != null)
            {
                if (AppData.CanInternetAction())
                {
                    if (this.IsJoinGroupNeedCheck)
                        SDKClient.SDKClient.Instance.UpdateGroup(groupVM.ID, SetGroupOption.设置入群验证方式, "1");
                    else
                        SDKClient.SDKClient.Instance.UpdateGroup(groupVM.ID, SetGroupOption.设置入群验证方式, "2");
                }
                else
                {
                    IsJoinGroupNeedCheck = false;
                    AppData.MainMV.TipMessage = "网络异常，请检查网络设置";
                }
            }
        }


        /// <summary>
        /// 群主右键设置管理员
        /// </summary>
        /// <param name="para"></param>
        private void RightButtonContextMenu(object para)
        {
            Dictionary<string, object> paraDic = para as Dictionary<string, object>;
            GroupMember member = paraDic["context1"] as GroupMember;
            Grid grid = paraDic["context2"] as Grid;
            ContextMenu cm = new ContextMenu();
            if (this.IsCreator)
            {
                int managerCount = this.ShowMembers.Count(x => x.IsManager);
                if (member.ID == -1 || member.ID == 0 || member.ID == AppData.Current.LoginUser.User.ID)//添加、踢掉、自己等三个图标不需要右键
                    return;
                if (member.IsCreator || this.IsAdmin)
                {
                    grid.ContextMenu = null;
                    return;
                }
                if (!member.IsManager)
                {
                    MenuItem mi = new MenuItem();
                    mi.Header = "设为管理员";
                    cm.Items.Add(mi);
                    grid.ContextMenu = cm;
                    mi.Click += (ss, tt) =>
                    {
                        if (AppData.CanInternetAction())
                        {
                            if (managerCount < 3)
                            {
                                SDKClient.SDKClient.Instance.SetGroupMemberAdmin(SetCancelGroupPowerOption.SetManager, member.ID, this.ID);
                                AppData.MainMV.TipMessage = string.Format("已将{0}设为管理员", member.TargetUser.DisplayName);
                                if (this.HasMoreMembers)
                                    this.ShowMoreMembers = false;
                            }
                            else
                                AppData.MainMV.TipMessage = "您设置的管理员已达上限";
                        }
                        else
                            AppData.MainMV.TipMessage = "网络异常，请检查网络设置";

                    };
                }
                else
                {
                    MenuItem mi = new MenuItem();
                    mi.Header = "取消管理员";
                    cm.Items.Add(mi);
                    grid.ContextMenu = cm;
                    mi.Click += (ss, tt) =>
                    {
                        if (AppData.CanInternetAction())
                        {
                            SDKClient.SDKClient.Instance.SetGroupMemberAdmin(SetCancelGroupPowerOption.CancelManager, member.ID, this.ID);
                            AppData.MainMV.TipMessage = string.Format("已取消{0}管理员", member.TargetUser.DisplayName);
                            if (this.HasMoreMembers)
                                this.ShowMoreMembers = false;
                        }
                        else
                            AppData.MainMV.TipMessage = "网络异常，请检查网络设置";
                    };
                }
            }
        }


        /// <summary>
        /// 邀请 
        /// </summary> 
        private async void GroupInviteMembers(object para)
        {

            if (para is IEnumerable<UserModel> users)
            {
                if (AppData.CanInternetAction() && users.Count() > 0)
                {
                    List<int> ids = users.Where(info => info.LinkDelType == 0 && info.LinkType < 2).Select(info => info.ID).ToList();


                    GroupModel group = this.Model as GroupModel;
                    InviteJoinGroupPackage.Data data = new InviteJoinGroupPackage.Data();
                    data.groupId = group.ID;
                    data.groupIntroduction = group.GroupRemark;
                    data.groupName = group.DisplayName;
                    data.groupPhoto = group.HeadImgMD5;
                    data.InviteUserId = AppData.Current.LoginUser.User.ID;
                    data.inviteUserName = AppData.Current.LoginUser.User.Name;
                    data.inviteUserPhoto = AppData.Current.LoginUser.User.HeadImgMD5;
                    data.userIds = ids;

                    string from = AppData.Current.LoginUser.ID.ToString();
                    string to = string.Empty;

                    if (ids.Count > 0)
                    {
                        to = group.ID.ToString();
                        if (group.Members.Count >= 500)
                        {
                            string content = "群成员已达上限";

                            await SDKClient.SDKClient.Instance.AppendLocalData_NotifyMessage(from, to, content, group.ID, SDKClient.SDKProperty.MessageType.notification, chatType: SDKClient.SDKProperty.chatType.groupChat);
                            ChatViewModel cvm = AppData.MainMV.ChatListVM.GetChat(group.ID, true);
                            cvm.AddMessageTipEx(content);
                            SDKClient.SDKClient.Instance.InviteJoinGroup(data);
                        }
                        else
                        {


                            SDKClient.SDKClient.Instance.InviteJoinGroup(data);

                            var names = users.Where(info => info.LinkDelType == 0 && info.LinkType < 2).Select(info => info.DisplayName).ToArray();

                            string nameValue = string.Empty;
                            foreach (var v in names)
                            {
                                nameValue += v + "、";
                            }

                            nameValue = nameValue.TrimEnd('、');
                            //string contet = $"你已成功对{ids.Count}位好友发送入群邀请，请等候{nameValue}申请入群";
                            string contet = string.Format("已对{0}发送入群邀请", nameValue);

                            await SDKClient.SDKClient.Instance.AppendLocalData_NotifyMessage(from, to, contet, group.ID,
                                  SDKClient.SDKProperty.MessageType.notification, chatType: SDKClient.SDKProperty.chatType.groupChat);

                            ChatViewModel chatVM = AppData.MainMV.ChatListVM.GetChat(group.ID, true);
                            chatVM.AddMessageTipEx(contet);
                        }

                    }
                    else //无成功，全部失败时弹窗提示
                    {
                        AppData.MainMV.TipMessage = "您所邀请群聊的好友中，有人已将您删除或拉黑无法创建群聊!";
                    }
                    var fails = users.Where(info => info.LinkDelType != 0 || info.LinkType >= 2).ToList();
                    foreach (var userModel in fails)
                    {
                        var target = AppData.MainMV.ChatListVM.GetChat(userModel.ID);
                        data.InviteUserId = userModel.ID;
                        to = userModel.ID.ToString();
                        InviteJoinGroupPackage package = new InviteJoinGroupPackage()
                        {
                            id = SDKClient.SDKProperty.RNGId,
                            from = from,
                            to = to,
                            time = DateTime.Now,
                            data = data,
                        };

                        MessageModel msg = new MessageModel()
                        {
                            MsgKey = SDKClient.SDKProperty.RNGId,
                            Sender = AppData.Current.LoginUser.User,
                            SendTime = DateTime.Now,
                            IsMine = true,
                            MsgType = MessageType.invitejoingroup,
                            Target = group,
                            MessageState = MessageStates.Fail,
                        };
                        await SDKClient.SDKClient.Instance.AppendLocalData_InviteJoinGroupPackage(package);
                        if (target.Chat.Chat is UserModel user && user.LinkType >= 2)
                        {
                            await SDKClient.SDKClient.Instance.AppendLocalData_NotifyMessage(from, to, "对方拒绝接受你的消息！", userModel.ID,
                            SDKClient.SDKProperty.MessageType.notification);

                            target.Chat.Messages.Add(msg);
                            target.AddMessageTipEx("对方拒绝接受你的消息！");
                        }
                        else
                        {
                            await SDKClient.SDKClient.Instance.AppendLocalData_NotifyMessage(from, to, "您不在对方通讯录内", userModel.ID,
                            SDKClient.SDKProperty.MessageType.notification);

                            target.Chat.Messages.Add(msg);
                            string actionableContent = null;
                            actionableContent = "有事找TA";
                            target.AddMessageTip("您不在对方通讯录内，", actionableContent: actionableContent);
                        }
                    }
                    //if (fails.Count > 0) //有部分失败，需要分别提示
                    //{
                    //    foreach (int id in fails)
                    //    {
                    //        var target = AppData.MainMV.ChatListVM.GetChat(id);
                    //        data.InviteUserId = id;
                    //        to = id.ToString();
                    //        InviteJoinGroupPackage package = new InviteJoinGroupPackage()
                    //        {
                    //            id = SDKClient.SDKProperty.RNGId,
                    //            from = from,
                    //            to = to,
                    //            time = DateTime.Now,
                    //            data = data,
                    //        };

                    //        await SDKClient.SDKClient.Instance.AppendLocalData_InviteJoinGroupPackage(package);
                    //        await SDKClient.SDKClient.Instance.AppendLocalData_NotifyMessage(from, to, "您不在对方通讯录内！", id,
                    //            SDKClient.SDKProperty.MessageType.notification);

                    //        ChatViewModel chatVM = AppData.MainMV.ChatListVM.GetChat(id);
                    //        MessageModel msg = new MessageModel()
                    //        {
                    //            MsgKey = package.id,
                    //            Sender = AppData.Current.LoginUser.User,
                    //            SendTime = package.time.Value,
                    //            IsMine = true,
                    //            MsgType = MessageType.invitejoingroup,
                    //            Target = group,
                    //            MessageState = MessageStates.Fail,
                    //        };

                    //        chatVM.Chat.Messages.Add(msg);
                    //        chatVM.AddMessageTipEx("您不在对方通讯录内！");
                    //    }
                    //}
                }
            }
        }

        /// <summary>
        /// 踢人 
        /// </summary> 
        private void GroupRemoveMembers(object para)
        {
            if (para is IEnumerable<UserModel> users && this.Model is GroupModel group)
            {
                if (AppData.CanInternetAction())
                {
                    List<int> ids = users.Select(info => info.ID).ToList();
                    List<string> userNames = users.Select(info => info.DisplayName).ToList();
                    SDKClient.SDKClient.Instance.ExitGroup(ids, AppData.Current.LoginUser.User.ID, this.ID, userNames);

                    foreach (UserModel user in users)
                    {
                        user.RemoveFromGroup(group);
                    }
                    GroupViewModel gvm = AppData.MainMV.GroupListVM.Items.ToList().FirstOrDefault(x => x.ID == group.ID);
                    gvm.InitializeData();
                    if (gvm.HasMoreMembers)
                        gvm.ShowMoreMembers = false;
                }
            }
        }

        /// <summary>
        /// 加入群
        /// </summary> 
        private void JoinGroup(object para)
        {
            if (AppData.CanInternetAction())
            {
                var user = AppData.MainMV.LoginUser.User;
                JoinGroupPackage.Data data = new JoinGroupPackage.Data()
                {
                    groupId = this.ID,
                    InviteUserId = (this.Model as GroupModel).AppendID,
                    isAccepted = true,
                    photo = user.HeadImgMD5,
                    remark = string.Format("我是 {0}", user.Name),
                    userId = user.ID,
                    userName = user.Name,
                };
                SDKClient.SDKClient.Instance.JoinGroup(data);
            }
        }

        /// <summary>
        /// 退出群
        /// </summary> 
        private void ExitGroup(object para)
        {
            if (AppData.CanInternetAction())
            {
                string tipInfo = null;
                if (this.IsCreator && !this.IsAdmin)
                {
                    tipInfo = "确定退出并解散该群吗？";
                }
                else
                {
                    tipInfo = "确定退出该群吗？";
                }

                if (Views.MessageBox.ShowDialogBox(tipInfo, "提示"))
                {
                    if (IsCreator && !this.IsAdmin)
                    {
                        //如果是群创建者，直接解散群
                        SDKClient.SDKClient.Instance.DismissGroup(this.ID);
                    }
                    else
                    {
                        List<int> memberIds = new List<int>() { AppData.Current.LoginUser.ID };
                        //自己退群 不传管理员ID
                        SDKClient.SDKClient.Instance.ExitGroup(memberIds, 0, this.ID);
                    }
                }
            }
        }

        /// <summary>
        /// 更改群名称
        /// </summary> 
        private void ChangedGroupName(object para)
        {
            GroupModel group = this.Model as GroupModel;
            string changedValue = (string)para;
            //ObservableCollection<object> o = para as ObservableCollection<object>;
            if (string.IsNullOrEmpty(changedValue))
                return;
            if (group.DisplayName.Equals(changedValue))
                return;

            //TextBlock textBlock = null;
            //if (paraDic!=null)
            //{
            //txbBox = paraDic["context1"] as TextBox;
            //textBlock = paraDic["context2"] as TextBlock;
            //}
            //else
            //{
            //    txbBox = o[0] as TextBox;
            //    textBlock = (o[1] as Border).Child as TextBlock;
            //}


            string value = string.Format("{0}", changedValue).Trim();
            int index = 0;
            bool isFirstBadWord = SDKClient.SDKProperty.stringSearchEx.FindFirst(value, out index);
            bool isContainsBadWord = SDKClient.SDKProperty.stringSearchEx.ContainsAny(value);
            List<string> badWordLi = SDKClient.SDKProperty.stringSearchEx.FindAll(value);
            List<string> goodWordLi = value.Split(badWordLi.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> badWordLiOperate = new List<string>();
            StringBuilder stringBuilder = new StringBuilder(value);
            foreach (string child in badWordLi)
            {
                badWordLiOperate.Add("|" + child + "|");
            }
            foreach (string child1 in badWordLi)
            {
                foreach (string child2 in badWordLiOperate)
                {
                    if (child1.Equals(child2.Replace("|", string.Empty)))
                        stringBuilder.Replace(child1, child2);
                }
            }
            List<string> resultList = new List<string>(stringBuilder.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
            if (isFirstBadWord || isContainsBadWord)
            {
                //if(paraDic!=null)
                //{
                //    txbBox.Visibility = System.Windows.Visibility.Collapsed;
                //    textBlock.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    txbBox.Visibility = System.Windows.Visibility.Collapsed;
                //    (o[1] as Border).Visibility = Visibility.Visible;
                //}

                //textBlock.Inlines.Clear();

                //BrushConverter brushConverter = new BrushConverter();
                //Brush brush = (Brush)brushConverter.ConvertFromString("#FEE057");
                //foreach (string child in resultList)
                //{
                //    if (badWordLi.Contains(child))
                //    {
                //        textBlock.Inlines.Add(new Run(child) { Background = brush });
                //    }
                //    else
                //    {
                //        textBlock.Inlines.Add(new Run(child));
                //    }
                //}
                StringBuilder sb = new StringBuilder();
                badWordLi.ForEach(x => sb.Append("\"" + x + "\"" + "、"));
                string result = sb.ToString().TrimEnd('、');

                if (IMClient.Views.MessageBox.ShowDialogBox(string.Format("【群名】中包含敏感词{0}，请修改后再试", result), isCancelShow: false))
                {
                    //if(paraDic!=null)
                    //{
                    //    textBlock.Visibility = Visibility.Collapsed;
                    //    txbBox.Visibility = Visibility.Visible;
                    //}
                    //else
                    //{
                    //    textBlock.Visibility = Visibility.Collapsed;
                    //    (o[1] as Border).Visibility = Visibility.Visible;
                    //}
                    //txbBox.Focus();
                }
            }
            else
            {
                if (group != null)
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





        }

        /// <summary>
        /// 更改群简介
        /// </summary> 
        private void ChangedGroupRemark(object para)
        {
            GroupModel group = this.Model as GroupModel;
            Dictionary<string, object> paraDic = para as Dictionary<string, object>;
            TextBox txbBox = paraDic["context1"] as TextBox;
            //TextBlock textBlock = paraDic["context2"] as TextBlock;

            string value = string.Format("{0}", txbBox.Text).Trim();
            int index = 0;
            bool isFirstBadWord = SDKClient.SDKProperty.stringSearchEx.FindFirst(value, out index);
            bool isContainsBadWord = SDKClient.SDKProperty.stringSearchEx.ContainsAny(value);
            List<string> badWordLi = SDKClient.SDKProperty.stringSearchEx.FindAll(value);
            List<string> goodWordLi = value.Split(badWordLi.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> badWordLiOperate = new List<string>();
            StringBuilder stringBuilder = new StringBuilder(value);
            foreach (string child in badWordLi)
            {
                badWordLiOperate.Add("|" + child + "|");
            }
            foreach (string child1 in badWordLi)
            {
                foreach (string child2 in badWordLiOperate)
                {
                    if (child1.Equals(child2.Replace("|", string.Empty)))
                        stringBuilder.Replace(child1, child2);
                }
            }
            List<string> resultList = new List<string>(stringBuilder.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
            if (isFirstBadWord || isContainsBadWord)
            {
                //txbBox.Visibility = System.Windows.Visibility.Collapsed;
                //textBlock.Visibility = Visibility.Visible;
                //textBlock.Inlines.Clear();

                //BrushConverter brushConverter = new BrushConverter();
                //Brush brush = (Brush)brushConverter.ConvertFromString("#FEE057");
                //foreach (string child in resultList)
                //{
                //    if (badWordLi.Contains(child))
                //    {
                //        textBlock.Inlines.Add(new Run(child) { Background = brush });
                //    }
                //    else
                //    {
                //        textBlock.Inlines.Add(new Run(child));
                //    }
                //}
                StringBuilder sb = new StringBuilder();
                badWordLi.ForEach(x => sb.Append("\"" + x + "\"" + "、"));
                string result = sb.ToString().TrimEnd('、');
                if (IMClient.Views.MessageBox.ShowDialogBox(string.Format("【群简介】中包含敏感词{0}，请修改后再试", result), isCancelShow: false))
                {
                    //textBlock.Visibility = Visibility.Collapsed;
                    //txbBox.Visibility = Visibility.Visible;
                    txbBox.Focus();
                }
            }
            else
            {
                if (group != null && value != string.Format("{0}", group.GroupRemark))
                {
                    if (AppData.CanInternetAction())
                    {
                        group.GroupRemark = value;
                        SDKClient.SDKClient.Instance.UpdateGroup(group.ID, SDKClient.Model.SetGroupOption.修改群简介, group.GroupRemark);
                    }
                    else //网络已断开
                    {
                        group.GroupRemark = group.GroupRemark;
                    }
                }
            }

        }

        /// <summary>
        /// 更改我在群里的昵称
        /// </summary> 
        private void ChangedMyNickNameInGroup(object para)
        {
            GroupModel group = this.Model as GroupModel;
            Dictionary<string, object> paraDic = para as Dictionary<string, object>;
            TextBox txbBox = paraDic["context1"] as TextBox;
            //TextBlock textBlock = paraDic["context2"] as TextBlock;

            string value = string.Format("{0}", txbBox.Text).Trim();
            int index = 0;
            bool isFirstBadWord = SDKClient.SDKProperty.stringSearchEx.FindFirst(value, out index);
            bool isContainsBadWord = SDKClient.SDKProperty.stringSearchEx.ContainsAny(value);
            List<string> badWordLi = SDKClient.SDKProperty.stringSearchEx.FindAll(value);
            List<string> goodWordLi = value.Split(badWordLi.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> badWordLiOperate = new List<string>();
            StringBuilder stringBuilder = new StringBuilder(value);
            foreach (string child in badWordLi)
            {
                badWordLiOperate.Add("|" + child + "|");
            }
            foreach (string child1 in badWordLi)
            {
                foreach (string child2 in badWordLiOperate)
                {
                    if (child1.Equals(child2.Replace("|", string.Empty)))
                        stringBuilder.Replace(child1, child2);
                }
            }
            List<string> resultList = new List<string>(stringBuilder.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
            if (isFirstBadWord || isContainsBadWord)
            {
                //txbBox.Visibility = System.Windows.Visibility.Collapsed;
                //textBlock.Visibility = Visibility.Visible;
                //textBlock.Inlines.Clear();

                //BrushConverter brushConverter = new BrushConverter();
                //Brush brush = (Brush)brushConverter.ConvertFromString("#FEE057");
                //foreach (string child in resultList)
                //{
                //    if (badWordLi.Contains(child))
                //    {
                //        textBlock.Inlines.Add(new Run(child) { Background = brush });
                //    }
                //    else
                //    {
                //        textBlock.Inlines.Add(new Run(child));
                //    }
                //}
                StringBuilder sb = new StringBuilder();
                badWordLi.ForEach(x => sb.Append("\"" + x + "\"" + "、"));
                string result = sb.ToString().TrimEnd('、');
                if (IMClient.Views.MessageBox.ShowDialogBox(string.Format("【我在本群的昵称】中包含敏感词{0}，请修改后再试", result), isCancelShow: false))
                {
                    //textBlock.Visibility = Visibility.Collapsed;
                    //txbBox.Visibility = Visibility.Visible;
                    txbBox.Focus();
                }
            }
            else
            {
                if (group != null && value != group.MyNickNameInGroup)
                {
                    //若为空,改为本身名称
                    if (AppData.CanInternetAction())
                    {
                        var gm = AppData.Current.LoginUser.User.GetInGroupMember(group);
                        group.MyNickNameInGroup = gm.NickNameInGroup = value;
                        SDKClient.SDKClient.Instance.UpdateUserSetsInGroup(group.ID, 1, value);
                    }
                    else //网络已断开
                    {
                        group.MyNickNameInGroup = group.MyNickNameInGroup;
                    }
                }
            }

        }

        /// <summary>
        ///  入群申请 -同意 
        /// </summary> 
        private void ApplyAccept(object para)
        {
            if (para is UserApplyModel apply && AppData.CanInternetAction())
            {
                var data = new JoinGroupAcceptedPackage.Data()
                {
                    auditStatus = 1,
                    groupId = this.ID,
                    memberId = apply.User.ID,
                    userName = apply.User.Name,
                    photo = apply.User.HeadImgMD5,
                    auditRemark = null,
                };

                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var gorup = this.Model as GroupModel;
                    this.ApplyUsers.Remove(apply);
                    if (this.ApplyUsers.Count == 0)
                        this.IsJoinGroupApply = false;
                    var member = apply.User.GetInGroupMember(gorup, false);

                    apply.User.AddToGroup(gorup);

                }));
                SDKClient.SDKClient.Instance.JoinGroupAccepted(data);
            }
            else
            {
                AppData.MainMV.TipMessage = "网络异常，请检查网络设置";
            }
        }



        /// <summary>
        /// 入群申请 -拒绝
        /// </summary> 
        private void ApplyReject(object para)
        {
            if (para is UserApplyModel apply && AppData.CanInternetAction())
            {
                var data = new JoinGroupAcceptedPackage.Data()
                {
                    auditStatus = 3,
                    groupId = this.ID,
                    memberId = apply.User.ID,
                    userName = apply.User.Name,
                    photo = apply.User.HeadImgMD5,
                    auditRemark = null,
                };

                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.ApplyUsers.Remove(apply);
                    if (this.ApplyUsers.Count == 0)
                        this.IsJoinGroupApply = false;
                }));
                SDKClient.SDKClient.Instance.JoinGroupAccepted(data);
            }
        }

        #endregion

        #endregion


        /// <summary>
        /// 获取群成员
        /// </summary>
        /// <param name="isUpdate">是否刷新，刷新则一定重新获取成员</param>
        public void GetGroupMemberList(bool isUpdate = false)
        {
            if (isUpdate || this.ShowMembers == null || this.ShowMembers.Count == 0)
            {
                try
                {
                    ThreadPool.QueueUserWorkItem(m =>
                    {
                        //获取成员列表
                        SDKClient.SDKClient.Instance.GetGroupMemberList(this.Model.ID, !isUpdate);
                    });
                }
                catch { }
            }
        }
        public void AddNotice(AddNoticePackage package)
        {
            GroupNotice gn = new GroupNotice()
            {
                GroupId = package.data.groupId,
                GroupName = package.data.groupName,
                NoticeId = package.data.noticeId,
                NoticeType = package.data.type,
                NoticeContent = package.data.content,
                NoticeTitle = package.data.title,
                NoticeReleTime = package.data.publishTime ?? DateTime.Now
            };
            RefreshNoticeList(gn);
        }

        private int _normalCount
        {
            get { return this.IsCreator || this.IsAdmin ? 6 : 7; }
        }


        /// <summary>
        /// 加载群成员
        /// </summary>
        /// <param name="dataMembers"></param>
        public void LoadMembers(IList<SDKClient.Model.groupmemberInfo> dataMembers)
        {
            GroupModel group = this.Model as GroupModel;
            group.Members = new ObservableCollection<GroupMember>();
            group.MenbersCount = dataMembers.Count;
            //群主 排在首位
            var owner = dataMembers.FirstOrDefault(info => info.userId == group.OwnerID);
            dataMembers.Remove(owner);
            GroupMember gm = CreateMember(owner);

            ////管理员紧随其后
            //var admins = dataMembers.Where(info => info.userPower == 1).OrderBy(info => info.createTime).ToList();
            //foreach (var admin in admins)
            //{
            //    App.Current.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        dataMembers.Remove(admin);
            //        CreateMember(admin);
            //    }));
            //}

            ////其他成员
            //dataMembers = dataMembers.OrderBy(info => info.createTime).ToList();
            //foreach (var m in dataMembers)
            //{
            //    App.Current.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        CreateMember(m);
            //    }));
            //}

            //this.HasMoreMembers = group.Members.Count > _normalCount;
            //CalShowMembers(false);


            Task.Factory.StartNew(() =>
            {
                var admins = dataMembers.Where(info => info.userPower == 1).OrderBy(info => info.createTime).ToList();
                foreach (var admin in admins)
                {
                    dataMembers.Remove(admin);
                    CreateMember(admin);
                }
            }).ContinueWith((t) =>
            {
                dataMembers = dataMembers.OrderBy(info => info.createTime).ToList();
                foreach (var m in dataMembers)
                {
                    CreateMember(m);
                }
            }).ContinueWith((t) =>
            {
                this.HasMoreMembers = group.Members.Count > _normalCount;
                CalShowMembers(false);
            });
        }

        private GroupMember CreateMember(SDKClient.Model.groupmemberInfo m)
        {
            if (m == null) return null;
            UserModel user = AppData.Current.GetUserModel(m.userId);
            GroupMember gm = user.AddToGroup(this.Model as GroupModel);

            user.Name = m.userName;
            user.PhoneNumber = m.mobile;

            //App.Current.Dispatcher.BeginInvoke(new Action(() =>
            //{
            var isHeadImgUpdate = true;
            if (!user.HeadImg.Equals(IMAssets.ImageDeal.DefaultHeadImage))
            {
                if (File.Exists(user.HeadImg))
                {
                    var info = new FileInfo(user.HeadImg);
                    if (info.Name == m.photo)
                    {
                        isHeadImgUpdate = false;
                    }
                }
            }

            if (isHeadImgUpdate)
                user.HeadImg = IMClient.Helper.ImageHelper.GetFriendFace(m.photo, (s) => user.HeadImg = s);
            //}));
            user.Sex = m.sex.ToString();
            user.Area = string.Format("{0} {1}", m.province, m.city);

            string displayNameInGroup;
            if (!string.IsNullOrEmpty(user.NickName))
            {
                displayNameInGroup = user.NickName;
            }
            else if (!string.IsNullOrEmpty(m.memoInGroup))
            {
                displayNameInGroup = m.memoInGroup;
                gm.NickNameInGroup = m.memoInGroup;
            }
            else
            {
                displayNameInGroup = m.userName;
            }

            gm.DisplayName = displayNameInGroup;

            if (gm.ID == AppData.Current.LoginUser.ID)
            {
                (this.Model as GroupModel).MyNickNameInGroup = gm.DisplayName;
            }

            switch (m.userPower)
            {
                case 0:
                    break;
                case 1:
                    gm.IsManager = true;
                    break;
                case 2:
                    gm.IsCreator = true;
                    break;
            }
            return gm;
        }

        public void CalShowMembers(bool isShowAll)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {

                GroupModel group = this.Model as GroupModel;

                ObservableCollection<GroupMember> showmembers = new ObservableCollection<GroupMember>() //添加邀请按钮
            {
                GroupMember.CreateEmpty(0),
            };

                if (this.IsCreator || this.IsAdmin) //移除按钮，只有群主和管理员有
                {
                    showmembers.Add(GroupMember.CreateEmpty(-1));
                }

                //if (this.IsCreator)
                //    showmembers.Add(GroupMember.CreateEmpty(-2));

                if (group.Members == null)
                    return;
                //暂时显示两行
                if (group.Members?.Count > _normalCount && !isShowAll)
                {
                    group.Members.Take(_normalCount).ToList().ForEach(x => showmembers.Add(x));
                    //showmembers.AddRange(group.Members.Take(_normalCount));
                }
                else
                {
                    group.Members.ToList().ForEach(x => showmembers.Add(x));
                    //showmembers.AddRange(group.Members);
                }

                this.ShowMembers = showmembers;
            }));
        }



        public void RefreshNoticeList(GroupNotice notice)
        {
            GroupViewModel gvm = AppData.MainMV.GroupListVM.Items.ToList().FirstOrDefault(x => (x.Model as GroupModel).ID == notice.GroupId);
            gvm.AllNotice.Insert(0, new GroupNotice()
            {
                GroupId = notice.GroupId,
                ID = notice.ID,
                GroupName = notice.GroupName,
                IsCanOperate = notice.IsCanOperate,
                NoticeId = notice.NoticeId,
                IsToTop = notice.IsToTop,
                NoticeContent = notice.NoticeContent,
                NoticeTitle = notice.NoticeTitle,
                NoticeType = notice.NoticeTitle.Equals("入群须知") ? 1 : 0,
                NoticeReleTime = notice.NoticeReleTime
            });
            if (gvm.AllNotice.Any(x => x.NoticeType == 1))
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    GroupNotice gnTice = gvm.AllNotice.FirstOrDefault(x => x.NoticeType == 1);
                    gvm.AllNotice.Remove(gnTice);
                    gvm.AllNotice.OrderByDescending(x => x.NoticeReleTime);
                    gvm.AllNotice.Insert(0, gnTice);
                    gvm.IsShowEmptyNotice = false;
                    gvm.IsShowEmptyNoticeOrdinary = false;

                }));
            }
            else
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    gvm.AllNotice.OrderByDescending(x => x.NoticeReleTime);
                    gvm.IsShowEmptyNotice = true;
                }));
            }
        }

    }
}
