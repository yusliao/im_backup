using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMClient.Views.ChildWindows;
using IMModels;

namespace IMClient.ViewModels
{
    public class AttentionVIewModel : ViewModel
    {
        public AttentionVIewModel(UserModel model) : base(model)
        {
            this.AttentionTip = model.IsAttention ? "取消关注" : "关注";
        }

        private char _firstChar;
        /// <summary>
        /// 首字母字符
        /// </summary>
        public char FirstChar
        {
            get { return _firstChar; }
            set { _firstChar = value; this.OnPropertyChanged(); }
        }

        private char _groupByChar;
        /// <summary>
        /// 分组字符（为了解决排序后‘#’分组不在最后的问题）
        /// </summary>
        public char GroupByChar
        {
            get { return _groupByChar; }
            set { _groupByChar = value; this.OnPropertyChanged(); }
        }

        private string _applyTip = "加为好友";
        public string ApplyTip
        {
            get { return _applyTip; }
            set { _applyTip = value; this.OnPropertyChanged(); }
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
        private string _attentionTip;
        /// <summary>
        /// 关注tip
        /// </summary>
        public string AttentionTip
        {
            get { return _attentionTip; }
            set { _attentionTip = value; this.OnPropertyChanged(); }
        }

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
        private VMCommand _defriendCommand;
        /// <summary>
        /// 移至黑名单
        /// </summary>
        public VMCommand DefriendCommand
        {
            get
            {
                if (_defriendCommand == null)
                    _defriendCommand = new VMCommand(DeFriend);
                return _defriendCommand;
            }
        }

        public void JupmToChat(object para)
        {
            if (this.Model is UserModel)
            {
                AppData.MainMV.JumpToChatModel(this.Model as UserModel);
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
                    SDKClient.SDKClient.Instance.DeleteAttentionUser(user.AttentionID);
                    AttentionTip = "关注";
                    AppData.MainMV.TipMessage = "取消关注成功！";
                }
                else
                {
                    SDKClient.SDKClient.Instance.AddAttention(AppData.Current.LoginUser.User.ID, user.ID);
                    AttentionTip = "取消关注";
                    AppData.MainMV.TipMessage = "关注成功！";
                }
                user.IsAttention = !user.IsAttention;
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

        public void ApplyFriend(object para)
        {

            UserModel user = this.Model as UserModel;
            if (user != null)
            {
                if (AppData.CanInternetAction())
                {
                    if (user.LinkDelType > 2)
                    {
                        var obj = SDKClient.SDKClient.Instance.GetUserPrivacySetting(user.ID);
                        if (obj?.data?.item != null && obj.data.item.verifyFriendApply)
                        {
                            //var isFriendApply= VerificationWindow.ShowInstance(user);
                            //var isFriendApply = VerificationWindow.ShowInstance(user);

                            //if (isFriendApply)
                            //{
                            //    user.IsApplyFriend = true;
                            //    AppData.MainMV.TipMessage = "好友申请已发出！";
                            //    ApplyTip = "已申请";
                            //}
                            //else
                            //{
                            //    user.IsApplyFriend = false;
                            //}
                            return;

                        }
                    }

                    string applyReason = string.Format("我是{0}", AppData.Current.LoginUser.User.Name);
                    //SDKClient.SDKClient.Instance.AddFriend(user.ID, applyReason, "");
                    if (!user.IsAttention && user.LinkDelType >= 2)
                        SDKClient.SDKClient.Instance.AddAttention(AppData.Current.LoginUser.User.ID, user.ID);
                    //user.IsApplyFriend = true;
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

        private void DeFriend(object para)
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
}
