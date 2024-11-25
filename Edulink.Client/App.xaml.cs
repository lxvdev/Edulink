using Edulink.Classes;
using Edulink.TCPHelper.Classes;
using Edulink.ViewModels;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Edulink.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex mutex;
        public static SettingsManager SettingsManager = new SettingsManager(AppDomain.CurrentDomain.BaseDirectory);
        public static Client Client;

        private static TaskbarIcon _taskbarIcon;
        private TrayIconViewModel _trayIconViewModel;

        private static System.Windows.Forms.NotifyIcon _notifyIcon;
        private static System.Windows.Forms.ToolStripLabel statusToolStripLabel;

        bool allowMultipleInstances = false; // Change before releasing: For debugging

        protected override async void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            mutex = new Mutex(true, "Edulink_Client_Mutex", out createdNew);

            if (!createdNew && !allowMultipleInstances)
            {
                MessageDialog.Show((string)Current.TryFindResource("Message.Content.AnotherInstanceIsAlreadyRunning"), null, MessageDialogButtons.Ok, MessageDialogType.Information);
                CloseApp();
            }
            else
            {
                CheckApplicationTask();
                base.OnStartup(e);
                SetLanguageDictionary(SettingsManager.Settings.Language);
                InitializeTrayIcon();
                //InitializeContextMenuStrip();
                if (SettingsManager.Settings?.IPAddress == null || SettingsManager.Settings?.Name == null || SettingsManager.Settings?.Port > 9999)
                {
                    FirstStepsWindow firstStepsWindow = new FirstStepsWindow();
                    firstStepsWindow.ShowDialog();
                }
                await ConnectionLoopAsync();
            }
        }

        // Old startup method

        //private void CheckBoot()
        //{
        //    string appLocation = Assembly.GetEntryAssembly().Location;
        //    string appName = Assembly.GetExecutingAssembly().GetName().Name;

        //    bool SetRegistryKey(RegistryKey baseKey, string key, string value)
        //    {
        //        try
        //        {
        //            using (RegistryKey regKey = baseKey.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
        //            {
        //                if (regKey != null)
        //                {
        //                    // Set the value in the registry
        //                    regKey.SetValue(appName, appLocation);
        //                    return true; // Successful write
        //                }
        //            }
        //        }
        //        catch (UnauthorizedAccessException)
        //        {
        //            // Handle the case where writing is not permitted due to lack of privileges
        //            return false; // Failed due to permissions
        //        }
        //        catch (Exception ex)
        //        {
        //            // Log or display any other exceptions
        //            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        }
        //        return false; // Failed for some other reason
        //    }

        //    // Try to set the registry key in LocalMachine
        //    if (!SetRegistryKey(Registry.LocalMachine, appName, appLocation))
        //    {
        //        // If setting the key fails, restart the app with admin privileges
        //        RestartAsAdmin();
        //    }
        //}

        public void CheckApplicationTask()
        {
            string taskName = "EdulinkClientTask";
            using (TaskSchedulerHelper taskScheduler = new TaskSchedulerHelper())
            {
                try
                {
                    if (taskScheduler.TaskExistsWithCorrectPath(taskName, Assembly.GetExecutingAssembly().Location) == false)
                    {
                        taskScheduler.CreateLoginTask(taskName, "A task for starting Edulink Client on logon.", Assembly.GetExecutingAssembly().Location);
                    }
                }
                catch (Exception)
                {
                    RestartAsAdmin();
                }
            }
        }

        private async Task ConnectionLoopAsync()
        {
            while (true)
            {
                try
                {
                    using (Client = new Client(SettingsManager.Settings.IPAddress, SettingsManager.Settings.Port, SettingsManager.Settings.Name))
                    {
                        if (await Client.ConnectAsync())
                        {
                            Console.WriteLine("Connected to server");
                            _trayIconViewModel.ConnectionStatus = (string)Current.TryFindResource("TrayContextMenu.Status.Connected");
                            Client.CommandReceived += Client_CommandReceivedAsync;
                            await Client.ListenForCommandsAsync();
                        }
                        else
                        {
                            Console.WriteLine("Failed to connect. Retrying in 5 seconds...");
                            _trayIconViewModel.ConnectionStatus = (string)Current.TryFindResource("TrayContextMenu.Status.Disconnected");
                            Client.CommandReceived -= Client_CommandReceivedAsync;
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                await Task.Delay(5000);
            }
        }

        private async void Client_CommandReceivedAsync(object sender, EdulinkCommand command)
        {
            try
            {
                switch (command.Command)
                {
                    case "LINK":
                        SystemUtility.OpenLink(command.Parameters["Link"]);
                        break;
                    case "MESSAGE":
                        _ = Current.Dispatcher.Invoke(async () =>
                        {
                            MessageDialogResult messageDialogResult = MessageDialog.Show(command.Parameters["Message"],
                                                                                         (string)Current.TryFindResource("Message.TitleBar"),
                                                                                         MessageDialogButtons.OkReply, MessageDialogType.Information);

                            if (messageDialogResult == MessageDialogButtonResult.Reply && !string.IsNullOrEmpty(messageDialogResult.ReplyResult))
                            {
                                await Client.Helper.SendCommandAsync(new EdulinkCommand
                                {
                                    Command = command.Command,
                                    Parameters = new Dictionary<string, string>
                                {
                                    { "Message", messageDialogResult.ReplyResult }
                                }
                                });
                            }
                        });
                        break;
                    case "DESKTOP":
                    case "PREVIEW":
                        int width = command.Parameters.ContainsKey("Width") ? int.Parse(command.Parameters["Width"]) : 0;
                        int height = command.Parameters.ContainsKey("Height") ? int.Parse(command.Parameters["Height"]) : 0;

                        using (Bitmap bitmap = SystemUtility.CaptureScreenshot(width, height))
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                byte[] bitmapData = ms.ToArray();

                                await Client.Helper.SendCommandAsync(new EdulinkCommand { Command = command.Command, Content = bitmapData });
                            }
                        }
                        break;
                    case "RESTARTAPP":
                        App.RestartApp();
                        break;
                    case "SHUTDOWN":
                        SystemUtility.ShutdownComputer();
                        break;
                    case "RESTART":
                        SystemUtility.RestartComputer();
                        break;
                    case "LOCKSCREEN":
                        SystemUtility.Lockscreen();
                        break;
                    case "LOGOFF":
                        SystemUtility.LogOffUser();
                        break;
                    case "CHANGENAME":
                        SettingsManager.Settings.Name = command.Parameters["Name"];
                        break;
                    case "UPDATE":
                        _ = Task.Run(() => StartUpdate());
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling command: {ex.Message}");

            }
        }

        private async Task StartUpdate()
        {
            using (NamedPipeClientStream namedPipeClient = new NamedPipeClientStream(".", "EdulinkUpdaterPipe", PipeDirection.Out))
            {
                Task connectTask = Task.Run(() => namedPipeClient.Connect());
                if (await Task.WhenAny(connectTask, Task.Delay(15000)) == connectTask)
                {
                    using (StreamWriter writer = new StreamWriter(namedPipeClient) { AutoFlush = true })
                    {
                        await writer.WriteLineAsync("UPDATE");
                    }
                    CloseApp();
                }
                else
                {
                    MessageBox.Show("Connection to service timed out. Make sure the service is running.", "Connection Timeout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RestartAsAdmin()
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                FileName = Assembly.GetExecutingAssembly().Location,
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                Process.Start(processStartInfo);
                CloseApp();
            }
            catch (Exception ex)
            {
                MessageDialog.Show((string)Current.TryFindResource("Message.Content.FailedToRestartAsAdmin"), null, MessageDialogButtons.Ok, MessageDialogType.Error);
            }
        }

        public static bool VerifyAdminRights()
        {
            if (PasswordDialog.Show(PasswordDialogType.EnterPassword) is bool result)
            {
                return result;
            }
            return false;
        }

        private void InitializeTrayIcon()
        {
            _trayIconViewModel = new TrayIconViewModel();
            _taskbarIcon = new TaskbarIcon
            {
                Icon = Edulink.Properties.Resources.Edulink_Client,
                ToolTipText = "Edulink",
                DataContext = _trayIconViewModel,
                ContextMenu = (ContextMenu)Current.TryFindResource("TrayContextMenu")
            };
        }

        // Can someone help me fix this? It just doesnt feel right and theres a memory leak :(
        public static void SetLanguageDictionary(string locale = null)
        {
            ResourceDictionary dictionary = new ResourceDictionary();
            if (locale != null)
            {
                try
                {
                    dictionary.Source = new Uri($"..\\Languages\\{locale}.xaml", UriKind.Relative);
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
                        dictionary.Source = new Uri("..\\Languages\\en-US.xaml", UriKind.Relative);
                        SettingsManager.Settings.Language = CultureInfo.InstalledUICulture.ToString();
                        break;
                    case "es-ES":
                        dictionary.Source = new Uri("..\\Languages\\es-ES.xaml", UriKind.Relative);
                        SettingsManager.Settings.Language = CultureInfo.InstalledUICulture.ToString();
                        break;
                    case "ro-RO":
                        dictionary.Source = new Uri("..\\Languages\\ro-RO.xaml", UriKind.Relative);
                        SettingsManager.Settings.Language = CultureInfo.InstalledUICulture.ToString();
                        break;
                    case "pl-PL":
                        dictionary.Source = new Uri("..\\Languages\\pl-PL.xaml", UriKind.Relative);
                        SettingsManager.Settings.Language = CultureInfo.InstalledUICulture.ToString();
                        break;
                    default:
                        dictionary.Source = new Uri("..\\Languages\\en-US.xaml", UriKind.Relative);
                        SettingsManager.Settings.Language = "en-US";
                        break;
                }
            }
            Current.Resources.MergedDictionaries.Add(dictionary);
        }

        // Old contextmenu

        //private static void InitializeContextMenuStrip()
        //{
        //    _notifyIcon?.Dispose();
        //    _notifyIcon = new System.Windows.Forms.NotifyIcon
        //    {
        //        Icon = Edulink.Properties.Resources.Edulink_Client,
        //        Text = "Edulink",
        //        ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip { ShowImageMargin = false },
        //        Visible = true,
        //    };
        //    _notifyIcon.ContextMenuStrip.Items.Add((string)Current.Resources["ToolStripSettings"], null, (s, _) =>
        //    {
        //        if (VerifyAdminRights())
        //        {
        //            SettingsWindow settingsWindow = new SettingsWindow();
        //            settingsWindow.Show();
        //        }
        //    });

        //    _notifyIcon.ContextMenuStrip.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        //    _notifyIcon.ContextMenuStrip.Items.Add((string)Current.Resources["ToolStripAbout"], null, (s, _) =>
        //    {
        //        Current.Dispatcher.Invoke(() =>
        //        {
        //            AboutDialog aboutWindow = new AboutDialog();
        //            aboutWindow.ShowDialog();
        //        });
        //    });
        //    _notifyIcon.ContextMenuStrip.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        //    statusToolStripLabel = new System.Windows.Forms.ToolStripLabel { Text = (string)Current.Resources["ToolStripStatusDisconnected"] };

        //    _notifyIcon.ContextMenuStrip.Items.Add(statusToolStripLabel);
        //    _notifyIcon.ContextMenuStrip.Items.Add(new System.Windows.Forms.ToolStripLabel { Text = "V" + Assembly.GetExecutingAssembly().GetName().Version, ForeColor = System.Drawing.Color.Gray });
        //    _notifyIcon.ContextMenuStrip.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        //    _notifyIcon.ContextMenuStrip.Items.Add((string)Current.Resources["ToolStripExit"], null, (s, _) =>
        //    {
        //        if (VerifyAdminRights())
        //        {
        //            CloseApp();
        //        }
        //    });
        //}

        public static void RestartApp()
        {
            Process.Start(ResourceAssembly.Location);
            Current.Shutdown();
        }

        public static void CloseApp()
        {
            Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            mutex?.ReleaseMutex();
            mutex?.Dispose();
            base.OnExit(e);
        }
    }
}
