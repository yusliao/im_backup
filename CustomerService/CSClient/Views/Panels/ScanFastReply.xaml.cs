using CSClient.ViewModels;
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

namespace CSClient.Views.Panels
{
    /// <summary>
    /// ScanFastReply.xaml 的交互逻辑
    /// </summary>
    public partial class ScanFastReply : UserControl
    {
        public ScanFastReply()
        {
            InitializeComponent();

            this.Loaded += ScanFastReply_Loaded;
        }

        private void ScanFastReply_Loaded(object sender, RoutedEventArgs e)
        {
            //if (!AppData.MainMV.IsAdmin)
            //{
            //    this.tabCommon.Visibility = Visibility.Collapsed;
            //    this.tab.SelectedIndex = 1;
            //}
            //else
            //{
            //    this.tab.SelectedIndex = 0;
            //}
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            AppData.MainMV.ChatListVM.SelectedItem.FastReply(tb.Text);
        }
    }
}
