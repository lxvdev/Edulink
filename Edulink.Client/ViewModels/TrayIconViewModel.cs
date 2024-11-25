using Edulink.Client;
using Edulink.Commands;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Input;

namespace Edulink.ViewModels
{
    public class TrayIconViewModel : INotifyPropertyChanged
    {
        private string _connectionStatus;

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set
            {
                if (_connectionStatus != value)
                {
                    _connectionStatus = value;
                    OnPropertyChanged(nameof(ConnectionStatus));
                }
            }
        }

        public string Version => $"v{Assembly.GetExecutingAssembly().GetName().Version}";
        //public string ConnectionStatus = (string)App.Current.TryFindResource((bool)(App.Client?.Connected) ? "TrayContextMenu.Status.Connected" : "TrayContextMenu.Status.Disconnected");

        public ICommand SettingsCommand => new RelayCommand(OpenSettings);
        public ICommand AboutCommand => new RelayCommand(OpenAbout);
        public ICommand ExitCommand => new RelayCommand(ExitApplication);

        private void OpenSettings()
        {
            if (App.VerifyAdminRights())
            {
                SettingsWindow settingsWindow = new SettingsWindow();
                settingsWindow.Show();
            }
        }

        private void OpenAbout()
        {
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.Show();
        }

        private void ExitApplication()
        {
            if (App.VerifyAdminRights())
            {
                App.CloseApp();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
