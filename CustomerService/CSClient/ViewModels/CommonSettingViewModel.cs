using CSClient.Views.ChildWindows;
using IMModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CSClient.ViewModels
{
    public class CommonSettingViewModel : ViewModel
    {
        public CommonSettingViewModel(CommonSettingModel model) : base(model)
        {

        }

        #region Commands

        private VMCommand _openTempDirectoryCommand;
        /// <summary>
        /// 打开缓存文件夹
        /// </summary> 
        public VMCommand OpenTempDirectoryCommand
        {
            get
            {
                if (_openTempDirectoryCommand == null)
                    _openTempDirectoryCommand = new VMCommand(OpenTempDirectory);
                return _openTempDirectoryCommand;
            }
        }

        private VMCommand _checkUpdateCommand;
        /// <summary>
        /// 检查更新
        /// </summary>
        public VMCommand CheckUpdateCommand
        {
            get
            {
                if (_checkUpdateCommand == null)
                    _checkUpdateCommand = new VMCommand(CheckUpdate);
                return _checkUpdateCommand;
            }
        }

        private VMCommand _clearCacheCommand;
        /// <summary>
        /// 清除缓存
        /// </summary>
        public VMCommand ClearCacheCommand
        {
            get
            {
                if (_clearCacheCommand == null)
                    _clearCacheCommand = new VMCommand(ClearCache);
                return _clearCacheCommand;
            }
        }

        private VMCommand _logoutCommand;
        /// <summary>
        /// 注销
        /// </summary>
        public VMCommand LogoutCommand
        {
            get
            {
                if (_logoutCommand == null)
                    _logoutCommand = new VMCommand(Logout);
                return _logoutCommand;
            }
        }
        #endregion

        private void OpenTempDirectory(object para)
        {
            System.Diagnostics.Process.Start("Explorer.exe", (this.Model as CommonSettingModel).TempFilePath);
        }

        private void CheckUpdate(object para)
        {
            string version;
            bool hasNewVersion = SDKClient.SDKClient.Instance.ScanNewVersion(out version);
            if (hasNewVersion)
            {
                //有新版本，直接打开升级程序
                System.Diagnostics.Process.Start(SDKClient.SDKProperty.LaunchObj, "IMUI");
            }
            else
            {
                DetectNewVersionWindow win = new DetectNewVersionWindow(hasNewVersion, version);
                win.ShowDialog();
            }
        }

        private void ClearCache(object para)
        {
            ClearCacheWindow win = new ClearCacheWindow();
            bool? result = App.IsCancelOperate("清除缓存", "您有文件正在传输中，确定终止文件传输吗？");
            if (result == true) //取消，不做操作
            {

            }
            else if (result == false) //继续操作，则直接执行
            {
                win.Clear();
            }
            else //未有提示，先提示
            {
                win.ShowDialog();
            }
        }

        private void Logout(object para)
        {
            bool? result = App.IsCancelOperate("注销登录", "您有文件正在传输中，确定终止文件传输吗？");
            if (result==true)//取消，不做操作
            {
                return;
            }
            else if (result ==null && Views.MessageBox.ShowDialogBox("确定注销当前登录吗？") != true) 
            {
                return;
            }
            
            (App.Current as App).ApplicationExit(null, null);
            SDKClient.SDKClient.Instance.StopAsync().ConfigureAwait(false);
            Application.Current?.Shutdown(0);

            string mainProgramPath = string.Format(@"{0}\IMUI.exe", AppDomain.CurrentDomain.BaseDirectory);
            System.Diagnostics.Process.Start(mainProgramPath);
        }
    }

    public class CommonSettingModel : BaseModel
    {
        private static System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);

        private bool _isAutoStartup;
        /// <summary>
        /// 是否开机自启动
        /// </summary>
        public bool IsAutoStartup
        {
            get { return _isAutoStartup; }
            set
            {
                _isAutoStartup = value;

                //NativeMethods.SetAutoBootStatu(value);
                this.OnPropertyChanged();
                config.AppSettings.Settings.Remove("IsAutoStartup");
                config.AppSettings.Settings.Add("IsAutoStartup", value.ToString());
                config.Save();
            }
        }

        private bool _isSavePassword;
        /// <summary>
        /// 是否记住密码
        /// </summary>
        public bool IsSavePassword
        {
            get { return _isSavePassword; }
            set
            {
                _isSavePassword = value;
                this.OnPropertyChanged();

                SDKClient.Model.LoginMode loginMode = _isSavePassword == true ? SDKClient.Model.LoginMode.Save : SDKClient.Model.LoginMode.None;
                SDKClient.SDKClient.Instance.UpdateAccountLoginModel(loginMode);

            }
        }

        private string _tempFilePath;
        /// <summary>
        /// 临时文件路径
        /// </summary>
        public string TempFilePath
        {
            get { return _tempFilePath; }
            set { _tempFilePath = value; this.OnPropertyChanged(); }
        }

        public CommonSettingModel()
        {
            this._tempFilePath = SDKClient.SDKProperty.rootPath;
            //this._isAutoStartup = bool.Parse(config.AppSettings.Settings["IsAutoStartup"].Value.ToLower());
            this._isSavePassword = AppData.Current.LoginUser.IsSavePassword;
        }
    }
}
