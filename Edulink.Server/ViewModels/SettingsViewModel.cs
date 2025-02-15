using Edulink.Server.Classes;
using Edulink.Server.Models;
using Edulink.Server.MVVM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Edulink.Server.ViewModels
{
    public class SettingsViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private SettingsManager _settingsManager = App.SettingsManager;

        public string Port
        {
            get => _settingsManager.Settings.Port == 0 ? string.Empty : _settingsManager.Settings.Port.ToString();
            set
            {
                ClearErrors(nameof(Port));

                if (string.IsNullOrWhiteSpace(value))
                {
                    _settingsManager.Settings.Port = 0;
                    AddError(nameof(Port), "Port cannot be empty.");
                    OnPropertyChanged();
                }
                else if (int.TryParse(value, out int intValue))
                {
                    _settingsManager.Settings.Port = intValue;
                    if (intValue < 0)
                    {
                        AddError(nameof(Port), "Port cannot be negative.");
                    }
                    else
                    {
                        OnPropertyChanged();
                    }
                }
            }
        }

        public bool PreviewEnabled
        {
            get => _settingsManager.Settings.PreviewEnabled;
            set
            {
                if (_settingsManager.Settings.PreviewEnabled != value)
                {
                    _settingsManager.Settings.PreviewEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public double PreviewFrequency
        {
            get => _settingsManager.Settings.PreviewFrequency;
            set
            {
                if (_settingsManager.Settings.PreviewFrequency != value)
                {
                    _settingsManager.Settings.PreviewFrequency = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Language
        {
            get => _settingsManager.Settings.Language;
            set
            {
                if (_settingsManager.Settings.Language != value)
                {
                    _settingsManager.Settings.Language = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _ipAddresses;
        public string IPAddresses => _ipAddresses;

        public SettingsViewModel()
        {
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

        public RelayCommand SaveAndRestartCommand => new RelayCommand(execute => SaveAndRestart(), canExecute => !HasErrors);
        public void SaveAndRestart()
        {
            Save(true);
        }

        public RelayCommand SaveCommand => new RelayCommand(execute => Save(), canExecute => !HasErrors);
        public void Save(bool restart = false)
        {
            try
            {
                App.SettingsManager.Save();

                if (restart)
                {
                    App.RestartApp();
                }
            }
            catch (Exception ex)
            {
                MessageDialog.Show($"{ex.Message}", MessageDialogTitle.Error, MessageDialogButton.Ok, MessageDialogIcon.Error);
            }
        }

        public RelayCommand ResetCommand => new RelayCommand(execute => Reset());
        public void Reset()
        {
            _settingsManager.Reset();
            OnPropertyChanged(nameof(Settings));
        }

        public RelayCommand RestartCommand => new RelayCommand(execute => Restart());
        public void Restart()
        {
            App.RestartApp();
        }

        public RelayCommand ExitCommand => new RelayCommand(execute => Exit());
        public void Exit()
        {
            App.CloseApp();
        }

        #region INotifyDataErrorInfo
        private readonly Dictionary<string, List<string>> _propertyErrors = new Dictionary<string, List<string>>();

        public bool HasErrors => _propertyErrors.Any();

        public IEnumerable GetErrors(string propertyName)
        {
            if (_propertyErrors.TryGetValue(propertyName, out List<string> errors))
            {
                return errors;
            }
            return null;
        }

        public void AddError(string propertyName, string errorMessage)
        {
            if (!_propertyErrors.ContainsKey(propertyName))
            {
                _propertyErrors[propertyName] = new List<string>();
            }

            _propertyErrors[propertyName].Add(errorMessage);
            OnErrorsChanged(propertyName);
        }

        public void ClearErrors(string propertyName)
        {
            if (_propertyErrors.Remove(propertyName))
            {
                OnErrorsChanged(propertyName);
            }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            OnPropertyChanged(nameof(HasErrors));
        }
        #endregion
    }
}
