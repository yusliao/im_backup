using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using IMClient.Helper;
using IMModels;

namespace IMClient.Views.ChildWindows
{
    /// <summary>
    /// ImageScanWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ImageScanWindow : Window
    {
        /// <summary>
        /// 当前窗口
        /// </summary>
        static ImageScanWindow _current = null;
        /// <summary>
        /// 可以呈现的集合路径
        /// </summary>
        IList<MessageModel> _sources;
        /// <summary>
        /// 当前呈现位置
        /// </summary>
        MessageModel _target;

        private double _scale = 1;
        private double Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                this.thumb.IsHitTestVisible = _scale > 1;
            }
        }

        static double _originalImageWidth;
        static double _originalImageHeight;

        Storyboard _showStory, _hideStory;

        public ImageScanWindow()
        {
            InitializeComponent();

            this.MaxHeight = PrimaryScreen.MaxAreaHeight;
            this.MaxWidth = PrimaryScreen.MaxAreaWidth;

            this.gridView.MouseWheel += gridView_MouseWheel;
            this.MouseLeftButtonDown += ImageScanWindow_MouseLeftButtonDown;

            _showStory = this.Resources["ShowDicBtn"] as Storyboard;
            _hideStory = this.Resources["HideDicBtn"] as Storyboard;

            //this.Topmost = true;
            //this.Owner = App.Current.MainWindow;
            this.PreviewKeyDown += ImageScanWindow_PreviewKeyDown;
            this.thumb.DragDelta += Thumb_DragDelta;
            this.Closed += ImageScanWindow_Closed;
            this.Loaded += ImageScanWindow_Loaded;
            //this.StateChanged += ImageScanWindow_StateChanged;
            ViewModels.MainViewModel.CloseAppend?.Invoke(false);

            this.gridView.MouseEnter += GridView_MouseEnter;

            this.gridView.MouseLeave += GridView_MouseLeave;

            this.img.ImgInitialized += delegate
            {
                SetState(true);
            };
        }

        private void GridView_MouseEnter(object sender, MouseEventArgs e)
        {
            if (this._sources != null && this._sources.Count > 1)
            {
                _showStory.Begin();
            }
        }

        private void GridView_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this._sources != null && this._sources.Count > 1)
            {
                _hideStory.Begin();
            }
        }


        //private void ImageScanWindow_StateChanged(object sender, EventArgs e)
        //{
        //    SetScale(this);
        //}

        private void ImageScanWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void ImageScanWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Activate();
        }

        private void ImageScanWindow_Closed(object sender, EventArgs e)
        {
            _current = null;
            this.PreviewKeyDown -= ImageScanWindow_PreviewKeyDown;
            this.thumb.DragDelta -= Thumb_DragDelta;
            this.Closed -= ImageScanWindow_Closed;
            this.Loaded -= ImageScanWindow_Loaded;

            Views.ChildWindows.AppendWindow.AutoClose = false;
            App.Current.MainWindow.Activate();
            Views.ChildWindows.AppendWindow.AutoClose = true;

            _sources = null;
            this.img.Dispose();
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (Scale > 1)
            {
                double x = e.HorizontalChange + translate.X,
                       y = e.VerticalChange + translate.Y;

                if (x > this.img.ActualWidth * Scale * 0.5 || x < -this.img.ActualWidth * Scale * 0.5)
                {
                    x = translate.X;
                }

                if (y > this.img.ActualHeight * Scale * 0.5 || y < -this.img.ActualHeight * Scale * 0.5)
                {
                    y = translate.Y;
                }

                translate.X = x;
                translate.Y = y;
            }
        }

        private void ImageScanWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            if (e.ClickCount == 2)
            {
                translate.X = translate.Y = 0;
                this.rotate.Angle = 0;
                this.scale.ScaleX = this.scale.ScaleY = 1;
            }
        }

        public static void ShowScan(ImageSource img)
        {
            if (img == null)
            {
                return;
            }
            bool isCreate;

            ImageScanWindow scan = CreateOrActiveWindow(out isCreate);

            scan.rotate.Angle = 0;
            scan.scale.ScaleX = scan.scale.ScaleY = 1;

            scan.img.Visibility = Visibility.Visible;
            SetProperty(img.ToString(), scan, img, false);
            string imgPath = img.ToString().Contains("file:///") ? img.ToString().Remove(0, "file:///".Length) : img.ToString();
            if (isCreate)
            {
                scan.ShowCurrent(imgPath);
            }
            else
            {
                Task.Delay(50).ContinueWith(task =>
                {
                    scan.Dispatcher.Invoke(new Action(() =>
                    {
                        if (scan.WindowState == WindowState.Minimized)
                        {
                            scan.WindowState = WindowState.Normal;
                        }
                        scan.Activate();
                    }));
                });
            }

        }

        public static void ShowScan(string imgPath)
        {
            if (imgPath != IMAssets.ImageDeal.DefaultHeadImage && !System.IO.File.Exists(imgPath))
            {
                return;
            }
            bool isCreate;
            if (_current != null)
            {
                _current.Close();
                _current = null;
            }
            ImageScanWindow scan = CreateOrActiveWindow(out isCreate);

            scan.rotate.Angle = 0;
            scan.scale.ScaleX = scan.scale.ScaleY = 1;
            scan.img.Visibility = Visibility.Visible;
            BitmapImage img = new BitmapImage(new Uri(imgPath, UriKind.RelativeOrAbsolute));
            SetProperty(imgPath, scan, img, false);

            if (isCreate)
            {
                scan.ShowCurrent(imgPath);
            }
            else
            {
                Task.Delay(50).ContinueWith(task =>
                {
                    scan.Dispatcher.Invoke(new Action(() =>
                    {
                        if (scan.WindowState == WindowState.Minimized)
                        {
                            scan.WindowState = WindowState.Normal;
                        }
                        scan.Activate();
                    }));
                });
            }
        }


        public static void ShowScan(IList<MessageModel> list, MessageModel target)
        {
            if (list == null || list.Count == 0 || target == null)
            {
                return;
            }

            if (_current != null)
            {
                _current.Close();
            }

            ImageScanWindow scan = _current = new ImageScanWindow();
            scan.img.Source = null;
            scan._sources = list;
            scan._target = target;
            scan.ShowCurrent();
        }

        private static void Scan_Loaded(object sender, RoutedEventArgs e)
        {
            var scan = sender as ImageScanWindow;

        }

        private void SetState(bool canOperate)
        {
            if (canOperate)
            {
                this.gridOperate.Visibility = Visibility.Visible;
            }
            else
            {
                this.gridOperate.Visibility = Visibility.Collapsed;
            }
        }
        private string _currentPath;
        bool isUploadImg;
        private void ShowCurrent(string _imgPath = "")
        {
            //string imgPath = _target.Content;
            var imgPath = string.Empty; ;
            if (!string.IsNullOrEmpty(_imgPath))
                imgPath = _imgPath;
            else if (_target != null)
            {
                if ((_target.MessageState == MessageStates.None || _target.MessageState == MessageStates.Success) && _target.ResourceModel != null && !string.IsNullOrEmpty(_target.ResourceModel.Key))
                {
                    string imagePath = System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, _target.ResourceModel.Key);
                    imgPath = imagePath;
                }
                else
                    imgPath = _target.Content;
            }

            this.rotate.Angle = 0;
            this.scale.ScaleX = this.scale.ScaleY = 1;

            string exten = System.IO.Path.GetExtension(imgPath).ToLower();
            var isUpload = false;
            if (_target!=null &&((_target.IsMine && _target.IsSync) || !_target.IsMine))
            {
                if (File.Exists(_target.Content))
                {
                    try
                    {

                        using (FileStream fs = File.OpenRead(imgPath))
                        {
                            string md5 = Util.Helpers.Encrypt.Md5By32(fs);
                            var imgMd5 = $"{md5}{System.IO.Path.GetExtension(imgPath)}";
                            string targetKey = _target.ResourceModel.Key;
                            if (targetKey != imgMd5)
                            {
                                isUpload = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }

            if (imgPath != IMAssets.ImageDeal.DefaultHeadImage && (!File.Exists(imgPath) || !App.ImageFilter.Contains(exten)) || isUpload)
            {
                if (_target.ResourceModel != null && !string.IsNullOrEmpty(_target.ResourceModel.Key))
                {
                    //if (isUploadImg)
                    //    return;
                    Task.Run(async () =>
                    {
                        //isUploadImg = true;
                        var resultFile = await SDKClient.SDKClient.Instance.FindFileResource(_target.ResourceModel.Key);
                        if (!resultFile)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                SetImage(IMAssets.ImageDeal.ImgLoadFail, true);
                            });
                            return;
                        }
                        SDKClient.SDKClient.Instance.DownLoadResource(_target.ResourceModel.Key, _target.ResourceModel.Key, SDKClient.Model.FileType.img, null, (b) =>
                        {

                            if (b)
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    SetImage(imgPath);
                                });
                                return;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(_target.Content) && File.Exists(_target.Content))
                                {
                                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        SetImage(_target.Content);
                                    });
                                }
                                return;
                            }
                        }, _target.MsgKey);
                    });
                    //.ContinueWith(t=> {
                    //    isUploadImg = false;
                    //});
                }

            }
            else
            {
                SetState(false);
                this.img.Visibility = Visibility.Visible;
                this.tbInfo.Text = string.Empty;
                SetImage(imgPath);
            }

        }

        private void SetImage(string imgPath, bool isImgExceed = false)
        {
            bool isCreate;
            ImageScanWindow scan = CreateOrActiveWindow(out isCreate);
            scan.Show();
            //Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            //{
            if (_currentPath != imgPath)
            {
                this.img.Dispose();
                AsyncHandler.CallFuncWithUI(System.Windows.Application.Current.Dispatcher, () =>
                {
                    try
                    {
                        scan.Dispatcher.Invoke(new Action(() =>
                        {
                            scan.loading.Visibility = Visibility.Visible;
                        }));
                        //this.img.Visibility = Visibility.Collapsed;
                        //this.tbInfo.Text = "图片加载中...";
                        //BitmapImage img = new BitmapImage(new Uri(imgPath, UriKind.RelativeOrAbsolute));

                        if (isImgExceed)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                this.img.Visibility = Visibility.Visible;
                                scan.loading.Visibility = Visibility.Collapsed;
                                BitmapImage img = new BitmapImage();
                                img.BeginInit();
                                img.CacheOption = BitmapCacheOption.OnLoad;
                                img.UriSource = new Uri(IMAssets.ImageDeal.ImgLoadFail);
                                img.EndInit();
                                img.Freeze();
                                this.tbInfo.Text = "图片已过期！";
                                this.img.Source = img;
                                //SetProperty(imgPath, scan, img, false);
                                SetState(true);
                            });
                            return null;
                        }
                        else
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();

                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = new MemoryStream(File.ReadAllBytes(imgPath));
                            //bitmap.StreamSource = new MemoryStream(ImageHelper.ImageToBytes(path));
                            bitmap.EndInit();
                            bitmap.Freeze();
                            return bitmap;
                        }
                    }
                    catch
                    {
                        try
                        {

                            BitmapImage img = new BitmapImage();
                            img.BeginInit();
                            img.CacheOption = BitmapCacheOption.OnLoad;
                            if (imgPath == IMAssets.ImageDeal.DefaultHeadImage)
                                img.UriSource = new Uri(imgPath);
                            else
                                img.StreamSource = new MemoryStream(ImageHelper.ImageToBytes(imgPath));
                            img.EndInit();
                            img.Freeze();
                            return img;
                        }
                        catch
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                this.img.Visibility = Visibility.Collapsed;
                                scan.loading.Visibility = Visibility.Collapsed;
                                IMClient.Helper.MessageHelper.LoadImgContent(_target);
                                _currentPath = imgPath;
                                this.Title = this.tbName.Text = System.IO.Path.GetFileName(imgPath);
                                this.tbInfo.Text = "加载失败！";
                                SetState(true);
                            });
                            return null;
                        }
                    }
                },
              (ex, data) =>
              {
                  if (data == null) return;
                  scan.Dispatcher.Invoke(new Action(() =>
                  {
                      loading.Visibility = Visibility.Collapsed;
                  }));
                  SetProperty(imgPath, scan, data, false);
                  _currentPath = imgPath;
                  this.Title = this.tbName.Text = System.IO.Path.GetFileName(imgPath);
                  //Task.Delay(30).ContinueWith(task =>
                  //{
                  //    scan.Dispatcher.Invoke(new Action(() =>
                  //    {
                  if (scan.WindowState == WindowState.Minimized)
                  {
                      scan.WindowState = WindowState.Normal;
                  }
                  SetState(true);
                  scan.Activate();
                  //}));
                  //});
                  //}));
              });
            }
            else
            {
                SetState(true);
            }
        }

        private static void SetProperty(string imgPath, ImageScanWindow scan, ImageSource imgSouce, bool isResize = true)
        {
            if (imgSouce != null && imgSouce.Width > 0 && imgSouce.Height > 0)
            {
                _originalImageWidth = imgSouce.Width;
                _originalImageHeight = imgSouce.Height;
                if (_originalImageHeight > 450 || _originalImageWidth > 650)
                {
                    double w = imgSouce.Width * 1.1, h = imgSouce.Height * 1.1;

                    //scan.Width = w + 60; scan.Height = h + 60;

                    w = Math.Max(w, PrimaryScreen.MaxAreaWidth * 0.4);
                    w = Math.Min(w, PrimaryScreen.MaxAreaWidth * 0.8);

                    h = Math.Max(h, PrimaryScreen.MaxAreaHeight * 0.4);
                    h = Math.Min(h, PrimaryScreen.MaxAreaHeight * 0.8);

                    if (_originalImageWidth > 650)
                        scan.img.Width = w;
                    else
                    {
                        scan.img.Width = _originalImageWidth;
                    }
                    if (_originalImageHeight > 450)
                        scan.img.Height = h;
                    else
                        scan.img.Height = _originalImageHeight;

                    scan.Width = scan.img.Width + 30;
                    scan.Height = scan.img.Height + 96;

                    scan.Left = (PrimaryScreen.MaxAreaWidth - scan.Width) / 2;
                    scan.Top = (PrimaryScreen.MaxAreaHeight - scan.Height) / 2;
                }
                else
                {

                    scan.img.Width = _originalImageWidth;
                    scan.img.Height = _originalImageHeight;
                    scan.Width = 650;
                    scan.Height = _originalImageHeight + 90;
                    scan.Left = (PrimaryScreen.MaxAreaWidth - scan.Width) * 0.5;
                    scan.Top = (PrimaryScreen.MaxAreaHeight - scan.Height) * 0.5;
                }
                //scan.img.Source = imgSouce;
                //scan.Show();
                //if (!File.Exists(imgPath)) return;
                if (imgPath.ToUpper().EndsWith(".GIF"))
                {
                    Task.Delay(10).ContinueWith(task =>
                    {
                        scan.Dispatcher.Invoke(new Action(() =>
                        {
                            scan.img.FilePath = imgPath;
                        }));
                    });
                }
                else
                {
                    scan.img.FilePath = imgPath;
                }
            }
        }

        public static ImageScanWindow CreateOrActiveWindow(out bool isCreate)
        {
            if (_current != null)
            {
                isCreate = false;
                return _current;
            }
            else
            {
                isCreate = true;
                return _current = new ImageScanWindow();
            }
        }

        private void gridView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                Scale *= 1.1;
                Scale = Scale > 20 ? 20 : Scale;
            }
            else
            {
                Scale *= 0.9;

                Scale = Scale < 0.1 ? 0.1 : Scale;
                translate.X = translate.Y = 0;
            }
            this.scale.ScaleX = this.scale.ScaleY = Scale;
        }

        private void Grid_Click(object sender, RoutedEventArgs e)
        {
            ButtonBase btn = e.Source as ButtonBase;
            if (btn != null)
            {
                switch (btn.Uid)
                {
                    case "Min":
                        this.WindowState = WindowState.Minimized;
                        break;
                    case "Max":
                        if (this.btnMax.IsChecked == true)
                        {
                            this.WindowState = WindowState.Maximized;
                            this.BorderThickness = new Thickness(0);
                        }
                        else
                        {
                            this.WindowState = WindowState.Normal;
                            this.BorderThickness = new Thickness(10);
                        }
                        break;
                    case "Close":
                        this.Close();
                        break;
                    case "Expand":
                        Scale *= 1.1;
                        Scale = Scale > 20 ? 20 : Scale;
                        this.scale.ScaleX = this.scale.ScaleY = Scale;
                        translate.X = translate.Y = 0;
                        break;
                    case "Shrink":
                        Scale *= 0.9;
                        Scale = Scale < 0.1 ? 0.1 : Scale;
                        this.scale.ScaleX = this.scale.ScaleY = Scale;

                        translate.X = translate.Y = 0;
                        break;
                    case "Rotate":
                        double angle = this.rotate.Angle + 90;
                        this.rotate.Angle = angle % 360;
                        translate.X = translate.Y = 0;

                        SetScale(this);
                        break;
                    case "Save":

                        Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                        //sfd.Filter = "图像文件(*.jpg)|*.jpg|图像文件(*.bmp)|*.bmp";
                        sfd.DefaultExt = System.IO.Path.GetExtension(this.img.FilePath);// 
                        string gifFilter = "|图像文件(*.gif)|*.gif";
                        string extension = sfd.DefaultExt;
                        sfd.Filter = string.Format("图像文件(*.{0})|*.{0}", extension);
                        if (this.img.FilePath.ToUpper().EndsWith(".GIF"))
                        {
                            sfd.Filter = sfd.Filter + gifFilter;
                            sfd.FilterIndex = 3;
                            extension = "gif";
                        }
                        sfd.FileName = string.Format("IM图片{0}.{1}", DateTime.Now.ToString("yyyyMMddHHmmss"), extension);
                        if (sfd.ShowDialog() == true)
                        {
                            if (File.Exists(this.img.FilePath))
                            {
                                File.Copy(this.img.FilePath, sfd.FileName, true);
                            }
                            else
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                                    //BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                                    BitmapImage img = new BitmapImage(new Uri(this.img.FilePath, UriKind.RelativeOrAbsolute));
                                    encoder.Frames.Add(BitmapFrame.Create(img as BitmapSource));
                                    encoder.Save(ms);
                                    ms.Seek(0, SeekOrigin.Begin);

                                    var bmpArray = ms.ToArray();

                                    File.WriteAllBytes(sfd.FileName, bmpArray);
                                    ms.Close();
                                }
                            }
                        }
                        break;
                }
            }
        }

        static void SetScale(ImageScanWindow scan)
        {
            double scaleX = 1d;
            if (scan.rotate.Angle == 90 || scan.rotate.Angle == 270)
            {
                if (_originalImageWidth > scan.gridScan.ActualHeight)
                {
                    scaleX = Math.Round(scan.gridScan.ActualHeight / _originalImageWidth, 1);
                }
                else if (_originalImageHeight > scan.gridScan.ActualWidth)
                {
                    scaleX = Math.Round(scan.gridScan.ActualWidth / _originalImageHeight, 1);
                }
            }
            else
            {
                if (_originalImageWidth > scan.gridScan.ActualWidth)
                {
                    scaleX = Math.Round(scan.gridScan.ActualWidth / _originalImageWidth, 1);
                }
                else if (_originalImageHeight > scan.gridScan.ActualHeight)
                {
                    scaleX = Math.Round(scan.gridScan.ActualHeight / _originalImageHeight, 1);
                }
            }

            scan.Scale = scaleX;
            scan.Scale = scan.Scale < 0.1 ? 0.1 : scan.Scale;
            scan.scale.ScaleX = scan.scale.ScaleY = scan.Scale;
        }

        private void btnPerImg_Click(object sender, RoutedEventArgs e)
        {
            if (this.btnPerImg.IsTabStop && this._sources != null)
            {
                int index = this._sources.IndexOf(_target) - 1;

                bool hasImg = false;
                if (index == 0)
                {
                    MessageModel msg = this._sources[0];
                    if (File.Exists(msg.Content))
                    {
                        string exten = System.IO.Path.GetExtension(msg.Content).ToLower();
                        if (App.ImageFilter.Contains(exten))
                        {
                            hasImg = true;
                            this._target = msg;
                        }
                    }
                }
                else
                {
                    for (int i = index; i > 0; i--)
                    {
                        MessageModel msg = this._sources[i];
                        if (File.Exists(msg.Content))
                        {
                            string exten = System.IO.Path.GetExtension(msg.Content).ToLower();
                            if (App.ImageFilter.Contains(exten))
                            {
                                hasImg = true;
                                this._target = msg;
                                break;
                            }
                        }
                    }
                }

                if (hasImg)
                {
                    ShowCurrent();
                }
                else
                {
                    this.txtLimit.Text = "已经是第一张图片了";
                    (this.Resources["ShowLimit"] as Storyboard).Begin();
                }
            }
        }

        private void btnNextImg_Click(object sender, RoutedEventArgs e)
        {
            if (this.btnNextImg.IsTabStop && this._sources != null)
            {
                int index = this._sources.IndexOf(_target) + 1;

                bool hasImg = false;

                for (int i = index; i < this._sources.Count; i++)
                {
                    MessageModel msg = this._sources[i];
                    if (File.Exists(msg.Content))
                    {
                        string exten = System.IO.Path.GetExtension(msg.Content).ToLower();
                        if (App.ImageFilter.Contains(exten))
                        {
                            hasImg = true;
                            this._target = msg;
                            break;
                        }
                    }
                }

                if (hasImg)
                {
                    ShowCurrent();
                }
                else
                {
                    this.txtLimit.Text = "已经是最后一张图片了";
                    (this.Resources["ShowLimit"] as Storyboard).Begin();
                }
            }
        }
    }
}
