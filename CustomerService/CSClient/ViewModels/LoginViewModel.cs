using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using IMModels;
using CSClient.Helper;

namespace CSClient.ViewModels
{
    public class LoginViewModel : ViewModel
    {
        public event Action<LoginUser> OnLoginSuccess;

        public LoginViewModel(IView view) : base(view)
        {
            LoadHistoryUsers();

            SDKClient.SDKClient.Instance.NewDataRecv += Instance_NewDataRecv;
        }



        private void Instance_NewDataRecv(object sender, SDKClient.Model.PackageInfo e)
        {
            switch (e.apiId)
            {
                case SDKClient.Protocol.ProtocolBase.authCode:
                    ReceivedAuthResponse(e);
                    break;
                case SDKClient.Protocol.ProtocolBase.loginCode:
                    ReceivedLoginResponse(e);
                    break;
                default:
                    break;

            }
            //this.LoginButtonEnabled = true; 
        }


        private void ReceivedLoginResponse(SDKClient.Model.PackageInfo packageInfo)
        {            
            if (packageInfo.code == 0)
            {
                
            }
            else
            {
                //重新发送连接业务请求
                this.ErrorInfo = packageInfo.error;
                this.IsWaitForLogin = true;
                App.MUTEX?.Close();
                App.MUTEX = null;
            }
        }

