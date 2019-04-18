using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;

namespace IMClient.Helper
{
    public class TrayHelper
    {
        #region 获取鼠标指针相对于屏幕的坐标
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;

            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetCursorPos(out POINT pt);
        #endregion

        #region 获取托盘图标的位置
        public class RG
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public RG(int left, int top, int width, int height)
            {
                Left = left;
                Top = top;
                Width = width;
                Height = height;
            }
        }

        public static RG GetIconRect(NotifyIcon icon)
        {
            RECT rect = new RECT();
            NOTIFYICONIDENTIFIER notifyIcon = new NOTIFYICONIDENTIFIER();

            notifyIcon.cbSize = Marshal.SizeOf(notifyIcon);
            notifyIcon.hWnd = GetHandle(icon);
            notifyIcon.uID = GetId(icon);

            int hresult = Shell_NotifyIconGetRect(ref notifyIcon, out rect);            
            return new RG(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public Int32 left;
            public Int32 top;
            public Int32 right;
            public Int32 bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NOTIFYICONIDENTIFIER
        {
            public Int32 cbSize;
            public IntPtr hWnd;
            public Int32 uID;
            public Guid guidItem;
        }

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern int Shell_NotifyIconGetRect([In]ref NOTIFYICONIDENTIFIER identifier, [Out]out RECT iconLocation);

        private static FieldInfo windowField = typeof(NotifyIcon).GetField("window", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        private static IntPtr GetHandle(NotifyIcon icon)
        {
            if (windowField == null) throw new InvalidOperationException("[Useful error message]");
            NativeWindow window = windowField.GetValue(icon) as NativeWindow;

            if (window == null) throw new InvalidOperationException("[Useful error message]");
            return window.Handle;
        }

        private static FieldInfo idField = typeof(NotifyIcon).GetField("id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        private static int GetId(NotifyIcon icon)
        {
            if (idField == null) throw new InvalidOperationException("[Useful error message]");
            return (int)idField.GetValue(icon);
        }
        #endregion

    }
}


