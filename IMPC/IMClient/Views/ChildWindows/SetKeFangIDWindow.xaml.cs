using IMClient.ViewModels;
using IMModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// SetKeFangIDWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SetKeFangIDWindow : Window
    {
        UserViewModel userViewModel;
        public SetKeFangIDWindow()
        {
            InitializeComponent();
            this.Loaded += SetKeFangIDWindow_Loaded;
        }

        private void SetKeFangIDWindow_Loaded(object sender, RoutedEventArgs e)
        {
            userViewModel = this.DataContext as UserViewModel;
            // btnSure.IsEnabled =true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        string tempTextId = string.Empty;
        private void txtCard_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            string pattern = "[\u4e00-\u9fbb]";
            var isChinese = Regex.IsMatch(textBox.Text, pattern);
            var issymbol = false;
            if (!isChinese)
            {
                string pattern1 = @"^[0-9A-Za-z]+$";
                var symbolpattern = new Regex("[%--`~!@#$^&*()=|{}':;',\\[\\].<>/?~！@#￥……&*）——| {}【】‘；：”“'\"。，、？\\\\]");
                //Regex symbolpattern = new Regex(pattern1);
               issymbol = symbolpattern.IsMatch(textBox.Text);
            }
            if ((isChinese || issymbol) && userViewModel != null)
            {
                textBox.Text = tempTextId;
                textBox.SelectionStart = textBox.Text.Length;
            }
            else
            {
                tempTextId = textBox.Text;
            }
        }

        private void txtCard_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.txtCard.Text = string.Empty;
        }

        private void btnSure_Click(object sender, RoutedEventArgs e)
        {
            if (userViewModel != null && (userViewModel.IsSureUpdate || (userViewModel.Model is UserModel userModel && userModel.HaveModifiedKfid == 1)))
            {
                this.Close();
            }
        }
    }
}

