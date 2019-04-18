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
    /// PassowrdBox.xaml 的交互逻辑
    /// </summary>
    public partial class PassowrdBox : UserControl
    {
        public PassowrdBox()
        {
            InitializeComponent();

            this.pbBox.IsKeyboardFocusedChanged += delegate
            {
                this.txtUserPwd.Tag = this.pbBox.IsKeyboardFocused ? "" : "密码";
            };
            this.GotFocus += PassowrdBox_GotFocus;
            this.tbtnEye.IsChecked = false;
        }

        private void PassowrdBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.pbBox.Focus();
        }

        public static DependencyProperty PasswordProperty = DependencyProperty.Register("Password",
           typeof(string), typeof(PassowrdBox), new PropertyMetadata(OnPasswordPropertyChanged));

        public static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PassowrdBox pb = d as PassowrdBox;

            string newValue = string.Format("{0}", e.NewValue);

            if (pb.IsEyeVisible)
            {
                if (pb.txtUserPwd.Text != newValue)
                {
                    pb.txtUserPwd.Text = newValue;
                    pb.pbBox.Password = newValue;
                }
            }
            else
            {
                if (pb.pbBox.Password != newValue)
                {
                    pb.txtUserPwd.Text = newValue;
                    pb.pbBox.Password = newValue;
                }
            }
        }

        public static DependencyProperty IsEyeVisibleProperty = DependencyProperty.Register("IsEyeVisible",
          typeof(bool), typeof(PassowrdBox), new PropertyMetadata(true, OnIsEyeVisiblePropertyChanged));

        public static void OnIsEyeVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PassowrdBox pb = d as PassowrdBox;

            bool value = (bool)e.NewValue;
            if (!value)
            {
                pb.tbtnEye.IsChecked = false;
            }
            pb.tbtnEye.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }

        public static DependencyProperty MaxLengthProperty = DependencyProperty.Register("MaxLength",
         typeof(int), typeof(PassowrdBox), new PropertyMetadata(OnMaxLengthPropertyChanged));

        public static void OnMaxLengthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PassowrdBox pb = d as PassowrdBox;
            pb.txtUserPwd.MaxLength = pb.pbBox.MaxLength = (int)e.NewValue;
        }

        /// <summary>
        ///  
        /// </summary>
        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }
        public bool IsEyeVisible
        {
            get { return (bool)GetValue(IsEyeVisibleProperty); }
            set { SetValue(IsEyeVisibleProperty, value); }
        }


        private void tbtnEye_Click(object sender, RoutedEventArgs e)
        {
            if (this.tbtnEye.IsChecked == true)
            {
                this.txtUserPwd.Foreground = this.tbtnEye.Foreground;
                this.pbBox.Foreground = null;
                this.txtUserPwd.Text = this.Password;
                this.pbBox.Visibility = Visibility.Collapsed;
                this.txtUserPwd.IsHitTestVisible = true;
                this.txtUserPwd.Focus();
                this.txtUserPwd.CaretIndex = this.txtUserPwd.Text.Length;
            }
            else
            {
                this.pbBox.Foreground = this.tbtnEye.Foreground;

                this.txtUserPwd.Foreground = null;

                this.pbBox.Password = this.Password;
                this.pbBox.Visibility = Visibility.Visible;
                this.txtUserPwd.IsHitTestVisible = false;

                this.pbBox.Focus();
                this.pbBox.SelectAll();
            }
        }

        private void pb_PasswordChanged(object sender, RoutedEventArgs e)
        {
            //string value = "";
            //value = value.PadRight(this.pbBox.Password.Length, this.pbBox.PasswordChar);

            //this.txtUserPwd.Text =  
            //this.Password = this.pbBox.Password;
            this.txtUserPwd.Text = this.pbBox.Password;
        }

        private void txtUserPwd_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.Password = this.txtUserPwd.Text;
        }

        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
