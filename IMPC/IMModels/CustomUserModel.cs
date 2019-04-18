using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMModels
{
    /// <summary>
    /// 客服用户模型
    /// </summary>
    public class CustomUserModel :BaseModel, ICustom
    {
        string _address;
        public string Address { get => _address; set { _address = value; this.OnPropertyChanged(); } }
        string _deviceName;
        public string DeviceName { get => _deviceName; set { _deviceName = value; this.OnPropertyChanged(); } }
        string _deviceType;
        public string DeviceType { get => _deviceType; set { _deviceType = value; this.OnPropertyChanged(); } }
        string _shopName;
        public string ShopName { get => _shopName; set { _shopName = value; this.OnPropertyChanged(); } }
        string _shopBackUrl;
        public string ShopBackUrl { get => _shopBackUrl; set { _shopBackUrl = value; this.OnPropertyChanged(); } }
        int _shopId;
        public int ShopId { get => _shopId; set { _shopId = value; this.OnPropertyChanged(); } }
        string _appType;
        public string AppType { get => _appType; set { _appType = value; this.OnPropertyChanged(); } }
        string _mobile;
        public string Mobile { get => _mobile; set { _mobile = value; this.OnPropertyChanged(); } }
    }
    public class CustomItem : BaseModel, IChat
    {

        string _displayName;
        public string DisplayName { get => _displayName; set { _displayName = value; this.OnPropertyChanged(); } }
        string _headImg;
        public string HeadImg { get => _headImg; set { _headImg = value; this.OnPropertyChanged(); } }
        string _headImgMD5;
        public string HeadImgMD5 { get => _headImgMD5; set { _headImgMD5 = value; this.OnPropertyChanged(); } }
        bool _isNotDisturb;
        public bool IsNotDisturb { get => _isNotDisturb; set { _isNotDisturb = value; this.OnPropertyChanged(); } }
        bool _isTopMost;
        public bool IsTopMost { get => _isTopMost; set { _isTopMost = value; this.OnPropertyChanged(); } }
        DateTime? _topMostTime;
        public DateTime? TopMostTime { get => _topMostTime; set { _topMostTime = value; this.OnPropertyChanged(); } }
        bool _isDefriend;
        public bool IsDefriend { get => _isDefriend; set { _isDefriend = value; this.OnPropertyChanged(); } }
    }
    public class OnlineStatusEntity : BaseModel
    {
        int _servicerId;
        public int Servicerid { get => _servicerId; set { _servicerId = value; this.OnPropertyChanged(); } }
        string _nickName;
        public string Nickname { get => _nickName; set { _nickName = value; this.OnPropertyChanged(); } }
        
        int _userId;
        public int UserId { get => _userId; set { _userId = value; this.OnPropertyChanged(); } }
    }
   
    public class HistoryChatItem : BaseModel, IChat
    {
        string _displayName;
        public string DisplayName { get => _displayName; set { _displayName = value; this.OnPropertyChanged(); } }
        string _headImg;
        public string HeadImg { get => _headImg; set { _headImg = value; this.OnPropertyChanged(); } }
        string _headImgMD5;
        public string HeadImgMD5 { get => _headImgMD5; set { _headImgMD5 = value; this.OnPropertyChanged(); } }
        bool _isNotDisturb;
        public bool IsNotDisturb { get => _isNotDisturb; set { _isNotDisturb = value; this.OnPropertyChanged(); } }
        bool _isTopMost;
        public bool IsTopMost { get => _isTopMost; set { _isTopMost = value; this.OnPropertyChanged(); } }
        DateTime? _topMostTime;
        public DateTime? TopMostTime { get => _topMostTime; set { _topMostTime = value; this.OnPropertyChanged(); } }
        bool _isDefriend;
        public bool IsDefriend { get => _isDefriend; set { _isDefriend = value; this.OnPropertyChanged(); } }
        public string imOpendId { get; set; }//用户imUserId 
        public string mobile { get; set; }//用户mobile 
        public string servicersName { get; set; }//接入过的客服名称集  
        public int sessionType { get; set; }//当前会话类型：0-当前未有会话，1-自己的会话，2-其他客服的会话 ,
        public string sessionId { get; set; }//会话ID 

       
        string _endDate;
        public string EndDate { get => _endDate; set { _endDate = value; this.OnPropertyChanged(); } }


    }


}
