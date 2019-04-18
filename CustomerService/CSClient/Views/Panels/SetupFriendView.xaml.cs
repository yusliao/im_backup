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
using CSClient.ViewModels;
using IMModels;

namespace CSClient.Views.Panels
{
    /// <summary>
    /// 好友信息设置面板
    /// </summary>
    public partial class SetupFriendView : UserControl
    {
        public SetupFriendView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 创建群聊
        /// </summary> 
        private void elpAdd_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if(this.DataContext is ChatViewModel cvm && !cvm.IsGroup)
            //{
            //    UserModel target = AppData.Current.GetUserModel(cvm.ID);

            //    var datas = AppData.MainMV.FriendListVM.Items.ToList();

            //    List<UserModel> source = new List<UserModel>();
            //    foreach (var d in datas)
            //    {
            //        if (d.Model is UserModel user && user.LinkDelType == 0)
            //        {
            //            source.Add(user);
            //            user.FirstChar = Helper.CommonHelper.GetFirstChar(user.DisplayName);
            //            user.GroupByChar = Helper.CommonHelper.GetFirstChar(user.DisplayName, true);
            //        }
            //    }

            //    source.Remove(target);
            //    source = source.OrderBy(info => info.DisplayName).ToList();
            //    source.Insert(0, target);
            //    target.IsLock = true;
            //    target.IsSelected = true;

            //    var selection = ChildWindows.GroupMemberDealWindow.ShowInstance("发起群聊", source);
            //    if (selection != null)
            //    {
            //        int count = selection.Count();
            //        //包括当前用户总共3个人以上才可群聊
            //        if (count == 1)
            //        {
            //            UserModel user = selection.FirstOrDefault();
            //            FriendViewModel friednVM = AppData.MainMV.FriendListVM.Items.FirstOrDefault(info => info.ID == user.ID);
            //            if (friednVM != null)
            //            {
            //                friednVM.JupmToChatCommand?.Execute(user);
            //            }
            //        }
            //        else if (count > 1)
            //        {
            //            ViewModels.GroupViewModel groupVM = new ViewModels.GroupViewModel(new IMModels.GroupModel(), "我创建的群");
            //            groupVM.GroupCreateCommand?.Execute(selection);
            //        }
            //    }
            //} 
        }
    }
}
