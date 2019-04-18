using System;
using System.Collections.Generic;
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

namespace CSClient.Views.Controls
{
    /// <summary>
    /// ClipWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ClipWindow : Window
    {

        /// <summary>
        /// 屏幕缩放比
        /// </summary>
        private double _screenScale;
        /// <summary>
        /// 系统Dpi
        /// </summary>
        private double _sysDpi;

        System.Drawing.Rectangle _maxRect = PrimaryScreen.FullRect;

        Point _posStart;
        BitmapSource _screenBitmap;

        string _clipSavePath;
        /// <summary>
        /// 是否初选完成
        /// </summary>
        public bool IsIniFrame = false;

        private ClipWindow()
        {
            InitializeComponent();

            _sysDpi = PrimaryScreen.DpiX;
            _screenScale = PrimaryScreen.ScaleX;

            this.gridmagnifier.Visibility = Visibility.Collapsed;


            Left = _maxRect.X;
            Top = _maxRect.Y;
            Width = _maxRect.Width;
            Height = _maxRect.Height;

            this.MouseDoubleClick += ClipWindow_MouseDoubleClick;

            this.PreviewMouseLeftButtonDown += ClipWindow_PreviewMouseLeftButtonDown;
            this.PreviewMouseRightButtonDown += ClipWindow_PreviewMouseRightButtonDown;

            this.stkpMenu.Visibility = Visibility.Collapsed;


            //MessageBox.Show(string.Format("{0}--{1}",rect.Size,_screenBitmap.Size));
            this.PreviewKeyDown += ClipWindow_PreviewKeyDown;

            this.visualBrush.Visual = this.canvasMain;
            this.rectClip.Rect = new Rect();
            this.rectFull.Rect = new Rect(_maxRect.X, _maxRect.Y, _maxRect.Width, _maxRect.Height);

            this.Loaded += ClipWindow_Loaded;
            this.Unloaded += ClipWindow_Unloaded;
        }

        private void ClipWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            this.canvasMain.Children.Clear();
        }

        private void ClipWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.canvasMain.Background = new ImageBrush(_screenBitmap);

