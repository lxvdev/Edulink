using Edulink.TCPHelper;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace Edulink.Client
{
    public class Client
    {
        private TcpClient tcpClient;
        public TcpHelper helper;
        private string ipAddress;
        private int port;
        private string name;
        public bool Connected => tcpClient != null && tcpClient.Connected;
        public Client(string ipAddress, int port, string name)
        {
            this.ipAddress = ipAddress;
            this.port = port;
            this.name = name;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(ipAddress, port);
                helper = new TcpHelper(tcpClient, 1024);

                await helper.SendTextAsync(name);

                string response = await helper.ReceiveTextWithTimeoutAsync(6000);
                if (string.IsNullOrEmpty(response))
                {
                    helper.Close();
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task ListenForCommandsAsync()
        {
            while (true)
            {
                try
                {
                    (string command, string argument) = await helper.ReceiveCommandAsync();

                    if (string.IsNullOrEmpty(command))
                    {
                        throw new Exception("Connection lost");
                    }

                    Console.WriteLine($"Command received: {command}");
                    await HandleCommand(command, argument);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool LockWorkStation();
        private async Task HandleCommand(string command, string argument)
        {
            try
            {
                switch (command)
                {
                    case "Link":
                        OpenLink(argument);
                        break;
                    case "SendMessage":
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBoxDialog messageBoxDialog = new MessageBoxDialog(argument);
                            messageBoxDialog.Show();
                        });
                        break;
                    case "DesktopPreview":
                        Bitmap bitmapImage = CaptureScreenshot();
                        await helper.SendCommandAsync(command);
                        await helper.SendImageAsync(bitmapImage);
                        break;
                    case "RestartApp":
                        App.RestartApp();
                        break;
                    case "Shutdown":
                        ShutdownComputer();
                        break;
                    case "Lockscreen":
                        LockWorkStation();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling command: {ex.Message}");
            }
        }

        private Bitmap CaptureScreenshot()
        {
            var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
            }

            return screenshot;
        }

        private void OpenLink(string url)
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

        private void ShutdownComputer()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "/s /t 0",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
        }

        public void Close()
        {
            helper?.Close();
        }
    }
}
