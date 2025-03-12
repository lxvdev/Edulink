using Edulink.Classes;
using Edulink.MVVM;
using Edulink.Views;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WPFLocalizeExtension.Engine;

namespace Edulink.ViewModels
{
    public class SettingsWindowViewModel : ValidatableClosableViewModel
    {
        private SettingsManager _settingsManager = App.SettingsManager;
        private readonly PaletteHelper _paletteHelper = new PaletteHelper();
        private readonly HashSet<string> _unsavedProperties = new HashSet<string>();

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

        private bool _previewEnabled;
        public bool PreviewEnabled
        {
            get => _previewEnabled;
            set
            {
                if (_previewEnabled != value)
                {
                    _previewEnabled = value;
                    OnPropertyChanged();
                    TrackUnsavedChanges(_settingsManager.Settings.PreviewEnabled);
                }
            }
        }

        private double _previewFrequency;
        public double PreviewFrequency
        {
            get => _previewFrequency;
            set
            {
                if (_previewFrequency != value)
                {
                    _previewFrequency = value;
                    OnPropertyChanged();
                    TrackUnsavedChanges(_settingsManager.Settings.PreviewFrequency);
                }
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

        private string _ipAddresses;
        public string IPAddresses => _ipAddresses;

        public SettingsWindowViewModel()
        {
            LoadSettings();
            ValidatePort();
            _ipAddresses = IPAddressProvider.GetIPAddresses();
        }

        public ICommand SaveAndRestartCommand => new RelayCommand(execute => Save(true), canExecute => !HasErrors);

        public ICommand SaveCommand => new RelayCommand(execute => Save(), canExecute => !HasErrors);
        private void Save(bool restart = false)
        {
            try
            {
                _settingsManager.Settings.Port = int.TryParse(Port, out int port) ? port : 0;
                _settingsManager.Settings.PreviewEnabled = PreviewEnabled;
                _settingsManager.Settings.PreviewFrequency = PreviewFrequency;
                _settingsManager.Settings.Language = Language.Equals(CultureInfo.InstalledUICulture) ? null : Language.ToString();
                _settingsManager.Settings.Theme = Theme;
                _settingsManager.Save();

                _unsavedProperties.Clear();

                if (restart)
                    App.RestartApp();
            }
            catch (Exception ex)
            {
                MessageDialog.Show($"{ex.Message}", MessageDialogTitle.Error, MessageDialogButton.Ok, MessageDialogIcon.Error);
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

        private void LoadSettings()
        {
            Port = _settingsManager.Settings.Port == 0 ? string.Empty : _settingsManager.Settings.Port.ToString();
            PreviewEnabled = _settingsManager.Settings.PreviewEnabled;
            PreviewFrequency = _settingsManager.Settings.PreviewFrequency;
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

        private bool HasUnsavedChanges()
        {
            return _unsavedProperties.Count > 0;
        }

        private void TrackUnsavedChanges(object originalValue, [CallerMemberName] string propertyName = null)
        {
            object newValue = GetType().GetProperty(propertyName)?.GetValue(this);

            if (Equals(originalValue, newValue))
            {
                _unsavedProperties.Remove(propertyName);
            }
            else
            {
                _unsavedProperties.Add(propertyName);
            }
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (HasUnsavedChanges())
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
