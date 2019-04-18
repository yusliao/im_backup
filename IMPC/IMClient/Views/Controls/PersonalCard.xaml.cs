using IMClient.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IMClient.Views.Controls
{
    /// <summary>
    /// PersonalCard.xaml 的交互逻辑
    /// </summary>
    public partial class PersonalCard : UserControl
    {
        MenuItem _miWithDraw;
        MessageModel messageModel;
        public PersonalCard()
        {
            InitializeComponent();
            this.Loaded += PersonalCard_Loaded;
            _miWithDraw = new MenuItem();
            _miWithDraw.Header = "撤回";
            _miWithDraw.Uid = "WITHDRAWimg";
            //this.btn_Check.LostFocus += Btn_Check_LostFocus;
        }
        private bool _hasContextMenu = true;

        public bool HasContexMenu
        {
            get { return _hasContextMenu; }
            set
            {
                _hasContextMenu = value;
                this.bdLayout.ContextMenu = value ? this.menu : null;
            }
        }
        private void Btn_Check_LostFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void PersonalCard_Loaded(object sender, RoutedEventArgs e)
        {
            messageModel = this.DataContext as MessageModel;
        }

        private void ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source is MenuItem item && this.DataContext is MessageModel msg)
            {
                var chatVM = AppData.MainMV.ChatListVM.SelectedItem;
                switch (item.Uid)
                {
                    case "DELETE":
                        //没有太好的关联方式，目前暂时做成当前窗口（因为只有当前窗口可能删除对应信息）；                         
                        if (chatVM != null)
                        {
                            chatVM.HideMessageCommand.Execute(msg);
                        }
                        break;
                    case "Forward"://转发
                        FowardImagMsg(chatVM.ID, chatVM.IsGroup);
                        break;
                    case "WITHDRAWimg"://撤回
                        if (chatVM != null)
                        {
                            chatVM.SendWithDrawMsg(msg);
                        }
                        break;
                    case "Open"://打开
                        ShowUserCard();
                        break;
                }
            }
        }


        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            ShowUserCard();
        }
        private void Ellipse_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //AppData.MainMV.IsOpenBusinessCard = false;
            //if (messageModel != null && messageModel.PersonCardModel != null)
            //{
            //    if (!string.IsNullOrEmpty(messageModel.PersonCardModel.PhotoImg))
            //    {
            //        ChildWindows.ImageScanWindow.ShowScan(messageModel.PersonCardModel.PhotoImg);
            //    }
            //}
        }
        /// <summary>
        /// 转发
        /// </summary>
        /// <param name="isGroup"></param>
        private void FowardImagMsg(int chatID, bool isGroup)
        {
            AppData.FowardMsg(chatID, messageModel.MsgKey, isGroup);
        }
        private void btn_Check_Click(object sender, RoutedEventArgs e)
        {
            //ShowUserCard();
            btn_Check.Focusable = false;
        }
        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
           // ShowUserCard();
            //e.Handled = false;
        }
        private void ShowUserCard()
        {
            MessageModel msg = this.DataContext as MessageModel;
            int userId = msg.PersonCardModel.UserId;
            SDKClient.SDKClient.Instance.GetUser(msg.PersonCardModel.UserId);
            UserModel user = AppData.Current.GetUserModel(userId);
            AppData.MainMV.ShowUserBusinessCard(user,true,ApplyFriendSource.FriendRecommend);
        }

        private void menu_Opened(object sender, RoutedEventArgs e)
        {
            if (e.Source is ContextMenu item && item.DataContext is MessageModel msg)
            {

                if (string.IsNullOrEmpty(msg.MsgKey))
                {

                    if (this.menu.Items.Contains(this._miWithDraw))
                    {
                        this.menu.Items.Remove(this._miWithDraw);
                    }
                }
                else
                {
                    if (msg.MessageState == MessageStates.Success || msg.MessageState == MessageStates.None)
                    {
                        this.Forward.Visibility = Visibility.Visible;
                    }
                    if ((DateTime.Now - msg.SendTime).TotalMinutes >= 2 || !msg.IsMine)
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
                            if (msg.MessageState == MessageStates.Success || msg.MessageState == MessageStates.None)
                                this.menu.Items.Add(this._miWithDraw);
                        }
                    }
                }
            }
        }
    }
}
