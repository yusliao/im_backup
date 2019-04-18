using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace IMLaunch
{
    public class UpdateViewModel : ViewModelBase
    {
        public RelayCommand<object> CmdClose { get; set; }
        public RelayCommand<object> CmdStartExe { get; set; }

        private string _version;
        private string _curVersion;
        private decimal _progress = 0.00M;
        private bool _installed;
        private bool _installFailed;
        private bool _isEnabledWindow;
        private string _description;
        private VersionInfo verInfo = null;
        private long totalSize = 0;
        private long curSize = 0;

        public bool IsError { get; set; }

        /// <summary>
        /// 是否启动主程序
        /// </summary>
        public bool IsStartMainApp { get; private set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; this.RaisePropertyChanged(); }
        }
        public bool IsEnabledWindow
        {
            get { return _isEnabledWindow; }
            set { _isEnabledWindow = value; this.RaisePropertyChanged(); }
        }
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version
        {
            get { return _version; }
            set { _version = value; this.RaisePropertyChanged(); }
        }
        public string CurVersion
        {
            get { return _curVersion; }
            set { _curVersion = value; this.RaisePropertyChanged(); }
        }
        /// <summary>
        /// 进度
        /// </summary>
        public decimal Progress
        {
            get { return _progress; }
            set { _progress = value; this.RaisePropertyChanged(); }
        }
        public long CurrentSize
        {
            get { return curSize; }
            set { curSize = value; this.RaisePropertyChanged(); }
        }
        public long TotalSize
        {
            get { return totalSize; }
            set { totalSize = value; this.RaisePropertyChanged(); }
        }
        /// <summary>
        /// 是否安装完成
        /// </summary>
        public bool Installed
        {
            get { return _installed; }
            set { _installed = value; this.RaisePropertyChanged(); }
        }

        /// <summary>
        /// 安装失败，是否使用旧版本
        /// </summary>
        public bool UserOldVersion
        {
            get { return _installFailed; }
            set { _installFailed = value; this.RaisePropertyChanged(); }
        }

        public UpdateViewModel(string updateVersion, string curVersion)
        {
            CmdStartExe = new RelayCommand<object>(StartExe);
            CmdClose = new RelayCommand<object>(Close);
            this.IsEnabledWindow = true;
            this.IsError = false;

            Version = updateVersion;
            CurVersion = curVersion;
            GetPcVersionDetail(curVersion);
            this.Description = "正在获取PC版本文件清单";
        }

        public static void KillMainProcess()
        {
            var runs = System.Diagnostics.Process.GetProcesses().Where(info => info.ProcessName == "IMUI").ToList();
            if (runs.Count > 0)
            {
                foreach (var r in runs)
                {
                    r.Kill();
                }
            }
        }

        /// <summary>
        /// 开启主程序
        /// </summary>
        /// <param name="obj"></param>
        void StartExe(object obj)
        {
            this.IsStartMainApp = true;
            this.IsEnabledWindow = false;
        }

        public void GetPcVersionDetail(string curVersion)
        {
            //System.Net.WebClient client = new System.Net.WebClient();
            //client.DownloadDataCompleted += Client_DownloadDataCompleted;
            //// client.DownloadProgressChanged += Client_DownloadProgressChanged;
            //client.DownloadDataAsync(new Uri($"{App.PcVersionDetail}={curVersion}"), "GetPcVersionDetail");

            System.Net.Http.HttpClient client = new HttpClient();

            var t1 = client.GetStringAsync($"{App.PcVersionDetail}={curVersion}").ContinueWith(t =>
            {
                if (t.Exception == null)
                {
                    if (!string.IsNullOrWhiteSpace(t.Result))
                    {
                        verInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<VersionInfo>(t.Result);

                        var lst = from item in verInfo.FileInfos
                                  select item.FileRelativePath;
                        var obj = new
                        {
                            FileList = lst
                        };
                        App.ReInstall = lst.Contains("IM_install.exe");
                        GetPcIncrementFiles(Newtonsoft.Json.JsonConvert.SerializeObject(obj));
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => this.Description = $"清单文件下载失败,失败原因:{t.Exception?.Message}");
                        //  System.Diagnostics.Debug.WriteLine($"清单文件下载失败,失败原因:{e.Error.Message}");
                        System.Threading.Thread.Sleep(2000);
                        Installed = false;
                        UserOldVersion = true;
                    }
                }
                else
                {
                    //出现异常
                    this.IsEnabledWindow = false;
                    this.IsError = true;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        UpdateFailedWindow.Instance().UpdateVersion = this._version;
                        UpdateFailedWindow.Instance().CurVersion = this._curVersion;
                        UpdateFailedWindow.Instance().ShowDialog();
                    });
                }
            });
        }

        private void Client_DownloadDataCompleted(object sender, System.Net.DownloadDataCompletedEventArgs e)
        {
            //  System.Diagnostics.Debug.WriteLine("下载完成！");

            if (!e.Cancelled && e.Error == null)
            {
                verInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<VersionInfo>(Encoding.UTF8.GetString(e.Result));

                var lst = from item in verInfo.FileInfos
                          select item.FileRelativePath; 
                var obj = new
                {
                    FileList = lst
                };

                App.ReInstall = lst.Contains("IM_install.exe");
                GetPcIncrementFiles(Newtonsoft.Json.JsonConvert.SerializeObject(obj));
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => this.Description = $"清单文件下载失败,失败原因:{e.Error.Message}");
                //  System.Diagnostics.Debug.WriteLine($"清单文件下载失败,失败原因:{e.Error.Message}");
                System.Threading.Thread.Sleep(2000);
                Installed = true;
            }
        }

        void GetPcIncrementFiles(string param)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"清单文件:{param}");
                System.Net.WebClient client = new System.Net.WebClient();
                client.UploadProgressChanged += Client_UploadProgressChanged; ;
                client.UploadDataCompleted += Client_UploadDataCompleted; ;
                client.Headers.Add(System.Net.HttpRequestHeader.ContentType, "application/json");
                client.UploadDataAsync(new Uri(App.PcIncrementFiles), Encoding.UTF8.GetBytes(param));
            }
            catch
            {
                //出现异常
                this.IsEnabledWindow = false;
                this.IsError = true;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    UpdateFailedWindow.Instance().UpdateVersion = this._version;
                    UpdateFailedWindow.Instance().CurVersion = this._curVersion;
                    UpdateFailedWindow.Instance().ShowDialog();
                });
            }
        }

        private void Client_UploadDataCompleted(object sender, System.Net.UploadDataCompletedEventArgs e)
        {
            //内容类型不是流，则为服务端出错
            if (((System.Net.WebClient)sender).ResponseHeaders == null ||
                ((System.Net.WebClient)sender).ResponseHeaders["Content-Type"] != "application/octet-stream" ||
                e.Error != null)
            {
                //出现异常
                this.IsEnabledWindow = false;
                this.IsError = true;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    UpdateFailedWindow.Instance().UpdateVersion = this._version;
                    UpdateFailedWindow.Instance().CurVersion = this._curVersion;
                    UpdateFailedWindow.Instance().ShowDialog();
                });

                return;
            }

            this.Progress = 100.00M;
            this.Description = "文件下载完毕";
            bool isUpdateSuccess = false;
            //备份的集合
            List<string> backs = new List<string>();
            //新增的集合
            List<string> adds = new List<string>();
            string tempPath = App.TempPath;
            try
            {
                if (File.Exists("myzip.zip"))
                {
                    File.Delete("myzip.zip");
                }

                this.Description = $"正在解压升级文件";
                //下载的临时文件路径

                Directory.CreateDirectory(tempPath);
                File.WriteAllBytes("myzip.zip", e.Result);
                using (ZipFile zip = new ZipFile("myzip.zip"))
                {
                    zip.ExtractAll(tempPath, ExtractExistingFileAction.OverwriteSilently);
                }

                //正在运行的实例
                KillMainProcess();

                if (App.ReInstall)
                {
                    string installpath = $"{tempPath}\\IM_install.exe";
                    System.Diagnostics.Process.Start(installpath);
                    //Environment.Exit(0);
                    this.IsEnabledWindow = false;
                    return;
                }

                this.Description = $"开始安装升级文件";
                foreach (var item in verInfo.FileInfos)
                {
                    this.Description = $"正在安装升级文件:{item.FileRelativePath}";
                    var info = new FileInfo(Path.Combine("temp", item.FileRelativePath));

                    //若存在，则备份后替换，否则直接拷贝
                    if (File.Exists(item.FileRelativePath))
                    {
                        string backPath = $"{item.FileRelativePath}_back";
                        //if(item.FileRelativePath==)
                        backs.Add(backPath);
                        info.Replace(item.FileRelativePath, backPath);
                    }
                    else
                    {
                        if (item.FileRelativePath.Contains("\\"))
                        {
                            string fileName = item.FileRelativePath.Split('\\').LastOrDefault();
                            string childDic = item.FileRelativePath.Remove(item.FileRelativePath.Length - fileName.Length);

                            Directory.CreateDirectory(childDic);
                        }

                        info.CopyTo(item.FileRelativePath, true);
                        adds.Add(item.FileRelativePath);
                    }
                }
                isUpdateSuccess = true;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.Description = $"升级文件失败,失败原因:{ex.Message}";
                });
                System.Diagnostics.Debug.WriteLine($"升级文件失败,失败原因:{ex.Message}");
            }

            if (isUpdateSuccess)
            {
                App.VersionNumber = verInfo.LatestVersion.ToString();
                App.Externalversion = verInfo.VersionName;
                Installed = true;

                //更新成功，删除所有备份的文件 
                try
                {
                    foreach (string back in backs)
                    {
                        try
                        {
                            File.Delete(back);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    string str = ex.Message;
                    //System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            else
            {
                //更新失败，恢复文件
                foreach (string back in backs)
                {
                    string old = back.Remove(back.LastIndexOf("_back"));

                    File.Copy(back, old, true);
                    File.Delete(back);
                }
                //删除新增的文件
                foreach (string add in adds)
                {
                    File.Delete(add);
                }

                Installed = false;
                UserOldVersion = true;
            }
            //删除临时文件夹
            if (Directory.Exists(tempPath))
                System.IO.Directory.Delete(tempPath, true);

        }

        bool isFirst = true;
        private void Client_UploadProgressChanged(object sender, System.Net.UploadProgressChangedEventArgs e)
        {
            if (isFirst && e.BytesReceived > 0)
            {
                this.Description = $"清单文件下载成功,升级文件大小:{e.TotalBytesToReceive / 1000}KB";
                isFirst = false;
            }
            this.Progress = Math.Round(decimal.Divide(e.BytesReceived, e.TotalBytesToReceive) * 100, 2);
        }

        void Close(object obj)
        {
            this.IsEnabledWindow = false;
        }
    }
}
