using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using IMClient.ViewModels;
using IMModels;

namespace IMClient.Views.ChildWindows
{
    /// <summary>
    /// 群邀请 的交互逻辑
    /// </summary>
    public partial class GroupInviteWindow : Window
    { 
        private GroupInviteWindow()
        {
            InitializeComponent();
            this.Top = -100;
            this.Owner = App.Current.MainWindow;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ButtonBase btn = e.Source as Button;
            switch (btn.Uid)
            {
                default:
                case "CANCEL":
                    this.DialogResult = false;
                    break;
                case "SURE":
                    this.DialogResult = true;
                    break;
                case "CHAT":
                    this.DialogResult = true;
                    break;
            }
        }

        /// <summary>
        /// 启动
        /// </summary> 
        /// <returns></returns>
        public static bool ShowInstance(GroupModel group)
        {
            GroupInviteWindow win = new GroupInviteWindow();
            win.DataContext = group;

            if (AppData.MainMV.GroupListVM.Items.ToList().Any(info=>info.ID==group.ID))
            {
                win.btnCancel.Visibility = win.btnSure.Visibility = Visibility.Collapsed;
                win.btnChat.Visibility = Visibility.Visible;
            }

            if (win.ShowDialog() == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
      
    }
}
