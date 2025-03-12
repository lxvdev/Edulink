using System;
using System.Runtime.InteropServices;

namespace Edulink.Classes
{
    public class AudioControl
    {
        private const uint WM_APPCOMMAND = 0x0319;
        private const uint APPCOMMAND_MUTE = 0x80000;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetForegroundWindow();

        public static void ToggleMuteAudio()
        {
            IntPtr hwnd = GetForegroundWindow();
            SendMessage(hwnd, WM_APPCOMMAND, hwnd, (IntPtr)APPCOMMAND_MUTE);
        }
    }
}
