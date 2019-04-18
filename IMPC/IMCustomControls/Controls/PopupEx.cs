using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;

namespace IMCustomControls.Controls
{
    public class PopupEx : Popup
    {

        public static DependencyProperty TopmostProperty = Window.TopmostProperty.AddOwner(typeof(PopupEx), new FrameworkPropertyMetadata(false, OnTopmostChanged));
        public bool Topmost
        {
            get { return (bool)GetValue(TopmostProperty); }
            set { SetValue(TopmostProperty, value); }
        }
        private static void OnTopmostChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as PopupEx).UpdateWindow();
        }
        protected override void OnOpened(EventArgs e)
        {
            UpdateWindow();
        }
        public bool IsUpdateWindow = false;

        private void UpdateWindow()
        {
            var hwnd = ((HwndSource)PresentationSource.FromVisual(this.Child)).Handle;
            RECT rect;
            if (GetWindowRect(hwnd, out rect))
            {
                IsUpdateWindow = true;
                SetWindowPos(hwnd, Topmost ? -1 : -2, rect.Left, rect.Top, (int)this.Width, (int)this.Height, 0);
            }
        }
        #region imports definitions

        public const int SWP_NOSIZE = 1; //{忽略 cx、cy, 保持大小}
        public const int SWP_NOMOVE = 2; //{忽略 X、Y, 不改变位置}
        public const int SWP_NOZORDER = 4; //{忽略 hWndInsertAfter, 保持 Z 顺序}
        public const int SWP_NOREDRAW = 8; //{不重绘}
        public const int SWP_NOACTIVATE = 10; //{不激活}
        public const int SWP_FRAMECHANGED = 20; //{强制发送 WM_NCCALCSIZE 消息, 一般只是在改变大小时才发送此消息}
        public const int SWP_SHOWWINDOW = 40; //{显示窗口}
        public const int SWP_HIDEWINDOW = 80; //{隐藏窗口}
        public const int SWP_NOCOPYBITS = 100; //{丢弃客户区}
        public const int SWP_NOOWNERZORDER = 200; //{忽略 hWndInsertAfter, 不改变 Z 序列的所有者}
        public const int SWP_NOSENDCHANGING = 400; //{不发出 WM_WINDOWPOSCHANGING 消息}
        public const int SWP_DRAWFRAME = SWP_FRAMECHANGED; //{画边框}
        public const int SWP_NOREPOSITION = SWP_NOOWNERZORDER;//{}
        public const int SWP_DEFERERASE = 2000; //{防止产生 WM_SYNCPAINT 消息}
        public const int SWP_ASYNCWINDOWPOS = 4000; //{若调用进程不拥有窗口, 系统会向拥有窗口的线程发出需求}

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32", EntryPoint = "SetWindowPos")]
        private static extern int SetWindowPos(IntPtr hWnd, int hwndInsertAfter, int x, int y, int cx, int cy, int wFlags);
        #endregion
    }
}
