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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CSClient.Helper;

namespace CSClient.Views.ChildWindows
{
    /// <summary>
    /// ImageScanWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ImageScanWindow : Window
    {
        public static Dictionary<string, ImageScanWindow> HistoryWIN = new Dictionary<string, ImageScanWindow>();

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

        string _key;

        static double _originalImageWidth;
        static double _originalImageHeight;

        private ImageScanWindow()
        {
            InitializeComponent();

            this.MaxHeight = PrimaryScreen.MaxAreaHeight;
            this.MaxWidth = PrimaryScreen.MaxAreaWidth;

            this.gridView.MouseWheel += gridView_MouseWheel;
            this.MouseLeftButtonDown += ImageScanWindow_MouseLeftButtonDown;


            //this.Topmost = true;
            //this.Owner = App.Current.MainWindow;
            this.PreviewKeyDown += ImageScanWindow_PreviewKeyDown;
            this.thumb.DragDelta += Thumb_DragDelta;
            this.Closed += ImageScanWindow_Closed;
            this.Loaded += ImageScanWindow_Loaded;
            this.StateChanged += ImageScanWindow_StateChanged;
            ViewModels.MainViewModel.CloseAppend?.Invoke();
        }

        private void ImageScanWindow_StateChanged(object sender, EventArgs e)
        {
            SetScale(this);
        }

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
            this.PreviewKeyDown -= ImageScanWindow_PreviewKeyDown;
            this.thumb.DragDelta -= Thumb_DragDelta;
            this.Closed -= ImageScanWindow_Closed;
            this.Loaded -= ImageScanWindow_Loaded;
            HistoryWIN.Remove(_key);

            Views.ChildWindows.AppendWindow.AutoClose = false;
            App.Current.MainWindow.Activate();
            Views.ChildWindows.AppendWindow.AutoClose = true;
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

            ImageScanWindow scan = CreateOrActiveWindow(img.ToString(), out isCreate);

            if (isCreate)
            {
                SetProperty(img.ToString(), scan, img);
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
            if (!System.IO.File.Exists(imgPath))
            {
                return;
            }
            bool isCreate;
            ImageScanWindow scan = CreateOrActiveWindow(imgPath, out isCreate);

            if (isCreate)
            {
                BitmapImage img = new BitmapImage(new Uri(imgPath, UriKind.RelativeOrAbsolute));
                SetProperty(imgPath, scan, img);
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

        private static void SetProperty(string imgPath, ImageScanWindow scan, ImageSource imgSouce)
        {
            if (imgSouce != null && imgSouce.Width > 0 && imgSouce.Height > 0)
            {
                _originalImageWidth = imgSouce.Width;
                _originalImageHeight = imgSouce.Height;

                double w = imgSouce.Width * 1.1, h = imgSouce.Height * 1.1;

                //scan.Width = w + 60; scan.Height = h + 60;

                w = Math.Max(w, PrimaryScreen.MaxAreaWidth * 0.2);
                w = Math.Min(w, PrimaryScreen.MaxAreaWidth * 0.8);

                h = Math.Max(h, PrimaryScreen.MaxAreaHeight * 0.2);
                h = Math.Min(h, PrimaryScreen.MaxAreaHeight * 0.8);

                scan.img.Width = w;
                scan.img.Height = h;

                scan.Width = w + 20;
                scan.Height = h + 86;

                //scan.img.Source = imgSouce;
                scan.Show();

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

                HistoryWIN.Add(scan._key = imgPath, scan);
            }
        }

        private static ImageScanWindow CreateOrActiveWindow(string key, out bool isCreate)
        {
            if (HistoryWIN.Keys.Contains(key))
            {
                isCreate = false;
                return HistoryWIN[key];
            }
            else
            {
                isCreate = true;
                return new ImageScanWindow();
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
                        sfd.Filter = "图像文件(*.jpg)|*.jpg|图像文件(*.bmp)|*.bmp";

                        string gifFilter = "|图像文件(*.gif)|*.gif";
                        string extension = "jpg";
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
    }
}
