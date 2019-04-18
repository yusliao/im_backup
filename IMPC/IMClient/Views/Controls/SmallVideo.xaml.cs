using DirectShowLib;
using DirectShowLib.DES;
using IMClient.Helper;
using IMClient.ViewModels;
using IMModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Util;

namespace IMClient.Views.Controls
{
    /// <summary>
    /// SmallVideo.xaml 的交互逻辑
    /// </summary>
    public partial class SmallVideo : UserControl
    {
        public event Action<MessageModel> ReSend;

        private string _thumbnailPath;
        private string _duration;
        private int _recordTime;

        MenuItem _miWithDraw;
        MenuItem _miCancel;

        DateTime? _completeTime;
        bool _isMine;
        bool _isSync;
        MessageModel _targetModel;

        /// <summary>
        /// 操作线程任务
        /// </summary>
        System.Threading.CancellationTokenSource _operateTask;
        public ChatViewModel ChatViewModel { get; private set; }
        /// <summary>
        /// 正在操作的文件对象
        /// </summary>
        public static List<SmallVideo> AcioningItems { get; private set; }
        /// <summary>
        /// 所有加载的文件项模型，保障对象唯一性
        /// </summary>
        public static List<MessageModel> FileItems { get; private set; }

        private bool _hasContextMenu = true;

        public bool HasContexMenu
        {
            get { return _hasContextMenu; }
            set
            {
                _hasContextMenu = value;
                this.bdLayout.ContextMenu = value ? this.menu : null;
            }
        }

        /// <summary>
        /// 文件状态
        /// </summary>
        public FileStates FileState
        {
            get { return (FileStates)GetValue(FileStateProperty); }
            set { SetValue(FileStateProperty, value); }
        }

        public string VideoPath
        {
            get { return (string)GetValue(VideoPathProperty); }
            set { SetValue(VideoPathProperty, value); }
        }

        public string VideoPreviewImage
        {
            get { return (string)GetValue(VideoPreviewImageProperty); }
            set { SetValue(VideoPreviewImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VideoPreviewImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VideoPreviewImageProperty =
            DependencyProperty.Register("VideoPreviewImage", typeof(string), typeof(SmallVideo), new PropertyMetadata(OnVideoPreviewImagePropertyChanged));

        public int RecordTime
        {
            get { return (int)GetValue(RecordTimeProperty); }
            set { SetValue(RecordTimeProperty, value); }
        }

        public static readonly DependencyProperty RecordTimeProperty =
            DependencyProperty.Register("RecordTime", typeof(int), typeof(SmallVideo), new PropertyMetadata(OnRecordTimePropertyChanged));

        public static readonly DependencyProperty VideoPathProperty =
            DependencyProperty.Register("VideoPath", typeof(string), typeof(SmallVideo), new PropertyMetadata(OnVideoPathPropertyChanged));

        public static readonly DependencyProperty FileStateProperty =
            DependencyProperty.Register("FileState", typeof(FileStates), typeof(SmallVideo), new PropertyMetadata(OnFileStatePropertyChanged));

        static SmallVideo()
        {
            AcioningItems = new List<SmallVideo>();
            FileItems = new List<MessageModel>();
        }
        bool isContinueLoad;
        public SmallVideo()
        {
            InitializeComponent();
            //HasContexMenu = false;

            _miWithDraw = new MenuItem();
            _miWithDraw.Header = "撤回";
            _miWithDraw.Uid = "WITHDRAWvideo";

            _miCancel = new MenuItem();
            _miCancel.Header = "取消";
            _miCancel.Uid = "CANCELvideo";
            this.Loaded += SmallVideo_Loaded;


        }

        private void SmallVideo_Loaded(object sender, RoutedEventArgs e)
        {
            _targetModel = this.DataContext as MessageModel;
            if (_targetModel != null && _targetModel.ResourceModel != null)
            {
                string targetKey = _targetModel.ResourceModel.PreviewKey;
                if (string.IsNullOrEmpty(_targetModel.ResourceModel.PreviewImagePath))
                {
                    _targetModel.ResourceModel.PreviewImagePath = System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, targetKey);
                }
                if (System.IO.File.Exists(_targetModel.ResourceModel.PreviewImagePath))
                    return;
                Task.Run(async () =>
                {
                    var resultFile = await SDKClient.SDKClient.Instance.FindFileResource(targetKey);
                    if (!resultFile)
                    {
                        return;
                    }

                    SDKClient.SDKClient.Instance.DownLoadResource(targetKey, targetKey, SDKClient.Model.FileType.img, null, (b) =>
                    {
                        if (b)
                        {
                            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                            {
                                var videoPreImg = System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, targetKey);
                                this.VideoPreviewImage = videoPreImg;
                                SetVideoPreviewImage();
                            }));
                        }
                        else
                        {
                            //imgPath = this.ImagePath;
                        }
                    }, _targetModel.MsgKey);
                });
            }
        }

