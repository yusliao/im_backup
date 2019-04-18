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
using System.Windows.Controls.Primitives;
using IMClient.Views.ChildWindows;

namespace IMClient.Views.Panels
{
    /// <summary>
    /// 好友信息设置面板
    /// </summary>
    public partial class SetupFriendView : UserControl
    {
        public SetupFriendView()
        {
            InitializeComponent();

            this.Loaded += SetupFriendView_Loaded;
        }

        private void SetupFriendView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(this.DataContext is ChatViewModel))
            {
                return;
            }

            ChatViewModel cvm = this.DataContext as ChatViewModel;
            if (cvm.IsTemporaryChat)
            {
                //临时聊天
                this.gridTop.Visibility = Visibility.Collapsed;
                //this.btnApplyFriend.Visibility = Visibility.Visible;
                this.btnClearMessage.Visibility = Visibility.Collapsed;
                this.pnlDefriend.Visibility = Visibility.Visible;
            }
            else
            {
                if (cvm.ID == -2)
                {
                    //粉丝留言
                    this.gridTop.Visibility = Visibility.Collapsed;
                    //this.btnApplyFriend.Visibility = Visibility.Collapsed;
                    this.btnClearMessage.Visibility = Visibility.Visible;
                    this.pnlDefriend.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.gridTop.Visibility = Visibility.Visible;
                    //this.btnApplyFriend.Visibility = Visibility.Collapsed;
                    this.btnClearMessage.Visibility = Visibility.Collapsed;
                    this.pnlDefriend.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// 创建群聊
        /// </summary> 
        private void elpAdd_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext is ChatViewModel cvm && !cvm.IsGroup)
            {
                UserModel target = AppData.Current.GetUserModel(cvm.ID);

                var datas = AppData.MainMV.FriendListVM.Items.ToList();

                List<UserModel> source = new List<UserModel>();
                foreach (var d in datas)
                {
                    var user = d.Model as UserModel;
                    if (user == null)
                        continue;
                    if (user.ID == AppData.Current.LoginUser.ID)
                        continue;
                    if (user.ID > 0 && user.LinkType == 0)
                    {
                        source.Add(user);
                        user.FirstChar = Helper.CommonHelper.GetFirstChar(user.DisplayName);
                        user.GroupByChar = Helper.CommonHelper.GetFirstChar(user.DisplayName, true);
                    }
                }

                source.Remove(target);
                source = source.OrderBy(info => info.DisplayName).ToList();
                if (target.LinkType == 0)
                {
                    source.Insert(0, target);
                    target.IsLock = true;
                    target.IsSelected = true;
                }

                var selection = ChildWindows.GroupMemberDealWindow.ShowInstance("发起群聊", source);
                if (selection != null)
                {
                    AppData.MainMV.GroupListVM.GroupCreateCommand?.Execute(selection);

                    //int count = selection.Count();
                    ////包括当前用户总共3个人以上才可群聊
                    //if (count == 1)
                    //{
                    //    UserModel user = selection.FirstOrDefault();
                    //    FriendViewModel friednVM = AppData.MainMV.FriendListVM.Items.FirstOrDefault(info => info.ID == user.ID);
                    //    if (friednVM != null)
                    //    {
                    //        friednVM.JupmToChatCommand?.Execute(user);
                    //    }
                    //}
                    //else if (count > 1)
                    //{ 
                    //    AppData.MainMV.GroupListVM.GroupCreateCommand?.Execute(selection);
                    //}
                }
            }
        }

        private void btnApplyFriend_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ChatViewModel cvm)
            {

                ButtonBase btn = sender as ButtonBase;
                UserModel user = (cvm.Model as ChatModel).Chat as UserModel;
                if (user != null)
                {
                    if (AppData.CanInternetAction())
                    {

                        //if (user.LinkType >= 2)
                        //{
                        //    AppData.MainMV.TipMessage = "申请加好友失败，对方已将您加入黑名单！";
                        //    return;
                        //}
                        if (user.LinkDelType > 2)
                        {
                            var obj = SDKClient.SDKClient.Instance.GetUserPrivacySetting(user.ID);
                            if (obj?.data?.item != null && obj.data.item.verifyFriendApply)
                            {
                                //var isFriendApply= VerificationWindow.ShowInstance(user);
                                //var isFriendApply = VerificationWindow.ShowInstance(user);

                                //if (isFriendApply)
                                //{
                                //    user.IsApplyFriend = true;
                                //    btn.IsEnabled = false;
                                //    btn.Content = "已申请";
                                //    AppData.MainMV.TipMessage = "好友申请已发出！";
                                //}
                                //else
                                //{
                                //    //btnApplyFriend.IsChecked = false;
                                //    user.IsApplyFriend = false;
                                //}
                                return;

                            }
                        }

                        string applyReason = string.Format("我是{0}", AppData.Current.LoginUser.User.Name);
                        //SDKClient.SDKClient.Instance.AddFriend(user.ID, applyReason, "");
                        if (!user.IsAttention && user.LinkDelType >= 2)
                            SDKClient.SDKClient.Instance.AddAttention(AppData.Current.LoginUser.User.ID, user.ID);
                        if (!user.IsAttention && user.LinkDelType >= 2)
                            SDKClient.SDKClient.Instance.AddAttention(AppData.Current.LoginUser.User.ID, user.ID);
                        user.IsApplyFriend = false;

                        btn.IsEnabled = false;
                        btn.Content = "已申请";
                        AppData.MainMV.TipMessage = "好友申请已发出！";
                    }

                    else
                    {
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            AppData.MainMV.TipMessage = "网络异常，请检查设置！";
                        }));
                    }

                }
            }

        }

        private async void btnClearMessage_Click(object sender, RoutedEventArgs e)
        {
            if (!Views.MessageBox.ShowDialogBox("确定清空粉丝留言？"))
            {
                return;
            }

            for (int i = 0; i < AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList.Count; i++)
            {
                AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList[i].Chat.Messages.Clear();
                await SDKClient.SDKClient.Instance.DeleteHistoryMsg(AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList[i].Chat.ID, SDKClient.SDKProperty.chatType.chat);
                SDKClient.SDKClient.Instance.UpdateChatRoomVisibility(AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList[i].Chat.ID, 0, false);
            }
            AppData.MainMV.ChatListVM.StrangerMessage.StrangerMessageList.Clear();

            AppData.MainMV.ChatListVM.DeleteChatItem(AppData.MainMV.ChatListVM.StrangerMessage.ID);
            App.Current.MainWindow.Activate();
        }
    }
}
