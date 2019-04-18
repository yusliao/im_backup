using IMClient.ViewModels;
using IMClient.Views.ChildWindows;
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
    /// GroupNotice.xaml 的交互逻辑
    /// </summary>
    public partial class GroupNoticeCard : UserControl
    {
        public GroupNoticeCard()
        {
            InitializeComponent();
            this.Loaded += GroupNoticeCard_Loaded;
        }

        private void GroupNoticeCard_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MessageModel msg)
            {
                if (msg.MessageState == MessageStates.Fail)
                {
                    this.Margin = new Thickness(0, 0, -30, 0);
                    Panel.SetZIndex(this, 100);
                }
            }
        }

        protected async override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (this.DataContext is MessageModel msg)
            {
                var chatVM = AppData.MainMV.ChatListVM.SelectedItem;
                var data = await SDKClient.SDKClient.Instance.GetGroupNotice(msg.NoticeModel.NoticeId);
                if (chatVM != null)
                {
                    chatVM.HasNewGroupNotice = false;
                }
                if (data==null)
                {
                    AppData.MainMV.TipMessage = "该群公告已被群主删除！";
                    return;
                }
                else
                {
                    GroupNoticeTipWindow win = new GroupNoticeTipWindow(msg,true);
                    win.Owner = App.Current.MainWindow;
                    win.ShowDialog();
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
                }
            }
        }
    }
}
