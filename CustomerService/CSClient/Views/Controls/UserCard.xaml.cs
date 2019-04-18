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
using CSClient.ViewModels;
using IMModels;

namespace CSClient.Views.Controls
{
    /// <summary>
    /// 个人名片
    /// </summary>
    public partial class UserCard : UserControl
    { 
        public UserCard()
        {
            InitializeComponent(); 
            
            this.tbNickName.KeyDown += TbNickName_KeyDown;
            
        }
         
        private void TbNickName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.cc.Focus();
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            this.cc.Focus();
        }
        private void Head_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AppData.MainMV.IsOpenBusinessCard = false;
            ChildWindows.ImageScanWindow.ShowScan(this.ibHead.ImageSource);
        }

        private void btnChat_Click(object sender, RoutedEventArgs e)
        { 
            AppData.MainMV.IsOpenBusinessCard = false;
        }
    }
}
