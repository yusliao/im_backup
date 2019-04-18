using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMModels
{
    public class GroupNotice:BaseModel
    {
        public GroupNotice()
        {

        }
        private string _noticeTitle;
        /// <summary>
        /// 公告标题
        /// </summary>
        public string NoticeTitle
        {
            get { return _noticeTitle; }
            set { _noticeTitle = value;this.OnPropertyChanged(); }
        }
        private string _noticeContent;
        /// <summary>
        /// 公告内容
        /// </summary>
        public string NoticeContent
        {
            get { return _noticeContent; }
            set { _noticeContent = value;this.OnPropertyChanged(); }
        }

        private bool _isCanOperate;
        /// <summary>
        /// 群公告是否可操作，用来绑定新增、删除按钮是否可用
        /// </summary>
        public bool IsCanOperate
        {
            get { return _isCanOperate; }
            set { _isCanOperate = value;this.OnPropertyChanged(); }
        }

        private bool _isToTop;
        /// <summary>
        /// 是否置顶
        /// </summary>
        public bool IsToTop
        {
            get { return _isToTop; }
            set { _isToTop = value;this.OnPropertyChanged(); }
        }

        private DateTime _noticeReleTime;
        public DateTime NoticeReleTime
        {
            get { return _noticeReleTime; }
            set
            {
                _noticeReleTime = value;
                this.OnPropertyChanged();
            }
        }

        private int _groupId;
        /// <summary>
        /// 群Id
        /// </summary>
        public int GroupId
        {
            get { return _groupId; }
            set { _groupId = value;this.OnPropertyChanged(); }
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

        private int _noticeId;
        /// <summary>
        /// 公告Id
        /// </summary>
        public int NoticeId
        {
            get { return _noticeId; }
            set { _noticeId = value;this.OnPropertyChanged(); }
        }

        private int _noticeType;
        /// <summary>
        /// 公告类型，0普通公告，1入群须知
        /// </summary>
        public int NoticeType
        {
            get { return _noticeType; }
            set { _noticeType = value;this.OnPropertyChanged(); }
        }
    }
}
