using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image;

namespace IM.Emoje.Controls
{
    public class AnimatedGIF : Image
    {
        public static readonly DependencyProperty GIFSourceProperty = DependencyProperty.Register(
            "GIFSource", typeof(string), typeof(AnimatedGIF), new PropertyMetadata(OnSourcePropertyChanged));

        /// <summary>
        /// GIF图片源，支持相对路径、绝对路径
        /// </summary>
        public string GIFSource
        {
            get { return (string)GetValue(GIFSourceProperty); }
            set { SetValue(GIFSourceProperty, value); }
        }

        public static readonly DependencyProperty BmpSourceProperty = DependencyProperty.Register(
            "BmpSource", typeof(System.Drawing.Bitmap), typeof(AnimatedGIF), new PropertyMetadata(OnSourcePropertyChanged));

        /// <summary>
        /// 图片源
        /// </summary>
        public System.Drawing.Bitmap BmpSource
        {
            get { return (System.Drawing.Bitmap)GetValue(BmpSourceProperty); }
            set { SetValue(BmpSourceProperty, value); }
        }

        internal System.Drawing.Bitmap Bitmap; // Local bitmap member to cache image resource
        internal BitmapSource BitmapSource;
        public delegate void FrameUpdatedEventHandler();

        /// <summary>
        /// Delete local bitmap resource
        /// </summary>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool DeleteObject(IntPtr hObject);

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.Loaded += AnimatedGIF_Loaded;
            this.Unloaded += AnimatedGIF_Unloaded;
        }

        void AnimatedGIF_Unloaded(object sender, RoutedEventArgs e)
        {
            this.StopAnimation();
        }

        void AnimatedGIF_Loaded(object sender, RoutedEventArgs e)
        {
            BindSource(this);
        }

        /// <summary>
        /// Start animation
        /// </summary>
        public void StartAnimation()
        {
            System.Drawing.ImageAnimator.Animate(Bitmap, OnFrameChanged);
        }

        /// <summary>
        /// Stop animation
        /// </summary>
        public void StopAnimation()
        {
            System.Drawing.ImageAnimator.StopAnimate(Bitmap, OnFrameChanged);
        }

        /// <summary>
        /// Event handler for the frame changed
        /// </summary>
        private void OnFrameChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                   new FrameUpdatedEventHandler(FrameUpdatedCallback));
        }

        private void FrameUpdatedCallback()
        {
            System.Drawing.ImageAnimator.UpdateFrames();

            if (BitmapSource != null)
                BitmapSource.Freeze();

            // Convert the bitmap to BitmapSource that can be display in WPF Visual Tree
            BitmapSource = GetBitmapSource(this.BmpSource, this.BitmapSource);
            Source = BitmapSource;
            InvalidateVisual();
        }

        /// <summary>
        /// 属性更改处理事件
        /// </summary>
        private static void OnSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            //AnimatedGIF gif = sender as AnimatedGIF;
            //if (gif == null) return;
            //if (!gif.IsLoaded) return;
            //BindSource(gif);
        }

        private static void BindSource(AnimatedGIF gif)
        {            
            if (gif.Bitmap != null)
            {
                return;
                //gif.Bitmap.Dispose();
            }

            //var path = gif.GIFSource;
            //gif.Bitmap = new System.Drawing.Bitmap(path);
            gif.StopAnimation();
            gif.Bitmap = gif.BmpSource;

            //if (gif.BmpSource.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif))
            //{
            //    gif.StartAnimation();
            //}

            if (gif.BitmapSource != null)
                gif.BitmapSource.Freeze();

            gif.BitmapSource = GetBitmapSource(gif.Bitmap, gif.BitmapSource);
            gif.Source = gif.BitmapSource;
            gif.InvalidateVisual();          
        }

        private static BitmapSource GetBitmapSource(System.Drawing.Bitmap bmap, BitmapSource bimg)
        {
            IntPtr handle = IntPtr.Zero;

            try
            {
                handle = bmap.GetHbitmap();
                bimg = Imaging.CreateBitmapSourceFromHBitmap(
                    handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                if (handle != IntPtr.Zero)
                    DeleteObject(handle);
            }

            return bimg;
        }
    }
}
