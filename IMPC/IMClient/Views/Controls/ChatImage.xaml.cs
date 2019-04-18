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
using IMClient.ViewModels;
using IMModels;
using IMClient.Helper;
using IMClient.Views.Panels;
using System.Threading;

namespace IMClient.Views.Controls
{
    /// <summary>
    /// 聊天内容图片控件
    /// </summary>
    public partial class ChatImage : UserControl
    {
        static Size _maxSize = new Size(240, 170);
        static Size _minSize = new Size(100, 80);

        MenuItem _miWithDraw;
        MenuItem _miCancel;
        public event Action<MessageModel> ReSend;
        private MessageModel messageModel;
        private string _previewImagePath = string.Empty;
        public ChatImage() : this(string.Empty) { }

        public ChatImage(string imgPath, string previewImagePath = null)
        {
            InitializeComponent();

            this.ImagePath = imgPath;
            _previewImagePath = previewImagePath;
            this.gridLayout.ContextMenu = null;

            _miWithDraw = new MenuItem();
            _miWithDraw.Header = "撤回";
            _miWithDraw.Uid = "WITHDRAWimg";

            _miCancel = new MenuItem();
            _miCancel.Header = "取消";
            _miCancel.Uid = "CANCELimg";
            this.AddHandler(ChatImage.PreviewMouseLeftButtonUpEvent, new RoutedEventHandler(this.OnPreviewMouseLeftButtonUp), true);
            this.AddHandler(ChatImage.PreviewMouseLeftButtonDownEvent, new RoutedEventHandler(OnPreviewMouseLeftButtonDown), true);
            this.MouseEnter += ChatImage_MouseEnter;
            this.Loaded += ChatImage_Loaded;
        }


        private void ChatImage_Loaded(object sender, RoutedEventArgs e)
        {
            messageModel = this.DataContext as MessageModel;
        }

        private void ChatImage_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        public bool IsMouseDown;
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (messageModel != null && messageModel.MessageState == MessageStates.ExpiredFile)
                return;
            if (!File.Exists(this.ImagePath))
            {
                if (messageModel != null)
                {
                    if (messageModel.MsgType == MessageType.img)
                    {
                        this.Loading();

                        IMClient.Helper.MessageHelper.LoadImgContent(messageModel);
                    }
                    else if (messageModel.MsgType == MessageType.file)
                    {
                        string filePath = messageModel.Content;
                        messageModel.Content = null;
                        messageModel.Content = filePath;
                    }
                }
            }
            base.OnPreviewMouseDown(e);
        }

