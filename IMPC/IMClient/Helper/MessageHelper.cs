using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;
using IMClient.ViewModels;
using IMClient.Views.Controls;
using IMModels;
using Obisoft.HSharp;
using Util;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using SDKClient;

namespace IMClient.Helper
{
    public static class MessageHelper
    {

        public static readonly Brush MainBrush = new SolidColorBrush() { Color = (Color)ColorConverter.ConvertFromString("#FF688fff") };

        private static string[] _emojes;
        private static string[] imgUrlList;

        public static string[] Emojes
        {
            get
            {
                if (_emojes == null || _emojes.Length == 0)
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
                            Uid = emoj.Name,
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


                                //run.ForceCursor = hl.ForceCursor;
                                //run.Cursor = hl.Cursor;
                                //run.BaselineAlignment = hl.BaselineAlignment;
                                //run.Foreground = hl.Foreground;
                                //run.TextDecorations = hl.TextDecorations;


                                Run hyperlink = new Run();
                                hyperlink.Tag = item;
                                hyperlink.ForceCursor = true;
                                hyperlink.Cursor = System.Windows.Input.Cursors.Hand;
                                hyperlink.BaselineAlignment = BaselineAlignment.TextBottom;
                                hyperlink.TextDecorations = TextDecorations.Underline;
                                BrushConverter converter = new BrushConverter();
                                Brush bsh = (Brush)converter.ConvertFromString("#FF2a57fc");
                                if (isMine)
                                {
                                    hyperlink.Foreground = bsh;// new SolidColorBrush(Colors.White);
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

            if (ContentInlines.FirstOrDefault() is InlineUIContainer)
            {
                ContentInlines.Insert(0, new Run() { BaselineAlignment = BaselineAlignment.TextBottom });
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

        //private static void Hyperlink_Click(object sender, RoutedEventArgs e)
        //{
        //    Hyperlink hyperlink = sender as Hyperlink;
        //    System.Diagnostics.Process.Start(hyperlink.Tag.ToString());
        //}
        //图片Url

        static MatchCollection IsContainsLink(string source)
        {
            //string pattern = @"((http|https)://)?(www.)?[a-z0-9\.]+(\.(com|net|cn|com\.cn|com\.net|net\.cn))(/[^\s\n]*)?";
            //string pattern = @"(http|https|ftp)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&$%\$#\=~])*$";
            //string pattern = @"((http|ftp|file|https)://)?(www.)?(([a-zA-Z0-9\._-]+\.[a-zA-Z]{2,6})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(:[0-9]{1,4})*(/[a-zA-Z0-9\&%_\./-~-]*)?";
            string pattern = "((http[s]{0,1}|ftp)://[a-zA-Z0-9\\.\\-]+\\.([a-zA-Z]{2,4})(:\\d+)?(/[a-zA-Z0-9\\.\\-~!@;#$%^&*+?:_/=]*)?)|(www.[a-zA-Z0-9\\.\\-]+\\.([a-zA-Z]{2,3})(:\\d+)?(/[a-zA-Z0-9\\.\\-~!@;#$%^&*+?:_/=]*)?)|(((http[s]{0,1}|ftp)://|)((?:(?:25[0-5]|2[0-4]\\d|((1\\d{2})|([1-9]?\\d)))\\.){3}(?:25[0-5]|2[0-4]\\d|((1\\d{2})|([1-9]?\\d))))(:\\d+)?(/[a-zA-Z0-9\\.\\-~!@;#$%^&*+?:_/=]*)?)";
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
                if (System.IO.File.Exists(view) && App.ImageFilter.Contains(System.IO.Path.GetExtension(view)))
                {
                    InlineUIContainer uic = new InlineUIContainer(new ChatImage(view)
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
                    tp = tb.CaretPosition = run.ElementEnd;
                }
            }
            else
            {
                Run run = new Run(content, tp);
                try
                {
                    tp = tb.CaretPosition = run.ElementEnd;
                }
                catch
                {

                }
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
            //var result= HTMLConverter.HtmlToXamlConverter.ConvertHtmlToXaml(content,false);

            //var root= XamlReader.Parse(result);
            //if(root is Section section)
            //{
            //    section.Blocks.Remove(section.Blocks.FirstBlock);
            //   var datas = RichTextBoxHelper.GetAllItems(section.Blocks);


            //    tb.Selection.Text = string.Empty;
            //    var tp = tb.CaretPosition;
            //    for (int i = 0; i < datas.Count; i++)
            //    {
            //        if (datas[i] is Run run)
            //        {
            //            run = new Run(run.Text, tp);
            //            tp = run.ContentEnd;
            //        }

            //        else if (datas[i] is InlineUIContainer uic)
            //        {
            //            if (uic.Child is Image img)
            //            {
            //                string path = img.Source.ToString();
            //                if (File.Exists(path))
            //                {
            //                }
            //                else if (img.Source is System.Windows.Media.Imaging.BitmapSource bitSource)
            //                {
            //                    path = Helper.ImageDeal.SaveBitmapImageIntoFile(bitSource);
            //                }
            //                else
            //                {
            //                    continue;
            //                }

            //                ChatImage chatImg = new ChatImage(path, new Size(320, 180))
            //                {
            //                    Uid = ViewModels.AppData.FlagImage + path
            //                };

            //                uic = new InlineUIContainer(chatImg, tp);
            //                tp = uic.ContentEnd;
            //            }
            //            else
            //            {
            //                uic.Child.ToString();
            //            }
            //        }
            //        else
            //        {
            //            //var vv = datas[i].ToString();
            //            //vv.ToString();
            //        }
            //    }
            //}
            //else
            //{

            ////}
            ////root.ToString();
            ////PasteTextDataToRichTextBox(tb, DataFormats.Xaml, result);
            ////PasteTextDataToRichTextBox(tb, DataFormats.Html, content);
            //return;
            try
            {
                var strMsg = ReplaceHtml_IPB(content);

                var tempContent = strMsg.Replace("<img>", "").Replace("</p>", "");
                if (!string.IsNullOrEmpty(tempContent.Trim()))
                {
                    strMsg = strMsg.Replace("<br/>", "\r\n");
                    strMsg = strMsg.Replace("\n", "");
                    strMsg = strMsg.Replace("</p>", "\r\n");
                    strMsg = strMsg.Replace("<img>", "");
                    strMsg = Regex.Replace(strMsg, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
                    strMsg = Regex.Replace(strMsg, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
                    strMsg = Regex.Replace(strMsg, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
                    strMsg = Regex.Replace(strMsg, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
                    if (!string.IsNullOrEmpty(strMsg))
                    {
                        IMClient.Helper.MessageHelper.AppendContentToRichTextBox(tb, strMsg);
                        return;
                    }
                }

                if (imgUrlList.Length > 0)
                {
                    TextPointer tp = tb.CaretPosition;
                    foreach (var img in imgUrlList)
                    {
                        var tempImgPath = img.Replace("file:///", "");
                        if (System.IO.File.Exists(tempImgPath))
                        {
                            InlineUIContainer uic = new InlineUIContainer(new ChatImage(tempImgPath)
                            {
                                Uid = ViewModels.AppData.FlagImage + tempImgPath
                            }, tp);
                            tp = uic.ElementEnd;
                        }
                        else
                        {
                            if (!File.Exists(tempImgPath) && IsUrlRegex(tempImgPath))
                            {
                                //DownloadPicture(imgurl, filePath, -1);
                            }
                        }
                    }
                }
                //    content = content.Replace("&nbsp;", "");
                //    var doc = HtmlConvert.DeserializeHtml(content);

                //    var div = doc.AllUnder.FirstOrDefault(info => info.TagName.ToUpper() == "DIV");

                //    if (div == null)
                //    {
                //        IMClient.Helper.MessageHelper.AppendContentToRichTextBox(tb, Clipboard.GetText());
                //        return;
                //    }

                //    var values = div.Children;

                //    if (values.FirstOrDefault(info => info.TagName.ToLower() == "img") == null)
                //    {
                //        IMClient.Helper.MessageHelper.AppendContentToRichTextBox(tb, Clipboard.GetText());
                //        return;
                //    }
                //    TextPointer tp = tb.CaretPosition;

                //    for (int i = 0; i < values.Count; i++)
                //    {
                //        switch (values[i].TagName.ToLower())
                //        {
                //            case "img":
                //                string src = string.Empty;
                //                if (values[i].Properties.ContainsKey("SRC"))
                //                {
                //                    src = values[i].Properties["SRC"].ToUpper().Replace("FILE:///", "");
                //                }
                //                else if (values[i].Properties.ContainsKey("src"))
                //                {
                //                    src = values[i].Properties["src"].ToUpper().Replace("FILE:///", "");
                //                }
                //                if (System.IO.File.Exists(src))
                //                {
                //                    InlineUIContainer uic = new InlineUIContainer(new ChatImage(src)
                //                    {
                //                        Uid = ViewModels.AppData.FlagImage + src
                //                    }, tp);
                //                    tp = uic.ElementEnd;
                //                }
                //                break;
                //            case "strong":
                //                break;
                //            default:
                //                Run run = new Run(values[i].ToString(), tp);
                //                tp = run.ElementEnd;
                //                break;
                //        }
                //    }
            }
            catch
            {
                IMClient.Helper.MessageHelper.AppendContentToRichTextBox(tb, Clipboard.GetText());
            }
        }
        /// <summary>
        /// 验证文件下载地址
        /// </summary>
        /// <returns></returns>
        public static bool IsUrlRegex(string url)
        {
            string Pattern = @"^(http|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?$";
            Regex r = new Regex(Pattern);
            Match m = r.Match(url);
            return m.Success;
        }

        private static string ReplaceHtml_IPB(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                //删除内含的 样式表代码
                Regex headhtml = new Regex(@"^(\w|\W)*<body(.*?)>", RegexOptions.IgnoreCase);
                string TempStr = headhtml.Replace(str, "");

                Regex bodyhtml = new Regex(@"^(\w|\W)*<body>", RegexOptions.IgnoreCase);
                TempStr = bodyhtml.Replace(TempStr, "");
                Regex bodyhtml1 = new Regex(@"^(\w|\W)*<BODY([^>])*>", RegexOptions.IgnoreCase);
                TempStr = bodyhtml1.Replace(TempStr, "");
                Regex styleHtml = new Regex(@"<style([^>])*>(\w|\W)*?</style([^>])*>", RegexOptions.IgnoreCase);
                TempStr = styleHtml.Replace(TempStr, "");
                Regex RightUserImgRemove = new Regex("<div class=\"rightimg\".*?>.*?</div>", RegexOptions.IgnoreCase);
                TempStr = RightUserImgRemove.Replace(TempStr, "");
                Regex LeftUserImgRemove = new Regex("<div class=\"leftimg\".*?>.*?</div>", RegexOptions.IgnoreCase);
                TempStr = LeftUserImgRemove.Replace(TempStr, "");
                Regex OnceSendFailImgRemove = new Regex("<div class=\"onceSendFail\".*?>.*?</div>", RegexOptions.IgnoreCase);
                TempStr = OnceSendFailImgRemove.Replace(TempStr, "");
                Regex OnceSendImgRemove = new Regex("<div class=\"onceSend\".*?>.*?</div>", RegexOptions.IgnoreCase);
                TempStr = OnceSendImgRemove.Replace(TempStr, "");
                //< div class="onceSend"
                //<([^>]+)> 不过滤 img标签 <br /></p>
                Regex pHtml1 = new Regex("<p(.*?)><br /></p>\n", RegexOptions.IgnoreCase);
                TempStr = pHtml1.Replace(TempStr, "[/p]");
                TempStr = TempStr.Replace("</p>", "[/p]");
                TempStr = TempStr.Replace("</P>", "[/p]");
                TempStr = TempStr.Replace("<p>", "[p]");
                TempStr = TempStr.Replace("<P>", "[p]");
                TempStr = TempStr.Replace("</div>", "[/div]");
                TempStr = TempStr.Replace("</Div>", "[/div]");
                TempStr = TempStr.Replace("<div>", "[div]");
                TempStr = TempStr.Replace("<Div>", "[div]");
                Regex pHtml = new Regex("<p(.*?)>", RegexOptions.IgnoreCase);
                TempStr = pHtml.Replace(TempStr, "[p]");
                //TempStr = Regex.Replace(TempStr, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
                //TempStr = Regex.Replace(TempStr, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
                //TempStr = Regex.Replace(TempStr, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
                //TempStr = Regex.Replace(TempStr, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
                TempStr = Regex.Replace(TempStr, @"&(nbsp|#160);", " ", RegexOptions.IgnoreCase);
                TempStr = Regex.Replace(TempStr, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
                TempStr = Regex.Replace(TempStr, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
                TempStr = Regex.Replace(TempStr, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
                TempStr = Regex.Replace(TempStr, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
                //TempStr = Regex.Replace(TempStr, @"&#32;", " ", RegexOptions.IgnoreCase);
                TempStr = Regex.Replace(TempStr, @"&#39;", "'", RegexOptions.IgnoreCase);
                TempStr = Regex.Replace(TempStr, @"&#(\d+);", " ", RegexOptions.IgnoreCase);
                //hrefUrlList = GetHrefUrls(TempStr);
                imgUrlList = GetHvtImgUrls(TempStr);
                Regex trHtml = new Regex("<tr(.*?)>", RegexOptions.IgnoreCase);
                TempStr = trHtml.Replace(TempStr, "[br/]");
                Regex tdHtml = new Regex("<td(.*?)>", RegexOptions.IgnoreCase);
                TempStr = tdHtml.Replace(TempStr, "   ");
                Regex liHtml = new Regex("<li([^>])*>", RegexOptions.IgnoreCase);
                TempStr = liHtml.Replace(TempStr, "[br/]");
                Regex brHtml = new Regex("<br(.*?)>", RegexOptions.IgnoreCase);
                TempStr = brHtml.Replace(TempStr, "[br/]");
                Regex spanHtml1 = new Regex("<span(.*?)>", RegexOptions.IgnoreCase);
                TempStr = spanHtml1.Replace(TempStr, "[span]");
                Regex spanHtml2 = new Regex("</span>", RegexOptions.IgnoreCase);
                TempStr = spanHtml2.Replace(TempStr, "[/span]");
                Regex imgHtml = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\t\r]*[""']?[\t\r\n]*(?<imgUrl>[^\t\r\n""'<>]*)[^<>]*?/?[\t\r\n]*>", RegexOptions.IgnoreCase);
                TempStr = imgHtml.Replace(TempStr, "[img]");
                //Regex imgHtml1 = new Regex("<IMG(.*?)>", RegexOptions.IgnoreCase);
                //TempStr = imgHtml1.Replace(TempStr, "[img]");
                Regex cutHtml = new Regex("<([^>]+)>", RegexOptions.IgnoreCase);
                TempStr = cutHtml.Replace(TempStr, "");
                Regex aHtml = new Regex("<a class=\"sup-anchor\".*?>", RegexOptions.IgnoreCase);
                TempStr = aHtml.Replace(TempStr, "");
                Regex aHtml1 = new Regex("<a class=\"sub-anchor\".*?>", RegexOptions.IgnoreCase);
                TempStr = aHtml1.Replace(TempStr, "");
                Regex aHtml2 = new Regex("<a name(.*?)></a>", RegexOptions.IgnoreCase);
                TempStr = aHtml2.Replace(TempStr, "");
                Regex aHtml3 = new Regex("<a id(.*?)>", RegexOptions.IgnoreCase);
                TempStr = aHtml3.Replace(TempStr, "");
                Regex regTitle = new Regex(@"<a\s+.*?href=""([^""]*)""\s+.*?title=""([^""]*)"".*?>", RegexOptions.IgnoreCase);
                TempStr = regTitle.Replace(TempStr, "");
                Regex aHtml4 = new Regex("<a(.*?)>.*?</a>", RegexOptions.IgnoreCase);
                TempStr = aHtml4.Replace(TempStr, "[a] ");
                //TempStr = TempStr.Replace ("/>" , ">");
                //Regex ImgHtml=new Regex("<img",RegexOptions.IgnoreCase);
                //格式化现有代码
                //TempStr = HttpUtility.HtmlEncode(TempStr);

                //TempStr = TempStr.Replace("[body]", "<body>");
                //TempStr = TempStr.Replace("[/body]", "</body>");
                TempStr = TempStr.Replace("[img]", "<img>");
                TempStr = TempStr.Replace("[span]", "");
                //TempStr = TempStr.Replace("[a", "<a");
                TempStr = TempStr.Replace("[a]", "<a>");
                TempStr = TempStr.Replace("[p]", "");
                TempStr = TempStr.Replace("\r\n", "");
                TempStr = TempStr.Replace("[/p]", "</p>");
                TempStr = TempStr.Replace("[br/]", "<br/>");
                TempStr = TempStr.Replace("[/span]", "");
                TempStr = TempStr.Replace("[/div]", "</p>");
                TempStr = TempStr.Replace("[div]", "");
                TempStr = TempStr.Trim("\r\n".ToCharArray());
                return TempStr;

            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// 解析HTML中Image的Url
        /// </summary>
        /// <param name="sHtmlText"></param>
        private static string[] GetHvtImgUrls(string sHtmlText)
        {
            // 定义正则表达式用来匹配 img 标签 
            Regex m_hvtRegImg = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\t\r]*[""']?[\t\r\n]*(?<imgUrl>[^\t\r\n""'<>]*)[^<>]*?/?[\t\r\n]*>", RegexOptions.IgnoreCase);
            // 搜索匹配的字符串 
            MatchCollection matches = m_hvtRegImg.Matches(sHtmlText);
            int m_i = 0;
            string[] sUrlList = new string[matches.Count];
            // 取得匹配项列表 
            foreach (Match match in matches)
                sUrlList[m_i++] = match.Groups["imgUrl"].Value.Replace("&amp;", "&");
            return sUrlList;
        }
        private static bool PasteTextDataToRichTextBox(RichTextBox tb, string dataFormat, string textData)
        {
            var pasted = false;

            var document = tb.Document;
            var textRange = new TextRange(document.ContentStart, document.ContentEnd);

            Stream stream = new MemoryStream();

            var bytesUnicode = Encoding.Unicode.GetBytes(textData);
            var bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, bytesUnicode);

            if (bytes.Length > 0 && textRange.CanLoad(dataFormat))
            {
                stream.Write(bytes, 0, bytes.Length);
                textRange.Load(stream, dataFormat);
                pasted = true;
            }

            return pasted;
        }

        public static void SetRichTextBoxSelectionToClipboard(List<Inline> datas)
        {
            if (datas.Count == 1 && datas[0] is Run value)
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
                }
                else if (datas[i] is LineBreak)
                {
                    sb.Append("\r\n");
                    strSB.Append("\r\n");
                }
                else if (datas[i] is InlineUIContainer uic)
                {
                    string uid = uic.Child.Uid;
                    if (uid.StartsWith(AppData.FlagImage))
                    {
                        var imgPath = uic.Child.Uid.Replace(AppData.FlagImage, string.Empty);
                        var chatImage = uic.Child as ChatImage;
                        if (chatImage != null && !string.IsNullOrEmpty(chatImage.Tag.ToString()))
                        {
                            var imgKey = chatImage.Tag.ToString();

                            string imagePath = System.IO.Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, imgKey);
                            if (File.Exists(imagePath))
                            {
                                imgPath = imagePath;
                            }
                            else
                            {
                                SDKClient.SDKClient.Instance.DownLoadResource(imgKey, imgKey, SDKClient.Model.FileType.img, null, (b) =>
                                {
                                    if (b)
                                    {
                                        return;
                                    }
                                    else
                                    {

                                        return;
                                    }
                                }, imgKey);
                            }

                            imgPath = imagePath;

                        }
                        strSB.Append(uic.Child.Uid + AppData.FlagImage);
                        string src = uic.Child.Uid.Replace(AppData.FlagImage, string.Empty);
                        sb.Append($"<img src=\"file:///{imgPath}\">\r");
                    }
                    else if (uid.StartsWith(AppData.FlagAt))
                    {

                        string atValue = uic.Child.Uid.Split('|')[1];
                        strSB.Append(atValue);
                        sb.Append(atValue);
                    }
                    else if (uid.StartsWith(AppData.FlagSmallVideo))
                    {

                    }
                    //else if (uic.Child is ChatImage cImg)
                    //{
                    //    sb.Append($"<IMG SRC ={uic.Child.Uid}>\r");
                    //}
                }
            }

            string strValue = sb.ToString();

            sb.Insert(0, "<DIV>\r");
            sb.Append("</DIV>\r");

            string result = sb.ToString();



            //string value = $"<DIV><IMG src=\"file:///{this.ImagePath }\"></DIV> ";
            //Helper.ClipboardHelper.CopyToClipboard(result, strValue);
            var dataObject = Helper.ClipboardHelper.CreateDataObject(result, strValue);
            dataObject.SetData(AppData.FlagCopy, strSB.ToString());
            System.Windows.Forms.Clipboard.SetDataObject(dataObject);
        }

        static bool IsUpdateUpload = false;
        public static void LoadImgContent(MessageModel msg, bool isLoadSmall = false, bool isShareMsg = false)
        {
            //if (IsUpdateUpload)
            //    return;
            Task.Run(async () =>
            {
                //    IsUpdateUpload = true;
                //ThreadPool.QueueUserWorkItem(m =>
                //{
                if (msg.MsgType != MessageType.img && !isShareMsg)
                {
                    return;
                }

                try
                {
                    if (string.IsNullOrEmpty(msg.Content))
                    {

                        string targetKey = isLoadSmall ? msg.ResourceModel.SmallKey : msg.ResourceModel.Key;
                        if (string.IsNullOrEmpty(targetKey))
                            targetKey = msg.ResourceModel.Key;
                        string imagePath = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, targetKey);
                        if (!isShareMsg)
                            msg.Content = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, targetKey);
                    }
                    var isUpload = false;
                    if ((msg.IsMine && msg.IsSync) || !msg.IsMine)
                    {
                        try
                        {
                            if (File.Exists(msg.Content))
                            {
                                using (FileStream fs = File.OpenRead(msg.Content))
                                {
                                    string md5 = Util.Helpers.Encrypt.Md5By32(fs);
                                    var imgMd5 = $"{md5}{System.IO.Path.GetExtension(msg.Content)}";
                                    string targetKey = isLoadSmall ? msg.ResourceModel.SmallKey : msg.ResourceModel.Key;
                                    if (targetKey != imgMd5)
                                    {
                                        isUpload = true;
                                    }
                                }

                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    if (!File.Exists(msg.Content) || isUpload)
                    {

                        string targetKey = isLoadSmall ? msg.ResourceModel.SmallKey : msg.ResourceModel.Key;
                        if (string.IsNullOrEmpty(targetKey))
                            targetKey = msg.ResourceModel.Key;
                        string imagePath = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, targetKey);

                        //msg.Content = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, targetKey);

                        msg.OperateTask = new System.Threading.CancellationTokenSource();
                        //isFail = !isFail;
                        //if (isFail)
                        //{
                        //    msg.MessageState = MessageStates.Fail;
                        //    return;
                        //}

                        msg.MessageState = MessageStates.None;
                        //Task.Run(async () =>
                        //{
                            var resultFile = await SDKClient.SDKClient.Instance.FindFileResource(targetKey);
                            if (!resultFile)
                            {
                                msg.MessageState = MessageStates.ExpiredFile;
                                return;
                            }
                        //});
                        SDKClient.SDKClient.Instance.DownLoadResource(targetKey, targetKey, SDKClient.Model.FileType.img, null, (b) =>
                        {
                            if (!isShareMsg)
                            {
                                string old = msg.Content;
                                msg.Content = null;
                            }
                            if (b)
                            {
                                var imgPath = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, targetKey);
                                if (!isShareMsg)
                                    msg.Content = imgPath;
                                else
                                    msg.ShareMsgImage = imgPath;
                                msg.MessageState = MessageStates.Success;
                                SDKClient.SDKClient.Instance.UpdateMsgFileName(msg.MsgKey, imgPath);
                            }
                            else
                            {

                                if (isLoadSmall && !msg.IsMine) //若下载缩略图失败，直接失败
                                {
                                    msg.MessageState = MessageStates.Fail;
                                }
                                else //若非缩略图，可以尝试下载缩略图
                                {
                                    LoadImgContent(msg, true);

                                }
                            }
                        }, msg.MsgKey);
                    }
                    else if (!msg.IsMine)
                    {

                        msg.MessageState = MessageStates.Success;
                        //msg.Content = imagePath;
                    }

                    //msg.Content = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, msg.P_resourcesmallId);
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
                //}).ContinueWith(t =>
                //{
                //    IsUpdateUpload = false;
                //});
            });
        }

        public static void LoadFileContent(MessageModel msg, System.Threading.CancellationTokenSource operate, ChatViewModel chatVM, Action<bool, SDKProperty.ErrorState> callback, string savePath = null)
        {
            Task.Run(async () =>
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
                    if (!File.Exists(msg.Content) || savePath != null || (msg.ResourceModel != null && msg.ResourceModel.Progress < 1))
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
                                       msg.MessageState = MessageStates.Success;
                                       msg.ResourceModel.FileName = Path.GetFileName(msg.Content);
                                       msg.ResourceModel.FileState = FileStates.Completed;
                                       callback?.Invoke(true, SDKProperty.ErrorState.None);
                                   }
                                   else
                                   {
                                       if (msg.MessageState == MessageStates.Fail)
                                       {
                                           return;
                                       }
                                       OnlineFail(chatVM, msg, result.notifyPackage.Content);
                                       callback?.Invoke(false, SDKProperty.ErrorState.None);
                                   }
                               },
                               (processSize) =>
                               {
                                   msg.ResourceModel.CompleteLength += processSize;
                               }, operate.Token);

                            if (!isSuccess)//连接不成功
                            {
                                long filesize = body.fileSize;

                                string content = $"网络异常，\"{Path.GetFileName(msg.Content)}\"({filesize.GetFileSizeString()})文件传输失败。";
                                OnlineFail(chatVM, msg, content);
                                callback?.Invoke(false, SDKProperty.ErrorState.None);
                            }
                        }
                        else
                        {
                            //Task.Run(async () =>
                            //{
                            var resultFile = await SDKClient.SDKClient.Instance.FindFileResource(msg.ResourceModel.Key);
                            if (!resultFile)
                            {
                                AppData.MainMV.TipMessage = "文件已过期！";
                                return;
                            }
                            //});
                            SDKClient.SDKClient.Instance.DownloadFileWithResume(msg.ResourceModel.Key, msg.Content, SDKClient.Model.FileType.file,
                            (processSize) =>
                            {
                                msg.ResourceModel.CompleteLength += processSize;
                            },
                            (result, errorState) =>
                            {
                                callback.Invoke(result, errorState);
                                if (result)// || msg.Sender == ViewModels.AppData.Current.LoginUser.User)
                                {
                                    SetFileReult(msg);

                                    msg.ResourceModel.FileName = Path.GetFileName(msg.Content);

                                    if (!Path.IsPathRooted(msg.Content))
                                    {
                                        msg.Content = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.filePath, msg.ResourceModel.FileName);
                                    }
                                    msg.MessageState = MessageStates.Success;
                                    msg.ResourceModel.FileState = FileStates.Completed;
                                    SDKClient.SDKClient.Instance.UpdateMsgFileName(msg.MsgKey, msg.Content);

                                }
                                else
                                {
                                    msg.ResourceModel.FileState = FileStates.WaitForReceieve;
                                    if (msg.MessageState == MessageStates.Fail)
                                    {
                                        return;
                                    }
                                    if (!operate.IsCancellationRequested)
                                        AppData.MainMV.TipMessage = "文件服务器异常，接收失败！";
                                    switch (errorState)
                                    {
                                        case SDKClient.SDKProperty.ErrorState.NetworkException:
                                        case SDKClient.SDKProperty.ErrorState.ServerException:

                                            return;

                                    }

                                    msg.ResourceModel.CompleteLength = 0;
                                    msg.MessageState = MessageStates.Fail;
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
        static bool isLoadVidoPre;
        /// <summary>
        /// 加载视频预览图
        /// </summary>
        /// <param name="msg"></param>
        public static void LoadVideoPreviewImage(MessageModel msg)
        {
            try
            {
                if (isLoadVidoPre)
                    return;
                string targetKey = msg.ResourceModel.PreviewKey;
                if (string.IsNullOrEmpty(msg.ResourceModel.PreviewImagePath))
                {
                    msg.ResourceModel.PreviewImagePath = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, targetKey);
                }

                Task.Run(() =>
                {
                    if (!File.Exists(msg.ResourceModel.PreviewImagePath))
                    {
                        msg.OperateTask = new System.Threading.CancellationTokenSource();
                        SDKClient.SDKClient.Instance.DownloadFileWithResume(targetKey, msg.ResourceModel.PreviewImagePath, SDKClient.Model.FileType.img, null, (b, errorState) =>
                        {
                            if (b)
                            {
                                msg.ResourceModel.PreviewImagePath = null;
                                msg.ResourceModel.PreviewImagePath = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.imgPath, targetKey);
                            }
                            else
                            {
                                msg.OperateTask?.Cancel();
                            }
                        }, msg.MsgKey, null, msg.OperateTask.Token);
                    }
                }).ContinueWith(t =>
                {
                    return isLoadVidoPre = false;
                });
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        public static void LoadVideoContent(MessageModel msg, Action<bool> callback = null)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (msg.IsSending)
                    {
                        return;
                    }

                    msg.Content = msg.ResourceModel.FullName;

                    if (!File.Exists(msg.Content))
                    {

                        //Task.Run(async () =>
                        //{
                        var resultFile = await SDKClient.SDKClient.Instance.FindFileResource(msg.ResourceModel.Key);
                        if (!resultFile)
                        {
                            AppData.MainMV.TipMessage = "文件已过期！";
                            msg.MessageState = MessageStates.None;
                            return;
                        }
                        //});
                        msg.MessageState = MessageStates.Loading;
                        msg.OperateTask = new System.Threading.CancellationTokenSource();
                        SDKClient.SDKClient.Instance.DownloadFileWithResume(msg.ResourceModel.Key, msg.Content, SDKClient.Model.FileType.file,
                            (processSize) =>
                            {
                                msg.ResourceModel.CompleteLength = processSize;
                            },
                            (result, errorState) =>
                            {
                                if (result || msg.Sender == ViewModels.AppData.Current.LoginUser.User)
                                {
                                    msg.ResourceModel.FileState = FileStates.Completed;
                                    msg.ResourceModel.FileName = Path.GetFileName(msg.Content);
                                    msg.MessageState = MessageStates.Success;
                                    SDKClient.SDKClient.Instance.UpdateMsgFileName(msg.MsgKey, msg.Content);
                                }
                                else
                                {
                                    msg.ResourceModel.CompleteLength = 0;
                                    msg.ResourceModel.FileState = FileStates.Fail;
                                    msg.MessageState = MessageStates.Fail;

                                    msg.OperateTask?.Cancel();
                                }

                                callback?.Invoke(result);
                            }, msg.MsgKey, cancellationToken: msg.OperateTask.Token);
                    }
                    else if (!msg.IsSending)
                    {
                        if (File.Exists(msg.Content))
                        {
                            msg.ResourceModel.CompleteLength = msg.ResourceModel.Length;
                            msg.ResourceModel.FileState = FileStates.Completed;
                            msg.ResourceModel.FileName = Path.GetFileName(msg.Content);
                            msg.MessageState = MessageStates.Success;
                        }
                        else
                        {
                            msg.ResourceModel.CompleteLength = 0;
                            msg.ResourceModel.FileState = FileStates.Fail;
                            msg.MessageState = MessageStates.Fail;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            });
        }

        private static void OnlineFail(ChatViewModel chatVM, MessageModel msg, string content)
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
                msg.ResourceModel.FileImg = Helper.FileHelper.GetFileImage(msg.Content, true);

            }
        }
    }

    public class RichTextBoxHelper
    {
        public static List<Inline> GetSelectionItems(RichTextBox richBox)
        {
            List<Inline> datas = new List<Inline>();
            var selection = richBox.Selection;
            if (!selection.IsEmpty)
            {
                var items = GetInlinesFromDocument(richBox.Document.Blocks).ToList();

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
                List<Inline> results = new List<Inline>() { datas[0] };

                Run per = null;
                if (datas[0] is Run run)
                {
                    per = run;
                }
                else if (datas[0] is LineBreak)
                {
                    results[0] = per = new Run("\r\n");
                }

                for (int i = 1; i < datas.Count; i++)
                {
                    if (datas[i] is Span span && i != datas.Count - 1)
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

                    if (datas[i] is Run nRun)
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
                    else if (datas[i] is LineBreak lb)
                    {
                        if (per == null)
                        {
                            results.Add(per = new Run("\r\n"));
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

        public static List<Inline> GetAllItems(BlockCollection blocks)
        {
            var datas = GetInlinesFromDocument(blocks).ToList();

            if (datas.Count > 0)
            {
                List<Inline> results = new List<Inline>() { datas[0] };

                Run per = null;
                if (datas[0] is Run run)
                {
                    per = run;
                }
                else if (datas[0] is LineBreak)
                {
                    results[0] = per = new Run("\r\n");
                }

                for (int i = 1; i < datas.Count; i++)
                {
                    if (datas[i] is Span span && i != datas.Count - 1)
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

                    if (datas[i] is Run nRun)
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
                    else if (datas[i] is LineBreak lb)
                    {
                        if (per == null)
                        {
                            results.Add(per = new Run("\r\n"));
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

            return new List<Inline>();
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
                else if (container.Child is FileChatItem fChat)
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

        static IEnumerable<Inline> GetInlinesFromDocument(BlockCollection blocks)
        {

            foreach (var block in blocks)
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