        private void ReceivedAuthResponse(SDKClient.Model.PackageInfo packageInfo)
        {
            this.IsWaitForLogin = true;
            if (packageInfo.code == 0)
            {
                this.SelectedLogin.ID = this.SelectedLogin.User.ID = (packageInfo as SDKClient.Model.AuthPackage).data.userId;

                //连接成功
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    SDKClient.SDKClient.Instance.NewDataRecv -= Instance_NewDataRecv;

                    this.OnLoginSuccess?.Invoke(this.SelectedLogin);
                });
            }
            else
            {
                this.ErrorInfo = packageInfo.error;
                this.IsWaitForLogin = true;
                App.MUTEX?.Close();
                App.MUTEX = null;
            }
        }
        #region  Commands

        private VMCommand _loginCommand;
        /// <summary>
        /// 登录命令
        /// </summary>
        public VMCommand LoginCommand
        {
            get
            {
                if (_loginCommand == null)
                    _loginCommand = new VMCommand(Login);
                return _loginCommand;
            }
        }


        private VMCommand _removeCommand;
        /// <summary>
        /// 重历史列表中移除命令
        /// </summary>
        public VMCommand RemoveCommand
        {
            get
            {
                if (_removeCommand == null)
                    _removeCommand = new VMCommand(Remove, new Func<object, bool>(o => o != null));
                return _removeCommand;
            }
        }


        private VMCommand _loginAccountInputCommand;
        /// <summary>
        /// 输入登录账户
        /// </summary>
        public VMCommand LoginAccountInputCommand
        {
            get
            {
                if (_loginAccountInputCommand == null)
                    _loginAccountInputCommand = new VMCommand(LoginAccountChanged, new Func<object, bool>(o => o != null));
                return _loginAccountInputCommand;
            }
        }

        #endregion
        #region Methods

        /// <summary>
        /// 加载历史登录用户
        /// </summary>
        private async void LoadHistoryUsers()
        {

            // var task = Util.Helpers.Async.Run(async () => await SDKClient.SDKClient.Instance.GetAccountListDESC());
            // var task = await SDKClient.SDKClient.Instance.GetAccountListDESC();
            await SDKClient.SDKClient.Instance.GetAccountListDESC().ContinueWith(t =>
            {
                
                foreach (var item in t.Result)
                {
                    UserModel user = new UserModel();
                    user.PhoneNumber = item.loginId;
                    user.HeadImg = CSClient.Helper.ImageHelper.GetAccountFace(item.headPic, (s) => user.HeadImg = s);
                    user.HeadImgMD5 = item.headPic;

                    LoginUser login = new LoginUser(user);

                    if (item.loginModel != SDKClient.Model.LoginMode.None)
                    {
                        login.IsSavePassword = !string.IsNullOrEmpty(item.password) && !string.IsNullOrEmpty(item.loginId);
                        login.Password = item.password;
                    }

                    this.HistoryUsers.Add(login);
                }
                this.SelectedLogin = this.HistoryUsers.FirstOrDefault();
            }, View.UItaskScheduler);
           


        }


        private LoginUser _newUser = new LoginUser(new UserModel() { HeadImg = ImagePathHelper.DefaultServiceHead });

        private void LoginAccountChanged(object para)
        {
            string account = string.Format("{0}", para);
            ErrorInfo = string.Empty;
            if (this.SelectedLogin == null || this.SelectedLogin.User.PhoneNumber != account)
            {
                var target = this.HistoryUsers.FirstOrDefault(info => info.User.PhoneNumber == account);

                if (target == null)
                {
                    _newUser.User.HeadImg = ImagePathHelper.DefaultServiceHead;
                    _newUser.User.PhoneNumber = account;
                    _newUser.IsSavePassword = false;
                    _newUser.Password = string.Empty;
                    target = _newUser;
                }
                //else
                //{
                //    _newUser.User.HeadImg = target.User.HeadImg;
                //    _newUser.User.PhoneNumber = account;
                //    _newUser.IsSavePassword = target.IsSavePassword;
                //    _newUser.Password = target.Password; 
                //}

                this.SelectedLogin = target;
            }
        }
        private void Login(object obj)
        {
            this.ErrorInfo = string.Empty;
            if (string.IsNullOrEmpty(this.SelectedLogin.User.PhoneNumber))
            {
                this.ErrorInfo = "账号不能为空";
                return;
            }

            if (string.IsNullOrEmpty(this.SelectedLogin.Password))
            {
                this.ErrorInfo = "密码不能为空";
                return;
            }

            //var runs = System.Diagnostics.Process.GetProcessesByName("IMUI");
            //foreach (var run in runs)
            //{

            //    if (run.MainWindowTitle.Contains(this.SelectedLogin.User.PhoneNumber))
            //    {
            //        //System.Windows.MessageBox.Show("已经有一个账号登录");
            //        //return;
            //        this.ErrorInfo = "此账号已登录";
            //        return;
            //    }
            //}

            //if (!SDKClient.SDKClient.Instance.IsConnected)
            //{
            //    this.ErrorInfo = "网络未连接";
            //    return;
            //}

            
            System.Threading.Mutex temp;
#if RELEASE

         
            if (System.Threading.Mutex.TryOpenExisting(this.SelectedLogin.User.PhoneNumber, out temp))
            { 
                this.ErrorInfo = "此账号已登录"; 
                return;
            }
            else
            {
                App.MUTEX = new System.Threading.Mutex(true, this.SelectedLogin.User.PhoneNumber); 
            }

            //if (System.Threading.Mutex.TryOpenExisting(this.SelectedLogin.User.PhoneNumber, out temp))
            //{
            //    this.ErrorInfo = "此账号已登录";
            //    return;
            //}
            //else
            //{
            //    MUTEX = new System.Threading.Mutex(true, this.SelectedLogin.User.PhoneNumber);
            //}
#else
            if (System.Threading.Mutex.TryOpenExisting(this.SelectedLogin.User.PhoneNumber + "cs", out temp))
            {
                this.ErrorInfo = "此账号已登录";
                return;
            }
            else
            {
                App.MUTEX = new System.Threading.Mutex(true, this.SelectedLogin.User.PhoneNumber + "cs");
            }
#endif

            this.IsWaitForLogin = false;

            Task.Run(() =>
            {
                SDKClient.Model.LoginMode mode = this.SelectedLogin.IsSavePassword ? SDKClient.Model.LoginMode.Save : SDKClient.Model.LoginMode.None;

                //this.LoginButtonEnabled = false;
              
                var t = Util.Helpers.Async.Run(async () => await SDKClient.SDKClient.Instance.StartAsync(this.SelectedLogin.User.PhoneNumber, this.SelectedLogin.Password, mode, SDKClient.SDKProperty.userType.customserver));
                if (t)
                {
                    return;
                }
                this.ErrorInfo = "无网络连接";
                this.IsWaitForLogin = true;
                App.MUTEX?.Close();
                App.MUTEX = null;
            });
        }

        private void Remove(object para)
        {
            if (para is LoginUser loginUser)
            {
                this.HistoryUsers.Remove(loginUser);
                SDKClient.SDKClient.Instance.DeleteHistoryAccount(loginUser.User.PhoneNumber);
                if (loginUser.ID == this.SelectedLogin.ID)
                {
                    this.SelectedLogin = this.HistoryUsers.FirstOrDefault();
                }
                this.IsOpenList = true;
            }
        }

        #endregion



        #region Propertys

        private ObservableCollection<LoginUser> _historyUsers = new ObservableCollection<LoginUser>();
        /// <summary>
        /// 历史登录用户列表
        /// </summary>
        public ObservableCollection<LoginUser> HistoryUsers
        {
            get { return _historyUsers; }
            private set { _historyUsers = value; this.OnPropertyChanged(); }
        }

        private LoginUser _selectedLogin;
        /// <summary>
        /// 选中的登录用户
        /// </summary>
        public LoginUser SelectedLogin
        {
            get { return _selectedLogin; }
            set
            {
                if (value != null)
                {
                    if (value != _newUser)
                    {
                        _newUser.User.HeadImgMD5 = value.User.HeadImgMD5;
                        if (value.User.HeadImg == ImagePathHelper.DefaultServiceHead)
                        {
                            _newUser.User.HeadImg = CSClient.Helper.ImageHelper.GetAccountFace(_newUser.User.HeadImgMD5, (s) => _newUser.User.HeadImg = s);
                        }
                        else
                        {
                            _newUser.User.HeadImg = value.User.HeadImg;
                        }
                        _newUser.User.PhoneNumber = value.User.PhoneNumber;
                        _newUser.IsSavePassword = value.IsSavePassword;
                        _newUser.Password = value.Password;
                    }
                    _selectedLogin = value = _newUser;

                }
                else
                {
                    _newUser.User.HeadImg = ImagePathHelper.DefaultServiceHead;
                    _newUser.User.PhoneNumber = string.Empty;
                    _newUser.IsSavePassword = false;
                    _newUser.Password = string.Empty;
                    value = _newUser;
                }
                this.IsQuickLogin = value.IsSavePassword;
                this.IsEyeVisible = !value.IsSavePassword;
                _selectedLogin = value;
                this.ErrorInfo = string.Empty;
                this.OnPropertyChanged();
                this.IsOpenList = false;
            }
        }

        private bool _isOpenList = true;
        /// <summary>
        /// 是否打开列表
        /// </summary>
        public bool IsOpenList
        {
            get { return _isOpenList; }
            set { _isOpenList = value; this.OnPropertyChanged(); }
        }

        private bool _isSetQuickLogin;
        private bool _isQuickLogin;
        /// <summary>
        /// 是否快捷登录
        /// </summary>
        public bool IsQuickLogin
        {
            get { return _isQuickLogin; }
            set
            {
                if (_isSetQuickLogin)
                {
                    //快速登录只有第一次起作用
                    value = false;
                }
                else
                {

                }
                _isSetQuickLogin = true;
                _isQuickLogin = value; this.OnPropertyChanged();
            }
        }

        private bool _isEyeVisible;
        /// <summary>
        /// 是否可查看密码明文
        /// 可查看条件：1、当前登录用户无登录历史；2、当前登录用户上次登录时未保存密码
        /// </summary>
        public bool IsEyeVisible
        {
            get { return _isEyeVisible; }
            set { _isEyeVisible = value; this.OnPropertyChanged(); }
        }


        private bool _isWaitForLogin = true;
        /// <summary>
        /// 是否等待登录
        /// </summary>
        public bool IsWaitForLogin
        {
            get { return _isWaitForLogin; }
            private set { _isWaitForLogin = value; this.OnPropertyChanged(); }
        }


        private string _errorInfo;
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorInfo
        {
            get { return _errorInfo; }
            private set { _errorInfo = value; this.OnPropertyChanged(); }
        }

        #endregion
    }
}
