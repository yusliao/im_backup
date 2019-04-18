using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using IMClient.Views.ChildWindows;
using IMModels;

namespace IMClient.ViewModels
{
    /// <summary>
    /// 用户VM
    /// </summary>
    public class UserViewModel : ViewModel
    {
        private bool IsUpdateRead;
        public UserViewModel(UserModel model) : base(model)
        {
            this.AttentionTip = model.IsAttention ? "取消关注" : "关注";
            //<<<<<<< .mine
            if (string.IsNullOrEmpty(model.KfNum))
            {
                if (!IsUpdateRead)
                {
                    var tempTask = Task.Run(() =>
                    {
                        IsUpdateRead = true;
                        SDKClient.SDKClient.Instance.GetUser(model.ID);
                    }).ContinueWith(t =>
                    {
                        IsUpdateRead = false;
                    });
                }
            }
            if (model.ID == AppData.Current.LoginUser.ID && model.HaveModifiedKfid == 0)
            {
                IsKfNumEdit = true;
            }
            //var isBool = this.IsNeedAccepted;
            var isDisplaySource = false;
            if (SDKClient.SDKClient.Instance.property.FriendApplyList.Count > 0)
            {
                var friendApply = SDKClient.SDKClient.Instance.property.FriendApplyList.FirstOrDefault(m => m.userId == model.ID);
                if (friendApply != null && !friendApply.IsChecked)
                {
                    isDisplaySource = true;
                }
            }

            if (model.LinkDelType == 0 || model.LinkDelType == 2 || model.LinkDelType == -1 || isDisplaySource)
            {

                KfNum = model.KfNum;
                if (model.FriendSource != 0)
                {
                    IsExitFriendSource = true;
                    switch (model.FriendSource)
                    {
                        case (int)ApplyFriendSource.FriendRecommend:
                            ApplyFriendContent = "好友推荐";
                            break;
                        case (int)ApplyFriendSource.Group:
                            ApplyFriendContent = "群聊:" + model.SourceGroupName;
                            break;
                        case (int)ApplyFriendSource.PhoneNumSearch:
                            ApplyFriendContent = "手机号搜索";
                            break;
                        case (int)ApplyFriendSource.Scan:
                            ApplyFriendContent = "扫一扫";
                            break;
                        case (int)ApplyFriendSource.ShopInvitation:
                            ApplyFriendContent = "开店邀请";
                            break;
                        case (int)ApplyFriendSource.FriendVerification:
                            ApplyFriendContent = "朋友验证";
                            break;
                        case (int)ApplyFriendSource.KeFangNum:
                            ApplyFriendContent = "可访号搜索";
                            break;
                        case (int)ApplyFriendSource.InvitationCard:
                            break;
                        case (int)ApplyFriendSource.Other:
                            break;
                    }
                }
                else
                {
                    IsExitFriendSource = false;
                    ApplyFriendContent = "";
                }
            }
            else
            {
                IsExitFriendSource = false;
                ApplyFriendContent = "";
                if (!string.IsNullOrEmpty(model.KfNum) && model.KfNum.Length > 4)
                {
                    var kfNum = model.KfNum.Remove(model.KfNum.Length - 4, 4);

                    model.KfNum = kfNum.Insert(kfNum.Length, "****");
                }
                KfNum = model.KfNum;
            }
            //||||||| .r14252
            //=======
            //            if (string.IsNullOrEmpty(model.KfNum))
            //                SDKClient.SDKClient.Instance.GetUser(model.ID);
            //            if (model.ID == AppData.Current.LoginUser.ID && model.HaveModifiedKfid == 0)
            //            {
            //                IsKfNumEdit = true;
            //            }
            //>>>>>>> .r15285
        }

