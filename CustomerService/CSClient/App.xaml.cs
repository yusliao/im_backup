using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using CSClient.ViewModels;
using NLog;
using Application = System.Windows.Application;
using CSClient.Views.ChildWindows;
using CSClient.Helper;
using static CSClient.Helper.TrayHelper;
using System.Collections.ObjectModel;

namespace CSClient
{

    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 是否附加内容
        /// 为了有些操作反复标记
        /// </summary>
        public static bool IsAppendContent = false;

        public static Logger Logger = LogManager.GetCurrentClassLogger();
        const string LogoIcon = "Icons/logo.ico";
        const string EmptyIcon = "Icons/empty.ico";

        DispatcherTimer _iconFlashTimer = new DispatcherTimer();
        int _counter = 0;

        static NotifyIcon _trayIcon;
        static System.Drawing.Icon _appIcon;
        public static System.Threading.Mutex MUTEX;

        static App()
        {
            //DispatcherHelper.Initialize();
        }

        public static void UpdateTray(IMModels.UserModel user)
        {
            if (_trayIcon != null && user != null)
            {
                _trayIcon.Text = $"可访\r\n{user.DisplayName}({user.PhoneNumber})";

                //if (System.IO.File.Exists(user.HeadImg))
                //{
                //    var icon = Helper.ImageHelper.ConvertToIcon(user.HeadImg);

                //    if (icon != null)
                //    {
                //        _trayIcon.Icon = _appIcon = icon;
                //    }
                //}
            }
        }

        public void Activate()
        {
            this.MainWindow.Show();
            this.MainWindow.Activate();
        }
        [DllImport("user32", CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindow(string cls, string win);
        [DllImport("user32")]
        static extern IntPtr SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32")]
        static extern bool IsIconic(IntPtr hWnd);
        [DllImport("user32")]
        static extern bool OpenIcon(IntPtr hWnd);
        private static void ActivateOtherWindow()
        {
            var other = FindWindow(null, "IM");
            if (other != IntPtr.Zero)
            {
                SetForegroundWindow(other);
                if (IsIconic(other))
                    OpenIcon(other);
            }
            else
            {
                other = FindWindow(null, "登录");
                if (other != IntPtr.Zero)
                {
                    SetForegroundWindow(other);
                    if (IsIconic(other))
                        OpenIcon(other);
                }
            }
        }

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            RemoveTrayIcon();
            AddTrayIcon();

            var win = new LoginWindow();
            win.Closed += Win_Closed;
            this.MainWindow = win;
            LoginViewModel loginViewModel = win.ViewModel as LoginViewModel;
            loginViewModel.OnLoginSuccess += LoginViewModel_OnLoginSuccess;
            win.Show();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var exception = e.ExceptionObject as Exception;
                if (exception != null)
                {
                    Logger.Error(exception, "非UI线程全局异常");
                    Logger.Error(exception.StackTrace, "非UI线程全局异常");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "不可恢复的非UI线程全局异常");
            }
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is System.OutOfMemoryException)
            {                
                Logger.Error(e.Exception, "内存溢出");
                Logger.Error(e.Exception?.StackTrace, "内存溢出");
            }
            else
            {
                Logger.Error(e.Exception, "UI线程全局异常");
                Logger.Error(e.Exception?.StackTrace, "非UI线程全局异常");
            }
            //System.Windows.MessageBox.Show("很抱歉，当前应用程序遇到一些问题，该操作已经终止，请进行重试，如果问题继续存在，请联系管理员.", "意外的操作", MessageBoxButton.OK, MessageBoxImage.Information);

