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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace IMClient.Views.ChildWindows
{
    /// <summary>
    /// 修改字符型 窗口
    /// </summary>
    public partial class EditStringValueWindow : Window
    {
        public  event Action<string> GroupNameEvent;
        bool isSure = false;
        public static EditStringValueWindow Win;
        static string oldGroupName = string.Empty;
        private EditStringValueWindow()
        {
            InitializeComponent();
            this.Top = -100;
            this.Owner = App.Current.MainWindow;
            //Topmost = true;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //ButtonBase btn = e.Source as Button;
            //switch (btn.Uid)
            //{
            //    default:
            //    case "Cancel":
            //    case "Close":
            //        this.DialogResult = false;
            //        break;
            //    case "Sure":
            //        this.DialogResult = true;
            //        break;
            //}
        }

        /// <summary>
        ///启动编辑
        /// </summary>
        /// <param name="value">原来的值</param>
        /// <param name="title">操作标题</param>
        /// <returns></returns>
        public static string ShowInstance(string value, string title, int maxLength = 15)
        {
            Win = new EditStringValueWindow();
            Win.tbTitle.Text = title;
            Win.tbValue.Text = value;
            Win.tbValue.MaxLength = maxLength;
            oldGroupName = value;
            Win.Owner = Application.Current.MainWindow;
            Win.Show();
            return Win.tbValue.Text.Trim();
            //if (win.ShowDialog() == true)
            //{
            //    return win.tbValue.Text.Trim();
            //}
            //else
            //{
            //    return value;
            //}
        }
        protected override void OnClosed(EventArgs e)
        {
            //ObservableCollection<object> o = new ObservableCollection<object>();
            //o.Add(this.tbValue);
            //o.Add(this.bord_Tip);
            if (isSure && Win != null)
            {
                GroupNameEvent?.Invoke(this.tbValue.Text);
            }
            else
            {
                this.Close();
            }
            base.OnClosed(e);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

            isSure = false;
            this.Close();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            isSure = false;
            this.Close();
        }

        private void btnSure_Click(object sender, RoutedEventArgs e)
        {
            TextBox txbBox = this.tbValue;
            //TextBlock textBlock = this.txbGroupName;

            string value = string.Format("{0}", txbBox.Text).Trim();
            int index = 0;
            bool isFirstBadWord = SDKClient.SDKProperty.stringSearchEx.FindFirst(value, out index);
            bool isContainsBadWord = SDKClient.SDKProperty.stringSearchEx.ContainsAny(value);
            List<string> badWordLi = SDKClient.SDKProperty.stringSearchEx.FindAll(value);
            List<string> goodWordLi = value.Split(badWordLi.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> badWordLiOperate = new List<string>();
            StringBuilder stringBuilder = new StringBuilder(value);
            foreach (string child in badWordLi)
            {
                badWordLiOperate.Add("|" + child + "|");
            }
            foreach (string child1 in badWordLi)
            {
                foreach (string child2 in badWordLiOperate)
                {
                    if (child1.Equals(child2.Replace("|", string.Empty)))
                        stringBuilder.Replace(child1, child2);
                }
            }
            List<string> resultList = new List<string>(stringBuilder.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
            isSure = true;
            if (isFirstBadWord || isContainsBadWord)
            {
                //txbBox.Visibility = System.Windows.Visibility.Collapsed;
                //this.bord_Tip.Visibility = Visibility.Visible;
                //textBlock.Inlines.Clear();

                //BrushConverter brushConverter = new BrushConverter();
                //Brush brush = (Brush)brushConverter.ConvertFromString("#FEE057");
                //foreach (string child in resultList)
                //{
                //    if (badWordLi.Contains(child))
                //    {
                //        textBlock.Inlines.Add(new Run(child) { Background = brush });
                //    }
                //    else
                //    {
                //        textBlock.Inlines.Add(new Run(child));
                //    }
                //}
                StringBuilder sb = new StringBuilder();
                badWordLi.ForEach(x => sb.Append("\"" + x + "\"" + "、"));
                string result = sb.ToString().TrimEnd('、');
                if (IMClient.Views.MessageBox.ShowDialogBox(string.Format("【群名】中包含敏感词{0}，请修改后再试",result), isCancelShow: false))
                {
                    //this.bord_Tip.Visibility = Visibility.Collapsed;
                    //txbBox.Visibility = Visibility.Visible;
                    txbBox.Focus();
                }
                else
                {
                    //this.bord_Tip.Visibility = Visibility.Collapsed;
                    //txbBox.Visibility = Visibility.Visible;
                    txbBox.Focus();
                }
            }
            else
            {
                this.Close();
            }
        }

        
    }
}
