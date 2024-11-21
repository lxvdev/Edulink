using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Edulink.Classes
{
    public static class SystemUtility
    {
        //Imports
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "LockWorkStation")]
        public static extern bool Lockscreen();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        public const uint EWX_LOGOFF = 0x00000000; // Log off the user
        public const uint EWX_FORCE = 0x00000004; // Force running applications to close

        public static Bitmap CaptureScreenshot(int width = 0, int height = 0)
        {
            Rectangle bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;

            int targetWidth = width > 0 ? width : bounds.Width;
            int targetHeight = height > 0 ? height : bounds.Height;

            float aspectRatio = (float)bounds.Width / bounds.Height;

            if (width > 0 && height > 0)
            {
                float targetAspectRatio = (float)width / height;
                if (targetAspectRatio > aspectRatio)
                {
                    targetWidth = (int)(height * aspectRatio);
                }
                else
                {
                    targetHeight = (int)(width / aspectRatio);
                }
            }

            using (Bitmap fullScreenshot = new Bitmap(bounds.Width, bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            {
                using (Graphics g = Graphics.FromImage(fullScreenshot))
                {
                    g.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
                }

                if (targetWidth == bounds.Width && targetHeight == bounds.Height)
                {
                    return new Bitmap(fullScreenshot);
                }

                Bitmap resizedScreenshot = new Bitmap(targetWidth, targetHeight);

                using (Graphics g = Graphics.FromImage(resizedScreenshot))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(fullScreenshot, 0, 0, targetWidth, targetHeight);
                }

                return resizedScreenshot;
            }
        }

        public static void OpenLink(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            else
            {
                Console.WriteLine("Invalid URL provided.");
            }
        }

        public static void ShutdownComputer()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "/s /f /t 0",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
        }

        public static void RestartComputer()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "/r /t 0",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
        }

        public static void LogOffUser()
        {
            if (!ExitWindowsEx(EWX_LOGOFF | EWX_FORCE, 0))
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Exception("Log off failed. Error code: " + errorCode);
            }
        }
    }
}
