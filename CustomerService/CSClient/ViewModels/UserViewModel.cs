using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using IMModels;

namespace CSClient.ViewModels
{
    /// <summary>
    /// 用户VM
    /// </summary>
    public class UserViewModel : ViewModel
    {

        public UserViewModel(UserModel model) : base(model)
        {
            this.AttentionTip = model.IsAttention ? "取消关注" : "关注";
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

        private string _applyTip = "加好友";

        public string ApplyTip
        {
            get { return _applyTip; }
            set { _applyTip = value; this.OnPropertyChanged(); }
        }


        private VMCommand _jumpToChatCommand;
        /// <summary>
        /// 跳转命令（针对我的好友）
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
                    _ShowBusinessCard = new VMCommand(AppData.MainMV.ShowUserBusinessCard, new Func<object, bool>(o => o != null));
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

        private void JupmToChat(object para)
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
                SDKClient.SDKClient.Instance.StartReConn();
            }
        }

        private void Attention(object para)
        {
            UserModel user = this.Model as UserModel;

            if (AppData.CanInternetAction())
            {
                if (user.IsAttention)
                {
                    SDKClient.SDKClient.Instance.AddAttention(AppData.Current.LoginUser.User.ID, user.ID);
                    AttentionTip = "取消关注";
                    AppData.MainMV.TipMessage = "关注成功！";
                }
                else
                {
                    SDKClient.SDKClient.Instance.DeleteAttentionUser(user.AttentionID);
                    AttentionTip = "关注";
                    AppData.MainMV.TipMessage = "取消关注成功！";
                }
            }
            else
            {
                if (AttentionTip.Equals("取消关注"))
                {
                    user.IsAttention = false;
                }
                else
                {
                    user.IsAttention = true;
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
                    string applyReason = string.Format("我是{0}", AppData.Current.LoginUser.User.Name);
                    SDKClient.SDKClient.Instance.AddFriend(this.Model.ID, applyReason);
                    user.IsApplyFriend = true;
                    AppData.MainMV.TipMessage = "好友申请已发出！";

                    ApplyTip = "已申请";
                }
            }
        }

        private void ChangedFriendNickName(object para)
        {
            //FriendViewModel vm = AppData.MainMV.FriendListVM.Items.FirstOrDefault(info => info.ID == this.Model.ID);
            //vm?.ChangedFriendNickNameCommand.Execute(para);
        }
    }
   
}
