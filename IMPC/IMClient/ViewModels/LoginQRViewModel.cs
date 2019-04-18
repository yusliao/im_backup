using IMModels;
using SDKClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util;

namespace IMClient.ViewModels
{
    public class LoginQRViewModel : ViewModel
    {

        public event Action<IMModels.UserModel> OnLoginSuccess;
        /// <summary>
        /// 是否正在登录
        /// </summary>
        bool _isLogin = false;

        public LoginQRViewModel(IView view) : base(view)
        {
            //GetQRimg(Guid.NewGuid().ToString());
            //SDKClient.SDKClient.Instance.ConnState += (s, isOnline) =>
            //{
            //    
            //};

           
        }
        public VMCommand ReturnQRLoginCommand
        {
            get { return new VMCommand(ReturnQRLogin); }
        }

        public void ReturnQRLogin(object para)
        {
            IsWaitingLogin = false;
            this.ErrorInfo = string.Empty;
            IsReturnQRLogin = false;
            SDKClient.SDKClient.Instance.GetLoginQRCode();
            //HasError = true;
        }
        public void TryConnect()
        { 
            this.IsWaitingLogin = false;
            SDKClient.SDKClient.Instance.NewDataRecv -= Instance_NewDataRecv;
            SDKClient.SDKClient.Instance.ConnState -= Instance_ConnState;
           

            //var result = Util.Helpers.Async.Run(async () => await SDKClient.SDKClient.Instance.StartQRLoginAsync());

            this.IsConnecting = true;
           Connect();
            this.IsActive = true; 
        }

        private async void Connect()
        {
            SDKClient.SDKClient.Instance.NewDataRecv += Instance_NewDataRecv;
            SDKClient.SDKClient.Instance.ConnState += Instance_ConnState;
            bool result= await SDKClient.SDKClient.Instance.StartQRLoginAsync();
            if (result)
            {
                this.ErrorInfo = string.Empty;
               
            }
            else
            {
                this.ErrorInfo = "当前网络无法使用\n请检查后重新尝试";
            }

            this.IsConnecting = false;
        }

        private void Instance_ConnState(object sender, bool e)
        {
            if (_isLogin) //若正在登录，关闭了二维码服务连接会引起失败
            {

            }
            else if(!e)
            {
                this.IsWaitingLogin = false;
                this.ErrorInfo = e ? string.Empty : "当前网络无法使用\n请检查后重新尝试"; 
            }
        }

        private void Instance_NewDataRecv(object sender, SDKClient.Model.PackageInfo e)
        { 
            this.ErrorInfo = string.Empty;
            switch (e.apiId)
            {
              
                case SDKClient.Protocol.ProtocolBase.GetLoginQRCodeCode: 
                    if (e is GetLoginQRCodePackage qrCodePackage)
                    {
                        GetQRimg(qrCodePackage.data.qrcode);
                    } 
                    break;
                case SDKClient.Protocol.ProtocolBase.GetClientIDCode: //获取Guid
                    if(e is GetClientIDPackage clinetID)
                    {
                        GetQRimg(clinetID.data.clientId);
                    }
                    break;
                case SDKClient.Protocol.ProtocolBase.QRScanCode: // 用户基本信息
                    if (e is QRScanPackage qrScan)
                    {
                        UserModel user = new UserModel()
                        {
                            ID = qrScan.data.userId.ToInt(),
                            PhoneNumber = qrScan.data.mobile,
                            Name = qrScan.data.username,
                            HeadImg = qrScan.data.photo,
                        };

                        user.HeadImg = IMClient.Helper.ImageHelper.GetAccountFace(user.HeadImg, (s) =>App.Current.Dispatcher.Invoke(()=> user.HeadImg = s));

                        CurrentUser = user;
                        if (CanLogin(false))
                        {
                            this.IsWaitingLogin = true;
                        }
                        IsReturnQRLogin = true;
                    }
                    break;
                case SDKClient.Protocol.ProtocolBase.QRConfirmCode: //手机端确认登录，
                    if (CanLogin(true))
                    {
                        _isLogin = true;
                        Util.Helpers.Async.Run(async () => await SDKClient.SDKClient.Instance.CreateConn());
                    } 
                    break;
                case SDKClient.Protocol.ProtocolBase.QRCancelCode: //手机端不登录，取消操作
                    //this.IsActive = false;
                    this.IsWaitingLogin = false;
                    this.CurrentUser = null;
                    IsReturnQRLogin = false;
                    SDKClient.SDKClient.Instance.GetLoginQRCode();
                    break;
                case SDKClient.Protocol.ProtocolBase.QRExpiredCode: //二维码过期处理
                 
                    if (e is QRExpiredPackage expired)
                    {
                        GetQRimg(expired.data.qrcode);
                        IsReturnQRLogin = false;
                    }
                    break;
                case SDKClient.Protocol.ProtocolBase.ForceExitCode: //手机端强制PC端退出
                    SDKClient.SDKClient.Instance.SendLogout(SDKClient.SDKProperty.LogoutModel.Logout_kickout);
                    App.ReStart(); 
                    break;

                case SDKClient.Protocol.ProtocolBase.authCode: 
                    ReceivedAuthResponse(e);
                    break;
                case SDKClient.Protocol.ProtocolBase.loginCode: 
                    ReceivedLoginResponse(e);
                    break;
            }
        }


