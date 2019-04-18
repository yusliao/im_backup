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
using CSClient.ViewModels;
using System.Windows.Controls.Primitives;
using CSClient.Converter;

namespace CSClient.Views.Controls
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
        MessageModel _targetModel;
        /// <summary>
        /// 操作线程任务
        /// </summary>
        System.Threading.CancellationTokenSource _operateTask;
        MenuItem _miWithDraw;

        public bool IsInDocument { get; set; }
        public FileChatItem()
        {
            InitializeComponent();
            _miWithDraw = new MenuItem()
            {
                Uid = "WITHDRAWfile",
                Header = "撤回",
            };
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
            //if (e.NewValue == this.pbProcess.Maximum)
            //{
            //    this.pbProcess.Visibility = Visibility.Collapsed;
            //    //this.bdLayout.BorderThickness = new Thickness(2);
            //    //this.bdLayout.CornerRadius = new CornerRadius(2);

            //    this.gridLayout.ContextMenu = this.HasContexMenu ? this.menu : null;

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
            if (ChatViewModel == null)
            {
                ChatViewModel = AppData.MainMV.ChatListVM.SelectedItem;
            }
            else if (_targetModel.MessageState == MessageStates.Fail)
            {
                if (_operateTask != null)
                {
                    _operateTask.Cancel();
                }
                AcioningItems.Remove(this);
            }

            if (_targetModel == null && this.DataContext is MessageModel item)
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

            if (this.IsMainView == false)
            {
                if (_targetModel.ResourceModel.FileState != FileStates.None)
                {
                    return;
                }
            }
            //是否发送者
            bool isSender = _targetModel.Sender.ID == AppData.Current.LoginUser.ID;
            _targetModel.ResourceModel.FileImg = Helper.FileHelper.GetFileImage(this.FullName, false);
            this.gridLayout.ToolTip = System.IO.Path.GetFileName(this.FullName);
            if (File.Exists(this.FullName))
            {
                if (isSender && _targetModel.IsSending)
                {
                    if (this._targetModel.ResourceModel.FileState == FileStates.SendOffline || this._targetModel.ResourceModel.FileState == FileStates.SendOnline)
                    {

                    }
                    else
                    {
                        this.FileState = _targetModel.ResourceModel.FileState = FileStates.SendOnline;
                        this.OnlineSend();
                    }
                }
                else
                {
                    this.FileState = _targetModel.ResourceModel.FileState = FileStates.Completed;
                    _completeTime = DateTime.Now.AddMinutes(-5);
                }
            }
            else
            {
                if (isSender) //发送者
                {
                    this.FileState = _targetModel.ResourceModel.FileState = FileStates.Completed;
                    _completeTime = DateTime.Now.AddMinutes(-5);
                }
                else
                {
                    this.FileState = _targetModel.ResourceModel.FileState = FileStates.WaitForReceieve;
                }
            }
        }

        private void SetFileStatesView()
        {
            this.pbProcess.Visibility = Visibility.Collapsed;
            this.BdIcon.Visibility = Visibility.Collapsed;
            foreach (UIElement ui in this.ugState.Children)
            {
                ui.Visibility = Visibility.Collapsed;
            }

            switch (this.FileState)
            {
                case FileStates.Fail:
                    if (ChatViewModel != null)
                    {
                        ChatViewModel.UpdateMsg(_targetModel);

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
                    this.btnOfflineSend.Visibility =
                    this.btnCancel.Visibility = Visibility.Visible;

                    this.pbProcess.Visibility = Visibility.Visible;
                    break;
                case FileStates.SendOffline:
                    this.btnOnlineSend.Visibility =
                    this.btnCancel.Visibility = Visibility.Visible;
                    this.pbProcess.Visibility = Visibility.Visible;

                    if (AppData.MainMV.ChatListVM.SelectedItem.IsGroup) //群聊不能 在线发送
                    {
                        this.btnOnlineSend.Visibility = Visibility.Collapsed;
                        this.tbConnecting.Visibility = Visibility.Visible;
                    }
                    break;
                case FileStates.WaitForReceieve:
                    this.btnReceive.Visibility =
                    this.btnSaveAs.Visibility =
                    this.btnCancel.Visibility = Visibility.Visible;
                    break;
                case FileStates.Receiving:
                    this.btnCancel.Visibility = Visibility.Visible;
                    this.pbProcess.Visibility = Visibility.Visible;
                    break;
                case FileStates.Completed:
                    this.btnOpen.Visibility =
                    this.btnSaveAs.Visibility =
                    this.btnDelete.Visibility = Visibility.Visible;
                    _completeTime = DateTime.Now;
                    this.BdIcon.Visibility = Visibility.Visible;
                    _operateTask = null;
                    AcioningItems.Remove(this);
                    if (File.Exists(this.FullName))
                    {
                        _targetModel.ResourceModel.FileImg = Helper.FileHelper.GetFileImage(this.FullName, false);//  Helper.WindowsThumbnailProvider.GetFileThumbnail(this.FullName);
                    }
                    this.pbProcess.Visibility = Visibility.Collapsed;
                    break;
            }

            if (this.IsInDocument)
            {
                this.btnDelete.Visibility = Visibility.Collapsed;
                this.btnCancel.Visibility = Visibility.Collapsed;
            }
            this.menu.Items.Clear();
            foreach (UIElement ui in this.ugState.Children)
            {
                if (ui.Visibility == Visibility.Visible && ui is ButtonBase btn)
                {
                    this.menu.Items.Add(new MenuItem() { Uid = btn.Uid, Header = btn.Content });
                }
            }
        }


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source is Control item)
            {
                switch (item.Uid)
                {
                    case "OfflineSend":
                        if (AppData.CanInternetAction())
                            this.OfflineSend();
                        break;
                    case "OnlineSend":
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
                    case "Cancel":
                        this.Cancel();
                        break;
                    case "WITHDRAWfile":
                        this.WithDraw(item.DataContext as MessageModel);
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
        /// 转离线发送
        /// </summary>
        private void OfflineSend()
        {

            if (ChatViewModel != null)
            {
                _targetModel.CanOperate = false;
                this.tbConnecting.Visibility = Visibility.Visible;

                if (!string.IsNullOrEmpty(_targetModel.MsgKey))
                {
                    SDKClient.SDKProperty.chatType messageType = ChatViewModel.IsGroup ? SDKClient.SDKProperty.chatType.groupChat : SDKClient.SDKProperty.chatType.chat;
                    SDKClient.SDKClient.Instance.SendRetractMessage(_targetModel.MsgKey,
                        ChatViewModel.ID.ToString(), messageType, ChatViewModel.IsGroup ? ChatViewModel.ID : 0, SDKClient.SDKProperty.RetractType.OnlineToOffline);

                    string size = Helper.FileHelper.FileSizeToString(_targetModel.ResourceModel.Length);
                    string content = $"您取消了\"{_targetModel.ResourceModel.FileName}\"({size})的发送，文件传输失败。";

                    MessageModel cancel = new MessageModel()
                    {
                        MsgKey = _targetModel.MsgKey,
                        Content = content,
                        MsgType = MessageType.notification,
                        MessageState = MessageStates.Fail,
                    };
                    ChatViewModel.Chat.Messages.Insert(ChatViewModel.Chat.Messages.Count - 1, cancel);

                    AcioningItems.Remove(this);
                    if (_operateTask != null)
                    {
                        _operateTask.Cancel();
                        _operateTask = null;
                    }
                }

                if (FileExists())
                {
                    AcioningItems.Add(this);
                    _operateTask = new System.Threading.CancellationTokenSource();
                    ChatViewModel.SendOfflineFile(_targetModel, _operateTask, (result) =>
                    {
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
                    ChatViewModel.Chat.Messages.Remove(_targetModel);
                }
            }
        }



        /// <summary>
        /// 在线发送
        /// </summary>
        private void OnlineSend()
        {
            if (ChatViewModel != null)
            {
                _targetModel.CanOperate = false;
                AcioningItems.Remove(this);
                if (_operateTask != null)
                {
                    _operateTask.Cancel();
                    _operateTask = null;
                }

                if (FileExists())
                {
                    AcioningItems.Add(this);
                    _operateTask = new System.Threading.CancellationTokenSource();
                    if (ChatViewModel.IsGroup)
                    {
                        this.tbConnecting.Visibility = Visibility.Visible;
                        //群聊只能离线发送
                        ChatViewModel.SendOfflineFile(_targetModel, _operateTask, (result) =>
                        {
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
                            if (_operateTask != null)
                            {
                                _operateTask.Cancel();
                                _operateTask = null;
                            }
                        });
                    }
                }
                else
                {
                    ChatViewModel.Chat.Messages.Remove(_targetModel);
                }
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
                this.FileState = FileStates.Receiving;
                _operateTask = new System.Threading.CancellationTokenSource();
                AcioningItems.Add(this);

                CSClient.Helper.MessageHelper.LoadFileContent(_targetModel, _operateTask, ChatViewModel, (result) =>
                {
                    AcioningItems.Remove(this);
                    if (_operateTask != null)
                    {
                        _operateTask.Cancel();
                        _operateTask = null;
                    }
                });
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
                    System.Diagnostics.Process.Start(this.FullName);
                }
                catch (Exception ex)
                {
                    AppData.MainMV.TipMessage = $"{ex.Message}\r\n请指定打开该类型文件的程序!";
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

                    Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                    sfd.FileName = _targetModel.ResourceModel.FileName;
                    //sfd.Filter = System.IO.Path.GetExtension(_targetModel.ResourceModel.FileName);
                    if (sfd.ShowDialog() == true)
                    {
                        try
                        {
                            File.Delete(sfd.FileName);
                        }
                        catch
                        {
                            AppData.MainMV.TipMessage = "文件已被占用，替换失败!";
                            return;
                        }
                        this.FileState = FileStates.Receiving;
                        _operateTask = new System.Threading.CancellationTokenSource();
                        AcioningItems.Add(this);
                        ChatViewModel chatVM = AppData.MainMV.ChatListVM.SelectedItem;
                        CSClient.Helper.MessageHelper.LoadFileContent(_targetModel, _operateTask, chatVM, (result) =>
                       {
                           AcioningItems.Remove(this);
                           if (_operateTask != null)
                           {
                               _operateTask.Cancel();
                               _operateTask = null;
                           }
                       }, sfd.FileName);
                    }
                }
            }
            else
            {
                if (FileExists())
                {
                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.FileName = System.IO.Path.GetFileName(this.FullName); // Default file name
                    dlg.DefaultExt = System.IO.Path.GetExtension(this.FullName);// fileName.Split('.').LastOrDefault(); // Default file extension

                    dlg.Filter = string.Format("文件 (.{0})|*.{0}", dlg.DefaultExt);// // Filter files by extension

                    dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    if (dlg.ShowDialog() == true)
                    {
                        try
                        {
                            File.Copy(this.FullName, dlg.FileName, true);
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
            AcioningItems.Remove(this);
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

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <returns></returns>
        private bool FileExists()
        {
            if (File.Exists(this.FullName))
            {
                return true;
            }
            else
            {
                AppData.MainMV.TipMessage = "此文件不存在，可能被删除或者移动到其他位置。";
                return false;
            }
        }

        #endregion

        private void menu_Opened(object sender, RoutedEventArgs e)
        {
            if (_completeTime == null)
            {
                return;
            }
            if (e.Source is ContextMenu item && item.DataContext is MessageModel msg)
            {
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
                        this.menu.Items.Add(this._miWithDraw);
                    }
                }
            }
        }
    }
}
