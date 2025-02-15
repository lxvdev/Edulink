using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Edulink.Classes
{
    public static class DarkTitleBar
    {
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        public static void Apply(Window window, bool enable = true)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            IntPtr hWnd = new WindowInteropHelper(window).Handle;
            if (hWnd == IntPtr.Zero) return;

            int useDarkMode = enable ? 1 : 0;
            int attribute = Environment.OSVersion.Version.Build >= 19041
                ? DWMWA_USE_IMMERSIVE_DARK_MODE  // Windows 10 20H1+ and Windows 11
                : DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1; // Older Windows 10 versions

            DwmSetWindowAttribute(hWnd, attribute, ref useDarkMode, sizeof(int));
        }
    }
}
