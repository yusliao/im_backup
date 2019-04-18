using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace UpdateVersionHelper
{
    public static class UpdateHelper
    {
        public const string MainProgramName = @"IMUI.exe";
        public const string LatestVersionNum = "http://192.168.4.24:8090/api/Version/GetLatestPcVersionNum";
        public const string PcVersionDetail = "http://192.168.4.24:8090/api/Version/GetPcVersionDetail";
        public const string PcIncrementFiles = "http://192.168.4.24:8090/api/Version/GetPcIncrementFiles";
        static System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration($"{MainProgramName}.config");
        private static VersionInfo verInfo = null;
        public static async Task<string> GetLatestVersionNum()
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(LatestVersionNum);
            var str = await response.Content.ReadAsStringAsync();
            return str;
        }
        public static bool ScanNewVersion(out string newVersion)
        {
            newVersion = GetLatestVersionNum().Result;
            var curVersion = GetCurrentVersion();
            if (string.Equals(newVersion, curVersion))
                return false;
            else
            {
                int l = int.Parse(newVersion ?? "1");
                int c = int.Parse(curVersion);
                if (l > c)
                {
                    return true;
                }
                else
                    return false;
            }
        }
        public static string GetCurrentVersion()
        {
          
            return config.AppSettings.Settings["version"].Value??"1";
        }
        public static void GetPcVersionDetail(Action<bool,System.Net.DownloadDataCompletedEventArgs>completedCB,Action<long>progressCB)
        {
            System.Net.WebClient client = new System.Net.WebClient();
            client.DownloadDataCompleted += (s,e)=> {
                if (!e.Cancelled && e.Error == null)
                {
                    completedCB?.Invoke(true, e);
                    verInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<VersionInfo>(Encoding.UTF8.GetString(e.Result));
                  
                }
                else
                    completedCB?.Invoke(false, e);
            };
         
            client.DownloadProgressChanged +=(s,e)=>progressCB?.Invoke(e.BytesReceived);
            client.DownloadDataAsync(new Uri(PcVersionDetail));
        }
       
        private static void GetPcIncrementFiles(Action<bool, System.Net.UploadDataCompletedEventArgs> completedCB, Action<long> progressCB)
        {
            var lst = from item in verInfo.FileInfos
                      select item.FileRelativePath;
            var obj = new
            {
                FileList = lst
            };

            System.Net.WebClient client = new System.Net.WebClient();
            client.UploadProgressChanged +=(s,e)=> progressCB?.Invoke(e.BytesReceived);
            client.UploadDataCompleted += (s,e)=>
            {
                if (!e.Cancelled && e.Error == null)
                {
                    completedCB?.Invoke(true, e);
                    if (File.Exists("myzip.zip"))
                    {
                        File.Delete("myzip.zip");
                    }
                    File.WriteAllBytes("myzip.zip", e.Result);
                    using (ZipFile zip = new ZipFile("myzip.zip"))
                    {
                        zip.ExtractAll("temp", ExtractExistingFileAction.OverwriteSilently);
                    }
                    foreach (var item in verInfo.FileInfos)
                    {
                        try
                        {
                            var info = new FileInfo(Path.Combine("temp", item.FileRelativePath));
                            info.Replace(item.FileRelativePath, $"bak_{item.FileRelativePath}");
                        }
                        catch (Exception)
                        {

                        }
                    }
                    UpdateAppSetting(verInfo.LatestVersion.ToString());
                }
                else
                    completedCB?.Invoke(false, e);
            };
           
            client.Headers.Add(System.Net.HttpRequestHeader.ContentType, "application/json");
            client.UploadDataAsync(new Uri(PcIncrementFiles), Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(obj)));
        }
        
        public static void UpdateAppSetting(string newVersion)
        {
            config.AppSettings.Settings.Remove("version");
            config.AppSettings.Settings.Add("version", newVersion);
            config.Save();
        }
        
    }
    public class VersionInfo
    {
        public List<FileUnitInfo> FileInfos { get; set; }
        public int LatestVersion { get; set; }
    }

    public class FileUnitInfo
    {
        public string FileRelativePath { get; set; }
        public long FileSize { get; set; }
        public float CurrentVersion { get; set; }
    }
}
