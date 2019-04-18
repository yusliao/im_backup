using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents; 
using System.Xml;
using CSClient.ViewModels;
using CSClient.Views.Controls;
using IMModels;
using Obisoft.HSharp;
using Util;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace CSClient.Helper
{
    public static class MessageHelper
    {
        public static readonly Brush MainBrush = new SolidColorBrush() { Color = (Color)ColorConverter.ConvertFromString("#FF688fff") };

        private static string[] _emojes;

        public static string[] Emojes
        {
            get
            {
                if (_emojes == null)
                {
                    _emojes = IM.Emoje.EmojeBox.EmojeKeys.Select(info => info.Name).ToArray();
                }
                return _emojes;
            }
        }

        public static IList<Inline> ContentInlines = null;
        public static IList<Inline> GetRichContent(string content, bool isMine = true)
        {
            if (content == null)
            {
                return null;
            }
            
            string replace;
            foreach (var em in IM.Emoje.EmojeBox.EmojeKeys)
            {
                replace = string.Format("{0}{1}{0}", ViewModels.AppData.FlagEmoje, em.Name);
                content = content.Replace(em.Name, replace);
            }

            string[] views = content.Split(new string[] { ViewModels.AppData.FlagEmoje }, StringSplitOptions.RemoveEmptyEntries);
            if (ContentInlines == null)
            {
                ContentInlines = new List<Inline>();
            }

            if (views.Length > 0)
            {                
                for (int i = 0; i < views.Length; i++)
                {
                    if (Emojes.Contains(views[i]))
                    {
                        var emoj = IM.Emoje.EmojeBox.EmojeKeys.FirstOrDefault(info => info.Name == views[i]);

                        InlineUIContainer uic = new InlineUIContainer(new Image()
                        {
                            Margin = new Thickness(1),
                            Source = emoj.ImageSource,
                            Width = 28,
                            Height = 28,
                            Uid =emoj.Name,
                            //Margin = new Thickness(0, 0, 0, -5)
                        });

                        uic.BaselineAlignment = BaselineAlignment.Bottom;
                        ContentInlines.Add(uic);
                    }
                    else
                    {
                        MatchCollection mc = IsContainsLink(views[i]);
                        if (mc.Count > 0)
                        {
                            int length = 0;
                            foreach (var item in mc)
                            {
                                int index = views[i].IndexOf(item.ToString());
                                if (index != 0)
                                {
                                    string temp = views[i].Substring(0, index);
                                    ContentInlines.Add(new Run(temp));
                                }
                                length = index + item.ToString().Length;
                                //Hyperlink hyperlink = new Hyperlink();
                                //hyperlink.Tag = item;
                                //hyperlink.ForceCursor = true;
                                //hyperlink.Cursor = System.Windows.Input.Cursors.Hand;
                                //hyperlink.BaselineAlignment = BaselineAlignment.TextBottom;
                                //if (isMine)
                                //{
                                //    hyperlink.Foreground = new SolidColorBrush(Colors.White);
                                //}
                                //hyperlink.Inlines.Add(item.ToString());
                                //hyperlink.Click += Hyperlink_Click;
                                //ContentInlines.Add(hyperlink);


                                Run hyperlink = new Run();
                                hyperlink.Tag = item;
                                hyperlink.ForceCursor = true;
                                hyperlink.Cursor = System.Windows.Input.Cursors.Hand;
                                hyperlink.BaselineAlignment = BaselineAlignment.TextBottom;
                                hyperlink.TextDecorations = TextDecorations.Underline;
                                if (isMine)
                                {
                                    hyperlink.Foreground = new SolidColorBrush() { Color = (Color)ColorConverter.ConvertFromString("#666666") };
                                }
                                else
                                {
                                    hyperlink.Foreground = MainBrush;
                                }

                                //hyperlink.Inlines.Add(item.ToString());
                                hyperlink.Text = item.ToString();
                                //hyperlink.Click += Hyperlink_Click;
                                hyperlink.PreviewMouseLeftButtonDown += Hyperlink_MouseDown;
                                ContentInlines.Add(hyperlink);


                                break;
                            }
                            views[i] = views[i].Remove(0, length);
                            if (views[i].Length > 0)
                            {
                                GetRichContent(views[i], isMine);
                            }
                        }
                        else
                        {
                            ContentInlines.Add(new Run(views[i]) { BaselineAlignment = BaselineAlignment.TextBottom });
                        }
                    }
                }
            }
            else
            { 
                //inlines = new Inline[] { new Run(content) };
            }

            if(ContentInlines.FirstOrDefault() is InlineUIContainer)
            {
                ContentInlines.Insert(0,new Run() { BaselineAlignment = BaselineAlignment.TextBottom });
            }
            if (ContentInlines.LastOrDefault() is InlineUIContainer)
            {
                ContentInlines.Add(new Run() { BaselineAlignment = BaselineAlignment.TextBottom });
            }
            return ContentInlines;

        }

        #region 超链接

        static Point DOWNpos;
        static int DOWNtime;
        static Run HYlink;
        private static void Hyperlink_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            HYlink = sender as Run;
            DOWNtime = e.Timestamp;


            var win = Window.GetWindow(HYlink);
            DOWNpos = e.GetPosition(win);
            win.PreviewMouseLeftButtonUp += ParentWin_PreviewMouseLeftButtonUp;
        }

        private static void ParentWin_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var win = sender as Window;
            win.PreviewMouseLeftButtonUp -= ParentWin_PreviewMouseLeftButtonUp;

            Point p = e.GetPosition(win);
            int t = e.Timestamp - DOWNtime;
            if (HYlink != null && t < 500 && (p - DOWNpos).Length < 3)
            {
                System.Diagnostics.Process.Start(HYlink.Text);
            }
            HYlink = null;
        }

        #endregion

        static MatchCollection IsContainsLink(string source)
        {
            //string pattern = @"((http|https)://)?(www.)?[a-z0-9\.]+(\.(com|net|cn|com\.cn|com\.net|net\.cn))(/[^\s\n]*)?";
            //string pattern = @"(http|https|ftp)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&$%\$#\=~])*$";
            string pattern = @"((http|ftp|file|https)://)?(www.)?(([a-zA-Z0-9\._-]+\.[a-zA-Z]{2,6})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(:[0-9]{1,4})*(/[a-zA-Z0-9\&%_\./-~-]*)?";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection m = r.Matches(source);
            return m;
        }

        public static string[] GetTextContent(string content)
        {
            if (content == null)
            {
                return null;
            }
            string replace;
            foreach (var em in IM.Emoje.EmojeBox.EmojeKeys)
            {
                replace = string.Format("{0}{1}{0}", ViewModels.AppData.FlagEmoje, em.Name);
                content = content.Replace(em.Name, replace);
            }

            string[] views = content.Split(new string[] { ViewModels.AppData.FlagEmoje }, StringSplitOptions.RemoveEmptyEntries);

            return views;
        }

        //public static void AppendContentToRichTextBox(RichTextBox tb, string content)
        //{
        //    if (tb == null || string.IsNullOrEmpty(content))
        //    {
        //        return;
        //    }

        //    string replace;
        //    foreach (var em in IM.Emoje.ContantClass.EmojiCode)
        //    {
        //        replace = string.Format("{0}{1}{0}", ViewModels.AppData.FlagEmoje, em.Key);
        //        content = content.Replace(em.Key, replace);
        //    }

        //    string[] views = content.Split(new string[] { ViewModels.AppData.FlagEmoje }, StringSplitOptions.RemoveEmptyEntries);

        //    if (views.Length > 0)
        //    { 
        //        for (int i = 0; i < views.Length; i++)
        //        {
        //            if (Emojes.Contains(views[i]))
        //            {
        //                var emoj = IM.Emoje.ContantClass.EmojiCode.FirstOrDefault(info => info.Key == views[i]);

        //                InlineUIContainer uic = new InlineUIContainer(new Image()
        //                {
        //                    Source = emoj.ImageSorce,
        //                    Width = 30,
        //                    Height = 30,
        //                    Uid = ViewModels.AppData.FlagEmoje +emoj.Key,
        //                },tb.CaretPosition); 
        //            }
        //            else
        //            {
        //                new Run(views[i], tb.CaretPosition);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        new Run(content, tb.CaretPosition);
        //    } 
        //}



        public static void AppendContentToRichTextBox(RichTextBox tb, string content)
        {
            if (tb == null || string.IsNullOrEmpty(content))
            {
                return;
            }

            string[] views = content.Split(new string[] { ViewModels.AppData.FlagImage }, StringSplitOptions.RemoveEmptyEntries);

            TextPointer tp = tb.CaretPosition;
            foreach (var view in views)
            {
                if (System.IO.File.Exists(view))
                {
                    InlineUIContainer uic = new InlineUIContainer(new ChatImage(view, new Size(320, 180))
                    {
                        Uid = ViewModels.AppData.FlagImage + view
                    }, tp);
                    tp = uic.ElementEnd;
                }
                else
                {
                    LoadPart(tb, view, ref tp);
                }
            }
        }

        private static void LoadPart(RichTextBox tb, string content, ref TextPointer tp)
        {
            if (content.Length > 0 && content.Contains('/'))
            {
                StringBuilder sb = new StringBuilder();
                bool hasValue = false;
                string value = string.Empty;
                for (int i = 0; i < content.Length; i++)
                {
                    if (content[i] == '/' && content.Length > i + 1)
                    {
                        string key = string.Format("{0}{1}", content[i], content[i + 1]);
                        var emoje = IM.Emoje.EmojeBox.EmojeKeys.FirstOrDefault(info => info.Name == key);
                        if (emoje == null)
                        {
                            if (content.Length > i + 2)
                            {
                                key = string.Format("{0}{1}{2}", content[i], content[i + 1], content[i + 2]);
                                emoje = IM.Emoje.EmojeBox.EmojeKeys.FirstOrDefault(info => info.Name == key);
                                if (emoje != null)
                                {
                                    i += 2;
                                }
                                else
                                {
                                    if (content.Length > i + 3)
                                    {
                                        key = string.Format("{0}{1}{2}{3}", content[i], content[i + 1], content[i + 2], content[i + 3]);
                                        emoje = IM.Emoje.EmojeBox.EmojeKeys.FirstOrDefault(info => info.Name == key);
                                        if (emoje != null)
                                        {
                                            i += 3;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            i += 1;
                        }

                        if (emoje != null)
                        {
                            if (hasValue)
                            {
                                value = sb.ToString();

                                //if(value.StartsWith(ViewModels.AppData.FlagImage))

                                Run run = new Run(value, tp);
                                sb.Clear();
                                hasValue = false;
                                tp = run.ElementEnd;
                            }

                            InlineUIContainer uic = new InlineUIContainer(new Image()
                            {
                                Margin = new Thickness(1),
                                Source = emoje.ImageSource,
                                Width = 28,
                                Height = 28,
                                Uid = ViewModels.AppData.FlagEmoje + emoje.Name,
                            }, tp);

                            tp = uic.ElementEnd;
                        }
                        else
                        {
                            sb.Append(content[i]);
                            hasValue = true;
                        }
                    }
                    else
                    {
                        sb.Append(content[i]);
                        hasValue = true;
                    }
                }
                if (hasValue)
                {
                    Run run = new Run(sb.ToString(), tp);
                    sb.Clear();
                    hasValue = false;
                    tp = run.ElementEnd;
                }
            }
            else
            {
                Run run = new Run(content, tp);
                tp = tb.CaretPosition = run.ElementEnd;
            }
        }

        //Version: 0.9
        //StartHTML: 00000112
        //EndHTML: 00000455
        //StartFragment: 00000126
        //EndFragment: 00000419
        // < !doctype html >
        //< html >
        //   < body >
        //       < !--StartFragment-- >
        //       < DIV >
        //            42354525的发送到发送到发 
        //            < IMG src = "file:///C:\Users\liyi\AppData\Roaming\Tencent\Users\50376968\TIM\WinTemp\RichOle\(`5GHE[LX4$`090{9)$T]7W.png" >
        //            发都发所发生的 
        //            < IMG src = "file:///C:\Users\liyi\Desktop\IM\nonono.gif" > 
        //            是的发生大发手动阀
        //        </ DIV >< !--EndFragment-- >
        //    </ body >
        //</ html >
        public static void AppendHtmlFromClipboard(this RichTextBox tb, string content)
        {
            try
            {
                content = content.Replace("&nbsp;", ""); 
                var doc = HtmlConvert.DeserializeHtml(content);
               
                var div = doc.AllUnder.FirstOrDefault(info => info.TagName.ToUpper() == "DIV");

                if (div == null)
                {
                    CSClient.Helper.MessageHelper.AppendContentToRichTextBox(tb, Clipboard.GetText());
                    return;
                }
               
                var  values = div.Children;
                  
                TextPointer tp = tb.CaretPosition;

                for (int i = 0; i < values.Count; i++)
                {
                    switch (values[i].TagName)
                    {
                        case "img":
                        case "IMG":
                            string src = string.Empty;
                            if (values[i].Properties.ContainsKey("SRC"))
                            {
                                src = values[i].Properties["SRC"].ToUpper().Replace("FILE:///", ""); 
                            }
                            else if (values[i].Properties.ContainsKey("src"))
                            {
                                src = values[i].Properties["src"].ToUpper().Replace("FILE:///", "");
                            } 
                            if (System.IO.File.Exists(src))
                            {
                                InlineUIContainer uic = new InlineUIContainer(new ChatImage(src, new Size(320, 180))
                                {
                                    Uid = ViewModels.AppData.FlagImage + src
                                }, tp);
                                tp = uic.ElementEnd;
                            }
                            break;
                       
                        default:
                            Run run = new Run(values[i].ToString(), tp);
                            tp = run.ElementEnd;
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                CSClient.Helper.MessageHelper.AppendContentToRichTextBox(tb, Clipboard.GetText());
            }  
        }


        public static void SetRichTextBoxSelectionToClipboard(List<Inline> datas)
        {
            if(datas.Count ==1 && datas[0] is Run value)
            {
                System.Windows.Forms.Clipboard.SetDataObject(value.Text);
                return;
            }
            StringBuilder sb = new StringBuilder();
            StringBuilder strSB = new StringBuilder();
            
            for (int i = 0; i < datas.Count; i++)
            {
                if (datas[i] is Run run)
                {
                    sb.Append(run.Text);
                    strSB.Append(run.Text);
                }else if (datas[i] is LineBreak)
                {
                    sb.Append("\r\n");
                    strSB.Append("\r\n");
                }
                else if (datas[i] is InlineUIContainer uic)
                {
                    string uid = uic.Child.Uid;
                    if (uid.StartsWith(AppData.FlagImage))
                    {
                        strSB.Append(uic.Child.Uid + AppData.FlagImage);
                        string src = uic.Child.Uid.Replace(AppData.FlagImage, string.Empty);
                        sb.Append($"<img src=\"file:///{src}\">\r");
                    }
                    else if (uid.StartsWith(AppData.FlagAt))
                    {
                       
                        string atValue = uic.Child.Uid.Split('|')[1];
                        strSB.Append(atValue);
                        sb.Append(atValue);
                    }
                    //else if (uic.Child is ChatImage cImg)
                    //{
                    //    sb.Append($"<IMG SRC ={uic.Child.Uid}>\r");
                    //}
                }
            }

            string strValue = sb.ToString();

            sb.Insert(0,"<DIV>\r");
            sb.Append("</DIV>\r"); 

            string result = sb.ToString(); 



            //string value = $"<DIV><IMG src=\"file:///{this.ImagePath }\"></DIV> ";
            //Helper.ClipboardHelper.CopyToClipboard(result, strValue);
            var dataObject =  Helper.ClipboardHelper.CreateDataObject(result, strValue);
            dataObject.SetData(AppData.FlagCopy, strSB.ToString()); 
            System.Windows.Forms.Clipboard.SetDataObject(dataObject);
        }

         

        public static void LoadImgContent(MessageModel msg)
        {
            try
            {
                string imagePath = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, msg.ResourceModel.Key);

                if (!File.Exists(imagePath))
                {
                    msg.Content = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, msg.ResourceModel.SmallKey);

                    msg.OperateTask = new System.Threading.CancellationTokenSource();
                    string temp = $"{Util.Helpers.Id.Guid()}{Path.GetExtension(msg.ResourceModel.Key)}";
                    SDKClient.SDKClient.Instance.DownLoadResource
                        (msg.ResourceModel.Key, temp, SDKClient.Model.FileType.img, null, (b) =>
                    {
                        msg.Content = null;
                        if (b)
                        {
                            msg.Content = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, temp);
                            msg.MessageState = MessageStates.Success;
                        }
                        else
                        {
                            msg.MessageState = MessageStates.Fail;
                        }
                    }, msg.MsgKey ,msg.OperateTask.Token);

                }
                else
                {
                    msg.Content = imagePath;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        public static void LoadFileContent(MessageModel msg, System.Threading.CancellationTokenSource operate, ChatViewModel chatVM,Action<bool> callback, string savePath = null)
        {

            Task.Run(() =>
            {
                try
                {
                    if (msg.IsSending)
                    {
                        return;
                    }
                    if (savePath == null)
                    {
                        msg.Content = msg.ResourceModel.FullName;// FileHelper.GetFileName(Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.filePath,msg.ResourceModel.FileName));// ;
                    }
                    else
                    {
                        msg.Content = savePath;
                    }
                    if (!File.Exists(msg.Content)|| savePath!=null)
                    {
                        //msg.Content = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, msg.P_resourceId);
                        if (msg.ResourceModel.RefInfo is SDKClient.Model.OnlineFileBody body) //在线
                        {
                            bool isSuccess = SDKClient.SDKClient.Instance.RecvOnlineFile(msg.MsgKey, body.IP, body.Port, msg.Sender.ID, body.fileSize, msg.Content, body.id,
                               (allSize) =>
                               {
                                   msg.ResourceModel.CompleteLength = allSize;
                               },
                               (result) =>
                               {
                                   if (result.isSuccess == 1)
                                   {
                                       SetFileReult(msg);
                                       msg.ResourceModel.FileState = FileStates.Completed;
                                       msg.ResourceModel.FileName = Path.GetFileName(msg.Content); 
                                   }
                                   else
                                   {
                                       if (msg.MessageState == MessageStates.Fail)
                                       {
                                           return;
                                       }
                                       OnlineFail(chatVM, msg,result.notifyPackage.Content);
                                       callback?.Invoke(false);
                                   }
                               },
                               (processSize) =>
                               {
                                   msg.ResourceModel.CompleteLength = processSize;
                               }, operate.Token);

                            if (!isSuccess)//连接不成功
                            {
                                long filesize = body.fileSize;
                                string content = $"对方取消了\"{Path.GetFileName(msg.Content)}\"({filesize.GetFileSizeString()})的接收，文件传输失败。";
                                OnlineFail(chatVM, msg, content);
                                callback?.Invoke(false);
                            }
                        }
                        else
                        {
                            SDKClient.SDKClient.Instance.DownLoadResource(msg.ResourceModel.Key, msg.Content, SDKClient.Model.FileType.file,
                            (processSize) =>
                            {
                                msg.ResourceModel.CompleteLength = processSize;
                            },
                            (result) =>
                            {
                                if (result || msg.Sender == ViewModels.AppData.Current.LoginUser.User)
                                {
                                    SetFileReult(msg);
                                    msg.ResourceModel.FileState = FileStates.Completed;
                                    msg.ResourceModel.FileName = Path.GetFileName(msg.Content);
                                    //msg.Content = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, msg.P_resourceId);
                                }
                                else
                                {
                                    callback?.Invoke(false);
                                    if (msg.MessageState == MessageStates.Fail)
                                    {
                                        return;
                                    }
                                    msg.ResourceModel.CompleteLength = 0;
                                    msg.ResourceModel.FileState = FileStates.WaitForReceieve;
                                    msg.MessageState = MessageStates.Fail; 
                                   
                                   
                                    AppData.MainMV.TipMessage = "当前无网络，请检查网络设置！";
                                }
                            }, msg.MsgKey, cancellationToken: operate.Token); //operate.Token
                        }
                    }
                    else if (!msg.IsSending)
                    {
                        SetFileReult(msg);
                    }
                    //msg.Content = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, msg.P_resourcesmallId);
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            });
        }

        private static void OnlineFail(ChatViewModel chatVM, MessageModel msg,string content)
        {
            msg.ResourceModel.FileState = FileStates.Fail;
            msg.MessageState = MessageStates.Fail;

            msg.MsgType = MessageType.notification;
            msg.SendTime = DateTime.Now;


            string size = Helper.FileHelper.FileSizeToString(msg.ResourceModel.Length);
           
            //string tip = $"对方取消了\"{msg.ResourceModel.FileName}\"({size})的接收，文件传输失败。";
            msg.Content = content;

            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                chatVM.UpdateMsg(msg);
                App.Current.MainWindow.Activate();
            }));
        }

        private static void SetFileReult(MessageModel msg)
        {
            if (File.Exists(msg.Content))
            {
                msg.ResourceModel.CompleteLength = msg.ResourceModel.Length;
                msg.ResourceModel.FileImg = Helper.FileHelper.GetFileImage(msg.Content,false);
            }
        }
    }

    public class RichTextBoxHelper
    { 
        public static List<Inline> GetSelectionItems( RichTextBox richBox)
        {
            List<Inline> datas = new List<Inline>();
            var selection = richBox.Selection;
            if (!selection.IsEmpty)
            {
                var items = GetInlinesFromDocument(richBox.Document).ToList();
                 
                foreach (var inline in items)
                {
                    var containsLeft = selection.Contains(inline.ContentStart);
                    var containsRight = selection.Contains(inline.ContentEnd);
                     
                    if (containsLeft == containsRight)
                    {
                        if (containsLeft)
                        {
                            ProcessSelectedInline(inline, null, datas);
                        }
                        else
                        {
                            if (inline is Run
                                && inline.ContentEnd.CompareTo(selection.End) > 0
                                && inline.ContentStart.CompareTo(selection.Start) < 0)
                            {
                                ProcessSelectedInline(inline, selection.Text, datas);
                            }
                        }
                    }
                    else if (inline is Run run)
                    {
                        if (containsRight)
                        {
                            var partialText = selection.Start.GetTextInRun(LogicalDirection.Forward);
                            if (run.Text.Contains(partialText))
                            { 
                                ProcessSelectedInline(inline, partialText, datas);
                            }
                        }
                        else
                        {
                            var partialText = selection.End.GetTextInRun(LogicalDirection.Backward);
                            if (run.Text.Contains(partialText))
                            {
                                ProcessSelectedInline(inline, partialText, datas);
                            }
                        }
                    }
                }
                //string value = $"<DIV><IMG src=\"file:///{this.ImagePath }\"></DIV> ";
                //Helper.ClipboardHelper.CopyToClipboard(value, "");
            }

            //Run perRun = null;
            //List<Inline> items = new List<Inline>();
            //for(int i = 0; i < datas.Count; i++)
            //{
            //    if(datas[i] is Run run)
            //    {
            //        if (perRun == null)
            //        { 
            //            items.Add(perRun= run);
            //        }
            //        else
            //        {
            //            perRun.Text += run.Text;
            //        }
            //    }
            //    else
            //    {
            //        perRun = null;
            //    }
            //}

            if (datas.Count > 0)
            { 
                List<Inline> results = new List<Inline>() {datas[0] };

                Run per = null;
                if (datas[0] is Run run)
                {
                    per = run; 
                }
                else if (datas[0] is LineBreak)
                { 
                    results[0]= per = new Run("\r\n");
                }

                for (int i = 1; i < datas.Count; i++)
                {
                    if (datas[i] is Span span && i!=datas.Count-1)
                    { 
                        if (span.NextInline == null)
                        {
                            datas[i] = new Run("\r\n");
                        }
                        else
                        {
                            continue;
                        }
                        //var vv = datas[i];
                        //vv.ToString();
                    }
           
                    if(datas[i] is Run nRun)
                    {
                        if (per == null)
                        {
                            results.Add(nRun);
                        }
                        else
                        {
                            per.Text += nRun.Text;
                        }
                    }
                    else if(datas[i] is LineBreak lb)
                    {
                        if (per == null)
                        {
                            results.Add(per= new Run("\r\n"));
                        }
                        else
                        {
                            per.Text += "\r\n";
                        }
                    }
                    else
                    {
                        results.Add(datas[i]);
                        per = null;
                    }
                }

                return results;
            }

            return datas;
        }

        static void ProcessSelectedInline(Inline inline, object partial, List<Inline> datas)
        {
            if (inline is Run run)
            {
                if (String.IsNullOrEmpty(run.Text) || run.Text == " ")
                {
                    return;
                }

                if (partial != null)
                {
                    if (!string.IsNullOrEmpty(partial.ToString()))
                    {
                        datas.Add(new Run(partial.ToString()));
                    }
                }
                else
                {
                    if (!datas.Contains(run))
                    {
                        datas.Add(new Run(run.Text));
                    }
                }
            }
            else if (inline is InlineUIContainer container)
            {
                if (container.Child.Uid.Contains(ViewModels.AppData.FlagEmoje))
                {
                    string key = container.Child.Uid.Replace(ViewModels.AppData.FlagEmoje, string.Empty);
                    datas.Add(new Run(key) { Tag = ViewModels.AppData.FlagEmoje });
                }
                //else if(container.Child is Image img) //可能来源于 word等复制的内容图片
                //{
                //    string key = $"{ViewModels.AppData.FlagEmoje}{img.Source}{ViewModels.AppData.FlagEmoje}";
                //    datas.Add(key);
                //}
                else if(container.Child is FileChatItem fChat)
                { 
                    datas.Add(new Run($"[文件: { fChat.tbName.Text}]"));
                }
                else
                {
                    datas.Add(container);
                }
            }
            else
            { 
                datas.Add(inline);
            } 
        }

       static IEnumerable<Inline> GetInlinesFromDocument(FlowDocument doc)
        {
            
            foreach (var block in doc.Blocks)
            {
                foreach (var inline in GetInlinesFromBlock(block)) 
                    yield return inline; 
            }
        }

        static IEnumerable<Inline> GetInlinesFromBlock(Block block)
        {
            if (block is Paragraph pa)
            {
                foreach (var inline in pa.Inlines)
                {
                    foreach (var subInline in GetInlinesFromInline(inline))
                    {
                        //if (subInline is Run run)
                        //{
                        //    yield return subInline;
                        //    //var v = run.Text;
                        //}
                        yield return subInline;
                    }
                }
            }
               
            else if (block is Section sec)
            { 
                foreach (var b in sec.Blocks)
                    foreach (var inline in GetInlinesFromBlock(b))
                        yield return inline;
            }
        }

        static IEnumerable<Inline> GetInlinesFromInline(Inline inline)
        {
            if (inline is Span span)  
            {
                foreach (var subInline in span.Inlines) 
                {
                    if (subInline is Run sRun)
                    {
                        var v = sRun.Text;
                    }
                    yield return subInline;
                }
            }
            yield return inline;
        }
    }
}
