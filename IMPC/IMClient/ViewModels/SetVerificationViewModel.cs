using IMModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMClient.ViewModels
{
    public class SetVerificationViewModel : ViewModel
    {
        UserModel userModel;
        UserViewModel userViewModel;
        public SetVerificationViewModel(UserViewModel model, bool isFriendAcceptedExpired = false) : base(model)
        {
            VerificationMsg = string.Format("我是{0}", AppData.Current.LoginUser.User.Name);
            userModel = model.Model as UserModel;
            userViewModel = model;
            IsFriendAcceptedExpired = isFriendAcceptedExpired;
        }

        #region 属性
        private string _remarksName;
        /// <summary>
        /// 名称备注
        /// </summary>
        public string RemarksName
        {
            get { return _remarksName; }
            set
            {
                _remarksName = value;
                OnPropertyChanged();
            }
        }
        private bool _isFriendAcceptedExpired;
        public bool IsFriendAcceptedExpired
        {
            get { return _isFriendAcceptedExpired; }
            set
            {
                _isFriendAcceptedExpired = value;
                OnPropertyChanged();
            }
        }

        private string _verificationMsg;
        /// <summary>
        /// 验证消息
        /// </summary>
        public string VerificationMsg
        {
            get { return _verificationMsg; }
            set
            {
                _verificationMsg = value;
                OnPropertyChanged();
            }
        }

        private string _cellPhoneNumber;
        /// <summary>
        /// 手机号
        /// </summary>
        public string CellPhoneNumber
        {
            get { return _cellPhoneNumber; }
            set
            {
                _cellPhoneNumber = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #region 命令
        /// <summary>
        /// 设置好友验证
        /// </summary>
        public VMCommand SureVerificationMsgCommand
        {
            get { return new VMCommand(SetVerificationMsg); }
        }

        //public VMCommand SurePhoneNumberVerifiCommand
        //{
        //    get { return new VMCommand(PhoneNumberVerification); }
        //}

        /// <summary>
        /// 确认好友申请并编辑备注命令
        /// </summary>
        public VMCommand SureRemarksNameCommand
        {
            get { return new VMCommand(SetRemarksName); }
        }
        /// <summary>
        /// 添加好友命令
        /// </summary>
        public VMCommand SureAddFriendCommand
        {
            get { return new VMCommand(ActiveAddFriend); }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 申请好友设置好友验证信息
        /// </summary>
        /// <param name="para"></param>
        private void SetVerificationMsg(object para)
        {
            if (AppData.CanInternetAction())
            {
                SDKClient.SDKClient.Instance.AddFriend(this.Model.ID, VerificationMsg, (int)userViewModel.ApplyFriendSourceType,userViewModel.SourceGroupID,userViewModel.SourceGroupName, RemarksName);
                if (!userModel.IsAttention && userModel.LinkDelType >= 2)
                    SDKClient.SDKClient.Instance.AddAttention(AppData.Current.LoginUser.User.ID, userModel.ID);
            }
            else
            {
                //AppData.MainMV.TipMessage = "网络异常，请检查网络设置";
            }
        }
        //private void PhoneNumberVerification(object para)
        //{

        //}
        /// <summary>
        ///接受好友申请并且给好友设置备注
        /// </summary>
        /// <param name="para"></param>
        private void SetRemarksName(object para)
        {
            if (AppData.CanInternetAction())
            {
                //若为空 则显示实际名字
                if (string.IsNullOrEmpty(RemarksName))
                {
                    userModel.DisplayName = userModel.Name;
                    userModel.NickName = string.Empty;
                }
                else
                {
                    userModel.DisplayName = userModel.NickName = RemarksName;
                }
                SDKClient.SDKClient.Instance.AddFriendAccepted(userModel.ID, SDKClient.Model.AuditStatus.已通过, userModel.Name, RemarksName, userModel.HeadImgMD5);
                //if (!string.IsNullOrEmpty(RemarksName))
                //    SDKClient.SDKClient.Instance.UpdateFriendSet(SDKClient.Model.UpdateFriendSetPackage.FriendSetOption.设置好友备注, RemarksName, userModel.ID);
            }
            else //网络已断开
            {
                userModel.NickName = userModel.NickName;
            }
        }

        private void ActiveAddFriend(object para)
        {

        }
        #endregion
    }
}
