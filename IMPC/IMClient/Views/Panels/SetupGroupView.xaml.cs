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
using System.Windows.Navigation;
using System.Windows.Shapes;
using IMClient.ViewModels;
using IMModels;

namespace IMClient.Views.Panels
{
    /// <summary>
    /// 组信息设置面板
    /// </summary>
    public partial class SetupGroupView : UserControl
    {
        public SetupGroupView()
        {
            InitializeComponent();
            this.tbDisplayName.KeyDown += TbDisplayName_KeyDown;

            this.tbRemark.KeyDown += TbRemark_KeyDown;
            this.tbMyNickName.KeyDown += TbMyNickName_KeyDown;
            
            this.Unloaded += SetupGroupView_Unloaded;
            this.Loaded += SetupGroupView_Loaded;
        }

        private void SetupGroupView_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ChatViewModel cvm && cvm.TargetVM is GroupViewModel gvm)
            {
                this.tbDisplayName.IsHitTestVisible = this.tbRemark.IsHitTestVisible
                    = gvm.IsAdmin || gvm.IsCreator;
                gvm.InitializeData();
            }
        }

        private void SetupGroupView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ChatViewModel cvm && cvm.TargetVM is GroupViewModel gvm)
            {
                gvm.ShowMoreMembers = false;
            }
        }

        private void TbDisplayName_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.DataContext is ChatViewModel cvm && e.Key == Key.Enter && cvm != null && cvm.TargetVM is GroupViewModel gvm)
            {
                gvm.ChangedGroupNameCommand.Execute(this.tbDisplayName.Text);
                this.btnTemp.Focus();
            }
        }
        private void TbRemark_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.DataContext is ChatViewModel cvm && e.Key == Key.Enter && cvm != null && cvm.TargetVM is GroupViewModel gvm)
            {                
                gvm.ChangedGroupRemarkCommand.Execute(this.tbRemark.Text);
                this.btnTemp.Focus();
            }
        }

        private void TbMyNickName_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.DataContext is ChatViewModel cvm && e.Key == Key.Enter && cvm != null && cvm.TargetVM is GroupViewModel gvm)
            {
                gvm.ChangedMyNickNameInGroupCommand.Execute(this.tbMyNickName.Text);
                this.btnTemp.Focus();
            }
        }

        private void gridAdd_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext is ChatViewModel cvm && cvm.TargetVM is GroupViewModel gvm
                && gvm.Model is GroupModel group)
            {
                var datas = AppData.MainMV.FriendListVM.Items.ToList();

                List<UserModel> source = new List<UserModel>();

                foreach (var d in datas)
                {
                    var user = d.Model as UserModel;
                    if (user == null)
                        continue;
                    if (user.ID == AppData.Current.LoginUser.ID)
                        continue;
                    if (user.LinkType == 1 || user.LinkType == 3)
                        continue;
                    if (user.ID > 0)
                    {
                        source.Add(user);
                        user.IsLock = user.GetInGroupMember(group, false) != null;

                        if (user.IsLock)
                        {
                            var target = group.Members.FirstOrDefault(info => info.ID == user.ID);
                            if (target == null)
                            {
                                user.IsLock = false;
                                user.RemoveFromGroup(group);
                            }
                        }

                        user.FirstChar = Helper.CommonHelper.GetFirstChar(user.DisplayName);
                        user.GroupByChar = Helper.CommonHelper.GetFirstChar(user.DisplayName, true);
                    }
                }

                var selection = ChildWindows.GroupMemberDealWindow.ShowInstance("邀请加入群聊", source);

                if (selection != null)
                {
                    var reuslts = selection.Where(info => !info.IsLock);
                    if (reuslts.Count() > 0)
                    {
                        gvm.GroupInviteMembersCommand?.Execute(reuslts);
                    }
                }
            }
        }

        private void gridRemove_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext is ChatViewModel cvm && cvm.TargetVM is GroupViewModel gvm
                   && gvm.Model is GroupModel group)
            {
                List<UserModel> source = new List<UserModel>();
                foreach (var m in group.Members)
                {
                    if (m.IsCreator || m.ID == AppData.Current.LoginUser.ID)
                    {
                        continue;
                    }
                    if (gvm.IsAdmin && m.IsManager)
                    {
                        continue;
                    }

                    UserModel user = m.TargetUser;
                    user.FirstChar = Helper.CommonHelper.GetFirstChar(user.DisplayName);
                    user.GroupByChar = Helper.CommonHelper.GetFirstChar(user.DisplayName, true);
                    source.Add(user);
                }

                //source.Remove(AppData.Current.LoginUser.User);

                var selection = ChildWindows.GroupMemberDealWindow.ShowInstance("删除群成员", source);

                if (selection != null)
                {
                    var reuslts = selection.Where(info => !info.IsLock);
                    if (reuslts.Count() > 0)
                    {
                        gvm.GroupRemoveMembersCommand?.Execute(reuslts);
                    }
                }
            }
        }
        /// <summary>
        /// 转让群
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridTransfer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext is ChatViewModel cvm && cvm.TargetVM is GroupViewModel gvm
               && gvm.Model is GroupModel group)
            {
                List<UserModel> userSource = new List<UserModel>();
                group.Members.ToList().ForEach(x => userSource.Add(x.TargetUser));
                var datas = AppData.MainMV.FriendListVM.Items.ToList();

                List<UserModel> source = new List<UserModel>();

                foreach (UserModel user in userSource)
                {
                    if (user.ID > 0)
                    {
                        source.Add(user);
                        user.IsLock = user.GetInGroupMember(group, false) != null;

                        if (user.IsLock)
                        {
                            var target = group.Members.FirstOrDefault(info => info.ID == user.ID);
                            if (target == null)
                            {
                                user.IsLock = false;
                                user.RemoveFromGroup(group);
                            }
                        }

                        user.FirstChar = Helper.CommonHelper.GetFirstChar(user.DisplayName);
                        user.GroupByChar = Helper.CommonHelper.GetFirstChar(user.DisplayName, true);
                    }
                }

                var selection = ChildWindows.GroupMemberDealWindow.ShowInstance("邀请加入群聊", source);

                if (selection != null)
                {
                    var reuslts = selection.Where(info => !info.IsLock);
                    if (reuslts.Count() > 0)
                    {
                        gvm.GroupInviteMembersCommand?.Execute(reuslts);
                    }
                }
            }
        }

       



        //private void richBox_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        //{
        //    ChatViewModel chatView = this.DataContext as ChatViewModel;
        //    GroupModel group = (chatView.TargetVM as GroupViewModel).Model as GroupModel;
        //    RichTextBox richBox = this.richBox;
        //    TextRange range = new TextRange(richBox.Document.ContentStart, richBox.Document.ContentEnd);
        //    string value = string.Format("{0}", range.Text).Trim();
        //    int index = 0;
        //    bool isFirstBadWord = SDKClient.SDKProperty.stringSearchEx.FindFirst(value, out index);
        //    bool isContainsBadWord = SDKClient.SDKProperty.stringSearchEx.ContainsAny(value);
        //    List<string> badWordLi = SDKClient.SDKProperty.stringSearchEx.FindAll(value);
        //    List<string> goodWordLi = value.Split(badWordLi.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
        //    List<string> badWordLiOperate = new List<string>();
        //    StringBuilder stringBuilder = new StringBuilder(value);
        //    foreach (string child in badWordLi)
        //    {
        //        badWordLiOperate.Add("|" + child + "|");
        //    }
        //    foreach (string child1 in badWordLi)
        //    {
        //        foreach (string child2 in badWordLiOperate)
        //        {
        //            if (child1.Equals(child2.Replace("|", string.Empty)))
        //                stringBuilder.Replace(child1, child2);
        //        }
        //    }
        //    List<string> resultList = new List<string>(stringBuilder.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
        //    if (isFirstBadWord || isContainsBadWord)
        //    {
        //        //txbBox.Visibility = System.Windows.Visibility.Collapsed;
        //        //textBlock.Visibility = Visibility.Visible;
        //        //textBlock.Inlines.Clear();

        //        BrushConverter brushConverter = new BrushConverter();
        //        Brush brush = (Brush)brushConverter.ConvertFromString("#FEE057");
        //        richBox.Document.Blocks.Clear();
        //        Paragraph pa = new Paragraph();
        //        foreach (string child in resultList)
        //        {
        //            if (badWordLi.Contains(child))
        //            {
        //                pa.Inlines.Add(new Run(child) { Background = brush });
        //                richBox.Document.Blocks.Add(pa);
        //            }
        //            else
        //            {
        //                pa.Inlines.Add(new Run(child));
        //                richBox.Document.Blocks.Add(pa);
        //                //textBlock.Inlines.Add(new Run(child));
        //            }
        //        }
        //        if (IMClient.Views.MessageBox.ShowDialogBox("【群介绍】中包含敏感词，请修改后再试", isCancelShow: false))
        //        {
        //            this.richBox.Focus();
        //        }
        //    }
        //    else
        //    {
        //        if (group != null && value != string.Format("{0}", group.GroupRemark))
        //        {
        //            if (AppData.CanInternetAction())
        //            {
        //                group.GroupRemark = value;
        //                SDKClient.SDKClient.Instance.UpdateGroup(group.ID, SDKClient.Model.SetGroupOption.修改群简介, group.GroupRemark);
        //            }
        //            else //网络已断开
        //            {
        //                group.GroupRemark = group.GroupRemark;
        //            }
        //        }
        //    }
        //}

        //private void richBox_KeyDown(object sender, KeyEventArgs e)
        //{
        //    ChatViewModel chatView = this.DataContext as ChatViewModel;
        //    GroupModel group = (chatView.TargetVM as GroupViewModel).Model as GroupModel;
        //    RichTextBox richBox = this.richBox;
        //    TextRange range = new TextRange(richBox.Document.ContentStart, richBox.Document.ContentEnd);
        //    string value = string.Format("{0}", range.Text).Trim();
        //    int index = 0;
        //    bool isFirstBadWord = SDKClient.SDKProperty.stringSearchEx.FindFirst(value, out index);
        //    bool isContainsBadWord = SDKClient.SDKProperty.stringSearchEx.ContainsAny(value);
        //    List<string> badWordLi = SDKClient.SDKProperty.stringSearchEx.FindAll(value);
        //    List<string> goodWordLi = value.Split(badWordLi.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
        //    List<string> badWordLiOperate = new List<string>();
        //    StringBuilder stringBuilder = new StringBuilder(value);
        //    foreach (string child in badWordLi)
        //    {
        //        badWordLiOperate.Add("|" + child + "|");
        //    }
        //    foreach (string child1 in badWordLi)
        //    {
        //        foreach (string child2 in badWordLiOperate)
        //        {
        //            if (child1.Equals(child2.Replace("|", string.Empty)))
        //                stringBuilder.Replace(child1, child2);
        //        }
        //    }
        //    List<string> resultList = new List<string>(stringBuilder.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
        //    if (isFirstBadWord || isContainsBadWord)
        //    {
        //        //txbBox.Visibility = System.Windows.Visibility.Collapsed;
        //        //textBlock.Visibility = Visibility.Visible;
        //        //textBlock.Inlines.Clear();

        //        BrushConverter brushConverter = new BrushConverter();
        //        Brush brush = (Brush)brushConverter.ConvertFromString("#FEE057");
        //        richBox.Document.Blocks.Clear();
        //        Paragraph pa = new Paragraph();
        //        foreach (string child in resultList)
        //        {
        //            if (badWordLi.Contains(child))
        //            {
        //                pa.Inlines.Add(new Run(child) { Background = brush });
        //                richBox.Document.Blocks.Add(pa);
        //            }
        //            else
        //            {
        //                pa.Inlines.Add(new Run(child));
        //                richBox.Document.Blocks.Add(pa);
        //                //textBlock.Inlines.Add(new Run(child));
        //            }
        //        }
        //        if (IMClient.Views.MessageBox.ShowDialogBox("【群介绍】中包含敏感词，请修改后再试", isCancelShow: false))
        //        {
        //            this.richBox.Focus();
        //        }
        //    }
        //    else
        //    {
        //        if (group != null && value != string.Format("{0}", group.GroupRemark))
        //        {
        //            if (AppData.CanInternetAction())
        //            {
        //                group.GroupRemark = value;
        //                SDKClient.SDKClient.Instance.UpdateGroup(group.ID, SDKClient.Model.SetGroupOption.修改群简介, group.GroupRemark);
        //            }
        //            else //网络已断开
        //            {
        //                group.GroupRemark = group.GroupRemark;
        //            }
        //        }
        //    }
        //}


        //private void richBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        //{
        //    ChatViewModel chatView = this.DataContext as ChatViewModel;
        //    GroupModel group = (chatView.TargetVM as GroupViewModel).Model as GroupModel;
        //    RichTextBox richBox = this.richBox;
        //    TextRange range = new TextRange(richBox.Document.ContentStart, richBox.Document.ContentEnd);
        //    string value = string.Format("{0}", range.Text).Trim();
        //    int index = 0;
        //    bool isFirstBadWord = SDKClient.SDKProperty.stringSearchEx.FindFirst(value, out index);
        //    bool isContainsBadWord = SDKClient.SDKProperty.stringSearchEx.ContainsAny(value);
        //    List<string> badWordLi = SDKClient.SDKProperty.stringSearchEx.FindAll(value);
        //    List<string> goodWordLi = value.Split(badWordLi.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
        //    List<string> badWordLiOperate = new List<string>();
        //    StringBuilder stringBuilder = new StringBuilder(value);
        //    foreach (string child in badWordLi)
        //    {
        //        badWordLiOperate.Add("|" + child + "|");
        //    }
        //    foreach (string child1 in badWordLi)
        //    {
        //        foreach (string child2 in badWordLiOperate)
        //        {
        //            if (child1.Equals(child2.Replace("|", string.Empty)))
        //                stringBuilder.Replace(child1, child2);
        //        }
        //    }
        //    List<string> resultList = new List<string>(stringBuilder.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
        //    if (isFirstBadWord || isContainsBadWord)
        //    {
        //        //txbBox.Visibility = System.Windows.Visibility.Collapsed;
        //        //textBlock.Visibility = Visibility.Visible;
        //        //textBlock.Inlines.Clear();

        //        BrushConverter brushConverter = new BrushConverter();
        //        Brush brush = (Brush)brushConverter.ConvertFromString("#FEE057");
        //        richBox.Document.Blocks.Clear();
        //        Paragraph pa = new Paragraph();
        //        foreach (string child in resultList)
        //        {
        //            if (badWordLi.Contains(child))
        //            {
        //                pa.Inlines.Add(new Run(child) { Background = brush });
        //                richBox.Document.Blocks.Add(pa);
        //            }
        //            else
        //            {
        //                pa.Inlines.Add(new Run(child));
        //                richBox.Document.Blocks.Add(pa);
        //                //textBlock.Inlines.Add(new Run(child));
        //            }
        //        }
        //        if (IMClient.Views.MessageBox.ShowDialogBox("【群介绍】中包含敏感词，请修改后再试", isCancelShow: false))
        //        {
                    
        //        }
        //    }
        //    else
        //    {
        //        if (group != null && value != string.Format("{0}", group.GroupRemark))
        //        {
        //            if (AppData.CanInternetAction())
        //            {
        //                group.GroupRemark = value;
        //                SDKClient.SDKClient.Instance.UpdateGroup(group.ID, SDKClient.Model.SetGroupOption.修改群简介, group.GroupRemark);
        //            }
        //            else //网络已断开
        //            {
        //                group.GroupRemark = group.GroupRemark;
        //            }
        //        }
        //    }
        //}
    }
}
