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
using IMClient.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using IMClient.Views.ChildWindows;
using IMClient.Helper;
using System.Windows.Media.Animation;

namespace IMClient.Views.Controls
{
    /// <summary>
    /// MessageEditor.xaml 的交互逻辑
    /// </summary>
    public partial class MessageEditor : UserControl
    {
        public event Func<ChatViewModel> GotDataContext;

        /// <summary>
        /// 最大可预览长度（图片、视频）
        /// </summary>
        const long MAXSCANLENGTH = 1024 * 1024 * 10 + 1024;
        const long MAXFILELENGTH = 1024 * 1024 * 101;
        const int fileCount = 10;
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
                emojePanel.Selected += EmojePanel_Selected;
                this.bdEmoje.Child = emojePanel;
            }
            catch
            {

            }
            this.GotFocus += MessageEditor_GotFocus;
            this.richBox.PreviewDragOver += RichBox_DragOver;
            this.richBox.PreviewDrop += RichBox_Drop;
            this.Loaded += MessageEditor_Loaded;
            //DataObject.AddPastingHandler(this.richBox, (s, e) => { e.CancelCommand(); e.Handled = true; });

            this.richBox.AllowDrop = true;
            this.richBox.IsUndoEnabled = false;
            this.richBox.PreviewKeyDown += RichBox_PreviewKeyDown;
            this.richBox.TextChanged += richBox_TextChanged;
            this.richBox.PreviewMouseRightButtonDown += RichBox_PreviewMouseRightButtonDown;
            this.richBox.PreviewMouseLeftButtonDown += RichBox_PreviewMouseLeftButtonDown;
            this.richBox.PreviewTextInput += RichBox_TextInput;
        }

        private void MessageEditor_Loaded(object sender, RoutedEventArgs e)
        {
            this.richBox.Focus();
            this.GetAtList();
        }

        private void RichBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            this.richBox.TextChanged -= richBox_TextChanged;
            this.richBox.TextChanged += richBox_TextChanged;
            this.btnEmoji.IsChecked = false;
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
            else if (e.Source is SmallVideo smallVideo)
            {
                smallVideo.Play();

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
            try
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
                        case Key.Z:
                            this.richBox.Undo();
                            break;
                        case Key.A: //暂时保留全选
                            return;

                    }
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
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

                    e.Handled = true;
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

                    e.Handled = true;
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
                foreach (var f in files)
                {
                    var tension = System.IO.Path.GetExtension(f).ToLower();
                    if (App.ImageFilter.Contains(tension))
                    {
                        AddImageItem(f);
                    }
                    else if (App.VideoFilter.Contains(tension))
                    {
                        AddSmallVideoItem(f);
                    }
                    else
                    {
                        AddFileItemsAsync(new string[] { f });
                    }
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                string value = e.Data.GetData(DataFormats.UnicodeText).ToString();
                this.AddTextItem(value);
            }
        }

        private void EmojePanel_Selected(IM.Emoje.EmojeItem target)
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

        private void btnImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Reset();
            dialog.Filter = string.Format("{0}|{1}|{2}", App.ImageAndVideoFilter, App.ImageFilter, App.VideoFilter);
            dialog.Multiselect = true;


            if (dialog.ShowDialog(App.Current.MainWindow) == true)
            {
                this.richBox.Selection.Text = string.Empty;
                foreach (string path in dialog.FileNames.Take(fileCount))
                {
                    if (App.ImageFilter.Contains(System.IO.Path.GetExtension(path).ToLower()))
                    {
                        AddImageItem(path);
                    }
                    else
                    {
                        AddSmallVideoItem(path);
                    }
                }
                if (dialog.FileNames.Length > fileCount)
                {
                    MessageBox.ShowDialogBox("最多只能选择10个文件。", "提示", "确定", false);
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
                              音视频|*.mp3;*.mp4;*.rmvb;*.wmv;*.avi;*.mkv|  
                              压缩文件|*.rar;*.zip";
            dialog.Multiselect = true;

            if (dialog.ShowDialog(App.Current.MainWindow) == true)
            {
                this.richBox.Selection.Text = string.Empty;
                var files = dialog.FileNames.Take(fileCount).ToArray();
                AddFileItemsAsync(files);
                if (dialog.FileNames.Length > fileCount)
                {
                    MessageBox.ShowDialogBox("最多只能选择10个文件。", "提示", "确定", false);
                }
            }
        }

        private void btnShake_Click(object sender, RoutedEventArgs e)
        {
            var window = App.Current.MainWindow;
            //DoubleAnimation DAnimation = new DoubleAnimation();
            //DAnimation.From = 0;//起点
            //DAnimation.To = 30;//终点
            //DAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.09));//时间
            //DAnimation.AutoReverse = true;
            //DAnimation.RepeatBehavior = new RepeatBehavior(3);
            //DAnimation.FillBehavior = FillBehavior.Stop;
            //DependencyProperty[] propertyChain = new DependencyProperty[]
            //{
            //   UIElement.RenderTransformProperty,
            //   TranslateTransform.XProperty,

            //};
            //RotateTransform rotate = new RotateTransform(0);
            //window.RenderTransform = rotate;
            //Storyboard.SetTarget(DAnimation, window);
            //Storyboard.SetTargetProperty(DAnimation, new PropertyPath("(0).(1)",propertyChain));
            //Storyboard story = new Storyboard();
            //story.Children.Add(DAnimation);
            //story.Begin();

            DoubleAnimation DAnimation = new DoubleAnimation();
            DAnimation.From = window.Left;//起点
            DAnimation.To = window.Left + 10;//终点
            DAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.09));//时间

            Storyboard.SetTarget(DAnimation, window);
            Storyboard.SetTargetProperty(DAnimation, new PropertyPath(Window.LeftProperty));
            Storyboard story = new Storyboard();

            story.Completed += new EventHandler(story_Completed);//完成后要做的事
            DAnimation.RepeatBehavior = new RepeatBehavior(3);
            story.Children.Add(DAnimation);
            story.Begin();
        }
        void story_Completed(object sender, EventArgs e)
        {
            var window = App.Current.MainWindow;
            DoubleAnimation DAnimation = new DoubleAnimation();
            DAnimation.From = window.Left;//起点
            DAnimation.To = window.Left - 10;//终点
            DAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.05));//时间

            Storyboard.SetTarget(DAnimation, window);
            Storyboard.SetTargetProperty(DAnimation, new PropertyPath(Window.LeftProperty));
            Storyboard story = new Storyboard();
            DAnimation.RepeatBehavior = new RepeatBehavior(3);
            story.Children.Add(DAnimation);
            story.Begin();
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
                Tag = ViewModels.AppData.FlagEmoje
            };


            this.AddElement(img);
        }
        /// <summary>
        /// 装载表情图片
        /// </summary>
        /// <param name="faceTag"></param>
        /// <param name="completeUrl"></param>
        public void Win_Face_GetUrl(string faceUrl, string uid, string completeUrl = "")
        {
            try
            {
                string imgurl = faceUrl;
                var bi = string.IsNullOrEmpty(completeUrl) ? new BitmapImage(new Uri("pack://application:,,,/IM.Emoje;component/Image/" + imgurl)) : new BitmapImage(new Uri(completeUrl));
                Image image = new Image
                {
                    Source = bi,
                    Tag = ViewModels.AppData.FlagEmoje,
                    Uid = uid
                };
                image.Height = 28;
                image.Width = 28;

                image.Stretch = System.Windows.Media.Stretch.Fill;
                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
                AddElement(image);
            }
            catch (Exception ex)
            {

            }
        }

        private async Task AddFileItemsAsync(string[] files)
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

                    if (fileInfo.Name.Length > 60)
                    {
                        AppData.MainMV.TipMessage = $"文件名长度不能超过60字符！";
                        continue;
                    }
                    if (fileInfo.Length == 0)
                    {
                        hasNull = true;
                        continue;
                    }
                    //else if (fileInfo.Length > MAXFILELENGTH)
                    //{
                    //    AppData.MainMV.TipMessage = $"不支持超过500M的文件传输";
                    //    continue;
                    //}
                    else if (fileInfo.Length < MAXSCANLENGTH)
                    {
                        string tension = System.IO.Path.GetExtension(path).ToLower();
                        if (App.IsVideo(path))
                        {
                            AddSmallVideoItem(path);
                        }
                        else if (App.IsPicture(path))
                        {
                            AddImageItem(path);
                        }
                        else
                        {
                            if (GetFileCount()<10)
                            {
                                FilePackage file = new FilePackage()
                                {
                                    FileInfo = fileInfo,
                                    Uid = ViewModels.AppData.FlagFile + path
                                };
                                this.AddElement(file);
                            }
                            else
                            {
                                //ViewModels.AppData.MainMV.TipMessage = "最多只能选择10个文件。";
                                MessageBox.ShowDialogBox("最多只能选择10个文件。", "提示", "确定", false);
                            }
                        }
                    }
                    else
                    {
                        if (fileInfo.Length > MAXFILELENGTH)
                        {
                            //AppData.MainMV.TipMessage = $"不支持超过100M的文件传输";
                            MessageBox.ShowDialogBox("发送的文件大小不能大于100M", "提示", "确定", false);
                            continue;
                        }
                        if (GetFileCount() < 10)
                        {
                            string tension = System.IO.Path.GetExtension(path).ToLower();
                            FilePackage file = new FilePackage()
                            {
                                FileInfo = fileInfo,
                                Uid = ViewModels.AppData.FlagFile + path
                            };
                            var thumbnailPath = string.Empty;
                            var duration = string.Empty;
                            if (App.VideoFilter.Contains(tension))
                            {
                                thumbnailPath = System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, Guid.NewGuid() + ".jpg");
                                GetOneFrameImageFromVideo(path, thumbnailPath);
                                duration = GetVideoDuration(path);
                                file.Uid = ViewModels.AppData.FlagFile + path + "|" + thumbnailPath;
                            }

                            this.AddElement(file);
                        }
                        else
                        {
                            MessageBox.ShowDialogBox("最多只能选择10个文件。", "提示", "确定", false);
                        }
                    }
                }
            }

            if (hasNull)
            {
                ViewModels.AppData.MainMV.TipMessage = "文件大小不能为空!";
            }

        }

        /// <summary>  
        /// 从视频中截取一帧  
        /// </summary>
        private string GetOneFrameImageFromVideo(string videoPath, string thumbnailPath)
        {
            FFmpegHelper.GetOneFrameImageFromVideo(videoPath, thumbnailPath, () =>
            {
                //if (System.IO.File.Exists(thumbnailPath))
                //{

                //}
            });
            return thumbnailPath;
        }

        /// <summary>
        /// 获取视频时长，格式（00:10）
        /// </summary>
        /// <returns></returns>
        private string GetVideoDuration(string videoPath)
        {
            return FFmpegHelper.GetVideoDuration(videoPath);
        }
        /// <summary>
        /// 消息编辑框中的文件复制粘贴
        /// </summary>
        /// <param name="filePath"></param>
        private void AddFileItem(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                if (IsFileInUse(filePath))
                {
                    AppData.MainMV.TipMessage = $"[{System.IO.Path.GetFileName(filePath)}]文件已被占用，无法进行传输！ ";
                    return;
                }
                if (GetFileCount() <10)
                {
                    FileInfo fileInfo = new System.IO.FileInfo(filePath);
                    FilePackage file = new FilePackage()
                    {
                        FileInfo = fileInfo,
                        Uid = ViewModels.AppData.FlagFile + filePath
                    };
                    this.AddElement(file);
                }
                else
                {
                    MessageBox.ShowDialogBox("最多只能选择10个文件。", "提示", "确定", false);
                }
            }
        }
        public void AddImageItem(string imgPath)
        {
            if (GetFileCount() < 10)
            {
                if (File.Exists(imgPath))
                {
                    if (IsImage(imgPath))
                    {
                        FileInfo fileInfo = new FileInfo(imgPath);
                        if (fileInfo.Length < MAXSCANLENGTH)
                        {
                            ChatImage img = new ChatImage(imgPath)
                            {
                                Uid = ViewModels.AppData.FlagImage + imgPath
                            };
                            this.AddElement(img);
                            //this.AddUI(img);
                        }
                        else if (fileInfo.Length > MAXFILELENGTH)
                        {
                            MessageBox.ShowDialogBox("发送的文件大小不能大于100M", "提示", "确定", false);
                        }
                        else
                        {
                            AddFileItemsAsync(new string[] { imgPath });
                        }
                    }
                }
            }
            else
            {
                MessageBox.ShowDialogBox("最多只能选择10个文件。", "提示", "确定", false);
            }
        }

        private bool IsImage(string imageName)
        {
            System.Drawing.Image img = null;
            try
            {
                img = System.Drawing.Image.FromFile(imageName);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                img?.Dispose();
            }
        }

        public void AddSmallVideoItem(string videoPath)
        {
            if (!File.Exists(videoPath))
            {
                return;
            }

            FileInfo fileInfo = new FileInfo(videoPath);
            if (fileInfo.Length < MAXSCANLENGTH)
            {
                if (GetFileCount() < 10)
                {
                    SmallVideo smallVideo = new SmallVideo(videoPath, 130, 80);
                    smallVideo.FileState = FileStates.None;
                    this.AddUI(smallVideo);
                }
                else
                {
                    MessageBox.ShowDialogBox("最多只能选择10个文件。", "提示", "确定", false);
                }
            }
            else if (fileInfo.Length > MAXFILELENGTH)
            {
                MessageBox.ShowDialogBox("发送的文件大小不能大于100M", "提示", "确定", false);
            }
            else
            {
                if (GetFileCount() < 10)
                {
                    FilePackage file = new FilePackage()
                    {
                        FileInfo = fileInfo,
                        Uid = ViewModels.AppData.FlagFile + videoPath
                    };
                    this.AddUI(file);
                }
                else
                {
                    MessageBox.ShowDialogBox("最多只能选择10个文件。", "提示", "确定", false);
                }
            }
        }
        /// <summary>
        /// 复制编辑框中的视频
        /// </summary>
        /// <param name="videoPath"></param>
        /// <param name="thumbnailPath"></param>
        public void AddSmallVideoItem(string videoPath, string thumbnailPath)
        {
            if (!File.Exists(videoPath))
            {
                return;
            }

            FileInfo fileInfo = new FileInfo(videoPath);
            if (fileInfo.Length < MAXSCANLENGTH)
            {
                SmallVideo smallVideo = new SmallVideo(videoPath, thumbnailPath);
                smallVideo.FileState = FileStates.None;
                this.AddElement(smallVideo);
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
            try
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
            catch (Exception ex)
            {
            }

        }

        private void SendToUIThread(string text)
        {
            this.richBox.Dispatcher.BeginInvoke(new Action(() => { System.Windows.Forms.SendKeys.Send(text); }),
                System.Windows.Threading.DispatcherPriority.Input);
        }

        /// <summary>
        /// 编辑框内粘贴消息内容处理
        /// </summary>
        public void PasteMsgContent()
        {
            var result = Clipboard.GetData(format);
            if (result != null)
            {
                if (!richBox.Selection.IsEmpty)
                {
                    TextRange textRange = new TextRange(richBox.Selection.Start, richBox.Selection.End);
                    textRange.Text = "";
                }
                var elementList = result as List<MessageContent>;
                foreach (var element in elementList)
                {
                    switch (element.MsgType)
                    {
                        //表情
                        case MsgContentType.Emoji:
                            Win_Face_GetUrl("", element.Uid, element.MsgContent);
                            //Win_Face_GetUrl("", element.MsgContent);
                            break;
                        //文件
                        case MsgContentType.File:
                            AddFileItem(element.MsgContent);
                            //Win_Face_GetUrl("", element.MsgContent, true);
                            break;
                        case MsgContentType.Video:
                            AddSmallVideoItem(element.MsgContent, element.VideoThumbnail);
                            break;
                        //图片
                        case MsgContentType.Image:
                            AddImageItem(element.MsgContent);
                            //ImageOnLoad(element.MsgContent);
                            break;
                        //文字
                        case MsgContentType.Text:
                            InsertText(element.MsgContent);
                            break;
                    }
                }
            }

        }
        /// <summary>
        /// 添加图片元素并定位
        /// </summary>
        public void AddElement(UIElement element)
        {
            if (App.Current.MainWindow.IsActive)
            {
                richBox.Focus();
                TextPointer point = richBox.Selection.Start;
                InlineUIContainer uiContainer = new InlineUIContainer(element, point);
                TextPointer nextPoint = uiContainer.ContentEnd;
                richBox.CaretPosition = nextPoint;
            }
        }
        /// <summary>
        /// 编辑框添加字符串
        /// </summary>
        public void InsertText(string text)
        {
            if (richBox.IsSelectionActive)
            {
                TextPointer textPointer = richBox.Selection.Start;
                Run run = new Run(text, textPointer);
                TextPointer pointer = run.ContentEnd;
                if (!richBox.Selection.IsEmpty)
                {
                    TextRange textRange = new TextRange(pointer, richBox.Selection.End);
                    textRange.Text = "";
                }
                richBox.CaretPosition = pointer;
            }
            else
            {
                TextPointer textPointer = richBox.Selection.Start;
                Run run = new Run(text, textPointer);
                TextPointer pointer = run.ContentEnd;
                richBox.CaretPosition = pointer;
            }
        }
        private void DoPaste()
        {
            if (Clipboard.ContainsData(format))
            {
                PasteMsgContent();
                return;
            }
            this.richBox.Selection.Text = string.Empty;
            if (Clipboard.ContainsFileDropList())
            {
                var files = Clipboard.GetFileDropList();
                foreach (var f in files)
                {
                    var tension = System.IO.Path.GetExtension(f).ToLower();
                    if (App.ImageFilter.Contains(tension))
                    {
                        AddImageItem(f);
                    }
                    else if (App.VideoFilter.Contains(tension))
                    {
                        AddSmallVideoItem(f);
                    }
                    else
                    {
                        AddFileItemsAsync(new string[] { f });
                    }
                }
            }

            else if (Clipboard.ContainsImage())
            {
                BitmapSource bitmap = Clipboard.GetImage();
                string imgPath = Helper.ImageDeal.SaveBitmapImageIntoFile(bitmap);
                //if (bitmap.Width > 1000 * 10 || bitmap.Height > 1000 * 10)
                //    return;
                AddImageItem(imgPath);
                //内存同样图片，已经创建一次，直接赋值给文件形式  
                //Clipboard.SetData(ViewModels.AppData.FlagCopy, new List<string> { ViewModels.AppData.FlagImage + imgPath });
            }
            else
            {
                var formats = Clipboard.GetDataObject().GetFormats();
                if (Clipboard.GetDataObject().GetFormats().Contains(DataFormats.Html))
                {
                    string html = Clipboard.GetData(DataFormats.Html).ToString();
                    IMClient.Helper.MessageHelper.AppendHtmlFromClipboard(this.richBox, html);
                }
                else if (formats.Contains(DataFormats.Rtf))
                {
                    IMClient.Helper.MessageHelper.AppendContentToRichTextBox(this.richBox, Clipboard.GetText());
                    #region Load rtf content
                    //var rtf = Clipboard.GetData(DataFormats.Rtf);
                    ////将rtf载入内存内的RichTextBox 
                    //using (MemoryStream rtfMemoryStream = new MemoryStream())
                    //{
                    //    using (StreamWriter rtfStreamWriter = new StreamWriter(rtfMemoryStream))
                    //    {
                    //        rtfStreamWriter.Write(rtf);
                    //        rtfStreamWriter.Flush();
                    //        rtfMemoryStream.Seek(0, SeekOrigin.Begin);
                    //        //Load the MemoryStream into TextRange ranging from start to end of RichTextBox.
                    //        this.richBox.Selection.Load(rtfMemoryStream, DataFormats.Rtf);
                    //    }
                    //}
                    //var datas = Helper.RichTextBoxHelper.GetSelectionItems(this.richBox);
                    //this.richBox.Selection.Text = string.Empty;
                    //var tp = this.richBox.CaretPosition;
                    //for (int i = 0; i < datas.Count - 1; i++)
                    //{
                    //    if (datas[i] is Run run)
                    //    {
                    //        run = new Run(run.Text, tp);
                    //        tp = run.ContentEnd;
                    //    }

                    //    else if (datas[i] is InlineUIContainer uic)
                    //    {
                    //        if (uic.Child is Image img)
                    //        {
                    //            string path = img.Source.ToString();
                    //            if (File.Exists(path))
                    //            {
                    //            }
                    //            else if (img.Source is BitmapSource bitSource)
                    //            {
                    //                path = Helper.ImageDeal.SaveBitmapImageIntoFile(bitSource);
                    //            }
                    //            else
                    //            {
                    //                continue;
                    //            }

                    //            ChatImage chatImg = new ChatImage(path)
                    //            {
                    //                Uid = ViewModels.AppData.FlagImage + path
                    //            };

                    //            uic = new InlineUIContainer(chatImg, tp);
                    //            tp = uic.ContentEnd;
                    //        }
                    //        else
                    //        {
                    //            uic.Child.ToString();
                    //        }
                    //    }
                    //    else
                    //    {
                    //        //var vv = datas[i].ToString();
                    //        //vv.ToString();
                    //    }
                    //}

                    #endregion
                }
                else if (formats.Contains(IMClient.ViewModels.AppData.FlagCopy))
                {
                    string textCopy = Clipboard.GetData(IMClient.ViewModels.AppData.FlagCopy).ToString();
                    IMClient.Helper.MessageHelper.AppendContentToRichTextBox(this.richBox, textCopy);
                }
                else
                {
                    IMClient.Helper.MessageHelper.AppendContentToRichTextBox(this.richBox, Clipboard.GetText());
                }
            }
        }

        #region  Do Copy

        private void CopyData(bool isCut = false)
        {
            CopyMsgContent();
            if (!Clipboard.ContainsData(format))
            {
                var imageLst = Clipboard.GetFileDropList();
                if (imageLst.Count == 0)
                {
                    Clipboard.Clear();
                    Clipboard.SetDataObject(richBox.Selection.Text);
                }
            }
            if (!isCut) return;
            if (!richBox.Selection.IsEmpty)
            {
                TextRange textRange = new TextRange(richBox.Selection.Start, richBox.Selection.End);
                textRange.Text = "";
            }
            return;
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
                    if (isCut)
                    {

                        try
                        {
                            Hyperlink hlink = new Hyperlink(this.richBox.Selection.Start, this.richBox.Selection.End);

                            if (hlink.Parent is Paragraph ph)
                            {
                                ph.Inlines.Remove(hlink);
                            }
                        }
                        catch
                        {
                            TextRange tr = new TextRange(this.richBox.Selection.Start, this.richBox.Selection.End);
                            tr.Text = string.Empty;
                            //this.richBox.Selection.Text = string.Empty;
                        }
                    }
                    if (datas.Count > 0)
                    {
                        Helper.MessageHelper.SetRichTextBoxSelectionToClipboard(datas);
                    }
                }));

            });
        }

        #endregion

        public int CurrentNumberOfCharacters { get; set; }

        public bool IsNewline { get; set; }
        private void richBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //var add = e.Changes.Sum(info => info.AddedLength);
            //var delete = e.Changes.Sum(info => info.RemovedLength);
            //if (add < delete)
            //{
            //}

            var start = this.richBox.Document.ContentStart;
            var end = this.richBox.Document.ContentEnd;

            CurrentNumberOfCharacters = start.GetOffsetToPosition(end);
            if (start.GetOffsetToPosition(end) > 2000)
            {
                this.richBox.TextChanged -= richBox_TextChanged;

                var pos = start.GetPositionAtOffset(2002);
                var remove = new TextRange(pos, end);

                Hyperlink hlink = new Hyperlink(remove.Start, remove.End);
                if (hlink.Parent is Paragraph ph)
                {
                    ph.Inlines.Remove(hlink);
                }

                this.richBox.TextChanged += richBox_TextChanged;
            }

            var text = new TextRange(start, end).Text;
            if (IsNewline)
            {
                IsNewline = false;
                this.richBox.TextChanged -= richBox_TextChanged;
                this.CloseAtPopup();
                this.richBox.TextChanged += richBox_TextChanged;
                return;
            }
            this.RecognizeAt(text);
        }

        #region @功能相关

        /*....................../´¯/) 
        ......................,/¯../ 
        ...................../..../ 
        .............../´¯/'...'/´¯¯`·¸ 
        ............/'/.../..../......./¨¯\ 
        ..........('(...´...´.... ¯~/'...') 
        ...........\.................'...../ 
        ............''...\.......... _.·´ 
        ..............\..............( 
        ................\.............\*/

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
            AtPackage atPcg = new AtPackage();

            AtUser atUser = new AtUser();
            atUser.ID = ichat.ID;
            if (ichat.ID == -1)
            {
                atUser.Name = ichat.DisplayName;
            }
            else
            {
                GroupMember groupMember = ichat as GroupMember;
                UserModel userModel = groupMember.TargetUser;
                atUser.Name = string.IsNullOrEmpty(groupMember.NickNameInGroup) ? groupMember.TargetUser.Name : groupMember.NickNameInGroup;
            }

            atPcg.DataContext = atUser;
            var uic = this.AddUI(atPcg);
            uic.BaselineAlignment = BaselineAlignment.Bottom;
        }

        string format = "EditMsgList";
        /// <summary>
        /// 编辑框内复制消息内容处理
        /// </summary>
        public List<MessageContent> CopyMsgContent()
        {
            var selection = richBox.Selection;

            List<MessageContent> elementList = new List<MessageContent>();
            var uiElements = from block in richBox.Document.Blocks
                             from inline in (block as Paragraph).Inlines
                             where inline.GetType() == typeof(InlineUIContainer) || inline.GetType() == typeof(Run)
                             select inline;

            var enumerable = uiElements as Inline[] ?? uiElements.ToArray();
            if (!enumerable.Any()) return elementList;
            foreach (var element in enumerable)
            {
                var containsLeft = selection.Contains(element.ContentStart);
                var containsRight = selection.Contains(element.ContentEnd);
                if (containsLeft == containsRight && containsLeft)
                {
                    if (element is InlineUIContainer)
                    {
                        InlineUIContainer line = element as InlineUIContainer;
                        if (line.Child is Image)
                        {
                            var image = line.Child as Image;
                            if (image.Tag != null)
                            {
                                if (image.Tag.ToString() == AppData.FlagEmoje)
                                {
                                    elementList.Add(new MessageContent()
                                    {
                                        MsgType = MsgContentType.Emoji,
                                        MsgContent = image.Source?.ToString() ?? string.Empty,
                                        Uid = image.Uid
                                    });
                                }
                            }
                            else if (!string.IsNullOrEmpty(image.Uid) && image.Uid.Contains(AppData.FlagEmoje))
                            {
                                elementList.Add(new MessageContent()
                                {
                                    MsgType = MsgContentType.Emoji,
                                    MsgContent = image.Source?.ToString() ?? string.Empty,
                                    Uid = image.Uid
                                });
                            }

                            else
                            {
                                elementList.Add(new MessageContent()
                                {
                                    MsgType = MsgContentType.Image,
                                    MsgContent = image.Source?.ToString() ?? string.Empty
                                });
                            }

                        }
                        else if (line.Child is ChatImage img)
                        {
                            elementList.Add(new MessageContent()
                            {
                                MsgType = MsgContentType.Image,
                                MsgContent = img.ImagePath?.ToString() ?? string.Empty
                            });
                        }
                        else if (line.Child is FilePackage file)
                        {
                            elementList.Add(new MessageContent()
                            {
                                MsgType = MsgContentType.File,
                                MsgContent = file.FileInfo?.FullName ?? string.Empty
                            });
                        }
                        else if (line.Child is SmallVideo video)
                        {
                            elementList.Add(new MessageContent()
                            {
                                MsgType = MsgContentType.Video,
                                MsgContent = video.VideoPath ?? string.Empty,
                                VideoThumbnail = video.imgFirstFrame.Source?.ToString().Replace("file:///", "").Replace("/", "//") ?? string.Empty
                            });
                        }
                        else if (line.Child is AtPackage atMsg)
                        {
                            elementList.Add(new MessageContent()
                            {
                                MsgType = MsgContentType.Text,
                                MsgContent = atMsg.txtBox.Text
                            });
                        }
                    }
                    else if (element is InlineUIContainer line)
                    {
                        if (line.Child is Image)
                        {
                            var image = line.Child as Image;
                            elementList.Add(new MessageContent()
                            {
                                MsgType = MsgContentType.Image,
                                MsgContent = image.Source?.ToString() ?? string.Empty
                            });
                        }
                    }
                    else if (element is Run run)
                    {
                        if (elementList.Count == 0 || elementList[elementList.Count - 1].MsgType != MsgContentType.Emoji
                            || elementList[elementList.Count - 1].MsgType != MsgContentType.Image || elementList[elementList.Count - 1].MsgType != MsgContentType.File)
                            elementList.Add(new MessageContent() { MsgType = MsgContentType.Text, MsgContent = run.Text });
                        else
                            elementList[elementList.Count - 1].MsgContent += run.Text;
                    }
                }
                else if (element is Run)
                {
                    if (containsRight)
                    {
                        var partialText = selection.Start.GetTextInRun(LogicalDirection.Forward);
                        if (!string.IsNullOrEmpty(partialText))
                        {
                            Run run = new Run(partialText);
                            if (elementList.Count == 0 ||
                                elementList[elementList.Count - 1].MsgType != MsgContentType.Emoji ||
                                elementList[elementList.Count - 1].MsgType != MsgContentType.Image
                                || elementList[elementList.Count - 1].MsgType != MsgContentType.File)
                                elementList.Add(new MessageContent()
                                {
                                    MsgType = MsgContentType.Text,
                                    MsgContent = run.Text
                                });
                            else
                                elementList[elementList.Count - 1].MsgContent += run.Text;
                        }
                    }
                    else if (containsLeft)
                    {
                        var partialText = selection.End.GetTextInRun(LogicalDirection.Backward);
                        if (!string.IsNullOrEmpty(partialText))
                        {
                            Run run = new Run(partialText);
                            if (elementList.Count == 0 ||
                                elementList[elementList.Count - 1].MsgType != MsgContentType.Emoji ||
                                elementList[elementList.Count - 1].MsgType != MsgContentType.Image ||
                                 elementList[elementList.Count - 1].MsgType != MsgContentType.File)
                                elementList.Add(new MessageContent()
                                {
                                    MsgType = MsgContentType.Text,
                                    MsgContent = run.Text
                                });
                            else
                                elementList[elementList.Count - 1].MsgContent += run.Text;
                        }
                    }
                }

            }
            var isHasImg = elementList.Exists(m => m.MsgType == MsgContentType.Emoji || m.MsgType == MsgContentType.Image);
            var isHasEmoji = elementList.Exists(m => m.MsgType == MsgContentType.Emoji);
            var isHasFile = elementList.Exists(m => m.MsgType == MsgContentType.File || m.MsgType == MsgContentType.Video);
            var isHasText = elementList.Exists(m => m.MsgType == MsgContentType.Text && !string.IsNullOrEmpty(m.MsgContent));

            //如果只有复制图片或文字，就不重新定义剪切板格式（为了给外部软件使用，比如：QQ）
            if ((isHasText && isHasImg) || isHasEmoji || isHasFile)
            {
                Clipboard.Clear();
                Clipboard.SetData(format, elementList);
            }
            else if (isHasImg)
            {
                string[] file = new string[elementList.Count];
                for (int i = 0; i < elementList.Count; i++)
                {
                    if (elementList[i].MsgContent.Contains("file:///"))
                    {
                        file[i] = elementList[i].MsgContent.Replace("file:///", "").Replace("/", "//");
                    }
                    else if (elementList[i].MsgContent.Contains("pack://application:,,,/IM.Emoje;component"))
                    {
                        file[i] = elementList[i].MsgContent.Replace("pack://application:,,,/IM.Emoje;component", AppDomain.CurrentDomain.BaseDirectory).Replace("/", "//");
                    }
                    else
                    {
                        file[i] = elementList[i].MsgContent;
                    }


                }
                DataObject dataObject = new DataObject();
                dataObject.SetData(DataFormats.FileDrop, file);
                Clipboard.SetDataObject(dataObject, true);
            }
            else if (isHasText)
            {
                StringBuilder builder = new StringBuilder();
                elementList.ToList().ForEach(x => builder.Append(x.MsgContent));
                Clipboard.SetDataObject(builder.ToString(), true);
                return elementList;
            }
            return elementList;
        }

        private void GetAtList()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                ChatViewModel chatViewModel = this.GotDataContext?.Invoke();

                if (chatViewModel == null || !chatViewModel.IsGroup) return;
                if (!AppData.MainMV.GroupListVM.Items.ToList().Any(info => info.ID == chatViewModel.ID))
                {
                    this._isRecognizeAt = false;
                    return;
                }

                this._isRecognizeAt = true;
                this._groupModel = chatViewModel.Chat.Chat as GroupModel;
                if (this._groupModel.Members == null || this._groupModel.Members.Count == 0) return;

                GroupMember[] members = new GroupMember[this._groupModel.Members.Count];
                this._groupModel.Members.CopyTo(members, 0);

                this._groupMember = null;
                this._groupMember = new ObservableCollection<GroupMember>(members.Where(x => x.ID != AppData.Current.LoginUser.ID));

                UserModel user = AppData.Current.GetUserModel(AppData.Current.LoginUser.ID);
                var sender = user.GetInGroupMember(this._groupModel);
                if (this._groupModel.OwnerID == AppData.Current.LoginUser.ID ||
                    sender.IsManager)
                {
                    GroupMember member = GroupMember.CreateEmpty(ConstString.AtAllId);
                    member.DisplayName = Everyone;
                    this._groupMember.Insert(0, member);
                }
            });
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

        void RecognizeAt(string text)
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
                string tpForwardText = tpCaret.GetTextInRun(LogicalDirection.Forward);

                #region 过滤重复的@
                //如果为英文输入状态时，输入一个@时，会因为用了“Popup遮挡输入法的解决方案”而多次触发TextChanged事件;
                //导致输入一个@界面显示两个@，所以加一个bool字段去判断，删掉一个重复的@。
                if (this.ppMember.IsUpdateWindow)
                {
                    this.ppMember.IsUpdateWindow = false;
                    tpCaret.DeleteTextInRun(-1);
                    return;
                }

                #endregion

                if (!tpBackwardText.Contains("@"))
                {
                    this.CloseAtPopup();
                    return;
                }
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
            {
                this.CloseAtPopup();
                this._isPaste = false;
            }
        }

        void CalculatePopupLocation(TextPointer tpCaret)
        {
            Rect rect;
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
                else
                {
                    rect = tpCaret.GetCharacterRect(LogicalDirection.Backward);
                }
            }
            else
            {
                rect = tpCaret.GetCharacterRect(LogicalDirection.Backward);
            }
            Point p = new Point(rect.X, rect.Y);
            this.ppMember.HorizontalOffset = p.X + 10;
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

            for (int i = 0; i < 10; i++)
            {
                string at = tpEnd.GetTextInRun(LogicalDirection.Backward);
                if (at.Contains("@"))
                {
                    tpEnd.DeleteTextInRun(-_counter);
                    break;
                }
                else
                {
                    tpEnd = tpEnd.GetNextContextPosition(LogicalDirection.Backward);
                }
            }

            //TextRange tr = new TextRange(this.richBox.Document.ContentStart, tpEnd);

            //if (tr.Text.EndsWith("@"))
            //{

            //}
            //string tpBackwardText = tpEnd.GetTextInRun(LogicalDirection.Backward);
            //string tpForwardText = tpEnd.GetTextInRun(LogicalDirection.Forward);
            //if (string.IsNullOrEmpty(tpBackwardText))
            //{

            //}
            //else
            //{

            //}
            //tpEnd.DeleteTextInRun(-_counter);
            //tpBackwardText = tpEnd.GetTextInRun(LogicalDirection.Backward);
            //tpForwardText = tpEnd.GetTextInRun(LogicalDirection.Forward);

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

        internal int GetFileCount()
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
                {
                    count = ((Paragraph)block).Inlines.Where(m => m is InlineUIContainer ui && (ui.Child is ChatImage||ui.Child is FilePackage))?.Count() ?? 0;
                    //foreach (var inline in ((Paragraph)block).Inlines)
                    //{
                    //    if (inline is InlineUIContainer ui && ui.Child is ChatImage)
                    //    {
                    //        imageCount++;
                    //    }
                    //}
                }
            }

            return count;
        }

        //internal int GetFileCount()
        //{
        //    int count = 0;
        //    var isFirst = true;
        //    foreach (var block in this.richBox.Document.Blocks)
        //    {
        //        if (isFirst)
        //            isFirst = false;
        //        else
        //        {

        //        }

        //        if (block is Paragraph)
        //        {
        //            count = ((Paragraph)block).Inlines.Where(m => m is InlineUIContainer ui && ui.Child is FilePackage)?.Count() ?? 0;
        //            //foreach (var inline in ((Paragraph)block).Inlines)
        //            //{
        //            //    if (inline is InlineUIContainer ui && ui.Child is ChatImage)
        //            //    {
        //            //        imageCount++;
        //            //    }
        //            //}
        //        }
        //    }
        //    return count;
        //}

        private static bool IsFileInUse(string fileName)
        {
            bool inUse = true;

            FileStream fs = null;
            try
            {

                string exten = System.IO.Path.GetExtension(fileName);
                if (App.ImageFilter.Contains(exten))
                {
                    inUse = false;
                }
                else
                {
                    fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);

                    inUse = false;
                }

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

        /// <summary>
        /// 发送名片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSharePersonCard_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            ChatViewModel chatVM = btn.DataContext as ChatViewModel;
            AppData.SendPersonCard(chatVM.ID, null, false, chatVM.IsGroup);
        }
    }
    [Serializable]
    public class MessageContent
    {
        public MsgContentType MsgType { get; set; }
        public string MsgContent { get; set; }
        /// <summary>
        /// 视频缩略图
        /// </summary>
        public string VideoThumbnail { get; set; }
        /// <summary>
        /// 表情控件UID
        /// </summary>
        public string Uid { get; set; }
    }

    /// <summary>
    /// 消息内容类型
    /// </summary>
    public enum MsgContentType
    {
        Text,
        Image,
        Emoji,
        File,
        Video
    }
    public class AtUser
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