        private string _kfNum;
        /// <summary>
        /// 可访号
        /// </summary>
        public string KfNum
        {
            get { return _kfNum; }
            set
            {
                _kfNum = value;
                this.OnPropertyChanged();
            }
        }
        private bool _isRead = true;
        /// <summary>
        /// 是否已读
        /// </summary>
        public bool IsRead
        {
            get { return _isRead; }
            set { _isRead = value; this.OnPropertyChanged(); }
        }

        private bool _isNeedAccepted = true;
        /// <summary>
        /// 是否需要接受
        /// </summary>
        public bool IsNeedAccepted
        {
            get { return _isNeedAccepted; }
            set { _isNeedAccepted = value; this.OnPropertyChanged(); }
        }

        private string _attentionTip;
        /// <summary>
        /// 关注tip
        /// </summary>
        public string AttentionTip
        {
            get { return _attentionTip; }
            set
            {
                _attentionTip = value; this.OnPropertyChanged();
            }
        }
        private int _friendApplyId;
        /// <summary>
        /// 申请ID
        /// </summary>
        public int FriendApplyId
        {
            get { return _friendApplyId; }
            set
            {
                _friendApplyId = value;
                this.OnPropertyChanged();
            }
        }

        private string _applyTip = "加为好友";

        public string ApplyTip
        {
            get { return _applyTip; }
            set { _applyTip = value; this.OnPropertyChanged(); }
        }

        private bool _isEnableReconnectButton = true;