            e.Handled = true;
        }

        private void Win_Closed(object sender, EventArgs e)
        {
            //var win = sender as Models.ChatWindow.View.V2.LoginWindow;
            //LoginViewModel loginViewModel = win.DataContext as LoginViewModel;
            //if (!loginViewModel.IsLoginSuccess)
            //{
            //    SDKClient.SDKClient.Instance.StopAsync().ConfigureAwait(false);
            //    Application.Current?.Shutdown(0);
            //}
            //mutex?.ReleaseMutex(); 
        }

        private void LoginViewModel_OnLoginSuccess(IMModels.LoginUser login)
        {
            var loginWin = this.MainWindow;
            loginWin.Close();

            var mainWindow = new MainWindow(login);
            this.MainWindow = mainWindow;
            mainWindow.Closed += MainWindow_Closed;
            mainWindow.Closing += MainWindow_Closing;
            mainWindow.OnFlashIcon += MainWindow_OnFlashIcon;
            mainWindow.OnCloseTrayWindow += MainWindow_OnCloseTrayWindow;
            mainWindow.OnChangeTrayWindowSize += MainWindow_OnChangeTrayWindowSize;
            mainWindow.Show();

            _trayIcon.Icon = _appIcon = new System.Drawing.Icon(LogoIcon);
            ModifyTrayIcon();
            _trayIcon.Text = string.Format("满金店客服:{0}({1})", login.User.Name, login.User.PhoneNumber);
            _iconFlashTimer = new DispatcherTimer();
            _iconFlashTimer.Interval = TimeSpan.FromSeconds(0.4);
            _iconFlashTimer.Tick += _iconFlashTimer_Tick;
        }

        private void MainWindow_OnChangeTrayWindowSize()
        {
            ChangeTrayWindowLocation();
        }

        void ChangeTrayWindowLocation()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (_trayWin == null)
                {
                    return;
                }

                //获取托盘图标位置
                RG rg = GetIconRect(_trayIcon);

                //计算弹框的位置，使其在托盘图标的正上方中间的位置
                double y = SystemParameters.WorkArea.Height;
                _trayWin.Top = y - _trayWin.ActualHeight + 5;
                _trayWin.Left = rg.Left - (_trayWin.ActualWidth / 2) + (rg.Width / 2);
            });
        }

        private void MainWindow_OnCloseTrayWindow()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    if (_trayIcon != null)
                    {
                        _trayIcon.Icon = _appIcon == null ? _appIcon = new System.Drawing.Icon(LogoIcon) : _appIcon;
                        _iconFlashTimer.Stop();
                        _counter = 0;
                    }
                    _trayWin?.Hide();
                }
                catch (Exception ex)
                {

                }
            });
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.MainWindow.Visibility = Visibility.Hidden;
            if (this.MainWindow.WindowState == WindowState.Minimized)
            {
                this.MainWindow.WindowState = WindowState.Normal;
            }
            e.Cancel = true;
        }

        private void MainWindow_OnFlashIcon(bool isFlash, bool isDisplayMainWindow = true)
        {
            if (isFlash)
            {
                _iconFlashTimer.Start();
            }
            else
            {
                if (isDisplayMainWindow)
                {
                    if (this.MainWindow.WindowState == WindowState.Minimized)
                    {
                        this.MainWindow.WindowState = WindowState.Normal;
                    }
                    this.MainWindow.Visibility = Visibility.Visible;
                }

                int count = AppData.MainMV.ChatListVM.Items.Where(info => !info.Chat.Chat.IsNotDisturb && info.UnReadCount > 0 && !string.IsNullOrEmpty(info.Chat.Chat.DisplayName)).Count();
                if (count == 0)
                {
                    MainWindow_OnCloseTrayWindow();
                }
                else if (count == 1)
                {
                    AppData.MainMV.ChatListVM.SelectedItem = AppData.MainMV.ChatListVM.Items.Where(info => !info.Chat.Chat.IsNotDisturb && info.UnReadCount > 0 && !string.IsNullOrEmpty(info.Chat.Chat.DisplayName)).First();

                    MainWindow_OnCloseTrayWindow();
                }
                else
                {
                    AppData.MainMV.ChatListVM.SelectedItem = AppData.MainMV.ChatListVM.Items.Where(info => !info.Chat.Chat.IsNotDisturb && info.UnReadCount > 0 && !string.IsNullOrEmpty(info.Chat.Chat.DisplayName)).First();
                }
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void _iconFlashTimer_Tick(object sender, EventArgs e)
        {
            _counter++;
            if (_counter % 2 == 0)
            {
                _trayIcon.Icon = _appIcon == null ? _appIcon = new System.Drawing.Icon(LogoIcon) : _appIcon;
            }
            else
            {
                _trayIcon.Icon = new System.Drawing.Icon(EmptyIcon);
            }
        }

        private void AddTrayIcon()
        {
            if (_trayIcon != null)
            {
                return;
            }
            _trayIcon = new NotifyIcon
            {
                Icon = _appIcon = new System.Drawing.Icon(LogoIcon),
                Text = "满金店客服"
            };
            _trayIcon.Visible = true;
            _trayIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    return;
                }

                if (AppData.MainMV != null && AppData.MainMV.ChatListVM != null)
                {
                    int count = AppData.MainMV.ChatListVM.Items.Where(info => !info.Chat.Chat.IsNotDisturb && info.UnReadCount > 0 && !string.IsNullOrEmpty(info.Chat.Chat.DisplayName)).Count();
                    if (count == 0)
                    {
                        MainWindow_OnCloseTrayWindow();
                    }
                    else if (count == 1)
                    {
                        AppData.MainMV.ChatListVM.SelectedItem = AppData.MainMV.ChatListVM.Items.Where(info => !info.Chat.Chat.IsNotDisturb && info.UnReadCount > 0 && !string.IsNullOrEmpty(info.Chat.Chat.DisplayName)).First();

                        MainWindow_OnCloseTrayWindow();
                    }
                    else
                    {
                        AppData.MainMV.ChatListVM.SelectedItem = AppData.MainMV.ChatListVM.Items.Where(info => !info.Chat.Chat.IsNotDisturb && info.UnReadCount > 0 && !string.IsNullOrEmpty(info.Chat.Chat.DisplayName)).First();
                    }
                }

                if (this.MainWindow.WindowState == WindowState.Minimized)
                {
                    this.MainWindow.WindowState = WindowState.Normal;
                }

                this.MainWindow.Visibility = Visibility.Visible;
                this.MainWindow.Activate();
            };

            ContextMenu menu = new ContextMenu();

            MenuItem openItem = new MenuItem();
            openItem.Text = "打开主面板";
            openItem.Click += new EventHandler(delegate
            {
                if (this.MainWindow.WindowState == WindowState.Minimized)
                {
                    this.MainWindow.WindowState = WindowState.Normal;
                }
                this.MainWindow.Visibility = Visibility.Visible;
                this.MainWindow.Activate();
            });

            MenuItem closeItem = new MenuItem();
            closeItem.Text = "退出";
            closeItem.Click += new EventHandler(delegate
            {
                if (MUTEX != null)
                {
                    App.MUTEX.ReleaseMutex();
                    MUTEX.Dispose();
                }
                this.Shutdown();
            });

            menu.MenuItems.Add(openItem);
            menu.MenuItems.Add(closeItem);

            _trayIcon.ContextMenu = menu;
        }

        TrayWindow _trayWin;
        bool _isMouseEnterWindow = false;
        DispatcherTimer _trayWinTimer;

        private void _trayWin_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isMouseEnterWindow)
            {
                _trayWin?.Hide();
            }

            _isMouseEnterWindow = false;
        }

        private void _trayWin_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _isMouseEnterWindow = true;
        }

        private void _trayIcon_MouseMove(object sender, MouseEventArgs e)
        {
            if (AppData.MainMV.TotalUnReadCount == 0)
            {
                return;
            }

            if (_iconFlashTimer == null || !_iconFlashTimer.IsEnabled)
            {
                return;
            }

            //鼠标移入托盘图标后显示弹框
            if (_trayWin == null)
            {
                _trayWin = new TrayWindow();
                _trayWin.DataContext = AppData.MainMV.ChatListVM;
                _trayWin.MouseEnter += _trayWin_MouseEnter;
                _trayWin.MouseLeave += _trayWin_MouseLeave;
                _trayWin.SizeChanged += _trayWin_SizeChanged;
                _trayWin.Closed += delegate
                {
                    _trayWin.SizeChanged -= _trayWin_SizeChanged;
                    _trayWin = null;
                    _trayWinTimer.Stop();
                };
            }

            if (_trayWinTimer.IsEnabled == false)
            {
                _trayWinTimer.Start();
            }

            _trayWin.Show();
        }

        private void _trayWin_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeTrayWindowLocation();
        }

        private void Notifybar_timer_Tick(object sender, EventArgs e)
        {
            POINT pt = new POINT();

            //获取鼠标的位置
            GetCursorPos(out pt);

            //获取托盘图标的位置
            RG rg = GetIconRect(_trayIcon);

            if (pt.x > rg.Left && pt.x < (rg.Left + rg.Width) && pt.y > rg.Top && pt.y < (rg.Top + rg.Height))
            {
                //鼠标指针还在托盘图标中，不需要处理              
            }
            else
            {
                _trayWinTimer.Stop();

                //判断指针是否移入了弹框的区域
                if (_trayWin != null && _isMouseEnterWindow == false)
                {
                    _trayWin.Hide();
                }
            }
        }

        private void ModifyTrayIcon()
        {
            _trayIcon.ContextMenu = null;
            _trayIcon.MouseMove += _trayIcon_MouseMove;
            _trayWinTimer = new DispatcherTimer();
            _trayWinTimer.Interval = TimeSpan.FromSeconds(1);
            _trayWinTimer.Tick += Notifybar_timer_Tick;

            ContextMenu menu = new ContextMenu();

            MenuItem openItem = new MenuItem();
            openItem.Text = "打开主面板";
            openItem.Click += new EventHandler(delegate
            {
                if (this.MainWindow.WindowState == WindowState.Minimized)
                {
                    this.MainWindow.WindowState = WindowState.Normal;
                }

                this.MainWindow.Visibility = Visibility.Visible;
                this.MainWindow.Activate();
            });

            //MenuItem closeRemindItem = new MenuItem();
            //closeRemindItem.Text = "取消提醒";
            //closeRemindItem.Click += new EventHandler(delegate
            //{
            //    SDKClient.SDKClient.Instance.property.CurrentAccount.IsRemind = !SDKClient.SDKClient.Instance.property.CurrentAccount.IsRemind;
            //    if (SDKClient.SDKClient.Instance.property.CurrentAccount.IsRemind)
            //    {
            //        _iconFlashTimer.Stop();
            //        closeRemindItem.Text = "开启提醒";
            //        _trayIcon.Icon = new System.Drawing.Icon("onlineTray.ico");
            //    }
            //    else
            //    {
            //        _iconFlashTimer.Start();
            //        closeRemindItem.Text = "取消提醒";
            //    }
            //});

            //MenuItem closeSoundItem = new MenuItem();
            //closeSoundItem.Text = "取消声音";
            //closeSoundItem.Click += new EventHandler(delegate
            //{
            //    SDKClient.SDKClient.Instance.property.CurrentAccount.CloseSound = !SDKClient.SDKClient.Instance.property.CurrentAccount.CloseSound;
            //    closeSoundItem.Text = SDKClient.SDKClient.Instance.property.CurrentAccount.CloseSound ? "开启声音" : "取消声音";
            //});

            MenuItem closeItem = new MenuItem();
            closeItem.Text = "退出";
            closeItem.Click += new EventHandler(delegate
            {
                bool? result = IsCancelOperate("退出程序", "您有文件正在传输中，确定终止文件传输吗？");
                if (result == true) //取消
                {
                    return;
                }
                SDKClient.SDKClient.Instance.SetCustiomServerState(SDKClient.SDKProperty.customState.quit);
                foreach (var item in AppData.MainMV.ChatListVM.Items)
                {
                    if(item.sessionType==1)//结束我的会话
                        Util.Helpers.Async.Run(async () => await SDKClient.SDKClient.Instance.SendCustiomServerMsg(item.ID.ToString(), item.SessionId, SDKClient.SDKProperty.customOption.over));
                }
                ApplicationExit(null, null);
                SDKClient.SDKClient.Instance.StopAsync().ConfigureAwait(false);
                Application.Current?.Shutdown(0);
            });

            menu.MenuItems.Add(openItem);
            //menu.MenuItems.Add(closeRemindItem);
            //menu.MenuItems.Add(closeSoundItem);
            menu.MenuItems.Add(closeItem);

            _trayIcon.ContextMenu = menu;
        }

        private void RemoveTrayIcon()
        {
            if (_trayIcon != null)
            {
                _iconFlashTimer.Stop();

                _trayIcon.Visible = false;
                _trayIcon.Dispose();
                _trayIcon = null;
            }
        }

        public void ApplicationExit(object sender, ExitEventArgs e)
        {
            if (MUTEX != null)
            {
                App.MUTEX.ReleaseMutex();
                MUTEX.Close();
                App.MUTEX = null;
            }
            RemoveTrayIcon();
        }

        public static void CancelFileOperate(int targID = -1)
        {
            var list = Views.Controls.FileChatItem.AcioningItems.ToList();
            if (targID == -1)
            {
                if (list.Count > 0)
                {
                    foreach (var v in list)
                    {
                        v.Cancel();
                    }

                    System.Threading.Thread.Sleep(1000);
                }
            }
            else
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    foreach (var item in list)
                    {
                        if (item.ChatViewModel != null && item.ChatViewModel.ID == targID)
                        {
                            item.Cancel();
                        }
                    }
                    System.Threading.Thread.Sleep(500);
                }));
            }
        }

        /// <summary>
        /// 是否取消当前文件传输操作
        /// </summary>
        /// <param name="tip">操作内容提示提示内容</param>
        /// <param name="targID">目标ID，为-1 即视为主动操作，需要询问，默认</param>
        /// <returns>返回结果决定是否继续当前操作</returns>
        public static bool? IsCancelOperate(string tip, string content, int targetID = -1)
        {
            var list = Views.Controls.FileChatItem.AcioningItems.ToList();
            if (targetID == -1)
            {
                if (list.Count > 0)
                {
                    bool result = Views.MessageBox.ShowDialogBox(content, tip);
                    if (result)
                    {
                        foreach (var v in list)
                        {
                            v.Cancel();
                        }

                        System.Threading.Thread.Sleep(1000);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else
            {
                list = list.Where(info => info.ChatViewModel.ID == targetID).ToList();
                if (list.Count > 0)
                {
                    bool result = Views.MessageBox.ShowDialogBox(content, tip);
                    if (result)
                    {
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            foreach (var item in list)
                            {
                                item.Cancel();
                            }
                        }));
                        System.Threading.Thread.Sleep(500);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                return null;
            }

            return null;
        }
    }
}
