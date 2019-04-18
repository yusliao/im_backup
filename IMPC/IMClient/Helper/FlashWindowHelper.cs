using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Interop;

namespace IMClient.Helper
{
    public class FlashWindowHelper
    {
        Timer _timer;
        int _count = 0;
        int _maxTimes = 0;
        IntPtr _window;

        public void Flash(int times, double millliseconds, IntPtr window)
        {
            _maxTimes = times;
            _window = window;

            _timer = new Timer();
            _timer.Interval = millliseconds;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (++_count < _maxTimes)
            {
                FlashWindow(_window, (_count % 2) == 0);
            }
            else
            {
                _timer.Stop();
            }
        }

        /// <summary>
        /// 闪烁窗口
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="type">闪烁类型</param>
        /// <returns></returns>
        public static bool FlashWindowEx(System.Windows.Window win, FlashType type)
        {
            //if (win.IsActive)
            //    return false;

            WindowInteropHelper hWnd = new WindowInteropHelper(win);

            FLASHWINFO fInfo = new FLASHWINFO();
            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = hWnd.Handle;//要闪烁的窗口的句柄，该窗口可以是打开的或最小化的
            fInfo.dwFlags = (uint)type;//闪烁的类型
            fInfo.uCount = UInt32.MaxValue;//闪烁窗口的次数
            fInfo.dwTimeout = 0; //窗口闪烁的频度，毫秒为单位；若该值为0，则为默认图标的闪烁频度
            return FlashWindowEx(ref fInfo);
        }

        /// <summary>
        /// 停止闪烁
        /// </summary>
        /// <param name="win"></param>
        public static void StopFlashingWindow(System.Windows.Window win)
        {
            WindowInteropHelper h = new WindowInteropHelper(win);

            FLASHWINFO info = new FLASHWINFO();
            info.hwnd = h.Handle;
            info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
            info.dwFlags = (uint)FlashType.FLASHW_STOP;
            info.uCount = 0;// UInt32.MaxValue;
            info.dwTimeout = 0;

            FlashWindowEx(ref info);
        }

        /// <summary>
        /// 闪烁窗口
        /// </summary>
        /// <param name="pwfi">窗口闪烁信息结构</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);
        /// <summary>  
        /// 窗口闪动  
        /// </summary>  
        /// <param name="hwnd">窗口句柄</param>  
        /// <param name="bInvert">是否为闪</param>  
        /// <returns>成功返回0</returns>  
        [DllImport("user32.dll")]
        public static extern bool FlashWindow(IntPtr hwnd, bool bInvert);
        /// <summary>
        /// 包含系统应在指定时间内闪烁窗口次数和闪烁状态的信息
        /// </summary>
        public struct FLASHWINFO
        {
            /// <summary>
            /// 结构大小
            /// </summary>
            public uint cbSize;
            /// <summary>
            /// 要闪烁或停止的窗口句柄
            /// </summary>
            public IntPtr hwnd;
            /// <summary>
            /// 闪烁的类型
            /// </summary>
            public uint dwFlags;
            /// <summary>
            /// 闪烁窗口的次数
            /// </summary>
            public uint uCount;
            /// <summary>
            /// 窗口闪烁的频度，毫秒为单位；若该值为0，则为默认图标的闪烁频度
            /// </summary>
            public uint dwTimeout;
        }
    }
}
