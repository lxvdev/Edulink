using Edulink.Classes;
using Edulink.MVVM;
using Edulink.Views;
using MaterialDesignThemes.Wpf;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using WPFLocalizeExtension.Engine;

namespace Edulink.ViewModels
{
    public class SettingsWindowViewModel : TrackableValidatableClosableViewModel
    {
        private SettingsManager _settingsManager = App.SettingsManager;
        private readonly PaletteHelper _paletteHelper = new PaletteHelper();

        private readonly SnackbarMessageQueue _snackbarMessageQueue = new SnackbarMessageQueue();
        public ISnackbarMessageQueue SnackbarMessageQueue => _snackbarMessageQueue;

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    ValidateName();
                    OnPropertyChanged();
                    TrackUnsavedChanges(_settingsManager.Settings.Name);
                }
            }
        }

        private void ValidateName()
        {
            ClearErrors(nameof(Name));

            if (string.IsNullOrWhiteSpace(_name))
            {
                AddError("Name cannot be empty.", nameof(Name));
            }
        }

        private string _ipAddress;
        public string IPAddress
        {
            get => _ipAddress;
            set
            {
                if (_ipAddress != value)
                {
                    _ipAddress = value;
                    ValidateIPAddress();
                    OnPropertyChanged();
                    TrackUnsavedChanges(_settingsManager.Settings.IPAddress);
                }
            }
        }

        private void ValidateIPAddress()
        {
            ClearErrors(nameof(IPAddress));

            if (string.IsNullOrWhiteSpace(_ipAddress))
            {
                AddError("IP Address cannot be empty.", nameof(IPAddress));
            }
            else if (!System.Net.IPAddress.TryParse(_ipAddress, out _))
            {
                AddError("Invalid IP Address format.", nameof(IPAddress));
            }
        }

        private string _port;
        public string Port
        {
            get => _port;
            set
            {
                if (_port != value)
                {
                    _port = value;
                    ValidatePort();
                    OnPropertyChanged();
                    TrackUnsavedChanges(_settingsManager.Settings.Port == 0 ? string.Empty : _settingsManager.Settings.Port.ToString());
                }
            }
        }

        private void ValidatePort()
        {
            ClearErrors(nameof(Port));

            if (string.IsNullOrWhiteSpace(_port))
            {
                AddError("Port cannot be empty.", nameof(Port));
            }
            else if (!int.TryParse(_port, out int intValue) || intValue < 0)
            {
                AddError("Invalid port number.", nameof(Port));
            }
        }

        private CultureInfo _language;
        public CultureInfo Language
        {
            get => _language;
            set
            {
                if (_language != value)
                {
                    _language = value;
                    LocalizeDictionary.Instance.Culture = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PasswordButtonText));
                    TrackUnsavedChanges(!string.IsNullOrEmpty(_settingsManager.Settings.Language) ? new CultureInfo(_settingsManager.Settings.Language) : CultureInfo.InstalledUICulture);
                }
            }
        }

        private string _theme;
        public string Theme
        {
            get => _theme;
            set
            {
                if (_theme != value)
                {
                    _theme = value;
                    if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                    {
                        ApplyTheme(value);
                    }
                    OnPropertyChanged();
                    TrackUnsavedChanges(_settingsManager.Settings.Theme);
                }
            }
        }

        public string PasswordButtonText => string.IsNullOrEmpty(App.SettingsManager.Settings?.Password) ? "Settings.Password.SetPassword" : "Settings.Password.ChangePassword";

        public SettingsWindowViewModel()
        {
            LoadSettings();
            ValidateName();
            ValidateIPAddress();
            ValidatePort();
        }

        public ICommand SaveAndRestartCommand => new RelayCommand(execute => Save(true), canExecute => !HasErrors);

        public ICommand SaveCommand => new RelayCommand(execute => Save(), canExecute => !HasErrors);
        private void Save(bool restart = false)
        {
            try
            {
                _settingsManager.Settings.Name = Name;
                _settingsManager.Settings.IPAddress = IPAddress;
                _settingsManager.Settings.Port = int.TryParse(Port, out int port) ? port : 0;
                _settingsManager.Settings.Language = Language.Equals(CultureInfo.InstalledUICulture) ? null : Language.ToString();
                _settingsManager.Settings.Theme = Theme;
                _settingsManager.Save();

                ClearUnsavedChanges();

                _snackbarMessageQueue.Enqueue(LocalizedStrings.Instance["Settings.Snackbar.Saved"], new PackIcon { Kind = PackIconKind.Close }, () => { });

                if (restart)
                    App.RestartApp();
            }
            catch (Exception ex)
            {
                MessageDialog.Show(ex.Message, MessageDialogTitle.Error, MessageDialogButton.Ok, MessageDialogIcon.Error);
            }
        }

        public ICommand ResetCommand => new RelayCommand(execute => Reset());
        private void Reset()
        {
            _settingsManager.Reset();

            LoadSettings();
        }

        public ICommand RestartApplicationCommand => new RelayCommand(execute => App.RestartApp());

        public ICommand ExitCommand => new RelayCommand(execute => App.CloseApp());

        public ICommand PasswordCommand => new RelayCommand(execute => Password());
        private void Password()
        {
            if (PasswordDialog.Show(PasswordDialogType.SetOrChangePassword) is bool result && result)
            {
                OnPropertyChanged(nameof(PasswordButtonText));
            }
        }

        private void LoadSettings()
        {
            Name = _settingsManager.Settings.Name;
            IPAddress = _settingsManager.Settings.IPAddress;
            Port = _settingsManager.Settings.Port == 0 ? string.Empty : _settingsManager.Settings.Port.ToString();
            Language = !string.IsNullOrEmpty(_settingsManager.Settings.Language) ? new CultureInfo(_settingsManager.Settings.Language) : CultureInfo.InstalledUICulture;
            Theme = _settingsManager.Settings.Theme;
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

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (HasUnsavedChanges)
            {
                MessageDialogResult dialogResult = MessageDialog.ShowLocalized("Message.Content.AreYouSureYouWantToLeaveWithoutSavingChanges", MessageDialogTitle.Warning, MessageDialogButton.YesNo, MessageDialogIcon.Warning);
                if (dialogResult.ButtonResult == MessageDialogButtonResult.Yes)
                {
                    LoadSettings();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
