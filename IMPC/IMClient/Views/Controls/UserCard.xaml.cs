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
using System.Windows.Navigation;
using System.Windows.Shapes;
using IMClient.ViewModels;
using IMClient.Views.ChildWindows;
using IMModels;

namespace IMClient.Views.Controls
{
    /// <summary>
    /// 个人名片
    /// </summary>
    public partial class UserCard : UserControl
    {
        private UserViewModel userModelVM;
        public UserCard()
        {
            InitializeComponent();
            this.Loaded += UserCard_Loaded;
            this.tbNickName.KeyDown += TbNickName_KeyDown;
        }

        private void UserCard_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is UserViewModel userVM)
            {
                userModelVM = userVM;
            }
        }

        private void TbNickName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.cc.Focus();
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            this.cc.Focus();
        }
        private void Head_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AppData.MainMV.IsOpenBusinessCard = false;
            //ChildWindows.ImageScanWindow.ShowScan(this.ibHead.ImageSource);

            if (this.DataContext is UserViewModel userVM)
            {
                ChildWindows.ImageScanWindow.ShowScan((userVM.Model as UserModel).HeadImg);
            }
        }

        private void btnChat_Click(object sender, RoutedEventArgs e)
        {
            AppData.MainMV.IsOpenBusinessCard = false;
        }


        private void txtCard_KeyDown(object sender, KeyEventArgs e)
        {

        }
        string tempTextId = string.Empty;
        private void txtCard_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            string pattern = "[\u4e00-\u9fbb]";
            var isChinese= Regex.IsMatch(textBox.Text, pattern);
            var issymbol = false;
            if (!isChinese)
            {
                var symbolpattern = new Regex("[%--`~!@#$^&*()=|{}':;',\\[\\].<>/?~！@#￥……&*（）——| {}【】‘；：”“'\"。，、？]");
                issymbol=symbolpattern.IsMatch(textBox.Text);
            }
            if ((isChinese || issymbol) && userModelVM != null)
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
            var window = new SetKeFangIDWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            
            window.DataContext = userModelVM;
            window.Owner = App.Current.MainWindow;
            window.ShowDialog();

        }
    }
    /// <summary>
    /// 键盘操作帮助类
    /// </summary>
    public class KeyboardHelper
    {
        /// <summary>
        /// 键盘上的句号键
        /// </summary>
        public const int OemPeriod = 190;

        #region Fileds

        /// <summary>
        /// 控制键
        /// </summary>
        private static readonly List<Key> _controlKeys = new List<Key>
                                                             {
                                                                 Key.Back,
                                                                 Key.CapsLock,
                                                                 //Key.LeftCtrl,
                                                                 //Key.RightCtrl,
                                                                 Key.Down,
                                                                 Key.End,
                                                                 Key.Enter,
                                                                 Key.Escape,
                                                                 Key.Home,
                                                                 Key.Insert,
                                                                 Key.Left,
                                                                 Key.PageDown,
                                                                 Key.PageUp,
                                                                 Key.Right,
                                                                 Key.Decimal,
                                                                 Key.OemPeriod,
                                                                 Key.Add,
                                                                 Key.Subtract,
                                                                 Key.Divide,
                                                                 Key.OemComma,
                                                                 Key.Separator,
                                                                 Key.Subtract,
                                                                 //Key.LeftShift,
                                                                 //Key.RightShift,
                                                                 Key.Tab,
                                                                 Key.Up
                                                             };

        #endregion

        /// <summary>
        /// 是否是数字键
        /// </summary>
        /// <param name="key">按键</param>
        /// <returns></returns>
        public static bool IsDigit(Key key)
        {
            bool shiftKey = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            bool retVal;
            //按住shift键后，数字键并不是数字键
            if (key >= Key.D0 && key <= Key.D9 && !shiftKey)
            {
                retVal = true;
            }
            else
            {
                retVal = key >= Key.NumPad0 && key <= Key.NumPad9;
            }
            return retVal;
        }

        /// <summary>
        /// 是否是控制键
        /// </summary>
        /// <param name="key">按键</param>
        /// <returns></returns>
        public static bool IsControlKeys(Key key)
        {
            return _controlKeys.Contains(key);
        }

        /// <summary>
        /// 是否是小数点
        /// Silverlight中无法识别问号左边的那个小数点键
        /// 只能识别小键盘中的小数点
        /// </summary>
        /// <param name="key">按键</param>
        /// <returns></returns>
        public static bool IsDot(Key key)
        {
            bool shiftKey = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            bool flag = false;
            if (key == Key.Decimal)
            {
                flag = true;
            }
            if (key == Key.OemPeriod && !shiftKey)
            {
                flag = true;
            }
            return flag;
        }

        /// <summary>
        /// 是否是小数点
        /// </summary>
        /// <param name="key">按键</param>
        /// <param name="keyCode">平台相关的按键代码</param>
        /// <returns></returns>
        public static bool IsDot(Key key, int keyCode)
        {

            //return IsDot(key) || (key == Key.Unknown && keyCode == OemPeriod);
            return IsDot(key) || (keyCode == OemPeriod);
        }

        /// <summary>
        /// 是否是字母键
        /// </summary>
        /// <param name="key">按键</param>
        /// <returns></returns>
        public static bool IsCharacter(Key key)
        {
            return key >= Key.A && key <= Key.Z;
        }
    }


}
