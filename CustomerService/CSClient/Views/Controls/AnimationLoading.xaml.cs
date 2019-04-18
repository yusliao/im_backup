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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CSClient.Views.Controls
{
    /// <summary>
    /// AnimationLoading.xaml 的交互逻辑
    /// </summary>
    public partial class AnimationLoading : UserControl
    {
         
        public AnimationLoading()
        {
            InitializeComponent(); 
        }
         
        private void InitializeStoryboard(int second)
        {
            //second *= 1000;

            //double from = 0;
            //double to = 0;
            //double time = 0;
            //double beginTme = 0;
            //double all = 1;

            //for(int i = 0; i < 8; i++)
            //{
            //    beginTme = second * i / 8;
            //    to=

            //    DoubleAnimation da = new DoubleAnimation(from, to, new Duration(TimeSpan.FromMilliseconds(time)));
            //    da.BeginTime = TimeSpan.FromMilliseconds(beginTme);
            //}
        }

        public void Begin()
        {
            this.Visibility = Visibility.Visible;
            (this.Resources["Loading"] as Storyboard).Begin();
        }

        public void Stop()
        {
            this.Visibility = Visibility.Collapsed;
            (this.Resources["Loading"] as Storyboard).Stop();
        }

    }
}
