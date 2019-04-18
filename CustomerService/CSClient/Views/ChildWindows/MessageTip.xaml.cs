using CSClient.ViewModels;
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
using System.Windows.Shapes;

namespace CSClient.Views 
{
    /// <summary>
    /// MessageTip.xaml 的交互逻辑
    /// </summary>
    public partial class MessageTip : Window
    {
        Storyboard _story = new Storyboard();
        private MessageTip(string tip)
        {
            InitializeComponent();
            this.Topmost = true;
            this.Owner = App.Current.MainWindow; 
            this.tbTip.Text = tip;
            this.Deactivated += MessageTip_Deactivated;

            _story = this.Resources["Loading"] as Storyboard;

            _story.Completed += _story_Completed;
            _story.Begin();
        }

        private void _story_Completed(object sender, EventArgs e)
        {
            CloseTip();
        }

        private void MessageTip_Deactivated(object sender, EventArgs e)
        { 
            //_story.Stop();
            this.Opacity = 0;
        }


        private void CloseTip()
        {
            _story.Completed -= _story_Completed;
            this.Deactivated -= MessageTip_Deactivated;
            _tip = null;
            this.Close(); 
        } 

       static MessageTip _tip;
        /// <summary>
        /// 显示信息提示框
        /// </summary>
        /// <param name="msg">提示内容</param>
        /// <param name="title">提示标题</param>
        /// <returns></returns>
        public static void ShowTip(string msg)
        {
            if (_tip != null)
            {
                _tip.CloseTip();
            }
            Task.Delay(200).ContinueWith(t =>
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    _tip = new MessageTip(msg);
                    _tip.Show();
                })); 
            });
           
            
        }
    }
}