        private bool CanLogin(bool tryLogin)
        {
            bool canLogin = true;
            System.Threading.Mutex temp;
#if RELEASE
             
            if (System.Threading.Mutex.TryOpenExisting(this.CurrentUser.PhoneNumber, out temp))
            {
                this.ErrorInfo = "此账号已登录";
                return canLogin=false;
            }
            
#elif CHECK
            if (System.Threading.Mutex.TryOpenExisting(this.CurrentUser.PhoneNumber+"cs", out temp))
            {
                this.ErrorInfo = "此账号已登录";
               
                return canLogin=false;
            }
#elif DEBUG
            if (System.Threading.Mutex.TryOpenExisting(this.CurrentUser.PhoneNumber+"kf", out temp))
            {
                this.ErrorInfo = "此账号已登录";
               
                return canLogin=false;
            }
#elif HUIDU
            if (System.Threading.Mutex.TryOpenExisting(this.CurrentUser.PhoneNumber + "hd", out temp))
            {
                this.ErrorInfo = "此账号已登录";
                return canLogin = false;
            }
            //else if(tryLogin)
            //{
            //    App.MUTEX = new System.Threading.Mutex(true, this.CurrentUser.PhoneNumber + "kf");
            //}
#endif

            return canLogin;
        }
         

        private void ReceivedLoginResponse(SDKClient.Model.PackageInfo packageInfo)
        {
            if (packageInfo.code == 0)
            {
                //this.CurrentLogin.User.ID = (packageInfo as SDKClient.Model.AuthPackage).data.userId;
                //连接成功
            }
            else
            { 
                if (IsActive)
                {
                    IsReturnQRLogin = true;
                    this.ErrorInfo = packageInfo.error;
                }
            }
        }

        private void ReceivedAuthResponse(SDKClient.Model.PackageInfo packageInfo)
        {
            //this.IsWaitForLogin = true;
            if (packageInfo.code == 0 && this.CurrentUser!=null&& _isLogin)
            {
                //this.SelectedLogin.ID = this.SelectedLogin.User.ID = (packageInfo as SDKClient.Model.AuthPackage).data.userId;

                this.CurrentUser.ID = (packageInfo as SDKClient.Model.AuthPackage).data.userId;

                //连接成功
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    //IsLoginSuccess = true;

                    //this.IsEnabledWindow = false;

                    SDKClient.SDKClient.Instance.NewDataRecv -= Instance_NewDataRecv;
                    SDKClient.SDKClient.Instance.ConnState -= Instance_ConnState;

                    //if (_timer != null)
                    //{
                    //    _timer.Stop();
                    //    _timer.IsEnabled = false;
                    //    _timer = null;
                    //}
                    SDKClient.SDKClient.Instance.GetBadWordEditTime();
                    this.OnLoginSuccess?.Invoke(this.CurrentUser);
                });
            }
            else
            {
                if (IsActive)
                {
                    this.ErrorInfo = packageInfo.error;
                } 
                ////连接失败
                //LoginFailed(packageInfo.error);
            }
        }
         
        private bool _isActive;
        /// <summary>
        /// 是否当前界面活跃的（即是否显示二维码登录界面）
        /// </summary>
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value; this.OnPropertyChanged();

                if (!value)
                {
                    SDKClient.SDKClient.Instance.NewDataRecv -= Instance_NewDataRecv;
                    SDKClient.SDKClient.Instance.ConnState -= Instance_ConnState;
                }
            }
        }

        private bool _isConnecting;
        /// <summary>
        /// 是否正在连接中
        /// </summary>
        public bool IsConnecting
        {
            get { return _isConnecting; }
            set { _isConnecting = value; this.OnPropertyChanged(); }
        }


        private bool _isWaitingLogin;
        /// <summary>
        /// 是否正在等待登录
        /// </summary>
        public bool IsWaitingLogin
        {
            get { return _isWaitingLogin; }
            set { _isWaitingLogin = value; this.OnPropertyChanged(); }
        }

        private bool _isReturnQRLogin;
        public bool IsReturnQRLogin
        {
            get { return _isReturnQRLogin; }
            set {
                _isReturnQRLogin = value;
                this.OnPropertyChanged();
            }
        }
        public string ClientGuid { get; private set; }

        private string _qrImgPath = "/IMAssets;component/Images/qrcode.png";
        /// <summary>
        /// 二维码图片路径
        /// </summary>
        public string QrImgPath
        {
            get { return _qrImgPath; }
            set { _qrImgPath = value; this.OnPropertyChanged(); }
        }


        private UserModel _currentUser;
        /// <summary>
        /// 当前用户
        /// </summary>
        public UserModel CurrentUser
        {
            get { return _currentUser; }
            set { _currentUser = value; this.OnPropertyChanged(); }
        }


        private string _errorInfo;
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorInfo
        {
            get { return _errorInfo; }
            private set
            {
                _errorInfo = value;
                this.HasError = !string.IsNullOrEmpty(_errorInfo);
                this.OnPropertyChanged(); 
            }
        }

        private bool _hasError;
        /// <summary>
        /// 是否有错误
        /// </summary>
        public bool HasError
        {
            get { return _hasError; }
            set { _hasError = value; this.OnPropertyChanged(); }
        }



        private void GetQRimg(string id)
        {
            ClientGuid = id;
             
            Task.Run(() =>
            {
                this.IsActive = true;
                this.IsWaitingLogin = false;


                string imgPath= SDKClient.SDKClient.GetLoginQrCodeImg(ClientGuid);

                if (System.IO.File.Exists(imgPath))
                {
                    QrImgPath = imgPath;

                    //App.Current.Dispatcher.Invoke(new Action(() =>
                    //{
                    //    this.OnLoginSuccess(new IMModels.UserModel() { ID=6606, Name ="阿咕" });
                    //}));
                }
                else
                {

                }

                //System.Threading.Thread.CurrentThread.Join(10 * 1000);
                //this.IsActive = false;
            });
        }
    }
}
