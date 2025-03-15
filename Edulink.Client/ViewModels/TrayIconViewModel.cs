using Edulink.Classes;
using Edulink.Communication.Models;
using Edulink.Models;
using Edulink.MVVM;
using Edulink.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using WPFLocalizeExtension.Engine;

namespace Edulink.ViewModels
{
    public class TrayIconViewModel : ViewModelBase
    {
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

        public bool ConnectionStatus => App.Client.Connected;

        public string ConnectionStatusText => ConnectionStatus ? "TrayContextMenu.Connection.Status.Connected" : "TrayContextMenu.Connection.Status.Disconnected";

        public string ComputerName => !string.IsNullOrEmpty(App.SettingsManager.Settings.Name) ? App.SettingsManager.Settings.Name : LocalizedStrings.Instance["TrayContextMenu.NoName"];

        public bool? UpdateAvailable => App.UpdateAvailable;
        public string Version => $"v{Assembly.GetExecutingAssembly().GetName().Version}";

        public TrayIconViewModel()
        {
            App.Client.ConnectionStatusChanged += Client_ConnectionStatusChanged;
            LocalizeDictionary.Instance.PropertyChanged += LocalizeDictionary_PropertyChanged;
            App.UpdateAvailableChanged += App_IsUpdateAvailableChanged;
        }

        private void Client_ConnectionStatusChanged(object sender, bool e)
        {
            OnPropertyChanged(nameof(ConnectionStatus));
            OnPropertyChanged(nameof(ConnectionStatusText));
            CommandManager.InvalidateRequerySuggested();
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
                OnPropertyChanged(nameof(ComputerName));
            }
        }

        bool messagesAllowed = true;

        public ICommand SendMessageCommand => new RelayCommand(execute => SendMessage(), canExecute => ConnectionStatus && messagesAllowed);
        private void SendMessage()
        {
            InputDialogResult dialogResult = InputDialog.Show(LocalizedStrings.Instance["Input.Content.SendMessage"], LocalizedStrings.Instance["Input.Title.SendMessage"]);
            if (dialogResult.ButtonResult == InputDialogButtonResult.Ok && !string.IsNullOrEmpty(dialogResult.InputResult))
            {
                _ = App.Client.Helper.SendCommandAsync(new EdulinkCommand
                {
                    Command = Commands.Message.ToString(),
                    Parameters = new Dictionary<string, string>
                    {
                        { "Message", dialogResult.InputResult }
                    }
                });
            }
        }

        private void ShowWindow<T>(ref T window) where T : Window, new()
        {
            if (window == null || !window.IsVisible)
            {
                window?.Close();
                window = new T();
                window.Show();
            }
            else
            {
                window.WindowState = WindowState.Normal;
                window.Activate();
            }
        }

        public ICommand SendFileCommand => new RelayCommand(execute => ShowWindow(ref App.SendFileWindow), canExecute => ConnectionStatus && messagesAllowed);

        private SettingsWindow _settingsWindow;
        public ICommand SettingsCommand => new RelayCommand(execute => OpenSettings());

        private void OpenSettings()
        {
            if (App.ValidateCredentials())
            {
                ShowWindow(ref _settingsWindow);
            }
        }

        private UpdaterDialog _updaterDialog;
        public ICommand UpdaterCommand => new RelayCommand(execute => ShowWindow(ref _updaterDialog));

        private AboutDialog _aboutDialog;
        public ICommand AboutCommand => new RelayCommand(execute => ShowWindow(ref _aboutDialog));

        public ICommand RestartApplicationCommand => new RelayCommand(execute => RestartApplication());
        private void RestartApplication()
        {
            if (App.ValidateCredentials())
            {
                App.RestartApp();
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