        public SmallVideo(string videoPath, double width, double height, MessageModel msg = null)
            : this()
        {
            this.Width = width;
            this.Height = height;

            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                if (!System.IO.File.Exists(videoPath))
                {
                    if (msg != null)
                        this.DataContext = msg;
                    this.VideoPath = videoPath;
                    if (msg.ResourceModel.DBState == 0)
                    {
                        this.FileState = FileStates.WaitForReceieve;
                    }
                    else
                    {
                        this.FileState = FileStates.Fail;
                    }
                    return;
                }
                if (msg != null && msg.ResourceModel != null)
                {
                    if (System.IO.File.Exists(msg.ResourceModel.PreviewImagePath))
                    {
                        this.DataContext = msg;
                        this.VideoPath = videoPath;
                        this.VideoPreviewImage = msg.ResourceModel.PreviewImagePath;
                        SetVideoPreviewImage();
                        this.FileState = FileStates.Completed;
                        this.Uid = ViewModels.AppData.FlagSmallVideo + videoPath + "|" + _thumbnailPath + "|" + _recordTime.ToString();
                        return;
                    }
                    else if (!string.IsNullOrEmpty(msg.ResourceModel.SmallKey))
                    {
                        var tempImgPath = System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, msg.ResourceModel.SmallKey);
                        if (System.IO.File.Exists(tempImgPath))
                        {

                            this.DataContext = msg;
                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                this.VideoPath = videoPath;
                            }));
                            this.VideoPreviewImage = tempImgPath;
                            SetVideoPreviewImage();
                            this.FileState = FileStates.Completed;
                            this.Uid = ViewModels.AppData.FlagSmallVideo + videoPath + "|" + _thumbnailPath + "|" + _recordTime.ToString();
                            return;
                        }
                    }
                }

                this.VideoPath = videoPath;
                _thumbnailPath = System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, Guid.NewGuid() + ".jpg");
                GetOneFrameImageFromVideo();
                GetVideoDuration();
                CalculateRecordTimeVarDuration();
                this.Uid = ViewModels.AppData.FlagSmallVideo + videoPath + "|" + _thumbnailPath + "|" + _recordTime.ToString();
            }));

        }
        public SmallVideo(string videoPath, string thumbnailPath, double width = 130, double height = 80)
            : this()
        {
            this.Width = width;
            this.Height = height;
            this.VideoPath = videoPath;

            if (!System.IO.File.Exists(videoPath))
            {
                this.FileState = FileStates.Fail;
                return;
            }
            this.VideoPreviewImage = thumbnailPath;
            SetVideoPreviewImage();
            GetVideoDuration();
            CalculateRecordTimeVarDuration();
            this.Uid = ViewModels.AppData.FlagSmallVideo + videoPath + "|" + thumbnailPath + "|" + _recordTime.ToString();
        }

        private static void OnVideoPathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SmallVideo target = d as SmallVideo;
            target.CalFileState();
        }

        private static void OnFileStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SmallVideo target = d as SmallVideo;
            target.SetFileStatesView();
        }

        private static void OnRecordTimePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SmallVideo target = d as SmallVideo;
            target.CalculateDurationVarRecordTime();
        }

        private static void OnVideoPreviewImagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SmallVideo target = d as SmallVideo;
            target.SetVideoPreviewImage();
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (e.OriginalSource == pathResend)
            {
                this.pathResend.Visibility = Visibility.Collapsed;
                if (this.DataContext is MessageModel msg)
                {
                    if (msg.IsMine)
                    {
                        ReSend?.Invoke(this.DataContext as MessageModel);
                    }
                    else
                    {
                        IMClient.Helper.MessageHelper.LoadVideoContent(msg);
                    }
                }
            }
            else if (e.OriginalSource == this.pathPlay || e.OriginalSource == this.borderPlay)
            {
                Views.ChildWindows.AppendWindow.AutoClose = false;
                Play();
                Views.ChildWindows.AppendWindow.AutoClose = true;
            }
            else if (e.OriginalSource == this.imgDownload)
            {
                DownloadVideo();
            }
            else if (e.OriginalSource == this.imgReset)
            {
                if (this._targetModel != null && this._targetModel.IsMine)
                {
                    UploadVideo();
                }
                else
                {
                    DownloadVideo();
                }
            }

            base.OnPreviewMouseLeftButtonUp(e);
        }

        public void Play()
        {
            string videoPath = this.VideoPath;
            if (!System.IO.File.Exists(this.VideoPath))
            {
                if (this.DataContext is MessageModel msg)
                {
                    videoPath = msg.Content;
                    if (!System.IO.File.Exists(videoPath))
                    {
                        string filePath = System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.filePath, videoPath);
                        videoPath = filePath;
                    }
                }
            }

            if (System.IO.File.Exists(videoPath))
            {
                System.Diagnostics.Process.Start(videoPath);
            }
        }

        private void UploadVideo()
        {
            if (_targetModel == null && this.DataContext is MessageModel item)
            {
                _targetModel = item;
            }

            if (this._targetModel.MessageState == MessageStates.Loading || this._targetModel.IsSending)
            {
                AppData.MainMV.TipMessage = "视频正在上传，请稍候";
                return;
            }

            this._targetModel.MessageState = MessageStates.Loading;
            this.gridProgress.Visibility = Visibility.Visible;
            this.borderPlay.Visibility = Visibility.Collapsed;
            this.imgReset.Visibility = Visibility.Collapsed;
            this.imgDownload.Visibility = Visibility.Collapsed;

            ReSend?.Invoke(this.DataContext as MessageModel);
        }

        private void DownloadVideo(string videoPath = "")
        {
            if (_targetModel == null && this.DataContext is MessageModel item)
            {
                _targetModel = item;
            }
            if (!string.IsNullOrEmpty(videoPath))
            {
                _targetModel.Content = videoPath;
                _targetModel.ResourceModel.FullName = videoPath;
            }

            if (AppData.CanInternetAction())
            {

                if (this._targetModel.MessageState == MessageStates.Loading)
                {
                    AppData.MainMV.TipMessage = "视频正在下载，请稍候";
                    return;
                }
                if (this._targetModel.MessageState == MessageStates.Fail || this._targetModel.MessageState == MessageStates.Warn)
                {
                    AppData.MainMV.TipMessage = "视频消息发送失败，无法下载";
                    return;
                }
                this._targetModel.MessageState = MessageStates.Loading;
                this.gridProgress.Visibility = Visibility.Visible;
                this.borderPlay.Visibility = Visibility.Collapsed;
                this.imgReset.Visibility = Visibility.Collapsed;
                this.imgDownload.Visibility = Visibility.Collapsed;
                //IMClient.Helper.MessageHelper.LoadVideoPreviewImage(this._targetModel);
                IMClient.Helper.MessageHelper.LoadVideoContent(this._targetModel, (b) =>
                {
                    if (b)
                    {
                        this._targetModel.MessageState = MessageStates.Success;
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            this.gridProgress.Visibility = Visibility.Collapsed;
                            this.borderPlay.Visibility = Visibility.Visible;
                            this.imgReset.Visibility = Visibility.Collapsed;
                            this.imgDownload.Visibility = Visibility.Collapsed;
                        });
                    }
                    else
                    {
                        this._targetModel.MessageState = MessageStates.Fail;
                        this._targetModel.ResourceModel.FileState = FileStates.None;
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            this.gridProgress.Visibility = Visibility.Collapsed;
                            this.borderPlay.Visibility = Visibility.Collapsed;
                            this.imgReset.Visibility = Visibility.Collapsed;
                            this.imgDownload.Visibility = Visibility.Visible;
                        });
                    }
                });
            }
            else
            {
                this._targetModel.ResourceModel.FileState = FileStates.None;
                //this._targetModel.MessageState = MessageStates.None;
                //this.gridProgress.Visibility = Visibility.Collapsed;
                //this.borderPlay.Visibility = Visibility.Collapsed;
                //this.imgReset.Visibility = Visibility.Collapsed;
                //this.imgDownload.Visibility = Visibility.Visible;
                //IMClient.Helper.MessageHelper.LoadVideoContent(this._targetModel);
            }
        }

        private void CalFileState()
        {
            if (ChatViewModel == null)
            {
                ChatViewModel = AppData.MainMV.ChatListVM.SelectedItem;
            }
            else if (_targetModel?.MessageState == MessageStates.Fail)
            {
                if (_operateTask != null)
                {
                    _operateTask.Cancel();
                }
                AcioningItems.Remove(this);
            }

            if (_targetModel == null && this.DataContext is MessageModel item)
            {
                this.PreviewMouseRightButtonUp -= SmallVideo_PreviewMouseRightButtonUp;
                _targetModel = item;

                var target = FileItems.FirstOrDefault(info => info.MsgKey == item.MsgKey);

                if (target == null)
                {
                    target = item;
                    FileItems.Add(item);
                }

                if (string.IsNullOrEmpty(target.MsgKey))
                {
                    target.MsgKey = Guid.NewGuid().ToString();
                }
                _completeTime = _targetModel.SendTime;
                _isMine = _targetModel.IsMine;
                _isSync = _targetModel.IsSync;
                this.DataContext = _targetModel = target;
                if (_completeTime != null)
                    _targetModel.SendTime = _completeTime.Value;
                _targetModel.IsMine = _isMine;
                _targetModel.IsSync = _isSync;

                //this.gridProgress.Visibility = Visibility.Visible;
                //this.borderPlay.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (_targetModel != null) return;
                this.gridProgress.Visibility = Visibility.Collapsed;
                this.borderPlay.Visibility = Visibility.Visible;
                this.imgReset.Visibility = Visibility.Collapsed;
                this.imgDownload.Visibility = Visibility.Collapsed;

                this.PreviewMouseRightButtonUp += SmallVideo_PreviewMouseRightButtonUp;
                return;
            }
            if (SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden)
                _targetModel.MessageState = MessageStates.Warn;

            //是否发送者
            bool isSender = _targetModel.Sender.ID == AppData.Current.LoginUser.ID;
            string filePath = System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.filePath, this.VideoPath);
            if (System.IO.File.Exists(filePath) || System.IO.File.Exists(this.VideoPath))
            {
                if (isSender) //当前作为发送方
                {
                    if (_targetModel.IsSending) //若是正在发送中
                    {
                        this.FileState = _targetModel.ResourceModel.FileState = FileStates.SendOffline;
                        this.gridProgress.Visibility = Visibility.Visible;
                        this.borderPlay.Visibility = Visibility.Collapsed;
                        this.imgReset.Visibility = Visibility.Collapsed;
                        this.imgDownload.Visibility = Visibility.Collapsed;
                        SendVideo();
                    }
                    else if (_targetModel.ResourceModel.DBState == 0)
                    {
                        this.gridProgress.Visibility = Visibility.Collapsed;
                        this.borderPlay.Visibility = Visibility.Visible;
                        this.imgReset.Visibility = Visibility.Collapsed;
                        this.imgDownload.Visibility = Visibility.Collapsed;
                        //HasContexMenu = true;
                    }
                    else
                    {
                        this.FileState = _targetModel.ResourceModel.FileState = FileStates.Completed;
                        if (_targetModel.IsSync)
                        {
                            _completeTime = _targetModel.SendTime;
                        }
                        else if (_completeTime == null)
                        {
                            _completeTime = DateTime.Now.AddMinutes(-5);
                        }
                    }
                }
                else  //当前用户作为接收方，若文件已有则认为已经成功
                {
                    this.FileState = _targetModel.ResourceModel.FileState = FileStates.Completed;

                    if (_targetModel.IsSync)
                    {
                        _completeTime = _targetModel.SendTime;
                    }
                    else if (_completeTime == null)
                    {
                        _completeTime = DateTime.Now.AddMinutes(-5);
                    }
                }
            }
            else
            {
                if (isSender) //发送者
                {
                    this.FileState = _targetModel.ResourceModel.FileState = FileStates.WaitForReceieve;
                    if (_targetModel.IsSync)
                    {
                        _completeTime = _targetModel.SendTime;
                    }
                    else if (_completeTime == null)
                    {
                        _completeTime = DateTime.Now.AddMinutes(-5);
                    }
                }
                else
                {
                    this.FileState = _targetModel.ResourceModel.FileState = FileStates.WaitForReceieve;
                }
            }
        }

        private void SmallVideo_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void SetFileStatesView()
        {
            switch (this.FileState)
            {
                case FileStates.Fail:
                    if (ChatViewModel != null)
                    {
                        AcioningItems.Remove(this);
                        if (_operateTask != null)
                        {
                            _operateTask.Cancel();
                            _operateTask = null;
                        }
                    }
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        this.gridProgress.Visibility = Visibility.Collapsed;
                        this.borderPlay.Visibility = Visibility.Collapsed;
                        this.imgReset.Visibility = Visibility.Visible;
                        this.imgDownload.Visibility = Visibility.Collapsed;
                    });
                    if (this.DataContext is MessageModel _dataContextMsg)
                    {
                        bool isSender = _dataContextMsg.Sender.ID == AppData.Current.LoginUser.ID;
                        if (isSender)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                this.gridProgress.Visibility = Visibility.Collapsed;
                                this.borderPlay.Visibility = Visibility.Visible;
                                this.imgReset.Visibility = Visibility.Collapsed;
                                this.imgDownload.Visibility = Visibility.Collapsed;
                            });
                        }
                    }
                    //this.pathResend.Visibility = Visibility.Visible;
                    //if (this.DataContext is MessageModel msg && msg.IsMine)
                    //{
                    //    Grid.SetColumn(this.pathResend, 0);
                    //    this.grid.ColumnDefinitions[0].Width = new GridLength(20, GridUnitType.Pixel);
                    //    this.Width = this.Width + 20;
                    //    this.pathResend.Margin = new Thickness();
                    //    this.pathResend.ToolTip = "重新发送";
                    //}
                    //else
                    //{
                    //    Grid.SetColumn(this.pathResend, 2);
                    //    this.grid.ColumnDefinitions[2].Width = new GridLength(20, GridUnitType.Pixel);
                    //    this.Width = this.Width + 20;
                    //    this.pathResend.Margin = new Thickness();
                    //    this.pathResend.ToolTip = "重新接收";
                    //}
                    break;
                case FileStates.WaitForReceieve:
                    //App.Current.Dispatcher.Invoke(() =>
                    //{
                    //    this.gridProgress.Visibility = Visibility.Collapsed;
                    //    this.borderPlay.Visibility = Visibility.Collapsed;
                    //    this.imgReset.Visibility = Visibility.Collapsed;
                    //    this.imgDownload.Visibility = Visibility.Visible;
                    //});
                    break;
                case FileStates.Completed:
                    if (_targetModel == null && this.DataContext is MessageModel item)
                    {
                        _targetModel = item;
                    }
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.VideoPath = _targetModel?.Content;
                    }));

                    if (string.IsNullOrEmpty(_targetModel.ResourceModel.PreviewImagePath) ||
                        !System.IO.File.Exists(_targetModel.ResourceModel.PreviewImagePath))
                    {
                        string targetKey = _targetModel.ResourceModel.PreviewKey;
                        if (!string.IsNullOrEmpty(targetKey))
                        {
                            _targetModel.ResourceModel.PreviewImagePath = _thumbnailPath = System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, targetKey);
                        }
                        else
                        {
                            _targetModel.ResourceModel.PreviewImagePath = _thumbnailPath = System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, Guid.NewGuid() + ".jpg");
                        }
                        GetOneFrameImageFromVideo();
                    }
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        this.gridProgress.Visibility = Visibility.Collapsed;
                        this.borderPlay.Visibility = Visibility.Visible;
                        this.imgReset.Visibility = Visibility.Collapsed;
                        this.imgDownload.Visibility = Visibility.Collapsed;
                    });

                    //HasContexMenu = true;
                    _operateTask = null;
                    AcioningItems.Remove(this);
                    break;
                default:
                    break;
            }
        }

        /// <summary>  
        /// 从视频中截取一帧  
        /// </summary>
        private void GetOneFrameImageFromVideo()
        {
            //ThreadPool.QueueUserWorkItem(m =>
            //{
            FFmpegHelper.GetOneFrameImageFromVideo(this.VideoPath, this._thumbnailPath, () =>
            {
                if (System.IO.File.Exists(_thumbnailPath))
                {
                    App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                     {
                         this.imgFirstFrame.Source = new BitmapImage(new Uri(_thumbnailPath, UriKind.RelativeOrAbsolute))
                         {
                             CacheOption = BitmapCacheOption.OnLoad,
                         };
                     }));
                }
                //});
            });
        }

        /// <summary>
        /// 获取视频时长，格式（00:10）
        /// </summary>
        /// <returns></returns>
        private void GetVideoDuration()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                this._duration = FFmpegHelper.GetVideoDuration(this.VideoPath);
                if (!_duration.Equals("on N-"))
                {
                    this.tbDuration.Text = _duration;
                }
            });
        }

        private void ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source is Control item)
            {
                switch (item.Uid)
                {
                    case "OPENvideo":
                        this.OPENvideo();
                        break;
                    case "COPYvideo":
                        break;
                    case "SAVEas":
                        this.SAVEas();
                        break;
                    case "DELETEvideo":
                        this.DELETEvideo();
                        return;
                    case "CANCELvideo":
                        this.Cancel();
                        break;
                    case "FowardVideo":
                        FowardVideoMsg();
                        break;
                    case "DownloadVideo":
                        DownloadVideo();
                        break;
                    case "WITHDRAWvideo":
                        this.WithDraw(item.DataContext as MessageModel);
                        break;
                }
            }
        }

        private void SendVideo()
        {
            if (ChatViewModel != null)
            {
                _targetModel.CanOperate = false;

                if (FileExists())
                {
                    AcioningItems.Add(this);
                    _operateTask = new System.Threading.CancellationTokenSource();
                    _targetModel.ResourceModel.FileState = FileStates.SendOffline;
                    if (AppData.CanInternetAction(""))
                    {
                        ChatViewModel.SendSmallVideoFile(_targetModel, _operateTask,async (result) =>
                       {
                           if (SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden)
                           {
                               _targetModel.MessageState = MessageStates.Warn;
                               _targetModel.ResourceModel.FileState = FileStates.Fail;
                           }
                           
                           AcioningItems.Remove(this);
                           if (_operateTask != null)
                           {
                               _operateTask.Cancel();
                               _operateTask = null;
                           }
                       });
                    }
                    else
                    {
                        ChatViewModel.SaveSendingMessages(_targetModel);
                    }
                }
                else
                {
                    ChatViewModel.Chat.Messages.Remove(_targetModel);
                }
            }
        }
        /// <summary>
        /// 转发
        /// </summary>
        /// <param name="isGroup"></param>
        private void FowardVideoMsg()
        {
            if (_targetModel != null)
                AppData.FowardMsg(ChatViewModel.ID, _targetModel.MsgKey, ChatViewModel.IsGroup);
        }
        /// <summary>
        /// 通过时长计算视频总秒数
        /// </summary>
        private void CalculateRecordTimeVarDuration()
        {
            try
            {
                this._recordTime = this._duration.Split(':')[0].ToInt() * 60 + this._duration.Split(':')[1].ToInt();
            }
            catch
            {
                this._recordTime = 0;
            }
        }

        /// <summary>
        /// 通过总秒数计算视频时长
        /// </summary>
        private void CalculateDurationVarRecordTime()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                this.tbDuration.Text = this._duration = TimeSpan.FromSeconds(this.RecordTime).Minutes.ToString("00") + ":" + TimeSpan.FromSeconds(this.RecordTime).Seconds.ToString("00");
            });
        }

        private void SetVideoPreviewImage()
        {
            if (System.IO.File.Exists(this.VideoPreviewImage))
            {

                this.imgFirstFrame.Source = new BitmapImage(new Uri(this.VideoPreviewImage, UriKind.RelativeOrAbsolute))
                {
                    CacheOption = BitmapCacheOption.OnLoad,
                };

            }
        }

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <returns></returns>
        private bool FileExists()
        {
            if (System.IO.File.Exists(this.VideoPath))
            {
                return true;
            }
            else
            {
                AppData.MainMV.TipMessage = "此文件不存在，可能被删除或者移动到其他位置。";
                return false;
            }
        }

        private void OPENvideo()
        {
            Play();
        }

        public void Cancel(bool isInner = false)
        {
            AcioningItems.Remove(this);
            if (_targetModel.ResourceModel.Progress > 0.98)
            {
                AppData.MainMV.TipMessage = "视频消息已发出无法取消！";
                return;
            }
            if (ChatViewModel != null)
            {
                if (_operateTask != null)
                {
                    _operateTask.Cancel();
                    _operateTask = null;

                    if (isInner)
                    {
                        return;
                    }
                }

                string tip = _targetModel.Sender.ID == AppData.Current.LoginUser.ID ? "发送" : "接收";

                string size = Helper.FileHelper.FileSizeToString(_targetModel.ResourceModel.Length);
                string msg = $"您取消了\"{_targetModel.ResourceModel.FileName}\"({size})的{tip}，文件传输失败。";

                _targetModel.MsgType = MessageType.notification;
                _targetModel.MessageState = MessageStates.Fail;
                _targetModel.Content = msg;

                ChatViewModel.UpdateMsg(_targetModel);

                int roomID = ChatViewModel.Model.ID;

                var chatType = ChatViewModel.IsGroup ? SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;

                Task.Run(async()=>
                await SDKClient.SDKClient.Instance.AppendLocalData_NotifyMessage(AppData.Current.LoginUser.ID.ToString(), roomID.ToString(),
                                     msg,roomID, SDKClient.SDKProperty.MessageType.notification, chatType: chatType));
                if (this.FileState == FileStates.SendOnline ||
                    _targetModel.ResourceModel.RefInfo is SDKClient.Model.OnlineFileBody body) //在线消息会发送取消给对方
                {
                    SDKClient.SDKProperty.chatType messageType = ChatViewModel.IsGroup ? SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;
                    SDKClient.SDKClient.Instance.SendRetractMessage(_targetModel.MsgKey, ChatViewModel.ID.ToString(), messageType, ChatViewModel.IsGroup ? ChatViewModel.ID : 0, SDKClient.SDKProperty.RetractType.SourceEndOnlineRetract);

                }
                else //离线消息不发送
                {
                    _targetModel.Sender = null;
                    SDKClient.SDKClient.Instance.CancelOfflineFileRecv(_targetModel.MsgKey);
                }
                _targetModel.ResourceModel.FileState = FileStates.Fail;
            }
        }

        private void SAVEas()
        {
            if (System.IO.File.Exists(this.VideoPath))
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = System.IO.Path.GetFileName(this.VideoPath);
                dlg.DefaultExt = System.IO.Path.GetExtension(this.VideoPath);

                dlg.Filter = string.Format("视频 (.{0})|*.{0}", dlg.DefaultExt);

                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                if (dlg.ShowDialog() == true && this.VideoPath != dlg.FileName)
                {
                    if (System.IO.File.Exists(dlg.FileName))
                    {
                        System.IO.File.Delete(dlg.FileName);
                    }
                    System.IO.File.Copy(this.VideoPath, dlg.FileName, true);
                }
            }
            else
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = System.IO.Path.GetFileName(this.VideoPath);
                dlg.DefaultExt = System.IO.Path.GetExtension(this.VideoPath);

                dlg.Filter = string.Format("视频 (.{0})|*.{0}", dlg.DefaultExt);

                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                if (dlg.ShowDialog() == true)
                {
                    //if (_targetModel == null && this.DataContext is MessageModel item)
                    //{
                    //    _targetModel = item;
                    //    _targetModel.Content = dlg.FileName;
                    //    _targetModel.ResourceModel.FullName = dlg.FileName;
                    //}
                    DownloadVideo(dlg.FileName);
                }
            }
        }

        private void DELETEvideo()
        {
            ChatViewModel chatVM = AppData.MainMV.ChatListVM.SelectedItem;
            if (chatVM != null)
            {
                if (_operateTask != null)
                {
                    _operateTask.Cancel();
                    _operateTask = null;
                }
                chatVM.HideMessageCommand.Execute(_targetModel);

                string filePath = System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.filePath, _targetModel.ResourceModel.FileName);
            }
        }

        /// <summary>
        /// 撤回
        /// </summary>
        /// <param name="msg"></param>
        private void WithDraw(MessageModel msg)
        {
            ChatViewModel chatVM = AppData.MainMV.ChatListVM.SelectedItem;
            if (chatVM != null)
            {
                chatVM.SendWithDrawMsg(msg);
            }
        }

        private void menu_Opened(object sender, RoutedEventArgs e)
        {
            if (e.Source is ContextMenu item && item.DataContext is MessageModel msg)
            {
                if (msg.MessageState == MessageStates.Success || msg.ResourceModel.FileState == FileStates.Completed)
                {
                    this.Download.Visibility = Visibility.Collapsed;
                    this.Open.Visibility = Visibility.Visible;
                    this.Foward.Visibility = Visibility.Visible;
                }
                else
                {
                    this.Download.Visibility = Visibility.Visible;
                    this.Open.Visibility = Visibility.Collapsed;
                }
                if (_targetModel.ResourceModel.FileState == FileStates.SendOffline && msg.IsMine)
                {
                    if (!this.menu.Items.Contains(this._miCancel))
                    {
                        this.menu.Items.Add(this._miCancel);
                    }
                }
                else
                {
                    if (this.menu.Items.Contains(this._miCancel))
                    {
                        this.menu.Items.Remove(this._miCancel);
                    }
                }

                if (_completeTime == null)
                {
                    return;
                }

                if ((DateTime.Now - _completeTime.Value).TotalMinutes >= 2 || !msg.IsMine)
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
                        if (!_targetModel.IsSending)
                        {
                            if (msg.MessageState == MessageStates.Success || msg.MessageState == MessageStates.None)
                                this.menu.Items.Add(this._miWithDraw);
                        }

                    }
                }
            }
        }

    }
}
