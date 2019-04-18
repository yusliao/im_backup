using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using IMModels;
using SDKClient.Model;
using System.Windows.Data;
using System.ComponentModel;
using IMClient.Views.ChildWindows;
using System.Threading;
using SDKClient.DB;
using IMClient.Helper;

namespace IMClient.ViewModels
{
    /// <summary>
    /// 好友申请列表View
    /// </summary>
    public class FriendApplyListViewModel : ListViewModel<UserViewModel>
    {
        /// <summary>
        /// 查找用户列表View
        /// </summary>
        /// <param name="view"></param>
        public FriendApplyListViewModel(IListView view) : base(view)
        {

        }

        protected override IEnumerable<UserViewModel> GetSearchResult(string key)
        {
            return null;
        }
        //protected override IEnumerable<UserViewModel> GetSearchContactResult(string key)
        //{
        //    return null;
        //}

        //protected override IEnumerable<UserViewModel> GetSearchBlackResult(string key)
        //{
        //    return null;
        //}
        //protected override IEnumerable<UserViewModel> GetSearchGroupResult(string key)
        //{
        //    return null;
        //}


        private VMCommand _passApplyCommand;
        /// <summary>
        /// 通过申请命令（同意）
        /// </summary> 
        public VMCommand PassApplyCommand
        {
            get
            {
                if (_passApplyCommand == null)
                    _passApplyCommand = new VMCommand(PassApply, new Func<object, bool>(o => o != null));
                return _passApplyCommand;
            }
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        public void LoadDatas()
        {
            var datas = SDKClient.SDKClient.Instance.property.FriendApplyList;
            App.Current.Dispatcher.Invoke(() =>
            {
                this.Items.Clear();
            });
            foreach (var d in datas)
            {
                ReceiveApply(d,d.IsRead,d.IsChecked, d.TopTime);
            }
        }

        public void UpdateSort()
        {
            var tempFriendApplyItems = this.Items.ToList();
            var tempFriendListItems = AppData.MainMV.FriendListVM.Items.ToList();
            foreach (var friendApplly in tempFriendApplyItems)
            {
                var tempFriend = tempFriendListItems.FirstOrDefault(m => m.ID == friendApplly.ID);
                if (tempFriend != null)
                    friendApplly.IsNeedAccepted = false;

            }
            ICollectionView cv = CollectionViewSource.GetDefaultView(this.Items);
            if (cv == null)
            {
                return;
            }

            cv.SortDescriptions.Clear();
            cv.SortDescriptions.Add(new SortDescription("Model.AppendTime", ListSortDirection.Descending));
        }

        private static readonly object _lockObj = new object();

        /// <summary>
        /// 收到好友申请
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="isRead">是否已读</param>
        /// <param name="isAccepted">是否已接受</param>
        /// <param name="applyTime">申请时间</param>
        //public void ReceiveApply(int userID, bool isRead = true, bool isAccepted = false, DateTime? applyTime = null, string applyRemark = "", int friendApplyId = 0)
        public void ReceiveApply(object obj, bool isRead = true, bool isAccepted = false, DateTime? applyTime = null)
        {
            var userID = 0;
            var applyRemark = string.Empty;
            var friendApplyId = 0;
            var sourceType = 0;
            string sourceGroupId = string.Empty; ;
            var sourceGroupName = string.Empty;
            if (obj is friendApplyItem friend)
            {
                userID = friend.userId;
                applyRemark = friend.applyRemark;
                friendApplyId = friend.friendApplyId;
                sourceType = friend.friendSource;
                sourceGroupId = friend.sourceGroup;
                sourceGroupName = friend.sourceGroupName;
            }
            else if (obj is AddFriendPackage applyPG)
            {
                userID = applyPG.data.userId;
                applyRemark = applyPG.data.applyRemark;
                friendApplyId = applyPG.data.friendApplyId;
                sourceType = applyPG.data.friendSource;
                sourceGroupId = applyPG.data.sourceGroup;
                sourceGroupName = applyPG.data.sourceGroupName;
            }
            if (userID == AppData.Current.LoginUser.ID)
            {
                //我的申请回包
                return;
            }
           

            if (AppData.MainMV.FriendListVM.Items.ToList().Any(info => info.ID == userID))
            {
                isAccepted = true;
            }
            UserModel user = AppData.Current.GetUserModel(userID);
            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
           {

               var target = this.Items.ToList().FirstOrDefault(info => info.ID == userID);
               if (target== null)
               {
                   user.AppendTime = applyTime == null ? DateTime.Now : applyTime.Value;
                   target = new UserViewModel(user);
                   target.IsRead = isRead;
                   //lock (_lockObj)
                   //{
                   //    if (!this.Items.Contains(target))
                   //    {
                   //;
                   this.Items.Insert(0, target);
                   //}
                   //}
                   //ThreadPool.QueueUserWorkItem(m =>
                   //{
                   // SDKClient.SDKClient.Instance.GetUser(userID);
                   //});
               }
               else
               {
                   //int index = this.Items.IndexOf(target);
                   //App.Current.Dispatcher.Invoke(new Action(() =>
                   //{
                   //    this.Items.Move(index, 0);
                   //}));

                   user.AppendTime = applyTime == null ? DateTime.Now : applyTime.Value;
                   target.IsRead = isRead;
               }
               if (!string.IsNullOrEmpty(applyRemark))
               {
                   target.ApplyRemark = applyRemark;
               }
               else
               {
                   target.ApplyRemark = "请求添加你为好友";
               }
               target.FriendApplyId = friendApplyId;
               target.IsNeedAccepted = !isAccepted;
               target.ApplyFriendSourceType = (ApplyFriendSource)sourceType;
               user.FriendSource = sourceType;
               user.SourceGroup=target.SourceGroupID = sourceGroupId;
               user.SourceGroupName=target.SourceGroupName = sourceGroupName;
               ThreadPool.QueueUserWorkItem(m =>
               {
                   UpdateApplyCount();
               });
               //App.Current.Dispatcher.Invoke(new Action(() =>
               //{
               UpdateSort();
               //App.Current.MainWindow.Activate();
               //if (AppData.MainMV.ListViewModel == AppData.MainMV.FriendListVM)
               //{
               //    AppData.MainMV.ListViewModel = AppData.MainMV.FriendListVM;
               //}
               //AppData.MainMV.ChatListVM.IsCloseTrayWindow();
           }));
        }

