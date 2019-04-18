using IMClient.ViewModels;
using IMModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// SetFriendNotesWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SetFriendNotesWindow : Window
    {
        private string SetNoteingIcon = "/IMAssets;component/Images/setNoteing.png";
        private string SetcompletedIcon = "/IMAssets;component/Images/setcompleted.png";
        public SetFriendNotesWindow()
        {
            InitializeComponent();
        }
        private bool dialogResult = true;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.dialogResult = false;
            this.Close();
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
        public static bool ShowInstance(UserViewModel userModel,bool isFriendAcceptedExpired=false)
        {
            SetFriendNotesWindow win = new SetFriendNotesWindow();
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            //win.Topmost = true;
            win.Owner = App.Current.MainWindow;
            win.DataContext = new SetVerificationViewModel(userModel, isFriendAcceptedExpired);
            win.ShowDialog();
            return win.dialogResult;
        }

        private void btnSure_Click(object sender, RoutedEventArgs e)
        {
            if (AppData.CanInternetAction(""))
            {
                Thread.Sleep(200);
                this.dialogResult = true;
                this.Close();
            }
        }
    }
}
