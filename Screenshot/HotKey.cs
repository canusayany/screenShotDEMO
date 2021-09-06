using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace Screenshot
{
    /// <summary>
    /// Defines the <see cref="HotKey" />.
    /// </summary>
    [System.Security.SuppressUnmanagedCodeSecurity]
    public class HotKey
    {
        #region Constants

        /// <summary>
        /// 热键消息.
        /// </summary>
        internal const int WM_HOTKEY = 0x312;

        #endregion

        #region Fields

        /// <summary>
        /// Defines the source.
        /// </summary>
        private HwndSource source;

        /// <summary>
        /// Defines the action.
        /// </summary>
        private Action action;

        #endregion

        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="HotKey"/> class from being created.
        /// </summary>
        /// <param name="window">The window<see cref="Window"/>.</param>
        /// <param name="modifiers">The modifiers<see cref="ModifierKeys"/>.</param>
        /// <param name="key">The key<see cref="Keys"/>.</param>
        /// <param name="action">The action<see cref="Action"/>.</param>
        private HotKey(Window window, ModifierKeys modifiers, Keys key, Action action)
        {
            Modifiers = modifiers;
            Key = key;
            this.action = action;

            try
            {
                var helper = new WindowInteropHelper(window);
                var hwnd = helper.Handle;
                source = HwndSource.FromHwnd(hwnd);
                source.AddHook(WndProc);
                var strKey = GetString(modifiers, key);
                Id = GlobalFindAtom(strKey);
                if (Id != 0)
                {
                    UnregisterHotKey(hwnd, Id);
                }
                else
                {
                    Id = GlobalAddAtom(strKey);
                }

                if (!RegisterHotKey(hwnd, Id, modifiers, key))
                    throw new Exception("热键注册失败!");
            }
            catch (Exception e)
            {
                throw new HotKeyRegisterFailException(e);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Id.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the Key.
        /// </summary>
        public Keys Key { get; }

        /// <summary>
        /// Gets the Modifiers.
        /// </summary>
        public ModifierKeys Modifiers { get; }

        #endregion

        #region Methods

        /// <summary>
        /// The Register.
        /// </summary>
        /// <param name="window">The window<see cref="Window"/>.</param>
        /// <param name="modifiers">The modifiers<see cref="ModifierKeys"/>.</param>
        /// <param name="key">The key<see cref="Keys"/>.</param>
        /// <param name="action">The action<see cref="Action"/>.</param>
        /// <returns>The <see cref="HotKey"/>.</returns>
        public static HotKey Register(Window window, ModifierKeys modifiers, Keys key, Action action)
        {
            return new HotKey(window, modifiers, key, action);
        }

        /// <summary>
        /// The Unregister.
        /// </summary>
        public void Unregister()
        {
            UnregisterHotKey(source.Handle, Id);
            GlobalDeleteAtom(GetString(Modifiers, Key));
            source.RemoveHook(WndProc);
        }

        /// <summary>
        /// The GetString.
        /// </summary>
        /// <param name="modifiers">The modifiers<see cref="ModifierKeys"/>.</param>
        /// <param name="key">The key<see cref="Keys"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        internal static string GetString(ModifierKeys modifiers, Keys key)
            => $"Saar:{modifiers}+{key}";

        /// <summary>
        /// 向原子表中添加全局原子.
        /// </summary>
        /// <param name="lpString">The lpString<see cref="string"/>.</param>
        /// <returns>The <see cref="ushort"/>.</returns>
        [DllImport("kernel32", SetLastError = true)]
        internal static extern ushort GlobalAddAtom(string lpString);

        /// <summary>
        /// 在表中删除全局原子.
        /// </summary>
        /// <param name="nAtom">The nAtom<see cref="string"/>.</param>
        /// <returns>The <see cref="ushort"/>.</returns>
        [DllImport("kernel32", SetLastError = true)]
        internal static extern ushort GlobalDeleteAtom(string nAtom);

        /// <summary>
        /// 在表中搜索全局原子.
        /// </summary>
        /// <param name="lpString">The lpString<see cref="string"/>.</param>
        /// <returns>The <see cref="ushort"/>.</returns>
        [DllImport("kernel32", SetLastError = true)]
        internal static extern ushort GlobalFindAtom(string lpString);

        /// <summary>
        /// 注册热键.
        /// </summary>
        /// <param name="hWnd">The hWnd<see cref="IntPtr"/>.</param>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <param name="fsModifuers">The fsModifuers<see cref="ModifierKeys"/>.</param>
        /// <param name="vk">The vk<see cref="Keys"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        [DllImport("user32", SetLastError = true)]
        internal static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifuers, Keys vk);

        /// <summary>
        /// 注销热键.
        /// </summary>
        /// <param name="hWnd">The hWnd<see cref="IntPtr"/>.</param>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        [DllImport("user32", SetLastError = true)]
        internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        /// The WndProc.
        /// </summary>
        /// <param name="hwnd">The hwnd<see cref="IntPtr"/>.</param>
        /// <param name="msg">The msg<see cref="int"/>.</param>
        /// <param name="wParam">The wParam<see cref="IntPtr"/>.</param>
        /// <param name="lParam">The lParam<see cref="IntPtr"/>.</param>
        /// <param name="handle">The handle<see cref="bool"/>.</param>
        /// <returns>The <see cref="IntPtr"/>.</returns>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handle)
        {
            if (msg == WM_HOTKEY && (int)wParam == Id)
            {
                action();
                handle = true;
            }
            return IntPtr.Zero;
        }

        #endregion
    }

    /// <summary>
    /// Defines the <see cref="HotKeyRegisterFailException" />.
    /// </summary>
    public class HotKeyRegisterFailException : Exception
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HotKeyRegisterFailException"/> class.
        /// </summary>
        /// <param name="e">The e<see cref="Exception"/>.</param>
        public HotKeyRegisterFailException(Exception e) : base("注册热键失败。", e)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HotKeyRegisterFailException"/> class.
        /// </summary>
        /// <param name="message">The message<see cref="string"/>.</param>
        /// <param name="e">The e<see cref="Exception"/>.</param>
        public HotKeyRegisterFailException(string message, Exception e) : base(message, e)
        {
        }

        #endregion
    }
}
