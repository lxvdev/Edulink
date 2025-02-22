﻿using Edulink.Classes;
using Edulink.MVVM;
using Edulink.Views;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Input;

namespace Edulink.ViewModels
{
    public class SettingsWindowViewModel : ViewModelBase
    {
        private SettingsManager _settingsManager = App.SettingsManager;
        private readonly PaletteHelper _paletteHelper = new PaletteHelper();

        private string _port;
        public string Port
        {
            get => _port;
            set
            {
                if (_port != value)
                {
                    ClearErrors();

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        AddError("Port cannot be empty.");
                    }
                    else if (!int.TryParse(value, out int intValue) || intValue < 0)
                    {
                        AddError("Invalid port number.");
                    }

                    _port = value;
                    OnPropertyChanged();
                }
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
                }
            }
        }

        private string _language;
        public string Language
        {
            get => _language;
            set
            {
                if (_language != value)
                {
                    _language = value;
                    App.SetLanguage(value);
                    OnPropertyChanged();
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
                }
            }
        }

        private string _ipAddresses;
        public string IPAddresses => _ipAddresses;

        public SettingsWindowViewModel()
        {
            LoadSettings();
            _ipAddresses = GetIpAddresses();
        }

        private string GetIpAddresses()
        {
            IEnumerable<string> ips = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                .SelectMany(nic => nic.GetIPProperties().UnicastAddresses)
                .Where(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork && ip.Address.ToString() != "127.0.0.1")
                .Select(ip => ip.Address.ToString());

            return ips.Any() ? string.Join(", ", ips) : "No active network interfaces";
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
                _settingsManager.Settings.Language = Language;
                _settingsManager.Settings.Theme = Theme;
                _settingsManager.Save();

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
            Language = _settingsManager.Settings.Language;
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
    }
}
