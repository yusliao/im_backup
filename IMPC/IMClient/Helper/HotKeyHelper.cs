using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace IMClient.Helper
{
    public sealed class HotKey : IDisposable
    {
        public event Action<HotKey> HotKeyPressed;

        private readonly int _id;
        private bool _isKeyRegistered;
        readonly IntPtr _handle;

        public HotKey(ModifierKeys modifierKeys, Keys key, System.Windows.Window window)
            : this(modifierKeys, key, new WindowInteropHelper(window))
        {
            Contract.Requires(window != null);
        }

        public HotKey(ModifierKeys modifierKeys, Keys key, WindowInteropHelper window)
            : this(modifierKeys, key, window.Handle)
        {
            Contract.Requires(window != null);
        }

        public HotKey(ModifierKeys modifierKeys, Keys key, IntPtr windowHandle)
        {
            Contract.Requires(modifierKeys != ModifierKeys.None || key != Keys.None);
            Contract.Requires(windowHandle != IntPtr.Zero);

            Key = key;
            KeyModifier = modifierKeys;
            _id = GetHashCode();
            _handle = windowHandle;
            RegisterHotKey();
            ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessageMethod;
        }

        ~HotKey()
        {
            Dispose();
        }

        public Keys Key { get; private set; }

        public ModifierKeys KeyModifier { get; private set; }

        public void RegisterHotKey()
        {
            if (Key == Keys.None)
                return;
            if (_isKeyRegistered)
                UnregisterHotKey();
            _isKeyRegistered = HotKeyWinApi.RegisterHotKey(_handle, _id, KeyModifier, Key);
            if (!_isKeyRegistered)
            {
                //TODO：弹窗提醒 热键冲突
                //throw new ApplicationException("Hotkey already in use");

                //MessageBox.Show("快捷热键【Ctrl+Alt+A】已经被占用！");
            }
        }

        public void UnregisterHotKey()
        {
            _isKeyRegistered = !HotKeyWinApi.UnregisterHotKey(_handle, _id);
        }

        public void Dispose()
        {
            try
            {
                ComponentDispatcher.ThreadPreprocessMessage -= ThreadPreprocessMessageMethod;
                UnregisterHotKey();
            }
            catch
            {

            }
        }

        private void ThreadPreprocessMessageMethod(ref MSG msg, ref bool handled)
        {
            if (!handled)
            {
                if (msg.message == HotKeyWinApi.WmHotKey
                    && (int)(msg.wParam) == _id)
                {
                    OnHotKeyPressed();
                    handled = true;
                }
            }
        }

        private void OnHotKeyPressed()
        {
            if (HotKeyPressed != null)
                HotKeyPressed(this);
        }
    }

    internal class HotKeyWinApi
    {
        public const int WmHotKey = 0x0312;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, Keys vk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
