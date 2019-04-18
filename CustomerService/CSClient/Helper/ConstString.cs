using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSClient.Helper
{
    public class ConstString
    {
        /// <summary>
        /// @所有人对应的Id
        /// </summary>
        public const int AtAllId = -1;

        public const string Add = "添加";
        public const string Delete = "删除";

        public const string OwnerGroupTitle = "我创建的群";
        public const string AdminGroupTitle = "我管理的群";
        public const string JoinGroupTitle = "我加入的群";

        public const string MsgSticky = "置顶";
        public const string CancelMsgSticky = "取消置顶";
        public const string MsgDoNotDisturb = "消息免打扰";
        public const string CancelMsgDoNotDisturb = "开启新消息提醒";

        public const string FileUploadingTip = "您有文件正在传输是否关闭？";

        public const string FollowingIsNewMessage = "以下为新消息";

        public const string NetworkOffLineRemaind = "您已经处于离线状态，无法发送消息，请上线后再次尝试。";
        public const string EmptyFileRemaind = "为空文件，无法发送，请您重新选择。";
    }
}
