using IMClient.Helper;
using IMClient.ViewModels;
using IMModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// HyperlinkMessageCard.xaml 的交互逻辑
    /// </summary>
    public partial class HyperlinkMessageCard : UserControl
    {
        MenuItem _miWithDraw;
        MessageModel messageModel;
        public HyperlinkMessageCard()
        {
            InitializeComponent();
            _miWithDraw = new MenuItem();
            _miWithDraw.Header = "撤回";
            _miWithDraw.Uid = "WITHDRAWimg";
            this.Loaded += HyperlinkMessageCard_Loaded;
        }

        private void HyperlinkMessageCard_Loaded(object sender, RoutedEventArgs e)
        {
            messageModel = this.DataContext as MessageModel;
            if (messageModel != null && !string.IsNullOrEmpty(messageModel.ShareMsgImage))
            {
                if (messageModel.ResourceModel != null && !string.IsNullOrEmpty(messageModel.ResourceModel.SmallKey))
                {
                    IMClient.Helper.MessageHelper.LoadImgContent(messageModel, true, true);
                }
            }

        }

        private void Ellipse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //var messageModel = this.DataContext as MessageModel;
            //if (messageModel != null && !string.IsNullOrEmpty(messageModel.MsgHyperlink))
            //{
            //    Process.Start(new ProcessStartInfo(messageModel.MsgHyperlink));
            //}
            OpenUrl();
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
                    case "CopyUrl":
                        CopyUrl();
                        break;
                    case "Open"://打开
                        OpenUrl();
                        break;
                }
            }
        }

        private void OpenUrl()
        {
            if (messageModel != null && !string.IsNullOrEmpty(messageModel.MsgHyperlink))
            {
                Process.Start(new ProcessStartInfo(messageModel.MsgHyperlink));
            }
        }
        private void CopyUrl()
        {
            this.menu.IsOpen = false;

            string value = messageModel.MsgHyperlink; ;
            if (!string.IsNullOrEmpty(value))
                //Task.Delay(200).ContinueWith(t =>
                //{
                Clipboard.SetDataObject(value);

            //});
        }

        /// <summary>
        /// 转发
        /// </summary>
        /// <param name="isGroup"></param>
        private void FowardImagMsg(int chatID, bool isGroup)
        {
            AppData.FowardMsg(chatID, messageModel.MsgKey, isGroup);
        }
    }
}
