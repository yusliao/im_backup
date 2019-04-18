using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMModels
{
    /// <summary>
    /// 用户模型
    /// </summary>
    public class UserModel : BaseModel, IChat
    {
        private string _name;
        /// <summary>
        /// 名称，即用户方自己设置的昵称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                foreach (var gm in LinkGorups.Values)
                {
                    if (gm.DisplayName == _name)
                    {
                        gm.DisplayName = value;
                    }
                }
                if (string.IsNullOrEmpty(this.DisplayName))
                {
                    this.DisplayName = value;
                }
                _name = value; this.OnPropertyChanged();
            }
        }

        private string _displayName;
        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                _displayName = value; this.OnPropertyChanged();
                foreach (var gm in LinkGorups.Values)
                {
                    if (string.IsNullOrEmpty(gm.NickNameInGroup))
                    {
                        gm.DisplayName = value;
                    }
                }
            }
        }

        private int _haveModifiedKfid;
        /// <summary>
        /// 可访号是否被修改过(0未修改1已修改过)
        /// </summary>
        public int HaveModifiedKfid
        {
            get { return _haveModifiedKfid; }
            set
            {
                this._haveModifiedKfid = value;
                this.OnPropertyChanged();
            }
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

        private string _nickName;
        /// <summary>
        /// 备注昵称
        /// </summary>
        public string NickName
        {
            get { return _nickName; }
            set
            {
                _nickName = value; this.OnPropertyChanged();
                if (string.IsNullOrEmpty(value))
                {
                    foreach (var gm in LinkGorups.Values)
                    {
                        if (string.IsNullOrEmpty(gm.NickNameInGroup))
                        {
                            gm.DisplayName = this.Name;
                        }
                        else
                        {
                            gm.DisplayName = gm.NickNameInGroup;
                        }
                    }
                }
                else
                {
                    foreach (var gm in LinkGorups.Values)
                    {
                        gm.DisplayName = value;
                    }
                }
            }
        }

        private string _phoneNumber;
        /// <summary>
        /// 电话号码
        /// </summary>
        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set
            {
                if (_phoneNumber != value)
                {
                    _phoneNumber = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _sex;
        /// <summary>
        /// 性别，0-女，1-男
        /// </summary>
        public string Sex
        {
            get { return _sex; }
            set { _sex = value; this.OnPropertyChanged(); }
        }

        private string _headImg;
        /// <summary>
        /// 头像
        /// </summary>
        public string HeadImg
        {
            get { return _headImg; }
            set
            {
                _headImg = value; this.OnPropertyChanged();
                //foreach(var gm in LinkGorups.Values)
                //{
                //    gm.HeadImg = value;
                //}
            }
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

        private string _area;
        /// <summary>
        /// 地区
        /// </summary>
        public string Area
        {
            get { return _area; }
            set { _area = value; this.OnPropertyChanged(); }
        }

        private int _linkDelType = 3;
        /// <summary>
        /// 好友关系状态
        /// -1本人，0正常好友，1我删除对方，2对方删除我，3双方都已删除，
        /// </summary>
        public int LinkDelType
        {
            get { return _linkDelType; }
            set { _linkDelType = value; this.OnPropertyChanged(); }
        }

        private int _linkType;
        /// <summary>
        /// 关系类型
        /// 0正常，1我拉黑对方，2对方拉黑我，3双方都已拉黑
        /// </summary>
        public int LinkType
        {
            get { return _linkType; }
            set { _linkType = value; this.OnPropertyChanged(); }
        }

        #region relationShip
        /*
        /// <summary>
        /// 设置与当前登录人的关系
        /// -1 自己，0好友；1陌生人
        /// </summary>
        /// <param name="index"></param>
        public void SetRelationShip(int index)
        {
            this.IsMySelf = this.IsFriend = this.IsStranger = false;
            switch (index)
            {
                case -1:
                    this.IsMySelf = true;
                    break;
                case 0:
                    this.IsFriend = true;
                    break;
                default:
                case 1:
                    this.IsStranger = true;
                    break;
            }
        }

        private bool _isMySelf;
        /// <summary>
        /// 是否我自己
        /// </summary>
        public bool IsMySelf
        {
            get { return _isMySelf; }
            private set { _isMySelf = value; this.OnPropertyChanged(); }
        }

        private bool _isFriend;
        /// <summary>
        /// 是否我的好友
        /// </summary>
        public bool IsFriend
        {
            get { return _isFriend; }
            private set { _isFriend = value; this.OnPropertyChanged(); }
        }

        private bool _isStranger=true;
        /// <summary>
        /// 是否陌生人
        /// </summary>
        public bool IsStranger
        {
            get { return _isStranger; }
            private set { _isStranger = value; this.OnPropertyChanged(); }
        }

        */
        #endregion

        private int _attentionID;
        /// <summary>
        /// 关注ID
        /// </summary>
        public int AttentionID
        {
            get { return _attentionID; }
            set { _attentionID = value; this.OnPropertyChanged(); }
        }

        private bool _isAttention;
        /// <summary>
        /// 是否关注
        /// </summary>
        public bool IsAttention
        {
            get { return _isAttention; }
            set { _isAttention = value; this.OnPropertyChanged(); }
        }

        private bool _isApplyFriend;
        /// <summary>
        /// 是否已经申请好友
        /// </summary>
        public bool IsApplyFriend
        {
            get { return _isApplyFriend; }
            set { _isApplyFriend = value; this.OnPropertyChanged(); }
        }

        private DateTime _appendTime;
        /// <summary>
        /// 附加时间
        /// </summary>
        public DateTime AppendTime
        {
            get { return _appendTime; }
            set { _appendTime = value; this.OnPropertyChanged(); }
        }

        private bool _isSelected;
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; this.OnPropertyChanged(); }
        }

        private bool _isLock;
        /// <summary>
        /// 还是否锁定
        /// </summary>
        public bool IsLock
        {
            get { return _isLock; }
            set { _isLock = value; ; this.OnPropertyChanged(); }
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


        private int _friendSource;
        /// <summary>
        /// 好友来源 1：群 2：手机号搜索 3：扫一扫 4：好友推荐 5：开店邀请 6:朋友验证 7：可访号搜索
        /// </summary>
        public int FriendSource
        {
            get { return _friendSource; }
            set
            {
                _friendSource = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 来源群ID
        /// </summary>
        private string _sourceGroup;
        /// <summary>
        /// 来源群ID
        /// </summary>
        public string SourceGroup
        {
            get { return _sourceGroup; }
            set
            {
                _sourceGroup = value;
                this.OnPropertyChanged();
            }
        }

        private string _sourceGroupName;
        /// <summary>
        /// 群名称
        /// </summary>
        public string SourceGroupName
        {
            get { return _sourceGroupName; }
            set
            {
                _sourceGroupName = value;
                this.OnPropertyChanged();
            }
        }

        #region About Group

        private Dictionary<int, GroupMember> LinkGorups = new Dictionary<int, GroupMember>();
        private Dictionary<int, GroupMember> LinkVirtual = new Dictionary<int, GroupMember>();


        /// <summary>
        /// 获取在群中的成员
        /// </summary>
        /// <param name="group">指定的群</param>
        /// <param name="isVirtual">是否虚拟，是则实际不添加到群中只是虚构一个对象，否则可能不存在群中的时候返回为null</param>
        /// <returns></returns>
        public GroupMember GetInGroupMember(GroupModel group, bool isVirtual = true)
        {
            GroupMember member = null;
            if (LinkGorups.ContainsKey(group.ID))
            {
                member = LinkGorups[group.ID];
                member.HeadImg = LinkGorups[group.ID].TargetUser.HeadImg;
                member.HeadImgMD5 = LinkGorups[group.ID].TargetUser.HeadImgMD5;
            }
            else if (isVirtual)
            {

                if (LinkVirtual.ContainsKey(group.ID))
                {
                    member = LinkVirtual[group.ID];
                }
                else
                {
                    lock (LinkVirtual)
                    {
                        if (LinkVirtual.ContainsKey(group.ID))
                        {
                            member = LinkVirtual[group.ID];
                        }
                        else
                        {
                            member = new GroupMember(group.ID, this);
                            LinkVirtual.Add(group.ID, member);
                        }

                    }

                }
            }
            return member;
        }

        /// <summary>
        /// 添加成员到指定组中
        /// </summary> 
        public GroupMember AddToGroup(GroupModel group)
        {
            GroupMember member = null;
            if (LinkGorups.ContainsKey(group.ID))
            {
                member = LinkGorups[group.ID];
                var tempMembers = group.Members.ToList();
                if (tempMembers.Any(info => info.ID == this.ID))
                {

                }
                else
                {
                    group.Members.Add(member);
                }
            }
            else
            {
                if (LinkVirtual.ContainsKey(group.ID))
                {
                    member = LinkVirtual[group.ID];
                    LinkVirtual.Remove(group.ID);
                }
                else
                {
                    member = new GroupMember(group.ID, this);
                }
                lock (this)
                {

                    if (LinkGorups.ContainsKey(group.ID))
                    {
                        member = LinkGorups[group.ID];
                        var tempMembers = group.Members.ToList();
                        if (tempMembers.Any(info => info.ID == this.ID))
                        {

                        }
                        else
                        {
                            group.Members.Add(member);
                        }
                    }
                    else
                    {
                        if (LinkVirtual.ContainsKey(group.ID))
                        {
                            member = LinkVirtual[group.ID];
                            LinkVirtual.Remove(group.ID);
                        }
                        else
                        {
                            member = new GroupMember(group.ID, this);
                        }
                        LinkGorups.Add(group.ID, member);
                        group.Members.Add(member);
                    }

                }

            }

            member.HeadImg = this.HeadImg;
            return member;
        }

        /// <summary>
        /// 从指定组中移除成员
        /// </summary> 
        public GroupMember RemoveFromGroup(GroupModel group)
        {
            GroupMember member = null;
            if (LinkGorups.ContainsKey(group.ID))
            {
                member = LinkGorups[group.ID];
                member.IsManager = false;
                LinkGorups.Remove(group.ID);
                group.Members.Remove(member);
            }
            return member;
        }

        #endregion

    }

}
