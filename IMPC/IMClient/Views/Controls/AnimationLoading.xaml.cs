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

namespace IMClient.Views.Controls
{
    /// <summary>
    /// AnimationLoading.xaml 的交互逻辑
    /// </summary>
    public partial class AnimationLoading : UserControl
    {        
        public bool IsStop
        {
            get { return (bool)GetValue(IsStopProperty); }
            set { SetValue(IsStopProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsStop.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsStopProperty =
            DependencyProperty.Register("IsStop", typeof(bool), typeof(AnimationLoading), new PropertyMetadata(OnIsStopPropertyChanged));

        private static void OnIsStopPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnimationLoading target = d as AnimationLoading;
            target.BeginOrStopAnimation();
        }

        public AnimationLoading()
        {
            InitializeComponent(); 
        }
        
        private void BeginOrStopAnimation()
        {
            if (IsStop)
            {
                Stop();
            }
            else
            {
                Begin();
            }
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
