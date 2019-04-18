using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IMModels;

namespace CSClient.Views.Controls
{
    /// <summary>
    /// 聊天内容图片控件
    /// </summary>
    public partial class ChatImage : UserControl
    {
        Size _maxSize = new Size(320, 180);
        MenuItem _miWithDraw;
        MenuItem _miCancel;
        public event Action<MessageModel> ReSend;
        public ChatImage() : this(string.Empty, Size.Empty) { }

        public ChatImage(string imgPath, Size size)
        {
            InitializeComponent();

            if (!size.IsEmpty)
            {
                _maxSize = size;
            }
            
            this.ImagePath = imgPath;

            this.gridLayout.ContextMenu = null;

            _miWithDraw = new MenuItem();
            _miWithDraw.Header = "撤回";
            _miWithDraw.Uid = "WITHDRAWimg";

            _miCancel = new MenuItem();
            _miCancel.Header = "取消";
            _miCancel.Uid = "CANCELimg";
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (e.OriginalSource == pathResend)
            {
                this.pathResend.Visibility = Visibility.Visible;
                ReSend?.Invoke(this.DataContext as MessageModel);
            }
            else
            {
                Views.ChildWindows.AppendWindow.AutoClose = false;
                this.ScanImage();
                Views.ChildWindows.AppendWindow.AutoClose = true;
            }


            base.OnPreviewMouseLeftButtonUp(e);
        }

        public string ImagePath
        {
            get { return (string)GetValue(ImagePathProperty); }
            set { SetValue(ImagePathProperty, value); }
        }

        private bool _hasContextMenu = true;

        public bool HasContexMenu
        {
            get { return _hasContextMenu; }
            set
            {
                _hasContextMenu = value;
                this.gridLayout.ContextMenu = value ? this.menu : null;
            }
        }

        public static readonly DependencyProperty ImagePathProperty =
            DependencyProperty.Register("ImagePath", typeof(string), typeof(ChatImage), new PropertyMetadata(OnImagePathPropertyChanged));

        private static void OnImagePathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ChatImage target = d as ChatImage;
            target.LoadImg();
        }



        public MessageStates State
        {
            get { return (MessageStates)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MsessageState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(MessageStates), typeof(ChatImage), new PropertyMetadata(OnMsessageStatePropertyChanged));

        private static void OnMsessageStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ChatImage target = d as ChatImage;
            switch (target.State)
            {
                case MessageStates.Fail:
                    if (target.DataContext is MessageModel msg && msg.IsMine)
                    {
                        target.pathResend.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        target.gridFail.Visibility = Visibility.Visible;
                    }
                    target.aniLoading.Stop();
                    target.gridLayout.Children.Remove(target.aniLoading);
                    break;
                case MessageStates.Success:
                    target.LoadImg();
                    break;
            }
        }



        BitmapImage _source;
        private void LoadImg()
        { 
            if ( string.IsNullOrEmpty(this.ImagePath) || !System.IO.File.Exists(this.ImagePath))
            {
                return;
            }
            try
            {
                var source = _source = new BitmapImage(new Uri(this.ImagePath, UriKind.RelativeOrAbsolute))
                {
                    CacheOption = BitmapCacheOption.OnLoad,
                };
                double w = source.Width, h = source.Height;

                if (w > _maxSize.Width)
                {
                    h = _maxSize.Width * h / w;
                    w = _maxSize.Width;
                }

                if (h > _maxSize.Height)
                {
                    w = _maxSize.Height * w / h;
                    h = _maxSize.Height;
                }
                this.Width = w;
                this.Height = h;

                if (this.ImagePath.Trim().ToUpper().EndsWith(".GIF"))
                {
                    this.imgGif.FilePath = this.ImagePath;
                    this.imgGif.Visibility = Visibility.Visible;
                    this.img.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.img.Visibility = Visibility.Visible;
                    this.imgGif.Visibility = Visibility.Collapsed;

                    if (File.Exists(this.ImagePath) && (source.Width > w || source.Height > h))
                    {
                        this.img.Source = Helper.WindowsThumbnailProvider.GetFileThumbnail(this.ImagePath, (int)w, (int)h, Helper.ThumbnailOptions.ThumbnailOnly);
                    }
                    else
                    {
                        this.img.Source = source;
                    }
                }
                this.Uid = ViewModels.AppData.FlagImage + this.ImagePath;
                aniLoading.Stop();
                this.gridLayout.Children.Remove(this.aniLoading);
                this.gridLayout.ContextMenu = this.HasContexMenu ? this.menu : null;
            }
            catch (Exception ex)
            {
                this.imgGif.FilePath = null;
                this.img.Source = null;

                //this._sbLoading.Begin();
                //this.gridLoading.Visibility = Visibility.Visible;
            }
        }

