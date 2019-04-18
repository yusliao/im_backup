using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace IMLaunch
{
    /// <summary>
    /// UpdateFailedWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateFailedWindow : Window
    {
        public string UpdateVersion { get; set; }
        public string CurVersion { get; set; }

        private static UpdateFailedWindow _instance;
        private static readonly object ObjLok = new object();

        private UpdateFailedWindow()
        {
            InitializeComponent();
        }

        public static UpdateFailedWindow Instance()
        {
            lock (ObjLok)
            {
                return _instance ?? (_instance = new UpdateFailedWindow());
            }
        }

        /// <summary> 
        /// 重写Close,窗口关闭时设置为隐藏。  
        /// </summary>  
        protected override void OnClosing(CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Application.Current?.Shutdown(0);
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();

            UpdateViewModel vm = new UpdateViewModel(UpdateVersion, CurVersion);
            UpdateWindow win = new UpdateWindow();
            win.DataContext = vm;
            win.ShowDialog();

            if (!vm.IsError)
            {
                Application.Current?.Shutdown(0);
            }
            else
            {
                UpdateFailedWindow.Instance().ShowDialog();
            }

            if (vm.IsStartMainApp)
            {
                System.Diagnostics.Process.Start(string.Format(@"{0}\{1}", AppDomain.CurrentDomain.BaseDirectory, App.MainProgramName));
            }
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
