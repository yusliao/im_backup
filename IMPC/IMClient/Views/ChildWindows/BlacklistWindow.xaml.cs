using IMClient.ViewModels;
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
using System.Windows.Shapes;

namespace IMClient.Views.ChildWindows
{
    /// <summary>
    /// BlacklistWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BlacklistWindow : Window
    {
        public BlacklistWindow()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            AppData.MainMV.IsOpenBusinessCard = false;
            this.Close();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            AppData.MainMV.IsOpenBusinessCard = false;
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }
}
