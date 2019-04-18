
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMModels
{
    public enum GroupType
    {
        Owner,
        Admin,
        Join
    }

    /// <summary>
    /// 群组模型
    /// </summary>
    public class GroupModel : BaseModel, IChat
    {
        private int _ownerID;
        /// <summary>
        /// 群主ID
        /// </summary>
        public int OwnerID
        {
            get { return _ownerID; }
            set { _ownerID = value; }
        }


        private ObservableCollection<GroupMember> _members;
        /// <summary>
        /// 群组成员集合
        /// </summary>
        public ObservableCollection<GroupMember> Members
        {
            get
            {
                //if (_members.Count > 0)
                //{
                //    MenbersCount = _members.Count;
                //}
                return _members;

            }
            set
            {
                _members = value;
                //if (_members.Count > 0)
                //{
                //    MenbersCount = _members.Count;
                //}
                this.OnPropertyChanged();

            }
        }


        private int _membersCount;
        /// <summary>
        /// 群组成员个数
        /// </summary>
        public int MenbersCount
        {
            get { return _membersCount; }
            set
            {
                _membersCount = value;
                this.OnPropertyChanged();
            }
        }
        private GroupType _groupType;
        /// <summary>
        /// 群组
        /// </summary>
        public GroupType GroupType
        {
            get { return _groupType; }
            set { _groupType = value; }
        }

        private DateTime _createTime;
        /// <summary>
        /// 群创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get { return _createTime; }
            set { _createTime = value; this.OnPropertyChanged(); }
        }


        private string _groupName;
        /// <summary>
        /// 群名称
        /// </summary>
        public string GroupName
        {
            get { return _groupName; }
            set { _groupName = value; this.OnPropertyChanged(); }
        }

        private string _displayName = string.Empty;
        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; this.OnPropertyChanged(); }
        }


        private string _myNickNameInGroup;
        /// <summary>
        /// 我在群中显示的昵称
        /// </summary>
        public string MyNickNameInGroup
        {
            get { return _myNickNameInGroup; }
            set { _myNickNameInGroup = value; this.OnPropertyChanged(); }
        }


        private string _headImg;
        /// <summary>
        /// 好友头像或群头像
        /// </summary>
        public string HeadImg
        {
            get { return _headImg; }
            set { _headImg = value; this.OnPropertyChanged(); }
        }

        private string _headImgMD5;
        /// <summary>
        /// 头像MD5值
        /// </summary>
        public string HeadImgMD5
        {
            get { return _headImgMD5; }
            set { _headImgMD5 = value; this.OnPropertyChanged(); }
        }

        private string _groupRemark;
        /// <summary>
        /// 群备注说明
        /// </summary>
        public string GroupRemark
        {
            get { return _groupRemark; }
            set { _groupRemark = value; this.OnPropertyChanged(); }
        }

        private int _appendID;
        /// <summary>
        /// 附加ID
        /// </summary>
        public int AppendID
        {
            get { return _appendID; }
            set { _appendID = value; this.OnPropertyChanged(); }
        }

        private bool _isNotDisturb;
        /// <summary>
        /// 是否免打扰
        /// </summary>
        public bool IsNotDisturb
        {
            get { return _isNotDisturb; }
            set { _isNotDisturb = value; this.OnPropertyChanged(); }
        }

        private DateTime? _topMostTime;
        /// <summary>
        /// 置顶时间
        /// </summary>
        public DateTime? TopMostTime
        {
            get { return _topMostTime; }
            set { _topMostTime = value; this.OnPropertyChanged(); }
        }

        private bool _isTopMost;
        /// <summary>
        /// 是否置顶
        /// </summary>
        public bool IsTopMost
        {
            get { return _isTopMost; }
            set { _isTopMost = value; this.OnPropertyChanged(); }
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

        private string _qrCodePath;
        /// <summary>
        /// 二维码图片路径
        /// </summary>
        public string QrCodePath
        {
            get { return _qrCodePath; }
            set { _qrCodePath = value; this.OnPropertyChanged(); }
        }


        private int _joinAuthType;
        /// <summary>
        /// 入群验证方式: 1 管理员审批入群 2 自由入群 3 密码入群
        /// </summary>
        public int JoinAuthType
        {
            get { return _joinAuthType; }
            set { _joinAuthType = value; this.OnPropertyChanged(); }
        }

    }
}
