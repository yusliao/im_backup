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

namespace IMLaunch
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 是否来自主程序设置里面的更新
        /// </summary>
        public bool IsUpdateFromMainProcess { get; set; }

        public readonly string MainProgramPath;
        string _updateVersion;
        string _curVersion;
        bool _isForceUpdate;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateVersion">最新版本号</param>
        /// <param name="curVersion">当前版本号</param>
        /// <param name="isForceUpdate">是否是重要版本更新（强制更新）</param>
        /// <param name="updateContent">更新内容</param>
        public MainWindow(string updateVersion, string curVersion, bool isForceUpdate, string updateContent)
        {
            InitializeComponent();

            MainProgramPath = string.Format(@"{0}\{1}", AppDomain.CurrentDomain.BaseDirectory, App.MainProgramName);
            
            _updateVersion = this.tbVersion.Text = updateVersion;
            _curVersion = curVersion;
            _isForceUpdate = isForceUpdate;

            this.tbDescription.Text = isForceUpdate ? "重要版本更新" : "发现版本更新";
            this.btnIgnore.Visibility = isForceUpdate ? Visibility.Collapsed : Visibility.Visible;
            //this.tbUpdateContent.Text = "1. 清除缓存会清除图片，文件，聊天消息及聊天条目；\r\n" +
            //                                            "2.修改了发送文件接口；\r\n" +
            //                                            "3.修改了离线消息接口；\r\n" +
            //                                            "4.修改了 上翻时候重复问题；\r\n" +
            //                                            "5.修改了 置顶排序问题；\r\n" +
            //                                            "6.测试环境和正式环境同个账号不再冲突;\r\n" +
            //                                            "7.清除缓存会清除图片，文件，聊天消息及聊天条目；\r\n" +
            //                                            "8.修改了发送文件接口；\r\n" +
            //                                            "9.修改了离线消息接口；";
            this.tbUpdateContent.Text = updateContent;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (_isForceUpdate)
            {
                this.Close();

                AbandonUpdateWindow win = new AbandonUpdateWindow();
                win.OnUpdate += Win_OnUpdate;
                win.ShowDialog();
            }
            else
            {
                Application.Current?.Shutdown(0);
            }
        }

        private void Win_OnUpdate()
        {
            btnUpdate_Click(null, null);
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            UpdateViewModel.KillMainProcess();

            this.Close();

            UpdateViewModel vm = new UpdateViewModel(_updateVersion, _curVersion);
            UpdateWindow win = new UpdateWindow();
            win.DataContext = vm;
            win.ShowDialog();

            Application.Current?.Shutdown(0);

            if (vm.IsStartMainApp)
            {
                System.Diagnostics.Process.Start(MainProgramPath);
            }
        }

        private void btnIgnore_Click(object sender, RoutedEventArgs e)
        {
            Application.Current?.Shutdown(0);
            if (!IsUpdateFromMainProcess)
            {
                System.Diagnostics.Process.Start(MainProgramPath);
            }
        }
    }
}
