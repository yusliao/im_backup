using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using IMModels;
using ToolGood.Words;
using SDKClient.Model;
using System.Runtime.InteropServices;
using System.Security;
using Util;
using System.IO;
using System.Threading;

namespace IMClient.ViewModels
{
    /// <summary>
    /// 好友列表VM
    /// </summary>
    public class FriendListViewModel : ListViewModel<FriendViewModel>
    {
        //static List<char> CHARS = new List<char>();
        //static FriendListViewModel()
        //{
        //    for (int i = 0; i < 26; i++)
        //    {
        //        CHARS.Add((char)(65 + i));
        //    }
        //    CHARS.Add('#');
        //}

        private static FriendViewModel _newFriend;
        /// <summary>
        /// 新的朋友
        /// </summary>
        public static FriendViewModel NewFriend
        {
            get
            {
                if (_newFriend == null)
                {
                    UserModel user = new UserModel()
                    {
                        ID = -1,
                        DisplayName = "新的朋友",
                        HeadImg = IMAssets.ImageDeal.NewHeadImage
                    };

                    _newFriend = new FriendViewModel(user)
                    {
                        ID = user.ID,
                        FirstChar = ' ',
                        GroupByChar = ' ',
                    };
                }
                return _newFriend;
            }
        }

        private static FriendViewModel _fileAssistant;
        /// <summary>
        /// 新的朋友
        /// </summary>
        public static FriendViewModel FileAssistant
        {
            get
            {
                if (_fileAssistant == null)
                {
                    UserModel user = new UserModel()
                    {
                        ID = AppData.Current.LoginUser.ID,
                        DisplayName = "文件小助手",
                        HeadImg = IMAssets.ImageDeal.FileAssistantIcon,
                        TopMostTime = DateTime.MinValue,
                    };

                    _fileAssistant = new FriendViewModel(user)
                    {
                        ID = user.ID,
                        FirstChar = 'h',
                        GroupByChar = ' ',
                        IsFileAssistant = true
                    };
                }
                return _fileAssistant;
            }
        }
        private ChatListViewModel _SearchChatListViewModel;
        public ChatListViewModel SearchChatListViewModel
        {
            get { return _SearchChatListViewModel; }
            set
            {
                _SearchChatListViewModel = value;
                this.OnPropertyChanged();
            }
        }

