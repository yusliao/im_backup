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

namespace CSClient.Views.Controls
{
    /// <summary>
    /// 头像图片控件
    /// </summary>
    public partial class HeadImage : UserControl
    { 
        public HeadImage()
        {
            InitializeComponent();
        }
         
        public string ImagePath
        {
            get { return (string)GetValue(ImagePathProperty); }
            set { SetValue(ImagePathProperty, value); }
        }
         
        public static readonly DependencyProperty ImagePathProperty =
            DependencyProperty.Register("ImagePath", typeof(string), typeof(HeadImage), new PropertyMetadata(OnImagePathPropertyChanged));

        private static void OnImagePathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HeadImage target = d as HeadImage; 
            target.LoadImg(); 
        }

        private void LoadImg()
        {
            if (System.IO.File.Exists(this.ImagePath))
            {
                this.imgBrush.ImageSource = new BitmapImage(new Uri(ImagePath));
            }
            else
            {
                this.imgBrush.ImageSource = null;
            }
           
        }
    }
}
