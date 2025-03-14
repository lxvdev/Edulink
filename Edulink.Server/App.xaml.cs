using Edulink.Classes;
using Edulink.Models;
using Edulink.Views;
using Hardcodet.Wpf.TaskbarNotification;
using MaterialDesignThemes.Wpf;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WPFLocalizeExtension.Engine;

namespace Edulink
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static SettingsManager SettingsManager = new SettingsManager();
        private readonly PaletteHelper _paletteHelper = new PaletteHelper();

        public static TaskbarIcon TaskbarIcon;

        public static BalloonTipType ActiveBalloonTipType;

        private Mutex mutex;
        bool allowMultipleInstances = false;

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
            TaskbarIcon = new TaskbarIcon
            {
                Icon = Edulink.Properties.Resources.Edulink_Server,
                ToolTipText = "Edulink",
                NoLeftClickDelay = true
            };

            TaskbarIcon.TrayBalloonTipClicked += TaskbarIcon_TrayBalloonTipClicked;
            // Set up culture
            LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
            LocalizeDictionary.Instance.Culture = !string.IsNullOrEmpty(SettingsManager.Settings.Language) ? new CultureInfo(SettingsManager.Settings.Language) : CultureInfo.InstalledUICulture;
        }

        protected override void OnStartup(StartupEventArgs e)
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
            if (e.Args.Length == 1 && e.Args[0] == "--reset-settings")
            {
                SettingsManager.Reset();
                SettingsManager.Save();
                Environment.Exit(0);
                return;
            }
            else if (e.Args.Length == 1 && e.Args[0] == "--allow-multiple-instances")
            {
                Debug.WriteLine("Allowing multiple instances");
            }
            else
            {
                try
                {
                    mutex = new Mutex(true, "EdulinkServerMutex", out isPrimaryInstance);

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

            base.OnStartup(e);

            if (SettingsManager.Settings.CheckForUpdates)
            {
                _ = CheckForUpdates();
            }
        }

        private void TaskbarIcon_TrayBalloonTipClicked(object sender, RoutedEventArgs e)
        {
            if (ActiveBalloonTipType == BalloonTipType.Update)
            {
                UpdaterDialog updaterDialog = new UpdaterDialog();
                updaterDialog.ShowDialog();
            }
        }

        private async Task CheckForUpdates()
        {
            UpdateAvailable = null;

            ReleaseDetails releaseDetails = await OpenUpdater.GetLatestVersionAsync();
            UpdateAvailable = OpenUpdater.IsUpdateAvailable(releaseDetails);
            if (UpdateAvailable == true)
            {
                ShowBalloonTip(LocalizedStrings.Instance["TaskbarIcon.Title.UpdateAvailable"],
                               string.Format(LocalizedStrings.Instance["TaskbarIcon.Content.UpdateAvailable"], releaseDetails.Version),
                               BalloonIcon.Info, BalloonTipType.Update);
            }
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

        public static void ShowBalloonTip(string title, string message, BalloonIcon symbol, BalloonTipType type)
        {
            switch (type)
            {
                case BalloonTipType.Update:
                    if (SettingsManager.Settings.CheckForUpdates)
                    {
                        ActiveBalloonTipType = type;
                        TaskbarIcon.ShowBalloonTip(title, message, symbol);
                    }
                    break;
                case BalloonTipType.ComputerDisconnected:
                    if (SettingsManager.Settings.DisconnectionNotificationEnabled)
                    {
                        ActiveBalloonTipType = type;
                        TaskbarIcon.ShowBalloonTip(title, message, symbol);
                    }
                    break;
                default:
                    break;
            }

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

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }

    public enum BalloonTipType
    {
        Update,
        ComputerDisconnected
    }
}
