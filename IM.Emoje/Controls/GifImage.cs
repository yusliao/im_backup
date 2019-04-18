using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image;

namespace IM.Emoje.Controls
{
    /// <summary>
    /// GIF 动画
    /// </summary>
    public class GifImage : Image
    {
        List<ImageFrame> _frames;

        DispatcherTimer _frameTimer;

        /// <summary>
        /// 图片初始化完毕
        /// </summary>
        public Action<GifImage> ImgInitialized;

        bool _isDispose = false;

        public bool IsDownloadCompleted { get; set; }

        public GifImage()
        {
            this.IsVisibleChanged += GifImage_IsVisibleChanged;

            this.Loaded += GifImage_Unloaded;
            this.Unloaded += GifImage_Unloaded;
        }

        private void GifImage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.IsVisible)
            {
                if (_frames != null && this._isDispose)
                {
                    LoadGif(string.Format("{0}", this.FilePath));
                }
            }
            else
            {
                this.Dispose();
            }
        }

        private void GifImage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void SetBaseImageSource(ImageSource source)
        {
            base.Source = source;
        }
        public static readonly DependencyProperty ForceGifAnimProperty = DependencyProperty.Register("ForceGifAnim", typeof(bool), typeof(GifImage), new FrameworkPropertyMetadata(false));
        public bool ForceGifAnim
        {
            get
            {
                return (bool)this.GetValue(ForceGifAnimProperty);
            }
            set
            {
                this.SetValue(ForceGifAnimProperty, value);
            }
        }

        public new static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(ImageSource), typeof(GifImage),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnSourceChanged)));
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GifImage gif = (GifImage)d;
            gif.LoadGif(string.Format("{0}", e.NewValue));
        }

        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register("FilePath", typeof(string), typeof(GifImage), new PropertyMetadata(OnSourceChanged));

        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public new ImageSource Source
        {
            get
            {
                return (ImageSource)this.GetValue(SourceProperty);
            }
            set
            {
                this.SetValue(SourceProperty, value);
            }
        }

        void RaiseImageFailedEvent(Exception exp)
        {
            GifImageExceptionRoutedEventArgs newArgs = new GifImageExceptionRoutedEventArgs(ImageFailedEvent, this);
            newArgs.ErrorException = exp;
            RaiseEvent(newArgs);
        }

        private void Deal_FramesLoaded(List<ImageFrame> frames)
        {
            if (frames.Count > 0)
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    ImgInitialized?.Invoke(this);
                }));

                this.Dispatcher.Invoke(new Action(() =>
                {
                    _frames = frames;
                    _frameTimer = new System.Windows.Threading.DispatcherTimer();
                    _frameTimer.Tick += NextFrame;
                    _frameTimer.Interval = new TimeSpan(0, 0, 0, 0, _frames[0].Delay);

                    if (this.IsActive)
                    {
                        _frameTimer.Start();
                    }
                }));
            }
            _isDispose = false;
        }
        private static readonly object waiting = new object();

        public void LoadGif(string imgPath)
        {
            Task.Run(() =>
            {
                lock (waiting)
                {
                    try
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            this.Dispose();
                        }));

                        if (imgPath.ToUpper().EndsWith(".GIF"))
                        {
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                            {
                                try
                                {
                                    base.Source = GetBitmapImage(imgPath);// new BitmapImage(new Uri(imgPath, UriKind.RelativeOrAbsolute));
                                }
                                catch (Exception ex)
                                {

                                }
                            }));
                            GifImageDeal deal = new GifImageDeal(imgPath);
                            if (deal.Frames == null)
                            {
                                deal.FramesLoaded += Deal_FramesLoaded;
                            }
                            else
                            {
                                Deal_FramesLoaded(deal.Frames);
                            }

                        }
                        else
                        {
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                            {
                                try
                                {
                                    base.Source = new BitmapImage(new Uri(imgPath, UriKind.RelativeOrAbsolute));
                                }
                                catch
                                {
                                    BitmapImage img = new BitmapImage();
                                    img.BeginInit();
                                    img.CacheOption = BitmapCacheOption.OnLoad;
                                    img.StreamSource = new MemoryStream(ImageToBytes(imgPath));
                                    img.EndInit();
                                    img.Freeze();
                                    base.Source = img;
                                }
                                finally
                                {
                                    ImgInitialized?.Invoke(this);
                                }                                
                            }));
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });
        }

        public static byte[] ImageToBytes(string path)
        {
            System.Drawing.Image image = System.Drawing.Image.FromFile(path);
            System.Drawing.Imaging.ImageFormat format = image.RawFormat;
            using (MemoryStream ms = new MemoryStream())
            {
                if (format.Equals(System.Drawing.Imaging.ImageFormat.Jpeg))
                {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                else if (format.Equals(System.Drawing.Imaging.ImageFormat.Png))
                {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                }
                else if (format.Equals(System.Drawing.Imaging.ImageFormat.Bmp))
                {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                }
                else if (format.Equals(System.Drawing.Imaging.ImageFormat.Gif))
                {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                }
                else if (format.Equals(System.Drawing.Imaging.ImageFormat.Icon))
                {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Icon);
                }
                byte[] buffer = new byte[ms.Length];
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        private BitmapImage GetBitmapImage(string path)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();

            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = new MemoryStream(File.ReadAllBytes(path));
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private bool _isActive = true;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;

                if (this._frameTimer != null)
                {
                    if (_isActive)
                    {
                        this._frameTimer.Start();
                    }
                    else
                    {
                        this._frameTimer.Stop();
                    }
                }
            }
        }

        public void Dispose()
        {
            base.Source = null;
            this.Source = null;
            if (_frameTimer != null)
            {
                _frameTimer.Stop();
                _frameTimer = null;
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    if (_frames != null)
                    {
                        if (this._frames != null && this._frames.Count > 0)
                        {
                            foreach (var f in this._frames)
                            {
                                f.Stream.Dispose();
                                f.BSource = null;
                            }
                        }

                        GC.Collect();
                        _isDispose = true;
                    }
                });
            }
            //else
            //{
            //    GC.Collect();
            //}
        }

        int currentIndex = 0;
        private void NextFrame(object sender, EventArgs e)
        {
            if (_frameTimer == null)
            {
                return;
            }
            _frameTimer.Stop();
            if (IsDownloadCompleted)
            {
                IsDownloadCompleted = false;
                Console.WriteLine("----------" + this.FilePath);
                LoadGif(this.FilePath);
                return;
            }
            if (_frames != null && _frames.Count == 1)
            {
                base.Source = _frames[0].BSource;
                return;
            }

            currentIndex = currentIndex < 0 ? 0 : currentIndex;
            currentIndex = currentIndex > _frames.Count - 1 ? _frames.Count - 1 : currentIndex;

            base.Source = _frames[currentIndex].BSource;

            //base.Source = new BitmapImage(new Uri(_frames[currentIndex].Path));

            if (currentIndex < _frames.Count - 1)
            {
                currentIndex++;
            }
            else
            {
                currentIndex = 0;
            }

            _frameTimer.Interval = new TimeSpan(0, 0, 0, 0, _frames[currentIndex].Delay);
            _frameTimer.Start();
        }
    }
}
