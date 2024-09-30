using Edulink.Classes;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Edulink.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex mutex;
        public static SettingsManager configManager = new SettingsManager();
        public static Client client;

        private static System.Windows.Forms.NotifyIcon _notifyIcon;
        private static System.Windows.Forms.ToolStripLabel statusToolStripLabel;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;

            mutex = new Mutex(true, "EdulinkClient_Mutex", out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("Another instance of this application is already running.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                Current.Shutdown();
            }
            StartOnBoot();
            base.OnStartup(e);

            SetLanguageDictionary(configManager.Settings.Language);
            InitializeContextMenuStrip();
            if (configManager.Settings?.IPAddress == null || configManager.Settings?.Name == null)
            {
                FirstStepsWindow firstStepsWindow = new FirstStepsWindow();
                firstStepsWindow.ShowDialog();
            }
            ConnectionLoopAsync();
        }

        private async void ConnectionLoopAsync()
        {
            while (true)
            {
                try
                {
                    client = new Client(configManager.Settings.IPAddress, configManager.Settings.Port, configManager.Settings.Name);
                    if (await client.ConnectAsync())
                    {
                        Console.WriteLine("Connected to server");
                        statusToolStripLabel.Text = (string)Current?.Resources?["ToolStripStatusConnected"];
                        await client.ListenForCommandsAsync();
                    }
                    else
                    {
                        Console.WriteLine("Failed to connect. Retrying in 5 seconds...");
                        statusToolStripLabel.Text = (string)Current?.Resources?["ToolStripStatusDisconnected"];
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                await Task.Delay(5000);
            }
        }

        private static bool VerifyAdminRights()
        {
            string verifierPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AdminVerifier.exe");
            ProcessStartInfo startInfo = new ProcessStartInfo(verifierPath, "verify");
            startInfo.Verb = "runas";
            startInfo.UseShellExecute = true;
            startInfo.CreateNoWindow = true;

            try
            {
                using (Process proc = Process.Start(startInfo))
                {
                    proc.WaitForExit();
                    return proc.ExitCode == 0;
                }
            }
            catch (Win32Exception)
            {
                MessageBox.Show("Admin verification was canceled by the user.");
                return false;
            }
        }

        public static void SetLanguageDictionary(string locale = null)
        {
            ResourceDictionary dict = new ResourceDictionary();
            if (locale != null)
            {
                try
                {
                    dict.Source = new Uri($"..\\Languages\\{locale}.xaml", UriKind.Relative);
                }
                catch (Exception)
                {
                    Console.WriteLine("Could not find the specified language.");
                }

            }
            else
            {
                switch (CultureInfo.InstalledUICulture.ToString())
                {

                    case "en-US":
                        dict.Source = new Uri("..\\Languages\\en-US.xaml", UriKind.Relative);
                        configManager.Settings.Language = CultureInfo.InstalledUICulture.ToString();
                        break;
                    case "es-ES":
                        dict.Source = new Uri("..\\Languages\\es-ES.xaml", UriKind.Relative);
                        configManager.Settings.Language = CultureInfo.InstalledUICulture.ToString();
                        break;
                    case "ro-RO":
                        dict.Source = new Uri("..\\Languages\\ro-RO.xaml", UriKind.Relative);
                        configManager.Settings.Language = CultureInfo.InstalledUICulture.ToString();
                        break;
                    case "pl-PL":
                        dict.Source = new Uri("..\\Languages\\pl-PL.xaml", UriKind.Relative);
                        configManager.Settings.Language = CultureInfo.InstalledUICulture.ToString();
                        break;
                    default:
                        dict.Source = new Uri("..\\Languages\\en-US.xaml", UriKind.Relative);
                        configManager.Settings.Language = "en-US";
                        break;
                }
            }
            Current.Resources.MergedDictionaries.Add(dict);
            InitializeContextMenuStrip();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            mutex.ReleaseMutex();
            mutex.Dispose();
            base.OnExit(e);
        }

        private void StartOnBoot()
        {
            try
            {
                using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    object value = regKey.GetValue(Assembly.GetExecutingAssembly().GetName().Name);
                    if (value == null || !value.ToString().Equals(Assembly.GetEntryAssembly().Location, StringComparison.OrdinalIgnoreCase))
                    {
                        regKey.SetValue(Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetEntryAssembly().Location);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Could not set up start on boot", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private static void InitializeContextMenuStrip()
        {
            _notifyIcon?.Dispose();
            _notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = Edulink.Properties.Resources.EduLink_Client,
                Text = "EduLink",
                ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip { ShowImageMargin = false },
                Visible = true,
            };
            _notifyIcon.ContextMenuStrip.Items.Add((string)Current.Resources["ToolStripSettings"], null, (s, _) =>
            {
                if (VerifyAdminRights())
                {
                    SettingsWindow settingsWindow = new SettingsWindow();
                    settingsWindow.Show();
                }
            });

            _notifyIcon.ContextMenuStrip.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            _notifyIcon.ContextMenuStrip.Items.Add((string)Current.Resources["ToolStripAbout"], null, (s, _) =>
            {
                Current.Dispatcher.Invoke(() =>
                {
                    AboutWindow aboutWindow = new AboutWindow();
                    aboutWindow.ShowDialog();
                });
            });
            _notifyIcon.ContextMenuStrip.Items.Add(new System.Windows.Forms.ToolStripSeparator());

            statusToolStripLabel = new System.Windows.Forms.ToolStripLabel { Text = (string)Current.Resources["ToolStripStatusDisconnected"] };

            _notifyIcon.ContextMenuStrip.Items.Add(statusToolStripLabel);
            _notifyIcon.ContextMenuStrip.Items.Add(new System.Windows.Forms.ToolStripLabel { Text = "V" + Assembly.GetExecutingAssembly().GetName().Version, ForeColor = System.Drawing.Color.Gray });
            _notifyIcon.ContextMenuStrip.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            _notifyIcon.ContextMenuStrip.Items.Add((string)Current.Resources["ToolStripExit"], null, (s, _) =>
            {
                if (VerifyAdminRights())
                {
                    CloseApp();
                }
            });
        }
        public static void RestartApp()
        {
            Process.Start(ResourceAssembly.Location);
            Current.Shutdown();
        }

        public static void CloseApp()
        {
            Current.Shutdown();
        }
    }
}
