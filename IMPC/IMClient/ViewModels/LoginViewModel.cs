using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using IMClient.Helper;
using IMModels;

namespace IMClient.ViewModels
{
    public class LoginViewModel : ViewModel
    {
        public Action<LoginUser> OnLoginSuccess;

        public event Action SetStackPanelVisibility;
        /// <summary>
        /// 计时器，暂定为30秒
        /// </summary>
        DispatcherTimer _timer = new DispatcherTimer();
        public LoginViewModel(IView view) : base(view)
        {
            //LoadHistoryUsers();
            _timer.Interval = TimeSpan.FromSeconds(30);
            _timer.Tick += _timer_Tick;
            //SDKClient.SDKClient.Instance.NewDataRecv += Instance_NewDataRecv;
            //GetBadWordEditTime();
        }

        private static string lastSendTime = "2018-08-01 08:12";
        private void GetBadWordEditTime()
        {
            var obj = SDKClient.SDKClient.Instance.GetBadWordUpdate(lastSendTime);

            //DateTime dateTime = DateTime.Parse(obj.time);
            //DateTime last = DateTime.Parse(GetLastUpdateTime());
            WriteLatestValueToFile(obj.items);
            //UpdateTimeSubmit(obj.time);
        }

        private void WriteLatestValueToFile(List<SDKClient.WebAPI.GetSensitiveWordsResponse.Keyword> list)
        {
            string path = Environment.CurrentDirectory;
            string filepath = Path.Combine(path, "BadWord.txt");
            try
            {
                using (FileStream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        foreach (var item in list)
                        {
                            sw.WriteLine(item.word);
                        }
                        sw.Close();
                    }
                    fs.Close();
                }
            }
            catch (Exception ex)
            {

                throw;
            }


        }

        private string GetLastUpdateTime()
        {
            string filepath = System.IO.Path.Combine(Environment.CurrentDirectory, "Tools/Dataconfig.xml");
            if (!File.Exists(filepath))
                File.Create(filepath);
            XDocument xdoc = XDocument.Load(filepath);
            XElement xeRow = xdoc.Root.Element("BadWordTime");
            if (xeRow == null)
            {
                return DateTime.MinValue.ToString();
            }
            XElement xeValue = xeRow.Elements().OrderByDescending(x => DateTime.Parse(x.Value)).FirstOrDefault();
            if (xeValue == null)
                return DateTime.MinValue.ToString();
            return xeValue.Value;
        }