        public void ScanImage()
        {
            if (File.Exists(this.ImagePath))
            {
                ChildWindows.ImageScanWindow.ShowScan(this.ImagePath);
            }
        }

        public void DoCopy()
        {
            this.menu.IsOpen = false;

            string value = $"<DIV><IMG src=\"file:///{this.ImagePath }\"></DIV> ";

            Task.Delay(200).ContinueWith(t =>
            {
                Helper.ClipboardHelper.CopyToClipboard(value, "");

            });
        }

        private void ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source is MenuItem item && item.DataContext is MessageModel msg && msg.MsgType == MessageType.img)
            {
                var chatVM = ViewModels.AppData.MainMV.ChatListVM.SelectedItem;
                switch (item.Uid)
                {
                    case "OPENimg":
                        this.ScanImage();
                        break;
                    case "COPYimg":
                        this.DoCopy();
                        break;
                    case "SAVEas":
                        if (File.Exists(this.ImagePath))
                        {
                            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                            dlg.FileName = System.IO.Path.GetFileName(this.ImagePath); //  
                            dlg.DefaultExt = System.IO.Path.GetExtension(this.ImagePath);//  

                            dlg.Filter = string.Format("图片 (.{0})|*.{0}", dlg.DefaultExt);//  

                            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                            if (dlg.ShowDialog() == true && this.ImagePath != dlg.FileName)
                            {
                                if (File.Exists(dlg.FileName))
                                {
                                    File.Delete(dlg.FileName);
                                }
                                File.Copy(this.ImagePath, dlg.FileName, true);
                            }
                        }
                        break;
                    case "DELETEimg":
                        //没有太好的关联方式，目前暂时做成当前窗口（因为只有当前窗口可能删除对应信息）；                         
                        if (chatVM != null)
                        {
                            chatVM.HideMessageCommand.Execute(msg);
                        }
                        break;
                    case "WITHDRAWimg":
                        if (chatVM != null)
                        {
                            chatVM.SendWithDrawMsg(msg);
                        }
                        break;
                    //case "CANCELimg":
                    //    if (msg.OperateTask != null)
                    //    {
                    //        msg.OperateTask.Cancel();
                    //        msg.OperateTask = null;
                    //    }
                    //    if (chatVM != null)
                    //    {
                    //        chatVM.HideMessageCommand.Execute(msg);
                    //    }
                    //    break;
                        //case "OPENDirectory":

                        //    if (File.Exists(this.FullName))
                        //    {

                        //    }
                        //    string name = this.FullName.Split('\\').LastOrDefault();
                        //    string root = this.FullName.Replace(name, string.Empty);
                        //    if (Directory.Exists(root))
                        //    {
                        //        System.Diagnostics.Process.Start(root);
                        //    }
                        //    break;
                }
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }

        private void menu_Opened(object sender, RoutedEventArgs e)
        {
            if (e.Source is ContextMenu item && item.DataContext is MessageModel msg && msg.MsgType == MessageType.img)
            {
                if (string.IsNullOrEmpty(msg.MsgKey))
                {
                    //if (!this.menu.Items.Contains(this._miCancel))
                    //{
                    //    this.menu.Items.Add(this._miCancel);
                    //}

                    if (this.menu.Items.Contains(this._miWithDraw))
                    {
                        this.menu.Items.Remove(this._miWithDraw);
                    }
                }
                else
                {
                    //if (this.menu.Items.Contains(this._miCancel))
                    //{
                    //    this.menu.Items.Remove(this._miCancel);
                    //}

                    if ((DateTime.Now - msg.SendTime).TotalMinutes >= 2 || !msg.IsMine)
                    {
                        if (this.menu.Items.Contains(this._miWithDraw))
                        {
                            this.menu.Items.Remove(this._miWithDraw);
                        }
                    }
                    else
                    {
                        if (!this.menu.Items.Contains(this._miWithDraw))
                        {
                            this.menu.Items.Add(this._miWithDraw);
                        }
                    }
                }

                
            }
        }
    }
}
