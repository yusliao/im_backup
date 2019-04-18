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

namespace CSClient.Views.ChildWindows
{
    /// <summary>
    /// 修改字符型 窗口
    /// </summary>
    public partial class EditStringValueWindow : Window
    {
        private EditStringValueWindow()
        {
            InitializeComponent();
            this.Top = -100;
            this.Owner = App.Current.MainWindow;
            Topmost = true;
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
                case "Cancel": 
                case "Close":
                    this.DialogResult = false;
                    break;
                case "Sure":
                    this.DialogResult = true;
                    break;
            }
        }

        /// <summary>
        ///启动编辑
        /// </summary>
        /// <param name="value">原来的值</param>
        /// <param name="title">操作标题</param>
        /// <returns></returns>
        public static string ShowInstance(string value,string title)
        {
            EditStringValueWindow win = new EditStringValueWindow();
            win.tbTitle.Text = title;
            win.tbValue.Text = value;

            if(win.ShowDialog() == true)
            {
                return win.tbValue.Text.Trim();
            }
            else
            {
                return value;
            }
        } 
    }
}
