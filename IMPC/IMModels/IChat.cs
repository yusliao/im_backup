using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMModels
{
    /// <summary>
    /// 聊天者接口
    /// 可以是人也可以是群组
    /// </summary>
    public interface IChat
    {
        int ID { get; set; }
        string DisplayName { get; set; }
        string HeadImg { get; set; }
        string HeadImgMD5 { get; set; }

        bool IsNotDisturb { get; set; }

        bool IsTopMost { get; set; }
         
        DateTime? TopMostTime { get; set; }

        bool IsDefriend { get; set; }

       
    }
    /// <summary>
    /// 客
    /// </summary>
    public interface ICustom
    {
        /*
        * 客专有聊天属性
        */
        /// <summary>
        /// 移动设备地址信息（上网IP地址）
        /// </summary>
        string Address { get; set; }
        /// <summary>
        /// 移动设备系统名称
        /// </summary>
        string DeviceName { get; set; }
        /// <summary>
        /// 移动设备系统型号
        /// </summary>
        string DeviceType { get; set; }
        /// <summary>
        /// 显示名
        /// </summary>
        string ShopName { get; set; }
        /// <summary>
        /// 店铺URL
        /// </summary>
        string ShopBackUrl { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        int ShopId { get; set; }
    }
    
   
}
