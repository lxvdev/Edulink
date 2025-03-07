using Edulink.Classes;
using Edulink.MVVM;
using Edulink.Views;
using System;
using System.ComponentModel;
using System.Windows.Input;
using WPFLocalizeExtension.Engine;

namespace Edulink.ViewModels
{
    public class FirstStepsWindowViewModel : ViewModelBase
    {
        private SettingsManager _settingsManager = App.SettingsManager;

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

        public string PasswordButtonText => string.IsNullOrEmpty(App.SettingsManager.Settings?.Password) ? "FirstSteps.Password.SetPassword" : "FirstSteps.Password.ChangePassword";

        public FirstStepsWindowViewModel()
        {
            _name = _settingsManager.Settings.Name;
            _ipAddress = _settingsManager.Settings.IPAddress;
            _port = _settingsManager.Settings.Port == 0 ? string.Empty : _settingsManager.Settings.Port.ToString();

            LocalizeDictionary.Instance.PropertyChanged += LocalizeDictionary_PropertyChanged;
        }

        private void LocalizeDictionary_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LocalizeDictionary.Instance.Culture))
            {
                OnPropertyChanged(nameof(PasswordButtonText));
            }
        }

        public ICommand PasswordCommand => new RelayCommand(execute => Password());
        private void Password()
        {
            PasswordDialog passwordDialog = new PasswordDialog(PasswordDialogType.SetOrChangePassword);
            if (passwordDialog.ShowDialog() == true)
                OnPropertyChanged(nameof(PasswordButtonText));
        }

        public ICommand FinishCommand => new RelayCommand(execute => Finish(), canExecute => CanFinish());

        private void Finish()
        {
            if (string.IsNullOrEmpty(App.SettingsManager.Settings.Password))
            {
                MessageDialogResult confirmation = MessageDialog.ShowLocalized("Message.Content.NoPasswordSet", MessageDialogTitle.Warning, MessageDialogButton.YesNo, MessageDialogIcon.Warning);
                if (confirmation.ButtonResult == MessageDialogButtonResult.Yes)
                {
                    PasswordCommand.Execute(null);
                }
                else if (confirmation.ButtonResult == MessageDialogButtonResult.None)
                {
                    return;
                }
            }

            App.SettingsManager.Settings.Name = _name;
            App.SettingsManager.Settings.IPAddress = _ipAddress;
            App.SettingsManager.Settings.Port = int.Parse(_port);

            App.SettingsManager.Save();

            OnRequestClose();
        }

        private bool CanFinish()
        {
            return !string.IsNullOrEmpty(_name) && !string.IsNullOrEmpty(_ipAddress) && !string.IsNullOrEmpty(_port) && !HasErrors;
        }

        public event EventHandler RequestClose;
        protected virtual void OnRequestClose()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
