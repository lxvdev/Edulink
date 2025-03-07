using Edulink.Classes;
using Edulink.Communication.Models;
using Edulink.Models;
using Edulink.ViewModels;
using Edulink.Views;
using Hardcodet.Wpf.TaskbarNotification;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
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
        public static Client Client;

        private readonly PaletteHelper _paletteHelper = new PaletteHelper();

        private TrayIconViewModel _trayIconViewModel;

        private static Mutex mutex;
        bool allowMultipleInstances = false; // Change before releasing: For debugging

        #region Update properties
        public static event EventHandler IsUpdateAvailableChanged;

        private bool? _isUpdateAvailable = null;
        public bool? IsUpdateAvailable
        {
            get => _isUpdateAvailable;
            set
            {
                if (_isUpdateAvailable != value)
                {
                    _isUpdateAvailable = value;
                    IsUpdateAvailableChanged?.Invoke(this, EventArgs.Empty);
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

            if (e.Args.Length > 1 && e.Args[0] == "--apply-settings")
            {
                SettingsManager.Load(e.Args[1]);
                SettingsManager.Save(true);
                Environment.Exit(0);
                return;
            }
            else if (e.Args.Length >= 1 && e.Args[0] == "--create-task")
            {
                CheckApplicationTask();
                Environment.Exit(0);
                return;
            }
            else if (e.Args.Length >= 1 && e.Args[0] == "--startup")
            {
                // Foolproof way to check if the application is already running because anyone with the enough knowledge can bypass the mutex
                Process process = Process.GetCurrentProcess();

                string currentProcessPath = process.MainModule.FileName;

                bool isAlreadyRunning = Process.GetProcesses()
                    .Where(p => p.Id != process.Id)
                    .Any(p =>
                    {
                        try
                        {
                            return p.MainModule.FileName.Equals(currentProcessPath, StringComparison.OrdinalIgnoreCase);
                        }
                        catch
                        {
                            return false;
                        }
                    });

                if (isAlreadyRunning)
                {
                    Environment.Exit(0);
                    return;
                }
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

            ApplyTheme(SettingsManager.Settings.Theme);

            if (SettingsManager.Settings.CheckForUpdates)
            {
                _ = CheckForUpdates();
            }

            InitializeTrayIcon();

            if (SettingsManager.Settings?.IPAddress == null || SettingsManager.Settings?.Name == null)
            {
                FirstStepsWindow firstStepsWindow = new FirstStepsWindow();
                firstStepsWindow.ShowDialog();
            }
            await ConnectionLoopAsync();
        }

        private async Task CheckForUpdates()
        {
            IsUpdateAvailable = null;

            ReleaseDetails releaseDetails = await OpenUpdater.GetLatestVersionAsync();
            IsUpdateAvailable = OpenUpdater.IsUpdateAvailable(releaseDetails);
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
                        RestartAsAdmin(arguments);
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
                    using (Client = new Client(SettingsManager.Settings.IPAddress, SettingsManager.Settings.Port, SettingsManager.Settings.Name))
                    {
                        if (await Client.ConnectAsync())
                        {
                            Console.WriteLine("Connected to server");
                            _trayIconViewModel.ConnectionStatus = true;
                            Client.CommandReceived += Client_CommandReceivedAsync;
                            await Client.ListenForCommandsAsync();
                        }
                        else
                        {
                            Console.WriteLine("Failed to connect. Retrying in 5 seconds...");
                            _trayIconViewModel.ConnectionStatus = false;
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

        private InputBlocker _inputBlocker = new InputBlocker();
        private async void Client_CommandReceivedAsync(object sender, EdulinkCommand command)
        {
            try
            {
                switch (command.Command)
                {
                    case Commands.Link:
                        SystemUtility.OpenLink(command.Parameters["URL"]);
                        break;
                    case Commands.Message:
                        HandleMessage(command);
                        break;
                    case Commands.Desktop:
                    case Commands.Preview:
                        HandleDesktop(command);
                        break;
                    case Commands.RestartApplication:
                        App.RestartApp();
                        break;
                    case Commands.Shutdown:
                        SystemUtility.ShutdownComputer();
                        break;
                    case Commands.Restart:
                        SystemUtility.RestartComputer();
                        break;
                    case Commands.LockScreen:
                        SystemUtility.Lockscreen();
                        break;
                    case Commands.LogOff:
                        SystemUtility.LogOffUser();
                        break;
                    case Commands.ResetPassword:
                        SettingsManager.Settings.Password = null;
                        SettingsManager.Save();
                        break;
                    case Commands.BlockInput:
                        if (bool.TryParse(command.Parameters["Block"], out bool block))
                        {
                            _inputBlocker.BlockKeyboard(block);
                            _inputBlocker.BlockMouse(block);
                        }
                        break;
                    case Commands.RenameComputer:
                        SettingsManager.Settings.Name = command.Parameters["Name"];
                        SettingsManager.Save();
                        RestartApp();
                        break;
                    case Commands.Update:
                        ReleaseDetails releaseDetails = await OpenUpdater.GetLatestVersionAsync();
                        IsUpdateAvailable = OpenUpdater.IsUpdateAvailable(releaseDetails);
                        if (IsUpdateAvailable == true)
                        {
                            UpdaterDialog updateDialog = new UpdaterDialog(releaseDetails, true);
                            updateDialog.Show();
                        }
                        break;
                    case Commands.Disconnect:
                        Client.Dispose();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling command: {ex.Message}");

            }
        }

        private readonly MemoryStream _reusableMemoryStream = new MemoryStream();
        private void HandleDesktop(EdulinkCommand command)
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
                mutex?.ReleaseMutex();
                mutex?.Dispose();
                Client?.Disconnect();
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
