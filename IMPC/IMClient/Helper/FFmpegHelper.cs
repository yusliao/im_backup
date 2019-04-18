using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMClient.Helper
{
    public static class FFmpegHelper
    {
        static readonly string FFmpegPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Tools\ffmpeg.exe");

        private static Process CreateProcess(string filename)
        {
            Process p = new Process();
            p.StartInfo.FileName = filename;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.CreateNoWindow = true;

            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;

            return p;
        }

        /// <summary>  
        /// 从视频中截取一帧  
        /// </summary>
        public static void GetOneFrameImageFromVideo(string videoPath, string thumbnailPath, Action callback)
        {
            using (System.Diagnostics.Process ffmpeg = CreateProcess(FFmpegPath))
            {
                string tempVideoPath = string.Format("\"{0}\"", videoPath);
                ffmpeg.StartInfo.Arguments = " -i " + tempVideoPath + " -q:v 2 -f image2 " + thumbnailPath;
                ffmpeg.ErrorDataReceived += new DataReceivedEventHandler(delegate (object sender, DataReceivedEventArgs e)
                {
                    if (System.IO.File.Exists(thumbnailPath))
                    {
                        callback?.Invoke();
                    }
                });
                ffmpeg.OutputDataReceived += new DataReceivedEventHandler(delegate (object sender, DataReceivedEventArgs e)
                {
                    if (System.IO.File.Exists(thumbnailPath))
                    {
                        callback?.Invoke();
                    }
                });
                ffmpeg.Start();
                ffmpeg.BeginErrorReadLine();
                ffmpeg.BeginOutputReadLine();
            }
        }

        /// <summary>
        /// 获取视频时长，格式（00:10）
        /// </summary>
        /// <returns></returns>
        public static string GetVideoDuration(string videoPath)
        {
            using (System.Diagnostics.Process ffmpeg = CreateProcess(FFmpegPath))
            {
                string result;
                System.IO.StreamReader errorreader;
                string tempVideoPath = string.Format("\"{0}\"", videoPath);
                ffmpeg.StartInfo.Arguments = " -i " + tempVideoPath;
                ffmpeg.Start();

                errorreader = ffmpeg.StandardError;

                ffmpeg.WaitForExit();

                result = errorreader.ReadToEnd();

                result = result.Substring(result.IndexOf("Duration: ") + ("Duration: ").Length, ("00:00:00").Length).Substring(3, 5);

                return result;
            }
        }
    }
}
