using Edulink.MVVM;
using Edulink.Views;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using WPFLocalizeExtension.Engine;

namespace Edulink.ViewModels
{
    public class TrayIconViewModel : ViewModelBase
    {
        private bool _connectionStatus = false;
        public bool ConnectionStatus
        {
            get => _connectionStatus;
            set
            {
                if (_connectionStatus != value)
                {
                    _connectionStatus = value;
                    OnPropertyChanged(nameof(ConnectionStatus));
                    OnPropertyChanged(nameof(ConnectionStatusText));
                }
            }
        }

        public string UpdaterStatus
        {
            get
            {
                if (UpdateAvailable == true)
                {
                    return "TrayContextMenu.Updater.Available";
                }
                else if (UpdateAvailable == false)
                {
                    return "TrayContextMenu.Updater.UpToDate";
                }
                else
                {
                    return "TrayContextMenu.Updater";
                }
            }
        }

        public string ConnectionStatusText => _connectionStatus ? "TrayContextMenu.Connection.Status.Connected" : "TrayContextMenu.Connection.Status.Disconnected";

        public string ComputerName => App.SettingsManager.Settings.Name;

        public bool? UpdateAvailable => ((App)Application.Current).IsUpdateAvailable;
        public string Version => $"v{Assembly.GetExecutingAssembly().GetName().Version}";

        public TrayIconViewModel()
        {
            LocalizeDictionary.Instance.PropertyChanged += LocalizeDictionary_PropertyChanged;
            App.IsUpdateAvailableChanged += App_IsUpdateAvailableChanged;
        }

        private void App_IsUpdateAvailableChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(UpdateAvailable));
            OnPropertyChanged(nameof(UpdaterStatus));
        }

        private void LocalizeDictionary_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LocalizeDictionary.Instance.Culture))
            {
                OnPropertyChanged(nameof(ConnectionStatusText));
                OnPropertyChanged(nameof(UpdaterStatus));
            }
        }

        SettingsWindow settingsWindow;

        public ICommand SettingsCommand => new RelayCommand(execute => OpenSettings());
        private void OpenSettings()
        {
            if (App.ValidateCredentials())
            {
                if (settingsWindow == null)
                {
                    settingsWindow = new SettingsWindow();
                    settingsWindow.Closing += (sender, e) => settingsWindow = null;
                    settingsWindow.Show();
                }
            }
        }

        UpdaterDialog updaterDialog;

        public ICommand UpdaterCommand => new RelayCommand(execute => OpenUpdater());
        private void OpenUpdater()
        {
            if (updaterDialog == null)
            {
                updaterDialog = new UpdaterDialog();
                updaterDialog.Closing += (sender, e) => updaterDialog = null;
                updaterDialog.Show();
            }
        }

        AboutDialog aboutDialog;

        public ICommand AboutCommand => new RelayCommand(execute => OpenAbout());
        private void OpenAbout()
        {
            if (aboutDialog == null)
            {
                aboutDialog = new AboutDialog();
                aboutDialog.Closing += (sender, e) => aboutDialog = null;
                aboutDialog.Show();
            }
        }

        public ICommand ExitCommand => new RelayCommand(execute => ExitApplication());
        private void ExitApplication()
        {
            if (App.ValidateCredentials())
            {
                App.CloseApp();
            }
        }
    }
}
