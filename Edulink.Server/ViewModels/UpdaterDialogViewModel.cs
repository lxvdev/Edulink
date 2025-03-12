using Edulink.Classes;
using Edulink.Models;
using Edulink.MVVM;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Edulink.ViewModels
{
    public class UpdaterDialogViewModel : ClosableViewModel
    {
        public string CurrentVersion => $"{Assembly.GetExecutingAssembly().GetName().Version}";

        public string Product => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;

        private bool _checkForUpdates = App.SettingsManager.Settings.CheckForUpdates;
        public bool CheckForUpdates
        {
            get => _checkForUpdates;
            set
            {
                App.SettingsManager.Settings.CheckForUpdates = value;
                App.SettingsManager.Save();
                _checkForUpdates = value;
                OnPropertyChanged();
            }
        }

        private ReleaseDetails _releaseDetails;
        public ReleaseDetails ReleaseDetails
        {
            get => _releaseDetails;
            set
            {
                _releaseDetails = value;
                OnPropertyChanged();
            }
        }

        private bool _isCheckingUpdates;
        public bool IsCheckingUpdates
        {
            get => _isCheckingUpdates;
            set
            {
                if (_isCheckingUpdates != value)
                {
                    _isCheckingUpdates = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool? IsUpdateAvailable
        {
            get => App.UpdateAvailable;
            set
            {
                App.UpdateAvailable = value;
                OnPropertyChanged();
            }
        }

        private bool _isUpdating = false;
        public bool IsUpdating
        {
            get => _isUpdating;
            set
            {
                if (_isUpdating != value)
                {
                    _isUpdating = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _progress = 0;
        public int Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _updateFinished = false;
        public bool UpdateFinished
        {
            get => _updateFinished;
            set
            {
                if (_updateFinished != value)
                {
                    _updateFinished = value;
                    OnPropertyChanged();
                }
            }
        }

        public UpdaterDialogViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }

            IsCheckingUpdates = true;
            OpenUpdater.GetLatestVersionAsync().ContinueWith(version =>
            {
                try
                {
                    if (version.Result != null)
                    {
                        ReleaseDetails = version.Result;
                        IsUpdateAvailable = OpenUpdater.IsUpdateAvailable(ReleaseDetails);
                        IsCheckingUpdates = false;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            CommandManager.InvalidateRequerySuggested();
                        });
                    }
                }
                catch (Exception)
                {
                    IsCheckingUpdates = false;
                }
            });
        }

        public UpdaterDialogViewModel(ReleaseDetails updateDetails)
        {
            ReleaseDetails = updateDetails;
            IsUpdateAvailable = OpenUpdater.IsUpdateAvailable(ReleaseDetails);
        }

        public ICommand ShowInBrowserCommand => new RelayCommand(execute => Process.Start(ReleaseDetails.ReleasesUrl), canExecute => ReleaseDetails?.ReleasesUrl != null);

        public ICommand UpdateCommand => new RelayCommand(execute => Update(), canExecute => ReleaseDetails?.DownloadUrl != null && IsUpdateAvailable == true && !IsUpdating);
        private void Update()
        {
            try
            {
                IsUpdating = true;

                string tempFilePath = Path.Combine(Path.GetTempPath(), Assembly.GetExecutingAssembly().GetName().Name);
                string updateFilePath = Path.Combine(tempFilePath, "update.exe");

                if (!Directory.Exists(tempFilePath))
                {
                    Directory.CreateDirectory(tempFilePath);
                }

                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += (s, e) =>
                    {
                        Progress = e.ProgressPercentage;
                    };

                    webClient.DownloadFileCompleted += (s, e) =>
                    {
                        if (e.Error == null)
                        {
                            UpdateFinished = true;
                            Process.Start(updateFilePath, "/SILENT /SUPPRESSMSGBOXES");
                            OnRequestClose();
                        }
                        else
                        {
                            Debug.WriteLine($"Download error: {e.Error.Message}");
                        }

                        IsUpdating = false;
                    };

                    webClient.DownloadFileAsync(new Uri(ReleaseDetails.DownloadUrl), updateFilePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update error: {ex.Message}");
                IsUpdating = false;
            }
        }
    }
}