            string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Icons/cliparrow.cur");
            Cursor customCursor = new Cursor(iconPath, false);
            this.Cursor = customCursor;
            this.Activate();
        }

        private void ClipWindow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ClipWindow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _posStart = e.GetPosition(this);
            this.PreviewMouseLeftButtonUp += ClipWindow_PreviewMouseLeftButtonUp;
        }


        private void ClipWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
            }
            else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.Z)
            {
                if (this.inkCanvas.Strokes.Count > 0)
                {
                    this.inkCanvas.Strokes.RemoveAt(this.inkCanvas.Strokes.Count - 1);
                    this.rbtnBackout.IsEnabled = this.inkCanvas.Strokes.Count > 0;
                }
            }
        }

        private static ClipWindow clipWIN;
        public static string ShowClip()
        {
            string path = string.Empty;
            if (clipWIN == null)
            {
                clipWIN = new ClipWindow() { Topmost = true };
                clipWIN._screenBitmap = ImageDeal.GetScreenSnapshot(clipWIN._screenScale); ;
                clipWIN.ShowDialog();

                path = clipWIN._clipSavePath;
                clipWIN = null;
            }
            else
            {
                clipWIN.Activate();
            }

            // Rect rect = clipWIN.rectClip.Rect;

            // double wpfDpi = 96;

            // wpfDpi = clipWIN._sysDpi / wpfDpi;
            // double rateDpi = wpfDpi;

            // var clip = new System.Drawing.Rectangle((int)(rect.X * rateDpi), (int)(rect.Y * rateDpi),
            //(int)(rect.Width * rateDpi), (int)(rect.Height * rateDpi));

            // string root = SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath;
            // if (!System.IO.Directory.Exists(root))
            // {
            //     System.IO.Directory.CreateDirectory(root);
            // }
            // clipWIN._clipSavePath = System.IO.Path.Combine(root, $"{Guid.NewGuid().ToString()}.jpg");
            // BitmapSource source = ImageDeal.SaveUiToImageFile(clipWIN.canvasMain, clip, rateDpi, clipWIN._clipSavePath);
            // if (source == null)
            // {
            //     clipWIN._clipSavePath = null;
            // }
            // else
            // {
            //     Clipboard.SetImage(source);
            // }

            return path;
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!IsIniFrame)
            {
                Point pos = e.GetPosition(this);
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    this.rectClip.Rect = new Rect(_posStart, pos);
                }
            }

            UpdateMagnifierLayout();
            base.OnMouseMove(e);
        }

        private void ClipWindow_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.rectClip.Rect.Width > 0 && this.rectClip.Rect.Height > 0)
            {
                this.PreviewMouseLeftButtonUp -= ClipWindow_PreviewMouseLeftButtonUp;
                this.PreviewMouseLeftButtonDown -= ClipWindow_PreviewMouseLeftButtonDown;

                this.bdThumb.Child = new ClipRect(this, this.rectClip.Rect);

                IsIniFrame = true;
                this.stkpMenu.Visibility = Visibility.Visible;
                this.tbUnSelection.Visibility = Visibility.Collapsed;
                this.tbSelection.Visibility = Visibility.Visible;

                this.Cursor = Cursors.Arrow;
            }

        }

        private void ClipWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Adorner || (e.OriginalSource is Border b && !(b.Child is Shape)))
            {
                SaveToImageFile();
            }
        }

        private void SaveToImageFile()
        {
            if (_screenBitmap != null)
            {
                Rect rect = this.rectClip.Rect;

                double wpfDpi = 96;

                wpfDpi = _sysDpi / wpfDpi;
                double rateDpi = wpfDpi;//  _screenScale * wpfDpi;

                var clip = new System.Drawing.Rectangle((int)(rect.X * rateDpi), (int)(rect.Y * rateDpi),
               (int)(rect.Width * rateDpi), (int)(rect.Height * rateDpi));

                string root = SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath;
                if (!System.IO.Directory.Exists(root))
                {
                    System.IO.Directory.CreateDirectory(root);
                }
                _clipSavePath = System.IO.Path.Combine(root, $"{Guid.NewGuid().ToString()}.jpg");

                BitmapSource source = ImageDeal.SaveUiToImageFile(this.canvasMain, clip, rateDpi, _clipSavePath);
                if (source == null)
                {
                    _clipSavePath = null;
                }
                else
                {
                    Clipboard.SetImage(source);
                }
            }
            this.DialogResult = true;
        }

        public void UpdateMagnifierLayout()
        {
            Point mouse = Mouse.GetPosition(this);
            Rect rect = this.rectClip.Rect;

            if (gridmagnifier.Visibility != Visibility.Visible)
            {
                gridmagnifier.Visibility = Visibility.Visible;
            }

            double left = mouse.X, top = mouse.Y;
            if (IsIniFrame)
            {
                left = rect.X;
                top = rect.Y - this.gridmagnifier.ActualHeight;

                if (left + this.gridmagnifier.ActualWidth > _maxRect.Width)
                {
                    left = _maxRect.Width - this.gridmagnifier.ActualWidth;
                }

                if (top < _maxRect.Y)
                {
                    top = _maxRect.Y;
                }

                double mLeft = rect.X + rect.Width - (this.stkpMenu.Children.Count - 1) * 28,
                       mTop = rect.Y + rect.Height + 5;
                mLeft = _maxRect.X > mLeft ? _maxRect.X : mLeft;

                double maxY = _maxRect.Y + _maxRect.Height - 28;
                mTop = maxY < mTop ? maxY : mTop;

                Canvas.SetLeft(this.stkpMenu, mLeft);
                Canvas.SetTop(this.stkpMenu, mTop);
            }
            else
            {
                left = mouse.X + 20;
                top = mouse.Y + 20;
                if (left + this.gridmagnifier.ActualWidth > _maxRect.Width)
                {
                    left = mouse.X - this.gridmagnifier.ActualWidth;
                }

                if (top + this.gridmagnifier.ActualHeight > _maxRect.Height)
                {
                    top = mouse.Y - this.gridmagnifier.ActualHeight;
                }
            }
            Canvas.SetLeft(gridmagnifier, left);
            Canvas.SetTop(gridmagnifier, top);

            if (rect.Width > 0 || rect.Height > 0)
            {
                this.runSize.Text = this.runSize0.Text = string.Format("区域大小:{0:0}×{1:0}", rect.Width, rect.Height);
            }
            else
            {
                this.runSize.Text = this.runSize0.Text = string.Empty;
            }
            //SolidColorBrush color = GetPixelColor(mouse);
            //this.runRGB.Text = string.Format("RGB:({0}, {1}, {2})", color.Color.R, color.Color.G, color.Color.B);

            Rect viewBox = this.visualBrush.Viewbox;
            double xoffset = viewBox.Width / 2.0;
            double yoffset = viewBox.Height / 2.0;
            viewBox.X = mouse.X - xoffset;
            viewBox.Y = mouse.Y - yoffset;
            this.visualBrush.Viewbox = viewBox;
        }

        #region 获取RGB 相关

        [System.Runtime.InteropServices.DllImport("gdi32")]
        private static extern int GetPixel(IntPtr hdc, int nXPos, int nYPos);
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern IntPtr GetWindowDC(IntPtr hwnd);
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        private SolidColorBrush GetPixelColor(Point point)
        {
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;

            IntPtr lDC = GetWindowDC(handle);
            int intColor = GetPixel(lDC, (int)point.X, (int)point.Y);
            ReleaseDC(handle, lDC);
            byte b = (byte)((intColor >> 0x10) & 0xffL);
            byte g = (byte)((intColor >> 8) & 0xffL);
            byte r = (byte)(intColor & 0xffL);
            Color color = Color.FromRgb(r, g, b);
            return new SolidColorBrush(color);
        }

        #endregion

        private void stkpMenu_Click(object sender, RoutedEventArgs e)
        {
            List<string> list = new List<string>();

            Color c;
            ButtonBase button = e.Source as ButtonBase;
            switch (button.Uid)
            {
                case "Pen":
                    this.bdThumb.IsHitTestVisible = false;
                    this.inkCanvas.Visibility = Visibility.Visible;
                    break;
                case "Save":
                    Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                    sfd.Filter = "JPEG (*.jpg;*.jpeg)|*.jpg|BMP (*.bmp)|*.bmp|PNG (*.png)|*.png";
                    sfd.FileName = string.Format("IM截图{0}.jpg", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    sfd.FilterIndex = 1;
                    sfd.RestoreDirectory = true;

                    if (sfd.ShowDialog() == true)
                    {
                        Rect clip = this.rectClip.Rect;

                        double wpfDpi = 96;

                        wpfDpi = _sysDpi / wpfDpi;
                        double rateDpi = _screenScale * wpfDpi;

                        var sourceRect = new System.Drawing.Rectangle((int)(clip.X * rateDpi), (int)(clip.Y * rateDpi),
                       (int)(clip.Width * rateDpi), (int)(clip.Height * rateDpi));

                        BitmapSource source = ImageDeal.SaveUiToImageFile(this.canvasMain, sourceRect, rateDpi, sfd.FileName);
                        if (source != null)
                        {
                            Clipboard.SetImage(source);
                            this.DialogResult = false;
                        }
                    }
                    break;
                case "Backout":
                    if (this.inkCanvas.Strokes.Count > 0)
                    {
                        this.inkCanvas.Strokes.RemoveAt(this.inkCanvas.Strokes.Count - 1);
                        this.rbtnBackout.IsEnabled = this.inkCanvas.Strokes.Count > 0;
                    }
                    break;
                case "Cancel":
                    this.DialogResult = false;
                    break;
                case "OK":
                    SaveToImageFile();
                    break;
            }
        }

        private void listSizes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int value = (int)this.listSizes.SelectedItem;
            this.inkCanvas.DefaultDrawingAttributes.Width = this.inkCanvas.DefaultDrawingAttributes.Height = value;
        }

        private void listColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SolidColorBrush value = (SolidColorBrush)this.listColors.SelectedItem;
            this.inkCanvas.DefaultDrawingAttributes.Color = value.Color;
        }

        private void inkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            this.rbtnBackout.IsEnabled = this.inkCanvas.Strokes.Count > 0;
        }
    }
}
