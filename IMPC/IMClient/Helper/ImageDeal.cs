using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging; 

namespace IMClient.Helper
{
    public static class ImageDeal
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// Bitmap转换为ImageSource
        /// </summary>
        /// <param name="bitmap">图片</param>
        /// <returns>ImageSource</returns>
        public static System.Windows.Media.ImageSource ToImageSource(this System.Drawing.Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            System.Windows.Media.ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            if (!DeleteObject(hBitmap))//记得要进行内存释放。否则会有内存不足的报错。
            {
                throw new System.ComponentModel.Win32Exception();
            }
            return wpfBitmap;
        }

        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void keybd_event(Keys bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        /// <summary>
        /// 获取屏幕图像
        /// </summary>
        /// <param name="rate">屏幕实际缩放比例</param>
        /// <returns></returns>
        public static BitmapSource GetScreenSnapshot(double rate)
        {
            try
            {
                ////System.Windows.Clipboard.Clear();
                ////SendKeys.SendWait("{PRTSC}");
                //////keybd_event(Keys.PrintScreen, 0, 0, 0);

                ////BitmapSource img = System.Windows.Clipboard.GetImage();
                ////var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                ////encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create((System.Windows.Media.Imaging.BitmapSource)img));
                ////System.Windows.Clipboard.Clear();
                ////return img;
                System.Drawing.Rectangle rc = SystemInformation.VirtualScreen;

                int w = (int)(rc.Width * rate);
                int h = (int)(rc.Height * rate);

                //var bitmap = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                //using (Graphics memoryGrahics = Graphics.FromImage(bitmap))
                //{
                //    memoryGrahics.CopyFromScreen(rc.X, rc.Y, 0, 0, new System.Drawing.Size(w, h)); 

                //}

                ///CopyFromScreen的函数，可以方便地实现屏幕截图，然而这只是针对有窗口句柄的的，像迅雷7、wpf等无句柄窗口是无法截取的
                Bitmap bitmap = new Bitmap(w, h);
                using (Graphics gDest = Graphics.FromImage(bitmap))
                {
                    Graphics gSrc = Graphics.FromHwnd(IntPtr.Zero);
                    IntPtr hSrcDC = gSrc.GetHdc();
                    IntPtr hDC = gDest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, w, h, hSrcDC, 0, 0, (int)(CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt));
                    gDest.ReleaseHdc();
                    gSrc.ReleaseHdc();
                } 
                return BitmapToBitmapSource(bitmap);
            }

            catch (Exception)
            {

            }
            return null;
        }


        [DllImport("Gdi32.dll")]
        public extern static int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
        //Dll切图，可以截取全部窗口  
        //public static void CutImageWithDll()
        //{
        //    int width = Screen.PrimaryScreen.Bounds.Width;
        //    int height = Screen.PrimaryScreen.Bounds.Height;
        //    Bitmap screenCopy = new Bitmap(width, height);
        //    using (Graphics gDest = Graphics.FromImage(screenCopy))
        //    {
        //        Graphics gSrc = Graphics.FromHwnd(IntPtr.Zero);
        //        IntPtr hSrcDC = gSrc.GetHdc();
        //        IntPtr hDC = gDest.GetHdc();
        //        int retval = BitBlt(hDC, 0, 0, width, height, hSrcDC, 0, 0, (int)(CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt));
        //        gDest.ReleaseHdc();
        //        gSrc.ReleaseHdc();
        //    }
        //}



        /// <summary>
        /// 从指定图像中截取指定位置和大小的图像
        /// </summary>
        /// <param name="source"></param>
        /// <param name="region"></param>
        /// <returns></returns>
        public static Bitmap CopyFromParentBitmap(Bitmap source, Rect region)
        {
            double systemdpi = 1;// _screenScale * wpfDpi;
            var sourceRect = new System.Drawing.Rectangle((int)(region.X * systemdpi), (int)(region.Y * systemdpi),
                (int)(region.Width * systemdpi), (int)(region.Height * systemdpi));

            var destRect = new System.Drawing.Rectangle(0, 0, (int)(sourceRect.Width), (int)(sourceRect.Height));
            if (source != null)
            {
                var bitmap = new Bitmap((int)(sourceRect.Width), (int)(sourceRect.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.DrawImage(source, destRect, sourceRect, GraphicsUnit.Pixel);
                }
                return bitmap;
            }
            return null;
        }

        /// <summary>
        /// 保存指定控件的某部分到图片中
        /// </summary>
        /// <param name="targetUI"></param>
        /// <param name="clipRect"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static BitmapSource SaveUiToImageFile(FrameworkElement targetUI, System.Drawing.Rectangle clip,double dpiRate, string fileName)
        { 
            if (targetUI==null|| targetUI.ActualWidth==0||  targetUI.ActualHeight==0)
            {
                return null;
            }

            try
            {
                double w = dpiRate * targetUI.ActualWidth, h = dpiRate * targetUI.ActualHeight;
                RenderTargetBitmap targetBitmap = new RenderTargetBitmap((int)w, (int)h, PrimaryScreen.DpiX, PrimaryScreen.DpiY, System.Windows.Media.PixelFormats.Default);
                targetBitmap.Render(targetUI);

                  
                var partImage = GetPartImage(targetBitmap, clip.X, clip.Y, clip.Width, clip.Height);
                JpegBitmapEncoder saveEncoder = new JpegBitmapEncoder();
                saveEncoder.Frames.Add(BitmapFrame.Create(partImage));
                using (System.IO.FileStream fs = System.IO.File.Open(fileName, System.IO.FileMode.OpenOrCreate))
                {
                    saveEncoder.Save(fs);
                };
                return partImage;
            }
            catch(Exception ex)
            {
                ex.Message.ToString();
                return null;
            } 
        }
         

        public static BitmapSource BitmapToBitmapSource(Bitmap bmp)
        {
            BitmapSource bs = null;
            try
            {
                bs = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(),
                IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {

            }
            return bs;
        }

       

        /// <summary>
        /// 获取一张图片中的一部分 
        /// </summary>
        /// <param name="source">原图片</param>
        /// <param name="XCoordinate">要截取部分的X坐标</param>
        /// <param name="YCoordinate">要截取部分的Y坐标</param>
        /// <param name="Width">截取的宽度</param>
        /// <param name="Height">截取的高度</param>
        /// <returns></returns>
        public static BitmapSource GetPartImage(BitmapSource source, int XCoordinate, int YCoordinate, int Width, int Height)
        {
            return new CroppedBitmap(source, new Int32Rect(XCoordinate, YCoordinate, Width, Height));
        }

        //public static BitmapSource GetPartImage(BitmapSource bitmap,Int32Rect rect)
        //{
        //    var stride = bitmap.Format.BitsPerPixel * rect.Width / 8;
        //    //声明字节数组
        //    byte[] data = new byte[rect.Height * stride];
        //    //调用CopyPixels
        //    bitmap.CopyPixels(rect, data, stride, 0);

        //  return   BitmapSource.Create(100, 100, 0, 0, System.Windows.Media.PixelFormats.Bgr32, null, data, stride);
        //}



        /// <summary>
        /// 把内存里的BitmapImage数据保存到硬盘中
        /// </summary>
        /// <param name="souce">图像数据</param> 
        public static string  SaveBitmapImageIntoFile(BitmapSource souce)
        { 
            string path = System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, Guid.NewGuid().ToString()+".jpg");

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(souce));

            using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }

            return path;
        }

    }
    

}
