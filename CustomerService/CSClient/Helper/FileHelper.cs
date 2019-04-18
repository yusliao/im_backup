using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CSClient.Helper
{
  
    public static class FileHelper
    { 
        public static System.Windows.Media.ImageSource GetFileImage(string filePath,bool isEditor)
        { 
            string exten= System.IO.Path.GetExtension(filePath).ToLower();
            switch (exten)
            {
               
                case ".doc":
                case ".docx":
                    return isEditor ? IMAssets.ImageDeal.File_word0 : IMAssets.ImageDeal.File_word1;
                case ".xls":
                case ".xlsx":
                    return isEditor ? IMAssets.ImageDeal.File_excel0 : IMAssets.ImageDeal.File_excel1; 
                case ".ppt":
                    return isEditor ? IMAssets.ImageDeal.File_ppt0 : IMAssets.ImageDeal.File_ppt1;
                case ".exe":
                case ".msi":
                    return isEditor ? IMAssets.ImageDeal.File_exe0 : IMAssets.ImageDeal.File_exe1;
                case ".txt": 
                    return isEditor ? IMAssets.ImageDeal.File_text0 : IMAssets.ImageDeal.File_text1;
                case ".mp3":
                    return isEditor ? IMAssets.ImageDeal.File_mp30 : IMAssets.ImageDeal.File_mp31;
                case ".mp4":
                case ".mpg":
                case ".mpeg":
                case ".avi":
                case ".rm":
                case ".rmvb":
                case ".mov":
                case ".wmv":
                case ".asf":
                    return isEditor ? IMAssets.ImageDeal.File_mp40 : IMAssets.ImageDeal.File_mp41; 
                case ".pdf": 
                    return isEditor ? IMAssets.ImageDeal.File_pdf0 : IMAssets.ImageDeal.File_pdf1;
                case ".gif":
                case ".jpeg":
                case ".jpg":
                case ".png":
                case ".bmp":
                    return isEditor ? IMAssets.ImageDeal.File_image0 : IMAssets.ImageDeal.File_image0;  
                case ".zip":
                case ".rar":
                case ".gzip":
                case ".7z":
                case ".z":
                case ".iso":
                    return isEditor ? IMAssets.ImageDeal.File_rar1 : IMAssets.ImageDeal.File_rar1;

            }
            return isEditor ? IMAssets.ImageDeal.File_rar0 : IMAssets.ImageDeal.File_rar0;
        }
        public static string FileSizeToString(long size)
        {
            if (size < 0)
            {
                return "0B";
            }
            if (size < 1024)
            {
                return size + "B";
            }
            else if ((size / 1024) < 1024)
            {
                return string.Format("{0:0.0}KB", size / 1024d);
            }
            else
            {
                return string.Format("{0:0.00}MB", size / (1024 * 1024d));
            }
        }

        static System.Collections.Concurrent.ConcurrentBag<string> fileNameRecord = new System.Collections.Concurrent.ConcurrentBag<string>();

        /// <summary>
        /// 获取文件名
        /// </summary>
        /// <param name="combinePath">原始文件名</param>
        /// <param name="index">新文件序列号默认从1开始</param>
        /// <returns></returns>
        public static string GetFileName(string combinePath, int index = 1)
        {
            //新文件名称
            var filename = $"{Path.GetFileNameWithoutExtension(combinePath)}({index}){Path.GetExtension(combinePath)}";

            if (index == 1)//首次
            {
                if (File.Exists(combinePath))//原始文件存在
                {
                    var temp = $"{Path.Combine(Path.GetDirectoryName(combinePath), filename)}";
                    if (fileNameRecord.Any(s => s.Equals(filename)) || File.Exists(temp))//新文件已经存在
                    {
                        index += 1;
                        return GetFileName(combinePath, index);//递增序号
                    }
                    else
                    {
                        fileNameRecord.Add(filename);
                        return temp;
                    }
                }
                else//原始文件不存在
                {

                    if (fileNameRecord.Any(s => s.Equals(combinePath)))//原始文件内存中已经存在
                    {
                        var temp = $"{Path.Combine(Path.GetDirectoryName(combinePath), filename)}";
                        if (File.Exists(temp) || fileNameRecord.Any(s => s == filename))
                        {
                            index += 1;
                            return GetFileName(combinePath, index);//递增序号
                        }
                        else
                        {
                            fileNameRecord.Add(filename);
                            return temp;

                        }

                    }
                    else
                    {
                        fileNameRecord.Add(combinePath);
                        return combinePath;
                    }
                }
            }
            else
            {
                var temp = $"{Path.Combine(Path.GetDirectoryName(combinePath), filename)}";
                if (fileNameRecord.Any(s => s.Equals(filename)) || File.Exists(temp))//新文件已经存在
                {
                    index += 1;
                    return GetFileName(combinePath, index);//递增序号
                }
                else
                {
                    fileNameRecord.Add(filename);
                    return temp;
                }

            }


        }
    }

    [Flags]
    public enum ThumbnailOptions
    {
        None = 0x00,
        BiggerSizeOk = 0x01,
        InMemoryOnly = 0x02,
        IconOnly = 0x04,
        ThumbnailOnly = 0x08,
        InCacheOnly = 0x10,
    }

    public class WindowsThumbnailProvider
    {
        private const string IShellItem2Guid = "7E9FB0D3-919F-4307-AB2E-9B1860310C93";

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int SHCreateItemFromParsingName(
            [MarshalAs(UnmanagedType.LPWStr)] string path,
            // The following parameter is not used - binding context.  
            IntPtr pbc,
            ref Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out IShellItem shellItem);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr hObject);

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
        internal interface IShellItem
        {
            void BindToHandler(IntPtr pbc,
                [MarshalAs(UnmanagedType.LPStruct)]Guid bhid,
                [MarshalAs(UnmanagedType.LPStruct)]Guid riid,
                out IntPtr ppv);

            void GetParent(out IShellItem ppsi);
            void GetDisplayName(SIGDN sigdnName, out IntPtr ppszName);
            void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
            void Compare(IShellItem psi, uint hint, out int piOrder);
        };

        internal enum SIGDN : uint
        {
            NORMALDISPLAY = 0,
            PARENTRELATIVEPARSING = 0x80018001,
            PARENTRELATIVEFORADDRESSBAR = 0x8001c001,
            DESKTOPABSOLUTEPARSING = 0x80028000,
            PARENTRELATIVEEDITING = 0x80031001,
            DESKTOPABSOLUTEEDITING = 0x8004c000,
            FILESYSPATH = 0x80058000,
            URL = 0x80068000
        }

        internal enum HResult
        {
            Ok = 0x0000,
            False = 0x0001,
            InvalidArguments = unchecked((int)0x80070057),
            OutOfMemory = unchecked((int)0x8007000E),
            NoInterface = unchecked((int)0x80004002),
            Fail = unchecked((int)0x80004005),
            ElementNotFound = unchecked((int)0x80070490),
            TypeElementNotFound = unchecked((int)0x8002802B),
            NoObject = unchecked((int)0x800401E5),
            Win32ErrorCanceled = 1223,
            Canceled = unchecked((int)0x800704C7),
            ResourceInUse = unchecked((int)0x800700AA),
            AccessDenied = unchecked((int)0x80030005)
        }

        [ComImportAttribute()]
        [GuidAttribute("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IShellItemImageFactory
        {
            [PreserveSig]
            HResult GetImage(
            [In, MarshalAs(UnmanagedType.Struct)] NativeSize size,
            [In] ThumbnailOptions flags,
            [Out] out IntPtr phbm);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeSize
        {
            private int width;
            private int height;

            public int Width { set { width = value; } }
            public int Height { set { height = value; } }
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct RGBQUAD
        {
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
        }


        static Dictionary<string, System.Windows.Media.ImageSource> _hisOld =
            new Dictionary<string, System.Windows.Media.ImageSource>();

        public static System.Windows.Media.ImageSource GetFileThumbnail(string fileName, int width = 60, int height = 60, ThumbnailOptions options = ThumbnailOptions.IconOnly)
        {

            try
            {
                Bitmap bitmap = GetThumbnail(fileName, width, height, options);

                var source = ImageDeal.BitmapToBitmapSource(bitmap);

                //_hisOld.Add(value, source);
                return source;
            }
            catch
            {
                try
                {

                    return new System.Windows.Media.Imaging.BitmapImage(new Uri(fileName, UriKind.RelativeOrAbsolute));
                }
                catch
                {
                    return IMAssets.ImageDeal.DefaultImage;
                }
            }

            //if (File.Exists(fileName))
            //{
            //    using (FileStream file = new FileStream(fileName, FileMode.Open))
            //    {
            //        var md5 = System.Security.Cryptography.MD5.Create().ComputeHash(file);
            //        string value= BitConverter.ToString(md5);


            //        if (_hisOld.ContainsKey(value))
            //        {
            //            return _hisOld[value];
            //        }
            //        else
            //        {
            //            Bitmap bitmap = GetThumbnail(fileName, width, height, options);

            //            var source = ImageDeal.BitmapToBitmapSource(bitmap);

            //            _hisOld.Add(value, source);
            //            return source;
            //        }
            //    } 
            //}
            //else
            //{
            //    return null;
            //}
        }

        public static Bitmap GetThumbnail(string fileName, int width, int height, ThumbnailOptions options)
        {
            IntPtr hBitmap = GetHBitmap(Path.GetFullPath(fileName), width, height, options);

            try
            {
                // return a System.Drawing.Bitmap from the hBitmap  
                return GetBitmapFromHBitmap(hBitmap);
            }
            finally
            {
                // delete HBitmap to avoid memory leaks  
                DeleteObject(hBitmap);
            }
        }

        public static Bitmap GetBitmapFromHBitmap(IntPtr nativeHBitmap)
        {
            Bitmap bmp = Bitmap.FromHbitmap(nativeHBitmap);

            if (Bitmap.GetPixelFormatSize(bmp.PixelFormat) < 32)
                return bmp;

            return CreateAlphaBitmap(bmp, PixelFormat.Format32bppArgb);
        }

        public static Bitmap CreateAlphaBitmap(Bitmap srcBitmap, PixelFormat targetPixelFormat)
        {
            Bitmap result = new Bitmap(srcBitmap.Width, srcBitmap.Height, targetPixelFormat);

            Rectangle bmpBounds = new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height);

            BitmapData srcData = srcBitmap.LockBits(bmpBounds, ImageLockMode.ReadOnly, srcBitmap.PixelFormat);

            bool isAlplaBitmap = false;

            try
            {
                for (int y = 0; y <= srcData.Height - 1; y++)
                {
                    for (int x = 0; x <= srcData.Width - 1; x++)
                    {
                        Color pixelColor = Color.FromArgb(
                            Marshal.ReadInt32(srcData.Scan0, (srcData.Stride * y) + (4 * x)));

                        if (pixelColor.A > 0 & pixelColor.A < 255)
                        {
                            isAlplaBitmap = true;
                        }

                        result.SetPixel(x, y, pixelColor);
                    }
                }
            }
            finally
            {
                srcBitmap.UnlockBits(srcData);
            }

            if (isAlplaBitmap)
            {
                return result;
            }
            else
            {
                return srcBitmap;
            }
        }

        private static IntPtr GetHBitmap(string fileName, int width, int height, ThumbnailOptions options)
        {
            IShellItem nativeShellItem;
            Guid shellItem2Guid = new Guid(IShellItem2Guid);
            int retCode = SHCreateItemFromParsingName(fileName, IntPtr.Zero, ref shellItem2Guid, out nativeShellItem);

            if (retCode != 0)
                throw Marshal.GetExceptionForHR(retCode);

            NativeSize nativeSize = new NativeSize();
            nativeSize.Width = width;
            nativeSize.Height = height;

            IntPtr hBitmap;
            HResult hr = ((IShellItemImageFactory)nativeShellItem).GetImage(nativeSize, options, out hBitmap);

            Marshal.ReleaseComObject(nativeShellItem);

            if (hr == HResult.Ok) return hBitmap;

            throw Marshal.GetExceptionForHR((int)hr);
        }
    }
}