        /// <summary>
        /// 更新申请数量
        /// </summary>
        /// <param name="isReadAction">当前是否读取动作</param>
        public void UpdateApplyCount(bool isReadAction = false)
        {
            if (isReadAction)
            {
                foreach (var item in this.Items)
                {
                    item.IsRead = true;
                }
                foreach (var friendItem in SDKClient.SDKClient.Instance.property.FriendApplyList)
                {
                    if (!friendItem.IsRead)
                        friendItem.IsRead = true;
                }
                SDKClient.SDKClient.Instance.UpdateFriendApplyIsRead();
            }
            try
            {
                AppData.MainMV.FriendListVM.ApplyCount = this.Items.ToList().Count(info => !info.IsRead && info.IsNeedAccepted);
            }
            catch (Exception ex)
            {

            }
        }
       
        private void PassApply(object para)
        {
            if (para is UserModel user)
            {
                if (AppData.CanInternetAction())
                {
                    var frienApply = Items.ToList().FirstOrDefault(m => m.ID == user.ID);
                    //if (user.LinkDelType == 1) //
                    //{
                    //    string applyReason = string.Format("我是{0}", AppData.Current.LoginUser.User.Name);
                    //    SDKClient.SDKClient.Instance.AddFriend(user.ID, applyReason);
                    //    user.IsApplyFriend = true;
                    //}
                    //else if (user.LinkDelType == 3)
                    {
                        var auditStatus = SDKClient.SDKClient.Instance.GetFriendApplyStatus(frienApply.FriendApplyId);
                        if (auditStatus == 4)
                        {
                            //AppData.MainMV.TipMessage = "好友申请已过期，请主动添加对方！";
                            frienApply.ApplyFriendSourceType = ApplyFriendSource.FriendVerification;
                            frienApply.SourceGroupID = "";
                            var isSure = SetFriendNotesWindow.ShowInstance(frienApply, true);
                            if (isSure)
                            {
                                frienApply.ApplyFriend(user);
                            }
                        }
                        else if (auditStatus == 1)
                        {
                            frienApply.IsNeedAccepted = false;
                        }
                        else
                        {
                            var isSure = SetFriendNotesWindow.ShowInstance(frienApply);
                        }
                        //if (isSure)
                        //SDKClient.SDKClient.Instance.AddFriendAccepted(user.ID, SDKClient.Model.AuditStatus.已通过, user.Name, "", user.HeadImgMD5);

                    }
                    //AppData.MainMV.TipMessage = "正在处理...";
                }
                else
                {
                    // AppData.MainMV.TipMessage = "网络异常，请检查网络设置";
                }
            }
        }
        bool IsUpdateRead;
        /// <summary>
        /// 好友申请审核结果
        /// </summary>
        /// <param name="package">好友审核包</param>
        internal void PackageDeal(AddFriendAcceptedPackage package)
        {
            if (package == null || package.data == null)
            {
                return;
            }
           var tempApplyList=  this.Items.ToList();
            if (package.code != 0)//包错误
            {
                //Application.Current.Dispatcher.Invoke(() =>
                //{
                if (package.code == 636)
                {
                    AppData.MainMV.TipMessage = "已经是好友关系了";
                    UserViewModel userVM = tempApplyList.FirstOrDefault(info => info.ID == package.data.friendId);
                    userVM.IsNeedAccepted = false;
                    var friendApply= SDKClient.SDKClient.Instance.property.FriendApplyList.FirstOrDefault(m=>m.userId==package.data.friendId);
                    if (friendApply != null)
                        friendApply.IsChecked = true;

                }
                else if (package.code == 606)
                {
                    UserViewModel userVM = tempApplyList.FirstOrDefault(info => info.ID == package.data.friendId);
                    userVM.IsNeedAccepted = false;
                    AppData.MainMV.TipMessage = package.error;
                    var friendApply = SDKClient.SDKClient.Instance.property.FriendApplyList.FirstOrDefault(m => m.userId == package.data.friendId);
                    if (friendApply != null)
                        friendApply.IsChecked = true;
                }
                else
                {
                    //_newFriendViewModel.UpdateApplyStatus(package.data.friendId, 0);
                    //IMUI.View.V2.MessageTip.ShowTip("添加好友失败！", IMUI.View.V2.TipTypes.Warning);

                    AppData.MainMV.TipMessage = "添加好友失败！";
                }
                //}); 
            }
            else
            {
                var friendApply = SDKClient.SDKClient.Instance.property.FriendApplyList.FirstOrDefault(m => m.userId == package.data.friendId);
                if (friendApply != null)
                    friendApply.IsChecked = true;
                //我同意好友申请后的回包处理
                if (package.data.userId == AppData.Current.LoginUser.ID)
                {
                    UserViewModel userVM = tempApplyList.FirstOrDefault(info => info.ID == package.data.friendId);

                    if (userVM != null && userVM.Model is UserModel user)
                    {
                        user.LinkDelType = 0;

                        if (user.LinkType == 1 || user.LinkType == 3)
                        {

                        }
                        else
                        {
                          var friendVM =  AppData.MainMV.FriendListVM.AddNewFriend(package.data.friendId);
                            if (friendVM != null&& friendVM.Model is UserModel userModel)
                            {
                                userModel.FriendSource = friendApply.friendSource;
                                userModel.SourceGroup = friendApply.sourceGroup;
                                userModel.SourceGroupName = friendApply.sourceGroupName;
                                if (userModel.FriendSource != 0)
                                {
                                    friendVM.IsExitFriendSource = true;
                                    switch (user.FriendSource)
                                    {
                                        case (int)ApplyFriendSource.FriendRecommend:
                                            friendVM.ApplyFriendContent = "好友推荐";
                                            break;
                                        case (int)ApplyFriendSource.Group:
                                            friendVM.ApplyFriendContent = "群聊:" + userModel.SourceGroupName;
                                            break;
                                        case (int)ApplyFriendSource.PhoneNumSearch:
                                            friendVM.ApplyFriendContent = "手机号搜索";
                                            break;
                                        case (int)ApplyFriendSource.Scan:
                                            friendVM.ApplyFriendContent = "扫一扫";
                                            break;
                                        case (int)ApplyFriendSource.ShopInvitation:
                                            friendVM.ApplyFriendContent = "开店邀请";
                                            break;
                                        case (int)ApplyFriendSource.FriendVerification:
                                            friendVM.ApplyFriendContent = "朋友验证";
                                            break;
                                        case (int)ApplyFriendSource.KeFangNum:
                                            friendVM.ApplyFriendContent = "可访号搜索";
                                            break;
                                        case (int)ApplyFriendSource.InvitationCard:
                                            break;
                                        case (int)ApplyFriendSource.Other:
                                            break;
                                    }
                                }
                                else
                                {
                                    friendVM.IsExitFriendSource = false;
                                }
                            }
                            
                        }

                        var attention = AppData.MainMV.AttentionListVM.Items.ToList().FirstOrDefault(info => info.ID == package.data.friendId);
                        if (attention != null)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                AppData.MainMV.AttentionListVM.Items.Remove(attention);
                            });
                        }
                        //MessageType type;
                        //try
                        //{
                        //    type = (MessageType)Enum.Parse(typeof(MessageType), package.msgType.ToLower());
                        //}
                        //catch
                        //{
                        //    //Views.MessageBox.ShowBox("收到未知消息类型：" + m.msgType);
                        //    continue;
                        //}
                        var chatVM = AppData.MainMV.ChatListVM.GetChat(package.data.friendId);
                        if (chatVM.IsTemporaryChat)
                        {
                            MessageModel msg = new MessageModel()
                            {
                                MsgKey = package.id,
                                SendTime = package.time.Value,
                                IsMine = false,
                                MsgType = MessageType.addfriendaccepted
                                //Target = AppData.Current.LoginUser,
                            };
                            if (SDKClient.SDKClient.Instance.property.FriendApplyList != null && SDKClient.SDKClient.Instance.property.FriendApplyList.Count > 0)
                            {
                                var tempFriendApply = SDKClient.SDKClient.Instance.property.FriendApplyList.FirstOrDefault(a => a.userId == package.data.friendId);
                                if (tempFriendApply != null)
                                {

                                    if (string.IsNullOrEmpty(tempFriendApply.applyRemark))
                                    {
                                        msg.MsgType = MessageType.notification;
                                        msg.TipMessage = msg.Content = ConstString.BecomeFriendsTip;

                                    }
                                    else
                                    {
                                        msg.MsgType = MessageType.notification;
                                        msg.Sender = AppData.Current.GetUserModel(package.data.friendId);
                                        msg.IsMine = false;
                                        msg.Content = ConstString.BecomeFriendsTip;
                                        msg.TipMessage = msg.Content;

                                    }
                                }
                            }
                            else
                            {
                                msg.MsgType = MessageType.notification;
                                msg.TipMessage = msg.Content = ConstString.BecomeFriendsTip;
                            }
                            chatVM.AddMessage(msg);
                            //if (AppData.MainMV.ChatListVM.SelectedItem == chatVM)
                            //{
                            //    chatVM.AddMessageTipEx("已经成为好友，开始聊天吧", msg.SendTime, msg.MsgKey);
                            //    chatVM.AddMessageTipEx("以上为打招呼内容", msg.SendTime, msg.MsgKey);
                            //}


                            //chatVM.cha
                            //chatVM.AddMessageTipEx("已经成为好友，开始聊天吧");
                        }
                        else
                        {
                            user.IsNotDisturb = user.IsTopMost = false;
                            user.TopMostTime = DateTime.MinValue;
                        }
                        userVM.IsNeedAccepted = false;
                        ThreadPool.QueueUserWorkItem(m =>
                        {
                            var obj = SDKClient.SDKClient.Instance.GetFriend(package.data.friendId);
                            if (obj?.data?.item != null && !string.IsNullOrEmpty(obj.data.item.partnerRemark))
                            {
                                user.DisplayName = obj.data.item.partnerRemark;
                            }
                        });
                        chatVM.IsTemporaryChat = false;
                        chatVM.CurrentChatType = ChatType.Normal;
                        chatVM.Acitve();
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            AppData.MainMV.ChatListVM.DeleteStrangerItem(chatVM, false);
                        });
                        var target = tempApplyList.FirstOrDefault(info => info.ID == package.data.friendId);
                        target.IsRead = true;
                    }
                    else
                    {
                        if (package.data.friendId == AppData.Current.LoginUser.ID)
                            return;
                        var tempUser = AppData.Current.GetUserModel(package.data.friendId);
                        tempUser.LinkDelType = 0;

                        var friend = AppData.MainMV.FriendListVM.Items.ToList().FirstOrDefault(info => info.ID == package.data.friendId);

                        if (friend == null)
                        {
                            AppData.MainMV.FriendListVM.AddNewFriend(package.data.friendId);

                            friend = AppData.MainMV.FriendListVM.Items.ToList().FirstOrDefault(info => info.ID == package.data.friendId);
                            if (friend != null)
                            {
                                if (tempUser.LinkType == 1 || tempUser.LinkType == 3)
                                {
                                    friend.FirstChar = friend.GroupByChar = ' ';
                                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        AppData.MainMV.FriendListVM.UpdateGroupBy();
                                    });
                                }
                            }

                            SDKClient.SDKClient.Instance.GetUser(package.data.friendId);
                            var attention = AppData.MainMV.AttentionListVM.Items.ToList().FirstOrDefault(info => info.ID == package.data.friendId);
                            if (attention != null)
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    AppData.MainMV.AttentionListVM.Items.Remove(attention);
                                });
                            }

                            var chartVM = AppData.MainMV.ChatListVM.GetChat(package.data.friendId);
                            if (chartVM.IsTemporaryChat)
                            {
                                MessageModel msg = new MessageModel()
                                {
                                    MsgKey = package.id,
                                    SendTime = package.time.Value,
                                    IsMine = false,
                                    MsgType = MessageType.addfriendaccepted
                                    //Target = AppData.Current.LoginUser,
                                };
                                if (SDKClient.SDKClient.Instance.property.FriendApplyList != null && SDKClient.SDKClient.Instance.property.FriendApplyList.Count > 0)
                                {
                                    var tempFriendApply = SDKClient.SDKClient.Instance.property.FriendApplyList.FirstOrDefault(a => a.userId == package.data.friendId);
                                    if (tempFriendApply != null)
                                    {

                                        if (string.IsNullOrEmpty(tempFriendApply.applyRemark))
                                        {
                                            msg.MsgType = MessageType.notification;
                                            msg.TipMessage = msg.Content = ConstString.BecomeFriendsTip;

                                        }
                                        else
                                        {
                                            msg.Sender = AppData.Current.GetUserModel(package.data.friendId);
                                            msg.IsMine = false;
                                            msg.MsgType = MessageType.notification;
                                            msg.Content = ConstString.BecomeFriendsTip;
                                            msg.TipMessage = ConstString.BecomeFriendsTip;

                                        }
                                    }
                                }
                                else
                                {
                                    msg.MsgType = MessageType.notification;
                                    msg.TipMessage = msg.Content = ConstString.BecomeFriendsTip;
                                }
                                chartVM.AddMessage(msg);
                                if (AppData.MainMV.ChatListVM.SelectedItem == chartVM)
                                {
                                    chartVM.AddMessageTipEx("以上为打招呼内容", msg.SendTime, msg.MsgKey);
                                    //chartVM.AddMessageTipEx("已经成为好友，开始聊天吧", msg.SendTime, msg.MsgKey);
                                }
                            }
                            else
                            {
                                var target = chartVM.Chat.Chat as UserModel;
                                target.IsNotDisturb = target.IsTopMost = false;
                                target.TopMostTime = DateTime.MinValue;
                            }
                            chartVM.IsTemporaryChat = false;
                            chartVM.CurrentChatType = ChatType.Normal;
                            chartVM.Acitve();
                            App.Current.Dispatcher.Invoke(() =>
                                                                                   {

                                                                                       AppData.MainMV.ChatListVM.DeleteStrangerItem(chartVM, false);
                                                                                   });




                        }
                    }
                }
                else//我的申请被对方处理后，得到的回包处理
                {
                    UserModel user = AppData.Current.GetUserModel(package.data.userId);
                    
                    if (user.LinkDelType == 2)
                    {
                        if (!IsUpdateRead)
                        {
                            var tempTask = Task.Run(() =>
                            {
                                IsUpdateRead = true;
                                SDKClient.SDKClient.Instance.GetContactsList();
                            }).ContinueWith(t =>
                            {
                                IsUpdateRead = false;
                            });
                        }
                        
                    }
                    user.LinkDelType = 0;

                    var friend = AppData.MainMV.FriendListVM.Items.ToList().FirstOrDefault(info => info.ID == package.data.userId);

                    if (friend == null)
                    {
                        AppData.MainMV.FriendListVM.AddNewFriend(package.data.userId);

                        friend = AppData.MainMV.FriendListVM.Items.ToList().FirstOrDefault(info => info.ID == package.data.userId);
                        if (friend != null)
                        {
                            if (user.LinkType == 1 || user.LinkType == 3)
                            {
                                friend.FirstChar = friend.GroupByChar = ' ';
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    AppData.MainMV.FriendListVM.UpdateGroupBy();
                                });
                            }
                        }

                        //SDKClient.SDKClient.Instance.GetUser(package.data.userId);
                        ThreadPool.QueueUserWorkItem(m =>
                        {
                            var obj = SDKClient.SDKClient.Instance.GetFriend(package.data.userId);
                            if (user.HeadImgMD5 != obj.data.item.photo)
                            {
                                user.HeadImgMD5 = obj.data.item.photo;
                                IMClient.Helper.ImageHelper.GetFriendFace(obj.data.item.photo, (s) =>
                                {
                                    user.HeadImg = s;
                                });

                            }
                            if (obj?.data?.item != null && !string.IsNullOrEmpty(obj.data.item.partnerRemark))
                            {
                                if (string.IsNullOrEmpty(user.NickName) || user.DisplayName != obj.data.item.partnerRemark)
                                {
                                    user.DisplayName = obj.data.item.partnerRemark;
                                    user.NickName = obj.data.item.partnerRemark;
                                    AppData.MainMV.FriendListVM.UpdateFriendSort(package.data.userId);
                                }
                            }
                        });
                        var attention = AppData.MainMV.AttentionListVM.Items.ToList().FirstOrDefault(info => info.ID == package.data.userId);
                        if (attention != null)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                AppData.MainMV.AttentionListVM.Items.Remove(attention);
                            });
                        }

                        var chartVM = AppData.MainMV.ChatListVM.GetChat(package.data.userId);
                        if (chartVM.IsTemporaryChat)
                        {
                            chartVM.AddMessageTipEx("已经成为好友，开始聊天吧");
                            //chartVM.IsTemporaryChat = false;
                        }
                        else
                        {
                            var target = chartVM.Chat.Chat as UserModel;
                            target.IsNotDisturb = target.IsTopMost = false;
                            target.TopMostTime = DateTime.MinValue;
                        }
                        chartVM.IsTemporaryChat = false;
                        chartVM.CurrentChatType = ChatType.Normal;
                        chartVM.Acitve();
                        App.Current.Dispatcher.Invoke(() =>
                                              {
                                                  AppData.MainMV.ChatListVM.DeleteStrangerItem(chartVM, false);
                                              });



                    }
                    else
                    {
                        // 不为空实际上，我的好友里有对方，而对方删除了我后重新添加
                        //此种情况为服务器代发的审核包，不需要处理，即本地实际上不知道对方删除后又加了我

                        var chartVM = AppData.MainMV.ChatListVM.GetChat(package.data.userId);
                        if (chartVM.IsTemporaryChat)
                        {
                            chartVM.AddMessageTipEx("已经成为好友，开始聊天吧");
                        }
                        else
                        {
                            var target = chartVM.Chat.Chat as UserModel;
                            target.IsNotDisturb = target.IsTopMost = false;
                            target.TopMostTime = DateTime.MinValue;
                        }
                        chartVM.IsTemporaryChat = false;

                        chartVM.CurrentChatType = ChatType.Normal;
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            AppData.MainMV.ChatListVM.DeleteStrangerItem(chartVM, false);
                        });
                    }

                    var apply = tempApplyList.FirstOrDefault(info => info.ID == package.data.userId);

                    if (apply != null)
                    {
                        apply.IsNeedAccepted = false;
                        apply.IsRead = true;
                        // this.UpdateApplyCount(true);
                    }
                }
            }

            UpdateApplyCount();
            App.Current.Dispatcher.Invoke(() =>
            {
                AppData.MainMV.ChatListVM.ResetSort();
            });
        }
    }

}
