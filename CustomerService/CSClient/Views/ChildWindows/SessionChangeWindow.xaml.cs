using CSClient.ViewModels;
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

namespace CSClient.Views.ChildWindows
{
    /// <summary>
    /// SeesionChanageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SessionChangeWindow : Window
    {
        public SessionChangeWindow(int to)
        {
            userid = to;
            InitializeComponent();
            this.Loaded += SessionChangeWindow_Loaded1;

                
        }

        private void SessionChangeWindow_Loaded1(object sender, RoutedEventArgs e)
        {
            this.cobSession.ItemsSource = CSItems;
            this.cobSession.SelectedIndex = 0;
        }

        private void SessionChangeWindow_Loaded(object sender, RoutedEventArgs e)
        {
           
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// 当前在线客服集合
        /// </summary>
        public List<OnlineStatusEntity> CSItems { get; set; }
        int userid;
        
        OnlineStatusEntity selectItem;

        private void CobSession_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combox = sender as ComboBox;
            var obj= combox.SelectedItem as OnlineStatusEntity;
            selectItem = obj;
            
        }

        private async void BtnSure_Click(object sender, RoutedEventArgs e)
        {
            if (selectItem != null)
            {
                await SDKClient.SDKClient.Instance.SendCustiomExchangeMsg(userid.ToString(), selectItem.Servicerid).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        AppData.MainMV.ShowTip("会话转移失败");
                        return;
                    }
                    else
                    {
                        if (t.Result)
                        {
                            AppData.MainMV.ShowTip("会话转移成功");
                            this.Close();
                        }
                        else
                        {
                            AppData.MainMV.ShowTip("会话转移失败");
                            return;
                        }
                    }
                });
            }
            else
            {
                AppData.MainMV.ShowTip("请选择要转移的客服");
            }
        }
    }
}
