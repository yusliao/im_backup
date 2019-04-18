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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IMModels;
using IMClient.ViewModels;
using System.Windows.Controls.Primitives;
using IMClient.Converter;
using IMClient.Helper;
using IMClient.Views.Panels;

namespace IMClient.Views.Controls
{
    /// <summary>
    ///聊天内容项 -文件
    /// </summary>
    public partial class FileChatItem : UserControl
    {
        static FileChatItem()
        {
            AcioningItems = new List<FileChatItem>();
            FileItems = new List<MessageModel>();
            SendingUuids = new List<Guid>();
        }
        const long MAXFILELENGTH = 1024 * 1024 * 101;
        const long ONLINEFILELENGTH = 1024 * 1024 * 100;
        DateTime? _completeTime;
        /// <summary>
        /// 正在操作的文件对象
        /// </summary>
        public static List<FileChatItem> AcioningItems { get; private set; }
        public static List<Guid> SendingUuids { get; private set; }
        /// <summary>
        /// 所有加载的文件项模型，保障对象唯一性
        /// </summary>
        public static List<MessageModel> FileItems { get; private set; }

        public ChatViewModel ChatViewModel { get; private set; }
        public MessageModel _targetModel { get; set; }
        private bool _isOnlineSendFile;
        /// <summary>
        /// 操作线程任务
        /// </summary>
        System.Threading.CancellationTokenSource _operateTask;
        List<MenuItem> menuItems = new List<MenuItem>();
        MenuItem _miWithDraw;
        MenuItem _miFoward;

        public bool IsInDocument { get; set; }
        public FileChatItem()
        {
            InitializeComponent();
            _miWithDraw = new MenuItem()
            {
                Uid = "WITHDRAWfile",
                Header = "撤回",
            };
            _miFoward = new MenuItem()
            {
                Uid = "Foward",
                Header = "转发",
            };
            //_targetModel = this.DataContext as MessageModel;
            //ChatViewModel = AppData.MainMV.ChatListVM.SelectedItem;
        }

        public string FullName
        {
            get { return (string)GetValue(FullNameProperty); }
            set { SetValue(FullNameProperty, value); }
        }

        private bool _isMainView = true;
        public bool IsMainView
        {
            get { return _isMainView; }
            set
            {
                _isMainView = value;
                this.gridLayout.ContextMenu = value ? this.menu : null;
            }
        }

        private void pbProcess_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue < this.pbProcess.Maximum)
            {
                //    this.pbProcess.Visibility = Visibility.Collapsed;
                //    //this.bdLayout.BorderThickness = new Thickness(2);
                //    //this.bdLayout.CornerRadius = new CornerRadius(2);

                //    this.gridLayout.ContextMenu = this.HasContexMenu ? this.menu : null;
                if (FileState != FileStates.Receiving)
                    FileState = FileStates.Receiving;
            }
            //else
            //{