        private void UpdateTimeSubmit(string currentUptime)
        {
            string filepath = System.IO.Path.Combine(Environment.CurrentDirectory, "Tools/Dataconfig.xml");
            if (!File.Exists(filepath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement xmlRoot = xmlDoc.CreateElement("root");
                XmlElement xeRow = xmlDoc.CreateElement("", "BadWordTime", "");
                XmlElement xeItem = xmlDoc.CreateElement("", "item", "");
                xeItem.Value = currentUptime;
                xeRow.AppendChild(xeItem);
                xmlRoot.AppendChild(xeRow);
                xmlDoc.AppendChild(xmlRoot);
                xmlDoc.Save(filepath);
            }
            else
            {
                XDocument xdoc = XDocument.Load(filepath);
                if (xdoc.Root.Elements().Count(x => x.Name.LocalName.ToLower().Equals("BadWordTime")) > 0)
                {
                    return;
                }
                else
                {
                    XElement xRoot = xdoc.Root;
                    XElement xRow = xRoot.Element("BadWordTime");
                    xRoot.Add(new XElement("item", currentUptime));
                    xRoot.Add(xRow);
                    xdoc.Save(filepath);
                }
            }
        }

        public void SetState(bool isActive)
        {
            if (isActive)
            {
                SDKClient.SDKClient.Instance.NewDataRecv += Instance_NewDataRecv;
            }
            else
            {
                SDKClient.SDKClient.Instance.NewDataRecv -= Instance_NewDataRecv;
            }
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (_timer != null && _timer.IsEnabled)
            {
                this.IsErrorShow = true;
                this.ErrorInfo = "当前网络无法使用\n请检查后重新尝试";
                this.IsLogin = false;
                this.IsWaitForLogin = true;
                App.MUTEX?.Close();
                App.MUTEX = null;
            }
        }

        private void Instance_NewDataRecv(object sender, SDKClient.Model.PackageInfo e)
        {
            switch (e.apiId)
            {
                case SDKClient.Protocol.ProtocolBase.authCode:
                    //if (!this.IsWaitForLogin)
                    ReceivedAuthResponse(e);
                    
                    break;
                case SDKClient.Protocol.ProtocolBase.loginCode:
                    ReceivedLoginResponse(e);
                    break;
                case SDKClient.Protocol.ProtocolBase.QRConfirmCode: //手机端确认登录，
                    Util.Helpers.Async.Run(async () => await SDKClient.SDKClient.Instance.CreateConn());
                    break;
                case SDKClient.Protocol.ProtocolBase.QRCancelCode: //手机端不登录，取消操作
                    //SDKClient.SDKClient.Instance.GetLoginQRCode();
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        ChangeQRLogin();
                    });
                    break;
                case SDKClient.Protocol.ProtocolBase.ForceExitCode: //手机端强制PC端退出
                    SDKClient.SDKClient.Instance.SendLogout(SDKClient.SDKProperty.LogoutModel.Logout_kickout);
                    App.ReStart();
                    break;


                default:
                    break;

            }
            //this.LoginButtonEnabled = true; 
        }
        private void Instance_ConnState(object sender, bool e)
        {
            //if (IsWaitForLogin) //若正在登录，关闭了二维码服务连接会引起失败
            //{

            //}
            //else 
            if (!e)
            {
                //this.IsWaitingLogin = false;
                this.ErrorInfo = e ? string.Empty : "当前网络无法使用\n请检查后重新尝试";
            }
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
                ////重新发送连接业务请求
                //LoginFailed("网络未连接或服务器地址不可用");
                this.IsErrorShow = true;// "网络未连接或服务器地址不可用";
                if (!IsMoblePhone(this.SelectedLogin.User.PhoneNumber))
                    this.ErrorInfo = "用户名或密码错误";
                else
                    this.ErrorInfo = packageInfo.error;
                IsWaitPhoneConfirm = false;
                this.IsLogin = false;
                this.IsWaitForLogin = true;
                App.MUTEX?.Close();
                App.MUTEX = null;
            }
        }

        private void ReceivedAuthResponse(SDKClient.Model.PackageInfo packageInfo)
        {
            IsLogin = false;
            if (packageInfo.code == 0)
            {
                this.SelectedLogin.ID = this.SelectedLogin.User.ID = (packageInfo as SDKClient.Model.AuthPackage).data.userId;
                this.IsWaitForLogin = true;
                //连接成功
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    SDKClient.SDKClient.Instance.NewDataRecv -= Instance_NewDataRecv;
                    SDKClient.SDKClient.Instance.ConnState -= Instance_ConnState;
                    if (_timer != null)
                    {
                        _timer.Stop();
                        _timer.IsEnabled = false;
                        _timer = null;
                    }
                    SDKClient.SDKClient.Instance.GetBadWordEditTime();
                    this.OnLoginSuccess?.Invoke(this.SelectedLogin);
                });
            }
            else
            {
                this.IsWaitForLogin = false;
                this.IsErrorShow = true;
                this.ErrorInfo = packageInfo.error;
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

        private VMCommand _cancelLoginCommand;
        /// <summary>
        /// 取消登录命令
        /// </summary>
        public VMCommand CancelLoginCommand
        {
            get
            {
                if (_cancelLoginCommand == null)
                    _cancelLoginCommand = new VMCommand(CancelLogin);
                return _cancelLoginCommand;
            }
        }
        /// <summary>
        /// 切换账号
        /// </summary>
        public VMCommand ChangeLoginAccountCommand
        {
            get { return new VMCommand(ChangeLoginAccount); }

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

        /// <summary>
        /// 首次登陆
        /// </summary>
        private VMCommand _onFirstLogin;
        public VMCommand OnFirstLogin
        {
            get
            {
                if (_onFirstLogin == null)
                    _onFirstLogin = new VMCommand(FirstLogin);
                return _onFirstLogin;
            }
        }

        #endregion

        #region Methods
        private void ChangeLoginAccount(object para)
        {
            IsQuickLogin = false;
            //QRContentControl = new LoginQR();
            App.Current.Dispatcher.Invoke(() =>
            {
                SDKClient.SDKClient.Instance.NewDataRecv -= Instance_NewDataRecv;
                SDKClient.SDKClient.Instance.ConnState -= Instance_ConnState;
                var loginQR = new LoginQR();
                loginQR.RefVM = this;
                QRContentControl = loginQR;
            });

        }
        /// <summary>
        /// 点击立即安装时，跳转网页下载APP
        /// </summary>
        /// <param name="para"></param>
        private void FirstLogin(object para)
        {
            string url = (string)para;
            System.Diagnostics.Process.Start(url);
        }

        SDKClient.DB.historyAccountDB historyAccountDB;

        /// <summary>
        /// 加载历史登录用户
        /// </summary>
        public void LoadHistoryUsers()
        {
            AsyncHandler.CallFuncWithUI(System.Windows.Application.Current.Dispatcher, () =>
            {
                var task = SDKClient.SDKClient.Instance.GetAccountListDESC();
                // var task = await SDKClient.SDKClient.Instance.GetAccountListDESC();
                //var task = await SDKClient.SDKClient.Instance.GetAccountListDESC();
                if (task == null || task.Result.Count == 0)
                {
                    //this.SetStackPanelVisibility?.Invoke();

                    return null;
                }
                return task.Result;
            },
                  (ex, data) =>
                  {
                      if (data == null || data.Count == 0)
                      {

                          IsQuickLogin = false;
                          ChangeLoginAccount(null);
                          return;
                      }
                      var tempData = data.Where(m => !string.IsNullOrEmpty(m.token)).OrderByDescending(m => m.lastlastLoginTime).ToList();
                      if (tempData.Count > 0)
                      {
                          historyAccountDB = tempData[0];
                          if (historyAccountDB.FirstLoginTime != null && DateTime.Now.Day - historyAccountDB.FirstLoginTime.Value.Day <= 7)
                          {
                              IsQuickLogin = true;
                          }
                          else
                          {
                              IsQuickLogin = false;
                              ChangeLoginAccount(null);
                              return;
                          }

                      }
                      else
                      {
                          IsQuickLogin = false;
                          ChangeLoginAccount(null);
                          return;
                      }
                      IsQuickLogin = true;

                      //foreach (var item in data)
                      //{
                      UserModel user = new UserModel();
                      user.PhoneNumber = historyAccountDB.loginId;
                      user.HeadImg = IMClient.Helper.ImageHelper.GetAccountFace(historyAccountDB.headPic, (s) => App.Current.Dispatcher.Invoke(() => user.HeadImg = s));
                      user.HeadImgMD5 = historyAccountDB.headPic;
                      user.DisplayName = historyAccountDB.userName;
                      LoginUser login = new LoginUser(user);

                      if (historyAccountDB.loginModel != SDKClient.Model.LoginMode.None)
                      {
                          if (historyAccountDB.lastlastLoginTime.HasValue)
                          {
                              var interval = DateTime.Now - historyAccountDB.lastlastLoginTime.Value;

                              if (interval.TotalDays < 30)
                              {
                                  login.IsSavePassword = !string.IsNullOrEmpty(historyAccountDB.password) && !string.IsNullOrEmpty(historyAccountDB.loginId);
                                  login.Password = historyAccountDB.password;
                              }
                              else
                              {
                                  login.IsSavePassword = false;
                                  login.Password = string.Empty;
                                  App.Current.Dispatcher.BeginInvoke(new Action(() =>
                                  {
                                      this.HistoryUsers.Remove(login);
                                  }));
                              }
                          }
                          else
                          {

                          }

                      }
                      App.Current.Dispatcher.BeginInvoke(new Action(() =>
                      {
                          this.HistoryUsers.Add(login);
                          if (this.SelectedLogin == null)
                          {
                              this.SetStackPanelVisibility?.Invoke();
                              this.SelectedLogin = this.HistoryUsers.FirstOrDefault();
                          }
                      }));
                      //}


                  });

        }

        private LoginUser _newUser = new LoginUser(new UserModel() { HeadImg = IMAssets.ImageDeal.DefaultHeadImage });

        private void LoginAccountChanged(object para)
        {
            string account = string.Format("{0}", para);
            ErrorInfo = string.Empty;
            if (this.SelectedLogin == null || this.SelectedLogin.User.PhoneNumber != account)
            {
                //if (string.IsNullOrEmpty(account))
                //    account = this.SelectedLogin.User.PhoneNumber;
                var target = this.HistoryUsers.FirstOrDefault(info => info.User.PhoneNumber == account);

                if (target == null)
                {
                    _newUser.User.HeadImg = IMAssets.ImageDeal.DefaultHeadImage;
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

        private void CancelLogin(object obj)
        {
            SDKClient.SDKClient.Instance.NewDataRecv -= Instance_NewDataRecv;
            SDKClient.SDKClient.Instance.ConnState -= Instance_ConnState;
            App.MUTEX?.Close();
            App.MUTEX = null;
            if (_timer != null)
            {
                _timer.Stop();
                _timer.IsEnabled = false;
            }
            this.IsLogin = false;
            this.IsWaitForLogin = true;
        }

        public static bool IsMoblePhone(string input)
        {
            Regex regex = new Regex("^1[34578]\\d{9}$");
            return regex.IsMatch(input);
        }

        private void Login(object obj)
        {
            if (historyAccountDB == null)
                return;
            this.ErrorInfo = string.Empty;
            //if (string.IsNullOrEmpty(this.SelectedLogin.User.PhoneNumber))
            //{
            //    this.IsErrorShow = true;
            //    this.ErrorInfo = "账号不能为空";
            //    return;
            //}

            //if (string.IsNullOrEmpty(this.SelectedLogin.Password))
            //{
            //    this.IsErrorShow = true;
            //    this.ErrorInfo = "密码不能为空";
            //    return;
            //}

            System.Threading.Mutex temp;
#if RELEASE


            if (System.Threading.Mutex.TryOpenExisting(this.SelectedLogin.User.PhoneNumber, out temp))
            {
                this.IsErrorShow = true;
                this.ErrorInfo = "此账号已登录";
                return;
            }

#elif CHECK
            if (System.Threading.Mutex.TryOpenExisting(this.SelectedLogin.User.PhoneNumber + "cs", out temp))
            {
                this.IsErrorShow = true;
                this.ErrorInfo = "此账号已登录";
                return;
            }
#elif DEBUG
            if (System.Threading.Mutex.TryOpenExisting(this.SelectedLogin.User.PhoneNumber+"kf", out temp))
            {
                this.IsErrorShow = true;
                this.ErrorInfo = "此账号已登录";
                return;
            }
#elif HUIDU
            if (System.Threading.Mutex.TryOpenExisting(this.SelectedLogin.User.PhoneNumber+"hd", out temp))
            {
                this.IsErrorShow = true;
                this.ErrorInfo = "此账号已登录";
                return;
            }
#endif

            this.IsWaitForLogin = false;
            IsLogin = true;
            Task.Run(async () =>
            {
                SDKClient.Model.LoginMode mode = this.SelectedLogin.IsSavePassword ? SDKClient.Model.LoginMode.Save : SDKClient.Model.LoginMode.None;


                _timer?.Start();
                //this.LoginButtonEnabled = false;

                SDKClient.SDKClient.Instance.NewDataRecv -= Instance_NewDataRecv;
                SDKClient.SDKClient.Instance.ConnState -= Instance_ConnState;
                SDKClient.SDKClient.Instance.NewDataRecv += Instance_NewDataRecv;
                SDKClient.SDKClient.Instance.ConnState += Instance_ConnState;
                var t = await SDKClient.SDKClient.Instance.StartQRLoginAsync(true, historyAccountDB?.token);
                if (t)
                {
                    //IsLogin = false;
                    this.ErrorInfo = string.Empty;
                    this.IsWaitForLogin = false;
                    IsWaitPhoneConfirm = true;
                    return;
                }


                //this.IsConnecting = false;
                IsLogin = false;
                this.IsErrorShow = true;
                this.ErrorInfo = "当前网络无法使用\n请检查后重新尝试";
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
                this.SetStackPanelVisibility?.Invoke();
            }
        }
        private void ChangeQRLogin()
        {
            SDKClient.SDKClient.Instance.NewDataRecv -= Instance_NewDataRecv;
            SDKClient.SDKClient.Instance.ConnState -= Instance_ConnState;
            var loginQR = new LoginQR();
            loginQR.RefVM = this;
            QRContentControl = loginQR;
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
                        if (value.User.HeadImg == IMAssets.ImageDeal.DefaultHeadImage)
                        {
                            _newUser.User.HeadImg = IMClient.Helper.ImageHelper.GetAccountFace(_newUser.User.HeadImgMD5, (s) => _newUser.User.HeadImg = s);
                        }
                        else
                        {
                            _newUser.User.HeadImg = value.User.HeadImg;
                        }
                        _newUser.User.PhoneNumber = value.User.PhoneNumber;
                        _newUser.IsSavePassword = value.IsSavePassword;
                        _newUser.Password = value.Password;
                        _newUser.User.DisplayName = value.User.DisplayName;
                    }
                    _selectedLogin = value = _newUser;

                }
                else
                {
                    _newUser.User.HeadImg = IMAssets.ImageDeal.DefaultHeadImage;
                    _newUser.User.PhoneNumber = string.Empty;
                    _newUser.IsSavePassword = false;
                    _newUser.Password = string.Empty;
                    value = _newUser;
                }


                //this.IsQuickLogin = value.IsSavePassword;

                this.IsEyeVisible = !value.IsSavePassword;
                _selectedLogin = value;
                this.ErrorInfo = string.Empty;
                this.OnPropertyChanged();
                this.IsOpenList = false;
            }
        }

        private bool _isOpenList = false;
        /// <summary>
        /// 是否打开列表
        /// </summary>
        public bool IsOpenList
        {
            get { return _isOpenList; }
            set
            {
                _isOpenList = value;
                this.OnPropertyChanged("SelectedLogin");
                this.OnPropertyChanged();
            }
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
                //if (_isQuickLogin)
                //{
                //快速登录只有第一次起作用
                //value = false;
                //App.Current.Dispatcher.Invoke(() =>
                //{
                //    ChangeQRLogin();
                //});

                //IsQROverdue = true;
                //}
                //else
                //{

                //}

                //_isSetQuickLogin = true;
                _isQuickLogin = value; this.OnPropertyChanged();
            }
        }

        private bool _isWaitPhoneConfirm;
        /// <summary>
        /// 
        /// </summary>
        public bool IsWaitPhoneConfirm
        {
            get { return _isWaitPhoneConfirm; }
            set
            {
                _isWaitPhoneConfirm = value;
                this.OnPropertyChanged();
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
            set { _isWaitForLogin = value; this.OnPropertyChanged(); }
        }

        private ContentControl _QRContentControl;
        /// <summary>
        /// 扫码登录控件
        /// </summary>
        public ContentControl QRContentControl
        {
            get { return this._QRContentControl; }
            set
            {
                this._QRContentControl = value;
                this.OnPropertyChanged();
            }

        }


        private bool _isLogin = false;
        /// <summary>
        ///登录是否失效
        /// </summary>
        public bool IsLogin
        {
            get { return _isLogin; }
            set
            {
                _isLogin = value;
                this.OnPropertyChanged();
            }
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
                if (!string.IsNullOrEmpty(value) && _timer != null)
                {
                    _timer.Stop();
                    _timer.IsEnabled = false;
                }
                this.OnPropertyChanged();

            }
        }

        private bool _isErrorShow;
        /// <summary>
        /// 是否显示错误信息提示
        /// </summary>
        public bool IsErrorShow
        {
            get { return _isErrorShow; }
            set { _isErrorShow = value; this.OnPropertyChanged(); }
        }

        #endregion
    }
}