        private string _applyRemark = "请求添加你为好友";
        /// <summary>
        /// 好友申请验证消息
        /// </summary>
        public string ApplyRemark
        {
            get { return _applyRemark; }
            set
            {
                _applyRemark = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isAddFriend;
        /// <summary>
        /// 是否可添加为好友
        /// </summary>
        public bool IsAddFriend
        {
            get { return _isAddFriend; }
            set
            {
                _isAddFriend = value;
                this.OnPropertyChanged();
            }
        }

        private string _numPhone;
        public string NumPhone
        {
            get { return _numPhone; }
            set
            {
                _numPhone = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsEnableReconnectButton
        {
            get { return _isEnableReconnectButton; }
            set { _isEnableReconnectButton = value; this.OnPropertyChanged(); }
        }
        private bool _isAuto;
        /// <summary>
        /// 是否主动点击关注
        /// </summary>
        public bool IsAuto
        {
            get { return _isAuto; }
            set { _isAuto = value; this.OnPropertyChanged(); }
        }



        private bool _isKfNumEdit;

        /// <summary>
        /// 可访号是否可编辑
        /// </summary>
        public bool IsKfNumEdit
        {
            get { return _isKfNumEdit; }
            set
            {
                _isKfNumEdit = value;
                this.OnPropertyChanged();
            }
        }

        private string _errorPromt;
        /// <summary>
        /// 错误提示
        /// </summary>
        public string ErrorPromt
        {
            get { return _errorPromt; }
            set
            {
                _errorPromt = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isShowErrorPromt;
        /// <summary>
        /// 是否有错误提示
        /// </summary>
        public bool IsShowErrorPromt
        {
            get { return _isShowErrorPromt; }
            set
            {
                _isShowErrorPromt = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isSureUpdate;
        /// <summary>
        /// 是否确认修改
        /// </summary>
        public bool IsSureUpdate
        {
            get { return _isSureUpdate; }
            set
            {
                _isSureUpdate = value;
                OnPropertyChanged();
            }
        }
        private ApplyFriendSource _applyFriendSourceType = ApplyFriendSource.Other;
        /// <summary>
        /// 好友申请来源
        /// </summary>
        public ApplyFriendSource ApplyFriendSourceType
        {
            get
            {
                return _applyFriendSourceType;
            }
            set
            {
                _applyFriendSourceType = value;
                //if()
                OnPropertyChanged();
            }
        }

        private string _applyFriendContent;
        public string ApplyFriendContent
        {
            get { return _applyFriendContent; }
            set
            {
                _applyFriendContent = value;
                this.OnPropertyChanged();
            }
        }
        public bool IsExitFriendSource { get; set; }
        public string SourceGroupID { get; set; }
        public string SourceGroupName { get; set; }

        private VMCommand _jumpToChatCommand;
        /// <summary>
        /// 发消息
        /// </summary> 
        public VMCommand JupmToChatCommand
        {
            get
            {
                if (_jumpToChatCommand == null)
                    _jumpToChatCommand = new VMCommand(JupmToChat);
                return _jumpToChatCommand;
            }
        }


        private VMCommand _sharePersonCard;
        /// <summary>
        /// 发送个人名片
        /// </summary>
        public VMCommand SharePersonCard
        {
            get
            {
                if (_sharePersonCard == null)
                    _sharePersonCard = new VMCommand(SendPersonCard);
                return _sharePersonCard;
            }
        }

        private VMCommand _reConnectCommand;
        /// <summary>
        /// 重新连接命令（针对当前用户）
        /// </summary> 
        public VMCommand ReConnectCommand
        {
            get
            {
                if (_reConnectCommand == null)
                    _reConnectCommand = new VMCommand(ReConnect);
                return _reConnectCommand;
            }
        }

        private VMCommand _attentionCommand;
        /// <summary>
        /// 关注与否命令（针对陌生人）
        /// </summary> 
        public VMCommand AttentionCommand
        {
            get
            {
                if (_attentionCommand == null)
                    _attentionCommand = new VMCommand(Attention);
                return _attentionCommand;
            }
        }

        private VMCommand _applyFriendCommand;
        /// <summary>
        /// 好友申请（针对陌生人）
        /// </summary> 
        public VMCommand ApplyFriendCommand
        {
            get
            {
                if (_applyFriendCommand == null)
                    _applyFriendCommand = new VMCommand(ApplyFriend);
                return _applyFriendCommand;
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
                    _ShowBusinessCard = new VMCommand(ShowUserInfoCard, new Func<object, bool>(o => o != null));
                return _ShowBusinessCard;
            }
        }

        private VMCommand _changedFriendNickNameCommand;
        /// <summary>
        /// 修改好友备注
        /// </summary> 
        public VMCommand ChangedFriendNickNameCommand
        {
            get
            {
                if (_changedFriendNickNameCommand == null)
                    _changedFriendNickNameCommand = new VMCommand(ChangedFriendNickName, new Func<object, bool>(o => o != null));
                return _changedFriendNickNameCommand;
            }
        }

        private VMCommand _chanagedKfCardCommand;
        public VMCommand ChanagedKfCardCommand
        {
            get
            {
                if (_chanagedKfCardCommand == null)
                    _chanagedKfCardCommand = new VMCommand(ChanagedKfCard);
                return _chanagedKfCardCommand;
            }
        }

        public VMCommand GotFocusCommand
        {
            get
            {
                return new VMCommand(GotFocus);
            }
        }
        private VMCommand _deleteBlacklistCommand;
        /// <summary>
        /// 移出黑名单
        /// </summary>
        public VMCommand DeleteBlacklistCommand
        {
            get
            {
                if (_deleteBlacklistCommand == null)
                    _deleteBlacklistCommand = new VMCommand(DeleteBlacklist);
                return _deleteBlacklistCommand;
            }
        }

        private VMCommand _defriendCommand;
        /// <summary>
        /// 移至黑名单
        /// </summary>
        public VMCommand DefriendCommand
        {
            get
            {
                if (_defriendCommand == null)
                    _defriendCommand = new VMCommand(Defriend);
                return _defriendCommand;
            }
        }
        /// <summary>
        /// 打开个人名片
        /// </summary>
        /// <param name="para"></param>
        public void ShowUserInfoCard(object para)
        {
            AppData.MainMV.ShowUserBusinessCard(para, IsAddFriend);
        }
        private void GotFocus(object para)
        {
            IsShowErrorPromt = false;
            ErrorPromt = string.Empty;
        }
        private void ChanagedKfCard(object para)
        {
            //if (para == null)
            //    return;
            //var kfId = para.ToString();
            if (!AppData.CanInternetAction())
            {
                return;
            }
            var user = this.Model as UserModel;
            if (KfNum.Length < 6)
            {
                IsShowErrorPromt = true;
                ErrorPromt = "可访号至少6位！";
                return;
            }
            var s = KfNum.First().ToString();
            Regex regEnglish = new Regex("^[a-zA-Z]");
            if (!regEnglish.IsMatch(s))
            {
                ErrorPromt = "可访号必须以字母开头！";
                IsShowErrorPromt = true;
                return;
            }
            if (this.Model is UserModel userModel && userModel.HaveModifiedKfid == 1)
            {
                AppData.MainMV.TipMessage = "可访号已被修改！";
                return;
            }
            if (user != null && user.KfNum == KfNum)
            {
                IsShowErrorPromt = true;
                ErrorPromt = "此可访号已存在，请重新输入！";
                return;
            }

            if (IsSureUpdate && user != null && user.KfNum != KfNum)
            {

                //if (!isUpdate)
                //{
                //    KfNum = userModel.KfNum;
                //    return;
                //}
                SDKClient.SDKClient.Instance.UpdateUserKfId(KfNum);
            }
            else
            {
                IsSureUpdate = true;
            }
        }

        private void SendPersonCard(object para)
        {
            UserModel user = this.Model as UserModel;
            AppData.SendPersonCard(-1, user, true, false);
        }


        public void JupmToChat(object para)
        {
            if (this.Model is UserModel)
            {
                AppData.MainMV.JumpToChatModel(this.Model as UserModel);
            }
        }

        private void ReConnect(object para)
        {
            if (this.Model is UserModel)
            {
                IsEnableReconnectButton = false;

                SDKClient.SDKClient.Instance.StartReConn();

                Task.Delay(5000).ContinueWith((t) =>
                {
                    IsEnableReconnectButton = true;
                });
            }
        }

        public void Attention(object para)
        {
            UserModel user = this.Model as UserModel;

            if (AppData.CanInternetAction())
            {
                if (user.IsAttention)
                {
                    IsAuto = true;
                    SDKClient.SDKClient.Instance.AddAttention(AppData.Current.LoginUser.User.ID, user.ID);
                    AttentionVIewModel vm = new AttentionVIewModel(user);
                    vm.FirstChar = Helper.CommonHelper.GetFirstChar(user.DisplayName);
                    vm.GroupByChar = Helper.CommonHelper.GetFirstChar(user.DisplayName, true);

                    if (user.LinkType == 1 || user.LinkType == 3)
                    {
                        vm.FirstChar = vm.GroupByChar = ' ';
                    }
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        AppData.MainMV.AttentionListVM.Items.Add(vm);
                    });
                }
                else
                {
                    if (user.AttentionID <= 0)
                    {
                        return;
                    }
                    SDKClient.SDKClient.Instance.DeleteAttentionUser(user.AttentionID);
                    AttentionTip = "关注";
                    AppData.MainMV.TipMessage = "取消关注成功！";
                }
            }
            else
            {
                if (AttentionTip.Equals("取消关注"))
                {
                    user.IsAttention = true;
                }
                else
                {
                    user.IsAttention = false;
                }
            }


        }
        public void AttentionResult()
        {
            AttentionTip = "取消关注";
            AppData.MainMV.TipMessage = "关注成功！";
        }
        public void DeleteAttentionResult()
        {
            AttentionTip = "关注";
            AppData.MainMV.TipMessage = "取消关注成功！";
        }

        public void ApplyFriend(object para)
        {
            UserModel user = this.Model as UserModel;
            if (para != null)
                user = para as UserModel;
            if (user != null)
            {
                if (AppData.CanInternetAction())
                {

                    //if (user.LinkType >= 2)
                    //{
                    //    AppData.MainMV.TipMessage = "申请加好友失败，对方已将您加入黑名单！";
                    //    return;
                    //}
                    if (user.LinkDelType > 2)
                    {
                        var obj = SDKClient.SDKClient.Instance.GetUserPrivacySetting(user.ID);
                        if (obj?.data?.item != null && obj.data.item.verifyFriendApply)
                        {
                            //var isFriendApply= VerificationWindow.ShowInstance(user);
                            var isFriendApply = VerificationWindow.ShowInstance(this);

                            if (isFriendApply)
                            {
                                user.IsApplyFriend = false;
                                ApplyTip = "已申请";
                                AppData.MainMV.TipMessage = "好友申请已发出！";
                            }
                            else
                            {
                                user.IsApplyFriend = false;
                            }
                            return;

                        }
                    }

                    string applyReason = string.Format("我是{0}", AppData.Current.LoginUser.User.Name);
                    SDKClient.SDKClient.Instance.AddFriend(this.Model.ID, applyReason, (int)ApplyFriendSourceType, SourceGroupID, SourceGroupName);
                    if (!user.IsAttention && user.LinkDelType >= 2)
                        SDKClient.SDKClient.Instance.AddAttention(AppData.Current.LoginUser.User.ID, user.ID);
                    user.IsApplyFriend = false;
                    AppData.MainMV.TipMessage = "好友申请已发出！";

                    ApplyTip = "已申请";
                }
                else
                {
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        AppData.MainMV.TipMessage = "网络异常，请检查设置！";
                    }));
                }
            }
        }

        private void ChangedFriendNickName(object para)
        {
            FriendViewModel vm = AppData.MainMV.FriendListVM.Items.ToList().FirstOrDefault(info => info.ID == this.Model.ID);
            vm?.ChangedFriendNickNameCommand.Execute(para);
        }

        private void DeleteBlacklist(object para)
        {
            UserModel user = this.Model as UserModel;
            if (user != null)
            {
                string tip = $"确定将{(this.Model as UserModel).DisplayName}移出黑名单？";
                var result = Views.MessageBox.ShowDialogBox(tip);
                if (result && AppData.CanInternetAction())
                {
                    user.IsDefriend = false;
                    AppData.MainMV.BlacklistVM.UpdateFriendRelation(user.ID, 0);
                    AppData.MainMV.BlacklistVM.DeleteBlacklistItem(user);
                }
            }
        }

        public void Defriend(object para)
        {
            if (this.Model is UserModel model)
            {
                string tip = $"确定将{(this.Model as UserModel).DisplayName}移至黑名单？";
                var result = Views.MessageBox.ShowDialogBox(tip);
                if (result && AppData.CanInternetAction())
                {
                    model.IsApplyFriend = false;
                    model.IsDefriend = true;

                    SDKClient.SDKClient.Instance.UpdateFriendRelation(1, model.ID);
                }
            }
        }
    }
    /// <summary>
    /// 好友来源 1：群 2：手机号搜索 3：扫一扫 4：好友推荐 5：开店邀请 6:朋友验证 7：可访号搜索
    /// </summary>
    public enum ApplyFriendSource
    {
        /// <summary>
        /// 群
        /// </summary>
        Group = 1,
        /// <summary>
        ///  手机号搜索
        /// </summary>
        PhoneNumSearch,
        /// <summary>
        ///扫一扫 
        /// </summary>
        Scan,
        /// <summary>
        /// 好友推荐
        /// </summary>
        FriendRecommend,
        /// <summary>
        /// 开店邀请
        /// </summary>
        ShopInvitation,
        /// <summary>
        /// 朋友验证
        /// </summary>
        FriendVerification,
        /// <summary>
        /// 可访号搜索
        /// </summary>
        KeFangNum,
        /// <summary>
        /// 名片邀请
        /// </summary>
        InvitationCard,
        /// <summary>
        /// 其它
        /// </summary>
        Other = 0
    }
}
