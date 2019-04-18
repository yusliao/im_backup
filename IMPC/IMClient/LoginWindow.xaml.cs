using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace IMClient
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window, IView
    {

        public LoginWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += LoginWindow_MouseLeftButtonDown;
            this.DataContext = ViewModel = new LoginViewModel(this);

            //this.pbBox.txtUserPwd.TextChanged += LoginInfo_Changed;
            //this.tbAccount.TextChanged += LoginInfo_Changed;
            //this.tbtnQR.Checked += TbtnQR_Checked;
            //this.tbtnQR.Unchecked += TbtnQR_Checked;
            this.Loaded += LoginWindow_Loaded;

        }

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {

            if (this.DataContext is LoginViewModel loginVM)
            {
                loginVM.LoadHistoryUsers();
                loginVM.SetStackPanelVisibility += LoginVM_SetStackPanelVisibility;
            }
        }

        private void LoginVM_SetStackPanelVisibility()
        {
            LoginViewModel logVM = this.DataContext as LoginViewModel;
            int hisCount = logVM.HistoryUsers.Count;
            if (hisCount > 0)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    this.sp_Error.Visibility = Visibility.Collapsed;
                });
            }
            else
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    this.sp_Error.Visibility = Visibility.Visible;
                });
            }

        }

        private void TbtnQR_Checked(object sender, RoutedEventArgs e)
        {
            //if (this.tbtnQR.IsChecked == true)
            //{
            //    this.qrPanel.RefVM.SetState(false);
            //}
            //else
            //{
            //    this.qrPanel.RefVM.SetState(true);
            //}
        }

        private void LoginInfo_Changed(object sender, TextChangedEventArgs e)
        {
            //this.btnLogin.IsEnabled = !string.IsNullOrEmpty(this.pbBox.Password) && !string.IsNullOrEmpty(this.tbAccount.Text);
        }


        private void LoginWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                }
                this.DragMove();

                if (this.Left > 210)
                {
                    //this.gridHisList.Margin = new Thickness(0, 0, -10, 0);
                    //this.btnLoginUser.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    //this.btnLoginUser.FlowDirection = FlowDirection.RightToLeft;
                    //this.gridHisList.Margin = new Thickness(-10, 0, 0, 0);
                }
            }
            catch
            {

            }

        }

        public ViewModel ViewModel { get; private set; }


        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            (App.Current as App).ApplicationExit(null, null);
            App.Current.Shutdown();
        }

        private void ToggleButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //controlQR.Content = new LoginQR();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var tgBtu = sender as ToggleButton;
            if (!tgBtu.IsChecked.HasValue)
            {
                controlQR.Content = new LoginQR();
            }
        }
    }
}
