using System;
using System.Collections.Generic;
using System.IO;
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
using IMModels;

namespace IMClient.Views.Controls
{
    /// <summary>
    /// FilePackage.xaml 文件包 的交互逻辑
    /// </summary>
    public partial class FilePackage : UserControl
    {
        public FilePackage()
        {
            InitializeComponent();
            this.PreviewMouseRightButtonUp += FilePackage_PreviewMouseRightButtonUp;
        }

        private void FilePackage_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        public FileInfo FileInfo
        {
            get { return (FileInfo)GetValue(FileInfoProperty); }
            set { SetValue(FileInfoProperty, value); }
        }
         
        public static readonly DependencyProperty FileInfoProperty =
            DependencyProperty.Register("FileInfo", typeof(FileInfo), typeof(FilePackage),new PropertyMetadata(OnFileInfoPropertyChanged) );

        private static void OnFileInfoPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FilePackage target = d as FilePackage;
            target.LoadFile();
        }
         

        private ImageSource _thumbnail;
        /// <summary>
        /// 缩略图
        /// </summary>
        public ImageSource Thumbnail
        {
            get { return  _thumbnail; }
            private set {  _thumbnail = value; }
        } 


        private void LoadFile()
        {
            string fileName = System.IO.Path.Combine(this.FileInfo.DirectoryName, this.FileInfo.Name);
            
            this.DataContext = this.FileInfo;
            this.img.Source = this.Thumbnail = Helper.FileHelper.GetFileImage(fileName,false);
        }

       
    }
}
