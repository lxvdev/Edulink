using Edulink.Classes;
using Edulink.Communication.Models;
using Edulink.Converters;
using Edulink.Models;
using Edulink.ViewModels;
using Edulink.Views;
using Hardcodet.Wpf.TaskbarNotification;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WPFLocalizeExtension.Engine;

namespace Edulink
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static SettingsManager SettingsManager = new SettingsManager();
        public static Client Client = new Client();

        public static SendFileWindow SendFileWindow;

        private readonly PaletteHelper _paletteHelper = new PaletteHelper();

        private TrayIconViewModel _trayIconViewModel;

        private Mutex mutex;
        bool allowMultipleInstances = false; // Change before releasing: For debugging

        #region Update properties
        public static event EventHandler UpdateAvailableChanged;

        private static bool? _updateAvailable = null;
        public static bool? UpdateAvailable
        {
            get => _updateAvailable;
            set
            {
                if (_updateAvailable != value)
                {
                    _updateAvailable = value;
                    UpdateAvailableChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }
        #endregion

        public App()
        {
            // Set up culture
            LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
            LocalizeDictionary.Instance.Culture = !string.IsNullOrEmpty(SettingsManager.Settings.Language) ? new CultureInfo(SettingsManager.Settings.Language) : CultureInfo.InstalledUICulture;
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            bool isPrimaryInstance;

            ApplyTheme(SettingsManager.Settings.Theme);

            if (e.Args.Length > 1 && e.Args[0] == "--apply-settings")
            {
                SettingsManager.Load(e.Args[1]);

                if (!SettingsManager.Save(noRetry: true))
                {
                    Environment.Exit(1);
                }

                Environment.Exit(0);
                return;
            }
            else if (e.Args.Length == 1 && e.Args[0] == "--create-task")
            {
                CheckApplicationTask();
                Environment.Exit(0);
                return;
            }
            else if (e.Args.Length == 1 && e.Args[0] == "--startup")
            {
                mutex = new Mutex(true, "EdulinkClientMutex", out isPrimaryInstance);

                // Foolproof way to check if the application is already running because anyone with the enough knowledge can bypass the mutex
                //Process process = Process.GetCurrentProcess();

                //string currentProcessPath = process.MainModule.FileName;

                //bool isAlreadyRunning = Process.GetProcesses()
                //    .Where(p => p.Id != process.Id)
                //    .Any(p =>
                //    {
                //        try
                //        {
                //            return p.MainModule.FileName.Equals(currentProcessPath, StringComparison.OrdinalIgnoreCase);
                //        }
                //        catch
                //        {
                //            return false;
                //        }
                //    });

                if (!isPrimaryInstance && !allowMultipleInstances)
                {
                    Environment.Exit(0);
                    return;
                }
            }
            else if (e.Args.Length == 1 && e.Args[0] == "--allow-multiple-instances")
            {
                Debug.WriteLine("Allowing multiple instances");
            }
            else
            {
                try
                {
                    mutex = new Mutex(true, "EdulinkClientMutex", out isPrimaryInstance);

                    if (!isPrimaryInstance && !allowMultipleInstances)
                    {
                        MessageDialog.ShowLocalized("Message.Content.AlreadyRunning", MessageDialogTitle.Information, MessageDialogButton.Ok, MessageDialogIcon.Information);
                        Environment.Exit(0);
                        return;
                    }
                }
                catch (AbandonedMutexException)
                {
                    isPrimaryInstance = true;
                }
            }

            CheckApplicationTask();

            base.OnStartup(e);

            InitializeTrayIcon();

            if (SettingsManager.Settings?.IPAddress == null || SettingsManager.Settings?.Name == null)
            {
                SetupWindow setupWindow = new SetupWindow();
                setupWindow.ShowDialog();
            }

            if (SettingsManager.Settings.CheckForUpdates)
            {
                await CheckForUpdates();
            }

            await ConnectionLoopAsync();
        }

        private async Task CheckForUpdates()
        {
            try
            {
                UpdateAvailable = null;

                ReleaseDetails releaseDetails = await OpenUpdater.GetLatestVersionAsync();
                UpdateAvailable = OpenUpdater.IsUpdateAvailable(releaseDetails);
            }
            catch (Exception)
            {
                Debug.WriteLine("Error checking for updates");
            }
        }

        public void CheckApplicationTask()
        {
            string taskName = "EdulinkClientTask";
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            using (TaskSchedulerHelper taskScheduler = new TaskSchedulerHelper())
            {
                try
                {
                    if (taskScheduler.TaskExistsWithCorrectPath(taskName, exePath) == false)
                    {
                        taskScheduler.CreateLoginTask(taskName, "A task for starting Edulink Client on logon.", exePath, false, "--startup");
                    }
                }
                catch (Exception)
                {
                    string arguments = "--create-task";

                    ProcessStartInfo processStartInfo = new ProcessStartInfo()
                    {
                        FileName = Assembly.GetExecutingAssembly().Location,
                        UseShellExecute = true,
                        Arguments = arguments,
                        Verb = "runas"
                    };

                    try
                    {
                        Process.Start(processStartInfo);
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine("Could not create startup task");
                    }
                }
            }
        }

        private async Task ConnectionLoopAsync()
        {
            while (true)
            {
                try
                {
                    Client.CommandReceived += Client_CommandReceivedAsync;

                    Client.IPAddress = SettingsManager.Settings.IPAddress;
                    Client.Port = SettingsManager.Settings.Port;
                    Client.Name = SettingsManager.Settings.Name;

                    if (await Client.ConnectAsync())
                    {
                        Console.WriteLine("Connected to server");
                        await Client.ListenForCommandsAsync();
                    }
                    else
                    {
                        Console.WriteLine("Failed to connect. Retrying in 5 seconds...");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                await Task.Delay(5000);
            }
        }

        #region Handle commands
        private InputBlocker _inputBlocker = new InputBlocker();
        private async void Client_CommandReceivedAsync(object sender, EdulinkCommand edulinkCommand)
        {
            try
            {
                string command = edulinkCommand.Command;
                if (command == Commands.Link.ToString())
                {
                    SystemUtility.OpenLink(edulinkCommand.Parameters["URL"]);
                }
                else if (command == Commands.Message.ToString())
                {
                    HandleMessage(edulinkCommand);
                }
                else if (command == Commands.ViewDesktop.ToString() ||
                         command == Commands.Preview.ToString())
                {
                    HandleScreenshot(edulinkCommand);
                }
                else if (command == Commands.RestartApplication.ToString())
                {
                    RestartApp();
                }
                else if (command == Commands.Shutdown.ToString())
                {
                    SystemUtility.ShutdownComputer();
                }
                else if (command == Commands.Restart.ToString())
                {
                    SystemUtility.RestartComputer();
                }
                else if (command == Commands.LockScreen.ToString())
                {
                    SystemUtility.Lockscreen();
                }
                else if (command == Commands.LogOff.ToString())
                {
                    SystemUtility.LogOffUser();
                }
                else if (command == Commands.ResetPassword.ToString())
                {
                    SettingsManager.Settings.Password = null;
                    SettingsManager.Save();
                }
                else if (command == Commands.BlockInput.ToString())
                {
                    if (bool.TryParse(edulinkCommand.Parameters["Block"], out bool block))
                    {
                        _inputBlocker.BlockKeyboard(block);
                        _inputBlocker.BlockMouse(block);
                    }
                }
                else if (command == Commands.RenameComputer.ToString())
                {
                    SettingsManager.Settings.Name = edulinkCommand.Parameters["Name"];
                    if (SettingsManager.Save())
                    {
                        RestartApp();
                    }
                }
                else if (command == Commands.Update.ToString())
                {
                    ReleaseDetails releaseDetails = await OpenUpdater.GetLatestVersionAsync();
                    UpdateAvailable = OpenUpdater.IsUpdateAvailable(releaseDetails);
                    if (UpdateAvailable == true)
                    {
                        UpdaterDialog updateDialog = new UpdaterDialog(releaseDetails, true);
                        updateDialog.Show();
                    }
                }
                else if (command == Commands.ToggleMute.ToString())
                {
                    AudioControl.ToggleMuteAudio();
                }
                else if (command == Commands.ComputerList.ToString())
                {
                    HandleComputerList(edulinkCommand);
                }
                else if (command == Commands.RequestSendFile.ToString())
                {
                    Computer sourceComputer = JsonConvert.DeserializeObject<Computer>(edulinkCommand.Parameters["SourceComputer"]);

                    if (sourceComputer == null)
                    {
                        Debug.WriteLine("Source computer is null.");
                        return;
                    }

                    long fileLength = long.Parse(edulinkCommand.Parameters["FileLength"]);

                    ReceiveFileWindow receiveFileWindow = new ReceiveFileWindow(sourceComputer, edulinkCommand.Parameters["FileName"], fileLength);
                    receiveFileWindow.Show();
                }
                else if (command == Commands.ResponseSendFile.ToString())
                {
                    bool.TryParse(edulinkCommand.Parameters["Accepted"], out bool accepted);

                    if (accepted)
                    {
                        IEnumerable<IPEndPoint> ipAddresses = JsonConvert.DeserializeObject<IEnumerable<IPEndPoint>>(edulinkCommand.Parameters["IP"], new IPEndPointConverter());
                        SendFileWindow?.HandleResponse(accepted, ipAddresses);
                    }
                    else
                    {
                        SendFileWindow?.HandleResponse(accepted);
                    }
                }
                else if (command == Commands.Disconnect.ToString())
                {
                    Client.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling command: {ex.Message}");
            }
        }

        private void HandleComputerList(EdulinkCommand edulinkCommand)
        {
            if (SendFileWindow != null)
            {
                bool success = false;
                bool sendTeacher = false;

                // Parse parameters
                if (edulinkCommand.Parameters.ContainsKey("Success"))
                {
                    bool.TryParse(edulinkCommand.Parameters["Success"], out success);
                }

                if (edulinkCommand.Parameters.ContainsKey("SendTeacher"))
                {
                    bool.TryParse(edulinkCommand.Parameters["SendTeacher"], out sendTeacher);
                }

                // If successful, deserialize the list of computers and update the UI
                if (success && edulinkCommand.Parameters.ContainsKey("List"))
                {
                    List<Computer> computers = JsonConvert.DeserializeObject<List<Computer>>(edulinkCommand.Parameters["List"]);
                    SendFileWindow.UpdateList(computers);
                }

                SendFileWindow.SetSharingStatus(success, sendTeacher);
            }
        }

        private readonly MemoryStream _reusableMemoryStream = new MemoryStream();
        private void HandleScreenshot(EdulinkCommand command)
        {
            int width = command.Parameters.ContainsKey("Width") ? int.Parse(command.Parameters["Width"]) : 0;
            int height = command.Parameters.ContainsKey("Height") ? int.Parse(command.Parameters["Height"]) : 0;

            using (Bitmap bitmap = SystemUtility.CaptureScreenshot(width, height))
            {
                _reusableMemoryStream.SetLength(0);

                bitmap.Save(_reusableMemoryStream, System.Drawing.Imaging.ImageFormat.Png);

                byte[] bitmapData = _reusableMemoryStream.ToArray();

                _ = Client.Helper.SendCommandAsync(new EdulinkCommand { Command = command.Command, Content = bitmapData });
            }
        }

        private void HandleMessage(EdulinkCommand command)
        {

            MessageDialogResult messageDialogResult = MessageDialog.Show(command.Parameters["Message"],
                                                                         LocalizedStrings.Instance["Message.Title.MessageFromTeacher"],
                                                                         MessageDialogButton.OkReply, MessageDialogIcon.None);

            if (messageDialogResult.ButtonResult == MessageDialogButtonResult.Reply && !string.IsNullOrEmpty(messageDialogResult.ReplyResult))
            {
                _ = Client.Helper.SendCommandAsync(new EdulinkCommand
                {
                    Command = command.Command,
                    Parameters = new Dictionary<string, string>
                    {
                        { "Message", messageDialogResult.ReplyResult }
                    }
                });
            }
        }
        #endregion

        public static bool ValidateCredentials()
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
            TaskbarIcon _taskbarIcon = new TaskbarIcon
            {
                Icon = Edulink.Properties.Resources.Edulink_Client,
                ToolTipText = "Edulink",
                DataContext = _trayIconViewModel,
                ContextMenu = (ContextMenu)Current.TryFindResource("TrayContextMenu")
            };
        }

        private void ApplyTheme(string theme)
        {
            Theme materialTheme = _paletteHelper.GetTheme();

            if (theme.Equals("Dark", StringComparison.OrdinalIgnoreCase))
            {
                materialTheme.SetBaseTheme(BaseTheme.Dark);
            }
            else if (theme.Equals("Light", StringComparison.OrdinalIgnoreCase))
            {
                materialTheme.SetBaseTheme(BaseTheme.Light);
            }
            else if (theme.Equals("Auto", StringComparison.OrdinalIgnoreCase))
            {
                materialTheme.SetBaseTheme(BaseTheme.Inherit);
            }

            _paletteHelper.SetTheme(materialTheme);
        }

        public static void RestartApp()
        {
            Current.Shutdown();
            Process.Start(ResourceAssembly.Location);
        }

        public static void CloseApp()
        {
            Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                Client.CommandReceived -= Client_CommandReceivedAsync;
                mutex?.ReleaseMutex();
                mutex?.Dispose();
                Client?.Dispose();
                _inputBlocker?.Dispose();
            }
            catch (Exception)
            {
                // There's nothing we can do
            }
            base.OnExit(e);
        }
    }
}
