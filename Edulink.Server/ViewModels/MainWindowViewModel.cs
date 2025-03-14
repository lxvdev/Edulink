using Edulink.Classes;
using Edulink.Communication.Models;
using Edulink.Core;
using Edulink.Models;
using Edulink.MVVM;
using Edulink.Views;
using Hardcodet.Wpf.TaskbarNotification;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Edulink.ViewModels
{
    public class MainWindowViewModel : ClosableViewModel
    {
        private ObservableCollection<Client> _clients = new ObservableCollection<Client>();
        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set
            {
                _clients = value;
                OnPropertyChanged(nameof(Clients));
            }
        }

        public bool IsBroadcast
        {
            get => Clients.Any() && Clients.Where(client => client.IsVisible).Any() && Clients.Where(client => client.IsVisible).All(client => client.IsSelected);
            set
            {
                Clients.Where(client => client.IsVisible).ToList().ForEach(client => client.IsSelected = value);
                OnPropertyChanged(nameof(IsBroadcast));
                OnPropertyChanged(nameof(SelectedClients));
            }
        }

        public List<Client> SelectedClients
        {
            get => Clients.Where(client => client.IsVisible && client.IsSelected).ToList();
            set
            {
                foreach (Client client in Clients)
                {
                    client.IsSelected = value.Any(selected => selected == client);
                }
                OnPropertyChanged(nameof(SelectedClients));
                OnPropertyChanged(nameof(IsBroadcast));
            }
        }

        public bool HasClients => Clients.Any();

        public int ShowMainContent => HasClients ? 1 : 0;

        public int ClientsCount => Clients.Count;

        private readonly SnackbarMessageQueue _snackbarMessageQueue = new SnackbarMessageQueue();
        public ISnackbarMessageQueue SnackbarMessageQueue => _snackbarMessageQueue;

        public MainWindowViewModel(Server server)
        {
            _clients.CollectionChanged += Clients_CollectionChanged;
            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;

            ApplySorting();
            StartPreviewUpdates();
        }

        #region Sorting
        public ICollectionView ClientsCollectionView => CollectionViewSource.GetDefaultView(Clients);

        private string _sortProperty = nameof(Client.Name);
        public string SortProperty
        {
            get => _sortProperty;
            set
            {
                _sortProperty = value;
                OnPropertyChanged();
                ApplySorting();
            }
        }

        private ListSortDirection _sortDirection = ListSortDirection.Ascending;
        public ListSortDirection SortDirection
        {
            get => _sortDirection;
            set
            {
                _sortDirection = value;
                OnPropertyChanged();
                ApplySorting();
            }
        }

        private void ApplySorting()
        {
            ClientsCollectionView.SortDescriptions.Clear();
            if (!string.IsNullOrEmpty(SortProperty))
            {
                ClientsCollectionView.SortDescriptions.Add(new SortDescription(SortProperty, SortDirection));
            }
        }
        #endregion

        #region Preview updates
        // TODO: Stop or start preview timer instantly on setting change
        private Timer _previewTimer;
        public void StartPreviewUpdates()
        {
            if (App.SettingsManager.Settings.PreviewEnabled && App.SettingsManager.Settings.PreviewFrequency > 1000)
            {
                _previewTimer = new Timer(PreviewTimerCallback, null, 0, (int)App.SettingsManager.Settings.PreviewFrequency);
            }
        }

        private Rectangle DesktopPreviewResolution { get; } = new Rectangle(0, 0, 240, 135);
        private void PreviewTimerCallback(object state = null)
        {
            Clients.Where(client => client.IsVisible && !client.IsExcludedFromPreview).ToList()
                   .ForEach(client => RequestPreview(client));
        }

        private void RequestPreview(Client client)
        {
            if (App.SettingsManager.Settings.PreviewEnabled)
            {
                _ = client.Helper.SendCommandAsync(new EdulinkCommand
                {
                    Command = Commands.Preview.ToString(),
                    Parameters = new Dictionary<string, string>
                    {
                        { "Width", DesktopPreviewResolution.Width.ToString() },
                        { "Height", DesktopPreviewResolution.Height.ToString() }
                    }
                });
            }
        }
        #endregion

        #region Property updates
        private void Clients_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasClients));
            OnPropertyChanged(nameof(ShowMainContent));

            if (e.NewItems != null)
            {
                foreach (Client client in e.NewItems)
                {
                    client.PropertyChanged += Client_PropertyChanged;
                    // I dont like this but it works
                    //Task.Delay(200).ContinueWith(task => RequestPreview(client));
                    // I fixed it!!!
                    _ = Task.Run(() =>
                    {
                        RequestPreview(client);
                    });
                }
            }

            if (e.OldItems != null)
            {
                foreach (Client client in e.OldItems)
                {
                    client.PropertyChanged -= Client_PropertyChanged;
                }
            }

            OnPropertyChanged(nameof(IsBroadcast));
            OnPropertyChanged(nameof(SelectedClients));
            OnPropertyChanged(nameof(ClientsCount));
        }

        private void Client_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Client.IsSelected) || e.PropertyName == nameof(Client.IsVisible))
            {
                OnPropertyChanged(nameof(IsBroadcast));
                OnPropertyChanged(nameof(SelectedClients));
            }
        }
        #endregion

        #region Handle connection/disconnection
        private void Server_ClientConnected(object sender, Server.ClientConnectedEventArgs e)
        {
            _clients.Add(e.Client);
        }

        private void Server_ClientDisconnected(object sender, Server.ClientDisconnectedEventArgs e)
        {
            _clients.Remove(e.Client);
            App.ShowBalloonTip(LocalizedStrings.Instance["TaskbarIcon.Title.ComputerDisconnected"],
                               string.Format(LocalizedStrings.Instance["TaskbarIcon.Content.ComputerDisconnected"], e.Client?.Name ?? "???"),
                               BalloonIcon.Info, BalloonTipType.ComputerDisconnected);
        }
        #endregion

        #region MenuItems
        public ICommand RefreshCommand => new RelayCommand(execute => RefreshList());
        private void RefreshList()
        {
            PreviewTimerCallback();
            _snackbarMessageQueue.Clear();
            _snackbarMessageQueue.Enqueue(LocalizedStrings.Instance["Main.Refreshed"], new PackIcon { Kind = PackIconKind.Close }, () => { });
        }

        public ICommand ExitCommand => new RelayCommand(execute => OnRequestClose());

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
                window.Focus();
            }
        }

        public ICommand AboutCommand => new RelayCommand(execute => ShowAbout());
        private AboutDialog _aboutDialog;
        private void ShowAbout()
        {
            ShowWindow(ref _aboutDialog);
        }

        public ICommand SettingsCommand => new RelayCommand(execute => ShowSettings());
        private SettingsWindow _settingsWindow;
        private void ShowSettings()
        {
            ShowWindow(ref _settingsWindow);
        }

        public ICommand UpdaterCommand => new RelayCommand(execute => ShowUpdater());
        private UpdaterDialog _updaterDialog;
        private void ShowUpdater()
        {
            ShowWindow(ref _updaterDialog);
        }
        #endregion

        #region Commands
        private async Task<bool> SendCommandAsync(EdulinkCommand command, List<Client> clients = null)
        {
            if (clients?.Any() != true)
            {
                MessageDialog.ShowLocalized("Message.Content.NoClientsSelected", MessageDialogTitle.Error, MessageDialogButton.Ok, MessageDialogIcon.Error);
                return false;
            }

            await Task.WhenAll(clients.Select(client =>
                    client.Helper.SendCommandAsync(new EdulinkCommand { Command = command.Command, Parameters = command.Parameters })));

            return true;
        }

        #region Link command
        public ICommand LinkCommand => new RelayCommand(execute => HandleLink(), canExecute => CanExecuteLink());

        private void HandleLink()
        {
            InputDialogResult urlInputDialogResult = InputDialog.ShowLocalized("Input.Content.SendLink", LocalizedStrings.Instance["Input.Title.SendLink"]);

            if (urlInputDialogResult.ButtonResult == InputDialogButtonResult.Ok && !string.IsNullOrEmpty(urlInputDialogResult.InputResult))
            {
                _ = SendCommandAsync(new EdulinkCommand
                {
                    Command = Commands.Link.ToString(),
                    Parameters =
                    {
                        { "URL", PrepareUrl(urlInputDialogResult.InputResult) }
                    }
                }, SelectedClients);
            }
        }

        private bool CanExecuteLink()
        {
            return SelectedClients.Any() && SelectedClients.All(client => client.Version >= Commands.Link.MinimumVersion);
        }

        private string PrepareUrl(string url)
        {
            url = url.Trim();
            url = HttpUtility.UrlPathEncode(url);

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            // Caused some problems
            //if (!Regex.IsMatch(url, @"\.[a-z]{2,}$"))
            //{
            //    url += ".com";
            //}

            return url;
        }
        #endregion

        #region Message command
        public ICommand MessageCommand => new RelayCommand(execute => HandleMessage(), canExecute => CanExecuteMessage());

        private void HandleMessage()
        {
            InputDialogResult messageInputDialog = InputDialog.ShowLocalized("Input.Content.SendMessage", LocalizedStrings.Instance["Input.Title.SendMessage"]);

            if (messageInputDialog.ButtonResult == InputDialogButtonResult.Ok && !string.IsNullOrEmpty(messageInputDialog.InputResult))
            {
                _ = SendCommandAsync(new EdulinkCommand
                {
                    Command = Commands.Message.ToString(),
                    Parameters = {
                        { "Message", messageInputDialog.InputResult }
                    }
                }, SelectedClients);
            }
        }

        private bool CanExecuteMessage()
        {
            return SelectedClients.Any() && SelectedClients.All(client => client.Version >= Commands.Message.MinimumVersion);
        }
        #endregion

        #region View desktop command
        public List<DesktopDialog> openDesktopDialogs = new List<DesktopDialog>();

        public ICommand ViewDesktopCommand => new RelayCommand(execute => HandleViewDesktop(), canExecute => CanExecuteViewDesktop());

        private void HandleViewDesktop()
        {
            SelectedClients.ForEach(async client => await RequestDesktopAsync(client));
        }

        private async Task RequestDesktopAsync(Client client)
        {
            DesktopDialog existingDialog = openDesktopDialogs.FirstOrDefault(dialog => dialog.Client == client);

            if (existingDialog != null)
            {
                existingDialog.Focus();
            }
            else
            {
                DesktopDialog desktopPreviewDialog = new DesktopDialog(client);
                openDesktopDialogs.Add(desktopPreviewDialog);
                desktopPreviewDialog.Closed += (s, _) => openDesktopDialogs.Remove(desktopPreviewDialog);
                desktopPreviewDialog.Show();
            }

            await client.Helper.SendCommandAsync(new EdulinkCommand { Command = Commands.ViewDesktop.ToString() });
        }

        private bool CanExecuteViewDesktop()
        {
            return SelectedClients.Any() && SelectedClients.All(client => client.Version >= Commands.ViewDesktop.MinimumVersion);
        }
        #endregion

        #region Simple command
        public ICommand SimpleCommand => new RelayCommand(execute => HandleSimpleCommand((Command)execute), canExecute => CanExecuteSimpleCommand((Command)canExecute));

        private void HandleSimpleCommand(Command command)
        {
            _ = SendCommandAsync(new EdulinkCommand { Command = command.ToString() }, SelectedClients);
        }

        private bool CanExecuteSimpleCommand(Command command)
        {
            return SelectedClients.Any() && SelectedClients.All(client => client.Version >= command.MinimumVersion);
        }
        #endregion

        #region Block input command
        public ICommand BlockInputCommand => new RelayCommand(execute => HandleBlockInput(bool.Parse((string)execute)), canExecute => CanExecuteBlockInput());

        private void HandleBlockInput(bool block)
        {
            _ = SendCommandAsync(new EdulinkCommand
            {
                Command = Commands.BlockInput.ToString(),
                Parameters = new Dictionary<string, string>
                {
                    { "Block", block.ToString() }
                }
            }, SelectedClients);
        }

        private bool CanExecuteBlockInput()
        {
            return SelectedClients.Any() && SelectedClients.All(client => client.Version >= Commands.BlockInput.MinimumVersion);
        }
        #endregion

        #region Rename command
        public ICommand RenameCommand => new RelayCommand(execute => Rename(), canExecute => CanExecuteRename());

        private void Rename()
        {
            InputDialogResult renameInputDialog = InputDialog.ShowLocalized("Input.Content.RenameComputer", LocalizedStrings.Instance["Input.Title.RenameComputer"]);

            if (renameInputDialog.ButtonResult == InputDialogButtonResult.Ok && !string.IsNullOrEmpty(renameInputDialog.InputResult))
            {
                _ = SendCommandAsync(new EdulinkCommand
                {
                    Command = Commands.RenameComputer.ToString(),
                    Parameters = {
                        { "Name", renameInputDialog.InputResult }
                    }
                }, SelectedClients);
            }
        }

        private bool CanExecuteRename()
        {
            return SelectedClients.Any() && SelectedClients.All(client => client.Version >= Commands.RenameComputer.MinimumVersion);
        }
        #endregion

        #region Exclude preview command
        public ICommand ExcludePreviewCommand => new RelayCommand(execute => ExcludePreview(bool.Parse((string)execute)));

        private void ExcludePreview(bool exclude)
        {
            foreach (Client client in SelectedClients.Where(c => c.IsExcludedFromPreview == !exclude))
            {
                client.IsExcludedFromPreview = exclude;
                if (exclude)
                {
                    client.Preview = null;
                }
                else
                {
                    RequestPreview(client);
                }
            }
        }
        #endregion

        #endregion
    }
}