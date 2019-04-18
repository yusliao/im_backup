using IMClient.ViewModels;
using IMModels;
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
    /// FowardWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FowardWindow : Window
    {
        public FowardWindow(int chatID, string msgID, bool isGroup = false, bool isInviteJoin = false)
        {
            InitializeComponent();
            this.DataContext = new FowardViewModel(chatID,msgID,isGroup, isInviteJoin);
            fowardView.btnCancel.Click += Button_Click;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }
}
