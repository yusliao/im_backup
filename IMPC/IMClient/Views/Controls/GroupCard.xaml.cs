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

namespace IMClient.Views.Controls
{
    /// <summary>
    /// 群名片
    /// </summary>
    public partial class GroupCard : UserControl
    {
        private MessageModel messageModel;
        /// <summary>
        /// 群名片
        /// </summary>
        public GroupCard()
        {
            InitializeComponent();

            this.Loaded += GroupCard_Loaded;
        }

        private void GroupCard_Loaded(object sender, RoutedEventArgs e)
        {

            if (this.DataContext is MessageModel)
            {
                messageModel = this.DataContext as MessageModel;
                if (messageModel.IsMine && messageModel.MessageState == MessageStates.Fail)
                {
                    this.bdFail.Visibility = Visibility.Visible;
                    this.tbInfo.Text = "你邀请对方加入群聊";
                    this.Margin = new Thickness(0, 0, -30, 0);
                    Panel.SetZIndex(this, 100);
                    this.bdFail.ToolTip = "名片已失效";
                }
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (this.DataContext is MessageModel msg && msg.Target is IMModels.GroupModel group)
            {
                if (msg.MessageState == MessageStates.Fail)
                {
                    AppData.MainMV.TipMessage = "名片已失效！";
                    return;
                }
                if (ChildWindows.GroupInviteWindow.ShowInstance(group))
                {
                    GroupViewModel groupVM = AppData.MainMV.GroupListVM.Items.ToList().FirstOrDefault(info => info.ID == group.ID);
                    if (groupVM == null) //未加入过的群
                    {
                        groupVM = new GroupViewModel(group, "我加入的群");
                        groupVM.JoinGroupCommand?.Execute(group);
                    }
                    else //我加入过的群
                    {
                        groupVM.JupmToChatCommand?.Execute(group);
                    }
                    //if (!SDKClient.SDKClient.Instance.IsConnected)
                    //{
                    //    IMUI.View.V2.MessageTip.ShowTip("申请入群失败", IMUI.View.V2.TipTypes.Error);
                    //}
                    //else
                    //{
                    //    JoinGroupPackage.Data data = new JoinGroupPackage.Data();
                    //    data.groupId = this.Group.Id;
                    //    data.InviteUserId = InviteUserId;
                    //    data.isAccepted = true;
                    //    data.photo = MainViewModel.Instance.Myself.HeadImgMD5;
                    //    data.remark = string.Format("我是 {0}", MainViewModel.Instance.Myself.Name);
                    //    data.userId = MainViewModel.Instance.Myself.Id;
                    //    data.userName = MainViewModel.Instance.Myself.Name;
                    //    SDKClient.SDKClient.Instance.JoinGroup(data);
                    //}
                }
            }
        }

        private void ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source is MenuItem item && this.DataContext is MessageModel msg)
            {
                var chatVM = ViewModels.AppData.MainMV.ChatListVM.SelectedItem;
                switch (item.Uid)
                {
                    case "DELETE":
                        //没有太好的关联方式，目前暂时做成当前窗口（因为只有当前窗口可能删除对应信息）；                         
                        if (chatVM != null)
                        {
                            chatVM.HideMessageCommand.Execute(msg);
                        }
                        break;
                    case "Forward":
                        FowardImagMsg(chatVM.ID, chatVM.IsGroup,true);
                        break;
                    case "Retract":
                        chatVM.SendWithDrawMsg(msg);
                        break;

                }
            }
        }
        /// <summary>
        /// 转发
        /// </summary>
        /// <param name="isGroup"></param>
        private void FowardImagMsg(int chatID, bool isGroup,bool isInviteJoin)
        {
            AppData.FowardMsg(chatID, messageModel.MsgKey, isGroup, isInviteJoin);
        }
        private void menu_Opened(object sender, RoutedEventArgs e)
        {
            if (messageModel != null)
            {
                if (string.IsNullOrEmpty(messageModel.MsgKey))
                {

                    //this.Retract.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (messageModel.MessageState == MessageStates.Success || messageModel.MessageState == MessageStates.None)
                    {
                        //this.Forward.Visibility = Visibility.Visible;
                    }

                    if ((DateTime.Now - messageModel.SendTime).TotalMinutes >= 2 || !messageModel.IsMine)
                    {
                        //this.Retract.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        //if (messageModel.MessageState == MessageStates.Success || messageModel.MessageState == MessageStates.None)
                        //    this.Retract.Visibility = Visibility.Visible;
                    }
                }

            }
        }
    }
}
