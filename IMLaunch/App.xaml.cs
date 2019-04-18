using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace IMLaunch
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 是否需要重新安装
        /// </summary>
        public static bool ReInstall = false;
        public static string TempPath = Path.Combine(Environment.CurrentDirectory, "temp");
        public const string MainProgramName = @"IMUI.exe";
#if DEBUG || CHECK

        public const string LatestVersionNum = "http://ltxxx.com/api/Version/GetPCVersionNum?versionNum=";
        public const string PcVersionDetail = "http://ltxxx.com/api/Version/GetPcVersionDetail?versionNum";
        public const string LatestVersionInfo = "http://ltxxx.com/api/Version/GetPCVersion?versionNum=";
        public const string PcIncrementFiles = "http://ltxxx.com/api/Version/GetPcIncrementFiles";
#elif HUIDU
        public const string LatestVersionNum = "https://otxxx.com/api/Version/GetPCVersionNum?versionNum=";
        public const string PcVersionDetail = "https://otxxx.com/api/Version/GetPcVersionDetail?versionNum";
        public const string LatestVersionInfo = "https://otxxx.com/api/Version/GetPCVersion?versionNum=";
        public const string PcIncrementFiles = "https://otxxx.com/api/Version/GetPcIncrementFiles";
#elif RELEASE
        public const string LatestVersionNum = "https://xxx.com/api/Version/GetPCVersionNum?versionNum=";
        public const string LatestVersionInfo = "https://xxx.com/api/Version/GetPCVersion?versionNum=";

        public const string PcVersionDetail = "https://xxx.com/api/Version/GetPcVersionDetail?versionNum";
        public const string PcIncrementFiles = "https://xxx.com/api/Version/GetPcIncrementFiles";
#endif

        private static System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration($"{MainProgramName}");
        public static string version = config.AppSettings.Settings["version"].Value ?? "1";
        public static string VersionNumber
        {
            get
            {
                if (string.IsNullOrEmpty(version))
                {
                    version = "1";
                }
                return version;
            }
            set
            {
                config.AppSettings.Settings["version"].Value = value;

                config.Save();
                ConfigurationManager.RefreshSection("appSettings");
            }
        }
        private static string _externalversion = string.Empty;
        public static string Externalversion
        {
            get
            {
                return _externalversion;
            }

            set
            {
                if (config.AppSettings.Settings["externalversion"] == null)
                {
                    config.AppSettings.Settings.Add("externalversion", value);
                }
                else
                {
                    config.AppSettings.Settings["externalversion"].Value = value;
                }
                config.Save();
                ConfigurationManager.RefreshSection("appSettings");
            }
        }
        protected async override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string install = System.IO.Path.Combine(Environment.CurrentDirectory, "IM_install.exe");
            if (System.IO.File.Exists(install))
            {
                System.IO.File.Delete(install);
            }
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;


            dynamic result = await ScanNewVersion();
            bool isNew = result.isNew;
            VersionInfo info = result.info;
            if (isNew)
            {
                bool isForceUpdate = info.IsForce;
                MainWindow win = new MainWindow(info.VersionName, VersionNumber, isForceUpdate, info.UpdateContent);

                if (e.Args != null && e.Args.Length > 0)
                {
                    win.IsUpdateFromMainProcess = true;
                }
                win.ShowDialog();
            }
            else
            {
                Application.Current?.Shutdown(0);

                string mainProgramPath = string.Format(@"{0}\{1}", AppDomain.CurrentDomain.BaseDirectory, MainProgramName);
                System.Diagnostics.Process.Start(mainProgramPath);
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var exception = e.ExceptionObject as Exception;
                if (exception != null)
                {

                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string mainProgramPath = string.Format(@"{0}\{1}", AppDomain.CurrentDomain.BaseDirectory, MainProgramName);
            System.Diagnostics.Process.Start(mainProgramPath);

            e.Handled = true;
        }

        public async Task<VersionInfo> GetLatestVersionNum()
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync($"{LatestVersionInfo}{VersionNumber}");
            var str = await response.Content.ReadAsStringAsync();

            return Newtonsoft.Json.JsonConvert.DeserializeObject<VersionInfo>(str);
        }
        public async Task<dynamic> ScanNewVersion()
        {
            var version = await GetLatestVersionNum();

            if (!version.Success)
                return new { isNew = true, info = version };
            else
            {
                int l = version.VersionNum;
                int c = int.Parse(VersionNumber);
                if (l > c)
                {
                    return new { isNew = true, info = version };
                }
                else
                    return new { isNew = false, info = version };
            }
        }
    }

    public class VersionInfo
    {
        public List<FileUnitInfo> FileInfos { get; set; }
        public int LatestVersion { get; set; }
        public string VersionName { get; set; }
        /// <summary>
        /// 应用名称
        /// </summary>
        public string appName { get; set; }
        /// <summary>
        /// 返回码
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 错误消息
        /// </summary>
        public string error { get; set; }
        /// <summary>
        /// 请求是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        public int VersionNum { get; set; }
        /// <summary>
        /// 是否需要强制更新
        /// </summary>
        public bool IsForce { get; set; }
        public string UpdateContent { get; set; }
    }

    public class FileUnitInfo
    {
        public string FileRelativePath { get; set; }
        public long FileSize { get; set; }
        public float CurrentVersion { get; set; }
    }

}