        public override FriendViewModel SelectedSearchItem
        {
            get =>
                base.SelectedSearchItem;
            set =>
                base.SelectedSearchItem = value;
        }
        /// <summary>
        /// 好友列表VM
        /// </summary> 
        public FriendListViewModel(IListView view) : base(view)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.Items.Clear();
                this.Items.Add(NewFriend);
                this.Items.Add(FileAssistant);
            }));
        }

        protected override IEnumerable<FriendViewModel> GetSearchResult(string key)
        {
            return this.Items.ToList().Where(info => info.ID != -1 && (info.Model as UserModel).DisplayName.Contains(key));
        }
        //public override IEnumerable<FriendViewModel> GetSearchContactResult(string key)
        //{
        //    return null;
        //}

        //public override IEnumerable<FriendViewModel> GetSearchBlackResult(string key)
        //{
        //    return null;
        //}
        //public override IEnumerable<FriendViewModel> GetSearchGroupResult(string key)
        //{
        //    return null;
        //}

        private FriendApplyListViewModel _applyUserListVM;
        /// <summary>
        /// 好友申请列表VM
        /// </summary>
        public FriendApplyListViewModel ApplyUserListVM
        {
            get { return _applyUserListVM; }
            set { _applyUserListVM = value; this.OnPropertyChanged(); }
        }

        private int _applyCount;
        /// <summary>
        /// 申请数量（未同意的数量）
        /// </summary>
        public int ApplyCount
        {
            get { return _applyCount; }
            set { _applyCount = value; this.OnPropertyChanged(); }
        }

        private List<char> _chars;

        public List<char> Chars
        {
            get { return _chars; }
            set { _chars = value; this.OnPropertyChanged(); }
        }

        //private VMCommand _charSelectedCommand;
        ///// <summary>
        ///// 选中首字母
        ///// </summary> 
        //public VMCommand CharSelectedCommand
        //{
        //    get
        //    {
        //        if (_charSelectedCommand == null)
        //            _charSelectedCommand = new VMCommand(CharSelected, new Func<object, bool>(o => o != null));
        //        return _charSelectedCommand;
        //    }
        //}

        //private void CharSelected(object para)
        //{

        //}

        public void LoadDatas(SDKClient.Model.GetContactsListPackage package)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.Items.Clear();
                this.Items.Add(NewFriend);
                this.Items.Add(FileAssistant);
                foreach (var item in package.data.items)
                {
                    UserModel user = AppData.Current.GetUserModel(item.partnerUserId);

                    int value = item.linkDelType;

                    if (value != 0)
                    {
                        value.ToString();
                    }

                    user.Name = item.userName;
                    user.NickName = item.partnerRemark;
                    user.DisplayName = string.IsNullOrEmpty(user.NickName) ? user.Name : user.NickName;
                    user.PhoneNumber = item.mobile;
                    user.Sex = item.sex.ToString();
                    user.Area = string.Format("{0} {1}", item.province, item.city);
                    user.LinkDelType = item.linkDelType;
                    user.LinkType = item.linkType;
                    user.IsNotDisturb = item.doNotDisturb == 0 ? false : true;
                    user.TopMostTime = item.friendTopTime.HasValue ? item.friendTopTime.Value : DateTime.MinValue;
                    user.IsTopMost = (user.TopMostTime == DateTime.MinValue) ? false : true;
                    user.IsDefriend = (user.LinkType == 1 || user.LinkType == 3) ? true : false;

                    user.FriendSource = item.friendSource;
                    user.SourceGroup = item.sourceGroup;
                    user.SourceGroupName = item.sourceGroupName;
                    FriendViewModel friendVM = new FriendViewModel(user);
                    friendVM.FirstChar = Helper.CommonHelper.GetFirstChar(user.DisplayName);
                    friendVM.GroupByChar = Helper.CommonHelper.GetFirstChar(user.DisplayName, true);
                    if (user.FriendSource != 0)
                    {
                        friendVM.IsExitFriendSource = true;
                        switch (user.FriendSource)
                        {
                            case (int)ApplyFriendSource.FriendRecommend:
                                friendVM.ApplyFriendContent = "好友推荐";
                                break;
                            case (int)ApplyFriendSource.Group:
                                friendVM.ApplyFriendContent = "群聊:" + user.SourceGroupName;
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
                    if (user.LinkType == 1 || user.LinkType == 3)
                    {
                        friendVM.FirstChar = friendVM.GroupByChar = ' ';
                    }
                    user.HeadImgMD5 = item.photo;
                    var isHeadImgUpdate = true;
                    if (!user.HeadImg.Equals(IMAssets.ImageDeal.DefaultHeadImage))
                    {
                        if (File.Exists(user.HeadImg))
                        {
                            var info = new FileInfo(user.HeadImg);
                            if (info.Name == item.photo)
                            {
                                isHeadImgUpdate = false;
                            }
                        }
                    }

                    if (isHeadImgUpdate)
                        user.HeadImg = IMClient.Helper.ImageHelper.GetFriendFace(item.photo, (s) =>
                        {
                            user.HeadImg = s;
                        });
                    if (!this.Items.ToList().Any(x => x.ID == item.partnerUserId))
                    {
                        //App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                        //{
                        this.Items.Add(friendVM);
                        //}));
                    }
                }

                UpdateGroupBy();
                //this.ApplyUserListVM.LoadDatas();


                //防止条目数据先加载完，好友列表数据后收到之后，好友某些属性值不一样（主要体现在条目置顶）
                var tempFriendListItems = AppData.MainMV.FriendListVM.Items.ToList();
                var tempItems = AppData.MainMV.ChatListVM.Items.ToList();
                for (int i = 0; i < tempItems.Count; i++)
                {
                    if (tempItems[i].IsGroup)
                    {
                        continue;
                    }
                    var tempFriend = tempFriendListItems.FirstOrDefault(m => m.ID == tempItems[i].ID);
                    if (tempFriend == null)
                    {
                        //if (!tempItems[i].IsTemporaryChat)
                        //{
                        //    AppData.MainMV.ChatListVM.DeleteChatItem(tempItems[i].ID);
                        //}
                        continue;
                    }
                    else if (tempItems[i].ID > 0 && tempFriend != null && tempFriend.Model is UserModel)
                    {
                        if (tempItems[i].IsTemporaryChat)
                        {
                            tempItems[i].IsTemporaryChat = false;
                        }
                        var tempUser = tempFriend.Model as UserModel;
                        if (tempUser.IsTopMost)
                        {
                            if (tempItems[i].Model is ChatModel)
                            {
                                var tempChatModel = tempItems[i].Model as ChatModel;
                                if (tempChatModel.Chat is UserModel)
                                {
                                    var user = tempChatModel.Chat as UserModel;
                                    if (!user.IsTopMost)
                                        user = tempUser;
                                    else
                                        continue;
                                }
                            }
                        }
                    }
                }
                var tempUserModelLst = AppData.TempUserModel.ToList();
                if (tempUserModelLst.Count > 0)
                {
                    foreach (var tempUser in tempUserModelLst)
                    {
                        var tempFriend = tempFriendListItems.FirstOrDefault(m => m.ID == tempUser.ID);
                        if (tempFriend == null)
                        {
                            var tempItem = tempItems.FirstOrDefault(m => m.ID == tempUser.ID);
                            if (tempItem != null)
                                AppData.MainMV.ChatListVM.DeleteChatItem(tempItem.ID);
                        }

                    }
                }

                AppData.MainMV.ChatListVM.ResetSort();
                AppData.MainMV.ChatListVM.IsCloseTrayWindow();
                ApplyUserListVM.UpdateApplyCount();
                AppData.MainMV.UpdateUnReadMsgCount();
                AppData.MainMV.ChatListVM.TotalUnReadCount = AppData.MainMV.TotalUnReadCount;

            }));
            //ThreadPool.QueueUserWorkItem(m =>
            //{

            //});
            //App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            //{

            //}));
        }

        /// <summary>
        /// 添加新好友
        /// </summary>
        /// <param name="userID"></param>
        public FriendViewModel AddNewFriend(int userID)
        {
            var target = this.Items.FirstOrDefault(info => info.Model.ID == userID);
            if (target == null)
            {
                UserModel user = AppData.Current.GetUserModel(userID);
                user.DisplayName = string.IsNullOrEmpty(user.NickName) ? user.Name : user.NickName;
                target = new FriendViewModel(user);
                target.FirstChar = Helper.CommonHelper.GetFirstChar(user.DisplayName);
                target.GroupByChar = Helper.CommonHelper.GetFirstChar(user.DisplayName, true);

                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    try
                    {
                        this.Items.Add(target);
                        UpdateGroupBy();
                    }
                    catch (Exception ex)
                    {

                    }
                }));
            }

            return target;
        }

        /// <summary>
        /// 更新好友列表排序
        /// </summary>
        /// <param name="userID"></param>
        public void UpdateFriendSort(int userID)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                var target = this.Items.ToList().FirstOrDefault(info => info.Model.ID == userID);
                if (target == null) return;
                var user = target.Model as UserModel;
                if (user == null) return;
                target.FirstChar = Helper.CommonHelper.GetFirstChar(user.DisplayName);
                target.GroupByChar = Helper.CommonHelper.GetFirstChar(user.DisplayName, true);
                UpdateGroupBy();
            }));
        }

        public List<CollectionViewGroup> GroupViews { get; private set; }

        public void UpdateGroupBy()
        {
            var cv = (ListCollectionView)CollectionViewSource.GetDefaultView(this.Items);
            if (cv == null)
            {
                return;
            }

            var chars = this.Items.ToList().Where(m => !m.IsFileAssistant).Select(info => info.FirstChar).Distinct().OrderBy(info => info).ToList();
            if (chars.Count >= 2)
            {
                if (chars.LastOrDefault() == '[')
                {
                    chars[chars.Count - 1] = '#';
                }
                chars.RemoveAt(0);
            }

            //if (chars.Count < 24)
            //{
            //    for (int i = 0; i < 24 - chars.Count; i++)
            //    {
            //        chars.Add(' ');
            //    }
            //}

            this.Chars = chars;

            cv.SortDescriptions.Clear();

            cv.SortDescriptions.Add(new SortDescription("FirstChar", ListSortDirection.Ascending));
            cv.SortDescriptions.Add(new SortDescription("Model.DisplayName", ListSortDirection.Ascending));

            cv.GroupDescriptions.Clear();
            cv.GroupDescriptions.Add(new PropertyGroupDescription("GroupByChar"));

            if (GroupViews == null)
            {
                GroupViews = new List<CollectionViewGroup>();
            }
            else
            {
                GroupViews.Clear();
            }

            foreach (var g in cv.Groups)
            {
                GroupViews.Add(g as CollectionViewGroup);
            }
        }

        //public sealed class NaturalStringComparer : IComparer
        //{
        //    public int Compare(object x, object y)
        //    {
        //        if (x == null && y == null) return 0;
        //        if (x == null) return -1;
        //        if (y == null) return 1;

        //        if (((FriendViewModel)x).FirstChar.ToString() == "#" && ((FriendViewModel)y).FirstChar.ToString() == "#")
        //        {
        //            return 0;
        //        }

        //        if (((FriendViewModel)x).FirstChar.ToString() == "#" && ((FriendViewModel)y).FirstChar.ToString() != "#")
        //            return 1;

        //        if (((FriendViewModel)x).FirstChar.ToString() != "#" && ((FriendViewModel)y).FirstChar.ToString() == "#")
        //            return -1;

        //        return string.Compare(((FriendViewModel)x).FirstChar.ToString(), ((FriendViewModel)y).FirstChar.ToString(), true);
        //    }
        //}

        /// <summary>
        /// 删除好友结果 处理
        /// </summary>
        /// <param name="opID">操作ID</param>
        /// <param name="delID">被删除的ID</param>
        public void DealDeleteFriend(int opID, int delID, bool isSync = false, bool isAutoDel = false)
        {
            int targetID = -1;

            IListViewModel listVM = AppData.MainMV.ListViewModel;

            if (opID == AppData.Current.LoginUser.User.ID) //我删除对方
            {
                targetID = delID;
                App.CancelFileOperate(targetID);
                UserModel target = AppData.Current.GetUserModel(delID);

                var friend = AppData.MainMV.FriendListVM.Items.ToList().FirstOrDefault(info => info.Model.ID == target.ID);
                if (friend != null)
                {
                    friend.View = null;
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            AppData.MainMV.FriendListVM.Items.Remove(friend);
                        }
                        catch (Exception ex)
                        {

                        }
                    });
                    if (AppData.MainMV.FriendListVM.SelectedItem == friend)
                    {
                        AppData.MainMV.FriendListVM.SelectedItem = null;
                    }
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    this.UpdateGroupBy();
                });

                //好友备注名已经不存在（可能在共同的群中显示）
                target.DisplayName = target.Name;
                if (target.LinkDelType == 2)
                {
                    //对方已经删除了我 则变成双边删除
                    target.LinkDelType = 3;
                }
                else
                {
                    AppData.TempUserModel.Add(target);
                    target.LinkDelType = 1;
                }
                if (AppData.MainMV.AttentionListVM.Items.Count > 0)
                {
                    var attentionList = AppData.MainMV.AttentionListVM.Items.ToList();
                    var tempAttention = attentionList.FirstOrDefault(m => m.Model.ID == delID);
                    if (tempAttention == null && target.IsAttention)
                        target.IsAttention = false;
                }
                else
                {
                    target.IsAttention = false;
                }
                if (target.IsApplyFriend)
                    target.IsApplyFriend = false;
                var chatVM = AppData.MainMV.ChatListVM.Items.ToList().FirstOrDefault(info => info.Model.ID == target.ID);

                if (chatVM != null)
                {
                    if (chatVM.IsTemporaryChat || isAutoDel)
                    {
                        return;
                    }
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        //chatVM.View = null;
                        chatVM.Chat.Messages.Clear();
                        chatVM.UnReadCount = 0;

                    }));
                    SDKClient.SDKClient.Instance.UpdateChatRoomVisibility(chatVM.ID, 0, false);
                    Util.Helpers.Async.Run(async () => await SDKClient.SDKClient.Instance.DeleteHistoryMsg(chatVM.ID, SDKClient.SDKProperty.chatType.chat));
                    AppData.MainMV.ChatListVM.DeleteChatItem(chatVM.ID);
                    if (AppData.MainMV.ChatListVM.SelectedItem == chatVM)
                    {
                        AppData.MainMV.ChatListVM.SelectedItem = null;
                    }
                }
                else
                {
                    AppData.DeleteFriendIDItems.Add(target.ID);
                }

                if (!isSync)
                {
                    AppData.MainMV.TipMessage = "删除成功！";
                }
            }
            else //对方删除我
            {
                targetID = opID;
                App.CancelFileOperate(targetID);
                UserModel target = AppData.Current.GetUserModel(opID);
                if (target.LinkDelType == 1)
                {
                    //我已经删除对方 则变成双边删除
                    target.LinkDelType = 3;
                }
                else
                {
                    target.LinkDelType = 2;
                }

            }

            AppData.MainMV.ListViewModel = listVM;
        }

        public void UpdateFriendSet(UpdateFriendSetPackage pg)
        {
            if (pg == null || pg.data == null)
            {
                return;
            }

            UserModel model = AppData.Current.GetUserModel(pg.data.friendId);

            switch (pg.data.setType)
            {
                case (int)UpdateFriendSetPackage.FriendSetOption.设置免打扰:
                    if (pg.data.content.Equals("1"))
                    {
                        model.IsNotDisturb = true;
                    }
                    else
                    {
                        model.IsNotDisturb = false;
                    }
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        AppData.MainMV.ChatListVM.IsCloseTrayWindow(false);
                    });

                    break;
                case (int)UpdateFriendSetPackage.FriendSetOption.设置好友备注:
                    model.NickName = pg.data.content;
                    model.DisplayName = string.IsNullOrEmpty(model.NickName) ? model.Name : model.NickName;
                    var friend = AppData.MainMV.FriendListVM.Items.ToList().FirstOrDefault(info => info.Model.ID == model.ID);
                    friend.FirstChar = Helper.CommonHelper.GetFirstChar(model.DisplayName);
                    friend.GroupByChar = Helper.CommonHelper.GetFirstChar(model.DisplayName, true);
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        this.UpdateGroupBy();
                    }));

                    break;
                case (int)UpdateFriendSetPackage.FriendSetOption.设置是否消息置顶:
                    model.TopMostTime = pg.data.content.Equals("1") ? DateTime.Now : DateTime.MinValue;
                    if (pg.data.content.Equals("1"))
                    {
                        model.IsTopMost = true;
                    }
                    else
                    {
                        model.IsTopMost = false;
                    }
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        AppData.MainMV.ChatListVM.ResetSort();
                    }));
                    break;
                default:
                    break;
            }
        }

        public void UpdateFriendRelation(UpdateFriendRelationPackage pg)
        {
            if (pg == null || pg.code != 0 || pg.data == null)
            {
                return;
            }

            int from = pg.from.ToInt();
            int to = pg.to.ToInt();
            if (from != AppData.MainMV.LoginUser.ID)
            {
                UserModel target = AppData.Current.GetUserModel(from);
                if (target == null)
                {
                    return;
                }

                if ((target.LinkType == 1 || target.LinkType == 3) && pg.data.relationType >= 1)
                {
                    target.LinkType = 3;
                }
                else if ((target.LinkType == 1 || target.LinkType == 3) && pg.data.relationType == 0)
                {
                    target.LinkType = 1;
                }
                else
                {
                    target.LinkType = pg.data.relationType;
                }

                if (target.LinkType != 0)
                {
                    App.CancelFileOperate(target.ID);
                }
            }
            else
            {
                //我更新好友的拉黑状态后收到的回包
                UserModel target = AppData.Current.GetUserModel(pg.data.friendId);
                if (target == null)
                {
                    return;
                }

                if ((target.LinkType == 2 || target.LinkType == 3) && pg.data.relationType >= 1)
                {
                    target.LinkType = 3;
                }
                else if ((target.LinkType == 2 || target.LinkType == 3) && pg.data.relationType == 0)
                {
                    target.LinkType = 2;
                }
                else
                {
                    target.LinkType = pg.data.relationType;
                }

                var friendVM = this.Items.ToList().FirstOrDefault(x => x.ID == pg.data.friendId);
                var attentionVM = AppData.MainMV.AttentionListVM.Items.ToList().FirstOrDefault(x => x.ID == pg.data.friendId);

                if (target.LinkType == 0 || target.LinkType == 2)
                {
                    AppData.MainMV.BlacklistVM.DeleteBlacklistItem(target);
                    if (friendVM != null)
                    {
                        friendVM.FirstChar = Helper.CommonHelper.GetFirstChar(target.DisplayName);
                        friendVM.GroupByChar = Helper.CommonHelper.GetFirstChar(target.DisplayName, true);
                    }
                    else if (target.LinkType == 0 && (target.LinkDelType == 0 || target.LinkDelType == 2))
                    {
                        AddNewFriend(target.ID);
                    }
                    if (attentionVM != null)
                    {
                        attentionVM.FirstChar = Helper.CommonHelper.GetFirstChar(target.DisplayName);
                        attentionVM.GroupByChar = Helper.CommonHelper.GetFirstChar(target.DisplayName, true);
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            AppData.MainMV.AttentionListVM.UpdateGroupBy();
                        }));
                    }
                    target.IsDefriend = false;
                }
                else if (target.LinkType == 1 || target.LinkType == 3)
                {
                    AppData.MainMV.BlacklistVM.AddBlacklistItem(target);
                    if (friendVM != null)
                    {
                        friendVM.FirstChar = friendVM.GroupByChar = ' ';
                    }
                    if (attentionVM != null)
                    {
                        attentionVM.FirstChar = attentionVM.GroupByChar = ' ';
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            AppData.MainMV.AttentionListVM.UpdateGroupBy();
                        }));
                    }
                    target.IsDefriend = true;
                }

                if (friendVM != null)
                {
                    if (AppData.MainMV.FriendListVM.SelectedItem == friendVM)
                    {
                        AppData.MainMV.FriendListVM.SelectedItem = null;
                    }
                }

                if (attentionVM != null)
                {
                    if (AppData.MainMV.AttentionListVM.SelectedItem == attentionVM)
                    {
                        AppData.MainMV.AttentionListVM.SelectedItem = null;
                    }
                }


                if (target.LinkDelType == 3)
                {
                    SDKClient.SDKClient.Instance.GetAttentionList();
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    UpdateGroupBy();
                });
            }
        }
    }
}
