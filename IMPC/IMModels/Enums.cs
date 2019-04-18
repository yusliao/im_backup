using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMModels
{
    /// <summary>
    /// 消息状态
    /// </summary>
    public enum MessageStates
    {
        /// <summary>
        /// 未知，可忽略
        /// </summary>
        None,
        /// <summary>
        /// 成功
        /// </summary>
        Success,
        /// <summary>
        /// 失败
        /// </summary>
        Fail,
        /// <summary>
        /// 警告
        /// </summary>
        Warn,
        /// <summary>
        /// 发送中
        /// </summary>
        sending,
        /// <summary>
        /// 正在加载，上传或下载
        /// </summary>
        Loading,
        /// <summary>
        /// 文件过期
        /// </summary>
        ExpiredFile
    }

    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageType
    {
        txt = 0,
        img,
        file,
        audio,
        video,
        smallvideo,
        retract,//撤回
        notification,//通知 
        bigtxt,

        invitejoingroup,//邀请加入群

        exitgroup,//退群
        setmemberpower,//设置成员权限
        dismissgroup,

        redenvelopessendout,
        redenvelopesreceive,

        onlinefile,
        eval,
        goods,
        order,
        custom,
        addgroupnotice,
        deletegroupnotice,
        usercard,
        foreigndyn,
        addfriendaccepted,
        all = 999
    }

    public enum FileStates
    {
        /// <summary>
        /// 不处理
        /// 默认预留
        /// </summary>
        None,
        /// <summary>
        /// 在线发送
        /// </summary>
        SendOnline,
        /// <summary>
        /// 离线发送
        /// </summary>
        SendOffline,
        /// <summary>
        /// 等待接收
        /// </summary>
        WaitForReceieve,
        /// <summary>
        /// 文件发送中
        /// </summary>
        FileSending,
        /// <summary>
        /// 正在接收
        /// </summary>
        Receiving,
        /// <summary>
        /// 发送/接收 完成
        /// </summary>
        Completed,
        /// <summary>
        /// 发送/接收失败
        /// </summary>
        Fail,
    }
}