            //}
        }

        /// <summary>
        /// 文件状态
        /// </summary>
        public FileStates FileState
        {
            get { return (FileStates)GetValue(FileStateProperty); }
            set { SetValue(FileStateProperty, value); }
        }

        public static readonly DependencyProperty FullNameProperty =
         DependencyProperty.Register("FullName", typeof(string), typeof(FileChatItem), new PropertyMetadata(OnFullNamePropertyChanged));

        private static void OnFullNamePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FileChatItem target = d as FileChatItem;
            target.CalFileState();
        }

        public static readonly DependencyProperty FileStateProperty =
            DependencyProperty.Register("FileState", typeof(FileStates), typeof(FileChatItem), new PropertyMetadata(OnFileStatePropertyChanged));

        private static void OnFileStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FileChatItem target = d as FileChatItem;
            target.SetFileStatesView();
        }

        private void CalFileState()
        {
            //this.column0.Width = new GridLength(1, GridUnitType.Auto);
            //this.column1.Width = new GridLength(1, GridUnitType.Star);
            if (this.FileState == FileStates.Receiving)
            {
                return;
            }
            try
            {
                this.tbName.ToolTip = System.IO.Path.GetFileName(this.FullName);//?.Split('\\').LastOrDefault();
            }
            catch
            {
                return;
            }


            if (this.DataContext is MessageModel item)
            {
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
                this.DataContext = _targetModel = target;
            }
            if (ChatViewModel == null)
            {
                ChatViewModel = AppData.MainMV.ChatListVM.SelectedItem;
                AppData.MainMV.ChatListVM.SelectedItem.ReSendFileEvent -= ChatViewModel_ReSendFileEvent;
                if (_targetModel.IsMine && !_targetModel.IsSync)
                    AppData.MainMV.ChatListVM.SelectedItem.ReSendFileEvent += ChatViewModel_ReSendFileEvent;
            }
            else if (_targetModel.MessageState == MessageStates.Fail)
            {
                if (_operateTask != null)
                {
                    _operateTask.Cancel();
                }
                AcioningItems.Remove(this);
            }
            if (this.IsMainView == false)
            {
                if (_targetModel.ResourceModel.FileState != FileStates.None)
                {
                    return;
                }
            }


            if (this.gridScan.Visibility == Visibility.Visible)
            {
                this.gridLayout.Width = 210;
                this.gridLayout.Height = 80;
                this.gridLayout.Margin = new Thickness(12, 5, 12, 0);
                this.gridScan.Visibility = Visibility.Collapsed;
                this.gridScan.Children.RemoveAt(0);
            }


            //是否发送者
            bool isSender = _targetModel.Sender.ID == AppData.Current.LoginUser.ID;
            _targetModel.ResourceModel.FileImg = Helper.FileHelper.GetFileImage(this.FullName, true);
            this.bdLayout.ToolTip = System.IO.Path.GetFileName(this.FullName);
            if (File.Exists(this.FullName))
            {

                if (isSender) //当前作为发送方
                {
                    if (_targetModel.IsSending) //若是正在发送中
                    {
                        if (this._targetModel.ResourceModel.FileState == FileStates.SendOffline || this._targetModel.ResourceModel.FileState == FileStates.SendOnline)
                        {
                            if (!AcioningItems.Contains(this))
                            {
                                AcioningItems.Add(this);
                            }
                        }
                        else
                        {
                            if (AppData.CanInternetAction(""))
                            {
                                FileInfo fileInfo = new System.IO.FileInfo(this.FullName);
                                if (ChatViewModel.IsGroup || ChatViewModel.IsFileAssistant || fileInfo.Length < MAXFILELENGTH)
                                {
                                    this.FileState = _targetModel.ResourceModel.FileState = FileStates.SendOffline;
                                    this.OfflineSend(false);
                                }
                                else
                                {
                                    this.FileState = _targetModel.ResourceModel.FileState = FileStates.SendOnline;
                                    this.OnlineSend();
                                }
                            }
                            else
                            {
                                this.FileState = _targetModel.ResourceModel.FileState = FileStates.SendOffline;
                                this.OfflineSend(false);
                            }
                        }
                    }
                    else if (_targetModel.ResourceModel.DBState == 0 && _targetModel.ResourceModel.FileState != FileStates.Completed)
                    {
                        this.FileState = _targetModel.ResourceModel.FileState = FileStates.SendOnline;
                    }
                    else
                    {
                        if (_targetModel.ResourceModel.DBState == 4)
                        {
                            this.FileState = _targetModel.ResourceModel.FileState = FileStates.Fail;
                        }
                        else
                        {

                            this.FileState = _targetModel.ResourceModel.FileState = FileStates.Completed;
                        }
                        //if (_targetModel.IsSync)
                        //{
                        //    _completeTime = _targetModel.SendTime;
                        //}
                        //else
                        //{
                        //    _completeTime = DateTime.Now.AddMinutes(-5);
                        //}
                        _completeTime = _targetModel.SendTime;
                    }
                }
                else  //当前用户作为接收方，若文件已有则认为已经成功
                {
                    FileInfo file = new FileInfo(this.FullName);
                    if (file.Length != _targetModel.ResourceModel?.Length)
                    {
                        _targetModel.ResourceModel.CompleteLength = file.Length;
                        this.FileState = _targetModel.ResourceModel.FileState = FileStates.WaitForReceieve;
                    }
                    else
                    {
                        this.FileState = _targetModel.ResourceModel.FileState = FileStates.Completed;
                    }
                    //if (_targetModel.IsSync)
                    //{
                    //    _completeTime = _targetModel.SendTime;
                    //}
                    //else
                    //{
                    //    _completeTime = DateTime.Now.AddMinutes(-5);
                    //}
                    _completeTime = _targetModel.SendTime;

                }
            }
            else
            {

                if (isSender) //发送者
                {
                    if (_targetModel.IsMine && _targetModel.IsSync && _targetModel.ResourceModel.DBState == 0)
                    {
                        this.FileState = _targetModel.ResourceModel.FileState = FileStates.WaitForReceieve;
                    }
                    else
                    {
                        this.FileState = _targetModel.ResourceModel.FileState = FileStates.Completed;
                    }
                    //if (_targetModel.IsSync)
                    //{
                    //    _completeTime = _targetModel.SendTime;
                    //}
                    //else
                    //{
                    //    _completeTime = DateTime.Now.AddMinutes(-5);
                    //}
                    _completeTime = _targetModel.SendTime;
                }
                else
                {
                    switch (_targetModel.ResourceModel.DBState)
                    {
                        case 0:
                            this.FileState = _targetModel.ResourceModel.FileState = FileStates.WaitForReceieve;
                            break;
                        case 1:
                            this.FileState = _targetModel.ResourceModel.FileState = FileStates.Receiving;
                            break;
                        case 2:
                            this.FileState = _targetModel.ResourceModel.FileState = FileStates.Completed;
                            break;
                    }

                    if (this.FileState != FileStates.Completed)
                        if (_targetModel.ResourceModel.RefInfo != null && !AcioningItems.Contains(this))
                        {
                            AcioningItems.Add(this);
                        }
                }
            }
        }
        string tempMsgId = string.Empty;
        /// <summary>
        /// 重发文件
        /// </summary>
        private void ChatViewModel_ReSendFileEvent(string msgID)
        {
            if (_targetModel?.MsgKey == msgID && _targetModel.MessageState == MessageStates.Fail)
            {
                tempMsgId = msgID;
                _targetModel.MessageState = MessageStates.None;
                OfflineSend(false, true);
            }
        }

        private void SetFileStatesView()
        {
            this.pbProcess.Visibility = Visibility.Collapsed;
            //this.BdIcon.Visibility = Visibility.Collapsed;

            //this.column0.Width = new GridLength(1, GridUnitType.Auto);
            //this.column1.Width = new GridLength(1, GridUnitType.Star);

            //foreach (UIElement ui in this.ugState.Children)
            //{
            //    ui.Visibility = Visibility.Collapsed;
            //}

            switch (this.FileState)
            {
                case FileStates.Fail:
                    if (ChatViewModel != null)
                    {
                        //ChatViewModel.UpdateMsg(_targetModel);
                        if (_targetModel?.ResourceModel != null && File.Exists(_targetModel.ResourceModel.FullName))
                        {
                            if (AppData.CanInternetAction(""))
                            {
                                var t = Task.Run(async () =>
                                {
                                    var result = await SDKClient.SDKClient.Instance.FindResource(_targetModel.ResourceModel.FullName);
                                    //if (!result.existed)
                                    //{
                                    _targetModel.ResourceModel.Length = result.fileSize;
                                    _targetModel.ResourceModel.CompleteLength = result.fileinitValue;
                                    //}
                                });
                            }
                        }
                        //this.btnCancel.Visibility = Visibility.Visible;
                        if (this._targetModel.ResourceModel.Progress > 0 && _targetModel.ResourceModel.Progress < 1)
                            this.pbProcess.Visibility = Visibility.Visible;
                        AcioningItems.Remove(this);
                        if (_operateTask != null)
                        {
                            _operateTask.Cancel();
                            _operateTask = null;
                        }
                    }
                    return;
                default:
                case FileStates.None:

                    break;
                case FileStates.SendOnline:
                    //this.btnOfflineSend.Visibility = this.btnCancel.Visibility = Visibility.Visible;

                    this.pbProcess.Visibility = Visibility.Visible;
                    //if (_targetModel.MessageState == MessageStates.Success)
                    //this.btnForward.Visibility = Visibility.Visible;
                    break;
                case FileStates.SendOffline:
                    //if (AppData.CanInternetAction(""))
                    //    this.btnOnlineSend.Visibility = this.btnCancel.Visibility = Visibility.Visible;
                    //else
                    //    this.btnCancel.Visibility = Visibility.Visible;
                    this.pbProcess.Visibility = Visibility.Visible;

                    if (AppData.MainMV.ChatListVM.SelectedItem.IsGroup || ChatViewModel.IsFileAssistant) //群聊不能 在线发送
                    {
                        //this.btnOnlineSend.Visibility = Visibility.Collapsed;
                        //this.tbConnecting.Visibility = Visibility.Visible;
                    }
                    //if (_targetModel.MessageState == MessageStates.Success)
                    //this.btnForward.Visibility = Visibility.Visible;
                    break;
                case FileStates.WaitForReceieve:

                    if (_targetModel.Sender.ID == AppData.Current.LoginUser.ID && _targetModel.IsMine && _targetModel.IsSync)
                    {
                        //this.btnOpen.Visibility = this.btnOpenFolder.Visibility = this.btnDelete.Visibility = Visibility.Visible;
                        this.gridUplaod.Visibility = Visibility.Visible;
                    }
                    else
                    {

                        if (_targetModel.ResourceModel != null && _targetModel.ResourceModel.Progress > 0)
                        {
                            this.pbProcess.Visibility = Visibility.Visible;
                            this.gridUplaodCompleted.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            this.gridUplaod.Visibility = Visibility.Visible;
                            //this.btnReceive.Visibility =
                            //this.btnSaveAs.Visibility =
                            //this.btnCancel.Visibility = Visibility.Visible;
                        }
                    }
                    break;
                case FileStates.Receiving:
                    this.gridUplaod.Visibility = Visibility.Collapsed;
                    if (!AppData.MainMV.ChatListVM.SelectedItem.IsGroup)
                    {
                        if (_targetModel == null && this.DataContext is MessageModel item)
                        {
                            if (item.Sender.ID == AppData.Current.LoginUser.ID)
                            {
                                if (_isOnlineSendFile)
                                {
                                    //this.btnOfflineSend.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    //this.btnOnlineSend.Visibility = Visibility.Visible;
                                }
                            }
                        }
                    }
                    //this.btnCancel.Visibility = Visibility.Visible;
                    this.pbProcess.Visibility = Visibility.Visible;
                    break;
                case FileStates.Completed:

                    //this.btnOpen.Visibility = this.btnOpenFolder.Visibility = this.btnDelete.Visibility = Visibility.Visible;
                    //this.btnSaveAs.Visibility = Visibility.Collapsed;
                    string extension = System.IO.Path.GetExtension(this.FullName).ToLower();
                    if (!_targetModel.IsSync)
                    {
                        _completeTime = DateTime.Now;
                    }

                    if (extension.Length > 0 && System.IO.File.Exists(this.FullName) && App.ImageFilter.Contains(extension))
                    {
                        this.gridLayout.Margin = new Thickness();
                        this.gridLayout.Width = this.gridLayout.Height = double.NaN;
                        this.gridScan.Visibility = Visibility.Visible;
                        this.gridFile.Visibility = Visibility.Collapsed;
                        //this.column0.Width = new GridLength(0);
                        //this.column1.Width = new GridLength(0);
                        var previewImagePath = string.Empty;
                        if (!string.IsNullOrEmpty(_targetModel.ResourceModel.Key))
                            previewImagePath = System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, $"my{_targetModel.ResourceModel.Key}");
                        ChatImage chatImg = new ChatImage(this.FullName, previewImagePath);
                        this.gridScan.Children.Insert(0, chatImg);

                        //MessageModel msModel = this.DataContext as MessageModel;
                        //var ChatViewModel = AppData.MainMV.ChatListVM.SelectedItem;
                        //if (ChatViewModel.Chat is ChatModel chat)
                        //{
                        //    var tempMsg= chat.Messages[chat.Messages.Count - 1];
                        //    if (ChatViewModel.View is ChatView chatView)
                        //    {
                        //        chatView.chatBox.ScallToCurrent(msModel);
                        //    }
                        //}
                    }
                    else if (extension.Length > 0 && System.IO.File.Exists(this.FullName) && App.VideoFilter.Contains(extension))
                    {
                        this.gridLayout.Margin = new Thickness();
                        this.gridLayout.Width = this.gridLayout.Height = double.NaN;
                        this.gridScan.Visibility = Visibility.Visible;
                        this.gridFile.Visibility = Visibility.Collapsed;

                        //this.column0.Width = new GridLength(0);
                        //this.column1.Width = new GridLength(0);

                        SmallVideo video = new SmallVideo(this.FullName, 142, 220, _targetModel);
                        video.tbDuration.Visibility = Visibility.Collapsed;

                        video.IsHitTestVisible = false;
                        gridScan.MouseLeftButtonDown -= GridScan_MouseLeftButtonDown;
                        gridScan.MouseLeftButtonDown += GridScan_MouseLeftButtonDown;
                        this.gridScan.Children.Insert(0, video);
                    }
                    else
                    {
                        //this.btnOpen.Visibility = this.btnOpenFolder.Visibility = this.btnDelete.Visibility = Visibility.Visible;
                        //this.btnSaveAs.Visibility = Visibility.Collapsed;
                        //this.BdIcon.Visibility = Visibility.Visible;
                        this.gridUplaod.Visibility = Visibility.Collapsed;
                        this.gridUplaodCompleted.Visibility = Visibility.Visible;
                        _operateTask = null;
                        AcioningItems.Remove(this);
                        if (File.Exists(this.FullName))
                        {
                            _targetModel.ResourceModel.FileImg = Helper.FileHelper.GetFileImage(this.FullName, true);//  Helper.WindowsThumbnailProvider.GetFileThumbnail(this.FullName);
                        }
                        this.pbProcess.Visibility = Visibility.Collapsed;
                    }
                    //if (ChatViewModel.View is ChatView chatview)
                    //{
                    //    App.Current.Dispatcher.Invoke(new Action(() =>
                    //    {
                    //        chatview.chatBox.ScallToEnd();
                    //    }));
                    //}
                    break;
            }

            if (this.IsInDocument)
            {
                //this.btnDelete.Visibility = Visibility.Collapsed;
                //this.btnCancel.Visibility = Visibility.Collapsed;
            }
            App.Current.Dispatcher.Invoke(() =>
            {
                //if (this.menu.Items != null)
                //    this.menu.Items.Clear();
            });

            //foreach (UIElement ui in this.ugState.Children)
            //{
            //    if (ui.Visibility == Visibility.Visible && ui is ButtonBase btn)
            //    {

            //        this.menu.Items.Add(new MenuItem() { Uid = btn.Uid, Header = btn.Content });
            //        if (_targetModel.MsgType == MessageType.onlinefile || _targetModel.ResourceModel.RefInfo is SDKClient.Model.OnlineFileBody || _isOnlineSendFile)
            //            continue;
            //        if (btn.Uid == "Open")
            //        {
            //            this.menu.Items.Add(new MenuItem() { Uid = "Forward", Header = "转发" });
            //        }
            //    }
            //}
        }

        private void GridScan_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Open();
        }
        private List<MenuItem> FileMenuItems()
        {
            var tempMenuItems = new List<MenuItem>();
            if (_targetModel.ResourceModel.FileState == FileStates.WaitForReceieve)
            {
                tempMenuItems.Add(new MenuItem
                {
                    Uid = "Receive",
                    Header = "开始接收",
                });
            }
            //if (_targetModel.MessageState == MessageStates.sending)
            //{
            //    tempMenuItems.Add(new MenuItem
            //    {
            //        Uid = "PauseSend",
            //        Header = "暂停发送",
            //    });
            //}
            //tempMenuItems.Add(new MenuItem
            //{
            //    Uid = "ContinueSend",
            //    Header = "重新发送",
            //});
            if (_targetModel.MessageState == MessageStates.Success || _targetModel.ResourceModel.FileState == FileStates.Completed)
            {
                tempMenuItems.Add(new MenuItem()
                {
                    Uid = "Forward",
                    Header = "转发",
                });
            }

            //if ((_completeTime != null && (DateTime.Now - _completeTime.Value).TotalMinutes >= 2))
            //{
            //    //if (this.menu.Items.Contains(this._miWithDraw))
            //    //{
            //    //    this.menu.Items.Remove(this._miWithDraw);
            //    //}
            //}
            //else 
            if ((_completeTime != null && (DateTime.Now - _completeTime.Value).TotalMinutes <= 2) && _targetModel.IsMine)
            {
                if (!this.menu.Items.Contains(this._miWithDraw))
                {
                    if (_targetModel.MessageState == MessageStates.Success || _targetModel.MessageState == MessageStates.None)
                    {
                        tempMenuItems.Add(new MenuItem()
                        {
                            Uid = "WITHDRAWfile",
                            Header = "撤回",
                        });
                    }

                }
            }

            if (_targetModel.ResourceModel.FileState == FileStates.Completed || (_targetModel.IsMine && !_targetModel.IsSync))
            {
                tempMenuItems.Add(new MenuItem()
                {
                    Uid = "Open",
                    Header = "查看",
                });
                //}
                //if (_targetModel.ResourceModel.FileState == FileStates.Completed || (_targetModel.IsMine && !_targetModel.IsSync))
                //{
                tempMenuItems.Add(new MenuItem()
                {
                    Uid = "OpenFolder",
                    Header = "在文件夹中显示",
                });
            }
            tempMenuItems.Add(new MenuItem()
            {
                Uid = "SaveAs",
                Header = "另存为",
            });
            tempMenuItems.Add(new MenuItem()
            {
                Uid = "Delete",
                Header = "删除",
            });
            return tempMenuItems;
        }


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Control item)
            {
                switch (item.Uid)
                {
                    case "OfflineSend":
                        if (_targetModel != null && _targetModel.MessageState == MessageStates.Warn)
                            break;
                        if (AppData.CanInternetAction("当前网络异常，无法转离线！"))
                            this.OfflineSend();
                        break;
                    case "OnlineSend":
                        if (_targetModel != null && _targetModel.MessageState == MessageStates.Warn)
                            break;
                        if (AppData.CanInternetAction("当前无网络，无法转在线发送！"))
                            this.OnlineSend();
                        break;
                    case "Receive":
                        this.Receive();
                        break;
                    case "Open":
                        this.Open();
                        break;
                    case "SaveAs":
                        this.SaveAs();
                        break;
                    case "Delete":
                        this.Delete();
                        return;
                    case "PauseSend":
                        break;
                    case "ContinueSend":
                        break;
                    case "Cancel":
                        this.Cancel();
                        break;
                    case "Forward"://转发
                        FowardFileMsg();
                        break;
                    case "WITHDRAWfile":
                        this.WithDraw(item.DataContext as MessageModel);
                        break;
                    case "OpenFolder":
                        OpenFileDirectory();
                        break;

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

        #region OperateMethods
        private void OpenFileDirectory()
        {
            if (FileExists())
            {

                FileInfo info = new FileInfo(this.FullName);

                //System.Diagnostics.Process.Start("Explorer.exe", info.Directory.FullName);
                //System.Diagnostics.Process.Start("Explorer.exe", "/select," + info.Directory.FullName + "\\" + info.Name);
                System.Diagnostics.Process.Start("Explorer", "/select," + info.Directory.FullName + "\\" + info.Name);
            }
        }
        /// <summary>
        /// 撤回
        /// </summary>
        /// <param name="msg"></param>
        private void WithDraw(MessageModel msg)
        {
            AcioningItems.Remove(this);
            ChatViewModel chatVM = AppData.MainMV.ChatListVM.SelectedItem;
            if (chatVM != null)
            {
                chatVM.SendWithDrawMsg(msg);
            }
        }
        /// <summary>
        /// 转发
        /// </summary>
        /// <param name="isGroup"></param>
        private void FowardFileMsg()
        {
            if (_targetModel != null)
                AppData.FowardMsg(ChatViewModel.ID, _targetModel.MsgKey, ChatViewModel.IsGroup);
        }
        /// <summary>
        /// 离线发送
        /// </summary>
        private void OfflineSend(bool isAuto = true, bool isReSend = false)
        {
            if (!AppData.CanInternetAction(""))
            {
                _targetModel.MessageState = MessageStates.Fail;
                return;
            }
            if (ChatViewModel != null)
            {
                if (!File.Exists(this.FullName))
                {
                    AppData.MainMV.TipMessage = "文件不存在！";
                    return;
                }
                FileInfo fileInfo = new System.IO.FileInfo(this.FullName);
                if (fileInfo.Length > MAXFILELENGTH)
                {
                    AppData.MainMV.TipMessage = $"文件大于100M，不支持离线发送！";
                    return;
                }
                _isOnlineSendFile = false;
                if (!isReSend)
                {
                    _targetModel.ResourceModel.CompleteLength = 0;
                    _targetModel.CanOperate = true;
                    //this.tbConnecting.Visibility = Visibility.Visible;
                }

                if (isAuto && !string.IsNullOrEmpty(_targetModel.MsgKey))
                {
                    SDKClient.SDKProperty.chatType messageType = ChatViewModel.IsGroup ? SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;
                    SDKClient.SDKClient.Instance.SendRetractMessage(_targetModel.MsgKey,
                        ChatViewModel.ID.ToString(), messageType, ChatViewModel.IsGroup ? ChatViewModel.ID : 0, SDKClient.SDKProperty.RetractType.OnlineToOffline);

                    //string size = Helper.FileHelper.FileSizeToString(_targetModel.ResourceModel.Length);
                    //string content = $"您取消了\"{_targetModel.ResourceModel.FileName}\"({size})的发送，文件传输失败。";

                    //MessageModel cancel = new MessageModel()
                    //{
                    //    MsgKey=_targetModel.MsgKey,                                          
                    //    Content= content,
                    //    MsgType=MessageType.notification,
                    //    MessageState=MessageStates.Fail,
                    //};
                    //ChatViewModel.Chat.Messages.Insert(ChatViewModel.Chat.Messages.Count - 1, cancel);

                    AcioningItems.Remove(this);
                    if (_operateTask != null)
                    {
                        _operateTask.Cancel();
                        _operateTask = null;
                    }
                }

                if (File.Exists(this.FullName))
                {
                    if (!AcioningItems.Contains(this))
                    {
                        if (!ChatViewModel.IsTemporaryChat)
                        {
                            if (AppData.CanInternetAction(""))
                                AcioningItems.Add(this);
                        }
                        else
                        {
                            this.menu.Visibility = Visibility.Collapsed;
                        }
                    }
                    _operateTask = new System.Threading.CancellationTokenSource();

                    if (App.VideoFilter.ToLower().Contains(System.IO.Path.GetExtension(this.FullName).ToLower()))
                    {
                        string thumbnailPath = System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, Guid.NewGuid() + ".jpg");
                        FFmpegHelper.GetOneFrameImageFromVideo(this.FullName, thumbnailPath, () => { });
                        _targetModel.ResourceModel.PreviewImagePath = thumbnailPath;
                    }
                    if (SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden)
                        _targetModel.MessageState = MessageStates.Warn;

                    if (AppData.CanInternetAction(""))
                    {
                        ChatViewModel.SendOfflineFile(_targetModel, _operateTask, (errorInfo) =>
                        {
                            if (errorInfo != null)
                            {
                                //if (errorInfo == "TokenCancel")
                                //{
                                //    errorInfo = UpdateToCancelMsg("发送");
                                //}
                                //int roomID = ChatViewModel.Model.ID;

                                //var chatType = ChatViewModel.IsGroup ? SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;
                                //await SDKClient.SDKClient.Instance.AppendLocalData_NotifyMessage(AppData.Current.LoginUser.ID.ToString(), roomID.ToString(),
                                //    errorInfo, roomID, SDKClient.SDKProperty.MessageType.notification, chatType: chatType);

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
                    AppData.MainMV.TipMessage = "该文件已被移动或删除，操作失败！";

                    //string size = Helper.FileHelper.FileSizeToString(_targetModel.ResourceModel.Length);
                    //string content = $"您取消了\"{_targetModel.ResourceModel.FileName}\"({size})的发送，文件传输失败。";

                    //MessageModel cancel = new MessageModel()
                    //{
                    //    MsgKey = _targetModel.MsgKey,
                    //    Content = content,
                    //    MsgType = MessageType.notification,
                    //    MessageState = MessageStates.Fail,
                    //};
                    //ChatViewModel.Chat.Messages.Insert(ChatViewModel.Chat.Messages.Count - 1, cancel);


                    ChatViewModel.Chat.Messages.Remove(_targetModel);
                }
            }
        }

        /// <summary>
        /// 在线发送
        /// </summary>
        private async void OnlineSend()
        {
            if (ChatViewModel != null)
            {
                _isOnlineSendFile = true;
                _targetModel.ResourceModel.CompleteLength = 0;
                _targetModel.CanOperate = true;
                AcioningItems.Remove(this);
                if (_operateTask != null)
                {
                    _operateTask.Cancel();
                    _operateTask = null;
                }

                bool isOnLine = await SDKClient.SDKClient.Instance.GetUserPcOnlineInfo(ChatViewModel.ID);
                if (!isOnLine)
                {
                    this.FileState = FileStates.SendOffline;
                    OfflineSend(false);
                    //this.btnOnlineSend.Visibility = Visibility.Collapsed;
                    return;
                }


                if (FileExists())
                {
                    if (!AcioningItems.Contains(this))
                    {
                        if (!ChatViewModel.IsTemporaryChat)
                        {
                            AcioningItems.Add(this);
                        }
                        else
                        {
                            this.menu.Visibility = Visibility.Collapsed;
                        }
                    }
                    _operateTask = new System.Threading.CancellationTokenSource();
                    if (SDKClient.SDKClient.Instance.property.CurrentAccount.Isforbidden)
                        _targetModel.MessageState = MessageStates.Warn;
                    if (ChatViewModel.IsGroup)
                    {
                        //this.tbConnecting.Visibility = Visibility.Visible;
                        //群聊只能离线发送

                        if (App.VideoFilter.ToLower().Contains(System.IO.Path.GetExtension(this.FullName).ToLower()))
                        {
                            string thumbnailPath = System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, Guid.NewGuid() + ".jpg");
                            FFmpegHelper.GetOneFrameImageFromVideo(this.FullName, thumbnailPath, () => { });
                            _targetModel.ResourceModel.PreviewImagePath = thumbnailPath;
                        }

                        ChatViewModel.SendOfflineFile(_targetModel, _operateTask, (errorInfo) =>
                        {
                            //if (errorInfo != null)
                            //{
                            //    if (errorInfo == "TokenCancel")
                            //    {
                            //        errorInfo = UpdateToCancelMsg("发送");
                            //    }
                            //    int roomID = ChatViewModel.Model.ID;
                            //    await SDKClient.SDKClient.Instance.AppendLocalData_NotifyMessage(AppData.Current.LoginUser.ID.ToString(), roomID.ToString(),
                            //        errorInfo, roomID, SDKClient.SDKProperty.MessageType.notification, chatType: SDKClient.SDKProperty.chatType.groupChat);
                            //}
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
                        ChatViewModel.SendOnlineFile(ChatViewModel, _targetModel, _operateTask, (result) =>
                        {
                            AcioningItems.Remove(this);
                            //if (_operateTask != null)
                            //{
                            //    _operateTask.Cancel();
                            //    _operateTask = null;
                            //}
                        });
                    }
                }
                else
                {
                    ChatViewModel.Chat.Messages.Remove(_targetModel);
                }
                ChatViewModel.WarningInfo = "如果对方长时间未接收，建议您发送离线文件";
            }
        }

        /// <summary>
        /// 开始接收
        /// </summary>
        private void Receive()
        {
            if (ChatViewModel != null && AppData.CanInternetAction())
            {
                _targetModel.MessageState = MessageStates.None;
                App.Current.Dispatcher.Invoke(() =>
                {
                    this.FileState = FileStates.Receiving;
                });
                _operateTask = new System.Threading.CancellationTokenSource();
                if (!AcioningItems.Contains(this))
                {
                    AcioningItems.Add(this);
                }


                IMClient.Helper.MessageHelper.LoadFileContent(_targetModel, _operateTask, ChatViewModel, (result, errorState) =>
                {

                    AcioningItems.Remove(this);

                    if (_operateTask != null)
                    {
                        _operateTask.Cancel();
                        _operateTask = null;
                    }
                    switch (errorState)
                    {
                        case SDKClient.SDKProperty.ErrorState.NetworkException:
                        case SDKClient.SDKProperty.ErrorState.ServerException:
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                //this.btnReceive.Visibility = Visibility.Visible;
                            });
                            return;
                    }

                    //if (result)
                    //{
                    //    App.Current.Dispatcher.Invoke(() =>
                    //    {
                    //        this.FileState = FileStates.Completed;
                    //    });
                    //}
                });
                MessageModel msModel = this.DataContext as MessageModel;
                if (ChatViewModel.View is ChatView chatView)
                {
                    chatView.chatBox.ScallToCurrent(msModel);
                }



            }
        }

        /// <summary>
        /// 打开 
        /// </summary>
        private void Open()
        {

            if (FileExists())
            {
                Views.ChildWindows.AppendWindow.AutoClose = false;
                try
                {
                    if (App.ImageFilter.Contains(System.IO.Path.GetExtension(this.FullName)))
                    {
                        ChildWindows.ImageScanWindow.ShowScan(this.FullName);
                    }
                    else
                    {
                        System.Diagnostics.Process.Start(this.FullName);
                    }
                }
                catch (Exception ex)
                {
                    App.Logger.Error(ex.StackTrace);
                    AppData.MainMV.TipMessage = $"打开文件失败！当前操作系统未指定该类型文件的默认打开方式!";
                }
                Views.ChildWindows.AppendWindow.AutoClose = true;
            }
        }

        /// <summary>
        /// 另存为
        /// </summary>
        private void SaveAs()
        {
            if (_targetModel.ResourceModel.FileState == FileStates.WaitForReceieve)
            {
                if (AppData.CanInternetAction())
                {
                    if (string.IsNullOrEmpty(_targetModel.ResourceModel.FileName)) return;
                    Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                    sfd.FileName = _targetModel.ResourceModel.FileName;
                    var indexOf = _targetModel.ResourceModel.FileName.LastIndexOf(".");
                    if (indexOf > 0)
                    {
                        indexOf = indexOf + 1;
                        var extension = _targetModel.ResourceModel.FileName.Substring(indexOf, _targetModel.ResourceModel.FileName.Length - indexOf);
                        //var str = string.Format("文件(*.{0})|*.{0}", extension);
                        sfd.Filter = string.Format("文件 (*.{0})|*.{0}", extension);
                    }
                    if (sfd.ShowDialog() == true)
                    {
                        SaveAsDownLoadFile(sfd.FileName);
                    }
                }
            }
            else
            {
                if (FileExists(true))
                {
                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

                    dlg.FileName = System.IO.Path.GetFileName(this.FullName); // Default file name
                    dlg.DefaultExt = System.IO.Path.GetExtension(this.FullName);// fileName.Split('.').LastOrDefault(); // Default file extension
                    dlg.Filter = string.Format("文件(*.{0})|*.{0}", dlg.DefaultExt);// // Filter files by extension

                    dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    if (dlg.ShowDialog() == true)
                    {
                        try
                        {
                            if (this.FullName == dlg.FileName)
                            {
                                try
                                {
                                    FileInfo info = new FileInfo(this.FullName);
                                    var fileName = info.Name;
                                    var tempFileName = "temp_" + fileName;
                                    var tempFilePath = dlg.FileName.Replace(fileName, tempFileName);
                                    File.Copy(this.FullName, tempFilePath, true);
                                    File.Delete(this.FullName);
                                    if (System.IO.File.Exists(tempFilePath))
                                    {
                                        System.IO.File.Move(tempFilePath, this.FullName);
                                        File.Delete(tempFilePath);
                                    }
                                }
                                catch
                                {
                                    return;
                                }
                                //if (AppData.CanInternetAction())
                                //{
                                //    SaveAsDownLoadFile(dlg.FileName);
                                //}
                            }
                            else
                            {
                                File.Copy(this.FullName, dlg.FileName, true);
                            }
                        }
                        catch
                        {
                            AppData.MainMV.TipMessage = "文件已被占用，替换失败!";
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 另存为下载文件
        /// </summary>
        private void SaveAsDownLoadFile(string fileName)
        {

            try
            {
                File.Delete(fileName);
            }
            catch
            {
                AppData.MainMV.TipMessage = "文件已被占用，替换失败!";
                return;
            }
            this.FileState = FileStates.Receiving;
            _operateTask = new System.Threading.CancellationTokenSource();
            if (!AcioningItems.Contains(this))
            {
                AcioningItems.Add(this);
            }
            _targetModel.Content = fileName;
            ChatViewModel chatVM = AppData.MainMV.ChatListVM.SelectedItem;
            IMClient.Helper.MessageHelper.LoadFileContent(_targetModel, _operateTask, chatVM, (result, errorState) =>
            {
                AcioningItems.Remove(this);
                if (_operateTask != null)
                {
                    _operateTask.Cancel();
                    _operateTask = null;
                }
                if (result)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        this.FileState = FileStates.Completed;
                    });
                }
            }, fileName);
        }
        /// <summary>
        /// 删除
        /// </summary>
        private void Delete()
        {
            //没有太好的关联方式，目前暂时做成当前窗口（因为只有当前窗口可能删除对应信息）；
            ChatViewModel chatVM = AppData.MainMV.ChatListVM.SelectedItem;
            if (chatVM != null)
            {
                AcioningItems.Remove(this);
                if (_operateTask != null)
                {
                    _operateTask.Cancel();
                    _operateTask = null;
                }
                chatVM.HideMessageCommand.Execute(_targetModel);

                string filePath = System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.filePath, _targetModel.ResourceModel.FileName);
                //if (_targetModel.ResourceModel.FullName == filePath && File.Exists(filePath))
                //{
                //    try
                //    {
                //        File.Delete(filePath);
                //    }
                //    catch
                //    {

                //    }
                //}
            }
        }

        /// <summary>
        /// 取消
        /// </summary>
        public void Cancel(bool isInner = false)
        {
            try
            {
                if (_targetModel != null && _targetModel.MessageState == MessageStates.Warn)
                {
                    return;
                }
                AcioningItems.Remove(this);
                if (_targetModel.ResourceModel.Progress > 0.98 && _targetModel.MessageState != MessageStates.Fail)
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

                    }
                    if (isInner)
                    {
                        return;
                    }

                    if (this.FileState == FileStates.SendOnline ||
                        _targetModel.ResourceModel.RefInfo is SDKClient.Model.OnlineFileBody body) //在线消息会发送取消给对方
                    {
                        SDKClient.SDKProperty.chatType messageType = ChatViewModel.IsGroup ? SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;

                        SDKClient.SDKProperty.RetractType rType = _targetModel.Sender.ID == AppData.Current.LoginUser.ID ?
                            SDKClient.SDKProperty.RetractType.SourceEndOnlineRetract : SDKClient.SDKProperty.RetractType.TargetEndOnlineRetract;
                        SDKClient.SDKClient.Instance.SendRetractMessage(_targetModel.MsgKey, ChatViewModel.ID.ToString(), messageType, ChatViewModel.IsGroup ? ChatViewModel.ID : 0, rType);
                        if (this.FileState == FileStates.SendOnline)
                            UpdateToCancelMsg("发送");
                        else
                            UpdateToCancelMsg("接收");
                    }
                    else //离线消息不发送
                    {
                        if (_targetModel.Sender.ID == AppData.Current.LoginUser.ID && !_targetModel.IsSync)
                        {
                            //int roomID = ChatViewModel.Model.ID;
                            //SDKClient.SDKClient.Instance.AppendLocalData_NotifyMessage(AppData.Current.LoginUser.ID.ToString(), roomID.ToString(), msg, roomID,
                            //    SDKClient.SDKProperty.MessageType.notification);
                            //var errorInfo = UpdateToCancelMsg("发送");
                            if (_targetModel.MessageState == MessageStates.Fail)
                            {
                                int roomID = ChatViewModel.Model.ID;
                                var chatType = ChatViewModel.IsGroup ? SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;
                                Task.Run(async () =>
                                {
                                    //await SDKClient.SDKClient.Instance.AppendLocalData_NotifyMessage(AppData.Current.LoginUser.ID.ToString(), roomID.ToString(),
                                    //    errorInfo, roomID, SDKClient.SDKProperty.MessageType.notification, chatType: SDKClient.SDKProperty.chatType.groupChat);
                                    await SDKClient.SDKClient.Instance.DeleteMsgToMsgID(_targetModel.MsgKey);
                                });
                            }

                        }
                        else if (_targetModel.Sender.ID == AppData.Current.LoginUser.ID && _targetModel.IsSync)
                        {
                            SDKClient.SDKClient.Instance.CancelOfflineFileRecv(_targetModel.MsgKey);
                            //UpdateToCancelMsg("发送");
                        }
                        else
                        {
                            SDKClient.SDKClient.Instance.CancelOfflineFileRecv(_targetModel.MsgKey);
                            //UpdateToCancelMsg("接收");
                        }

                        _targetModel.Sender = null;
                    }



                    _targetModel.ResourceModel.FileState = FileStates.Fail;

                    AppData.MainMV.ChatListVM.SelectedItem?.DisplayHint();
                    //if(SDKClient.SDKProperty.filePath)

                }
            }
            catch
            {

            }
        }

        private string UpdateToCancelMsg(string tip)
        {
            string size = Helper.FileHelper.FileSizeToString(_targetModel.ResourceModel.Length);
            string msg = $"您取消了\"{_targetModel.ResourceModel.FileName}\"({size})的{tip}，文件传输失败。";

            _targetModel.MsgType = MessageType.notification;
            _targetModel.MessageState = MessageStates.Fail;
            _targetModel.Content = msg;
            _targetModel.ResourceModel.FileState = FileStates.Fail;
            ChatViewModel.UpdateMsg(_targetModel);
            return msg;
        }



        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <returns></returns>
        private bool FileExists(bool isSaveAs = false)
        {
            if (File.Exists(this.FullName))
            {
                return true;
            }
            else
            {
                Task.Delay(50).ContinueWith(t =>
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (this.FileState == FileStates.Completed && (_targetModel.ResourceModel.RefInfo is SDKClient.Model.OnlineFileBody || _isOnlineSendFile))
                        {
                            MessageBox.ShowDialogBox("文件被删除或更改？", "提示", "确定", false);
                        }
                        else
                        {
                            if (_targetModel.IsMine && (!_targetModel.IsSync || _targetModel.ResourceModel.FileState == FileStates.Completed))
                            {
                                MessageBox.ShowDialogBox("文件被删除或更改？", "提示", "确定", false);
                            }
                            else
                            {
                                bool reDownLoad = MessageBox.ShowDialogBox("文件被删除或更改，是否重新下载？", "提示", "重新下载");
                                if (reDownLoad)
                                {
                                    if (isSaveAs)
                                    {
                                        this.FileState = FileStates.WaitForReceieve;
                                        this.SaveAs();
                                    }
                                    else
                                    {
                                        CalFileState();
                                        this.Receive();
                                    }
                                }
                            }
                        }
                    }));
                });

                //AppData.MainMV.TipMessage = "此文件不存在，可能被删除或者移动到其他位置。";
                return false;
            }
        }

        #endregion

        private void menu_Opened(object sender, RoutedEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                this.menu.ItemsSource = FileMenuItems();
            });
            //if (_completeTime == null)
            //{
            //    return;
            //}
            //if (e.Source is ContextMenu item && item.DataContext is MessageModel msg)
            //{
            //    if ((DateTime.Now - _completeTime.Value).TotalMinutes >= 2 || !msg.IsMine)
            //    {
            //        if (this.menu.Items.Contains(this._miWithDraw))
            //        {
            //            this.menu.Items.Remove(this._miWithDraw);
            //        }
            //    }
            //    else
            //    {
            //        if (!this.menu.Items.Contains(this._miWithDraw))
            //        {
            //            if (msg.MessageState == MessageStates.Success || msg.MessageState == MessageStates.None)
            //                this.menu.Items.Add(this._miWithDraw);

            //        }
            //    }
            //    if (msg.ResourceModel.FileState == FileStates.Fail || msg.MessageState == MessageStates.Warn || msg.MessageState == MessageStates.Loading)
            //    {
            //        MenuItem menuItem = null;
            //        foreach (var child in this.menu.Items)
            //        {
            //            MenuItem mi = child as MenuItem;
            //            if (mi != null)
            //            {
            //                if (mi.Header.ToString().Equals("转发"))
            //                    menuItem = mi;
            //            }
            //        }
            //        if (menuItem != null)
            //            this.menu.Items.Remove(menuItem);
            //    }

            //int i = 0;
            //foreach (UIElement ui in this.ugState.Children)
            //{
            //    if (ui.Visibility == Visibility.Visible && ui is ButtonBase btn)
            //    {
            //        i++;
            //        if (_targetModel.ResourceModel.RefInfo is SDKClient.Model.OnlineFileBody || _isOnlineSendFile)
            //            continue;
            //        if (btn.Uid == "Open")
            //        {
            //            this.menu.Items.Insert(i,new MenuItem() { Uid = "Forward", Header = "转发" });
            //        }
            //    }

            //}
            //}
        }

        private void gridLayout_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_targetModel == null) return;
            if (_targetModel.ResourceModel.FileState == FileStates.SendOffline || _targetModel.ResourceModel.FileState == FileStates.Completed)
            {
                //if (FileExists())
                //{
                if (IsAllowedExtension(this.FullName))
                    this.Open();
                else
                    OpenFileDirectory();
                //}

            }
            else if (_targetModel.ResourceModel.FileState == FileStates.WaitForReceieve)
            {
                this.Receive();
            }
        }
        private bool IsAllowedExtension(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            string exten = System.IO.Path.GetExtension(filePath).ToLower();
            switch (exten)
            {
                case ".doc":
                case ".docx":
                case ".xls":
                case ".xlsx":
                case ".ppt":
                case ".txt":
                case ".pdf":
                case ".xml":
                case ".html":
                case ".vsd":
                    return true;
            }
            return false;
        }
    }

    public enum FileExtension
    {
        JPG = 255216,
        GIF = 7173,
        BMP = 6677,
        PNG = 13780,
        COM = 7790,
        EXE = 7790,
        DLL = 7790,
        RAR = 8297,
        ZIP = 8075,
        XML = 6063,
        HTML = 6033,
        ASPX = 239187,
        CS = 117115,
        JS = 119105,
        TXT = 210187,
        SQL = 255254,
        BAT = 64101,
        BTSEED = 10056,
        RDP = 255254,
        PSD = 5666,
        PDF = 3780,
        CHM = 7384,
        LOG = 70105,
        REG = 8269,
        HLP = 6395,
        DOC = 208207,
        XLS = 208207,
        DOCX = 208207,
        XLSX = 208207,
    }
}
