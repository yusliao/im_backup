using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Microsoft.Win32;
using IMModels;
using CSClient.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using CSClient.Views.ChildWindows;
using CSClient.Helper;

namespace CSClient.Views.Controls
{
    /// <summary>
    /// MessageEditor.xaml 的交互逻辑
    /// </summary>
    public partial class MessageEditor : UserControl
    {
        public event Func<ChatViewModel> GotDataContext;

        /// <summary>
        /// 输入时是否需要识别'@'字符
        /// </summary>
        bool _isRecognizeAt;
        GroupModel _groupModel;
        ObservableCollection<GroupMember> _groupMember;

        IM.Emoje.EmojeBox emojePanel;

        public MessageEditor()
        {
            InitializeComponent();

            this.Document = this.richBox.Document = new FlowDocument();
            try
            {
                emojePanel = new IM.Emoje.EmojeBox();
                emojePanel.Selected += EmojePanel_Close;
                this.bdEmoje.Child = emojePanel;
            }
            catch
            {

            }
            this.GotFocus += MessageEditor_GotFocus;
            this.richBox.PreviewDragOver += RichBox_DragOver;
            this.richBox.PreviewDrop += RichBox_Drop;

            this.Loaded += delegate
            {
                this.richBox.Focus();
                ChatViewModel chatViewModel = this.GotDataContext?.Invoke();
                chatViewModel.OnFastReply -= ChatViewModel_OnFastReply;
                chatViewModel.OnFastReply += ChatViewModel_OnFastReply;
                App.Current.Dispatcher.Invoke(() =>
                {
                    if (!_isEvaluate)
                    {
                        this.btnEvaluate.IsEnabled = !chatViewModel.IsSessionEnd;
                    }
                    else
                    {
                        this.btnEvaluate.IsEnabled = false;
                    }

                    this.btnStopSession.IsEnabled = !chatViewModel.IsSessionEnd;
                    this.btnChangeSession.IsEnabled = !chatViewModel.IsSessionEnd;
                });
            };

            //DataObject.AddPastingHandler(this.richBox, (s, e) => { e.CancelCommand(); e.Handled = true; });

            this.richBox.AllowDrop = true;
            this.richBox.IsUndoEnabled = false;
            this.richBox.PreviewKeyDown += RichBox_PreviewKeyDown;
            this.richBox.TextChanged += richBox_TextChanged;
            this.richBox.PreviewMouseRightButtonDown += RichBox_PreviewMouseRightButtonDown;
            this.richBox.PreviewMouseLeftButtonDown += RichBox_PreviewMouseLeftButtonDown;
        }

        private void ChatViewModel_OnFastReply(string content)
        {
            this.AddTextItem(content);
        }

