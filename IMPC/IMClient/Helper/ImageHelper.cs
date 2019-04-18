using SDKClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace IMClient.Helper
{
    public static class ImageHelper
    {
        public static string JoinImage(string[] images, int width, int height)
        {
            System.Drawing.Bitmap[] imageFiles = new System.Drawing.Bitmap[images.Length];
            for (int i = 0; i < imageFiles.Length; i++)
            {
                BitmapImage img = new BitmapImage(new Uri(images[i], UriKind.RelativeOrAbsolute));
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)img));
                encoder.Save(ms);
                imageFiles[i] = new System.Drawing.Bitmap(ms);
                ms.Close();
            }

            string directory = SDKClient.SDKClient.Instance.property.CurrentAccount.facePath;

            int currentHeight = 0;
            int currentWidth = 0;

            if (imageFiles.Length > 9)
            {
                Bitmap source9 = new Bitmap(width, height);
                Graphics graph9 = Graphics.FromImage(source9);
                graph9.SmoothingMode = SmoothingMode.AntiAlias;
                graph9.Clear(System.Drawing.Color.White);
                graph9.DrawImage(source9, width, height);
                for (int i = 0; i < 9; i++)
                {
                    Bitmap desc9 = new Bitmap(imageFiles[i], (width - 2) / 3, (height - 2) / 3);
                    currentWidth = desc9.Width;
                    currentHeight = desc9.Height;
                    switch (i)
                    {
                        case 0:
                            graph9.DrawImage(desc9, 0, 0);
                            desc9.Dispose();
                            break;
                        case 1:
                            graph9.DrawImage(desc9, currentWidth + 1, 0);
                            desc9.Dispose();
                            break;
                        case 2:
                            graph9.DrawImage(desc9, 2 * (currentWidth + 1), 0);
                            desc9.Dispose();
                            break;
                        case 3:
                            graph9.DrawImage(desc9, 0, currentHeight + 1);
                            desc9.Dispose();
                            break;
                        case 4:
                            graph9.DrawImage(desc9, currentWidth + 1, currentHeight + 1);
                            desc9.Dispose();
                            break;
                        case 5:
                            graph9.DrawImage(desc9, 2 * (currentWidth + 1), currentHeight + 1);
                            desc9.Dispose();
                            break;
                        case 6:
                            graph9.DrawImage(desc9, 0, 2 * (currentHeight + 1));
                            desc9.Dispose();
                            break;
                        case 7:
                            graph9.DrawImage(desc9, currentWidth + 1, 2 * (currentHeight + 1));
                            desc9.Dispose();
                            break;
                        case 8:
                            graph9.DrawImage(desc9, 2 * (currentWidth + 1), 2 * (currentHeight + 1));
                            desc9.Dispose();
                            break;
                    }
                }
                graph9.Dispose();
                //source9.Save(fileName);
                directory = CreateImage(directory, source9);
            }
            else
            {
                switch (imageFiles.Length)
                {
                    case 3:
                        Bitmap source3 = new Bitmap(width + 1, height + 1);
                        Graphics graph3 = Graphics.FromImage(source3);
                        graph3.SmoothingMode = SmoothingMode.AntiAlias;
                        graph3.Clear(System.Drawing.Color.White);
                        graph3.DrawImage(source3, width, height);

                        for (int i = 0; i < imageFiles.Length; i++)
                        {
                            Bitmap desc3 = new Bitmap(imageFiles[i], width, height);
                            currentWidth = desc3.Width;
                            currentHeight = desc3.Height;
                            switch (i)
                            {
                                case 0:
                                    graph3.DrawImage(desc3, 0, -((currentHeight / 2) + 1));
                                    break;
                                case 1:
                                    graph3.DrawImage(desc3, -((currentWidth / 2) + 1), (currentHeight / 2) + 1);
                                    break;
                                case 2:
                                    graph3.DrawImage(desc3, (currentWidth / 2) + 1, (currentHeight / 2) + 1);
                                    break;
                            }
                            desc3.Dispose();
                        }
                        graph3.Dispose();
                        directory = CreateImage(directory, source3);
                        // source3.Save(fileName);
                        break;
                    case 4:
                        Bitmap source4 = new Bitmap(width, height);
                        Graphics graph4 = Graphics.FromImage(source4);
                        graph4.SmoothingMode = SmoothingMode.AntiAlias;
                        graph4.Clear(System.Drawing.Color.White);
                        graph4.DrawImage(source4, width, height);

                        for (int i = 0; i < imageFiles.Length; i++)
                        {
                            Bitmap desc4 = new Bitmap(imageFiles[i], width / 2 - 1, height / 2 - 1);
                            currentWidth = desc4.Width;
                            currentHeight = desc4.Height;
                            switch (i)
                            {
                                case 0:
                                    graph4.DrawImage(desc4, 0, 0);
                                    desc4.Dispose();
                                    break;
                                case 1:
                                    graph4.DrawImage(desc4, currentWidth + 1, 0);
                                    desc4.Dispose();
                                    break;
                                case 2:
                                    graph4.DrawImage(desc4, 0, currentHeight + 1);
                                    desc4.Dispose();
                                    break;
                                case 3:
                                    graph4.DrawImage(desc4, currentWidth + 1, currentHeight + 1);
                                    desc4.Dispose();
                                    break;
                            }
                        }
                        graph4.Dispose();
                        // source4.Save(fileName);
                        directory = CreateImage(directory, source4);
                        break;
                    case 5:
                        Bitmap source5 = new Bitmap(width, height);
                        Graphics graph5 = Graphics.FromImage(source5);
                        graph5.SmoothingMode = SmoothingMode.AntiAlias;
                        graph5.Clear(System.Drawing.Color.White);
                        graph5.DrawImage(source5, width, height);

                        for (int i = 0; i < imageFiles.Length; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    Bitmap b0 = new Bitmap(imageFiles[i], (width - 1) / 2, (height - 1) / 2);
                                    graph5.DrawImage(b0, 0, 0);
                                    break;
                                case 1:
                                    Bitmap b1 = new Bitmap(imageFiles[i], (width - 1) / 2, (height - 1) / 2);
                                    graph5.DrawImage(b1, b1.Width + 1, 0);
                                    break;
                                case 2:
                                    Bitmap b2 = new Bitmap(imageFiles[i], (width - 2) / 3, (height - 1) / 2);
                                    graph5.DrawImage(b2, 0, b2.Height + 1);
                                    break;
                                case 3:
                                    Bitmap b3 = new Bitmap(imageFiles[i], (width - 2) / 3, (height - 1) / 2);
                                    graph5.DrawImage(b3, b3.Width + 1, b3.Height + 1);
                                    break;
                                case 4:
                                    Bitmap b4 = new Bitmap(imageFiles[i], (width - 2) / 3, (height - 1) / 2);
                                    graph5.DrawImage(b4, 2 * b4.Width + 2, b4.Height + 1);
                                    break;
                            }
                        }
                        graph5.Dispose();
                        // source5.Save(fileName);
                        directory = CreateImage(directory, source5);
                        break;
                    case 6:
                        Bitmap source6 = new Bitmap(width, height);
                        Graphics graph6 = Graphics.FromImage(source6);
                        graph6.SmoothingMode = SmoothingMode.AntiAlias;
                        graph6.Clear(System.Drawing.Color.White);
                        graph6.DrawImage(source6, source6.Width, source6.Height);

                        for (int i = 0; i < imageFiles.Length; i++)
                        {
                            Bitmap desc6 = new Bitmap(imageFiles[i], width / 2, height / 2);
                            currentWidth = desc6.Width;
                            currentHeight = desc6.Height;
                            switch (i)
                            {
                                case 0:
                                    graph6.DrawImage(desc6, -(3 * currentWidth) / 6, 0);
                                    break;
                                case 1:
                                    graph6.DrawImage(desc6, (3 * currentWidth) / 6, 0);
                                    break;
                                case 2:
                                    graph6.DrawImage(desc6, (((3 * currentWidth) / 6)) * 3, 0);
                                    break;
                                case 3:
                                    graph6.DrawImage(desc6, -(3 * currentWidth) / 6, currentHeight + 1);
                                    break;
                                case 4:
                                    graph6.DrawImage(desc6, (3 * currentWidth) / 6, currentHeight + 1);
                                    break;
                                case 5:
                                    graph6.DrawImage(desc6, (((3 * currentWidth) / 6)) * 3, currentHeight + 1);
                                    break;
                            }
                            desc6.Dispose();
                        }
                        graph6.Dispose();
                        //  source6.Save(fileName);
                        directory = CreateImage(directory, source6);
                        break;
                    case 7:
                        Bitmap source7 = new Bitmap(width, height);
                        Graphics graph7 = Graphics.FromImage(source7);
                        graph7.SmoothingMode = SmoothingMode.AntiAlias;
                        graph7.Clear(System.Drawing.Color.White);
                        graph7.DrawImage(source7, source7.Width, source7.Height);
                        for (int i = 0; i < imageFiles.Length; i++)
                        {
                            Bitmap desc7 = new Bitmap(imageFiles[i], (width - 2) / 3, (height - 2) / 3);
                            currentWidth = desc7.Width;
                            currentHeight = desc7.Height;
                            switch (i)
                            {
                                case 0:
                                    graph7.DrawImage(desc7, currentWidth, 0);
                                    break;
                                case 1:
                                    graph7.DrawImage(desc7, 0, currentHeight + 1);
                                    break;
                                case 2:
                                    graph7.DrawImage(desc7, currentWidth + 1, currentHeight + 1);
                                    break;
                                case 3:
                                    graph7.DrawImage(desc7, 2 * (currentWidth + 1), currentHeight + 1);
                                    break;
                                case 4:
                                    graph7.DrawImage(desc7, 0, 2 * (currentHeight + 1));
                                    break;
                                case 5:
                                    graph7.DrawImage(desc7, currentWidth + 1, 2 * (currentHeight + 1));
                                    break;
                                case 6:
                                    graph7.DrawImage(desc7, 2 * (currentWidth + 1), 2 * (currentHeight + 1));
                                    break;
                            }
                            desc7.Dispose();
                        }
                        graph7.Dispose();
                        //source7.Save(fileName);
                        directory = CreateImage(directory, source7);
                        break;
                    case 8:
                        Bitmap source8 = new Bitmap(width, height);
                        Graphics graph8 = Graphics.FromImage(source8);
                        graph8.SmoothingMode = SmoothingMode.AntiAlias;
                        graph8.Clear(System.Drawing.Color.White);
                        graph8.DrawImage(source8, source8.Width, source8.Height);
                        for (int i = 0; i < imageFiles.Length; i++)
                        {
                            Bitmap desc8 = new Bitmap(imageFiles[i], (width - 2) / 3, (height - 2) / 3);
                            currentWidth = desc8.Width;
                            currentHeight = desc8.Height;
                            switch (i)
                            {
                                case 0:
                                    graph8.DrawImage(desc8, 3 * currentWidth / 6, 0);
                                    break;
                                case 1:
                                    graph8.DrawImage(desc8, (3 * currentWidth / 6) * 3, 0);
                                    break;
                                case 2:
                                    graph8.DrawImage(desc8, 0, currentHeight + 1);
                                    break;
                                case 3:
                                    graph8.DrawImage(desc8, currentWidth + 1, currentHeight + 1);
                                    break;
                                case 4:
                                    graph8.DrawImage(desc8, 2 * (currentWidth + 1), currentHeight + 1);
                                    break;
                                case 5:
                                    graph8.DrawImage(desc8, 0, 2 * (currentHeight + 1));
                                    break;
                                case 6:
                                    graph8.DrawImage(desc8, currentWidth + 1, 2 * (currentHeight + 1));
                                    break;
                                case 7:
                                    graph8.DrawImage(desc8, 2 * (currentWidth + 1), 2 * (currentHeight + 1));

                                    break;
                            }
                            desc8.Dispose();
                        }
                        graph8.Dispose();
                        // source8.Save(fileName);
                        directory = CreateImage(directory, source8);
                        break;
                    case 9:
                        Bitmap source9 = new Bitmap(width, height);
                        Graphics graph9 = Graphics.FromImage(source9);
                        graph9.SmoothingMode = SmoothingMode.AntiAlias;
                        graph9.Clear(System.Drawing.Color.White);
                        graph9.DrawImage(source9, width, height);
                        for (int i = 0; i < imageFiles.Length; i++)
                        {
                            Bitmap desc9 = new Bitmap(imageFiles[i], (width - 2) / 3, (height - 2) / 3);
                            currentWidth = desc9.Width;
                            currentHeight = desc9.Height;
                            switch (i)
                            {
                                case 0:
                                    graph9.DrawImage(desc9, 0, 0);
                                    desc9.Dispose();
                                    break;
                                case 1:
                                    graph9.DrawImage(desc9, currentWidth + 1, 0);
                                    desc9.Dispose();
                                    break;
                                case 2:
                                    graph9.DrawImage(desc9, 2 * (currentWidth + 1), 0);
                                    desc9.Dispose();
                                    break;
                                case 3:
                                    graph9.DrawImage(desc9, 0, currentHeight + 1);
                                    desc9.Dispose();
                                    break;
                                case 4:
                                    graph9.DrawImage(desc9, currentWidth + 1, currentHeight + 1);
                                    desc9.Dispose();
                                    break;
                                case 5:
                                    graph9.DrawImage(desc9, 2 * (currentWidth + 1), currentHeight + 1);
                                    desc9.Dispose();
                                    break;
                                case 6:
                                    graph9.DrawImage(desc9, 0, 2 * (currentHeight + 1));
                                    desc9.Dispose();
                                    break;
                                case 7:
                                    graph9.DrawImage(desc9, currentWidth + 1, 2 * (currentHeight + 1));
                                    desc9.Dispose();
                                    break;
                                case 8:
                                    graph9.DrawImage(desc9, 2 * (currentWidth + 1), 2 * (currentHeight + 1));
                                    desc9.Dispose();
                                    break;
                            }
                        }
                        graph9.Dispose();
                        //source9.Save(fileName);
                        directory = CreateImage(directory, source9);
                        break;
                    default:
                        break;
                }
            }

            return directory;
        }

        private static string CreateImage(string directory, Bitmap source3)
        {
            string filePath = null;
            using (MemoryStream ms = new MemoryStream())
            {
                source3.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                ms.Seek(0, SeekOrigin.Begin);
                var bmpArray = ms.ToArray();

                string md5 = Util.Helpers.Encrypt.Md5By32(bmpArray);
                filePath = string.Format(@"{0}\{1}.png", directory, md5);

                if (Directory.Exists(directory) == false)
                    Directory.CreateDirectory(directory);
                if (!File.Exists(filePath))
                {
                    File.WriteAllBytes(filePath, bmpArray);
                }

            }
            return filePath;

        }

        private static readonly object lock_obj = new object();
        /// <summary>
        ///  下载头像
        /// </summary>
        /// <param name="resourceName">头像的MD5值</param>
        /// <param name="downloadcallback">下载完毕后的回掉方法</param>
        /// <returns></returns>
        public static string GetFriendFace(string resourceName, Action<string> downloadcallback = null)
        {
            string headImagePath;
            if (string.IsNullOrEmpty(resourceName))
            {
                headImagePath = ImagePathHelper.DefaultHeadImage;
            }
            else
            {
                var r1 = Uri.IsWellFormedUriString(resourceName, UriKind.Absolute);
                if (r1)
                {
                    string rn = Path.GetFileName(resourceName);
                    if (string.IsNullOrEmpty(Path.GetExtension(rn)))
                        return GetFriendFace(null);
                    headImagePath = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.facePath, rn);
                    if (File.Exists(headImagePath))
                    {
                        return headImagePath;
                    }
                    else
                    {
                        var _loadHisTask = Task.Run(() =>
                        {
                            SDKClient.SDKClient.Instance.DownLoadFacePhoto(resourceName,
                                () => downloadcallback(ImagePathHelper.DefaultHeadImage),
                                () => 
                                downloadcallback(headImagePath)
                                );
                        });
                        return ImagePathHelper.DefaultHeadImage;
                    }
                }
                else
                {
                    resourceName = Path.GetFileName(resourceName);
                    if (string.IsNullOrEmpty(Path.GetExtension(resourceName)))
                        return GetFriendFace(null);
                    headImagePath = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.facePath, resourceName);
                    if (File.Exists(headImagePath))
                    {
                        return headImagePath;
                    }
                    else
                    {
                        var _loadHisTask = Task.Run(() =>
                        {
                            SDKClient.SDKClient.Instance.DownLoadFacePhoto(resourceName,
                                () => downloadcallback(ImagePathHelper.DefaultHeadImage),
                                () => downloadcallback(headImagePath));
                        });
                        return ImagePathHelper.DefaultHeadImage;
                    }
                }
               
                
            }
            return headImagePath;
        }
        public static string GetAccountFace(string resourceName, Action<string> downloadcallback = null)
        {
            string headImagePath;
            if (string.IsNullOrEmpty(resourceName))
            {
                headImagePath = ImagePathHelper.DefaultHeadImage;
            }
            else
            {
                resourceName = Path.GetFileName(resourceName);
                if (string.IsNullOrEmpty(Path.GetExtension(resourceName)))
                    return GetAccountFace(null);

                headImagePath = Path.Combine(SDKProperty.facePath, resourceName);
                if (File.Exists(headImagePath))
                {
                    return headImagePath;
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(m =>
                    {
                        SDKClient.SDKClient.Instance.DownLoadFacePhoto(resourceName, () =>
                    {
                        downloadcallback(ImagePathHelper.DefaultHeadImage);

                    }, () => downloadcallback(headImagePath));
                    });
                    return ImagePathHelper.DefaultHeadImage;
                }
            }
            return headImagePath;
        }

        public static Bitmap BitmapSourceToBitmap(this BitmapSource source)
        {
            using (var stream = new MemoryStream())
            {
                var e = new BmpBitmapEncoder();
                e.Frames.Add(BitmapFrame.Create(source));
                e.Save(stream);

                var bmp = new Bitmap(stream);

                return bmp;
            }
        }

        /// <summary>
        /// 转换Image文件为Icon
        /// </summary> 
        public static Icon ConvertToIcon(string imgPath)
        {
            Bitmap image = new Bitmap(imgPath);
            if (image == null)
            {
                return null;
                //throw new ArgumentNullException("image");
            }

            using (MemoryStream msImg = new MemoryStream()
                              , msIco = new MemoryStream())
            {
                image.Save(msImg, System.Drawing.Imaging.ImageFormat.Png);

                using (var bin = new BinaryWriter(msIco))
                {
                    //写图标头部
                    bin.Write((short)0);           //0-1保留
                    bin.Write((short)1);           //2-3文件类型。1=图标, 2=光标
                    bin.Write((short)1);           //4-5图像数量（图标可以包含多个图像）

                    bin.Write((byte)image.Width);  //6图标宽度
                    bin.Write((byte)image.Height); //7图标高度
                    bin.Write((byte)0);            //8颜色数（若像素位深>=8，填0。这是显然的，达到8bpp的颜色数最少是256，byte不够表示）
                    bin.Write((byte)0);            //9保留。必须为0
                    bin.Write((short)0);           //10-11调色板
                    bin.Write((short)32);          //12-13位深
                    bin.Write((int)msImg.Length);  //14-17位图数据大小
                    bin.Write(22);                 //18-21位图数据起始字节

                    //写图像数据
                    bin.Write(msImg.ToArray());

                    bin.Flush();
                    bin.Seek(0, SeekOrigin.Begin);
                    return new Icon(msIco);
                }
            }
        }

        public static byte[] ImageToBytes(string path)
        {
            Image image = Image.FromFile(path);
            ImageFormat format = image.RawFormat;
            using (MemoryStream ms = new MemoryStream())
            {
                if (format.Equals(ImageFormat.Jpeg))
                {
                    image.Save(ms, ImageFormat.Jpeg);
                }
                else if (format.Equals(ImageFormat.Png))
                {
                    image.Save(ms, ImageFormat.Png);
                }
                else if (format.Equals(ImageFormat.Bmp))
                {
                    image.Save(ms, ImageFormat.Bmp);
                }
                else if (format.Equals(ImageFormat.Gif))
                {
                    image.Save(ms, ImageFormat.Gif);
                }
                else if (format.Equals(ImageFormat.Icon))
                {
                    image.Save(ms, ImageFormat.Icon);
                }
                byte[] buffer = new byte[ms.Length];
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }
    }
}
