using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMModels
{
    /// <summary>
    /// 群组成员模型
    /// </summary>
    public class GroupMember : BaseModel, IChat
    {
        public static GroupMember CreateEmpty(int id)
        {
            return new GroupMember() { ID = id };
        }

        private GroupMember()
        { 
        }
         
        /// <summary>
        /// 群成员对象
        /// </summary>
        /// <param name="user"></param>
        internal GroupMember(int groupID, UserModel user)
        {
            if (user != null)
            {
                this.ID = user.ID; 
                this.TargetUser = user;
                this.DisplayName = user.DisplayName;
                this.HeadImg = user.HeadImg;
                this.HeadImgMD5 = user.HeadImgMD5;
            }
            this.GroupID = groupID; 
        }
         
        public int GroupID { get; }


        /// <summary>
        /// 对应的用户模型
        /// </summary>
        public UserModel TargetUser { get; }

        private int _index = 10;
        /// <summary>
        /// 加入群的顺序
        /// 默认为10，群主将设置为0,预留1-9为其他管理员顺序
        /// </summary>
        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        private bool _isCreator;
        /// <summary>
        /// 是否群创建者
        /// </summary>
        public bool IsCreator
        {
            get { return _isCreator; }
            set { _isCreator = value; this.OnPropertyChanged(); }
        }

        private bool _isManager;
        /// <summary>
        /// 是否群管理员
        /// </summary>
        public bool IsManager
        {
            get { return _isManager; }
            set { _isManager = value; this.OnPropertyChanged(); }
        }

        private string _displayName;
        /// <summary>
        /// 在群中显示的昵称
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; this.OnPropertyChanged(); }
        }

        private string _nickNameInGroup;

        public string NickNameInGroup
        {
            get { return _nickNameInGroup; }
            set
            {
                if(_nickNameInGroup== value)
                {
                    return;
                }
                _nickNameInGroup = value; this.OnPropertyChanged();
                if (string.IsNullOrEmpty(TargetUser.NickName))
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        this.DisplayName = this.TargetUser.Name;
                    }
                    else
                    {
                        this.DisplayName = value;
                    }
                }
                else
                {
                    this.DisplayName = TargetUser.NickName;
                }
            }
        }



        private string _headImg;
        /// <summary>
        /// 群中显示的头像
        /// </summary>
        public string HeadImg
        {
            get { return _headImg; }
            set { _headImg = value; this.OnPropertyChanged(); }
        }

        private string _headImgMD5;
        /// <summary>
        /// 群中显示的头像
        /// </summary>
        public string HeadImgMD5
        {
            get { return _headImgMD5; }
            set {   _headImgMD5 = value;  }
        }

        private bool _isNotDisturb;
        /// <summary>
        /// 是否免打扰
        /// </summary>
        public bool IsNotDisturb
        {
            get { return _isNotDisturb; }
            set { _isNotDisturb = value; ; this.OnPropertyChanged(); }
        }

        private DateTime? _topMostTime;
        /// <summary>
        /// 置顶时间
        /// </summary>
        public DateTime? TopMostTime
        {
            get { return _topMostTime; }
            set { _topMostTime = value; ; this.OnPropertyChanged(); }
        }

        private bool _isTopMost;
        /// <summary>
        /// 是否置顶
        /// </summary>
        public bool IsTopMost
        {
            get { return _isTopMost; }
            set { _isTopMost = value; ; this.OnPropertyChanged(); }
        }

        private bool _isDefriend;
        /// <summary>
        /// 是否拉黑
        /// </summary>
        public bool IsDefriend
        {
            get { return _isDefriend; }
            set { _isDefriend = value; this.OnPropertyChanged(); }
        }

        private bool _isReceiveStrangerMessage;
        /// <summary>
        /// 是否接收粉丝留言
        /// </summary>
        public bool IsReceiveStrangerMessage
        {
            get { return _isReceiveStrangerMessage; }
            set { _isReceiveStrangerMessage = value; this.OnPropertyChanged(); }
        }
    }
}
