using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IMAssets
{
    public class ImageDeal
    {
        public static string DefaultHeadImage = @"pack://application:,,,/IMAssets;component/Images/normalhead.png";
        public static string NewHeadImage = @"pack://application:,,,/IMAssets;component/Images/friend_new.png";
        public static string FileAssistantIcon = @"pack://application:,,,/IMAssets;component/Images/fileassistant_icon.png";
        public static string NewsDefaultIcon = @"pack://application:,,,/IMAssets;component/Images/newsdefault_icon.png";
        public static string DefaultGroupHeadImage = @"pack://application:,,,/IMAssets;component/Images/normalhead.png";
        public static string DefaultDownloadImg = @"pack://application:,,,/IMAssets;component/Images/download.png";
        public static string ImgLoadFail = @"pack://application:,,,/IMAssets;component/Images/ImageFail.png";

        public static readonly string StrangerMessageImage = @"pack://application:,,,/IMAssets;component/Images/StrangerMessage.png";

        public static readonly BitmapSource DefaultImage =
            new BitmapImage(new Uri(DefaultGroupHeadImage, UriKind.RelativeOrAbsolute));
        public static readonly ImageSource File_default0 =
      new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_default0.png", UriKind.RelativeOrAbsolute));

        public static readonly ImageSource File_image0 =
       new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_image0.png", UriKind.RelativeOrAbsolute));
        public static readonly ImageSource File_image1 =
           new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_image1.png", UriKind.RelativeOrAbsolute));

        public static readonly ImageSource File_mp30 =
       new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_mp30.png", UriKind.RelativeOrAbsolute));
        public static readonly ImageSource File_mp31 =
           new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_mp31.png", UriKind.RelativeOrAbsolute));

        public static readonly ImageSource File_mp40 =
       new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_mp40.png", UriKind.RelativeOrAbsolute));
        public static readonly ImageSource File_mp41 =
           new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_mp41.png", UriKind.RelativeOrAbsolute));


        public static readonly ImageSource File_excel0 =
         new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_excel0.png", UriKind.RelativeOrAbsolute));
        public static readonly ImageSource File_excel1 =
           new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_excel1.png", UriKind.RelativeOrAbsolute));

        public static readonly ImageSource File_exe0 =
        new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_exe0.png", UriKind.RelativeOrAbsolute));
        public static readonly ImageSource File_exe1 =
           new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_exe1.png", UriKind.RelativeOrAbsolute));

        public static readonly ImageSource File_pdf0 =
        new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_pdf0.png", UriKind.RelativeOrAbsolute));
        public static readonly ImageSource File_pdf1 =
           new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_pdf1.png", UriKind.RelativeOrAbsolute));

        public static readonly ImageSource File_ppt0 =
        new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_ppt0.png", UriKind.RelativeOrAbsolute));
        public static readonly ImageSource File_ppt1 =
           new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_ppt1.png", UriKind.RelativeOrAbsolute));

        public static readonly ImageSource File_rar0 =
        new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_rar0.png", UriKind.RelativeOrAbsolute));
        public static readonly ImageSource File_rar1 =
           new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_rar1.png", UriKind.RelativeOrAbsolute));

        public static readonly ImageSource File_text0 =
        new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_text0.png", UriKind.RelativeOrAbsolute));
        public static readonly ImageSource File_text1 =
           new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_text1.png", UriKind.RelativeOrAbsolute));

        public static readonly ImageSource File_word0 =
        new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_word0.png", UriKind.RelativeOrAbsolute));
        public static readonly ImageSource File_word1 =
           new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/file_word1.png", UriKind.RelativeOrAbsolute));


        private static BitmapImage _activeCursorIcon;
        /// <summary>
        /// 屏幕剪切时鼠标样式图片
        /// </summary>
        public static BitmapImage ActiveCursorIcon
        {
            get
            {
                if (_activeCursorIcon == null)
                {
                    _activeCursorIcon = new BitmapImage(new Uri(@"pack://application:,,,/IMAssets;component/Images/normalhead.png"));
                }
                return _activeCursorIcon;
            }
        }

        private static Bitmap _activeCursorBitmap;
        /// <summary> 
        /// 屏幕剪切时鼠标样式图片
        /// </summary>
        public static Bitmap ActiveCursorBitmap
        {
            get
            {
                if (_activeCursorBitmap == null)
                {
                    _activeCursorBitmap = BitmapSourceToBitmap(ActiveCursorIcon);
                }
                return _activeCursorBitmap;
            }
        }





        private static Bitmap BitmapSourceToBitmap(BitmapSource source)
        {
            Bitmap target = null;
            try
            {
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)source));
                    encoder.Save(ms);

                    target = new Bitmap(ms);
                    ms.Close();
                }

            }
            catch
            {

            }
            return target;
        }

    }
}
