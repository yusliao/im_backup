using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using IMModels;
using SDKClient.Model;

namespace IMClient.ViewModels
{
    /// <summary>
    /// 我的好友模型
    /// </summary>
    public class FriendViewModel : ViewModel
    {
        public FriendViewModel(UserModel model) : base(model)
        {
           
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

        private bool _isActive;
        /// <summary>
        /// 是否活跃(特定情况下，显示一下）
        /// </summary>
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; this.OnPropertyChanged(); }
        }

        private bool _isFileAssistant;

        public bool IsFileAssistant
        {
            get { return _isFileAssistant; }
            set
            {
                _isFileAssistant = value;
                this.OnPropertyChanged();
            }
        }
        //private ApplyFriendSource _applyFriendSourceType = ApplyFriendSource.Other;
        ///// </summary>
        //public ApplyFriendSource ApplyFriendSourceType
        //{
        //    get
        //    {
        //        return _applyFriendSourceType;
        //    }
        //    set
        //    {
        //        _applyFriendSourceType = value;
        //        OnPropertyChanged();
        //    }
        //}

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
        //public int SourceGroupID { get; set; }
        //public string SourceGroupName { get; set; }
        #region Commands

        private VMCommand _jumpToChatCommand;
        /// <summary>
        /// 跳转命令
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

        private VMCommand _deleteFriendCommand;
        /// <summary>
        /// 删除好友命令
        /// </summary> 
        public VMCommand DeleteFriendCommand
        {
            get
            {
                if (_deleteFriendCommand == null)
                    _deleteFriendCommand = new VMCommand(DeleteFriend);
                return _deleteFriendCommand;
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

       
        #region Command methods

       

        private void JupmToChat(object para)
        {
            if (this.Model is UserModel user && user.ID > 0)
            {
                if (!IsFileAssistant)
                {
                    AppData.MainMV.JumpToChatModel(this.Model as UserModel);
                }
                else
                {
                    AppData.MainMV.JumpToChatModel(this.Model as UserModel,false,"",true);
                }
            }
        }

        private void DeleteFriend(object para)
        {
            if (this.Model is UserModel model)
            {
                var result = Views.MessageBox.ShowDialogBox(string.Format("是否确认删除好友【{0}】？", model.DisplayName),
                    "删除好友");

                if (result && AppData.CanInternetAction())
                {
                    model.IsApplyFriend = false;
                    model.IsAttention = false;
                    model.NickName = null;
                    model.DisplayName = model.Name;

                    SDKClient.SDKClient.Instance.DeleteFriend(model.ID);
                    ChatViewModel chatVM = AppData.MainMV.ChatListVM.Items.ToList().FirstOrDefault(info => info.ID == model.ID);
                    if (chatVM != null)
                    {
                        chatVM.Chat.Messages.Clear();
                        chatVM.UnReadCount = 0;
                        AppData.MainMV.UpdateUnReadMsgCount();
                        AppData.MainMV.ChatListVM.TotalUnReadCount = AppData.MainMV.TotalUnReadCount;
                    }
                }
            }
        }

        private void ChangedFriendNickName(object para)
        {
            UserModel user = this.Model as UserModel;
            string value = string.Format("{0}", para).Trim();
            if (user != null && value != string.Format("{0}", user.NickName))
            {

                if (AppData.CanInternetAction())
                {
                    //若为空 则显示实际名字
                    if (string.IsNullOrEmpty(value))
                    {
                        user.DisplayName = user.Name;
                        user.NickName = string.Empty;
                    }
                    else
                    {
                        user.DisplayName = user.NickName = value;
                    }
                    SDKClient.SDKClient.Instance.UpdateFriendSet(SDKClient.Model.UpdateFriendSetPackage.FriendSetOption.设置好友备注, value, user.ID);
                }
                else //网络已断开
                {
                    user.NickName = user.NickName;
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
                    model.IsAttention = false;
                    model.NickName = null;
                    model.DisplayName = model.Name;
                    model.IsDefriend = true;
                    SDKClient.SDKClient.Instance.UpdateFriendRelation(1, model.ID);
                }
            }
        }

        #endregion

        #endregion
    }
}
