using IMClient.ViewModels;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IMClient
{
    /// <summary>
    /// LoginQR.xaml 的交互逻辑
    /// </summary>
    public partial class LoginQR : UserControl, IView
    {
        LoginQRViewModel _arVM;
        
        public LoginQR()
        {
            InitializeComponent();
            
            //this.Visibility = Visibility.Collapsed; 

            this.IsVisibleChanged += LoginQR_IsVisibleChanged;
        
            this.DataContext = ViewModel = _arVM = new LoginQRViewModel(this);
            this.Loaded += LoginQR_Loaded;
            _arVM.OnLoginSuccess += LoginViewModel_OnLoginSuccess;

            //this.tbtnActive.Checked += TbtnActive_CheckedOrNot;
            //this.tbtnActive.Unchecked += TbtnActive_CheckedOrNot;
        }

        private void LoginQR_Loaded(object sender, RoutedEventArgs e)
        {
            (this.Resources["Loading"] as Storyboard).Begin();
            _arVM.TryConnect();
        }

        private void TbtnActive_CheckedOrNot(object sender, RoutedEventArgs e)
        {
            //this.IsActive = this.tbtnActive.IsChecked.Value ;
        }

        private void LoginQR_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if (this.IsVisible)
            //{
            //    (this.Resources["Loading"] as Storyboard).Begin();
            //    _arVM.TryConnect();
            //}
        }

        private void LoginViewModel_OnLoginSuccess(IMModels.UserModel user)
        {
            if (this.RefVM is LoginViewModel loginVM)
            {
                loginVM.OnLoginSuccess(new IMModels.LoginUser(user));
            }
        }


        public ViewModel ViewModel { get; private set; }



        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }


        // Using a DependencyProperty as the backing store for IsBackToView.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(LoginQR), new PropertyMetadata(OnIsActivePropertyChanged));


        public static void OnIsActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LoginQR qr = d as LoginQR;
            qr.Visibility = qr.IsActive ? Visibility.Visible : Visibility.Collapsed;
        }



        public LoginViewModel RefVM { get; set; }

        private void tbtnReConnect_Click(object sender, RoutedEventArgs e)
        {
            Storyboard story = this.Resources["Loading"] as Storyboard;
            
            story.Begin();
            story.Seek(TimeSpan.FromSeconds(0.5));
            _arVM?.TryConnect();
        }
    }
}
