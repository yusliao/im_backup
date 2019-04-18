using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMModels
{
    public class GroupNoticeModel:BaseModel
    {
        private string _noticeTitle;
        /// <summary>
        /// 消息头
        /// </summary>
        public string NoticeTitle
        {
            get { return _noticeTitle; }
            set { _noticeTitle = value; this.OnPropertyChanged(); }
        }
        private int _noticeId;
        /// <summary>
        /// 消息Id
        /// </summary>
        public int NoticeId
        {
            get { return _noticeId; }
            set { _noticeId = value; this.OnPropertyChanged(); }
        }

        private int _groupMId;
        /// <summary>
        /// groupdId
        /// </summary>
        public int GroupMId
        {
            get { return _groupMId; }
            set { _groupMId = value; this.OnPropertyChanged(); }
        }


        private bool _isHasDelete;
        /// <summary>
        /// 是否被删除了
        /// </summary>
        public bool IsHasDelete
        {
            get { return _isHasDelete; }
            set { _isHasDelete = value; this.OnPropertyChanged(); }
        }

        private string _groupNoticeType;
        /// <summary>
        /// 公告类型，0，普通群公告，1，入群须知
        /// </summary>
        public string GroupNoticeType
        {
            get { return _groupNoticeType; }
            set { _groupNoticeType = value;this.OnPropertyChanged(); }
        }

        private string _groupNoticeContent;
        /// <summary>
        /// 公告内容
        /// </summary>
        public string GroupNoticeContent
        {
            get { return _groupNoticeContent; }
            set { _groupNoticeContent = value;this.OnPropertyChanged(); }
        }
    }
}
