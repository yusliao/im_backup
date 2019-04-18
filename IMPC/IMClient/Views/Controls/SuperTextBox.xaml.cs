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
    /// SuperTextBox.xaml 的交互逻辑
    /// </summary>
    public partial class SuperTextBox : UserControl
    {
        public SuperTextBox()
        {
            InitializeComponent();
        }

       

        private void tbx_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.tbk.Inlines.Clear();
            bool isContainsBadWord = SDKClient.SDKProperty.stringSearchEx.ContainsAny((sender as TextBox).Text);
            List<string> badWordLi = SDKClient.SDKProperty.stringSearchEx.FindAll((sender as TextBox).Text);
            List<string> goodWordLi = (sender as TextBox).Text.Split(badWordLi.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> badWordLiOperate = new List<string>();
            StringBuilder stringBuilder = new StringBuilder((sender as TextBox).Text);
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
            BrushConverter brushConverter = new BrushConverter();
            Brush brush = (Brush)brushConverter.ConvertFromString("#FEE057");
            foreach (string child in resultList)
            {
                if (badWordLi.Contains(child))
                {
                    tbk.Inlines.Add(new Run(child) { Background = brush });
                }
                else
                {
                    tbk.Inlines.Add(child);
                }
            }
            if (badWordLi.Count > 0)
                IMClient.Views.MessageBox.ShowDialogBoxEx("【{0}】中包含敏感词，请修改后再试", (string)this.Tag, isCancelShow: false);

            
             
        }

        private void tbx_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            tbx_TextChanged(sender, null);
        }
    }
}
