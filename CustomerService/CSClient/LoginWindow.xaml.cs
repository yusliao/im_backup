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
using System.Windows.Shapes;
using CSClient.ViewModels;

namespace CSClient
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window,IView
    {
       
        public LoginWindow()
        {
            
            InitializeComponent();
            UItaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.MouseLeftButtonDown += LoginWindow_MouseLeftButtonDown; 
            this.DataContext = ViewModel= new LoginViewModel(this); 
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
                    this.gridHisList.Margin = new Thickness(0, 0, -10, 0);
                    this.btnLoginUser.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    this.btnLoginUser.FlowDirection = FlowDirection.RightToLeft;
                    this.gridHisList.Margin = new Thickness(-10, 0, 0, 0);
                }
            }
            catch
            {

            }
           
        }

        public ViewModel ViewModel { get; private set; }

        public TaskScheduler UItaskScheduler { get; private set; }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            (App.Current as App).ApplicationExit(null, null);
            App.Current.Shutdown(); 
        }

       
    }
}
