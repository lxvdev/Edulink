using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;

namespace Edulink.Classes
{
    public static class SystemUtility
    {
        //Imports
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "LockWorkStation")]
        public static extern bool Lockscreen();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        private const uint EWX_LOGOFF = 0x00; // Log off the user
        private const uint EWX_FORCE = 0x04; // Force running applications to close

        public static Bitmap CaptureScreenshot(int width = 0, int height = 0)
        {
            //Rectangle bounds = new Rectangle((int)SystemParameters.VirtualScreenLeft, (int)SystemParameters.VirtualScreenTop,
            //                                 (int)SystemParameters.VirtualScreenWidth, (int)SystemParameters.VirtualScreenHeight);

            Rectangle bounds = new Rectangle(0, 0, (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);

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

            using (Bitmap fullScreenshot = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppRgb))
            {
                // Take screenshot
                using (Graphics graphics = Graphics.FromImage(fullScreenshot))
                {
                    graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
                }

                // If its the desired size then return
                if (targetWidth == bounds.Width && targetHeight == bounds.Height)
                {
                    return new Bitmap(fullScreenshot);
                }

                // Resize screenshot
                Bitmap resizedScreenshot = new Bitmap(targetWidth, targetHeight);

                using (Graphics g = Graphics.FromImage(resizedScreenshot))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
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
                    UseShellExecute = false
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
