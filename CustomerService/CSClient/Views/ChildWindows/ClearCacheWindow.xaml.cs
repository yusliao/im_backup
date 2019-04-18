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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CSClient.Views.ChildWindows
{
    /// <summary>
    /// ClearCacheWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ClearCacheWindow : Window
    {
        public ClearCacheWindow()
        {
            InitializeComponent();

            this.Owner = App.Current.MainWindow;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ButtonBase btn = e.Source as Button;
            switch (btn.Uid)
            {
                default:
                case "CANCEL":
                    this.DialogResult = false;
                    break;
                case "SURE":
                    Clear(true);
                    break;
            }
        }

        public void Clear(bool hasShow=false)
        {
            var filePath = System.IO.Path.Combine(SDKClient.SDKProperty.filePath, SDKClient.SDKClient.Instance.property.CurrentAccount.loginId);
            if (System.IO.Directory.Exists(filePath))
            {
                try
                {
                    var files = System.IO.Directory.GetFiles(filePath);
                    foreach (var item in files)
                    {
                        System.IO.File.Delete(item);
                    }
                    System.IO.Directory.Delete(filePath);
                }
                catch (Exception)
                {

                }
            }

            var imgPath = System.IO.Path.Combine(SDKClient.SDKProperty.imgPath, SDKClient.SDKClient.Instance.property.CurrentAccount.loginId);
            if (System.IO.Directory.Exists(imgPath))
            {
                try
                {
                    var files = System.IO.Directory.GetFiles(imgPath);
                    foreach (var item in files)
                    {
                        System.IO.File.Delete(item);
                    }
                    System.IO.Directory.Delete(imgPath, true);
                }
                catch (Exception)
                {

                }
            }

            AppData.MainMV.TotalUnReadCount = 0;
            AppData.MainMV.ChatListVM.Items.Clear();
            SDKClient.SDKClient.Instance.DeleteHistoryMsg();
            if (hasShow)
            {
                this.DialogResult = true;
            }
            //IMUI.View.V2.MessageTip.ShowTip("缓存已清除", IMUI.View.V2.TipTypes.OK);
        }
    }
}
