using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMModels
{
    /// <summary>
    /// 用户申请模型
    /// </summary>
    public class UserApplyModel:BaseModel
    {
        public UserApplyModel(UserModel user)
        {
            this.ID = user.ID;
            this.User = user;
        }
         
        /// <summary>
        /// 对应的用户
        /// </summary>
        public UserModel User { get; }


        private int _inviteUserId;
        /// <summary>
        /// 关联ID
        /// </summary>
        public int InviteUserId
        {
            get { return _inviteUserId; }
            set { _inviteUserId = value; }
        }

        private DateTime _applyTime;
        /// <summary>
        /// 申请时间
        /// </summary>
        public DateTime ApplyTime
        {
            get { return _applyTime; }
            set { _applyTime = value; this.OnPropertyChanged(); }
        }

        private string _applyReason;
        /// <summary>
        /// 申请理由
        /// </summary>
        public string ApplyReason
        {
            get { return _applyReason; }
            set { _applyReason = value; this.OnPropertyChanged(); }
        }

    }
}