        public void StartOrStopSession(bool isStart)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (isStart)
                {
                    this._isEvaluate = false;
                }
                else
                {
                    this._isEvaluate = true;
                }
                this.btnStopSession.IsEnabled = isStart;
                this.btnEvaluate.IsEnabled = isStart;
                this.btnChangeSession.IsEnabled = isStart;
            });
        }

        private void MessageEditor_GotFocus(object sender, RoutedEventArgs e)
        {
            this.richBox.Focus();
        }

        private void RichBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is ChatImage chatImg)
            {
                if (File.Exists(chatImg.ImagePath))
                {
                    ChildWindows.ImageScanWindow.ShowScan(chatImg.ImagePath);
                }

                e.Handled = true;
            }
        }

        private void RichBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is ChatImage chatImg)
            {
                e.Handled = true;
                this.miImgSaveAs.Visibility = Visibility.Visible;
                this.miCopy.Tag =
                this.miImgSaveAs.Tag = chatImg;
            }
            else
            {
                this.miCopy.Tag =
                this.miImgSaveAs.Tag = null;
                this.miImgSaveAs.Visibility = Visibility.Collapsed;
            }
        }

        private void RichBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Enter)
                {
                    return;
                }

                switch (e.Key)
                {
                    case Key.X:
                        this.CopyData(true);
                        break;
                    case Key.C:
                        this.CopyData();
                        break;
                    case Key.V:
                        this._isPaste = true;
                        this.DoPaste();
                        break;
                    case Key.A: //暂时保留全选
                        return;

                }
                e.Handled = true;
            }

            if (this.ppMember.IsOpen)
            {
                if (e.Key == Key.Up)
                {
                    int idx = listMember.SelectedIndex;
                    if (idx == 0)
                    {
                        listMember.SelectedItem = listMember.Items[0];
                    }
                    else
                    {
                        listMember.SelectedItem = listMember.Items[idx - 1];
                    }
                    this.listMember.ScrollIntoView(listMember.SelectedItem);
                }
                else if (e.Key == Key.Down)
                {
                    int idx = listMember.SelectedIndex;
                    if (idx == listMember.Items.Count - 1)
                    {
                        listMember.SelectedItem = listMember.Items[listMember.Items.Count - 1];
                    }
                    else
                    {
                        listMember.SelectedItem = listMember.Items[idx + 1];
                    }

                    this.listMember.ScrollIntoView(listMember.SelectedItem);
                }
                else if (e.Key == Key.Enter)
                {
                    GroupMember member = this.listMember.SelectedItem as GroupMember;
                    this.DeleteChar();
                    AddAtItem(member);
                    this.ppMember.IsOpen = false;
                    e.Handled = true;
                }
            }

        }

        private void RichBox_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void RichBox_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                this.AddFileItems(files);
            }
            else if (e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                string value = e.Data.GetData(DataFormats.UnicodeText).ToString();
                this.AddTextItem(value);
            }
        }

        private void EmojePanel_Close(IM.Emoje.EmojeItem target)
        {
            this.richBox.Selection.Text = string.Empty;
            AddEmoje(emojePanel.SelectEmoje.ImageSource, emojePanel.SelectEmoje.Name);
            //System.Windows.Forms.SendKeys.SendWait("^{END}");//将光标移动到最后
        }

        public FlowDocument Document
        {
            get { return (FlowDocument)GetValue(DocumentProperty); }
            private set { SetValue(DocumentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Document.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(FlowDocument), typeof(MessageEditor),
                new PropertyMetadata());

        #region button  click events

        static string ImageFilter = @"所有图片|*.gif;*.jpeg;*.jpg;*.png;*.bmp";
        private void btnImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Reset();
            dialog.Filter = ImageFilter;
            dialog.Multiselect = true;
            if (dialog.ShowDialog(App.Current.MainWindow) == true)
            {
                this.richBox.Selection.Text = string.Empty;

                if (dialog.FileNames.Length > 10)
                {
                    ViewModels.AppData.MainMV.TipMessage = "单次发送图片不能超过10张，你要控制你自己";
                }

                foreach (string path in dialog.FileNames.Take(10))
                {
                    AddImageItem(path);
                }
            }
        }

        private void btnClip_Click(object sender, RoutedEventArgs e)
        {
            string savePath = Views.Controls.ClipWindow.ShowClip();

            if (System.IO.File.Exists(savePath))
            {
                this.richBox.Selection.Text = string.Empty;
                AddImageItem(savePath);
            }
        }

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog() { Multiselect = true };
            dialog.Filter = @"所有文件|*.*|   
                              文本文件 |*.txt;*.pdf;*.doc;*.docx;*.xls;*.xlsx;*.ppt;*.pptx| 
                              图片|*.gif;*.jpeg;*.jpg;*.png;*.bmp| 
                              音视频|*.mp4;*.mp3|  
                              压缩文件|*.rar;*.zip";
            dialog.Multiselect = true;

            if (dialog.ShowDialog(App.Current.MainWindow) == true)
            {
                this.richBox.Selection.Text = string.Empty;
                AddFileItems(dialog.FileNames);
            }
        }

        #endregion


        #region methods

        private void AddEmoje(BitmapImage imgSource, string key)
        {
            //ChatImage img = new ChatImage(emojePath, new Size(30, 30))
            //{
            //    Uid = ViewModels.AppData.FlagEmoje + key,
            //    //Uid = emojePanel.SelectEmoji.Key,
            //};

            Image img = new Image()
            {
                Margin = new Thickness(1),
                Width = 28,
                Height = 28,
                Source = imgSource,
                Uid = ViewModels.AppData.FlagEmoje + key,
            };


            this.AddUI(img);
        }

        private void AddFileItems(string[] files)
        {
            bool hasNull = false;

            foreach (var path in files)
            {
                if (System.IO.File.Exists(path))
                {
                    if (IsFileInUse(path))
                    {
                        AppData.MainMV.TipMessage = $"[{System.IO.Path.GetFileName(path)}]文件已被占用，无法进行传输！ ";
                        continue;
                    }
                    FileInfo fileInfo = new System.IO.FileInfo(path);
                    if (fileInfo.Length == 0)
                    {
                        hasNull = true;
                        continue;
                    }
                    else
                    {
                        FilePackage file = new FilePackage()
                        {
                            FileInfo = fileInfo,
                            Uid = ViewModels.AppData.FlagFile + path
                        };
                        this.AddUI(file);
                    }
                }
            }

            if (hasNull)
            {
                ViewModels.AppData.MainMV.TipMessage = "文件大小不能为空!";
            }

        }

        public void AddImageItem(string imgPath)
        {
            if (GetImageCount() < 10)
            {
                if (File.Exists(imgPath))
                {
                    FileInfo fileInfo = new FileInfo(imgPath);
                    if (fileInfo.Length < 50 * 1024 * 1024)
                    {
                        ChatImage img = new ChatImage(imgPath, new Size(320, 180))
                        {
                            Uid = ViewModels.AppData.FlagImage + imgPath
                        };
                        this.AddUI(img);
                    }
                    else
                    {
                        ViewModels.AppData.MainMV.TipMessage = "图片超过50MG,建议文件形式发送！";
                    }
                }
            }

        }

        private void AddTextItem(string text)
        {

            Run run = new Run(text, this.richBox.CaretPosition);
            this.richBox.CaretPosition = run.ElementEnd;
            this.richBox.Focus();
        }

        private InlineUIContainer AddUI(UIElement ui)
        {
            var container = new InlineUIContainer(ui, this.richBox.CaretPosition);
            this.richBox.CaretPosition = container.ElementEnd;

            this.richBox.Focus();
            return container;

        }

        #endregion

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            try
            {

                this.miCopy.IsEnabled = this.miCut.IsEnabled = !this.richBox.Selection.IsEmpty;
                this.miPaste.IsEnabled = Clipboard.GetText() != null;
            }
            catch
            {

            }

        }

        private void ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source is MenuItem item)
            {
                switch (item.Uid)
                {
                    case "CUT":
                        CopyData(true);
                        break;
                    case "COPY":
                        CopyData();
                        break;
                    case "PASTE":
                        DoPaste();
                        break;
                    case "IMGSAVEAS":

                        if (item.Tag is ChatImage chatImg && File.Exists(chatImg.ImagePath))
                        {
                            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                            dlg.FileName = System.IO.Path.GetFileName(chatImg.ImagePath); //  
                            dlg.DefaultExt = System.IO.Path.GetExtension(chatImg.ImagePath);//  

                            dlg.Filter = string.Format("图片 (.{0})|*.{0}", dlg.DefaultExt);//  

                            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                            if (dlg.ShowDialog() == true && chatImg.ImagePath != dlg.FileName)
                            {
                                File.Copy(chatImg.ImagePath, dlg.FileName, true);
                            }
                        }
                        break;
                }
            }
        }

        private void SendToUIThread(string text)
        {
            this.richBox.Dispatcher.BeginInvoke(new Action(() => { System.Windows.Forms.SendKeys.Send(text); }),
                System.Windows.Threading.DispatcherPriority.Input);
        }

        private void DoPaste()
        {
            this.richBox.Selection.Text = string.Empty;
            if (Clipboard.ContainsFileDropList())
            {
                var files = Clipboard.GetFileDropList();
                foreach (var f in files)
                {
                    var tension = System.IO.Path.GetExtension(f).ToLower();
                    if (ImageFilter.Contains(tension))
                    {
                        AddImageItem(f);
                    }
                    else
                    {
                        AddFileItems(new string[] { f });
                    }
                }
            }
            else if (Clipboard.ContainsImage())
            {
                BitmapSource bitmap = Clipboard.GetImage();
                string imgPath = Helper.ImageDeal.SaveBitmapImageIntoFile(bitmap);

                AddImageItem(imgPath);
                //内存同样图片，已经创建一次，直接赋值给文件形式  
                //Clipboard.SetData(ViewModels.AppData.FlagCopy, new List<string> { ViewModels.AppData.FlagImage + imgPath });
            }
            else
            {
                var formats = Clipboard.GetDataObject().GetFormats();
                if (formats.Contains(DataFormats.Rtf))
                {
                    #region Load rtf content
                    var rtf = Clipboard.GetData(DataFormats.Rtf);
                    //将rtf载入内存内的RichTextBox 
                    using (MemoryStream rtfMemoryStream = new MemoryStream())
                    {
                        using (StreamWriter rtfStreamWriter = new StreamWriter(rtfMemoryStream))
                        {
                            rtfStreamWriter.Write(rtf);
                            rtfStreamWriter.Flush();
                            rtfMemoryStream.Seek(0, SeekOrigin.Begin);
                            //Load the MemoryStream into TextRange ranging from start to end of RichTextBox.
                            this.richBox.Selection.Load(rtfMemoryStream, DataFormats.Rtf);
                        }
                    }

                    var datas = Helper.RichTextBoxHelper.GetSelectionItems(this.richBox);
                    this.richBox.Selection.Text = string.Empty;
                    var tp = this.richBox.CaretPosition;
                    for (int i = 0; i < datas.Count; i++)
                    {
                        if (datas[i] is Run run)
                        {
                            run = new Run(run.Text, tp);
                            tp = run.ContentEnd;
                        }

                        else if (datas[i] is InlineUIContainer uic)
                        {
                            if (uic.Child is Image img)
                            {
                                string path = img.Source.ToString();
                                if (File.Exists(path))
                                {
                                }
                                else if (img.Source is BitmapSource bitSource)
                                {
                                    path = Helper.ImageDeal.SaveBitmapImageIntoFile(bitSource);
                                }
                                else
                                {
                                    continue;
                                }

                                ChatImage chatImg = new ChatImage(path, new Size(320, 180))
                                {
                                    Uid = ViewModels.AppData.FlagImage + path
                                };

                                uic = new InlineUIContainer(chatImg, tp);
                                tp = uic.ContentEnd;
                            }
                            else
                            {
                                uic.Child.ToString();
                            }
                        }
                        else
                        {
                            //var vv = datas[i].ToString();
                            //vv.ToString();
                        }
                    }

                    #endregion
                }
                else if (formats.Contains(CSClient.ViewModels.AppData.FlagCopy))
                {
                    string textCopy = Clipboard.GetData(CSClient.ViewModels.AppData.FlagCopy).ToString();
                    CSClient.Helper.MessageHelper.AppendContentToRichTextBox(this.richBox, textCopy);
                }
                else if (formats.Contains(DataFormats.Html))
                {
                    string html = Clipboard.GetData(DataFormats.Html).ToString();
                    CSClient.Helper.MessageHelper.AppendHtmlFromClipboard(this.richBox, html);
                }
                else
                {
                    CSClient.Helper.MessageHelper.AppendContentToRichTextBox(this.richBox, Clipboard.GetText());
                }
            }
        }

        #region  Do Copy


        private void CopyData(bool isCut = false)
        {
            //if (this.miCopy.Tag is ChatImage chatImg)
            //{
            //    chatImg.DoCopy();
            //    return;
            //}
            this.cmMenu.IsOpen = false;

            Task.Delay(200).ContinueWith(t =>
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    var datas = Helper.RichTextBoxHelper.GetSelectionItems(this.richBox);
                    if (datas.Count > 0)
                    {
                        Helper.MessageHelper.SetRichTextBoxSelectionToClipboard(datas);
                    }
                    if (isCut)
                    {
                        this.richBox.Selection.Text = string.Empty;
                    }
                }));

            });
        }




        #endregion

        private void richBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = GetText();

            if (text.Length > 2000)
            {
                TextRange textRange = new TextRange(this.richBox.Document.ContentStart, this.richBox.Document.ContentEnd);
                textRange.Text = text.Substring(0, 2000);
                this.richBox.CaretPosition = textRange.End;
            }

            this.RecognizeAt(text, e);
        }

        #region @功能相关

        const string Everyone = "所有人";
        /// <summary>
        /// 输入@后，模糊查找的字符数，用来删掉后替换成'@XXX'控件
        /// </summary>
        int _counter = 0;
        bool _isPaste;
        bool _isAtSomeoneFromChatBox;

        public void AtSomeoneFromChatBox(IChat ichat)
        {
            _isAtSomeoneFromChatBox = true;
            AddAtItem(ichat);
            _isAtSomeoneFromChatBox = false;
        }

        public void AddAtItem(IChat ichat)
        {
            AtPackage at = new AtPackage();
            at.DataContext = ichat;
            var uic = this.AddUI(at);
            uic.BaselineAlignment = BaselineAlignment.Bottom;
        }

        private void GetAtList()
        {
            //App.Current.Dispatcher.Invoke(() =>
            //{
            //    ChatViewModel chatViewModel = this.GotDataContext?.Invoke();

            //    if (chatViewModel==null ||!chatViewModel.IsGroup) return;
            //    if (!AppData.MainMV.GroupListVM.Items.Any(info => info.ID == chatViewModel.ID))
            //    {
            //        this._isRecognizeAt = false;
            //        return;
            //    }

            //    this._isRecognizeAt = true;
            //    this._groupModel = chatViewModel.Chat.Chat as GroupModel;
            //    if (this._groupModel.Members == null) return;

            //    GroupMember[] members = new GroupMember[this._groupModel.Members.Count];
            //    this._groupModel.Members.CopyTo(members, 0);

            //    this._groupMember = null;
            //    this._groupMember = new ObservableCollection<GroupMember>(members.Where(x => x.ID != AppData.Current.LoginUser.ID));

            //    UserModel user = AppData.Current.GetUserModel(AppData.Current.LoginUser.ID);
            //    var sender = user.GetInGroupMember(this._groupModel);
            //    if (this._groupModel.OwnerID == AppData.Current.LoginUser.ID ||
            //        sender.IsManager)
            //    {
            //        GroupMember member = GroupMember.CreateEmpty(ConstString.AtAllId);
            //        member.DisplayName = Everyone;
            //        this._groupMember.Insert(0, member);
            //    }
            //});
        }

        public class OrdinalComparer : System.Collections.Generic.IComparer<String>
        {
            public int Compare(String x, String y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                if (IsNumeric(x) && IsNumeric(y))
                {
                    if (Convert.ToInt32(x) > Convert.ToInt32(y)) return 1;
                    if (Convert.ToInt32(x) < Convert.ToInt32(y)) return -1;
                    if (Convert.ToInt32(x) == Convert.ToInt32(y)) return 0;
                }

                char s1 = CommonHelper.GetFirstChar(x);
                char s2 = CommonHelper.GetFirstChar(y);

                if (s1 == CommonHelper.SORT_CHAR && s2 != CommonHelper.SORT_CHAR)
                    return 1;

                if (s1 != CommonHelper.SORT_CHAR && s2 == CommonHelper.SORT_CHAR)
                    return -1;

                return string.Compare(s1.ToString(), s2.ToString(), true);
            }

            public static bool IsNumeric(object value)
            {
                return int.TryParse(value.ToString(), out int i);
            }
        }

        void RecognizeAt(string text, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(text))
            {
                this.CloseAtPopup();
                return;
            }

            if (!_isRecognizeAt) return;

            if (_isAtSomeoneFromChatBox) return;

            if (text.Contains("@"))
            {
                TextPointer tpCaret = this.richBox.CaretPosition;
                string tpBackwardText = tpCaret.GetTextInRun(LogicalDirection.Backward);

                #region 过滤重复的@
                //如果为英文输入状态时，输入一个@时，会因为用了“Popup遮挡输入法的解决方案”而多次触发TextChanged事件;
                //导致输入一个@界面显示两个@，所以加一个bool字段去判断，删掉一个重复的@。
                if (this.ppMember.IsUpdateWindow)
                {
                    this.ppMember.IsUpdateWindow = false;

                    tpCaret.DeleteTextInRun(-1);
                    e.Handled = true;
                    return;
                }
                #endregion

                int offset = tpCaret.GetOffsetToPosition(this.richBox.Document.ContentStart);
                if (!this._isPaste && offset == -2)
                {
                    this.CloseAtPopup();
                    return;
                }

                this.CalculatePopupLocation(tpCaret);

                this.GetAtList();
                if (!_isRecognizeAt) return;

                string key;
                if (this._isPaste)
                {
                    key = Clipboard.GetText().Split('@').Last();
                    this._isPaste = false;
                }
                else
                    key = tpBackwardText.Split('@').Last();

                try
                {
                    if (string.IsNullOrEmpty(key))
                    {
                        this.listMember.ItemsSource = this._groupMember.OrderBy(x => x.DisplayName, new OrdinalComparer()).OrderBy(x => KeySelecter(x));
                        this.OpenAtPopup(0);
                    }
                    else
                    {
                        if (this._groupMember.Any(x => x.DisplayName.ToLower().Contains(key.ToLower())))
                        {
                            this.listMember.ItemsSource = this._groupMember.Where(x => x.DisplayName.ToLower().Contains(key.ToLower())).OrderBy(x => x.DisplayName, new OrdinalComparer()).OrderBy(x => KeySelecter(x));
                            this.OpenAtPopup(key.Length);
                        }
                        else
                            this.CloseAtPopup();
                    }
                }
                catch
                {
                    this.CloseAtPopup();
                }
            }
            else
                this.CloseAtPopup();
        }

        void CalculatePopupLocation(TextPointer tpCaret)
        {
            var rect = tpCaret.GetCharacterRect(LogicalDirection.Backward);
            if (this._isPaste)
            {
                //如果是通过键盘复制的文本，需要重新计算@列表的弹出位置
                TextPointer tpEnd = this.richBox.Document.ContentEnd;
                var range = new TextRange(tpCaret, tpEnd);

                string pasteText = Clipboard.GetText();

                if ((pasteText + "\r\n").Equals(range.Text))
                {
                    rect = tpCaret.GetNextContextPosition(LogicalDirection.Forward).GetCharacterRect(LogicalDirection.Backward);
                }
            }

            Point p = new Point(rect.X, rect.Y);
            this.ppMember.HorizontalOffset = p.X + 20;
            this.ppMember.VerticalOffset = p.Y + 60;
        }

        int KeySelecter(GroupMember member)
        {
            int i = Array.IndexOf(new string[] { Everyone }, member.DisplayName);
            if (i != -1)
            {
                if (member.ID != ConstString.AtAllId)
                {
                    return int.MaxValue;
                }

                return i;
            }
            else
            {
                return int.MaxValue;
            }
        }

        void OpenAtPopup(int charLength)
        {
            this.listMember.SelectedIndex = 0;
            this.listMember.ScrollIntoView(listMember.Items[0]);

            this.ppMember.IsOpen = true;
            this.ppMember.IsUpdateWindow = false;
            this.richBox.Focus();

            _counter = charLength + 1;
        }

        void CloseAtPopup()
        {
            this.ppMember.IsOpen = false;
            _counter = 0;
        }

        private void grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Grid grid = sender as Grid;
            GroupMember member = grid.DataContext as GroupMember;

            this.DeleteChar();

            AddAtItem(member);

            this.ppMember.IsOpen = false;
            e.Handled = true;
        }

        /// <summary>
        /// 删除字符
        /// </summary>
        void DeleteChar()
        {
            TextPointer tpEnd = this.richBox.CaretPosition;
            tpEnd.DeleteTextInRun(-_counter);

            _counter = 0;
        }
        #endregion

        internal string GetText()
        {
            var sb = new StringBuilder();
            var isFirst = true;
            foreach (var block in this.richBox.Document.Blocks)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sb.AppendLine();

                if (block is Paragraph)
                    foreach (var inline in ((Paragraph)block).Inlines)
                    {
                        if (inline is Run)
                            sb.Append(((Run)inline).Text);
                        else if (inline is LineBreak)
                            sb.AppendLine();
                    }
            }
            return sb.ToString();
        }

        internal int GetImageCount()
        {
            int count = 0;
            var isFirst = true;
            foreach (var block in this.richBox.Document.Blocks)
            {
                if (isFirst)
                    isFirst = false;
                else
                {

                }

                if (block is Paragraph)
                    foreach (var inline in ((Paragraph)block).Inlines)
                    {
                        if (inline is InlineUIContainer ui && ui.Child is ChatImage)
                        {
                            count++;
                        }
                    }
            }

            return count;
        }


        private static bool IsFileInUse(string fileName)
        {
            bool inUse = true;

            FileStream fs = null;
            try
            {

                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read,

                FileShare.None);

                inUse = false;
            }
            catch
            {

            }
            finally
            {
                if (fs != null)

                    fs.Close();
            }
            return inUse;//true表示正在使用,false没有使用  
        }


        bool _isEvaluate = false;
        private void btnEvaluate_Click(object sender, RoutedEventArgs e)
        {
            if (!AppData.CanInternetAction())
            {
                return;
            }
            ChatViewModel chatViewModel = this.GotDataContext?.Invoke();
            if (chatViewModel.sessionType == 1)
            {
                if (!Views.MessageBox.ShowDialogBox("是否确认发送评分？"))
                {
                    return;
                }

                this.btnEvaluate.IsEnabled = false;
                _isEvaluate = true;


                var result = SDKClient.SDKClient.Instance.SendCustiomServerMsg(chatViewModel.ID.ToString(), chatViewModel.SessionId, SDKClient.SDKProperty.customOption.requestappraisal).Result;

               // chatViewModel.SendTextMsgToServer("请您对我的服务做出评价，亲");
                chatViewModel.AddMessageTip("请您对我的服务做出评价，亲", isSetLastMsg: false);
            }
            else if(chatViewModel.sessionType==2)
            {
                AppData.MainMV.ShowTip("当前用户正在和其他客服进行沟通中");
            }

        }

        private void btnStopSession_Click(object sender, RoutedEventArgs e)
        {
            if (!AppData.CanInternetAction())
            {
                return;
            }
            ChatViewModel chatViewModel = this.GotDataContext?.Invoke();
            if (chatViewModel.sessionType == 1)
            {
                if (!Views.MessageBox.ShowDialogBox("是否确认结束会话？"))
                {
                    return;
                }

                this.btnStopSession.IsEnabled = false;
                this.btnEvaluate.IsEnabled = false;
                this.btnChangeSession.IsEnabled = false;

                chatViewModel.IsSessionEnd = true;
                chatViewModel.AddMessageTip("结束聊天");
                chatViewModel.AppendMsg();

                var result = SDKClient.SDKClient.Instance.SendCustiomServerMsg(chatViewModel.ID.ToString(), chatViewModel.SessionId, SDKClient.SDKProperty.customOption.over).Result;
            }
            else if(chatViewModel.sessionType==2)
            {
                AppData.MainMV.ShowTip("当前用户正在和其他客服进行沟通中");
            }

        }
        SessionChangeWindow win;
        private async void btnChangeSession_Click(object sender, RoutedEventArgs e)
        {
            var uits = TaskScheduler.FromCurrentSynchronizationContext();
            await SDKClient.SDKClient.Instance.GetfreeServicers().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    AppData.MainMV.ShowTip("获取客服列表失败");
                    return;
                }
                var lst = t.Result;
                ChatViewModel chatViewModel = this.GotDataContext?.Invoke();
                win = new SessionChangeWindow(chatViewModel.ID);
                if (t.Result.code == 1)
                {
                    win.CSItems = new List<OnlineStatusEntity>();
                    foreach (var item in lst.data)
                    {
                        if (item.imopenid != SDKClient.SDKClient.Instance.property.CurrentAccount.userID.ToString())
                        {
                            OnlineStatusEntity entity = new OnlineStatusEntity()
                            {
                                ID = int.Parse(item.imopenid),
                                UserId = int.Parse(item.imopenid),
                                Nickname = item.nickname,
                                Servicerid = item.servicerid
                            };
                            win.CSItems.Add(entity);
                        }
                    }
                }
                win.Owner = App.Current.MainWindow;
                win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                win.ShowDialog();
            }, uits);
           
        }
    }

}
