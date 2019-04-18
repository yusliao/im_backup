using IMClient.Helper;
using IMClient.ViewModels;
using IMClient.Views.Panels;
using IMModels;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace IMClient.Views.ChildWindows
{
    /// <summary>
    /// GroupNoticeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GroupNoticeWindow : Window
    {
        object pageContext = null;
        public GroupNoticeWindow()
        {
            InitializeComponent();
            this.MaxHeight = PrimaryScreen.MaxAreaHeight;
            this.MaxWidth = PrimaryScreen.MaxAreaWidth;
        }
        public GroupNoticeWindow(object _pageContext,string title=null):this()
        {
            GroupNotice gnTice = _pageContext as GroupNotice;
            if(AppData.ordinaryGroupNoticeDic.Count>0||AppData.joinGroupNeedKnowDic.Count>0)
            {
                if (gnTice.NoticeType==0)
                {
                    if (AppData.ordinaryGroupNoticeDic.ContainsKey(gnTice.GroupId))
                        this.DataContext = AppData.ordinaryGroupNoticeDic[gnTice.GroupId];
                    else
                        this.DataContext = gnTice;
                }
                else
                {
                    if (AppData.joinGroupNeedKnowDic.ContainsKey(gnTice.GroupId))
                        this.DataContext = AppData.joinGroupNeedKnowDic[gnTice.GroupId];
                    else
                        this.DataContext = gnTice;
                }   
            }
            else
            {
                this.pageContext = _pageContext;
                this.DataContext = pageContext;
            }
            if (!string.IsNullOrEmpty(title))
            {
                this.tbName.Text = title;
            }
            this.MouseLeftButtonDown += GroupNoticeWindow_MouseLeftButtonDown;
            this.Loaded += GroupNoticeWindow_Loaded;
        }

        private void GroupNoticeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Activate();
        }

        private void GroupNoticeWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void Grid_Click(object sender, RoutedEventArgs e)
        {
            Button btn = e.Source as Button;
            if (btn!=null)
            {
                string uid = btn.Uid;
                switch (uid)
                {
                    case "Cancel":
                    case "Close":
                        GroupNotice groupNotice = this.DataContext as GroupNotice;
                        if (groupNotice != null)
                        {
                            if (groupNotice.NoticeType == 0)
                            {
                                if (!AppData.ordinaryGroupNoticeDic.ContainsKey(groupNotice.GroupId))
                                    AppData.ordinaryGroupNoticeDic.Add(groupNotice.GroupId, groupNotice);
                            }
                            else
                            {
                                if (!AppData.joinGroupNeedKnowDic.ContainsKey(groupNotice.GroupId))
                                    AppData.joinGroupNeedKnowDic.Add(groupNotice.GroupId, groupNotice);
                            }
                        }   
                        this.Close();
                        break;
                    case "Sure":
                        GroupNotice gn = this.DataContext as GroupNotice;
                        if(AppData.CanInternetAction())
                        {
                            if (gn != null)
                            {
                                List<ChatViewModel> list = AppData.MainMV.ChatListVM.Items.ToList().Where(x => x.IsGroup).Where(x => x.TargetVM != null).ToList();
                                ChatViewModel chatVM = list.FirstOrDefault(x => ((x.TargetVM as GroupViewModel).Model as GroupModel).ID == gn.GroupId);
                                if (gn.NoticeType == 0)
                                {
                                    if (!AppData.ordinaryGroupNoticeDic.ContainsKey(gn.GroupId))
                                        AppData.ordinaryGroupNoticeDic.Add(gn.GroupId, gn);
                                }
                                else
                                {
                                    if (!AppData.joinGroupNeedKnowDic.ContainsKey(gn.GroupId))
                                        AppData.joinGroupNeedKnowDic.Add(gn.GroupId, gn);
                                }
                                GroupViewModel gvm = AppData.MainMV.GroupListVM.Items.ToList().FirstOrDefault(x => (x.Model as GroupModel).ID == gn.GroupId);
                                if (CheckDataLegal(gn))
                                {
                                    int index = 0;
                                    //bool isFirstBadWordTitle = SDKClient.SDKProperty.stringSearchEx.FindFirst(gn.NoticeTitle, out index);
                                    bool isContainsBadWordTitle = SDKClient.SDKProperty.stringSearchEx.ContainsAny(gn.NoticeTitle);
                                    //bool isFirstBadWordContent = SDKClient.SDKProperty.stringSearchEx.FindFirst(gn.NoticeContent, out index);
                                    bool isContainsBadWordContent = SDKClient.SDKProperty.stringSearchEx.ContainsAny(gn.NoticeContent);
                                    if (!isContainsBadWordTitle && !isContainsBadWordContent)
                                    {
                                        MessageModel msg = new MessageModel()
                                        {
                                            Sender = AppData.Current.LoginUser.User,
                                            SendTime = DateTime.Now,
                                            MsgType = MessageType.addgroupnotice,
                                            IsMine = true,
                                            Content = gn.NoticeContent
                                        };
                                        GroupNoticeModel gnModel = new GroupNoticeModel()
                                        {
                                            NoticeTitle = gn.NoticeTitle,
                                            NoticeId = gn.NoticeId,
                                            GroupMId = (gvm.Model as GroupModel).ID,
                                            GroupNoticeContent=gn.NoticeContent
                                        };
                                        msg.NoticeModel = gnModel;
                                        msg.TipMessage = string.Format("{0}", msg.NoticeModel.NoticeTitle);
                                        if (gn.NoticeTitle.Equals("入群须知"))
                                            SDKClient.SDKClient.Instance.AddNotice(gn.NoticeTitle, gn.NoticeContent, gn.GroupId, gn.GroupName, (result) =>
                                            {
                                                if (result.Item1)
                                                {
                                                    gn.NoticeId = result.Item2;
                                                    App.Current.Dispatcher.Invoke(new Action(() =>
                                                    {
                                                        AppData.MainMV.TipMessage = "发布成功！";
                                                        if (AppData.joinGroupNeedKnowDic.ContainsKey(gn.GroupId))
                                                            AppData.joinGroupNeedKnowDic.Remove(gn.GroupId);
                                                        gvm.RefreshNoticeList(gn);
                                                        msg.MsgKey = result.Item3;
                                                        gnModel.NoticeId = result.Item2;
                                                        msg.NoticeModel = gnModel;
                                                        chatVM.AddMessage(msg);
                                                    }));
                                                }
                                                else
                                                    App.Current.Dispatcher.Invoke(new Action(() =>
                                                    {
                                                        AppData.MainMV.TipMessage = "发布失败！";
                                                        if (!AppData.joinGroupNeedKnowDic.ContainsKey(gn.GroupId))
                                                            AppData.joinGroupNeedKnowDic.Add(gn.GroupId, gn);
                                                    }));
                                            }, SDKClient.SDKProperty.NoticeType.JoinGroupNotice);
                                        else
                                            SDKClient.SDKClient.Instance.AddNotice(gn.NoticeTitle, gn.NoticeContent, gn.GroupId, gn.GroupName, (result) =>
                                            {
                                                if (result.Item1)
                                                {
                                                    gn.NoticeId = result.Item2;
                                                    App.Current.Dispatcher.Invoke(new Action(() =>
                                                    {
                                                        AppData.MainMV.TipMessage = "发布成功！";
                                                        if (AppData.ordinaryGroupNoticeDic.ContainsKey(gn.GroupId))
                                                            AppData.ordinaryGroupNoticeDic.Remove(gn.GroupId);
                                                        gvm.RefreshNoticeList(gn);
                                                        msg.MsgKey = result.Item3;
                                                        gnModel.NoticeId = result.Item2;
                                                        msg.NoticeModel = gnModel;
                                                        chatVM.AddMessage(msg);
                                                    }));
                                                }
                                                else
                                                    App.Current.Dispatcher.Invoke(new Action(() =>
                                                    {
                                                        AppData.MainMV.TipMessage = "发布失败！";
                                                        if (!AppData.ordinaryGroupNoticeDic.ContainsKey(gn.GroupId))
                                                            AppData.ordinaryGroupNoticeDic.Add(gn.GroupId, gn);
                                                    }));
                                            }, SDKClient.SDKProperty.NoticeType.Common);
                                        this.Close();
                                    }
                                    else
                                    {
                                        if (isContainsBadWordTitle)
                                            TitleContainsBadWord(gn.NoticeTitle);
                                        else
                                        {
                                            if (isContainsBadWordContent)
                                                ContentContainsBadWord(gn.NoticeContent);
                                        } 
                                    }
                                }
                                else
                                {
                                    App.Current.Dispatcher.Invoke(new Action(() => {
                                        if (gn.NoticeType == 0)
                                        {
                                            if (AppData.ordinaryGroupNoticeDic.ContainsKey(gn.GroupId))
                                                this.DataContext = AppData.ordinaryGroupNoticeDic[gn.GroupId];
                                        }
                                        else
                                        {
                                            if (AppData.joinGroupNeedKnowDic.ContainsKey(gn.GroupId))
                                                this.DataContext = AppData.joinGroupNeedKnowDic[gn.GroupId];
                                        }
                                        //AppData.MainMV.TipMessage = "发布失败！";
                                    }));
                                    return;
                                }
                            }
                            //else
                            //{
                            //    App.Current.Dispatcher.Invoke(new Action(() =>
                            //    {
                            //        AppData.MainMV.TipMessage = "请检查数据合法性！";
                            //    }));
                            //}
                        }
                        else
                        {
                            App.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                if (gn.NoticeType == 0)
                                {
                                    if (!AppData.ordinaryGroupNoticeDic.ContainsKey(gn.GroupId))
                                        AppData.ordinaryGroupNoticeDic.Add(gn.GroupId, gn);
                                }
                                else
                                {
                                    if (!AppData.joinGroupNeedKnowDic.ContainsKey(gn.GroupId))
                                        AppData.joinGroupNeedKnowDic.Add(gn.GroupId, gn);
                                }
                                AppData.MainMV.TipMessage = "网络异常，请检查网络设置！";
                            }));
                        }
                        break;
                }
            }
           
        }
        /// <summary>
        /// 校验数据合法性
        /// </summary>
        /// <param name="gn">数据实体</param>
        /// <returns></returns>
        private bool CheckDataLegal(GroupNotice gn)
        {
            if (!string.IsNullOrEmpty(gn.NoticeTitle)&&!string.IsNullOrEmpty(gn.NoticeContent))
            {
                int titleLegth = gn.NoticeTitle.Replace(" ", "").Length;
                int contentLegth = gn.NoticeContent.Replace(" ", "").Length;
                if (titleLegth < 4)
                {
                    this.sp_Error.Visibility = Visibility.Visible;
                    this.txb_ErrorTip.Text = "标题至少需要4位！";
                    return false;
                }

                if (contentLegth < 20)
                {
                    this.sp_Error.Visibility = Visibility.Visible;
                    this.txb_ErrorTip.Text = "正文最少需要20位！";
                    return false;
                }
                return true;
            }
            else if(string.IsNullOrEmpty(gn.NoticeTitle))
            {
                this.sp_Error.Visibility = Visibility.Visible;
                this.txb_ErrorTip.Text = "标题不能为空！";
                return false;
            }
            else if(string.IsNullOrEmpty(gn.NoticeContent))
            {
                this.sp_Error.Visibility = Visibility.Visible;
                this.txb_ErrorTip.Text = "内容不能为空！";
                return false;
            }
            return false;
        }

        /// <summary>
        /// 激活数据输入时，显示错误提示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txb_Title_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            TextBox tb = sender as TextBox;
            int length = tb.Text.Replace(" ","").Length;
            //tb.Text = tb.Text.Replace(" ", "");//去掉所有的空格
            //tb.SelectionStart = tb.Text.Length;//尝试将光标移到结尾处
            if (length >= 4)
            {
                this.sp_Error.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (length < 4)
                {
                    this.sp_Error.Visibility = Visibility.Visible;
                    this.txb_ErrorTip.Text = "标题至少需要4位！";
                }
            }
        }

       

        /// <summary>
        /// 激活数据输入时，显示错误提示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txb_Content_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            int length = tb.Text.Length;
            this.txb_Input.Text = length.ToString();
            //tb.Text = tb.Text.Replace(" ", "");//去掉所有的空格
            //tb.SelectionStart = tb.Text.Length;//尝试将光标移到结尾处
            if (length >= 20)
            {
                this.sp_Error.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (length < 20)
                {
                    this.sp_Error.Visibility = Visibility.Visible;
                    this.txb_ErrorTip.Text = "正文最少需要20位！";
                }
            }
        }

       

        private void RefreshNoticeList(GroupNotice notice)
        {
            GroupNotice gn = this.DataContext as GroupNotice;
            GroupViewModel gvm = AppData.MainMV.GroupListVM.Items.ToList().FirstOrDefault(x => (x.Model as GroupModel).ID == gn.GroupId);
            gvm.AllNotice.Add(new GroupNotice()
            {
                GroupId = notice.GroupId,
                ID = notice.ID,
                GroupName = notice.GroupName,
                IsCanOperate = notice.IsCanOperate,
                NoticeId = notice.NoticeId,
                IsToTop = notice.IsToTop,
                NoticeContent = notice.NoticeContent,
                NoticeTitle = notice.NoticeTitle,
                NoticeType = notice.NoticeTitle.Equals("入群须知") ? 1 : 0
            });
            if (gvm.AllNotice.Any(x => x.NoticeType == 1))
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    gvm.AllNotice.OrderByDescending(x => x.NoticeType == 1).OrderByDescending(x => x.NoticeReleTime);
                    gvm.IsShowEmptyNotice = false;
                    gvm.IsShowEmptyNoticeOrdinary = false;
                   
                }));
            }
            else
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    gvm.AllNotice.OrderByDescending(x => x.NoticeReleTime);
                    gvm.IsShowEmptyNotice = true;
                }));
            } 
        }

        private void TitleContainsBadWord(string para)
        {
            //GroupNotice notice = this.DataContext as GroupNotice;
            //string para = notice.NoticeTitle;
            int index = 0;
            bool isFirstBadWord = SDKClient.SDKProperty.stringSearchEx.FindFirst(para, out index);
            bool isContainsBadWord = SDKClient.SDKProperty.stringSearchEx.ContainsAny(para);
            List<string> badWordLi = SDKClient.SDKProperty.stringSearchEx.FindAll(para);
            List<string> goodWordLi = para.Split(badWordLi.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> badWordLiOperate = new List<string>();
            StringBuilder stringBuilder = new StringBuilder(para);
            foreach (string child in badWordLi)
            {
                badWordLiOperate.Add("|" + child + "|");
            }
            foreach (string child1 in badWordLi)
            {
                foreach (string child2 in badWordLiOperate)
                {
                    if (child1.Equals(child2.Replace("|", string.Empty)))
                        stringBuilder.Replace(child1, child2);
                }
            }
            List<string> resultList = new List<string>(stringBuilder.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
            if (isFirstBadWord || isContainsBadWord)
            {
                //this.txb_Title.Visibility = Visibility.Collapsed;
                //this.border_Title.Visibility = Visibility.Visible;
                //this.textBlockTitle.Inlines.Clear();
                //BrushConverter brushConverter = new BrushConverter();
                //Brush brush = (Brush)brushConverter.ConvertFromString("#FEE057");
                //foreach (string child in resultList)
                //{
                //    if (badWordLi.Contains(child))
                //    {
                //        this.textBlockTitle.Inlines.Add(new Run(child) { Background = brush });
                //    }
                //    else
                //    {
                //        this.textBlockTitle.Inlines.Add(new Run(child));
                //    }
                //}
                StringBuilder sb = new StringBuilder();
                badWordLi.ForEach(x => sb.Append("\"" + x + "\"" + "、"));
                string result = sb.ToString().TrimEnd('、');
                
                MessageBoxEx mbox = new MessageBoxEx(string.Format("【正文】中包含敏感词{0}，请修改后再试",result) , isCancelShow: false);
                mbox.ShowDialog();
                //this.txb_Title.Visibility = Visibility.Visible;
                //this.border_Title.Visibility = Visibility.Collapsed;
                this.txb_Title.Focus();
            }
        }
        private void ContentContainsBadWord(string para)
        {
            //GroupNotice notice = this.DataContext as GroupNotice;
            //string para = notice.NoticeContent;
            this.UpdateLayout();
            this.txb_Title.Visibility = Visibility.Visible;
            int index = 0;
            bool isFirstBadWord = SDKClient.SDKProperty.stringSearchEx.FindFirst(para, out index);
            bool isContainsBadWord = SDKClient.SDKProperty.stringSearchEx.ContainsAny(para);
            List<string> badWordLi = SDKClient.SDKProperty.stringSearchEx.FindAll(para);
            List<string> goodWordLi = para.Split(badWordLi.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> badWordLiOperate = new List<string>();
            StringBuilder stringBuilder = new StringBuilder(para);
            foreach (string child in badWordLi)
            {
                badWordLiOperate.Add("|" + child + "|");
            }
            foreach (string child1 in badWordLi)
            {
                foreach (string child2 in badWordLiOperate)
                {
                    if (child1.Equals(child2.Replace("|", string.Empty)))
                        stringBuilder.Replace(child1, child2);
                }
            }
            List<string> resultList = new List<string>(stringBuilder.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
            if (isFirstBadWord || isContainsBadWord)
            {
                //this.txb_Content.Visibility = Visibility.Collapsed;
                //this.border_Content.Visibility = Visibility.Visible;
                //this.textBlockContent.Inlines.Clear();
                //BrushConverter brushConverter = new BrushConverter();
                //Brush brush = (Brush)brushConverter.ConvertFromString("#FEE057");
                //foreach (string child in resultList)
                //{
                //    if (badWordLi.Contains(child))
                //    {
                //        this.textBlockContent.Inlines.Add(new Run(child) { Background = brush });
                //    }
                //    else
                //    {
                //        this.textBlockContent.Inlines.Add(new Run(child));
                //    }
                //}
                StringBuilder sb = new StringBuilder();
                badWordLi.ForEach(x => sb.Append("\"" + x + "\"" + "、"));
                string result = sb.ToString().TrimEnd('、');
                MessageBoxEx mbox = new MessageBoxEx(string.Format("【标题】中包含敏感词{0}，请修改后再试", result), isCancelShow: false);
                mbox.ShowDialog();
                //this.txb_Content.Visibility = Visibility.Visible;
                //this.border_Content.Visibility = Visibility.Collapsed;
                this.txb_Content.Focus();
            }
        }


    }
}