        protected void OnPreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            if (messageModel != null && messageModel.MessageState == MessageStates.ExpiredFile)
            {
                e.Handled = false;
                return;
            }
            IsMouseDown = true;
            e.Handled = true;
            //base.OnPreviewMouseLeftButtonDown(e);
        }
        //protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        //{
        //    base.OnPreviewMouseUp(e);
        //}
        protected void OnPreviewMouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            if (messageModel != null && messageModel.MessageState == MessageStates.ExpiredFile)
                return;
            if (e.OriginalSource == pathResend)
            {
                this.pathResend.Visibility = Visibility.Hidden;
                //if (this.DataContext is MessageModel msg)
                //{
                if (messageModel != null)
                {
                    if (messageModel.IsMine && !messageModel.IsSync && messageModel.MessageState != MessageStates.Warn)
                    {
                        AppData.MainMV.ChatListVM.SelectedItem.ReSend(messageModel);
                    }
                    else
                    {
                        if (this.State != MessageStates.Loading)
                        {
                            this.Loading();
                            IMClient.Helper.MessageHelper.LoadImgContent(messageModel);
                        }
                    }
                }
                //}
            }
            else
            {
                if (IsMouseDown)
                {
                    Views.ChildWindows.AppendWindow.AutoClose = false;
                    this.ScanImage();
                    Views.ChildWindows.AppendWindow.AutoClose = true;
                }
            }
            IsMouseDown = false;
            //base.OnPreviewMouseLeftButtonUp(e);
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
            if (string.IsNullOrEmpty(target.ImagePath) || !System.IO.File.Exists(target.ImagePath))
            {
                return;
            }
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
                    if (target.DataContext is MessageModel msg)
                    {
                        //if (target.messageModel != null)
                        //{
                        if (msg.IsMine)
                        {
                            if (msg.IsSync)
                            {
                                target.pathResend.Visibility = Visibility.Visible;
                                Grid.SetColumn(target.pathResend, 0);
                                target.pathResend.Margin = new Thickness();
                                target.pathResend.ToolTip = "重新接收";
                                target.gridFail.Visibility = Visibility.Visible;
                            }
                        }
                        else
                        {
                            target.pathResend.Visibility = Visibility.Visible;
                            Grid.SetColumn(target.pathResend, 2);
                            target.pathResend.Margin = new Thickness(5);
                            target.pathResend.ToolTip = "重新接收";
                            target.gridFail.Visibility = Visibility.Visible;
                        }
                    }
                    //}

                    target.aniLoading.Stop();
                    target.gridLayout.Children.Remove(target.gridLoading);
                    break;
                case MessageStates.Success:
                    target.LoadImg();
                    break;
                case MessageStates.Loading:
                    target.Loading();
                    break;
                case MessageStates.Warn:
                    if (target.gridLayout.Children.Contains(target.gridLoading))
                        target.gridLayout.Children.Remove(target.gridLoading);
                    break;
                case MessageStates.ExpiredFile:
                    target.gridLoading.Visibility = Visibility.Collapsed;
                    target.gridFail.Visibility = Visibility.Visible;
                    target.tbLoadFail.Visibility = Visibility.Visible;
                    target.gridFail.ToolTip = "图片已过期";
                    target.tbLoadFail.Text = "图片已过期";
                    break;
            }
        }


        private void LoadImg()
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                if (this.ImagePath != null &&
                    (this.ImagePath.Contains("wuyf") || this.ImagePath.Contains("lanwj") || this.ImagePath.Contains("chenpj")))
                {

                }
                if (string.IsNullOrEmpty(this.ImagePath) || !System.IO.File.Exists(this.ImagePath))
                {
                    return;
                }

                try
                {
                    //var source =  new BitmapImage(new Uri(this.ImagePath, UriKind.RelativeOrAbsolute))
                    //{
                    //    CacheOption = BitmapCacheOption.OnLoad,
                    //};

                    BitmapImage source = null;
                    if (!string.IsNullOrEmpty(_previewImagePath) && File.Exists(_previewImagePath))
                    {
                        source = GetBitmapImage(_previewImagePath);
                    }
                    else
                        source = GetBitmapImage(this.ImagePath);

                    double w = source.Width, h = source.Height;
                    if (w > _maxSize.Width || h > _maxSize.Height)
                    {
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
                        if (w < _minSize.Width)
                        {
                            h = h * _minSize.Width / w;
                            w = _minSize.Width;
                        }
                        if (h < _minSize.Height)
                        {
                            w = w * _minSize.Height / h;
                            h = _minSize.Height;
                        }
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

                        if (messageModel != null && File.Exists(this.ImagePath) && (source.Width > w || source.Height > h))
                        {
                            this.img.Source = Helper.WindowsThumbnailProvider.GetFileThumbnail(this.ImagePath, (int)w, (int)h, Helper.ThumbnailOptions.ThumbnailOnly);
                        }
                        else
                        {
                            if (w > _maxSize.Width)
                            {
                                this.img.Source = Helper.WindowsThumbnailProvider.GetFileThumbnail(this.ImagePath, (int)_maxSize.Width, (int)h, Helper.ThumbnailOptions.ThumbnailOnly);
                            }
                            else if (h > _maxSize.Height)
                            {
                                this.img.Source = Helper.WindowsThumbnailProvider.GetFileThumbnail(this.ImagePath, (int)w, (int)_maxSize.Height, Helper.ThumbnailOptions.ThumbnailOnly);
                            }
                            else
                            {
                                this.img.Source = source;
                            }
                        }
                    }
                    this.Uid = ViewModels.AppData.FlagImage + this.ImagePath;
                    aniLoading.Stop();
                    this.gridLayout.Children.Remove(this.gridLoading);
                    this.gridLayout.ContextMenu = this.HasContexMenu ? this.menu : null;
                    this.gridFail.Visibility = Visibility.Collapsed;
                    this.pathResend.Visibility = Visibility.Collapsed;// Visibility.Hidden;
                    this.tbLoadFail.Visibility = Visibility.Collapsed;
                    //MessageModel msModel = this.DataContext as MessageModel;
                    //var ChatViewModel = AppData.MainMV.ChatListVM.SelectedItem;
                    //if (ChatViewModel.View is ChatView chatView)
                    //{
                    //    chatView.chatBox.ScallToCurrent(messageModel);
                    //}
                }
                catch (Exception ex)
                {
                    App.Logger.Error(ex.StackTrace, this.ImagePath);
                    this.imgGif.FilePath = null;
                    this.img.Source = null;
                    this.gridFail.Visibility = Visibility.Visible;

                    this.aniLoading.Stop();
                    this.gridLayout.Children.Remove(this.gridLoading);

                    this.tbLoadFail.Visibility = Visibility.Visible;
                    this.gridFail.ToolTip = "加载失败";
                    //this._sbLoading.Begin();
                    //this.gridLoading.Visibility = Visibility.Visible;
                }
            }));
        }

        public void Loading()
        {
            this.tbLoadFail.Visibility = Visibility.Collapsed;
            this.gridFail.Visibility = Visibility.Collapsed;
            this.gridLayout.Children.Remove(this.gridLoading);
            this.gridLayout.Children.Add(this.gridLoading);
            this.aniLoading.Begin();
        }
        private static object myimg_lock = new object();
        private BitmapImage GetBitmapImage(string path)
        {
            BitmapImage bitmap;
            try
            {
                bitmap = new BitmapImage();
                bitmap.BeginInit();

                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = new MemoryStream(File.ReadAllBytes(path));
                //bitmap.StreamSource = new MemoryStream(ImageHelper.ImageToBytes(path));
                bitmap.EndInit();
                bitmap.Freeze();
                this.aniLoading.Stop();
            }
            catch
            {
                try
                {
                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    //bitmap.StreamSource = new MemoryStream(File.ReadAllBytes(path));
                    bitmap.StreamSource = new MemoryStream(ImageHelper.ImageToBytes(path));
                    bitmap.EndInit();
                    bitmap.Freeze();
                    this.aniLoading.Stop();
                }
                catch
                {
                    bitmap = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
                    this.aniLoading.Stop();
                }
            }
            return bitmap;
        }



        public void ScanImage()
        {
            if (messageModel != null)
            {

                if (File.Exists(this.ImagePath))
                {
                    IList<MessageModel> list;
                    if (this.HasContexMenu)
                    {
                        //IMClient.Helper.MessageHelper.LoadImgContent(msg);

                        //var current = AppData.MainMV.ChatListVM.SelectedItem;

                        //var list = current.Chat.Messages.Where(info => info.MsgType == MessageType.img).ToList();
                        //int index = list.IndexOf(msg);

                        //string[] paths = list.Select(info => info.Content).ToArray();

                        //ChildWindows.ImageScanWindow.ShowScan(paths, index);

                        list = AppData.MainMV.ChatListVM.SelectedItem.Chat.Messages;

                    }
                    else
                    {
                        list = Panels.ChatHistoryView.HisMsgTarget.Items;
                    }
                    ChildWindows.ImageScanWindow.ShowScan(list, messageModel);
                }
                else
                {
                    IMClient.Helper.MessageHelper.LoadImgContent(messageModel);
                }
            }
        }
        public void DoCopy()
        {
            this.menu.IsOpen = false;
            string imgPath = this.ImagePath;
            if (messageModel?.ResourceModel != null && !string.IsNullOrEmpty(messageModel.ResourceModel.Key))
            {
                string imagePath = System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, messageModel.ResourceModel.Key);
                if (File.Exists(imagePath))
                {
                    FileInfo infoFile = new FileInfo(imagePath);

                    imgPath = imagePath;
                }
                else
                {
                    SDKClient.SDKClient.Instance.DownLoadResource(messageModel.ResourceModel.Key, messageModel.ResourceModel.Key, SDKClient.Model.FileType.img, null, (b) =>
                    {
                        if (b)
                        {
                            imgPath = imagePath;
                        }
                        else
                        {
                            imgPath = this.ImagePath;
                        }
                    }, messageModel.MsgKey);
                }

            }
            //Task.Delay(600).ContinueWith(t =>
            //{
            Thread.Sleep(600);
            CopyToClipboard(imgPath);
            //});


            //IDataObject data = new DataObject(DataFormats.FileDrop, new string[] { this.ImagePath });
            //MemoryStream memo = new MemoryStream(4);
            //byte[] bytes = new byte[] { (byte)(5), 0, 0, 0 };
            //memo.Write(bytes, 0, bytes.Length);
            //data.SetData("ttt", memo);
            //Clipboard.SetDataObject(data);
            //IDataObject dataObj = new DataObject();
            //dataObj.SetData(DataFormats.Bitmap, new System.Drawing.Bitmap(this.ImagePath), true);
            //dataObj.SetData(DataFormats.FileDrop, new string[] { this.ImagePath }, true);
            //dataObj.SetData(DataFormats.StringFormat, this.ImagePath, true);
            //Clipboard.SetDataObject(dataObj);
        }

        private void CopyToClipboard(string imgPath)
        {
            string value = $"<DIV><IMG src=\"file:///{imgPath}\"></DIV> ";
            Helper.ClipboardHelper.CopyToClipboard(value, "");
        }


        private void ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source is MenuItem item && item.DataContext is MessageModel msg)
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
                        string imgPath = this.ImagePath;
                        if (!string.IsNullOrEmpty(messageModel.ResourceModel.Key))
                        {
                            string imagePath = System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, messageModel.ResourceModel.Key);
                            if (File.Exists(imagePath))
                            {
                                if (File.Exists(imagePath))
                                {
                                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                                    dlg.FileName = System.IO.Path.GetFileName(imagePath); //  
                                    dlg.DefaultExt = System.IO.Path.GetExtension(imagePath);//  

                                    dlg.Filter = string.Format("图片(*.{0})|*.{0}", dlg.DefaultExt);//  

                                    dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                                    if (dlg.ShowDialog() == true && imagePath != dlg.FileName)
                                    {
                                        if (File.Exists(dlg.FileName))
                                        {
                                            File.Delete(dlg.FileName);
                                        }
                                        File.Copy(imagePath, dlg.FileName, true);
                                    }
                                }
                            }
                            else
                            {
                                SDKClient.SDKClient.Instance.DownLoadResource(messageModel.ResourceModel.Key, messageModel.ResourceModel.Key, SDKClient.Model.FileType.img, null, (b) =>
                                {
                                    if (b)
                                    {
                                        if (File.Exists(imagePath))
                                        {
                                            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                                            dlg.FileName = System.IO.Path.GetFileName(imagePath); //  
                                            dlg.DefaultExt = System.IO.Path.GetExtension(imagePath);//  

                                            dlg.Filter = string.Format("图片(*.{0})|*.{0}", dlg.DefaultExt);//  

                                            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                                            if (dlg.ShowDialog() == true && imagePath != dlg.FileName)
                                            {
                                                if (File.Exists(dlg.FileName))
                                                {
                                                    File.Delete(dlg.FileName);
                                                }
                                                File.Copy(imagePath, dlg.FileName, true);
                                            }
                                        }
                                        return;
                                    }
                                    else
                                    {
                                        AppData.MainMV.TipMessage = "图片另存为失败";
                                        return;
                                    }
                                }, messageModel.MsgKey);
                            }
                            //imgPath = imagePath;
                        }

                        break;
                    case "DELETEimg":
                        //没有太好的关联方式，目前暂时做成当前窗口（因为只有当前窗口可能删除对应信息）；                         
                        if (chatVM != null)
                        {
                            chatVM.HideMessageCommand.Execute(msg);
                        }
                        break;
                    case "ForwardImg"://转发
                        FowardImagMsg(chatVM.ID, chatVM.IsGroup);
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
            else if (e.Source is MenuItem child && child.DataContext is ChatViewModel chatVM)
            {
                switch (child.Uid)
                {
                    case "OPENimg":
                        ChildWindows.ImageScanWindow.ShowScan(this.ImagePath);
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

                            dlg.Filter = string.Format("图片(*.{0})|*.{0}", dlg.DefaultExt);//  

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
                        ChatView chatView = chatVM.View as ChatView;
                        int index = 0;
                        ChatImage image = null;
                        foreach (var child1 in chatView.msgEditor.richBox.Document.Blocks)
                        {
                            if (child1 is Paragraph ph)
                            {

                                foreach (var child2 in ph.Inlines)
                                {
                                    if (child2 is InlineUIContainer inline)
                                    {
                                        if (inline.Child is ChatImage chatImage)
                                        {
                                            index = ph.Inlines.ToList().IndexOf(child2);
                                        }
                                    }
                                }
                            }
                        }
                        //(chatView.msgEditor.richBox.Document.Blocks.First() as Paragraph).Inlines.ToList().Remove



                        chatView.msgEditor.richBox.Document.Blocks.Clear();
                        break;
                    default:
                        break;
                }

            }

        }
        /// <summary>
        /// 转发
        /// </summary>
        /// <param name="isGroup"></param>
        private void FowardImagMsg(int chatID, bool isGroup)
        {
            AppData.FowardMsg(chatID, messageModel.MsgKey, isGroup);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        private void menu_Opened(object sender, RoutedEventArgs e)
        {
            if (e.Source is ContextMenu item && item.DataContext is MessageModel msg)
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
                    if (msg.MessageState == MessageStates.Success || msg.MessageState == MessageStates.None)
                    {
                        this.Forward.Visibility = Visibility.Visible;
                    }
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
                            if (msg.MessageState == MessageStates.Success || msg.MessageState == MessageStates.None)
                                this.menu.Items.Add(this._miWithDraw);
                        }
                    }
                    //if (msg.MessageState == MessageStates.Fail)
                    //    this._miWithDraw.Visibility = Visibility.Collapsed;
                }


            }
        }
    }
}
